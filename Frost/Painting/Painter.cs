// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Atlasing;
using Frost.Shaping;

namespace Frost.Painting
{
	public abstract class Painter
	{
		private readonly Thread _BoundThread;
		private readonly Device2D _Device2D;

		private bool _ActiveAntialiasing;
		private LineCap _ActiveDashCap;
		private LineStyle _ActiveLineStyle;
		private float _ActiveMiterLimit;
		private LineCap _ActiveStrokeCap;
		private LineJoin _ActiveStrokeJoin;
		private float _ActiveStrokeWidth;
		private Matrix3X2 _ActiveTransformation;

		private bool _IsAntialiasingInvalid;
		private bool _IsDashCapInvalid;
		private bool _IsLineStyleInvalid;
		private bool _IsMiterLimitInvalid;
		private bool _IsStrokeCapInvalid;
		private bool _IsStrokeJoinInvalid;
		private bool _IsStrokeWidthInvalid;
		private bool _IsTransformationInvalid;

		protected Painter(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			this._BoundThread = Thread.CurrentThread;
			this._Device2D = device2D;

			Contract.Assert(Device2D.Equals(device2D));
		}

		protected bool IsTransformationInvalid
		{
			get { return this._IsTransformationInvalid; }
			set { this._IsTransformationInvalid = value; }
		}

		protected bool IsStrokeWidthInvalid
		{
			get { return this._IsStrokeWidthInvalid; }
			set { this._IsStrokeWidthInvalid = value; }
		}

		protected bool IsStrokeJoinInvalid
		{
			get { return this._IsStrokeJoinInvalid; }
			set { this._IsStrokeJoinInvalid = value; }
		}

		protected bool IsStrokeCapInvalid
		{
			get { return this._IsStrokeCapInvalid; }
			set { this._IsStrokeCapInvalid = value; }
		}

		protected bool IsMiterLimitInvalid
		{
			get { return this._IsMiterLimitInvalid; }
			set { this._IsMiterLimitInvalid = value; }
		}

		protected bool IsLineStyleInvalid
		{
			get { return this._IsLineStyleInvalid; }
			set { this._IsLineStyleInvalid = value; }
		}

		protected bool IsDashCapInvalid
		{
			get { return this._IsDashCapInvalid; }
			set { this._IsDashCapInvalid = value; }
		}

		protected bool IsAntialiasingInvalid
		{
			get { return this._IsAntialiasingInvalid; }
			set { this._IsAntialiasingInvalid = value; }
		}

		protected Matrix3X2 ActiveTransformation
		{
			get { return this._ActiveTransformation; }
		}

		protected float ActiveStrokeWidth
		{
			get { return this._ActiveStrokeWidth; }
		}

		protected LineJoin ActiveStrokeJoin
		{
			get { return this._ActiveStrokeJoin; }
		}

		protected LineCap ActiveStrokeCap
		{
			get { return this._ActiveStrokeCap; }
		}

		protected float ActiveMiterLimit
		{
			get { return this._ActiveMiterLimit; }
		}

		protected LineStyle ActiveLineStyle
		{
			get { return this._ActiveLineStyle; }
		}

		protected LineCap ActiveDashCap
		{
			get { return this._ActiveDashCap; }
		}

		protected bool ActiveAntialiasing
		{
			get { return this._ActiveAntialiasing; }
		}

		public Device2D Device2D
		{
			get
			{
				Contract.Ensures(
					Contract.Result<Device2D>().Equals(this._Device2D));

				return this._Device2D;
			}
		}

		public Thread BoundThread
		{
			get
			{
				Contract.Ensures(
					Contract.Result<Thread>().Equals(this._BoundThread));

				return this._BoundThread;
			}
		}

		public float MiterLimit
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(
					Contract.Result<float>().Equals(this._ActiveMiterLimit));

				return this._ActiveMiterLimit;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(Check.IsPositive(value));

