// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;

using Frost.DirectX.Common;
using Frost.DirectX.Common.Diagnostics;
using Frost.DirectX.Composition.Properties;
using Frost.Effects;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;

using BlendOperation = Frost.Composition.BlendOperation;

namespace Frost.DirectX.Composition
{
	internal sealed class Compositor : Frost.Composition.Compositor, IDisposable, IShaderCompiler
	{
		private readonly IntegerCounter _BatchItemCount = new IntegerCounter(
			"Composition", "BatchItemCount");

		private readonly Stack<BatchedItemQueue> _BatchedItemLists;
		private readonly PixelShader _DefaultPixelShader;

		private readonly Device _Device3D;

		private readonly IntegerCounter _FrameBatchCount = new IntegerCounter(
			"Composition", "FrameBatchCount");

		private readonly TimeSpanCounter _FrameDuration = new TimeSpanCounter(
			"Composition", "FrameDuration");

		private readonly float[] _ItemOpacities;
		private readonly RenderableItemQueue _RenderableItems;
		private readonly Renderer _Renderer;

		private readonly ShaderCache _Shaders;
		private readonly Stack<State> _States;
		private readonly Stopwatch _Watch;

		private PixelShader _ActiveShader;
		private Surface2D _ActiveSurface;

		private BlendOperation _BatchedBlendOperation;
		private EffectContext _BatchedEffect;
		private BatchedItemQueue _BatchedItems;
		private PixelShader _BatchedShader;
		private Surface2D _BatchedSurface;

		private Canvas.ResolvedContext _Target;

		public Compositor(Device device3D, Device2D device2D) : base(device2D)
		{
			Contract.Requires(device3D != null);
			Contract.Requires(device2D != null);

			device2D.Diagnostics.Register(_FrameDuration);
			device2D.Diagnostics.Register(_BatchItemCount);
			device2D.Diagnostics.Register(_FrameBatchCount);

			_Device3D = device3D;

			_States = new Stack<State>();
			_BatchedItemLists = new Stack<BatchedItemQueue>();

			_Watch = new Stopwatch();
			_RenderableItems = new RenderableItemQueue();

			_Shaders = new ShaderCache(device3D, device2D);
			_Renderer = new Renderer(device3D, device2D);

			_ItemOpacities = new float[Renderer.BatchItemCount];

			CompileDefaultShader(out _DefaultPixelShader);
		}

		internal IntegerCounter FrameBatchCount
		{
			get
			{
				Contract.Ensures(Contract.Result<IntegerCounter>() != null);
				Contract.Ensures(Contract.Result<IntegerCounter>().Equals(_FrameBatchCount));

				return _FrameBatchCount;
			}
		}

		internal IntegerCounter BatchItemCount
		{
			get
			{
				Contract.Ensures(Contract.Result<IntegerCounter>() != null);
				Contract.Ensures(Contract.Result<IntegerCounter>().Equals(_BatchItemCount));

				return _BatchItemCount;
			}
		}

		internal TimeSpanCounter FrameDuration
		{
			get
			{
				Contract.Ensures(Contract.Result<TimeSpanCounter>() != null);
				Contract.Ensures(Contract.Result<TimeSpanCounter>().Equals(_FrameDuration));

				return _FrameDuration;
			}
		}

