// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Shaping;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	internal sealed class TextGeometryCache : IDisposable
	{
		private readonly Dictionary<FontHandle, Dictionary<TextGeometryKey, Geometry>> _Cache;

		private readonly TextGeometrySink _Sink;

		private float[] _GlyphAdvances;
		private short[] _GlyphIndices;

		private GlyphOffset[] _GlyphOffsets;

		public TextGeometryCache()
		{
			_GlyphIndices = new short[0];
			_GlyphAdvances = new float[0];

			_GlyphOffsets = new GlyphOffset[0];

			_Cache = new Dictionary<FontHandle, Dictionary<TextGeometryKey, Geometry>>();

			_Sink = new TextGeometrySink();
		}

		public void Dispose()
		{
			_Sink.Dispose();
		}

		public Geometry Retrieve(int clusterIndex, byte bidiLevel, FontHandle font, FormatterSink input)
		{
			Contract.Requires(clusterIndex >= 0);
			Contract.Requires(font != null);
			Contract.Requires(input != null);

			FormattedCluster cluster = input.Clusters[clusterIndex];

			ResizeInternalBuffers(cluster.Glyphs.Length);

			int index = 0;

			for(int i = cluster.Glyphs.Start; i <= cluster.Glyphs.End; ++i)
			{
				_GlyphAdvances[index] = Convert.ToSingle(cluster.Region.Width);

				_GlyphIndices[index] = input.Glyphs[i].Index;
				_GlyphOffsets[index] = input.Glyphs[i].Offset;

				++index;
			}

			TextGeometryKey key;

			key.Advances = _GlyphAdvances;
			key.Indices = _GlyphIndices;
			key.Offsets = _GlyphOffsets;

			Dictionary<TextGeometryKey, Geometry> cacheForFont;

			if(_Cache.TryGetValue(font, out cacheForFont))
			{
				Geometry result;

				if(cacheForFont.TryGetValue(key, out result))
				{
					return result;
				}
			}
			else
			{
				cacheForFont = new Dictionary<TextGeometryKey, Geometry>();

				_Cache.Add(font, cacheForFont);
			}

			Geometry geometry = _Sink.CreateGeometry(key, bidiLevel, font);

			TextGeometryKey newKey;

			newKey.Advances = (float[])key.Advances.Clone();
			newKey.Indices = (short[])key.Indices.Clone();
			newKey.Offsets = (GlyphOffset[])key.Offsets.Clone();

			cacheForFont.Add(newKey, geometry);

			return geometry;
		}

		private void ResizeInternalBuffers(int count)
		{
			Contract.Requires(count >= 0);

			if(count != _GlyphIndices.Length)
			{
				_GlyphIndices = new short[count];
				_GlyphAdvances = new float[count];

				_GlyphOffsets = new GlyphOffset[count];
			}
		}
	}
}