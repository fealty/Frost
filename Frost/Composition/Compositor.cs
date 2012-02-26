// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Atlasing;
using Frost.Effects;

namespace Frost.Composition
{
	public abstract class Compositor
	{
		private readonly Thread _BoundThread;
		private readonly Device2D _Device2D;

		private BlendOperation _ActiveBlendOperation;
		private EffectContext _ActiveEffectContext;
		private float _ActiveOpacity;
		private Matrix3X2 _ActiveTransformation;

		private bool _IsBlendOperationInvalid;
		private bool _IsEffectContextInvalid;
		private bool _IsOpacityInvalid;
		private bool _IsTransformationInvalid;

		protected Compositor(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			_BoundThread = Thread.CurrentThread;
			_Device2D = device2D;
		}

		protected bool IsTransformationInvalid
		{
			get { return _IsTransformationInvalid; }
			set { _IsTransformationInvalid = value; }
		}

		protected bool IsOpacityInvalid
		{
			get { return _IsOpacityInvalid; }
			set { _IsOpacityInvalid = value; }
		}

		protected bool IsEffectContextInvalid
		{
			get { return _IsEffectContextInvalid; }
			set { _IsEffectContextInvalid = value; }
		}

		protected bool IsBlendOperationInvalid
		{
			get { return _IsBlendOperationInvalid; }
			set { _IsBlendOperationInvalid = value; }
		}

		protected Matrix3X2 ActiveTransformation
		{
			get { return _ActiveTransformation; }
		}

		protected float ActiveOpacity
		{
			get { return _ActiveOpacity; }
		}

		protected EffectContext ActiveEffectContext
		{
			get { return _ActiveEffectContext; }
		}

		protected BlendOperation ActiveBlendOperation
		{
			get { return _ActiveBlendOperation; }
		}

		public Device2D Device2D
		{
			get
			{
				Contract.Ensures(Contract.Result<Device2D>().Equals(_Device2D));
				Contract.Ensures(Contract.Result<Device2D>() != null);

				return _Device2D;
			}
		}

		public Thread BoundThread
		{
			get
			{
				Contract.Ensures(Contract.Result<Thread>().Equals(_BoundThread));
				Contract.Ensures(Contract.Result<Thread>() != null);

				return _BoundThread;
			}
		}

		public EffectContext Effect
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(Contract.Result<EffectContext>() == _ActiveEffectContext);

