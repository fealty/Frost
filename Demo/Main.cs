// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Windows.Forms;

using Frost;
using Frost.Atlasing;
using Frost.Diagnostics;
using Frost.Formatting;
using Frost.Surfacing;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D10;
using SharpDX.Windows;

using Debug = System.Diagnostics.Debug;
using Device1 = SharpDX.Direct3D10.Device1;
using Device2D = Frost.DirectX.Device2D;
using Point = System.Drawing.Point;
using Rectangle = Frost.Rectangle;
using RectangleF = System.Drawing.RectangleF;
using Resource = SharpDX.Direct3D10.Resource;

namespace Demo
{
	internal class DemoMain : IDisposable
	{
		private readonly DistanceField mDistance;
		private readonly DistanceFieldEffect mDistanceEffect;
		private readonly RenderForm mForm;
		private readonly Stopwatch mTimer;
		private IDeviceCounter<int> mCompositionFrameBatchCount;
		private IDeviceCounter<TimeSpan> mCompositionFrameDuration;
		private Device1 mDevice;
		private Device2D mDevice2D;
		private Factory1 mFactory;
		private Dx101Renderer mGuiRenderer;
		//private Yuki.Xenos.RootPanel mGuiRoot;
		private TimeSpan mLastFrameTime;
		private TimeSpan mLastUpdate;
		private IDeviceCounter<TimeSpan> mPaintingFrameDuration;
		private Point mPoint;
		private RenderTargetView mRenderView;
		private SwapChain mSwapChain;
		private Canvas mViewer;

		public DemoMain(RenderForm form)
		{
			mDistance = new DistanceField();
			mDistanceEffect = new DistanceFieldEffect();

			mTimer = new Stopwatch();

			mForm = form;

			mForm.ClientSizeChanged += HandleFormResize;
			mForm.MouseDown += HandleMouseDown;
			mForm.MouseUp += HandleMouseUp;
			mForm.MouseWheel += HandleMouseWheel;
			mForm.KeyDown += HandleKeyDown;
			mForm.KeyUp += HandleKeyUp;
			mForm.KeyPress += HandleKeyPress;
			mForm.MouseMove += HandleMouseMove;

			InitializeDirect3D();

			mTimer.Start();

			InitializeYuki();
		}

		public void Dispose()
		{
			mGuiRenderer.SafeDispose();
			mDevice2D.SafeDispose();

			mRenderView.SafeDispose();
			mSwapChain.SafeDispose();
			mDevice.SafeDispose();
			mFactory.SafeDispose();

			mForm.KeyUp -= HandleKeyUp;
			mForm.KeyDown -= HandleKeyDown;
			mForm.MouseWheel -= HandleMouseWheel;
			mForm.MouseUp -= HandleMouseUp;
			mForm.MouseDown -= HandleMouseDown;
			mForm.ClientSizeChanged -= HandleFormResize;
		}

		public void Run()
		{
			TimeSpan frameStartTime = mTimer.Elapsed;

			TimeSpan uiEndTime = mTimer.Elapsed;

			if(uiEndTime - mLastUpdate > TimeSpan.FromMilliseconds(250))
			{
				double drawTime = mPaintingFrameDuration.Value.Ticks / (double)TimeSpan.TicksPerMillisecond;

				double renderTime = mCompositionFrameDuration.Value.Ticks / (double)TimeSpan.TicksPerMillisecond;

				int batchCount = mCompositionFrameBatchCount.Value;

				double frameTimeMs = ((frameStartTime - mLastFrameTime).Ticks /
				                      (double)TimeSpan.TicksPerMillisecond);

				double uiTimeMs = ((uiEndTime - frameStartTime).Ticks / (double)TimeSpan.TicksPerMillisecond);

				/*mForm.Text =
					string.Format(
						"Frame Time: {0:N2} UI: {1:N2} (U: {2:00}% D: {3:00}% C: {4:00}%) GC: {5}, {6}, {7} Batch: {8} Mem: {9:N0}",
						frameTimeMs,
						uiTimeMs,
						Math.Min(
							100.0,
							(Math.Max(0.0, (uiTimeMs - (drawTime + renderTime)) / uiTimeMs)) * 100.0),
						Math.Min(
							100.0,
							(drawTime / uiTimeMs) * 100.0),
						Math.Min(
							100.0,
							(renderTime / uiTimeMs) * 100.0),
						GC.CollectionCount(0),
						GC.CollectionCount(1),
						GC.CollectionCount(2),
						batchCount,
						GC.GetTotalMemory(false));*/

				mLastUpdate = uiEndTime;
			}

			mDevice.OutputMerger.SetTargets(mRenderView);

			mDevice.ClearRenderTargetView(mRenderView, new Color4(0.75f, 0.75f, 0.75f, 1.0f));
			//mDevice.ClearRenderTargetView(mRenderView, new Color4(1.0f, 1.0f, 1.0f));
			//mDevice.ClearRenderTargetView(mRenderView, new Color4(0.0f, 0.0f, 0.0f));

			mGuiRenderer.BeginRendering();
			mGuiRenderer.Render(mViewer, RectangleF.Empty);
			mGuiRenderer.EndRendering();

			mSwapChain.Present(1, PresentFlags.None);

			mDevice2D.SignalUpdate();

			mLastFrameTime = frameStartTime;

			if(mTimer.ElapsedMilliseconds >= 30000)
			{
				//mForm.Close();
			}
		}

