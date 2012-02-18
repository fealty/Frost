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
		private readonly Canvas _Canvas;
		private readonly Rectangle _DestinationRegion;
		private readonly Rectangle _SourceRegion;
		private readonly Matrix3X2 _Transformation;

		public BatchedItem(
			Canvas canvas,
			Rectangle sourceRegion,
			Rectangle destinationRegion,
			BlendOperation blend,
			ref Matrix3X2 transformation)
		{
			Contract.Requires(canvas != null);

			this._Canvas = canvas;
			this._SourceRegion = sourceRegion;
			this._DestinationRegion = destinationRegion;
			this._Blend = blend;
			this._Transformation = transformation;
		}

		public Matrix3X2 Transformation
		{
			get { return this._Transformation; }
		}

		public Rectangle SourceRegion
		{
			get { return this._SourceRegion; }
		}

		public Rectangle DestinationRegion
		{
			get { return this._DestinationRegion; }
		}

		public Canvas Canvas
		{
			get { return this._Canvas; }
		}

		public BlendOperation Blend
		{
			get { return this._Blend; }
		}

		public bool Equals(BatchedItem other)
		{
			return other._Blend == this._Blend &&
			       other._Transformation.Equals(this._Transformation) &&
			       other._SourceRegion.Equals(this._SourceRegion) &&
			       other._DestinationRegion.Equals(this._DestinationRegion) &&
			       Equals(other._Canvas, this._Canvas);
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
				int result = ((int)this._Blend).GetHashCode();
				result = (result * 397) ^ this._Transformation.GetHashCode();
				result = (result * 397) ^ this._SourceRegion.GetHashCode();
				result = (result * 397) ^ this._DestinationRegion.GetHashCode();
				result = (result * 397) ^ this._Canvas.GetHashCode();
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