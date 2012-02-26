using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	public sealed class LineBreaker : ILineItemList, IBreakingPointList
	{
		public delegate void LineItemHandler(ref LineItem item);

		private readonly LinkedList<Breakpoint> mActive;
		private readonly Breakpoint[] mCandidates;

		private readonly List<BreakIndex> mIndices;
		private readonly List<LineItem> mItems;
		private readonly ILineProvider mLineProvider;

		private bool mIsBuildingParagraph;
		private Breakpoint mLastDeactivated;

		private Demerits mMinimumCandidate;

		private double mRunningShrinkSum;
		private double mRunningStretchSum;
		private double mRunningWidthSum;

		public LineBreaker(ILineProvider lineProvider)
		{
			Contract.Requires(lineProvider != null);

			mLineProvider = lineProvider;

			mItems = new List<LineItem>();
			mIndices = new List<BreakIndex>();
			mActive = new LinkedList<Breakpoint>();

			mCandidates = new Breakpoint[4];

			mIsBuildingParagraph = false;
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
			get { return mIndices[index]; }
		}

		List<BreakIndex>.Enumerator IBreakingPointList.GetEnumerator()
		{
			return mIndices.GetEnumerator();
		}

		int IBreakingPointList.Count
		{
			get { return mIndices.Count; }
		}

		IEnumerator<BreakIndex> IEnumerable<BreakIndex>.GetEnumerator()
		{
			return mIndices.GetEnumerator();
		}

		IEnumerator<LineItem> IEnumerable<LineItem>.GetEnumerator()
		{
			return mItems.GetEnumerator();
		}

		List<LineItem>.Enumerator ILineItemList.GetEnumerator()
		{
			return mItems.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		int ILineItemList.Count
		{
			get { return mItems.Count; }
		}

		LineItem ILineItemList.this[int index]
		{
			get { return mItems[index]; }
		}

		public void BeginParagraph()
		{
			mItems.Clear();

			mIsBuildingParagraph = true;
		}

		public void AddBox(double width, int position)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			mItems.Add(LineItem.CreateBox(width, position));
		}

		public void AddBox(double width)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			mItems.Add(LineItem.CreateBox(width, -1));
		}

		public void AddGlue(double width, double stretch, double shrink)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(shrink >= double.MinValue && shrink <= double.MaxValue);
			Contract.Requires(stretch >= double.MinValue && stretch <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			mItems.Add(LineItem.CreateGlue(width, -1, shrink, stretch));
		}

		public void AddGlue(double width, int position, double stretch, double shrink)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(shrink >= double.MinValue && shrink <= double.MaxValue);
			Contract.Requires(stretch >= double.MinValue && stretch <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			mItems.Add(LineItem.CreateGlue(width, position, shrink, stretch));
		}

		public void AddPenalty(double width, double penalty)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			AddPenalty(width, penalty, 0.0);
		}

		public void AddPenalty(double width, int position, double penalty)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			AddPenalty(width, position, penalty, 0.0);
		}

		public void AddPenalty(double width, double penalty, double flagged)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);
			Contract.Requires(flagged >= double.MinValue && flagged <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			mItems.Add(LineItem.CreatePenalty(width, -1, penalty, flagged));
		}

		public void AddPenalty(
			double width, int position, double penalty, double flagged)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(penalty >= double.MinValue && penalty <= double.MaxValue);
			Contract.Requires(flagged >= double.MinValue && flagged <= double.MaxValue);

			Debug.Assert(mIsBuildingParagraph);

			mItems.Add(LineItem.CreatePenalty(width, position, penalty, flagged));
		}

		public void EndParagraph()
		{
			Debug.Assert(mIsBuildingParagraph);

			mIsBuildingParagraph = false;
		}

		public bool FindBreakpoints(double tolerance)
		{
			Contract.Requires(
				tolerance >= 1.0 && tolerance <= double.MaxValue);

			Debug.Assert(!mIsBuildingParagraph);

			return FindBreakpoints(tolerance, false);
		}

		public bool FindBreakpoints(double tolerance, bool isOverfullAllowed)
		{
			Contract.Requires(
				tolerance >= 1.0 && tolerance <= double.MaxValue);

			Debug.Assert(!mIsBuildingParagraph);

			if(mItems.Count > 0)
			{
				return FindBreakpointsInternal(tolerance, isOverfullAllowed);
			}

			return false;
		}

		private bool IsFeasibleBreakpoint(int itemIndex)
		{
			Contract.Requires(itemIndex >= 0);

			Debug.Assert(!mIsBuildingParagraph);

			LineItem item = mItems[itemIndex];

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
					return mItems[itemIndex - 1].IsBox;
				}
			}

			return false;
		}

		private bool FindBreakpointsInternal(double tolerance, bool isOverfullAllowed)
		{
			Contract.Requires(
				tolerance >= 1.0 && tolerance <= double.MaxValue);

			mActive.AddFirst(Breakpoint.Empty);

			mRunningWidthSum = 0.0;
			mRunningStretchSum = 0.0;
			mRunningShrinkSum = 0.0;

			for(int i = 0; i < mItems.Count; ++i)
			{
				LineItem item = mItems[i];

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

				mRunningWidthSum += !item.IsPenalty ? item.Width : 0.0;
				mRunningStretchSum += item.IsGlue ? item.Stretch : 0.0;
				mRunningShrinkSum += item.IsGlue ? item.Shrink : 0.0;
			}

			Breakpoint breakpoint;

			if(mActive.Count == 0)
			{
				if(isOverfullAllowed)
				{
					breakpoint = mLastDeactivated;
				}
				else
				{
					mIndices.Clear();

					return false;
				}
			}
			else
			{
				LinkedListNode<Breakpoint> activeNode = mActive.First;

				for(var node = mActive.First; node != null; node = node.Next)
				{
					if(node.Value.Demerits < activeNode.Value.Demerits)
					{
						activeNode = node;
					}
				}

				breakpoint = activeNode.Value;
			}

			mActive.Clear();
			mIndices.Clear();

			while(breakpoint != null)
			{
				mIndices.Add(new BreakIndex(breakpoint.Position, breakpoint.Ratio));

				breakpoint = breakpoint.Previous;
			}

			mIndices.Reverse();

			mLastDeactivated = null;

			return true;
		}

		private void ComputeSum(
			int index,
			out double sumWidth,
			out double sumShrink,
			out double sumStretch)
		{
			Contract.Requires(index >= 0);

			sumWidth = mRunningWidthSum;
			sumShrink = mRunningShrinkSum;
			sumStretch = mRunningStretchSum;

			for(int i = index; i < mItems.Count; ++i)
			{
				LineItem item = mItems[i];

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
			ref LineItem indexItem,
			LinkedListNode<Breakpoint> activeNode,
			int lineNumber)
		{
			Contract.Requires(lineNumber >= 0);
			Contract.Requires(activeNode != null);

			double length = mRunningWidthSum - activeNode.Value.TotalWidth;

			if(indexItem.IsPenalty)
			{
				length = length + indexItem.Width;
			}

			double availableLength = mLineProvider.ProduceLine(lineNumber);

			if(length < availableLength)
			{
				double y = mRunningStretchSum - activeNode.Value.TotalStretch;

				return y > 0.0 ? (availableLength - length) / y : LineItem.Infinity;
			}

			if(length > availableLength)
			{
				double z = mRunningShrinkSum - activeNode.Value.TotalShrink;

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

			LineItem indexItem = mItems[index];

			double ratio = ComputeRatio(ref indexItem, activeNode, currentLine);

			if(ratio < -1.0 || indexItem.IsForcedBreak)
			{
				mActive.Remove(activeNode);

				mLastDeactivated = activeNode.Value;
			}

			if(ratio >= -1.0 && ratio <= tolerance)
			{
				Demerits demerits = indexItem.ComputeDemerits(ratio / tolerance);

				demerits = indexItem.CombineFlaggedDemerits(
					mItems[activeNode.Value.Position], demerits);

				LineFitness fitness = LineFitness.FromLineRatio(ratio / tolerance);

				if(fitness.MeasureFitnessGap(activeNode.Value.Fitness) > 1)
				{
					demerits += Demerits.FitnessPenalty;
				}

				demerits += activeNode.Value.Demerits;

				if(demerits < mCandidates[fitness].Demerits)
				{
					double totalWidth;
					double totalShrink;
					double totalStretch;

					ComputeSum(index, out totalWidth, out totalShrink, out totalStretch);

					mCandidates[fitness] = new Breakpoint(
						index,
						currentLine + 1,
						fitness,
						totalWidth,
						totalStretch,
						totalShrink,
						demerits,
						ratio,
						activeNode.Value);

					mMinimumCandidate = Math.Min(mMinimumCandidate.Value, demerits.Value);
				}
			}

			return nextNode;
		}

		private void AnalyzeBreakpoint(int index, double tolerance)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(
				tolerance >= 1.0 && tolerance <= double.MaxValue);

			LinkedListNode<Breakpoint> activeNode = mActive.First;

			while(activeNode != null)
			{
				mMinimumCandidate = Demerits.Infinity;

				mCandidates[0] = Breakpoint.MaxDemerits;
				mCandidates[1] = Breakpoint.MaxDemerits;
				mCandidates[2] = Breakpoint.MaxDemerits;
				mCandidates[3] = Breakpoint.MaxDemerits;

				while(activeNode != null)
				{
					activeNode = AnalyzeNode(activeNode, index, tolerance);
				}

				if(mMinimumCandidate < Demerits.Infinity)
				{
					Demerits limit = mMinimumCandidate + Demerits.FitnessPenalty;

					foreach(Breakpoint candidate in mCandidates)
					{
						if(candidate.Demerits <= limit)
						{
							mActive.AddLast(candidate);
						}
					}
				}
			}
		}

		public event LineItemHandler ExaminingItem;
		public event LineItemHandler ItemExamined;
	}
}