// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost.Atlasing;
using Frost.DirectX.Common;
using Frost.DirectX.Common.Diagnostics;
using Frost.Painting;
using Frost.Surfacing;

using SharpDX.Direct2D1;
using SharpDX.Direct3D10;

using LineJoin = Frost.Painting.LineJoin;

namespace Frost.DirectX.Painting
{
	internal sealed class Painter : Frost.Painting.Painter, IDisposable
	{
		private readonly Device2D _Device2D;
		private readonly Device _Device3D;
		private readonly Drawer _Drawer;
		private readonly Factory _Factory2D;

		private readonly TimeSpanCounter _FrameDuration =
			new TimeSpanCounter("Painting", "FrameDuration");

		private readonly Stack<State> _States;
		private readonly Stopwatch _Watch;

		private Brush _ActiveBrush;
		private BrushState _ActiveBrushState;
		private bool _IsBrushInvalid;

		private StrokeStyle _StrokeStyle;
		private Surface2D _TargetSurface;

		public Painter(
			Factory factory2D, Device2D device2D, Device device3D)
			: base(device2D)
		{
			Contract.Requires(factory2D != null);
			Contract.Requires(device3D != null);

			this._States = new Stack<State>();

			this._Drawer = new Drawer(factory2D);

			this._Device2D = device2D;
			this._Device3D = device3D;
			this._Factory2D = factory2D;

			device2D.Diagnostics.Register(this._FrameDuration);

			this._Watch = new Stopwatch();
		}

