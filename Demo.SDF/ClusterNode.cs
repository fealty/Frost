// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;

using Frost;
using Frost.Shaping;

namespace Demo.SDF
{
	internal class ClusterNode : IEnumerable<ClusterNode>
	{
		private readonly LinkedList<ClusterNode> _Children;
		private readonly LinkedListNode<ClusterNode> _ListNode;

		private Point _Center;
		private Rectangle _Region;

		public ClusterNode()
		{
			_Children = new LinkedList<ClusterNode>();

			_ListNode = new LinkedListNode<ClusterNode>(this);

			_Region = new Rectangle(Point.MaxValue, Size.MinValue);
		}

		public Rectangle Region
		{
			get { return _Region; }
		}

		public Point Center
		{
			get { return _Center; }
		}

		public virtual bool IsGroup
		{
			get { return true; }
		}

		public virtual Shape Geometry
		{
			get { return null; }
		}

		public LinkedListNode<ClusterNode> Node
		{
			get { return _ListNode; }
		}

		IEnumerator<ClusterNode> IEnumerable<ClusterNode>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void AddChild(LinkedListNode<ClusterNode> node)
		{
			Rectangle region = node.Value.Region;

			float left = _Region.Left;
			float top = _Region.Top;
			float right = _Region.Right;
			float bottom = _Region.Bottom;

			left = Math.Min(left, region.Left);
			top = Math.Min(top, region.Top);
			right = Math.Max(right, region.Right);
			bottom = Math.Max(bottom, region.Bottom);

			region = new Rectangle(left, top, right - left, bottom - top);

			SetRegion(ref region);

			_Children.AddLast(node);
		}

		public virtual void Query(ref Point point, out Sample.Location sample)
		{
			throw new NotSupportedException();
		}

		public virtual void ComputeDistancesFrom(
			ref Point point, out double minimumResult, out double maximumResult)
		{
			throw new NotSupportedException();
		}

		protected void SetRegion(ref Rectangle region)
		{
			_Region = region;

			_Center = _Region.Center;
		}

		private LinkedList<ClusterNode>.Enumerator GetEnumerator()
		{
			return _Children.GetEnumerator();
		}
	}
}