				return _ActiveEffectContext;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(_IsEffectContextInvalid || !Equals(value, _ActiveEffectContext))
				{
					_ActiveEffectContext = value;

					_IsEffectContextInvalid = true;
				}
			}
		}

		public float Opacity
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(Contract.Result<float>().Equals(_ActiveOpacity));
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));

				return _ActiveOpacity;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(Check.IsNormalized(value));

				if(!value.Equals(_ActiveOpacity))
				{
					_ActiveOpacity = value;

					_IsOpacityInvalid = true;
				}
			}
		}

		public BlendOperation Blend
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(Contract.Result<BlendOperation>() == _ActiveBlendOperation);

				return _ActiveBlendOperation;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				if(value != _ActiveBlendOperation)
				{
					_ActiveBlendOperation = value;

					_IsBlendOperationInvalid = true;
				}
			}
		}

		public Matrix3X2 Transformation
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Ensures(Contract.Result<Matrix3X2>().Equals(_ActiveTransformation));

				return _ActiveTransformation;
			}
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

		public void ResetState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			Opacity = 1.0f;
			Effect = null;
			Blend = BlendOperation.SourceOver;

			Matrix3X2 identity = Matrix3X2.Identity;

			Transform(ref identity, TransformMode.Replace);

			OnResetState();
		}

		public void PushLayer()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnPushLayer();
		}

		public void PushLayer(Retention retentionMode)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnPushLayer(retentionMode);
		}

		public void PopLayer()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnPopLayer();
		}

		public void DiscardLayer()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnDiscardLayer();
		}

		public void Flatten()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnFlatten();
		}

		public void CopyResult(Canvas destination)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(destination, Device2D));

			OnCopyResult(destination);
		}

		public void CopyResult(Rectangle sourceRegion, Canvas destination)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(destination, Device2D));

			OnCopyResult(ref sourceRegion, destination);
		}

		public void Flush()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnFlush();
		}

		public void CompositeResult()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnCompositeResult();
		}

		public void CompositeResult(Point location)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnCompositeResult(ref location);
		}

		public void CompositeResult(Rectangle region)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnCompositeResult(ref region);
		}

		public void CompositeResult(Rectangle srcRegion, Point dstLocation)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnCompositeResult(ref srcRegion, ref dstLocation);
		}

		public void CompositeResult(Rectangle srcRegion, Rectangle dstRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnCompositeResult(ref srcRegion, ref dstRegion);
		}

		public void Composite(Canvas source)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));

			OnComposite(source);
		}

		public void Composite(Canvas source, Point location)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));

			OnComposite(source, ref location);
		}

		public void Composite(Canvas source, Rectangle region)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));

			OnComposite(source, ref region);
		}

		public void Composite(Canvas source, Rectangle srcRegion, Point dstLocation)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));

			OnComposite(source, ref srcRegion, ref dstLocation);
		}

		public void Composite(Canvas source, Rectangle srcRegion, Rectangle dstRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));

			OnComposite(source, ref srcRegion, ref dstRegion);
		}

		public void Begin(Canvas target, Retention retention = Retention.ClearData)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(target, Device2D));

			_IsBlendOperationInvalid = true;
			_IsEffectContextInvalid = true;
			_IsOpacityInvalid = true;
			_IsTransformationInvalid = true;

			ResetState();

			OnBegin(target, retention);
		}

		public void End()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnEnd();
		}

		public void CopyResult(
			float sourceX, float sourceY, float sourceWidth, float sourceHeight, Canvas destination)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(sourceX));
			Contract.Requires(Check.IsFinite(sourceY));
			Contract.Requires(Check.IsPositive(sourceWidth));
			Contract.Requires(Check.IsPositive(sourceHeight));
			Contract.Requires(Check.IsValid(destination, Device2D));

			Rectangle sourceRegion = new Rectangle(
				sourceX, sourceY, sourceX + sourceWidth, sourceY + sourceHeight);

			OnCopyResult(ref sourceRegion, destination);
		}

		public void CompositeResult(float x, float y)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));

			Point location = new Point(x, y);

			OnCompositeResult(ref location);
		}

		public void CompositeResult(float x, float y, float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			Rectangle region = new Rectangle(x, y, x + width, y + height);

			OnCompositeResult(ref region);
		}

		public void CompositeResult(
			float srcX, float srcY, float srcWidth, float srcHeight, float dstX, float dstY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));

			Rectangle srcRegion = new Rectangle(srcX, srcY, srcX + srcWidth, srcY + srcHeight);

			Point dstLocation = new Point(dstX, dstY);

			OnCompositeResult(ref srcRegion, ref dstLocation);
		}

		public void CompositeResult(
			float srcX,
			float srcY,
			float srcWidth,
			float srcHeight,
			float dstX,
			float dstY,
			float dstWidth,
			float dstHeight)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));
			Contract.Requires(Check.IsPositive(dstWidth));
			Contract.Requires(Check.IsPositive(dstHeight));

			Rectangle srcRegion = new Rectangle(srcX, srcY, srcX + srcWidth, srcY + srcHeight);

			Rectangle dstRegion = new Rectangle(dstX, dstY, dstX + dstWidth, dstY + dstHeight);

			OnCompositeResult(ref srcRegion, ref dstRegion);
		}

		public void Composite(Canvas source, float x, float y)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));

			Point location = new Point(x, y);

			OnComposite(source, ref location);
		}

		public void Composite(Canvas source, float x, float y, float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			Rectangle region = new Rectangle(x, y, x + width, y + height);

			OnComposite(source, ref region);
		}

		public void Composite(
			Canvas source, float srcX, float srcY, float srcWidth, float srcHeight, float dstX, float dstY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));

			Rectangle srcRegion = new Rectangle(srcX, srcY, srcX + srcWidth, srcY + srcHeight);

			Point dstLocation = new Point(dstX, dstY);

			OnComposite(source, ref srcRegion, ref dstLocation);
		}

		public void Composite(
			Canvas source,
			float srcX,
			float srcY,
			float srcWidth,
			float srcHeight,
			float dstX,
			float dstY,
			float dstWidth,
			float dstHeight)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Check.IsValid(source, Device2D));
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));
			Contract.Requires(Check.IsPositive(dstWidth));
			Contract.Requires(Check.IsPositive(dstHeight));

			Rectangle srcRegion = new Rectangle(srcX, srcY, srcX + srcWidth, srcY + srcHeight);

			Rectangle dstRegion = new Rectangle(dstX, dstY, dstX + dstWidth, dstY + dstHeight);

			OnComposite(source, ref srcRegion, ref dstRegion);
		}

		public void Scale(float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
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
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			Scale(size.Width, size.Height);
		}

		public void Scale(float width, float height, float originX, float originY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
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
			Contract.Requires(Check.IsDegrees(angle));

			Rotate(angle, origin.X, origin.Y);
		}

		public void Translate(float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
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

			Translate(value.Width, value.Height);
		}

		public void Transform(
			ref Matrix3X2 transformation, TransformMode operation = TransformMode.Multiply)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			if(operation == TransformMode.Replace)
			{
				if(_IsTransformationInvalid || !transformation.Equals(_ActiveTransformation))
				{
					_ActiveTransformation = transformation;

					_IsTransformationInvalid = true;
				}
			}
			else
			{
				Matrix3X2 result;

				transformation.Multiply(ref _ActiveTransformation, out result);

				if(_IsTransformationInvalid || !result.Equals(_ActiveTransformation))
				{
					_ActiveTransformation = result;

					_IsTransformationInvalid = true;
				}
			}
		}

		public void ApplyEffect<T>(T options) where T : struct, IEffectSettings, IEquatable<T>
		{
			var oldEffectContext = Effect as EffectContext<T>;

			if(oldEffectContext != null)
			{
				if(oldEffectContext.Options.Equals(options))
				{
					return;
				}
			}

			Effect<T> effect = Device2D.Effects.Find<T>();

			Effect = effect != null ? new EffectContext<T>(effect, options) : null;
		}

		protected abstract void OnSaveState();
		protected abstract void OnRestoreState();
		protected abstract void OnResetState();

		protected abstract void OnPushLayer();
		protected abstract void OnPushLayer(Retention retentionMode);

		protected abstract void OnPopLayer();
		protected abstract void OnDiscardLayer();

		protected abstract void OnFlatten();
		protected abstract void OnFlush();

		protected abstract void OnCompositeResult();
		protected abstract void OnCompositeResult(ref Point location);
		protected abstract void OnCompositeResult(ref Rectangle region);

		protected abstract void OnCompositeResult(ref Rectangle srcRegion, ref Point dstLocation);

		protected abstract void OnCompositeResult(ref Rectangle srcRegion, ref Rectangle dstRegion);

		protected abstract void OnBegin(Canvas target, Retention retention);
		protected abstract void OnEnd();

		protected abstract void OnCopyResult(Canvas destination);

		protected abstract void OnCopyResult(ref Rectangle sourceRegion, Canvas destination);

		protected abstract void OnComposite(Canvas source);

		protected abstract void OnComposite(Canvas source, ref Point location);

		protected abstract void OnComposite(Canvas source, ref Rectangle region);

		protected abstract void OnComposite(Canvas source, ref Rectangle srcRegion, ref Point dstLocation);

		protected abstract void OnComposite(
			Canvas source, ref Rectangle srcRegion, ref Rectangle dstRegion);
	}
}