				if(!value.Equals(this._ActiveMiterLimit))
				{
					this._ActiveMiterLimit = value;

					this._IsMiterLimitInvalid = true;
				}
			}
		}

		public float StrokeWidth
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<float>().Equals(this._ActiveStrokeWidth));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				return this._ActiveStrokeWidth;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(Check.IsPositive(value));

				if(!value.Equals(this._ActiveStrokeWidth))
				{
					this._ActiveStrokeWidth = value;

					this._IsStrokeWidthInvalid = true;
				}
			}
		}

		public LineCap StrokeCap
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<LineCap>() == this._ActiveStrokeCap);

				return this._ActiveStrokeCap;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(value != this._ActiveStrokeCap)
				{
					this._ActiveStrokeCap = value;

					this._IsStrokeCapInvalid = true;
				}
			}
		}

		public LineJoin StrokeJoin
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<LineJoin>() == this._ActiveStrokeJoin);

				return this._ActiveStrokeJoin;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(value != this._ActiveStrokeJoin)
				{
					this._ActiveStrokeJoin = value;

					this._IsStrokeJoinInvalid = true;
				}
			}
		}

		public bool IsAntialiased
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<bool>().Equals(this._ActiveAntialiasing));

				return this._ActiveAntialiasing;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(value != this._ActiveAntialiasing)
				{
					this._ActiveAntialiasing = value;

					this._IsAntialiasingInvalid = true;
				}
			}
		}

		public LineCap DashCap
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<LineCap>() == this._ActiveDashCap);

				return this._ActiveDashCap;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(value != this._ActiveDashCap)
				{
					this._ActiveDashCap = value;

					this._IsDashCapInvalid = true;
				}
			}
		}

		public LineStyle LineStyle
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<LineStyle>() == this._ActiveLineStyle);

				return this._ActiveLineStyle;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(value != this._ActiveLineStyle)
				{
					this._ActiveLineStyle = value;

					this._IsLineStyleInvalid = true;
				}
			}
		}

		public Matrix3X2 Transformation
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(
					Contract.Result<Matrix3X2>().Equals(this._ActiveTransformation));

				return this._ActiveTransformation;
			}
		}

		public void Begin(
			Canvas target, Retention retention = Retention.ClearData)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(target, Device2D));

			this._IsAntialiasingInvalid = true;
			this._IsDashCapInvalid = true;
			this._IsLineStyleInvalid = true;
			this._IsMiterLimitInvalid = true;
			this._IsStrokeCapInvalid = true;
			this._IsStrokeJoinInvalid = true;
			this._IsStrokeWidthInvalid = true;
			this._IsTransformationInvalid = true;

			ResetState();

			OnBegin(target, retention);
		}

		public void End()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnEnd();
		}

		public void Clear()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnClear();
		}

		public void Clear(Rectangle region)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnClear(ref region);
		}

		public void ResetState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			IsAntialiased = true;
			MiterLimit = 10.0f;
			StrokeCap = LineCap.Butt;
			StrokeJoin = LineJoin.Miter;
			StrokeWidth = 1.0f;
			DashCap = LineCap.Butt;
			LineStyle = LineStyle.Solid;

			Matrix3X2 identity = Matrix3X2.Identity;

			Transform(ref identity, TransformMode.Replace);

			OnResetState();
		}

		public void SetBrush(Color color)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnSetBrush(color);
		}

		public void SaveState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnSaveState();
		}

		public void RestoreState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnRestoreState();
		}

		public void Stroke(Point lineStart, Point lineEnd)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnStroke(ref lineStart, ref lineEnd);
		}

		public void Stroke(Rectangle rectangleRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnStroke(ref rectangleRegion);
		}

		public void Stroke(Rectangle rectangleRegion, Size roundedRadius)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsPositive(roundedRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRadius.Height));

			OnStroke(ref rectangleRegion, ref roundedRadius);
		}

		public void Fill(Rectangle rectangleRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnFill(ref rectangleRegion);
		}

		public void Fill(Rectangle rectangleRegion, Size roundedRadius)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsPositive(roundedRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRadius.Height));

			OnFill(ref rectangleRegion, ref roundedRadius);
		}

		public void Stroke(Geometry geometry)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(geometry != null);

			OnStroke(geometry);
		}

		public void Fill(Geometry geometry)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(geometry != null);

			OnFill(geometry);
		}

		public void SetBrush(
			Point linearGradientStart,
			Point linearGradientEnd,
			Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(gradient != null);

			OnSetBrush(
				ref linearGradientStart, ref linearGradientEnd, gradient);
		}

		public void SetBrush(
			Point radialGradientCenter,
			Point radialGradientOffset,
			Size radialGradientRadius,
			Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsPositive(radialGradientRadius.Width));
			Contract.Requires(Check.IsPositive(radialGradientRadius.Height));
			Contract.Requires(gradient != null);

			OnSetBrush(
				ref radialGradientCenter,
				ref radialGradientOffset,
				ref radialGradientRadius,
				gradient);
		}

		public void SetBrush(Canvas source, Repetition extension)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));

			OnSetBrush(source, extension);
		}

		public void Clear(float x, float y, float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			Rectangle region = new Rectangle(x, y, x + width, y + height);

			OnClear(ref region);
		}

		public void Stroke(
			float lineStartX, float lineStartY, float lineEndX, float lineEndY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(lineStartX));
			Contract.Requires(Check.IsFinite(lineStartY));
			Contract.Requires(Check.IsFinite(lineEndX));
			Contract.Requires(Check.IsFinite(lineEndY));

			Point start = new Point(lineStartX, lineStartY);
			Point end = new Point(lineEndX, lineEndY);

			OnStroke(ref start, ref end);
		}

		public void Stroke(
			float rectangleX,
			float rectangleY,
			float rectangleWidth,
			float rectangleHeight,
			float roundedRadiusWidth,
			float roundedRadiusHeight)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(rectangleX));
			Contract.Requires(Check.IsFinite(rectangleY));
			Contract.Requires(Check.IsPositive(rectangleWidth));
			Contract.Requires(Check.IsPositive(rectangleHeight));
			Contract.Requires(Check.IsPositive(roundedRadiusWidth));
			Contract.Requires(Check.IsPositive(roundedRadiusHeight));

			Rectangle region = new Rectangle(
				rectangleX,
				rectangleY,
				rectangleX + rectangleWidth,
				rectangleY + rectangleHeight);

			Size radius = new Size(roundedRadiusWidth, roundedRadiusHeight);

			if(radius == Size.Empty)
			{
				OnStroke(ref region);
			}
			else
			{
				OnStroke(ref region, ref radius);
			}
		}

		public void Fill(
			float rectangleX,
			float rectangleY,
			float rectangleWidth,
			float rectangleHeight,
			float roundedRadiusWidth,
			float roundedRadiusHeight)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(rectangleX));
			Contract.Requires(Check.IsFinite(rectangleY));
			Contract.Requires(Check.IsPositive(rectangleWidth));
			Contract.Requires(Check.IsPositive(rectangleHeight));
			Contract.Requires(Check.IsPositive(roundedRadiusWidth));
			Contract.Requires(Check.IsPositive(roundedRadiusHeight));

			Rectangle region = new Rectangle(
				rectangleX,
				rectangleY,
				rectangleX + rectangleWidth,
				rectangleY + rectangleHeight);

			Size radius = new Size(roundedRadiusWidth, roundedRadiusHeight);

			if(radius == Size.Empty)
			{
				OnFill(ref region);
			}
			else
			{
				OnFill(ref region, ref radius);
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
			Contract.Requires(Check.IsFinite(linearGradientStartX));
			Contract.Requires(Check.IsFinite(linearGradientStartY));
			Contract.Requires(Check.IsFinite(linearGradientEndX));
			Contract.Requires(Check.IsFinite(linearGradientEndY));
			Contract.Requires(gradient != null);

			Point linearGradientStart = new Point(
				linearGradientStartX, linearGradientStartY);
			Point linearGradientEnd = new Point(
				linearGradientEndX, linearGradientEndY);

			OnSetBrush(
				ref linearGradientStart, ref linearGradientEnd, gradient);
		}

		public void SetBrush(
			float radialGradientCenterX,
			float radialGradientCenterY,
			float radialGradientOffsetX,
			float radialGradientOffsetY,
			float radialGradientRadiusWidth,
			float radialGradientRadiusHeight,
			Gradient gradient)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(radialGradientCenterX));
			Contract.Requires(Check.IsFinite(radialGradientCenterY));
			Contract.Requires(Check.IsFinite(radialGradientOffsetX));
			Contract.Requires(Check.IsFinite(radialGradientOffsetY));
			Contract.Requires(Check.IsPositive(radialGradientRadiusWidth));
			Contract.Requires(Check.IsPositive(radialGradientRadiusHeight));
			Contract.Requires(gradient != null);

			Point radialGradientCenter = new Point(
				radialGradientCenterX, radialGradientCenterY);
			Point radialGradientOffset = new Point(
				radialGradientOffsetX, radialGradientOffsetY);
			Size radialGradientRadius = new Size(
				radialGradientRadiusWidth, radialGradientRadiusHeight);

			OnSetBrush(
				ref radialGradientCenter,
				ref radialGradientOffset,
				ref radialGradientRadius,
				gradient);
		}

		public void Scale(float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			Matrix3X2 result;

			this._ActiveTransformation.Scale(width, height, out result);

			if(this._IsTransformationInvalid ||
			   !result.Equals(this._ActiveTransformation))
			{
				this._ActiveTransformation = result;

				this._IsTransformationInvalid = true;
			}
		}

		public void Scale(Size size)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			Scale(size.Width, size.Height);
		}

		public void Scale(
			float width, float height, float originX, float originY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));

			Matrix3X2 result;

			this._ActiveTransformation.Scale(
				width, height, originX, originY, out result);

			if(this._IsTransformationInvalid ||
			   !result.Equals(this._ActiveTransformation))
			{
				this._ActiveTransformation = result;

				this._IsTransformationInvalid = true;
			}
		}

		public void Skew(float angleX, float angleY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsDegrees(angleX));
			Contract.Requires(Check.IsDegrees(angleY));

			Matrix3X2 result;

			this._ActiveTransformation.Skew(angleX, angleY, out result);

			if(this._IsTransformationInvalid ||
			   !result.Equals(this._ActiveTransformation))
			{
				this._ActiveTransformation = result;

				this._IsTransformationInvalid = true;
			}
		}

		public void Rotate(float angle)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsDegrees(angle));

			Matrix3X2 result;

			this._ActiveTransformation.Rotate(angle, out result);

			if(this._IsTransformationInvalid ||
			   !result.Equals(this._ActiveTransformation))
			{
				this._ActiveTransformation = result;

				this._IsTransformationInvalid = true;
			}
		}

		public void Rotate(float angle, float originX, float originY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));

			Matrix3X2 result;

			this._ActiveTransformation.Rotate(
				angle, originX, originY, out result);

			if(this._IsTransformationInvalid ||
			   !result.Equals(this._ActiveTransformation))
			{
				this._ActiveTransformation = result;

				this._IsTransformationInvalid = true;
			}
		}

		public void Rotate(float angle, Point origin)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsDegrees(angle));

			Rotate(angle, origin.X, origin.Y);
		}

		public void Translate(float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			Matrix3X2 result;

			this._ActiveTransformation.Translate(width, height, out result);

			if(this._IsTransformationInvalid ||
			   !result.Equals(this._ActiveTransformation))
			{
				this._ActiveTransformation = result;

				this._IsTransformationInvalid = true;
			}
		}

		public void Translate(Size value)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			Translate(value.Width, value.Height);
		}

		public void Transform(
			ref Matrix3X2 transformation,
			TransformMode operation = TransformMode.Multiply)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			if(operation == TransformMode.Replace)
			{
				if(this._IsTransformationInvalid ||
				   !transformation.Equals(this._ActiveTransformation))
				{
					this._ActiveTransformation = transformation;

					this._IsTransformationInvalid = true;
				}
			}
			else
			{
				Matrix3X2 result;

				transformation.Multiply(
					ref this._ActiveTransformation, out result);

				if(this._IsTransformationInvalid ||
				   !result.Equals(this._ActiveTransformation))
				{
					this._ActiveTransformation = result;

					this._IsTransformationInvalid = true;
				}
			}
		}

		protected abstract void OnBegin(Canvas target, Retention retention);
		protected abstract void OnEnd();

		protected abstract void OnClear();
		protected abstract void OnClear(ref Rectangle region);

		protected abstract void OnStroke(ref Rectangle rectangleRegion);

		protected abstract void OnStroke(
			ref Point lineStart, ref Point lineEnd);

		protected abstract void OnStroke(
			ref Rectangle rectangleRegion, ref Size roundedRadius);

		protected abstract void OnFill(ref Rectangle rectangleRegion);

		protected abstract void OnFill(
			ref Rectangle rectangleRegion, ref Size roundedRadius);

		protected abstract void OnStroke(Geometry geometry);
		protected abstract void OnFill(Geometry geometry);

		protected abstract void OnSaveState();
		protected abstract void OnRestoreState();
		protected abstract void OnResetState();

		protected abstract void OnSetBrush(Color color);

		protected abstract void OnSetBrush(
			Canvas source, Repetition extension);

		protected abstract void OnSetBrush(
			ref Point linearGradientStart,
			ref Point linearGradientEnd,
			Gradient gradient);

		protected abstract void OnSetBrush(
			ref Point radialGradientCenter,
			ref Point radialGradientOffset,
			ref Size radialGradientRadius,
			Gradient gradient);
	}
}