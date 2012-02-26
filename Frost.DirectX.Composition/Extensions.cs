// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

namespace Frost.DirectX.Composition
{
	public static class Extensions
	{
		public static void WriteConstants<T>(
			this Cabbage.Compositor compositorSink, ConstantRegister register, T value)
			where T : struct, IConstantBufferData
		{
			Contract.Requires(compositorSink != null);

			IDeferredCompositor deferred = compositorSink.AsDeferred();

			if(deferred != null)
			{
				IDeferredCommands<ICompositorCommand> deferredCommands = deferred;

				WriteConstantsCommand command;

				command.Register = register;
				command.Object = new ObjectWrapper<T>(ref value);

				deferredCommands.Add(ref command);
			}
			else
			{
				Compositor2 dxc = compositorSink.ToImplementation<Compositor2>();

				if(dxc != null)
				{
					dxc.SetConstants(register, ref value);
				}
			}
		}

		public static void SetShader(this Cabbage.Compositor compositorSink, ShaderHandle? shader)
		{
			Contract.Requires(compositorSink != null);

			SetShaderCommand command;

			command.Shader = shader;

			IDeferredCompositor deferred = compositorSink.AsDeferred();

			if(deferred != null)
			{
				IDeferredCommands<ICompositorCommand> deferredCommands = deferred;

				deferredCommands.Add(ref command);
			}
			else
			{
				command.Execute(compositorSink, compositorSink.Device2D);
			}
		}

		private interface IObjectWrapper
		{
			void Execute(ConstantRegister register, ICompositorSink compositorSink, Device2D device2D);
		}

		private sealed class ObjectWrapper<T> : IObjectWrapper
			where T : struct, IConstantBufferData
		{
			private T mValue;

			public ObjectWrapper(ref T value)
			{
				mValue = value;
			}

			public void Execute(ConstantRegister register, ICompositorSink compositorSink, Device2D device2D)
			{
				Compositor2 cmp = (Compositor2)compositorSink;

				cmp.SetConstants(register, ref mValue);
			}
		}

		private struct SetShaderCommand : ICompositorCommand
		{
			public ShaderHandle? Shader;

			public CompositorCommand Command
			{
				get { return CompositorCommand.Custom; }
			}

			public void Execute(Cabbage.Compositor context, Device2D device2D)
			{
				Compositor2 cmp = context.ToImplementation<Compositor2>();

				cmp.Shader = Shader;
			}

			public void ToStream(CommandBuffer buffer)
			{
				object obj = Shader.HasValue ? Shader.Value.Reference : null;

				buffer.Write(ref obj);

				int index = Shader.HasValue ? Shader.Value.Index : 0;

				buffer.Write(ref index);
			}

			public void FromStream(CommandBuffer buffer)
			{
				object obj;

				buffer.Read(out obj);

				int index;

				buffer.Read(out index);

				if(obj != null)
				{
					Shader = new ShaderHandle(index, obj);
				}
				else
				{
					Shader = null;
				}
			}
		}

		private struct WriteConstantsCommand : ICompositorCommand
		{
			public IObjectWrapper Object;
			public ConstantRegister Register;

			public CompositorCommand Command
			{
				get { return CompositorCommand.Custom; }
			}

			public void Execute(Cabbage.Compositor context, Device2D device2D)
			{
				Object.Execute(Register, context, device2D);
			}

			public void ToStream(CommandBuffer buffer)
			{
				buffer.Write(ref Object);

				int register = (int)Register;

				buffer.Write(ref register);
			}

			public void FromStream(CommandBuffer buffer)
			{
				buffer.Read(out Object);

				int register;

				buffer.Read(out register);

				Register = (ConstantRegister)register;
			}
		}
	}
}