		internal ShaderHandle? Shader
		{
			set
			{
				Contract.Assert(_Target != null);
				
				_ActiveShader = value.HasValue ? _Shaders.Resolve(value.Value) : null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void IShaderCompiler.Compile(string text, string function, ref ShaderHandle result)
		{
			IShaderCompiler compiler = _Shaders;

			compiler.Compile(text, function, ref result);
		}

		protected override void OnSaveState()
		{
			Contract.Assert(_Target != null);

			_States.Push(
				new State
				{
					Blend = ActiveBlendOperation,
					Effect = ActiveEffectContext,
					Opacity = ActiveOpacity,
					Shader = _ActiveShader,
					Surface = _ActiveSurface,
					Transformation = ActiveTransformation
				});
		}

		protected override void OnRestoreState()
		{
			Contract.Assert(_Target != null);

			State newState = _States.Pop();

			Blend = newState.Blend;
			_ActiveShader = newState.Shader;
			Effect = newState.Effect;
			_ActiveSurface = newState.Surface;
			Opacity = newState.Opacity;
			Transformation = newState.Transformation;
		}

		protected override void OnResetState()
		{
			_ActiveShader = null;
			_ActiveSurface = null;
		}

		protected override void OnPushLayer()
		{
			Contract.Assert(_Target != null);

			PushLayer(Retention.ClearData);
		}

		protected override void OnPushLayer(Retention retentionMode)
		{
			Contract.Assert(_Target != null);

			Flush();

			SaveState();

			_Renderer.PushLayer(retentionMode, _Target.Region.Size);
		}

		protected override void OnPopLayer()
		{
			Contract.Assert(_Target != null);

			Flush();

			Canvas.ResolvedContext previousLayer;

			_Renderer.PopLayer(out previousLayer);

			try
			{
				Rectangle region = new Rectangle(Point.Empty, previousLayer.Region.Size);

				RestoreState();

				OnComposite(previousLayer, ref region, ref region);

				Flush();
			}
			finally
			{
				_Renderer.FreeLayer(previousLayer);
			}
		}

		protected override void OnDiscardLayer()
		{
			Contract.Assert(_Target != null);

			Flush();

			_Renderer.PopLayer();
		}

		protected override void OnFlatten()
		{
			Contract.Assert(_Target != null);

			Flush();

			while(_Renderer.HasExtraLayers)
			{
				PopLayer();
			}
		}

		protected override void OnFlush()
		{
			Contract.Assert(_Target != null);

			if(_BatchedItems != null && _BatchedItems.Count > 0)
			{
				BatchedItemQueue items = _BatchedItems;

				_BatchedItems = null;

				SaveState();
				ResetState();

				try
				{
					Effect = null;

					_BatchedEffect.EffectBase.Apply(items, _BatchedEffect, this);
				}
				finally
				{
					RestoreState();
				}

				items.Clear();

				_BatchedItemLists.Push(items);
			}

			RenderBatchedQueue();
		}

		protected override void OnCompositeResult()
		{
			Contract.Assert(_Target != null);

			Point location = _Renderer.ActiveLayer.Region.Location;

			OnCompositeResult(ref location);
		}

		protected override void OnCompositeResult(ref Point location)
		{
			Contract.Assert(_Target != null);

			Rectangle region = _Renderer.ActiveLayer.Region;

			OnCompositeResult(ref region, ref location);
		}

		protected override void OnCompositeResult(ref Rectangle region)
		{
			Contract.Assert(_Target != null);

			Rectangle activeRegion = _Renderer.ActiveLayer.Region;

			OnCompositeResult(ref activeRegion, ref region);
		}

		protected override void OnCompositeResult(ref Rectangle srcRegion, ref Point dstLocation)
		{
			Contract.Assert(_Target != null);

			Size activeSize = _Renderer.ActiveLayer.Region.Size;

			Rectangle dstRegion = new Rectangle(dstLocation, activeSize);

			OnCompositeResult(ref srcRegion, ref dstRegion);
		}

		protected override void OnCompositeResult(ref Rectangle srcRegion, ref Rectangle dstRegion)
		{
			Contract.Assert(_Target != null);

			Flush();

			_Renderer.PushLayer(Retention.RetainData, srcRegion.Size);

			Canvas.ResolvedContext previousLayer;

			_Renderer.PopLayer(out previousLayer);

			try
			{
				OnComposite(previousLayer, ref srcRegion, ref dstRegion);

				Flush();
			}
			finally
			{
				_Renderer.FreeLayer(previousLayer);
			}
		}

		protected override void OnBegin(Canvas.ResolvedContext target, Retention retention)
		{
			Contract.Assert(_Target == null);

			_Watch.Reset();
			_Watch.Start();

			_Target = target;

			_Renderer.PushLayer(Retention.ClearData, target.Region.Size);

			if(retention == Retention.RetainData)
			{
				_Target.Surface2D.CopyTo(
					_Target.Region, _Renderer.ActiveLayer.Surface2D, _Renderer.ActiveLayer.Region.Location);
			}
		}

		protected override void OnEnd()
		{
			Contract.Assert(_Target != null);

			try
			{
				Flatten();

				_Renderer.ActiveLayer.Surface2D.CopyTo(
					_Renderer.ActiveLayer.Region, _Target.Surface2D, _Target.Region.Location);
				_Renderer.PopLayer();
			}
			finally
			{
				_Target = null;

				_Watch.Stop();

				_FrameDuration.Value += _Watch.Elapsed;
			}
		}

		protected override void OnCopyResult(Canvas.ResolvedContext destination)
		{
			Contract.Assert(_Target != null);

			Flush();

			_Renderer.ActiveLayer.Surface2D.CopyTo(
				_Renderer.ActiveLayer.Region, destination.Surface2D, destination.Region.Location);
		}

		protected override void OnCopyResult(
			ref Rectangle sourceRegion, Canvas.ResolvedContext destination)
		{
			Contract.Assert(_Target != null);

			Flush();

			_Renderer.ActiveLayer.Surface2D.CopyTo(
				sourceRegion, destination.Surface2D, destination.Region.Location);
		}

		protected override void OnComposite(Canvas.ResolvedContext source)
		{
			Contract.Assert(_Target != null);

			Rectangle region = new Rectangle(Point.Empty, source.Region.Size);

			OnComposite(source, ref region);
		}

		protected override void OnComposite(Canvas.ResolvedContext source, ref Point location)
		{
			Contract.Assert(_Target != null);

			Rectangle region = new Rectangle(location, source.Region.Size);

			OnComposite(source, ref region);
		}

		protected override void OnComposite(Canvas.ResolvedContext source, ref Rectangle region)
		{
			Contract.Assert(_Target != null);

			Rectangle srcRegion = new Rectangle(Point.Empty, source.Region.Size);

			OnComposite(source, ref srcRegion, ref region);
		}

		protected override void OnComposite(
			Canvas.ResolvedContext source, ref Rectangle srcRegion, ref Point dstLocation)
		{
			Contract.Assert(_Target != null);

			Rectangle dstRegion = new Rectangle(dstLocation, srcRegion.Size);

			OnComposite(source, ref srcRegion, ref dstRegion);
		}

		protected override void OnComposite(
			Canvas.ResolvedContext source, ref Rectangle srcRegion, ref Rectangle dstRegion)
		{
			Contract.Assert(_Target != null);

			EffectContext effect = ActiveEffectContext;

			if(effect != null)
			{
				IShaderEffect shader = effect.EffectBase as IShaderEffect;

				if(shader != null)
				{
					shader.Compile(this);
				}
			}

			if(!Equals(effect, _BatchedEffect))
			{
				Flush();

				_BatchedEffect = effect;
			}

			if(effect != null)
			{
				if(_BatchedItems == null)
				{
					if(_BatchedItemLists.Count == 0)
					{
						_BatchedItemLists.Push(new BatchedItemQueue());
					}

					_BatchedItems = _BatchedItemLists.Pop();
				}

				Matrix3X2 transformation = ActiveTransformation;

				_BatchedItems.Add(
					new BatchedItem(source.Target, srcRegion, dstRegion, ActiveBlendOperation, ref transformation));
			}
			else
			{
				CompositeToQueue(source, ref srcRegion, ref dstRegion);
			}
		}

		internal void SetConstants<T>(ConstantRegister register, ref T value)
			where T : struct, IConstantBufferData
		{
			_Renderer.SetConstants(register, ref value);
		}

		private void CompositeToQueue(
			Canvas.ResolvedContext source, ref Rectangle sourceRegion, ref Rectangle destinationRegion)
		{
			Contract.Requires(source != null);

			Matrix3X2 transformation = ActiveTransformation;

			FlushOnStateChange(source);

			float xPixel = 1.0f / source.Surface2D.Region.Width;
			float yPixel = 1.0f / source.Surface2D.Region.Height;

			Rectangle tex = new Rectangle(
				(source.Region.X + sourceRegion.X) * xPixel,
				(source.Region.Y + sourceRegion.Y) * yPixel,
				(sourceRegion.Width * xPixel),
				(sourceRegion.Height * yPixel));

			RenderConstants item;

			item.TextureRegion.X = tex.Left;
			item.TextureRegion.Y = tex.Top;
			item.TextureRegion.Z = tex.Right;
			item.TextureRegion.W = tex.Bottom;

			item.DrawnRegion.X = destinationRegion.Left;
			item.DrawnRegion.Y = destinationRegion.Top;
			item.DrawnRegion.Z = destinationRegion.Right;
			item.DrawnRegion.W = destinationRegion.Bottom;

			item.Transform = Matrix.Identity;

			item.Transform.M11 = transformation.M11;
			item.Transform.M12 = transformation.M12;
			item.Transform.M21 = transformation.M21;
			item.Transform.M22 = transformation.M22;
			item.Transform.M41 = transformation.M31;
			item.Transform.M42 = transformation.M32;

			_ItemOpacities[_RenderableItems.Count] = Opacity;

			_RenderableItems.Add(item);
		}

		private void FlushOnStateChange(Canvas.ResolvedContext source)
		{
			Contract.Requires(source != null);

			if(!ReferenceEquals(_ActiveShader, _BatchedShader))
			{
				RenderBatchedQueue();

				_Renderer.ActiveShader = _ActiveShader;

				_BatchedShader = _ActiveShader;
			}

			if(!ReferenceEquals(source.Surface2D, _BatchedSurface))
			{
				RenderBatchedQueue();

				_BatchedSurface = (Surface2D)source.Surface2D;
			}

			if(_RenderableItems.Count >= Renderer.BatchItemCount)
			{
				RenderBatchedQueue();
			}

			if(ActiveBlendOperation != _BatchedBlendOperation)
			{
				RenderBatchedQueue();

				_BatchedBlendOperation = ActiveBlendOperation;
			}
		}

		private void CompileDefaultShader(out PixelShader shader)
		{
			Contract.Ensures(Contract.ValueAtReturn(out shader) != null);

			using(ShaderBytecode code = ShaderBytecode.Compile(Resources.PixelShader, "Main", "ps_4_0"))
			{
				shader = new PixelShader(_Device3D, code);
			}
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_Renderer.Dispose();
				_DefaultPixelShader.Dispose();
				_Shaders.Dispose();
			}
		}

		private void RenderBatchedQueue()
		{
			if(_RenderableItems.Count > 0)
			{
				if(_BatchedShader == null)
				{
					_Renderer.ActiveShader = _DefaultPixelShader;

					DefaultConstants constants;

					constants.Opacity = _ItemOpacities;
					constants.Count = _RenderableItems.Count;

					_Renderer.SetConstants(ConstantRegister.Reserved, ref constants);
				}

				_Renderer.Render(_RenderableItems, _BatchedSurface, _BatchedBlendOperation);

				_BatchItemCount.Value = _RenderableItems.Count;

				++_FrameBatchCount.Value;

				_RenderableItems.Clear();
			}
		}

		private sealed class BatchedItemQueue : List<BatchedItem>
		{
		}

		private struct DefaultConstants : IConstantBufferData
		{
			public int Count;
			public float[] Opacity;

			public int ByteSize
			{
				get { return Renderer.BatchItemCount * 16; }
			}

			public void Serialize(DataStream stream)
			{
				for(int i = 0; i < Count; ++i)
				{
					stream.Write(Opacity[i]);

					stream.Seek(12, SeekOrigin.Current);
				}
			}
		}

		private sealed class RenderableItemQueue : List<RenderConstants>
		{
		}

		private struct State
		{
			public BlendOperation Blend;
			public EffectContext Effect;
			public float Opacity;
			public PixelShader Shader;
			public Surface2D Surface;
			public Matrix3X2 Transformation;
		}
	}
}