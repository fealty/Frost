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

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D10;

using Geometry = Frost.Shaping.Geometry;
using LineJoin = Frost.Painting.LineJoin;

namespace Frost.DirectX.Painting
{
	internal sealed class Painter : Frost.Painting.Painter, IDisposable
	{
		private readonly Device2D _Device2D;
		private readonly Device _Device3D;
		private readonly Drawer _Drawer;
		private readonly Factory _Factory2D;

		private readonly TimeSpanCounter _FrameDuration = new TimeSpanCounter("Painting", "FrameDuration");

		private readonly Stack<State> _States;
		private readonly Stopwatch _Watch;

		private Brush _ActiveBrush;
		private BrushState _ActiveBrushState;
		private bool _IsBrushInvalid;

		private StrokeStyle _StrokeStyle;
		private Surface2D _TargetSurface;

		public Painter(Factory factory2D, Device2D device2D, Device device3D) : base(device2D)
		{
			Contract.Requires(factory2D != null);
			Contract.Requires(device3D != null);

			_States = new Stack<State>();

			_Drawer = new Drawer(factory2D);

			_Device2D = device2D;
			_Device3D = device3D;
			_Factory2D = factory2D;

			device2D.Diagnostics.Register(_FrameDuration);

			_Watch = new Stopwatch();
		}

