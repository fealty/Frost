// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	public sealed class LineBreaker : ILineItemList, IBreakingPointList
	{
		public delegate void LineItemHandler(ref LineItem item);

		private readonly LinkedList<Breakpoint> _Active;
		private readonly Breakpoint[] _Candidates;

		private readonly List<BreakIndex> _Indices;
		private readonly List<LineItem> _Items;
		private readonly ILineProvider _LineProvider;

		private bool _IsBuildingParagraph;
		private Breakpoint _LastDeactivated;

		private Demerits _MinimumCandidate;

		private double _RunningShrinkSum;
		private double _RunningStretchSum;
		private double _RunningWidthSum;

		public LineBreaker(ILineProvider lineProvider)
		{
			Contract.Requires(lineProvider != null);

			_LineProvider = lineProvider;

			_Items = new List<LineItem>();
			_Indices = new List<BreakIndex>();
			_Active = new LinkedList<Breakpoint>();

			_Candidates = new Breakpoint[4];

			_IsBuildingParagraph = false;
		}

		public ILineItemList Items
		{
			get { return this; }
		}

		public IBreakingPointList Breakpoints
		{
			get { return this; }
		}

		BreakIndex IBreakingPointList.this[int index]
		{
			get { return _Indices[index]; }
		}

		List<BreakIndex>.Enumerator IBreakingPointList.GetEnumerator()
		{
			return _Indices.GetEnumerator();
		}

		int IBreakingPointList.Count
		{
			get { return _Indices.Count; }
		}

		IEnumerator<BreakIndex> IEnumerable<BreakIndex>.GetEnumerator()
		{
			return _Indices.GetEnumerator();
		}

		IEnumerator<LineItem> IEnumerable<LineItem>.GetEnumerator()
		{
			return _Items.GetEnumerator();
		}

		List<LineItem>.Enumerator ILineItemList.GetEnumerator()
		{
			return _Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		int ILineItemList.Count
		{
			get { return _Items.Count; }
		}

		LineItem ILineItemList.this[int index]
		{
			get { return _Items[index]; }
		}

		public void BeginParagraph()
		{
			_Items.Clear();

			_IsBuildingParagraph = true;
		}

		public void AddBox(double width, int position)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			_Items.Add(LineItem.CreateBox(width, position));
		}

		public void AddBox(double width)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			_Items.Add(LineItem.CreateBox(width, -1));
		}

		public void AddGlue(double width, double stretch, double shrink)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(shrink >= double.MinValue && shrink <= double.MaxValue);
			Contract.Requires(stretch >= double.MinValue && stretch <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			_Items.Add(LineItem.CreateGlue(width, -1, shrink, stretch));
		}

		public void AddGlue(double width, int position, double stretch, double shrink)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(shrink >= double.MinValue && shrink <= double.MaxValue);
			Contract.Requires(stretch >= double.MinValue && stretch <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			_Items.Add(LineItem.CreateGlue(width, position, shrink, stretch));
		}

		public void AddPenalty(double width, double penalty)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			AddPenalty(width, penalty, 0.0);
		}

		public void AddPenalty(double width, int position, double penalty)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			AddPenalty(width, position, penalty, 0.0);
		}

		public void AddPenalty(double width, double penalty, double flagged)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);
			Contract.Requires(flagged >= double.MinValue && flagged <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			_Items.Add(LineItem.CreatePenalty(width, -1, penalty, flagged));
		}

		public void AddPenalty(double width, int position, double penalty, double flagged)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);
			Contract.Requires(flagged >= double.MinValue && flagged <= double.MaxValue);

			Debug.Assert(_IsBuildingParagraph);

			_Items.Add(LineItem.CreatePenalty(width, position, penalty, flagged));
		}

		public void EndParagraph()
		{
			Debug.Assert(_IsBuildingParagraph);

			_IsBuildingParagraph = false;
		}

		public bool FindBreakpoints(double tolerance)
		{
			Contract.Requires(tolerance >= 1.0 && tolerance <= double.MaxValue);

			Debug.Assert(!_IsBuildingParagraph);

			return FindBreakpoints(tolerance, false);
		}

		public bool FindBreakpoints(double tolerance, bool isOverfullAllowed)
		{
			Contract.Requires(tolerance >= 1.0 && tolerance <= double.MaxValue);

			Debug.Assert(!_IsBuildingParagraph);

			if(_Items.Count > 0)
			{
				return FindBreakpointsInternal(tolerance, isOverfullAllowed);
			}

			return false;
		}

		private bool IsFeasibleBreakpoint(int itemIndex)
		{
			Contract.Requires(itemIndex >= 0);

			Debug.Assert(!_IsBuildingParagraph);

			LineItem item = _Items[itemIndex];

			if(item.IsPenalty)
			{
				if(!Demerits.IsPositiveInfinity(item.Penalty))
				{
					return true;
				}
			}

			if(itemIndex > 0)
			{
				if(item.IsGlue)
				{
					return _Items[itemIndex - 1].IsBox;
				}
			}

			return false;
		}

		private bool FindBreakpointsInternal(double tolerance, bool isOverfullAllowed)
		{
			Contract.Requires(tolerance >= 1.0 && tolerance <= double.MaxValue);

			_Active.AddFirst(Breakpoint.Empty);

			_RunningWidthSum = 0.0;
			_RunningStretchSum = 0.0;
			_RunningShrinkSum = 0.0;

			for(int i = 0; i < _Items.Count; ++i)
			{
				LineItem item = _Items[i];

				if(ExaminingItem != null)
				{
					ExaminingItem(ref item);
				}

				if(IsFeasibleBreakpoint(i))
				{
					AnalyzeBreakpoint(i, tolerance);
				}

				if(ItemExamined != null)
				{
					ItemExamined(ref item);
				}

				_RunningWidthSum += !item.IsPenalty ? item.Width : 0.0;
				_RunningStretchSum += item.IsGlue ? item.Stretch : 0.0;
				_RunningShrinkSum += item.IsGlue ? item.Shrink : 0.0;
			}

			Breakpoint breakpoint;

			if(_Active.Count == 0)
			{
				if(isOverfullAllowed)
				{
					breakpoint = _LastDeactivated;
				}
				else
				{
					_Indices.Clear();

					return false;
				}
			}
			else
			{
				LinkedListNode<Breakpoint> activeNode = _Active.First;

				for(var node = _Active.First; node != null; node = node.Next)
				{
					if(node.Value.Demerits < activeNode.Value.Demerits)
					{
						activeNode = node;
					}
				}

				breakpoint = activeNode.Value;
			}

			_Active.Clear();
			_Indices.Clear();

			while(breakpoint != null)
			{
				_Indices.Add(new BreakIndex(breakpoint.Position, breakpoint.Ratio));

				breakpoint = breakpoint.Previous;
			}

			_Indices.Reverse();

			_LastDeactivated = null;

			return true;
		}

		private void ComputeSum(
			int index, out double sumWidth, out double sumShrink, out double sumStretch)
		{
			Contract.Requires(index >= 0);

			sumWidth = _RunningWidthSum;
			sumShrink = _RunningShrinkSum;
			sumStretch = _RunningStretchSum;

			for(int i = index; i < _Items.Count; ++i)
			{
				LineItem item = _Items[i];

				if(item.IsGlue)
				{
					sumWidth += item.Width;
					sumShrink += item.Shrink;
					sumStretch += item.Stretch;
				}
				else if(item.IsBox || (item.IsForcedBreak && i > index))
				{
					break;
				}
			}
		}

		private double ComputeRatio(
			ref LineItem indexItem, LinkedListNode<Breakpoint> activeNode, int lineNumber)
		{
			Contract.Requires(lineNumber >= 0);
			Contract.Requires(activeNode != null);

			double length = _RunningWidthSum - activeNode.Value.TotalWidth;

			if(indexItem.IsPenalty)
			{
				length = length + indexItem.Width;
			}

			double availableLength = _LineProvider.ProduceLine(lineNumber);

			if(length < availableLength)
			{
				double y = _RunningStretchSum - activeNode.Value.TotalStretch;

				return y > 0.0 ? (availableLength - length) / y : LineItem.Infinity;
			}

			if(length > availableLength)
			{
				double z = _RunningShrinkSum - activeNode.Value.TotalShrink;

				return z > 0.0 ? (availableLength - length) / z : LineItem.Infinity;
			}

			return 0.0;
		}

		private LinkedListNode<Breakpoint> AnalyzeNode(
			LinkedListNode<Breakpoint> activeNode, int index, double tolerance)
		{
			Contract.Requires(activeNode != null);
			Contract.Requires(index >= 0);
			Contract.Requires(tolerance >= 1.0 && tolerance <= double.MaxValue);

			LinkedListNode<Breakpoint> nextNode = activeNode.Next;

			int currentLine = activeNode.Value.Line;

			LineItem indexItem = _Items[index];

			double ratio = ComputeRatio(ref indexItem, activeNode, currentLine);

			if(ratio < -1.0 || indexItem.IsForcedBreak)
			{
				_Active.Remove(activeNode);

				_LastDeactivated = activeNode.Value;
			}

			if(ratio >= -1.0 && ratio <= tolerance)
			{
				Demerits demerits = indexItem.ComputeDemerits(ratio / tolerance);

				demerits = indexItem.CombineFlaggedDemerits(_Items[activeNode.Value.Position], demerits);

				LineFitness fitness = LineFitness.FromLineRatio(ratio / tolerance);

				if(fitness.MeasureFitnessGap(activeNode.Value.Fitness) > 1)
				{
					demerits += Demerits.FitnessPenalty;
				}

				demerits += activeNode.Value.Demerits;

				if(demerits < _Candidates[fitness].Demerits)
				{
					double totalWidth;
					double totalShrink;
					double totalStretch;

					ComputeSum(index, out totalWidth, out totalShrink, out totalStretch);

					_Candidates[fitness] = new Breakpoint(
						index,
						currentLine + 1,
						fitness,
						totalWidth,
						totalStretch,
						totalShrink,
						demerits,
						ratio,
						activeNode.Value);

					_MinimumCandidate = Math.Min(_MinimumCandidate.Value, demerits.Value);
				}
			}

			return nextNode;
		}

		private void AnalyzeBreakpoint(int index, double tolerance)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(tolerance >= 1.0 && tolerance <= double.MaxValue);

			LinkedListNode<Breakpoint> activeNode = _Active.First;

			while(activeNode != null)
			{
				_MinimumCandidate = Demerits.Infinity;

				_Candidates[0] = Breakpoint.MaxDemerits;
				_Candidates[1] = Breakpoint.MaxDemerits;
				_Candidates[2] = Breakpoint.MaxDemerits;
				_Candidates[3] = Breakpoint.MaxDemerits;

				while(activeNode != null)
				{
					activeNode = AnalyzeNode(activeNode, index, tolerance);
				}

				if(_MinimumCandidate < Demerits.Infinity)
				{
					Demerits limit = _MinimumCandidate + Demerits.FitnessPenalty;

					foreach(Breakpoint candidate in _Candidates)
					{
						if(candidate.Demerits <= limit)
						{
							_Active.AddLast(candidate);
						}
					}
				}
			}
		}

		public event LineItemHandler ExaminingItem;
		public event LineItemHandler ItemExamined;
	}
}