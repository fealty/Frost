// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Frost;
using Frost.Diagnostics;
using Frost.Surfacing;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D10;
using SharpDX.Windows;

using Device1 = SharpDX.Direct3D10.Device1;
using Device2D = Frost.DirectX.Device2D;
using Resource = SharpDX.Direct3D10.Resource;
using Size = Frost.Size;

namespace Demo.Framework
{
	public sealed class DemoApplication : IDisposable
	{
		private readonly IDeviceCounter<int> _CompositionFrameBatchCount;
		private readonly IDeviceCounter<TimeSpan> _CompositionFrameDuration;

		private readonly Device1 _Device;
		private readonly Device2D _Device2D;
		private readonly Factory1 _Factory;
		private readonly RenderForm _Form;
		private readonly IDeviceCounter<TimeSpan> _PaintingFrameDuration;
		private readonly D3D10Renderer _Renderer;
		private readonly SwapChain _SwapChain;
		private readonly Stopwatch _Timer;
		private IDemoContext _Context;
		private bool _IsResetQueued;

		private RenderTargetView _RenderView;
		private Canvas _Target;

		public DemoApplication()
		{
			//SharpDX.Configuration.EnableObjectTracking = true;

			_Form = new RenderForm();
			_Timer = new Stopwatch();

			_Form.Width = 1280;
			_Form.Height = 720;

			_Form.MaximumSize = new System.Drawing.Size(1280, 720);
			_Form.MinimumSize = new System.Drawing.Size(1280, 720);

			_Form.MaximizeBox = false;
			_Form.SizeGripStyle = SizeGripStyle.Hide;

			_Form.KeyPress += HandleKeyPress;
			_Form.ClientSizeChanged += HandleClientSizeChanged;
			_Form.KeyDown += HandleKeyDown;

			ModeDescription displayMode = new ModeDescription
			{
				Width = _Form.ClientSize.Width,
				Height = _Form.ClientSize.Height,
				RefreshRate = new Rational(60, 1),
				Format = Format.R8G8B8A8_UNorm
			};

			SwapChainDescription chainDescription = new SwapChainDescription
			{
				ModeDescription = displayMode,
				SampleDescription = new SampleDescription(1, 0),
				BufferCount = 2,
				IsWindowed = true,
				OutputHandle = _Form.Handle,
				SwapEffect = SwapEffect.Sequential,
				Usage = Usage.BackBuffer | Usage.RenderTargetOutput
			};

			_Factory = new Factory1();

			int availableAdapters = _Factory.GetAdapterCount1();

			if(availableAdapters == 0)
			{
				throw new Exception("no adapters are available");
			}

			for(int i = 0; i < availableAdapters; ++i)
			{
#if DEBUG
				using(Adapter1 dxgiSurfaceAdapter = _Factory.GetAdapter1(i))
				{
					try
					{
					Device1.CreateWithSwapChain(
						dxgiSurfaceAdapter,
						DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
						chainDescription,
						out _Device,
						out _SwapChain);
					}
					catch(SharpDXException)
					{
						continue;
					}

					_Device2D = new Device2D(dxgiSurfaceAdapter);
				}
#else
				using (Adapter1 dxgiSurfaceAdapter = _Factory.GetAdapter1(i))
				{
					try
					{
						Device1.CreateWithSwapChain(
							dxgiSurfaceAdapter,
							DeviceCreationFlags.BgraSupport,
							chainDescription,
							out _Device,
							out _SwapChain);

					}
					catch(SharpDXException)
					{
						continue;
					}

					_Device2D = new Device2D(dxgiSurfaceAdapter);
				}
#endif
			}

			_Device2D.Diagnostics.Query("Composition", "FrameBatchCount", out _CompositionFrameBatchCount);
			_Device2D.Diagnostics.Query("Composition", "FrameDuration", out _CompositionFrameDuration);
			_Device2D.Diagnostics.Query("Painting", "FrameDuration", out _PaintingFrameDuration);

			_Timer.Start();

			_Renderer = new D3D10Renderer(_Device);

			HandleClientSizeChanged(null, null);

			_IsResetQueued = true;
		}

		public Form Form
		{
			get { return _Form; }
		}

