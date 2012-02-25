// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Painting;
using Frost.Surfacing;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Direct3D10;

using DXGIResource = SharpDX.DXGI.Resource;
using Device = SharpDX.Direct3D10.Device;
using DxStop = SharpDX.Direct2D1.GradientStop;
using Factory = SharpDX.Direct2D1.Factory;
using Resource = SharpDX.Direct3D10.Resource;

namespace Frost.DirectX.Common
{
	public class Surface2D : ISurface2D, IEquatable<Surface2D>, IDisposable
	{
		//TODO: add configuration option for surface 2d cache limit?
		public const int CacheLimit = 5;

		private static long _AvailableUniqueId;

		private readonly Device2D _Device2D;
		private readonly Device _Device3D;

		private readonly CacheDictionary<Gradient, LinearGradientBrush>
			_LinearGradientBrushes;

		private readonly CacheDictionary<Gradient, RadialGradientBrush>
			_RadialGradientBrushes;

		private readonly Rectangle _Region;
		private readonly ShaderResourceView _ShaderView;
		private readonly Surface _Surface;
		private readonly RenderTarget _Target2D;
		private readonly RenderTargetView _TargetView;
		private readonly Texture2D _Texture;
		private readonly long _UniqueId;
		private readonly SurfaceUsage _Usage;

		private Bitmap _Bitmap;
		private BitmapBrush _BitmapBrush;
		private SolidColorBrush _SolidColorBrush;