		private void InitializeDirect3D()
		{
			ModeDescription md = new ModeDescription
			{
				Width = mForm.ClientSize.Width,
				Height = mForm.ClientSize.Height,
				RefreshRate = new Rational(60, 1),
				Format = Format.R8G8B8A8_UNorm
			};

			SampleDescription sd = new SampleDescription {Count = 1, Quality = 0};

			SwapChainDescription scd = new SwapChainDescription
			{
				ModeDescription = md,
				SampleDescription = sd,
				BufferCount = 3,
				IsWindowed = true,
				OutputHandle = mForm.Handle,
				SwapEffect = SwapEffect.Sequential,
				Usage = Usage.BackBuffer | Usage.RenderTargetOutput
			};

			mFactory = new Factory1();
#if DEBUG
			Device1.CreateWithSwapChain(
				mFactory.GetAdapter1(0),
				DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
				scd,
				out mDevice,
				out mSwapChain);
#else
			Device1.CreateWithSwapChain(
				mFactory.GetAdapter1(0),
				DeviceCreationFlags.BgraSupport,
				scd,
				out mDevice,
				out mSwapChain);
#endif
			CreateBuffers();
		}

		private void InitializeYuki()
		{
			mGuiRenderer = new Dx101Renderer(mDevice);

			mDevice2D = new Device2D();

			mDevice2D.Diagnostics.Query("Composition", "FrameBatchCount", out mCompositionFrameBatchCount);
			mDevice2D.Diagnostics.Query("Composition", "FrameDuration", out mCompositionFrameDuration);
			mDevice2D.Diagnostics.Query("Painting", "FrameDuration", out mPaintingFrameDuration);

			HandleFormResize(null, null);

			mDevice2D.Effects.Register<DistanceFieldEffect>();

			Paragraph p = Paragraph.Create().WithPointSize(12)
				//.ChangeFontStyle(FontStyle.Italic)
				.WithFeatures(new FontFeatureCollection(new[] {new FontFeature("swsh", 1)})).WithFamily(
					"Brioso Pro").WithAdditionalText("R").Build();

			ITextMetrics metrics = mDevice2D.Measure(
				p, new Rectangle(Frost.Point.Empty, Size.MaxValue), null);

			Outline outline = metrics.Outlines[0];

			GC.Collect(4);

			Canvas test2 = mDevice2D.CreateCanvas(new Size(128, 128));

			Stopwatch watch = new Stopwatch();
			watch.Start();
			Canvas test = mDistance.CreateField(
				outline.NormalizedOutline, outline.NormalizedBaseline, mDevice2D);
			watch.Stop();

			mForm.Text = string.Format("Time: {0}", watch.ElapsedMilliseconds);

			Debug.WriteLine("Time: {0}", watch.ElapsedMilliseconds);

			Rectangle reg = mDevice2D.ComputeRegion(outline.NormalizedOutline);

			mDevice2D.Painter.Begin(mViewer);
			mDevice2D.Painter.Translate(test.Region.X, test.Region.Y);

			mDevice2D.Painter.Translate(
				(0.5f - (reg.Width / 2.0f)) * 400, (0.5f - (reg.Height / 2.0f)) * 400);

			// translate the glyph to the left corner of the EM square
			mDevice2D.Painter.Translate(-reg.X * 400, -reg.Y * 400);
			mDevice2D.Painter.SetBrush(Color.Black);
			mDevice2D.Painter.Scale(400, 400);
			mDevice2D.Painter.Fill(outline.NormalizedOutline);
			mDevice2D.Painter.End(); //*/

			mDevice2D.Compositor.Begin(mViewer, Retention.RetainData);

			DistanceEffectSettings settings;
			mDevice2D.Compositor.Translate(400, 0);
			mDevice2D.Compositor.Scale(3.125f, 3.125f);
			mDevice2D.Compositor.ApplyEffect(settings);
			mDevice2D.Compositor.Composite(test);
			mDevice2D.Compositor.End();

			mDevice2D.Painter.Begin(mViewer, Retention.RetainData);
			mDevice2D.Painter.SetBrush(Color.IndianRed);
			mDevice2D.Painter.IsAntialiased = Antialiasing.Aliased;
			mDevice2D.Painter.StrokeWidth = DistanceField.EmLength / 400;
			mDevice2D.Painter.Scale(1.0f / DistanceField.EmLength, 1.0f / DistanceField.EmLength);
			mDevice2D.Painter.Scale(400, 400);
			TestTest(mDistance.Sample);
			mDevice2D.Painter.End();

			mDevice2D.DumpSurfaces(null, SurfaceUsage.Normal); //*/

			
		}