		public TimeSpanCounter FrameDuration
		{
			get { return this._FrameDuration; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected override void OnBegin(Canvas target, Retention retention)
		{
			this._Watch.Reset();
			this._Watch.Start();

			SetBrush(Color.Black);

			this._IsBrushInvalid = true;

			this._TargetSurface = (Surface2D)target.Surface2D;

			this._TargetSurface.AcquireLock();

			this._Drawer.Begin(target);
		}

		protected override void OnEnd()
		{
			try
			{
				try
				{
					this._Drawer.End();
				}
				finally
				{
					this._TargetSurface.ReleaseLock();

					this._ActiveBrush = null;
					this._TargetSurface = null;
				}
			}
			finally
			{
				this._Watch.Stop();

				this._FrameDuration.Value += this._Watch.Elapsed;
			}
		}

		protected override void OnClear()
		{
			this.Reconfigure();

			this._Drawer.Clear();
		}

		protected override void OnClear(ref Rectangle region)
		{
			this.Reconfigure();

			this._Drawer.Clear(region);
		}

		protected override void OnStroke(ref Rectangle rectangleRegion)
		{
			this.Reconfigure();

			this._Drawer.Stroke(
				rectangleRegion,
				this._ActiveBrush,
				this._StrokeStyle,
				this.ActiveStrokeWidth);
		}

		protected override void OnStroke(
			ref Point lineStart, ref Point lineEnd)
		{
			this.Reconfigure();

			this._Drawer.Stroke(
				lineStart,
				lineEnd,
				this._ActiveBrush,
				this._StrokeStyle,
				this.ActiveStrokeWidth);
		}

		protected override void OnStroke(
			ref Rectangle rectangleRegion, ref Size roundedRadius)
		{
			this.Reconfigure();

			this._Drawer.Stroke(
				rectangleRegion,
				roundedRadius,
				this._ActiveBrush,
				this._StrokeStyle,
				this.ActiveStrokeWidth);
		}

		protected override void OnFill(ref Rectangle rectangleRegion)
		{
			this.Reconfigure();

			this._Drawer.Fill(rectangleRegion, this._ActiveBrush);
		}

		protected override void OnFill(
			ref Rectangle rectangleRegion, ref Size roundedRadius)
		{
			this.Reconfigure();

			this._Drawer.Fill(
				rectangleRegion, roundedRadius, this._ActiveBrush);
		}

		protected override void OnStroke(Shaping.Geometry geometry)
		{
			this.Reconfigure();

			this._Drawer.Stroke(
				geometry,
				this._ActiveBrush,
				this._StrokeStyle,
				this.ActiveStrokeWidth);
		}

		protected override void OnFill(Shaping.Geometry geometry)
		{
			this.Reconfigure();

			this._Drawer.Fill(geometry, this._ActiveBrush);
		}

		protected override void OnSaveState()
		{
			this._States.Push(
				new State
				{
					DashCap = ActiveDashCap,
					IsAntialiased = ActiveAntialiasing,
					LineStyle = ActiveLineStyle,
					MiterLimit = ActiveMiterLimit,
					StrokeCap = ActiveStrokeCap,
					StrokeJoin = ActiveStrokeJoin,
					StrokeWidth = ActiveStrokeWidth,
					Transformation = ActiveTransformation
				});
		}

		protected override void OnRestoreState()
		{
			State newState = this._States.Pop();

			IsAntialiased = newState.IsAntialiased;
			DashCap = newState.DashCap;
			LineStyle = newState.LineStyle;
			MiterLimit = newState.MiterLimit;
			StrokeCap = newState.StrokeCap;
			StrokeJoin = newState.StrokeJoin;
			StrokeWidth = newState.StrokeWidth;

			Transform(ref newState.Transformation, TransformMode.Replace);
		}

		protected override void OnResetState()
		{
		}

		protected override void OnSetBrush(Color color)
		{
			this._ActiveBrushState.SolidColorColor = color;

			this._ActiveBrushState.BrushType = BrushType.SolidColor;

			this._IsBrushInvalid = true;
		}

		protected override void OnSetBrush(
			Canvas source, Repetition extension)
		{
			this._ActiveBrushState.PatternSurface.SafeDispose();

			Surface2D.Description description;

			description.Device2D = this._Device2D;
			description.Device3D = this._Device3D;
			description.Usage = SurfaceUsage.Normal;
			description.Size = source.Region.Size;
			description.Factory2D = null;

			this._ActiveBrushState.PatternSurface =
				Surface2D.FromDescription(ref description);

			source.Surface2D.CopyTo(
				source.Region, this._ActiveBrushState.PatternSurface, Point.Empty);

			this._ActiveBrushState.PatternRepetition = extension;

			this._ActiveBrushState.BrushType = BrushType.Pattern;

			this._IsBrushInvalid = true;
		}

		protected override void OnSetBrush(
			ref Point linearGradientStart,
			ref Point linearGradientEnd,
			Gradient gradient)
		{
			this._ActiveBrushState.Stops = gradient;

			this._ActiveBrushState.LinearGradientStart = linearGradientStart;
			this._ActiveBrushState.LinearGradientEnd = linearGradientEnd;

			this._ActiveBrushState.BrushType = BrushType.LinearGradient;

			this._IsBrushInvalid = true;
		}

		protected override void OnSetBrush(
			ref Point radialGradientCenter,
			ref Point radialGradientOffset,
			ref Size radialGradientRadius,
			Gradient gradient)
		{
			this._ActiveBrushState.Stops = gradient;

			this._ActiveBrushState.RadialGradientCenter = radialGradientCenter;
			this._ActiveBrushState.RadialGradientOffset = radialGradientOffset;
			this._ActiveBrushState.RadialGradientRadius = radialGradientRadius;

			this._ActiveBrushState.BrushType = BrushType.RadialGradient;

			this._IsBrushInvalid = true;
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				this._Drawer.Dispose();
				this._ActiveBrushState.PatternSurface.SafeDispose();
				this._StrokeStyle.SafeDispose();
			}
		}

		private void Reconfigure()
		{
			if(this._IsBrushInvalid)
			{
				this.ReconfigureBrush();

				this._IsBrushInvalid = false;
			}

			if(IsLineStyleInvalid || IsMiterLimitInvalid || IsStrokeCapInvalid ||
			   IsStrokeJoinInvalid || IsDashCapInvalid)
			{
				this.ReconfigureStrokeStyle();

				IsLineStyleInvalid = false;
				IsMiterLimitInvalid = false;
				IsStrokeCapInvalid = false;
				IsStrokeJoinInvalid = false;
				IsDashCapInvalid = false;
			}

			if(IsAntialiasingInvalid)
			{
				this.ReconfigureAntialiasing();

				IsAntialiasingInvalid = false;
			}

			if(IsTransformationInvalid)
			{
				Matrix3X2 transformation = ActiveTransformation;

				SharpDX.Matrix3x2 matrix = SharpDX.Matrix3x2.Identity;

				matrix.M11 = transformation.M11;
				matrix.M12 = transformation.M12;
				matrix.M21 = transformation.M21;
				matrix.M22 = transformation.M22;
				matrix.M31 = transformation.M31;
				matrix.M32 = transformation.M32;

				this._TargetSurface.Target2D.Transform = matrix;

				IsTransformationInvalid = false;
			}
		}

		private void ReconfigureBrush()
		{
			switch(this._ActiveBrushState.BrushType)
			{
				case BrushType.None:
					this._ActiveBrush =
						this._TargetSurface.GetSolidColorBrush(
							this._ActiveBrushState.SolidColorColor);
					break;
				case BrushType.SolidColor:
					this._ActiveBrush =
						this._TargetSurface.GetSolidColorBrush(
							this._ActiveBrushState.SolidColorColor);
					break;
				case BrushType.LinearGradient:
					this._ActiveBrush =
						this._TargetSurface.GetLinearGradientBrush(
							this._ActiveBrushState.LinearGradientStart,
							this._ActiveBrushState.LinearGradientEnd,
							this._ActiveBrushState.Stops);
					break;
				case BrushType.RadialGradient:
					this._ActiveBrush =
						this._TargetSurface.GetRadialGradientBrush(
							this._ActiveBrushState.RadialGradientCenter,
							this._ActiveBrushState.RadialGradientOffset,
							this._ActiveBrushState.RadialGradientRadius,
							this._ActiveBrushState.Stops);
					break;
				case BrushType.Pattern:
					this._ActiveBrush =
						this._TargetSurface.GetPatternBrush(
							this._ActiveBrushState.PatternSurface,
							this._ActiveBrushState.PatternRepetition);
					break;
			}
		}

		private void ReconfigureAntialiasing()
		{
			this._TargetSurface.Target2D.AntialiasMode = ActiveAntialiasing ==
			                                             Antialiasing.Default
			                                             	? AntialiasMode.
			                                             	  	PerPrimitive
			                                             	: AntialiasMode.
			                                             	  	Aliased;
		}

		private void ReconfigureStrokeStyle()
		{
			this._StrokeStyle.SafeDispose();

			StrokeStyleProperties newStyle = new StrokeStyleProperties
			{
				LineJoin = ActiveStrokeJoin.ToDirectWrite(),
				MiterLimit = ActiveMiterLimit,
				DashCap = ActiveDashCap.ToDirectWrite(),
				EndCap = ActiveStrokeCap.ToDirectWrite(),
				StartCap = ActiveStrokeCap.ToDirectWrite(),
				DashStyle = ActiveLineStyle.ToDirectWrite()
			};

			this._StrokeStyle = new StrokeStyle(
				this._Factory2D, newStyle, new float[0]);
		}

		internal struct BrushState
		{
			public BrushType BrushType;
			public Point LinearGradientEnd;
			public Point LinearGradientStart;
			public Repetition PatternRepetition;
			public Surface2D PatternSurface;
			public Point RadialGradientCenter;
			public Point RadialGradientOffset;
			public Size RadialGradientRadius;
			public Color SolidColorColor;
			public Gradient Stops;
		}

		internal enum BrushType
		{
			None,
			SolidColor,
			LinearGradient,
			RadialGradient,
			Pattern
		}

		public struct State
		{
			public LineCap DashCap;
			public Antialiasing IsAntialiased;
			public LineStyle LineStyle;
			public float MiterLimit;
			public LineCap StrokeCap;
			public LineJoin StrokeJoin;
			public float StrokeWidth;
			public Matrix3X2 Transformation;
		}
	}
}