		private Surface2D(ref Description surfaceDescription)
			: this(ref surfaceDescription, Descriptions.PrivateTexture)
		{
			Contract.Requires(surfaceDescription.Device2D != null);
			Contract.Requires(surfaceDescription.Device3D != null);
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Width));
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Height));
		}

		protected Surface2D(
			ref Description surfaceDescription,
			Texture2DDescription textureDescription)
		{
			Contract.Requires(surfaceDescription.Device2D != null);
			Contract.Requires(surfaceDescription.Device3D != null);
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Width));
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Height));

			this._LinearGradientBrushes =
				new CacheDictionary<Gradient, LinearGradientBrush>(CacheLimit);
			this._RadialGradientBrushes =
				new CacheDictionary<Gradient, RadialGradientBrush>(CacheLimit);

			this._UniqueId = Interlocked.Increment(ref _AvailableUniqueId) - 1;

			textureDescription.Width =
				Convert.ToInt32(surfaceDescription.Size.Width);
			textureDescription.Height =
				Convert.ToInt32(surfaceDescription.Size.Height);

			textureDescription.Format = Format.R8G8B8A8_UNorm;

			Device device3D = surfaceDescription.Device3D;

			this._Texture = new Texture2D(device3D, textureDescription);

			this._Surface = this._Texture.AsSurface();

			this._ShaderView = new ShaderResourceView(device3D, this._Texture);

			this._Usage = surfaceDescription.Usage;
			this._Region = new Rectangle(Point.Empty, surfaceDescription.Size);
			this._Device2D = surfaceDescription.Device2D;
			this._Device3D = surfaceDescription.Device3D;

			if(surfaceDescription.Factory2D != null)
			{
				this._Target2D = new RenderTarget(
					surfaceDescription.Factory2D,
					this._Surface,
					Descriptions.RenderTarget);
			}

			this._TargetView = new RenderTargetView(device3D, this._Texture);
		}

		public virtual IntPtr DeviceHandle
		{
			get { return IntPtr.Zero; }
		}

		public ShaderResourceView ShaderView
		{
			get
			{
				Contract.Ensures(Contract.Result<ShaderResourceView>() != null);
				Contract.Ensures(
					Contract.Result<ShaderResourceView>().Equals(this._ShaderView));

				return this._ShaderView;
			}
		}

		public RenderTarget Target2D
		{
			get
			{
				Contract.Ensures(Contract.Result<RenderTarget>() != null);
				Contract.Ensures(
					Contract.Result<RenderTarget>().Equals(this._Target2D));

				return this._Target2D;
			}
		}

		public RenderTargetView TargetView
		{
			get
			{
				Contract.Ensures(Contract.Result<RenderTargetView>() != null);
				Contract.Ensures(
					Contract.Result<RenderTargetView>().Equals(this._TargetView));

				return this._TargetView;
			}
		}

		public Texture2D Texture2D
		{
			get
			{
				Contract.Ensures(Contract.Result<Texture2D>() != null);
				Contract.Ensures(
					Contract.Result<Texture2D>().Equals(this._Texture));

				return this._Texture;
			}
		}

		public bool Equals(Surface2D other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return other._UniqueId == this._UniqueId;
		}

		public void CopyTo(
			Rectangle srcRegion, ISurface2D destination, Point dstLocation)
		{
			Surface2D dstSurface = (Surface2D)destination;

			int offsetX = Convert.ToInt32(dstLocation.X);
			int offsetY = Convert.ToInt32(dstLocation.Y);

			ResourceRegion sourceRegion = new ResourceRegion
			{
				Front = 0,
				Left = Convert.ToInt32(srcRegion.Left),
				Top = Convert.ToInt32(srcRegion.Top),
				Right = Convert.ToInt32(srcRegion.Right),
				Bottom = Convert.ToInt32(srcRegion.Bottom),
				Back = 1
			};

			try
			{
				AcquireLock();
				destination.AcquireLock();

				this._Device3D.CopySubresourceRegion(
					Texture2D,
					0,
					sourceRegion,
					dstSurface.Texture2D,
					0,
					offsetX,
					offsetY,
					0);
			}
			finally
			{
				ReleaseLock();
				destination.ReleaseLock();
			}
		}

		public virtual void AcquireLock()
		{
		}

		public virtual void ReleaseLock()
		{
		}

		public Device2D Device2D
		{
			get { return this._Device2D; }
		}

		public SurfaceUsage Usage
		{
			get { return this._Usage; }
		}

		public Rectangle Region
		{
			get { return this._Region; }
		}

		public void DumpToFile(string file)
		{
			try
			{
				AcquireLock();

				Resource.ToFile(this._Texture, ImageFileFormat.Png, file + ".png");
			}
			finally
			{
				ReleaseLock();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Clear()
		{
			this._Device3D.ClearRenderTargetView(
				this._TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
		}

		public Brush GetRadialGradientBrush(
			Point center, Point offset, Size radius, Gradient gradient)
		{
			Contract.Requires(Check.IsPositive(radius.Width));
			Contract.Requires(Check.IsPositive(radius.Height));
			Contract.Requires(gradient != null);
			Contract.Ensures(Contract.Result<Brush>() != null);

			RadialGradientBrush brush;

			if(this._RadialGradientBrushes.TryGetValue(gradient, out brush))
			{
				brush.Center = center.ToPointF();
				brush.GradientOriginOffset = offset.ToPointF();
				brush.RadiusX = Convert.ToSingle(radius.Width);
				brush.RadiusY = Convert.ToSingle(radius.Height);

				return brush;
			}

			using(var stopCollection = this.CreateStopCollection(gradient))
			{
				brush = new RadialGradientBrush(
					this._Target2D, Descriptions.RadialGradient, stopCollection)
				{
					Center = center.ToPointF(),
					GradientOriginOffset = offset.ToPointF(),
					RadiusX = Convert.ToSingle(radius.Width),
					RadiusY = Convert.ToSingle(radius.Height)
				};
			}

			try
			{
				this._RadialGradientBrushes.Add(gradient, brush);
			}
			catch
			{
				brush.Dispose();

				throw;
			}

			return brush;
		}

		public Brush GetLinearGradientBrush(
			Point startPoint, Point endPoint, Gradient gradient)
		{
			Contract.Requires(gradient != null);
			Contract.Ensures(Contract.Result<Brush>() != null);

			LinearGradientBrush brush;

			if(this._LinearGradientBrushes.TryGetValue(gradient, out brush))
			{
				brush.StartPoint = startPoint.ToPointF();
				brush.EndPoint = endPoint.ToPointF();

				return brush;
			}

			using(var stopCollection = this.CreateStopCollection(gradient))
			{
				brush = new LinearGradientBrush(
					this._Target2D, Descriptions.LinearGradient, stopCollection)
				{
					StartPoint = startPoint.ToPointF(),
					EndPoint = endPoint.ToPointF()
				};
			}

			try
			{
				this._LinearGradientBrushes.Add(gradient, brush);
			}
			catch
			{
				brush.Dispose();

				throw;
			}

			return brush;
		}

		public Brush GetSolidColorBrush(Color color)
		{
			Contract.Ensures(Contract.Result<Brush>() != null);

			Color4 newColor = color.ToColor4();

			if(this._SolidColorBrush == null)
			{
				this._SolidColorBrush = new SolidColorBrush(
					this._Target2D, newColor);
			}

			this._SolidColorBrush.Color = newColor;

			return this._SolidColorBrush;
		}

		public Brush GetPatternBrush(
			Surface2D surface, Repetition extension)
		{
			Contract.Requires(surface != null);
			Contract.Ensures(Contract.Result<Brush>() != null);

			this._Bitmap.SafeDispose();
			this._BitmapBrush.SafeDispose();

			this._Bitmap = new Bitmap(
				this._Target2D, surface, Descriptions.BitmapProperties);

			this._BitmapBrush = new BitmapBrush(
				this._Target2D, this._Bitmap, Descriptions.BitmapBrush);

			switch(extension)
			{
				case Repetition.Repeat:
					this._BitmapBrush.ExtendModeY = ExtendMode.Wrap;
					this._BitmapBrush.ExtendModeX = ExtendMode.Wrap;
					break;
				case Repetition.Horizontal:
					this._BitmapBrush.ExtendModeY = ExtendMode.Clamp;
					this._BitmapBrush.ExtendModeX = ExtendMode.Wrap;
					break;
				case Repetition.Vertical:
					this._BitmapBrush.ExtendModeY = ExtendMode.Wrap;
					this._BitmapBrush.ExtendModeX = ExtendMode.Clamp;
					break;
				case Repetition.Clamp:
					this._BitmapBrush.ExtendModeX = ExtendMode.Clamp;
					this._BitmapBrush.ExtendModeY = ExtendMode.Clamp;
					break;
			}

			return this._BitmapBrush;
		}

		public static Surface2D FromDescription(ref Description description)
		{
			Contract.Requires(description.Device2D != null);
			Contract.Requires(description.Device3D != null);
			Contract.Requires(Check.IsPositive(description.Size.Width));
			Contract.Requires(Check.IsPositive(description.Size.Height));
			Contract.Ensures(Contract.Result<Surface2D>() != null);

			if(description.Usage == SurfaceUsage.External)
			{
				return new ExternalSurface2D(ref description);
			}

			return new Surface2D(ref description);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			if(ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is Surface2D && Equals((Surface2D)obj);
		}

		public override int GetHashCode()
		{
			return this._UniqueId.GetHashCode();
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				this._LinearGradientBrushes.Dispose();
				this._RadialGradientBrushes.Dispose();

				this._Bitmap.SafeDispose();
				this._BitmapBrush.SafeDispose();
				this._SolidColorBrush.SafeDispose();

				this._ShaderView.Dispose();
				this._Surface.Dispose();
				this._Target2D.SafeDispose();
				this._TargetView.Dispose();
				this._Texture.Dispose();
			}
		}

		private GradientStopCollection CreateStopCollection(
			Gradient gradient)
		{
			Contract.Requires(gradient != null);
			Contract.Ensures(Contract.Result<GradientStopCollection>() != null);

			DxStop[] dxStops = new DxStop[gradient.Stops.Length];

			for(int i = 0; i < gradient.Stops.Length; ++i)
			{
				dxStops[i].Position = gradient.Stops[i].Position;
				dxStops[i].Color = gradient.Stops[i].Color.ToColor4();
			}

			return new GradientStopCollection(
				this._Target2D, dxStops, Gamma.Linear, ExtendMode.Clamp);
		}

		public struct Description
		{
			public Device2D Device2D;
			public Device Device3D;
			public Factory Factory2D;
			public Size Size;
			public SurfaceUsage Usage;
		}

		private class ExternalSurface2D : Surface2D
		{
			private readonly IntPtr _DeviceHandle;
			private readonly KeyedMutex _DeviceMutex;

			public ExternalSurface2D(ref Description surfaceDescription)
				: base(ref surfaceDescription, Descriptions.SharedTexture)
			{
				Contract.Requires(surfaceDescription.Device2D != null);
				Contract.Requires(surfaceDescription.Device3D != null);
				Contract.Requires(Check.IsPositive(surfaceDescription.Size.Width));
				Contract.Requires(
					Check.IsPositive(surfaceDescription.Size.Height));

				this._DeviceMutex = this._Texture.QueryInterface<KeyedMutex>();

				using(var resource = this._Texture.QueryInterface<DXGIResource>())
				{
					this._DeviceHandle = resource.SharedHandle;
				}
			}

			public override IntPtr DeviceHandle
			{
				get { return this._DeviceHandle; }
			}

			public override void AcquireLock()
			{
				this._DeviceMutex.Acquire(0, -1);
			}

			public override void ReleaseLock()
			{
				this._DeviceMutex.Release(0);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if(disposing)
				{
					this._DeviceMutex.Dispose();
				}
			}
		}

		public static implicit operator Surface(Surface2D surface)
		{
			return surface != null ? surface._Surface : null;
		}

		public static bool operator ==(Surface2D left, Surface2D right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Surface2D left, Surface2D right)
		{
			return !Equals(left, right);
		}
	}
}