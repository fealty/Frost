// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.DirectX.Common;
using Frost.Surfacing;

namespace Frost.DirectX.Composition
{
	public sealed class TargetLayer : Surface2D
	{
		private Canvas.ResolvedContext _Canvas;

		public TargetLayer(ref Description surfaceDescription) : base(ref surfaceDescription)
		{
		}

		public Canvas.ResolvedContext AcquireRegion(Size dimensions)
		{
			Contract.Requires(Check.IsPositive(dimensions.Width));
			Contract.Requires(Check.IsPositive(dimensions.Height));

			// the 2D surface must be capable of containing the dimensions
			if(!Region.Contains(new Rectangle(Region.Location, dimensions)))
			{
				return null;
			}

			Rectangle offsetRegion;

			ComputeOffsetRegion(dimensions, out offsetRegion);

			offsetRegion = new Rectangle(offsetRegion.Location, offsetRegion.Size + dimensions);

			// determine the region for the canvas as a part of the 2D surface
			Rectangle region = new Rectangle(offsetRegion.Location, dimensions);

			// create a new target context for the canvas
			_Canvas = new TargetContext(new Canvas(dimensions), region, this);

			// assign the target contex to the target canvas
			Canvas.Implementation.Assign(_Canvas.Target, _Canvas);

			return _Canvas;
		}

		private void ComputeOffsetRegion(Size desiredSize, out Rectangle result)
		{
			Contract.Requires(Check.IsPositive(desiredSize.Width));
			Contract.Requires(Check.IsPositive(desiredSize.Height));

			result = Rectangle.Empty;

			if(!desiredSize.Width.Equals(Region.Width))
			{
				result = new Rectangle(new Point(1, result.Y), new Size(2, result.Height));
			}

			if(!desiredSize.Height.Equals(Region.Height))
			{
				result = new Rectangle(new Point(result.X, 1), new Size(result.Width, 2));
			}
		}

		private sealed class TargetContext : Canvas.ResolvedContext
		{
			private readonly Canvas _Canvas;
			private readonly TargetLayer _Layer;
			private readonly Rectangle _Region;

			public TargetContext(Canvas canvas, Rectangle region, TargetLayer layer)
			{
				Contract.Requires(canvas != null);
				Contract.Requires(layer != null);

				_Canvas = canvas;
				_Region = region;
				_Layer = layer;
			}

			public override Rectangle Region
			{
				get { return _Region; }
			}

			public override ISurface2D Surface2D
			{
				get { return _Layer; }
			}

			public override Canvas Target
			{
				get { return _Canvas; }
			}

			public override void Forget()
			{
				throw new NotSupportedException();
			}
		}
	}
}