// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	/// <summary>
	///   optimally distributes items on a series of lines by determining the optimal breaking points
	/// </summary>
	public sealed class LineBreaker
	{
		public delegate void LineItemHandler(ref LineItem item);

		private readonly LinkedList<Breakpoint> _Active;
		private readonly Breakpoint[] _Candidates;

		private readonly List<BreakIndex> _Indices;
		private readonly ReadOnlyCollection<BreakIndex> _IndicesReadOnly;
		private readonly List<LineItem> _Items;
		private readonly ReadOnlyCollection<LineItem> _ItemsReadOnly;

		private bool _IsActive;
		private Breakpoint _LastDeactivated;

		private Demerits _MinimumCandidate;

		private double _RunningShrinkSum;
		private double _RunningStretchSum;
		private double _RunningWidthSum;

		public LineBreaker()
		{
			_Items = new List<LineItem>();
			_Indices = new List<BreakIndex>();
			_Active = new LinkedList<Breakpoint>();

			_IndicesReadOnly = _Indices.AsReadOnly();
			_ItemsReadOnly = _Items.AsReadOnly();

			_Candidates = new Breakpoint[4];

			_IsActive = false;
		}

		/// <summary>
		///   exposes a read-only view of the items to process
		/// </summary>
		public ReadOnlyCollection<LineItem> Items
		{
			get { return _ItemsReadOnly; }
		}

		/// <summary>
		///   exposes a read-only view of the discovered breaking points
		/// </summary>
		public ReadOnlyCollection<BreakIndex> Breakpoints
		{
			get { return _IndicesReadOnly; }
		}

		/// <summary>
		///   prepares the instance for a sequence of new line items
		/// </summary>
		public void Begin()
		{
			Contract.Assert(!_IsActive);

			_Items.Clear();

			_IsActive = true;
		}

		/// <summary>
		///   appends a new box to the item list
		/// </summary>
		/// <param name="width"> the width of the box </param>
		/// <param name="position"> the user-defined index position of the item </param>
		public void AddBox(double width, int position)
		{
			Contract.Requires(Check.IsPositive(width));

			Contract.Assert(_IsActive);

			_Items.Add(LineItem.CreateBox(width, position));
		}

		/// <summary>
		///   appends a new box to the item list
		/// </summary>
		/// <param name="width"> the width of the box </param>
		public void AddBox(double width)
		{
			Contract.Requires(Check.IsPositive(width));

			Contract.Assert(_IsActive);

			_Items.Add(LineItem.CreateBox(width, -1));
		}

		/// <summary>
		///   appends new glue to the item list
		/// </summary>
		/// <param name="width"> indicates the width of the glue </param>
		/// <param name="stretch"> indicates how many units the glue may stretch </param>
		/// <param name="shrink"> indicates how many units the glue may shrink </param>
		public void AddGlue(double width, double stretch, double shrink)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(shrink));
			Contract.Requires(Check.IsFinite(stretch));

			Contract.Assert(_IsActive);

			_Items.Add(LineItem.CreateGlue(width, -1, shrink, stretch));
		}

		/// <summary>
		///   appends new glue to the item list
		/// </summary>
		/// <param name="width"> indicates the width of the glue </param>
		/// <param name="position"> the user-defined index position of the item </param>
		/// <param name="stretch"> indicates how many units the glue may stretch </param>
		/// <param name="shrink"> indicates how many units the glue may shrink </param>
		public void AddGlue(
			double width, int position, double stretch, double shrink)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(shrink));
			Contract.Requires(Check.IsFinite(stretch));

			Contract.Assert(_IsActive);

			_Items.Add(LineItem.CreateGlue(width, position, shrink, stretch));
		}

		/// <summary>
		///   appends a new penalty to the item list
		/// </summary>
		/// <param name="width"> indicates the width of the penalty </param>
		/// <param name="penalty"> indicates the penalty incurred when chosen </param>
		public void AddPenalty(double width, double penalty)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(penalty));

			Contract.Assert(_IsActive);

			AddPenalty(width, penalty, 0.0);
		}

		/// <summary>
		///   appends a new penalty to the item list
		/// </summary>
		/// <param name="width"> indicates the width of the penalty </param>
		/// <param name="position"> the user-defined index position of the item </param>
		/// <param name="penalty"> indicates the penalty incurred when chosen </param>
		public void AddPenalty(double width, int position, double penalty)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(penalty));

			Contract.Assert(_IsActive);

			AddPenalty(width, position, penalty, 0.0);
		}

		/// <summary>
		///   appends a new penalty to the item list
		/// </summary>
		/// <param name="width"> indicates the width of the penalty </param>
		/// <param name="penalty"> indicates the penalty incurred when chosen </param>
		/// <param name="flagged"> indicates the flagged value for the penalty </param>
		public void AddPenalty(double width, double penalty, double flagged)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(penalty));
			Contract.Requires(Check.IsFinite(flagged));

			Contract.Assert(_IsActive);

			_Items.Add(LineItem.CreatePenalty(width, -1, penalty, flagged));
		}

		/// <summary>
		///   appends a new penalty to the item list
		/// </summary>
		/// <param name="width"> indicates the width of the penalty </param>
		/// <param name="position"> the user-defined index position of the item </param>
		/// <param name="penalty"> indicates the penalty incurred when chosen </param>
		/// <param name="flagged"> indicates the flagged value for the penalty </param>
		public void AddPenalty(
			double width, int position, double penalty, double flagged)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(penalty));
			Contract.Requires(Check.IsFinite(flagged));

			Contract.Assert(_IsActive);

			_Items.Add(LineItem.CreatePenalty(width, position, penalty, flagged));
		}

		/// <summary>
		///   signals the end of the item sequence
		/// </summary>
		public void End()
		{
			Contract.Assert(_IsActive);

			_IsActive = false;
		}

		//TODO: tolerance should probably be a property
		//TODO: emergency stretch should probably be a property

		/// <summary>
		///   finds the optimal breakpoints in the item sequence
		/// </summary>
		/// <param name="tolerance"> indicates the tolerance for suboptimal lines </param>
		/// <param name="lineProvider"> the line provider </param>
		/// <returns> <c>true</c> if the sequence of items was successfully broken into lines; otherwise, <c>false</c> </returns>
		public bool FindBreakpoints(double tolerance, ILineProvider lineProvider)
		{
			Contract.Requires(Check.IsFinite(tolerance));
			Contract.Requires(tolerance >= 1.0);
			Contract.Requires(lineProvider != null);

			Contract.Assert(_IsActive);

			return FindBreakpoints(tolerance, lineProvider, false);
		}

		/// <summary>
		///   finds the optimal breakpoints in the item sequence
		/// </summary>
		/// <param name="tolerance"> indicates the tolerance for suboptimal lines </param>
		/// <param name="lineProvider"> the line provider </param>
		/// <param name="isOverfullAllowed"> indicates whether overfull lines are permitted </param>
		/// <returns> <c>true</c> if the sequence of items was successfully broken into lines; otherwise, <c>false</c> </returns>
		public bool FindBreakpoints(
			double tolerance, ILineProvider lineProvider, bool isOverfullAllowed)
		{
			Contract.Requires(Check.IsFinite(tolerance));
			Contract.Requires(tolerance >= 1.0);
			Contract.Requires(lineProvider != null);

			Contract.Assert(_IsActive);

			if(_Items.Count > 0)
			{
				return FindBreakpointsInternal(tolerance, lineProvider, isOverfullAllowed);
			}

			return false;
		}

		/// <summary>
		///   determines whether the item at the given index is a possible breaking point
		/// </summary>
		/// <param name="itemIndex"> the item index to test </param>
		/// <returns> <c>true</c> if the item is a possible breaking point; otherwise, <c>false</c> </returns>
		private bool IsFeasibleBreakpoint(int itemIndex)
		{
			Contract.Requires(itemIndex >= 0);

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

		/// <summary>
		///   finds the optimal breakpoints in the item sequence
		/// </summary>
		/// <param name="tolerance"> indicates the tolerance for suboptimal lines </param>
		/// <param name="lineProvider"> the line provider </param>
		/// <param name="isOverfullAllowed"> indicates whether overfull lines are permitted </param>
		/// <returns> <c>true</c> if the sequence of items was successfully broken into lines; otherwise, <c>false</c> </returns>
		private bool FindBreakpointsInternal(
			double tolerance, ILineProvider lineProvider, bool isOverfullAllowed)
		{
			Contract.Requires(Check.IsFinite(tolerance));
			Contract.Requires(tolerance >= 1.0);
			Contract.Requires(lineProvider != null);

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

				// analyze each possible breaking point
				if(IsFeasibleBreakpoint(i))
				{
					AnalyzeBreakpoint(i, tolerance, lineProvider);
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
					// panic by using the last known breaking point
					breakpoint = _LastDeactivated;
				}
				else
				{
					// no possible breaking points with the given tolerance
					_Indices.Clear();

					return false;
				}
			}
			else
			{
				LinkedListNode<Breakpoint> activeNode = _Active.First;

				for(var node = _Active.First; node != null; node = node.Next)
				{
					// select the tree-line having the least demerits
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
				// add each breaking point in the chain
				_Indices.Add(new BreakIndex(breakpoint.Position, breakpoint.Ratio));

				breakpoint = breakpoint.Previous;
			}

			_Indices.Reverse();

			_LastDeactivated = null;

			return true;
		}

		/// <summary>
		///   computes the running sum for an index
		/// </summary>
		/// <param name="index"> indicates the index from which to start </param>
		/// <param name="sumWidth"> holds the total width </param>
		/// <param name="sumShrink"> holds the total shrink </param>
		/// <param name="sumStretch"> holds the total stretch </param>
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

		/// <summary>
		///   computes the ratio for the given line and node
		/// </summary>
		/// <param name="indexItem"> indicates the index of the line item </param>
		/// <param name="activeNode"> the active breakpoint node </param>
		/// <param name="lineNumber"> indicates the active line number </param>
		/// <param name="lineProvider"> the line provider </param>
		/// <returns> the ratio of the line. </returns>
		private double ComputeRatio(
			ref LineItem indexItem,
			LinkedListNode<Breakpoint> activeNode,
			int lineNumber,
			ILineProvider lineProvider)
		{
			Contract.Requires(lineNumber >= 0);
			Contract.Requires(activeNode != null);
			Contract.Requires(lineProvider != null);

			double length = _RunningWidthSum - activeNode.Value.TotalWidth;

			if(indexItem.IsPenalty)
			{
				length = length + indexItem.Width;
			}

			double availableLength = lineProvider.ProduceLine(lineNumber);

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

		/// <summary>
		///   analyzes a node
		/// </summary>
		/// <param name="activeNode"> the active node </param>
		/// <param name="index"> indicates the index of the item </param>
		/// <param name="tolerance"> indicates the tolerance for badness </param>
		/// <param name="lineProvider"> the line provider </param>
		/// <returns> the next node </returns>
		private LinkedListNode<Breakpoint> AnalyzeNode(
			LinkedListNode<Breakpoint> activeNode,
			int index,
			double tolerance,
			ILineProvider lineProvider)
		{
			Contract.Requires(activeNode != null);
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsFinite(tolerance));
			Contract.Requires(tolerance >= 1.0);
			Contract.Requires(lineProvider != null);

			LinkedListNode<Breakpoint> nextNode = activeNode.Next;

			int currentLine = activeNode.Value.Line;

			LineItem indexItem = _Items[index];

			double ratio = ComputeRatio(
				ref indexItem, activeNode, currentLine, lineProvider);

			if(ratio < -1.0 || indexItem.IsForcedBreak)
			{
				_Active.Remove(activeNode);

				_LastDeactivated = activeNode.Value;
			}

			if(ratio >= -1.0 && ratio <= tolerance)
			{
				Demerits demerits = indexItem.ComputeDemerits(ratio / tolerance);

				demerits =
					indexItem.CombineFlaggedDemerits(
						_Items[activeNode.Value.Position], demerits);

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

		/// <summary>
		///   analyzes a breaking point
		/// </summary>
		/// <param name="index"> indicates the index of the breaking point </param>
		/// <param name="tolerance"> indicates the tolerance for badness </param>
		/// <param name="lineProvider"> the line provider </param>
		private void AnalyzeBreakpoint(
			int index, double tolerance, ILineProvider lineProvider)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsFinite(tolerance));
			Contract.Requires(tolerance >= 1.0);
			Contract.Requires(lineProvider != null);

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
					activeNode = AnalyzeNode(activeNode, index, tolerance, lineProvider);
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

		/// <summary>
		///   fires before an item is examined
		/// </summary>
		public event LineItemHandler ExaminingItem;

		/// <summary>
		///   fires after an item is examined
		/// </summary>
		public event LineItemHandler ItemExamined;
	}
}