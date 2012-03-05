// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Threading;

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
		private Canvas _ActiveTarget;
		private Matrix3X2 _ActiveTransformation;

		private bool _IsBlendOperationInvalid;
		private bool _IsEffectContextInvalid;
		private bool _IsOpacityInvalid;
		private bool _IsTargetEmpty;
		private bool _IsTransformationInvalid;

		private Size _TargetDelta;

		protected Compositor(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			_BoundThread = Thread.CurrentThread;
			_Device2D = device2D;
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

		protected bool IsOpacityInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);
				
				return _IsOpacityInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);
				
				_IsOpacityInvalid = value;
			}
		}

		protected bool IsEffectContextInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);
				
				return _IsEffectContextInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);
				
				_IsEffectContextInvalid = value;
			}
		}

		protected bool IsBlendOperationInvalid
		{
			get
			{
				Contract.Requires(ActiveTarget != null);
				
				return _IsBlendOperationInvalid;
			}
			set
			{
				Contract.Requires(ActiveTarget != null);
				
				_IsBlendOperationInvalid = value;
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

		protected float ActiveOpacity
		{
			get
			{
				Contract.Requires(ActiveTarget != null);
				
				return _ActiveOpacity;
			}
		}

		protected EffectContext ActiveEffectContext
		{
			get
			{
				Contract.Requires(ActiveTarget != null);
				
				return _ActiveEffectContext;
			}
		}

		protected BlendOperation ActiveBlendOperation
		{
			get
			{
				Contract.Requires(ActiveTarget != null);
				
				return _ActiveBlendOperation;
			}
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
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<EffectContext>() == _ActiveEffectContext);

				return _ActiveEffectContext;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

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
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<float>().Equals(_ActiveOpacity));
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));

				return _ActiveOpacity;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);
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
				Contract.Requires(ActiveTarget != null);
				Contract.Ensures(Contract.Result<BlendOperation>() == _ActiveBlendOperation);

				return _ActiveBlendOperation;
			}
			set
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);
				Contract.Requires(ActiveTarget != null);

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

					_IsTransformationInvalid = true;
				}
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

		public void ResetState()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			Opacity = 1.0f;
			Effect = null;
			Blend = BlendOperation.SourceOver;
			Transformation = Matrix3X2.Identity;

			OnResetState();
		}

		public void PushLayer()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnPushLayer();
			}
		}

		public void PushLayer(Retention retentionMode)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnPushLayer(retentionMode);
			}
		}

		public void PopLayer()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnPopLayer();
			}
		}

		public void DiscardLayer()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnDiscardLayer();
			}
		}

		public void Flatten()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnFlatten();
			}
		}

		public void CopyResult(Canvas destination)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(destination != null);

			if(!_IsTargetEmpty)
			{
				Rectangle srcRegion = _ActiveTarget.Region;
				Rectangle dstRegion = destination.Region;

				if(dstRegion.Contains(srcRegion))
				{
					OnCopyResult(_Device2D.Resolve(destination));

					return;
				}

				throw new InvalidOperationException("Destination cannot contain source!");
			}
		}

		public void CopyResult(Rectangle sourceRegion, Canvas destination)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(destination != null);

			if(!_IsTargetEmpty && !sourceRegion.IsEmpty)
			{
				Rectangle srcRegion = _ActiveTarget.Region;
				Rectangle dstRegion = destination.Region;

				if(srcRegion.Contains(sourceRegion))
				{
					srcRegion = new Rectangle(Point.Empty, sourceRegion.Size);

					if(dstRegion.Contains(srcRegion))
					{
						// translate to 2D surface coordinate space
						sourceRegion = sourceRegion.Translate(_TargetDelta);

						OnCopyResult(ref sourceRegion, _Device2D.Resolve(destination));

						return;
					}

					throw new InvalidOperationException("Destination cannot contain source!");
				}

				throw new InvalidOperationException("Source does not contain the source region!");
			}
		}

		public void Flush()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnFlush();
			}
		}

		public void CompositeResult()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				OnCompositeResult();
			}
		}

		public void CompositeResult(Point location)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				location = location.Translate(_TargetDelta);

				OnCompositeResult(ref location);
			}
		}

		public void CompositeResult(Rectangle region)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				OnCompositeResult(ref region);
			}
		}

		public void CompositeResult(Rectangle srcRegion, Point dstLocation)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(_TargetDelta);
				dstLocation = dstLocation.Translate(_TargetDelta);

				OnCompositeResult(ref srcRegion, ref dstLocation);
			}
		}

		public void CompositeResult(Rectangle srcRegion, Rectangle dstRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);

			if(!_IsTargetEmpty)
			{
				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(_TargetDelta);
				dstRegion = dstRegion.Translate(_TargetDelta);

				OnCompositeResult(ref srcRegion, ref dstRegion);
			}
		}

		public void Composite(Canvas source)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				OnComposite(_Device2D.Resolve(source));
			}
		}

		public void Composite(Canvas source, Point location)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				// translate to 2D surface coordinate space
				location = location.Translate(_TargetDelta);

				OnComposite(_Device2D.Resolve(source), ref location);
			}
		}

		public void Composite(Canvas source, Rectangle region)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				OnComposite(_Device2D.Resolve(source), ref region);
			}
		}

		public void Composite(Canvas source, Rectangle srcRegion, Point dstLocation)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);

			if(!_IsTargetEmpty && !source.Region.IsEmpty)
			{
				var sourceContext = _Device2D.Resolve(source);

				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(sourceContext.Region.Location);
				dstLocation = dstLocation.Translate(_TargetDelta);

				OnComposite(sourceContext, ref srcRegion, ref dstLocation);
			}
		}

		public void Composite(Canvas source, Rectangle srcRegion, Rectangle dstRegion)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				var sourceContext = _Device2D.Resolve(source);

				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(sourceContext.Region.Location);
				dstRegion = dstRegion.Translate(_TargetDelta);

				OnComposite(sourceContext, ref srcRegion, ref dstRegion);
			}
		}

		public void Begin(Canvas target, Retention retention = Retention.ClearData)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget == null);
			Contract.Requires(target != null);

			bool isTargetEmpty = target.Region.IsEmpty;

			_IsBlendOperationInvalid = true;
			_IsEffectContextInvalid = true;
			_IsOpacityInvalid = true;
			_IsTransformationInvalid = true;

			_ActiveTarget = target;

			ResetState();

			if(!isTargetEmpty)
			{
				var targetContext = _Device2D.Resolve(target);

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

		public void CopyResult(
			float sourceX, float sourceY, float sourceWidth, float sourceHeight, Canvas destination)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(sourceX));
			Contract.Requires(Check.IsFinite(sourceY));
			Contract.Requires(Check.IsPositive(sourceWidth));
			Contract.Requires(Check.IsPositive(sourceHeight));
			Contract.Requires(destination != null);

			Rectangle sourceRegion = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

			if(!_IsTargetEmpty && !sourceRegion.IsEmpty)
			{
				Rectangle srcRegion = _ActiveTarget.Region;
				Rectangle dstRegion = destination.Region;

				if(srcRegion.Contains(sourceRegion))
				{
					srcRegion = new Rectangle(Point.Empty, sourceRegion.Size);

					if(dstRegion.Contains(srcRegion))
					{
						// translate to 2D surface coordinate space
						sourceRegion = sourceRegion.Translate(_TargetDelta);

						OnCopyResult(ref sourceRegion, _Device2D.Resolve(destination));

						return;
					}

					throw new InvalidOperationException("Destination cannot contain source!");
				}

				throw new InvalidOperationException("Source does not contain the source region!");
			}
		}

		public void CompositeResult(float x, float y)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));

			if(!_IsTargetEmpty)
			{
				Point location = new Point(x, y);

				// translate to 2D surface coordinate space
				location = location.Translate(_TargetDelta);

				OnCompositeResult(ref location);
			}
		}

		public void CompositeResult(float x, float y, float width, float height)
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

				OnCompositeResult(ref region);
			}
		}

		public void CompositeResult(
			float srcX, float srcY, float srcWidth, float srcHeight, float dstX, float dstY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));

			if(!_IsTargetEmpty)
			{
				Rectangle srcRegion = new Rectangle(srcX, srcY, srcWidth, srcHeight);

				Point dstLocation = new Point(dstX, dstY);

				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(_TargetDelta);
				dstLocation = dstLocation.Translate(_TargetDelta);

				OnCompositeResult(ref srcRegion, ref dstLocation);
			}
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
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));
			Contract.Requires(Check.IsPositive(dstWidth));
			Contract.Requires(Check.IsPositive(dstHeight));

			if(!_IsTargetEmpty)
			{
				Rectangle srcRegion = new Rectangle(srcX, srcY, srcWidth, srcHeight);
				Rectangle dstRegion = new Rectangle(dstX, dstY, dstWidth, dstHeight);

				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(_TargetDelta);
				dstRegion = dstRegion.Translate(_TargetDelta);

				OnCompositeResult(ref srcRegion, ref dstRegion);
			}
		}

		public void Composite(Canvas source, float x, float y)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				Point location = new Point(x, y);

				// translate to 2D surface coordinate space
				location = location.Translate(_TargetDelta);

				OnComposite(_Device2D.Resolve(source), ref location);
			}
		}

		public void Composite(Canvas source, float x, float y, float width, float height)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				Rectangle region = new Rectangle(x, y, width, height);

				// translate to 2D surface coordinate space
				region = region.Translate(_TargetDelta);

				OnComposite(_Device2D.Resolve(source), ref region);
			}
		}

		public void Composite(
			Canvas source, float srcX, float srcY, float srcWidth, float srcHeight, float dstX, float dstY)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				var sourceContext = _Device2D.Resolve(source);

				Rectangle srcRegion = new Rectangle(srcX, srcY, srcWidth, srcHeight);

				Point dstLocation = new Point(dstX, dstY);

				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(sourceContext.Region.Location);
				dstLocation = dstLocation.Translate(_TargetDelta);

				OnComposite(sourceContext, ref srcRegion, ref dstLocation);
			}
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
			Contract.Requires(ActiveTarget != null);
			Contract.Requires(source != null);
			Contract.Requires(Check.IsFinite(srcX));
			Contract.Requires(Check.IsFinite(srcY));
			Contract.Requires(Check.IsPositive(srcWidth));
			Contract.Requires(Check.IsPositive(srcHeight));
			Contract.Requires(Check.IsFinite(dstX));
			Contract.Requires(Check.IsFinite(dstY));
			Contract.Requires(Check.IsPositive(dstWidth));
			Contract.Requires(Check.IsPositive(dstHeight));

			if(!_IsTargetEmpty && !source.IsEmpty)
			{
				var sourceContext = _Device2D.Resolve(source);

				Rectangle srcRegion = new Rectangle(srcX, srcY, srcWidth, srcHeight);
				Rectangle dstRegion = new Rectangle(dstX, dstY, dstWidth, dstHeight);

				// translate to 2D surface coordinate space
				srcRegion = srcRegion.Translate(sourceContext.Region.Location);
				dstRegion = dstRegion.Translate(_TargetDelta);

				OnComposite(sourceContext, ref srcRegion, ref dstRegion);
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

		protected abstract void OnBegin(Canvas.ResolvedContext target, Retention retention);
		protected abstract void OnEnd();

		protected abstract void OnCopyResult(Canvas.ResolvedContext destination);

		protected abstract void OnCopyResult(
			ref Rectangle sourceRegion, Canvas.ResolvedContext destination);

		protected abstract void OnComposite(Canvas.ResolvedContext source);

		protected abstract void OnComposite(Canvas.ResolvedContext source, ref Point location);

		protected abstract void OnComposite(Canvas.ResolvedContext source, ref Rectangle region);

		protected abstract void OnComposite(
			Canvas.ResolvedContext source, ref Rectangle srcRegion, ref Point dstLocation);

		protected abstract void OnComposite(
			Canvas.ResolvedContext source, ref Rectangle srcRegion, ref Rectangle dstRegion);
	}
}