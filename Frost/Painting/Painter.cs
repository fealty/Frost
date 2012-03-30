// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Shaping;

namespace Frost.Painting
{
	public abstract class Painter
	{
		private readonly Thread _BoundThread;
		private readonly Device2D _Device2D;

		private Antialiasing _ActiveAntialiasing;
		private LineCap _ActiveDashCap;
		private LineStyle _ActiveLineStyle;
		private float _ActiveMiterLimit;
		private LineCap _ActiveStrokeCap;
		private LineJoin _ActiveStrokeJoin;
		private float _ActiveStrokeWidth;
		private Canvas _ActiveTarget;
		private Matrix3X2 _ActiveTransformation;

		private bool _IsAntialiasingInvalid;
		private bool _IsDashCapInvalid;
		private bool _IsLineStyleInvalid;
		private bool _IsMiterLimitInvalid;
		private bool _IsStrokeCapInvalid;
		private bool _IsStrokeJoinInvalid;
		private bool _IsStrokeWidthInvalid;
		private bool _IsTargetEmpty;
		private bool _IsTransformationInvalid;

		private Size _TargetDelta;

		protected Painter(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			_BoundThread = Thread.CurrentThread;
			_Device2D = device2D;

			Contract.Assert(Device2D.Equals(device2D));
		}

		public Canvas ActiveTarget
		{
			get { return _ActiveTarget; }
		}

