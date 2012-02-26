using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost.Shaping;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	internal sealed class TextGeometryCache : IDisposable
	{
		private readonly Dictionary<FontHandle, Dictionary<TextGeometryKey, Geometry>>
			mCache;

		private readonly TextGeometrySink mSink;

		private float[] mGlyphAdvances;
		private short[] mGlyphIndices;

		private GlyphOffset[] mGlyphOffsets;

		public TextGeometryCache()
		{
			mGlyphIndices = new short[0];
			mGlyphAdvances = new float[0];

			mGlyphOffsets = new GlyphOffset[0];

			mCache = new Dictionary<FontHandle, Dictionary<TextGeometryKey, Geometry>>();

			mSink = new TextGeometrySink();
		}

		public void Dispose()
		{
			mSink.Dispose();
		}

		public Geometry Retrieve(
			int clusterIndex,
			byte bidiLevel,
			FontHandle font,
			FormatterSink input)
		{
			Contract.Requires(clusterIndex >= 0);
			Contract.Requires(font != null);
			Contract.Requires(input != null);

			FormattedCluster cluster = input.Clusters[clusterIndex];

			_ResizeInternalBuffers(cluster.Glyphs.Length);

			int index = 0;

			for(int i = cluster.Glyphs.Start; i <= cluster.Glyphs.End; ++i)
			{
				mGlyphAdvances[index] = Convert.ToSingle(cluster.Region.Width);

				mGlyphIndices[index] = input.Glyphs[i].Index;
				mGlyphOffsets[index] = input.Glyphs[i].Offset;

				++index;
			}

			TextGeometryKey key;

			key.Advances = mGlyphAdvances;
			key.Indices = mGlyphIndices;
			key.Offsets = mGlyphOffsets;

			Dictionary<TextGeometryKey, Geometry> cacheForFont;

			if(mCache.TryGetValue(font, out cacheForFont))
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

				mCache.Add(font, cacheForFont);
			}

			Geometry geometry = mSink.CreateGeometry(key, bidiLevel, font);

			TextGeometryKey newKey;

			newKey.Advances = (float[])key.Advances.Clone();
			newKey.Indices = (short[])key.Indices.Clone();
			newKey.Offsets = (GlyphOffset[])key.Offsets.Clone();

			cacheForFont.Add(newKey, geometry);

			return geometry;
		}

		private void _ResizeInternalBuffers(int count)
		{
			Contract.Requires(count >= 0);

			if(count != mGlyphIndices.Length)
			{
				mGlyphIndices = new short[count];
				mGlyphAdvances = new float[count];

				mGlyphOffsets = new GlyphOffset[count];
			}
		}
	}
}