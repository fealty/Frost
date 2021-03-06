﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.IO;

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
		public const int CacheLimit = 5;

		private readonly Device2D _Device2D;
		private readonly Device _Device3D;
		private readonly Guid _Id;

		private readonly CacheDictionary<Gradient, LinearGradientBrush> _LinearGradientBrushes;

		private readonly CacheDictionary<Gradient, RadialGradientBrush> _RadialGradientBrushes;

		private readonly Rectangle _Region;
		private readonly ShaderResourceView _ShaderView;
		private readonly Surface _Surface;
		private readonly RenderTarget _Target2D;
		private readonly RenderTargetView _TargetView;
		private readonly Texture2D _Texture;
		private readonly SurfaceUsage _Usage;

		private Bitmap _Bitmap;
		private BitmapBrush _BitmapBrush;
		private SolidColorBrush _SolidColorBrush;

		protected Surface2D(ref Description surfaceDescription)
			: this(ref surfaceDescription, Descriptions.PrivateTexture)
		{
			Contract.Requires(surfaceDescription.Device2D != null);
			Contract.Requires(surfaceDescription.Device3D != null);
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Width));
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Height));
		}

		protected Surface2D(ref Description surfaceDescription, Texture2DDescription textureDescription)
		{
			Contract.Requires(surfaceDescription.Device2D != null);
			Contract.Requires(surfaceDescription.Device3D != null);
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Width));
			Contract.Requires(Check.IsPositive(surfaceDescription.Size.Height));

			_LinearGradientBrushes = new CacheDictionary<Gradient, LinearGradientBrush>(CacheLimit);
			_RadialGradientBrushes = new CacheDictionary<Gradient, RadialGradientBrush>(CacheLimit);

			_Id = Guid.NewGuid();

			textureDescription.Width = Convert.ToInt32(surfaceDescription.Size.Width);
			textureDescription.Height = Convert.ToInt32(surfaceDescription.Size.Height);

			textureDescription.Format = Format.R8G8B8A8_UNorm;

			Device device3D = surfaceDescription.Device3D;

			_Texture = new Texture2D(device3D, textureDescription);

			_Surface = _Texture.AsSurface();

			_ShaderView = new ShaderResourceView(device3D, _Texture);

			_Usage = surfaceDescription.Usage;
			_Region = new Rectangle(Point.Empty, surfaceDescription.Size);
			_Device2D = surfaceDescription.Device2D;
			_Device3D = surfaceDescription.Device3D;

			if(surfaceDescription.Factory2D != null)
			{
				_Target2D = new RenderTarget(surfaceDescription.Factory2D, _Surface, Descriptions.RenderTarget);
			}

			_TargetView = new RenderTargetView(device3D, _Texture);
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
				Contract.Ensures(Contract.Result<ShaderResourceView>().Equals(_ShaderView));

				return _ShaderView;
			}
		}

		public RenderTarget Target2D
		{
			get
			{
				Contract.Ensures(Contract.Result<RenderTarget>() != null);
				Contract.Ensures(Contract.Result<RenderTarget>().Equals(_Target2D));

				return _Target2D;
			}
		}

		public RenderTargetView TargetView
		{
			get
			{
				Contract.Ensures(Contract.Result<RenderTargetView>() != null);
				Contract.Ensures(Contract.Result<RenderTargetView>().Equals(_TargetView));

				return _TargetView;
			}
		}

		public Texture2D Texture2D
		{
			get
			{
				Contract.Ensures(Contract.Result<Texture2D>() != null);
				Contract.Ensures(Contract.Result<Texture2D>().Equals(_Texture));

				return _Texture;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public bool Equals(Surface2D other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			return ReferenceEquals(this, other) || other._Id.Equals(_Id);
		}

		public void CopyTo(Rectangle srcRegion, ISurface2D destination, Point dstLocation)
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

				_Device3D.CopySubresourceRegion(
					Texture2D, 0, sourceRegion, dstSurface.Texture2D, 0, offsetX, offsetY, 0);
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

		public Guid Id
		{
			get { return _Id; }
		}

		public Device2D Device2D
		{
			get { return _Device2D; }
		}

		public SurfaceUsage Usage
		{
			get { return _Usage; }
		}

		public Rectangle Region
		{
			get { return _Region; }
		}

		public void DumpToPNG(Stream stream)
		{
			try
			{
				AcquireLock();

				Resource.ToStream(_Texture, ImageFileFormat.Png, stream);
			}
			finally
			{
				ReleaseLock();
			}
		}

		public void Clear()
		{
			_Device3D.ClearRenderTargetView(_TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
		}

		public Brush GetRadialGradientBrush(Point center, Point offset, Size radius, Gradient gradient)
		{
			Contract.Requires(Check.IsPositive(radius.Width));
			Contract.Requires(Check.IsPositive(radius.Height));
			Contract.Requires(gradient != null);
			Contract.Ensures(Contract.Result<Brush>() != null);

			RadialGradientBrush brush;

			if(_RadialGradientBrushes.TryGetValue(gradient, out brush))
			{
				brush.Center = center.ToPointF();
				brush.GradientOriginOffset = offset.ToPointF();
				brush.RadiusX = radius.Width;
				brush.RadiusY = radius.Height;

				return brush;
			}

			using(var stopCollection = CreateStopCollection(gradient))
			{
				brush = new RadialGradientBrush(_Target2D, Descriptions.RadialGradient, stopCollection)
				{
					Center = center.ToPointF(),
					GradientOriginOffset = offset.ToPointF(),
					RadiusX = radius.Width,
					RadiusY = radius.Height
				};
			}

			try
			{
				_RadialGradientBrushes.Add(gradient, brush);
			}
			catch
			{
				brush.Dispose();

				throw;
			}

			return brush;
		}

		public Brush GetLinearGradientBrush(Point startPoint, Point endPoint, Gradient gradient)
		{
			Contract.Requires(gradient != null);
			Contract.Ensures(Contract.Result<Brush>() != null);

			LinearGradientBrush brush;

			if(_LinearGradientBrushes.TryGetValue(gradient, out brush))
			{
				brush.StartPoint = startPoint.ToPointF();
				brush.EndPoint = endPoint.ToPointF();

				return brush;
			}

			using(var stopCollection = CreateStopCollection(gradient))
			{
				brush = new LinearGradientBrush(_Target2D, Descriptions.LinearGradient, stopCollection)
				{StartPoint = startPoint.ToPointF(), EndPoint = endPoint.ToPointF()};
			}

			try
			{
				_LinearGradientBrushes.Add(gradient, brush);
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

			if(_SolidColorBrush == null)
			{
				_SolidColorBrush = new SolidColorBrush(_Target2D, newColor);
			}

			_SolidColorBrush.Color = newColor;

			return _SolidColorBrush;
		}

		public Brush GetPatternBrush(Surface2D surface, Repetition extension)
		{
			Contract.Requires(surface != null);
			Contract.Ensures(Contract.Result<Brush>() != null);

			_Bitmap.SafeDispose();
			_BitmapBrush.SafeDispose();

			_Bitmap = new Bitmap(_Target2D, surface, Descriptions.BitmapProperties);

			_BitmapBrush = new BitmapBrush(_Target2D, _Bitmap, Descriptions.BitmapBrush);

			switch(extension)
			{
				case Repetition.Repeat:
					_BitmapBrush.ExtendModeY = ExtendMode.Wrap;
					_BitmapBrush.ExtendModeX = ExtendMode.Wrap;
					break;
				case Repetition.Horizontal:
					_BitmapBrush.ExtendModeY = ExtendMode.Clamp;
					_BitmapBrush.ExtendModeX = ExtendMode.Wrap;
					break;
				case Repetition.Vertical:
					_BitmapBrush.ExtendModeY = ExtendMode.Wrap;
					_BitmapBrush.ExtendModeX = ExtendMode.Clamp;
					break;
				case Repetition.Clamp:
					_BitmapBrush.ExtendModeX = ExtendMode.Clamp;
					_BitmapBrush.ExtendModeY = ExtendMode.Clamp;
					break;
			}

			return _BitmapBrush;
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

			return obj.GetType() == typeof(Surface2D) && Equals((Surface2D)obj);
		}

		public override int GetHashCode()
		{
			return _Id.GetHashCode();
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				_LinearGradientBrushes.Dispose();
				_RadialGradientBrushes.Dispose();

				_Bitmap.SafeDispose();
				_BitmapBrush.SafeDispose();
				_SolidColorBrush.SafeDispose();

				_ShaderView.Dispose();
				_Surface.Dispose();
				_Target2D.SafeDispose();
				_TargetView.Dispose();
				_Texture.Dispose();
			}
		}

		private GradientStopCollection CreateStopCollection(Gradient gradient)
		{
			Contract.Requires(gradient != null);
			Contract.Ensures(Contract.Result<GradientStopCollection>() != null);

			DxStop[] dxStops = new DxStop[gradient.Stops.Length];

			for(int i = 0; i < gradient.Stops.Length; ++i)
			{
				dxStops[i].Position = gradient.Stops[i].Position;
				dxStops[i].Color = gradient.Stops[i].Color.ToColor4();
			}

			return new GradientStopCollection(_Target2D, dxStops, Gamma.Linear, ExtendMode.Clamp);
		}

		public struct Description
		{
			public Device2D Device2D;
			public Device Device3D;
			public Factory Factory2D;
			public Size Size;
			public SurfaceUsage Usage;
		}

		public class ExternalSurface2D : Surface2D
		{
			private readonly IntPtr _DeviceHandle;
			private readonly KeyedMutex _DeviceMutex;

			public ExternalSurface2D(ref Description surfaceDescription)
				: base(ref surfaceDescription, Descriptions.SharedTexture)
			{
				Contract.Requires(surfaceDescription.Device2D != null);
				Contract.Requires(surfaceDescription.Device3D != null);
				Contract.Requires(Check.IsPositive(surfaceDescription.Size.Width));
				Contract.Requires(Check.IsPositive(surfaceDescription.Size.Height));

				_DeviceMutex = _Texture.QueryInterface<KeyedMutex>();

				using(var resource = _Texture.QueryInterface<DXGIResource>())
				{
					_DeviceHandle = resource.SharedHandle;
				}
			}

			public override IntPtr DeviceHandle
			{
				get { return _DeviceHandle; }
			}

			public override void AcquireLock()
			{
				_DeviceMutex.Acquire(0, -1);
			}

			public override void ReleaseLock()
			{
				_DeviceMutex.Release(0);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if(disposing)
				{
					_DeviceMutex.Dispose();
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