		protected bool IsTransformationInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsTransformationInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsTransformationInvalid = value;
			}
		}

		protected bool IsStrokeWidthInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsStrokeWidthInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsStrokeWidthInvalid = value;
			}
		}

		protected bool IsStrokeJoinInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsStrokeJoinInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsStrokeJoinInvalid = value;
			}
		}

		protected bool IsStrokeCapInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsStrokeCapInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsStrokeCapInvalid = value;
			}
		}

		protected bool IsMiterLimitInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsMiterLimitInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsMiterLimitInvalid = value;
			}
		}

		protected bool IsLineStyleInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsLineStyleInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsLineStyleInvalid = value;
			}
		}

		protected bool IsDashCapInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsDashCapInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsDashCapInvalid = value;
			}
		}

		protected bool IsAntialiasingInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _IsAntialiasingInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);

				_IsAntialiasingInvalid = value;
			}
		}

		protected Matrix3X2 ActiveTransformation
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveTransformation;
			}
		}

		protected float ActiveStrokeWidth
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveStrokeWidth;
			}
		}

		protected LineJoin ActiveStrokeJoin
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveStrokeJoin;
			}
		}

		protected LineCap ActiveStrokeCap
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveStrokeCap;
			}
		}

		protected float ActiveMiterLimit
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveMiterLimit;
			}
		}

		protected LineStyle ActiveLineStyle
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveLineStyle;
			}
		}

		protected LineCap ActiveDashCap
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveDashCap;
			}
		}

		protected Antialiasing ActiveAntialiasing
		{
			get
			{
				Contract.Requires(ActiveTarget != null);

				return _ActiveAntialiasing;
			}
		}

		public Device2D Device2D
		{
			get
			{
				Contract.Ensures(Contract.Result<Device2D>().Equals(_Device2D));

				return _Device2D;
			}
		}

		public Thread BoundThread
		{
			get
			{
				Contract.Ensures(Contract.Result<Thread>().Equals(_BoundThread));

				return _BoundThread;
			}
		}

		public float MiterLimit
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_ActiveMiterLimit));

				return _ActiveMiterLimit;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Requires(Check.IsPositive(value));

				if(!value.Equals(_ActiveMiterLimit))
				{
					_ActiveMiterLimit = value;

					_IsMiterLimitInvalid = true;
				}
			}
		}

		public float StrokeWidth
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<float>().Equals(_ActiveStrokeWidth));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				return _ActiveStrokeWidth;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Requires(Check.IsPositive(value));

				if(!value.Equals(_ActiveStrokeWidth))
				{
					_ActiveStrokeWidth = value;

					_IsStrokeWidthInvalid = true;
				}
			}
		}

		public LineCap StrokeCap
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<LineCap>() == _ActiveStrokeCap);

				return _ActiveStrokeCap;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

				if(value != _ActiveStrokeCap)
				{
					_ActiveStrokeCap = value;

					_IsStrokeCapInvalid = true;
				}
			}
		}

		public LineJoin StrokeJoin
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<LineJoin>() == _ActiveStrokeJoin);

				return _ActiveStrokeJoin;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

				if(value != _ActiveStrokeJoin)
				{
					_ActiveStrokeJoin = value;

					_IsStrokeJoinInvalid = true;
				}
			}
		}

		public Antialiasing IsAntialiased
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<Antialiasing>().Equals(_ActiveAntialiasing));

				return _ActiveAntialiasing;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

				if(value != _ActiveAntialiasing)
				{
					_ActiveAntialiasing = value;

					_IsAntialiasingInvalid = true;
				}
			}
		}

		public LineCap DashCap
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<LineCap>() == _ActiveDashCap);

				return _ActiveDashCap;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

				if(value != _ActiveDashCap)
				{
					_ActiveDashCap = value;

					_IsDashCapInvalid = true;
				}
			}
		}

		public LineStyle LineStyle
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<LineStyle>() == _ActiveLineStyle);

				return _ActiveLineStyle;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

				if(value != _ActiveLineStyle)
				{
					_ActiveLineStyle = value;

					_IsLineStyleInvalid = true;
				}
			}
		}

		public Matrix3X2 Transformation
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<Matrix3X2>().Equals(_ActiveTransformation));

				return _ActiveTransformation;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

				if(_IsTransformationInvalid || !value.Equals(_ActiveTransformation))
				{
					_ActiveTransformation = value;

					IsTransformationInvalid = true;
				}
			}
		}

		public void Begin(Canvas target, Retention retention = Retention.ClearData)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget == null);
			Contract.Requires(target != null);

			bool isTargetEmpty = target.Region.IsEmpty;

			_IsAntialiasingInvalid = true;
			_IsDashCapInvalid = true;
			_IsLineStyleInvalid = true;
			_IsMiterLimitInvalid = true;
			_IsStrokeCapInvalid = true;
			_IsStrokeJoinInvalid = true;
			_IsStrokeWidthInvalid = true;
			_IsTransformationInvalid = true;

			_ActiveTarget = target;

			ResetState();

			if(!isTargetEmpty)
			{
				var targetContext = _Device2D.Resources.Resolve(target);

				_TargetDelta = targetContext.Region.Location;

				OnBegin(targetContext, retention);
			}
			else
			{
				// permit only nop and state operations on empty targets
				_TargetDelta = Size.Empty;
			}

			_IsTargetEmpty = isTargetEmpty;
		}

		public void End()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnEnd();
			}

			_ActiveTarget = null;
		}

		public void Clear()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnClear();
			}
		}

		public void Clear(Rectangle region)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				OnClear(ref region);
			}
		}

		public void ResetState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			IsAntialiased = Antialiasing.Default;
			MiterLimit = 10.0f;
			StrokeCap = LineCap.Butt;
			StrokeJoin = LineJoin.Miter;
			StrokeWidth = 1.0f;
			DashCap = LineCap.Butt;
			LineStyle = LineStyle.Solid;
			Transformation = Matrix3X2.Identity;

			OnResetState();
		}

		public void SetBrush(Color color)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnSetBrush(color);
			}
		}

		public void SaveState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			OnSaveState();
		}

		public void RestoreState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			OnRestoreState();
		}

		public void StrokeLine(Point lineStart, Point lineEnd)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				lineStart = lineStart.Translate(_TargetDelta);
				lineEnd = lineEnd.Translate(_TargetDelta);

				OnStrokeLine(ref lineStart, ref lineEnd);
			}
		}

		public void StrokeRectangle(Rectangle rectangleRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				rectangleRegion = rectangleRegion.Translate(_TargetDelta);

				OnStrokeRectangle(ref rectangleRegion);
			}
		}

		public void StrokeRectangle(Rectangle rectangleRegion, Size roundedRadius)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsPositive(roundedRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRadius.Height));

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				rectangleRegion = rectangleRegion.Translate(_TargetDelta);

				OnStrokeRectangle(ref rectangleRegion, ref roundedRadius);
			}
		}

		public void FillRectangle(Rectangle rectangleRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				rectangleRegion = rectangleRegion.Translate(_TargetDelta);

				OnFillRectangle(ref rectangleRegion);
			}
		}

		public void FillRectangle(Rectangle rectangleRegion, Size roundedRadius)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsPositive(roundedRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRadius.Height));

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				rectangleRegion = rectangleRegion.Translate(_TargetDelta);

				OnFillRectangle(ref rectangleRegion, ref roundedRadius);
			}
		}

		public void Stroke(Geometry geometry)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(geometry != null);

			if(!_IsTargetEmpty)
			{
				OnStroke(geometry);
			}
		}

		public void Fill(Geometry geometry)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(geometry != null);

			if(!_IsTargetEmpty)
			{
				OnFill(geometry);
			}
		}

		public void SetBrush(Point linearGradientStart, Point linearGradientEnd, Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(gradient != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				linearGradientStart = linearGradientStart.Translate(_TargetDelta);
				linearGradientEnd = linearGradientEnd.Translate(_TargetDelta);

				OnSetBrush(ref linearGradientStart, ref linearGradientEnd, gradient);
			}
		}

		public void SetBrush(
			Point radialGradientCenter,
			Size radialGradientOffset,
			Size radialGradientRadius,
			Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsPositive(radialGradientRadius.Width));
			Contract.Requires(Check.IsPositive(radialGradientRadius.Height));
			Contract.Requires(gradient != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				radialGradientCenter = radialGradientCenter.Translate(_TargetDelta);

				OnSetBrush(
					ref radialGradientCenter, ref radialGradientOffset, ref radialGradientRadius, gradient);
			}
		}

		public void SetBrush(Canvas source, Repetition extension)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				OnSetBrush(_Device2D.Resources.Resolve(source), extension);
			}
		}

		public void Clear(float x, float y, float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			if(!_IsTargetEmpty)
			{
				Rectangle region = new Rectangle(x, y, width, height);

				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				OnClear(ref region);
			}
		}

		public void StrokeLine(float lineStartX, float lineStartY, float lineEndX, float lineEndY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(lineStartX));
			Contract.Requires(Check.IsFinite(lineStartY));
			Contract.Requires(Check.IsFinite(lineEndX));
			Contract.Requires(Check.IsFinite(lineEndY));

			if(!_IsTargetEmpty)
			{
				Point start = new Point(lineStartX, lineStartY);
				Point end = new Point(lineEndX, lineEndY);

				// translate to 2D surface coordinate space
				start = start.Translate(_TargetDelta);
				end = end.Translate(_TargetDelta);

				OnStrokeLine(ref start, ref end);
			}
		}

		public void StrokeRectangle(
			float rectangleX,
			float rectangleY,
			float rectangleWidth,
			float rectangleHeight,
			float roundedRadiusWidth = 0.0f,
			float roundedRadiusHeight = 0.0f)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(rectangleX));
			Contract.Requires(Check.IsFinite(rectangleY));
			Contract.Requires(Check.IsPositive(rectangleWidth));
			Contract.Requires(Check.IsPositive(rectangleHeight));
			Contract.Requires(Check.IsPositive(roundedRadiusWidth));
			Contract.Requires(Check.IsPositive(roundedRadiusHeight));

			if(!_IsTargetEmpty)
			{
				Rectangle region = new Rectangle(rectangleX, rectangleY, rectangleWidth, rectangleHeight);

				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				Size radius = new Size(roundedRadiusWidth, roundedRadiusHeight);

				if(radius == Size.Empty)
				{
					OnStrokeRectangle(ref region);
				}
				else
				{
					OnStrokeRectangle(ref region, ref radius);
				}
			}
		}

		public void FillRectangle(
			float rectangleX,
			float rectangleY,
			float rectangleWidth,
			float rectangleHeight,
			float roundedRadiusWidth = 0.0f,
			float roundedRadiusHeight = 0.0f)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(rectangleX));
			Contract.Requires(Check.IsFinite(rectangleY));
			Contract.Requires(Check.IsPositive(rectangleWidth));
			Contract.Requires(Check.IsPositive(rectangleHeight));
			Contract.Requires(Check.IsPositive(roundedRadiusWidth));
			Contract.Requires(Check.IsPositive(roundedRadiusHeight));

			if(!_IsTargetEmpty)
			{
				Rectangle region = new Rectangle(rectangleX, rectangleY, rectangleWidth, rectangleHeight);

				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				Size radius = new Size(roundedRadiusWidth, roundedRadiusHeight);

				if(radius == Size.Empty)
				{
					OnFillRectangle(ref region);
				}
				else
				{
					OnFillRectangle(ref region, ref radius);
				}
			}
		}

		public void SetBrush(
			float linearGradientStartX,
			float linearGradientStartY,
			float linearGradientEndX,
			float linearGradientEndY,
			Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(linearGradientStartX));
			Contract.Requires(Check.IsFinite(linearGradientStartY));
			Contract.Requires(Check.IsFinite(linearGradientEndX));
			Contract.Requires(Check.IsFinite(linearGradientEndY));
			Contract.Requires(gradient != null);

			if(!_IsTargetEmpty)
			{
				Point linearGradientStart = new Point(linearGradientStartX, linearGradientStartY);
				Point linearGradientEnd = new Point(linearGradientEndX, linearGradientEndY);

				linearGradientStart = linearGradientStart.Translate(_TargetDelta);
				linearGradientEnd = linearGradientEnd.Translate(_TargetDelta);

				OnSetBrush(ref linearGradientStart, ref linearGradientEnd, gradient);
			}
		}

		public void SetBrush(
			float radialGradientCenterX,
			float radialGradientCenterY,
			float radialGradientOffsetWidth,
			float radialGradientOffsetHeight,
			float radialGradientRadiusWidth,
			float radialGradientRadiusHeight,
			Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(radialGradientCenterX));
			Contract.Requires(Check.IsFinite(radialGradientCenterY));
			Contract.Requires(Check.IsFinite(radialGradientOffsetWidth));
			Contract.Requires(Check.IsFinite(radialGradientOffsetHeight));
			Contract.Requires(Check.IsPositive(radialGradientRadiusWidth));
			Contract.Requires(Check.IsPositive(radialGradientRadiusHeight));
			Contract.Requires(gradient != null);

			if(!_IsTargetEmpty)
			{
				Point radialGradientCenter = new Point(radialGradientCenterX, radialGradientCenterY);

				// translate to 2D surface coordinate space
				radialGradientCenter = radialGradientCenter.Translate(_TargetDelta);

				Size radialGradientOffset = new Size(radialGradientOffsetWidth, radialGradientOffsetHeight);
				Size radialGradientRadius = new Size(radialGradientRadiusWidth, radialGradientRadiusHeight);

				OnSetBrush(
					ref radialGradientCenter, ref radialGradientOffset, ref radialGradientRadius, gradient);
			}
		}

		public void Scale(float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			Matrix3X2 result;

			_ActiveTransformation.Scale(width, height, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		public void Scale(Size size)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			Scale(size.Width, size.Height);
		}

		public void Scale(float width, float height, float originX, float originY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));

			Matrix3X2 result;

			_ActiveTransformation.Scale(width, height, originX, originY, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		public void Skew(float angleX, float angleY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsDegrees(angleX));
			Contract.Requires(Check.IsDegrees(angleY));

			Matrix3X2 result;

			_ActiveTransformation.Skew(angleX, angleY, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		public void Rotate(float angle)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsDegrees(angle));

			Matrix3X2 result;

			_ActiveTransformation.Rotate(angle, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		public void Rotate(float angle, float originX, float originY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));

			Matrix3X2 result;

			_ActiveTransformation.Rotate(angle, originX, originY, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		public void Rotate(float angle, Point origin)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsDegrees(angle));

			Rotate(angle, origin.X, origin.Y);
		}

		public void Translate(float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			Matrix3X2 result;

			_ActiveTransformation.Translate(width, height, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		public void Translate(Size value)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			Translate(value.Width, value.Height);
		}

		public void Transform(ref Matrix3X2 transformation)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			Matrix3X2 result;

			transformation.Multiply(ref _ActiveTransformation, out result);

			if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
			{
				_ActiveTransformation = result;

				_IsTransformationInvalid = true;
			}
		}

		protected abstract void OnBegin(Canvas.ResolvedContext target, Retention retention);
		protected abstract void OnEnd();

		protected abstract void OnClear();
		protected abstract void OnClear(ref Rectangle region);

		protected abstract void OnStrokeRectangle(ref Rectangle rectangleRegion);

		protected abstract void OnStrokeLine(ref Point lineStart, ref Point lineEnd);

		protected abstract void OnStrokeRectangle(ref Rectangle rectangleRegion, ref Size roundedRadius);

		protected abstract void OnFillRectangle(ref Rectangle rectangleRegion);

		protected abstract void OnFillRectangle(ref Rectangle rectangleRegion, ref Size roundedRadius);

		protected abstract void OnStroke(Geometry geometry);
		protected abstract void OnFill(Geometry geometry);

		protected abstract void OnSaveState();
		protected abstract void OnRestoreState();
		protected abstract void OnResetState();

		protected abstract void OnSetBrush(Color color);

		protected abstract void OnSetBrush(Canvas.ResolvedContext source, Repetition extension);

		protected abstract void OnSetBrush(
			ref Point linearGradientStart, ref Point linearGradientEnd, Gradient gradient);

		protected abstract void OnSetBrush(
			ref Point radialGradientCenter,
			ref Size radialGradientOffset,
			ref Size radialGradientRadius,
			Gradient gradient);
	}
}