		public TimeSpanCounter FrameDuration
		{
			get { return _FrameDuration; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected override void OnBegin(Canvas3 target, Retention retention)
		{
			_Watch.Reset();
			_Watch.Start();

			SetBrush(Color.Black);

			_IsBrushInvalid = true;

			_TargetSurface = (Surface2D)target.Surface2D;

			_TargetSurface.AcquireLock();

			_Drawer.Begin(target);
		}

		protected override void OnEnd()
		{
			try
			{
				try
				{
					_Drawer.End();
				}
				finally
				{
					_TargetSurface.ReleaseLock();

					_ActiveBrush = null;
					_TargetSurface = null;
				}
			}
			finally
			{
				_Watch.Stop();

				_FrameDuration.Value += _Watch.Elapsed;
			}
		}

		protected override void OnClear()
		{
			Reconfigure();

			_Drawer.Clear();
		}

		protected override void OnClear(ref Rectangle region)
		{
			Reconfigure();

			_Drawer.Clear(region);
		}

		protected override void OnStroke(ref Rectangle rectangleRegion)
		{
			Reconfigure();

			_Drawer.Stroke(rectangleRegion, _ActiveBrush, _StrokeStyle, ActiveStrokeWidth);
		}

		protected override void OnStroke(ref Point lineStart, ref Point lineEnd)
		{
			Reconfigure();

			_Drawer.Stroke(lineStart, lineEnd, _ActiveBrush, _StrokeStyle, ActiveStrokeWidth);
		}

		protected override void OnStroke(ref Rectangle rectangleRegion, ref Size roundedRadius)
		{
			Reconfigure();

			_Drawer.Stroke(rectangleRegion, roundedRadius, _ActiveBrush, _StrokeStyle, ActiveStrokeWidth);
		}

		protected override void OnFill(ref Rectangle rectangleRegion)
		{
			Reconfigure();

			_Drawer.Fill(rectangleRegion, _ActiveBrush);
		}

		protected override void OnFill(ref Rectangle rectangleRegion, ref Size roundedRadius)
		{
			Reconfigure();

			_Drawer.Fill(rectangleRegion, roundedRadius, _ActiveBrush);
		}

		protected override void OnStroke(Geometry geometry)
		{
			Reconfigure();

			_Drawer.Stroke(geometry, _ActiveBrush, _StrokeStyle, ActiveStrokeWidth);
		}

		protected override void OnFill(Geometry geometry)
		{
			Reconfigure();

			_Drawer.Fill(geometry, _ActiveBrush);
		}

		protected override void OnSaveState()
		{
			_States.Push(
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
			State newState = _States.Pop();

			IsAntialiased = newState.IsAntialiased;
			DashCap = newState.DashCap;
			LineStyle = newState.LineStyle;
			MiterLimit = newState.MiterLimit;
			StrokeCap = newState.StrokeCap;
			StrokeJoin = newState.StrokeJoin;
			StrokeWidth = newState.StrokeWidth;
			Transformation = newState.Transformation;
		}

		protected override void OnResetState()
		{
		}

		protected override void OnSetBrush(Color color)
		{
			_ActiveBrushState.SolidColorColor = color;

			_ActiveBrushState.BrushType = BrushType.SolidColor;

			_IsBrushInvalid = true;
		}

		protected override void OnSetBrush(Canvas3 source, Repetition extension)
		{
			_ActiveBrushState.PatternSurface.SafeDispose();

			Surface2D.Description description;

			description.Device2D = _Device2D;
			description.Device3D = _Device3D;
			description.Usage = SurfaceUsage.Normal;
			description.Size = source.Region.Size;
			description.Factory2D = null;

			_ActiveBrushState.PatternSurface = Surface2D.FromDescription(ref description);

			source.Surface2D.CopyTo(source.Region, _ActiveBrushState.PatternSurface, Point.Empty);

			_ActiveBrushState.PatternRepetition = extension;

			_ActiveBrushState.BrushType = BrushType.Pattern;

			_IsBrushInvalid = true;
		}

		protected override void OnSetBrush(
			ref Point linearGradientStart, ref Point linearGradientEnd, Gradient gradient)
		{
			_ActiveBrushState.Stops = gradient;

			_ActiveBrushState.LinearGradientStart = linearGradientStart;
			_ActiveBrushState.LinearGradientEnd = linearGradientEnd;

			_ActiveBrushState.BrushType = BrushType.LinearGradient;

			_IsBrushInvalid = true;
		}

		protected override void OnSetBrush(
			ref Point radialGradientCenter,
			ref Point radialGradientOffset,
			ref Size radialGradientRadius,
			Gradient gradient)
		{
			_ActiveBrushState.Stops = gradient;

			_ActiveBrushState.RadialGradientCenter = radialGradientCenter;
			_ActiveBrushState.RadialGradientOffset = radialGradientOffset;
			_ActiveBrushState.RadialGradientRadius = radialGradientRadius;

			_ActiveBrushState.BrushType = BrushType.RadialGradient;

			_IsBrushInvalid = true;
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_Drawer.Dispose();
				_ActiveBrushState.PatternSurface.SafeDispose();
				_StrokeStyle.SafeDispose();
			}
		}

		private void Reconfigure()
		{
			if(_IsBrushInvalid)
			{
				ReconfigureBrush();

				_IsBrushInvalid = false;
			}

			if(IsLineStyleInvalid || IsMiterLimitInvalid || IsStrokeCapInvalid || IsStrokeJoinInvalid ||
			   IsDashCapInvalid)
			{
				ReconfigureStrokeStyle();

				IsLineStyleInvalid = false;
				IsMiterLimitInvalid = false;
				IsStrokeCapInvalid = false;
				IsStrokeJoinInvalid = false;
				IsDashCapInvalid = false;
			}

			if(IsAntialiasingInvalid)
			{
				ReconfigureAntialiasing();

				IsAntialiasingInvalid = false;
			}

			if(IsTransformationInvalid)
			{
				Matrix3X2 transformation = ActiveTransformation;

				Matrix3x2 matrix = Matrix3x2.Identity;

				matrix.M11 = transformation.M11;
				matrix.M12 = transformation.M12;
				matrix.M21 = transformation.M21;
				matrix.M22 = transformation.M22;
				matrix.M31 = transformation.M31;
				matrix.M32 = transformation.M32;

				_TargetSurface.Target2D.Transform = matrix;

				IsTransformationInvalid = false;
			}
		}

		private void ReconfigureBrush()
		{
			switch(_ActiveBrushState.BrushType)
			{
				case BrushType.None:
					_ActiveBrush = _TargetSurface.GetSolidColorBrush(_ActiveBrushState.SolidColorColor);
					break;
				case BrushType.SolidColor:
					_ActiveBrush = _TargetSurface.GetSolidColorBrush(_ActiveBrushState.SolidColorColor);
					break;
				case BrushType.LinearGradient:
					_ActiveBrush = _TargetSurface.GetLinearGradientBrush(
						_ActiveBrushState.LinearGradientStart,
						_ActiveBrushState.LinearGradientEnd,
						_ActiveBrushState.Stops);
					break;
				case BrushType.RadialGradient:
					_ActiveBrush = _TargetSurface.GetRadialGradientBrush(
						_ActiveBrushState.RadialGradientCenter,
						_ActiveBrushState.RadialGradientOffset,
						_ActiveBrushState.RadialGradientRadius,
						_ActiveBrushState.Stops);
					break;
				case BrushType.Pattern:
					_ActiveBrush = _TargetSurface.GetPatternBrush(
						_ActiveBrushState.PatternSurface, _ActiveBrushState.PatternRepetition);
					break;
			}
		}

		private void ReconfigureAntialiasing()
		{
			_TargetSurface.Target2D.AntialiasMode = ActiveAntialiasing == Antialiasing.Default
			                                        	? AntialiasMode.PerPrimitive
			                                        	: AntialiasMode.Aliased;
		}

		private void ReconfigureStrokeStyle()
		{
			_StrokeStyle.SafeDispose();

			StrokeStyleProperties newStyle = new StrokeStyleProperties
			{
				LineJoin = ActiveStrokeJoin.ToDirectWrite(),
				MiterLimit = ActiveMiterLimit,
				DashCap = ActiveDashCap.ToDirectWrite(),
				EndCap = ActiveStrokeCap.ToDirectWrite(),
				StartCap = ActiveStrokeCap.ToDirectWrite(),
				DashStyle = ActiveLineStyle.ToDirectWrite()
			};

			_StrokeStyle = new StrokeStyle(_Factory2D, newStyle, new float[0]);
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