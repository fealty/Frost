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
	/// <summary>
	///   This class provides management of text geometry resources.
	/// </summary>
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

		/// <summary>
		///   This method retrieves the geometry for a formatted cluster.
		/// </summary>
		/// <param name="clusterIndex"> This parameter indicates the formatted cluster index. </param>
		/// <param name="bidiLevel"> This parameter indicates the bidi level of the cluster. </param>
		/// <param name="font"> This parameter references the font for the cluster. </param>
		/// <param name="input"> This parameter references the formatted text output. </param>
		/// <returns> This method returns the geometry for the formatted cluster if available; otherwise, this method returns <c>null</c>. </returns>
		public Geometry Retrieve(int clusterIndex, byte bidiLevel, FontHandle font, FormatterSink input)
		{
			Contract.Requires(clusterIndex >= 0);
			Contract.Requires(font != null);
			Contract.Requires(input != null);

			FormattedCluster cluster = input.Clusters[clusterIndex];

			ResizeInternalBuffers(cluster.Glyphs.Length);

			int index = 0;

			foreach(int itemIndex in cluster.Glyphs)
			{
				_GlyphAdvances[index] = cluster.Region.Width;

				_GlyphIndices[index] = input.Glyphs[itemIndex].Index;
				_GlyphOffsets[index] = input.Glyphs[itemIndex].Offset;

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

		/// <summary>
		///   This method resizes the internal buffers.
		/// </summary>
		/// <param name="count"> This parameter indicates the count of glyphs the internal buffers can hold. </param>
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