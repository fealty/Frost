// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Composition;

namespace Frost.Effects
{
	public struct BatchedItem : IEquatable<BatchedItem>
	{
		private readonly BlendOperation _Blend;
		private readonly Canvas3 _Canvas;
		private readonly Rectangle _DestinationRegion;
		private readonly Rectangle _SourceRegion;
		private readonly Matrix3X2 _Transformation;

		public BatchedItem(
			Canvas3 canvas,
			Rectangle sourceRegion,
			Rectangle destinationRegion,
			BlendOperation blend,
			ref Matrix3X2 transformation)
		{
			Contract.Requires(canvas != null);

			_Canvas = canvas;
			_SourceRegion = sourceRegion;
			_DestinationRegion = destinationRegion;
			_Blend = blend;
			_Transformation = transformation;
		}

		public Matrix3X2 Transformation
		{
			get { return _Transformation; }
		}

		public Rectangle SourceRegion
		{
			get { return _SourceRegion; }
		}

		public Rectangle DestinationRegion
		{
			get { return _DestinationRegion; }
		}

		public Canvas3 Canvas
		{
			get { return _Canvas; }
		}

		public BlendOperation Blend
		{
			get { return _Blend; }
		}

		public bool Equals(BatchedItem other)
		{
			return other._Blend == _Blend && other._Transformation.Equals(_Transformation) &&
			       other._SourceRegion.Equals(_SourceRegion) &&
			       other._DestinationRegion.Equals(_DestinationRegion) && Equals(other._Canvas, _Canvas);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is BatchedItem && Equals((BatchedItem)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = ((int)_Blend).GetHashCode();
				result = (result * 397) ^ _Transformation.GetHashCode();
				result = (result * 397) ^ _SourceRegion.GetHashCode();
				result = (result * 397) ^ _DestinationRegion.GetHashCode();
				result = (result * 397) ^ _Canvas.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(BatchedItem left, BatchedItem right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BatchedItem left, BatchedItem right)
		{
			return !left.Equals(right);
		}
	}
}