		public void Dispose()
		{
			_Form.SafeDispose();

			_Renderer.SafeDispose();
			_Device2D.SafeDispose();

			_RenderView.SafeDispose();
			_SwapChain.SafeDispose();
			_Device.SafeDispose();
			_Factory.SafeDispose();

			_Form.KeyPress -= HandleKeyPress;
			_Form.ClientSizeChanged -= HandleClientSizeChanged;
			_Form.KeyDown -= HandleKeyDown;
		}

		public void Execute(IDemoContext context)
		{
			Contract.Requires(context != null);

			try
			{
				_Form.Text = Application.ProductName;

				_Form.Icon = new Icon(Properties.Resources.frost_icon, 256, 256);

				RenderLoop.Run(_Form, () => Run(context));
			}
#if(!DEBUG)
			catch(Exception e)
#else
			catch(ApplicationException e)
#endif
			{
				var msg = string.Format("{0} \n {1}", e.Message, e.StackTrace);

				MessageBox.Show(msg);

				Debugger.Break();

				throw;
			}
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				_Form.Close();
			}
		}

		private void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar))
			{
				int keyIndex = int.Parse(Char.ToString(e.KeyChar)) - 1;

				if(_Context != null && keyIndex >= 0)
				{
					int count = _Context.Settings.Count();

					if(keyIndex < count)
					{
						var item = _Context.Settings.ElementAt(keyIndex);

						item.Action();

						_IsResetQueued = true;
					}
				}
			}
			else if(e.KeyChar == 'd')
			{
				_Device2D.Resources.DumpToFiles(string.Empty, SurfaceUsage.Normal);
				_Device2D.Resources.DumpToFiles(string.Empty, SurfaceUsage.Dynamic);
				_Device2D.Resources.DumpToFiles(string.Empty, SurfaceUsage.External);
				_Device2D.Resources.DumpToFiles(string.Empty, SurfaceUsage.Private);
			}
			else if(e.KeyChar == 'r')
			{
				_IsResetQueued = true;
			}
		}

		private void HandleClientSizeChanged(object sender, EventArgs e)
		{
			// reconfigure the direct3d rendering surface
			_Device.OutputMerger.SetTargets((RenderTargetView)null);

			if(_RenderView != null)
			{
				_RenderView.Dispose();
			}

			_SwapChain.ResizeBuffers(
				1,
				Math.Max(1, _Form.ClientSize.Width),
				Math.Max(1, _Form.ClientSize.Height),
				Format.R8G8B8A8_UNorm,
				0);

			using(var backBuffer = Resource.FromSwapChain<Texture2D>(_SwapChain, 0))
			{
				_RenderView = new RenderTargetView(_Device, backBuffer);
			}

			_Device.OutputMerger.SetTargets(_RenderView);

			Viewport view = new Viewport
			{
				TopLeftX = 0,
				TopLeftY = 0,
				Width = _Form.ClientSize.Width,
				Height = _Form.ClientSize.Height,
				MinDepth = 0.0f,
				MaxDepth = 1.0f
			};

			_Device.Rasterizer.SetViewports(view);

			// reconfigure the Frost rendering surface
			Size formSize = new Size(_Form.ClientSize.Width, _Form.ClientSize.Height);

			if(_Target != null)
			{
				_Target.Forget();
			}

			_Target = new Canvas(formSize, SurfaceUsage.External);

			formSize = new Size(Math.Max(formSize.Width, 640), Math.Max(formSize.Height, 480));

			_Device2D.Resources.PageSize = new Size(formSize.Width * 2, formSize.Height * 2);

			_IsResetQueued = true;
		}

		private void Run(IDemoContext context)
		{
			Contract.Requires(context != null);

			_Context = context;

			_Device2D.ProcessTick();

			if(_IsResetQueued)
			{
				if (!_Target.IsEmpty)
				{
					DemoInterface.ResetDemo(context, _Target, _Device2D);
				}

				_IsResetQueued = false;
			}

			_Device.OutputMerger.SetTargets(_RenderView);

			Color4 background = new Color4(0.75f, 0.75f, 0.75f, 1.0f);

			_Device.ClearRenderTargetView(_RenderView, background);

			_Renderer.BeginRendering();

			if(_Target != null)
			{
				_Renderer.Render(_Target, _Device2D);
			}

			_Renderer.EndRendering();

			_SwapChain.Present(1, PresentFlags.None);
		}
	}
}