		private void TestTest(Sample s)
		{
			Rectangle region = s.Region;

			mDevice2D.Painter.Stroke(region);

			/*float mag = 50;

			Frost.Point tl =
				new Frost.Point(
					region.Left + s.TopLeft.Intersection.X * mag,
					region.Top + s.TopLeft.Intersection.Y * mag);
			Frost.Point tr =
				new Frost.Point(
					region.Right + s.TopRight.Intersection.X * mag,
					region.Top + s.TopRight.Intersection.Y * mag);
			Frost.Point bl =
				new Frost.Point(
					region.Left + s.BottomLeft.Intersection.X * mag,
					region.Bottom + s.BottomLeft.Intersection.Y * mag);
			Frost.Point br =
				new Frost.Point(
					region.Right + s.BottomRight.Intersection.X * mag,
					region.Bottom + s.BottomRight.Intersection.Y * mag);

			tl = s.TopLeft.Intersection;
			tr = s.TopRight.Intersection;
			bl = s.BottomLeft.Intersection;
			br = s.BottomRight.Intersection;

			mDevice2D.Painter.SetBrush(Color.Green);
			if(s.TopLeft.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Left, region.Top, tl.X, tl.Y);
			if(s.TopRight.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Right, region.Top, tr.X, tr.Y);
			if(s.BottomLeft.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Left, region.Bottom, bl.X, bl.Y);
			if(s.BottomRight.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Right, region.Bottom, br.X, br.Y);//*/
			mDevice2D.Painter.SetBrush(Color.Red);

			foreach(Sample ss in s.Children)
			{
				TestTest(ss);
			}
		}

		private void CreateBuffers()
		{
			// Create the render target view.
			using(Texture2D backBuffer = Resource.FromSwapChain<Texture2D>(mSwapChain, 0))
			{
				mRenderView = new RenderTargetView(mDevice, backBuffer);
			}

			// Set the render target.
			mDevice.OutputMerger.SetTargets(mRenderView);

			// Create the viewport.
			Viewport view = new Viewport
			{
				TopLeftX = 0,
				TopLeftY = 0,
				Width = mForm.ClientSize.Width,
				Height = mForm.ClientSize.Height,
				MinDepth = 0.0f,
				MaxDepth = 1.0f
			};

			mDevice.Rasterizer.SetViewports(view);
		}

		private void HandleFormResize(object sender, EventArgs e)
		{
			mDevice.OutputMerger.SetTargets((RenderTargetView)null);

			// Release the render target view.
			mRenderView.Dispose();

			// Resize the swap chain buffers.
			mSwapChain.ResizeBuffers(
				1,
				Math.Max(1, mForm.ClientSize.Width),
				Math.Max(1, mForm.ClientSize.Height),
				Format.R8G8B8A8_UNorm,
				0);

			// Recreate the buffers and view port.
			CreateBuffers();

			// Set the new Yuki screen resolution.
			mGuiRenderer.TargetResolution = mForm.ClientSize;

			mViewer = mDevice2D.CreateCanvas(
				new Size(mForm.ClientSize.Width, mForm.ClientSize.Height), SurfaceUsage.External);

			mDevice2D.ResizeSurfaces(
				new Size(mForm.ClientSize.Width, mForm.ClientSize.Height), SurfaceUsage.External);
			mDevice2D.ResizeSurfaces(
				new Size(mForm.ClientSize.Width + 2, mForm.ClientSize.Height + 2), SurfaceUsage.Dynamic);
			mDevice2D.ResizeSurfaces(
				new Size(mForm.ClientSize.Width * 2, mForm.ClientSize.Height * 2), SurfaceUsage.Normal);
		}

		private void HandleMouseDown(object sender, MouseEventArgs e)
		{
		}

		private void HandleMouseUp(object sender, MouseEventArgs e)
		{
		}

		private void HandleMouseWheel(object sender, MouseEventArgs e)
		{
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				mForm.Close();
			}
		}

		private void HandleKeyUp(object sender, KeyEventArgs e)
		{
		}

		private void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == '6')
			{
				mDevice2D.DumpSurfaces(string.Empty, SurfaceUsage.Normal);
				mDevice2D.DumpSurfaces(string.Empty, SurfaceUsage.Dynamic);
				mDevice2D.DumpSurfaces(string.Empty, SurfaceUsage.External);
			}

			if(e.KeyChar == '4')
			{
				HandleFormResize(null, null);
			}
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			mPoint = e.Location;
		}
	}

	internal static class Application
	{
		//[STAThread]
		private static void Main()
		{
			try
			{
				using(RenderForm form = new RenderForm("YukiGUI Demo"))
				{
					using(DemoMain demo = new DemoMain(form))
					{
						RenderLoop.Run(form, demo.Run);
					}
				}
			}
			catch(ApplicationException e)
			{
				MessageBox.Show(string.Format("{0} \n {1}", e.Message, e.StackTrace));

				Debugger.Break();
			}
		}
	}
}