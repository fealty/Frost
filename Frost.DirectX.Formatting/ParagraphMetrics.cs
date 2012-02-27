// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Formatting;
using Frost.Shaping;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	public sealed class ParagraphMetrics : ITextMetrics
	{
		private static readonly List<Outline> _OutlineListBuilder;
		private static readonly List<Rectangle> _RegionListBuilder;
		private static readonly List<IndexedRange> _LineListBuilder;

		private readonly float _BaselineOffset;
		private readonly byte[] _ClusterBidiLevels;

		private readonly FormattedCluster[] _Clusters;
		private readonly Rectangle _LayoutRegion;
		private readonly float _Leading;
		private readonly LineCollection _Lines;
		private readonly OutlineCollection _Outlines;
		private readonly Paragraph _Paragraph;
		private readonly RegionCollection _Regions;

		private readonly Rectangle _TextRegion;
		private readonly int[] _TextToCluster;

		static ParagraphMetrics()
		{
			_LineListBuilder = new List<IndexedRange>();
			_RegionListBuilder = new List<Rectangle>();
			_OutlineListBuilder = new List<Outline>();
		}

		internal ParagraphMetrics(
			Paragraph paragraph, FormatterSink formattedData, TextGeometryCache geometryCache)
		{
			Contract.Requires(paragraph != null);
			Contract.Requires(formattedData != null);
			Contract.Requires(geometryCache != null);

			_LineListBuilder.Clear();

			_Paragraph = paragraph;

			_Leading = formattedData.Leading;
			_LayoutRegion = formattedData.LayoutRegion;
			_BaselineOffset = formattedData.BaselineOffset;

			_Clusters = formattedData.Clusters.ToArray();

			_ClusterBidiLevels = new byte[formattedData.Clusters.Count];

			_Outlines = CreateOutlineList(formattedData, geometryCache, out _ClusterBidiLevels);

			_TextToCluster = new int[formattedData.FullText.Length];

			for(int i = 0; i < _Clusters.Length; ++i)
			{
				IndexedRange characters = _Clusters[i].Characters;

				for(int j = characters.StartIndex; j <= characters.LastIndex; ++j)
				{
					_TextToCluster[j] = i;
				}
			}

			float left = float.MaxValue;
			float top = float.MaxValue;
			float right = float.MinValue;
			float bottom = float.MinValue;

			for(int i = 0; i < _Clusters.Length; ++i)
			{
				left = Math.Min(left, _Clusters[i].Region.Left);
				top = Math.Min(top, _Clusters[i].Region.Top);
				right = Math.Max(right, _Clusters[i].Region.Right);
				bottom = Math.Max(bottom, _Clusters[i].Region.Bottom);
			}

			_TextRegion = new Rectangle(left, top, right, bottom);

			int lastLine = 0;

			ClusterRange range = ClusterRange.Empty;

			for(int i = 0; i < formattedData.Runs.Count; ++i)
			{
				if(formattedData.Runs[i].LineNumber > lastLine)
				{
					_LineListBuilder.Add(ToTextRange(range));

					range = new ClusterRange(formattedData.Runs[i].Clusters.Start, 0);

					lastLine = formattedData.Runs[i].LineNumber;
				}

				range = new ClusterRange(range.Start, range.Length + formattedData.Runs[i].Clusters.Length);
			}

			if(range.Length > 0)
			{
				_LineListBuilder.Add(ToTextRange(range));
			}

			_Lines = new LineCollection(_LineListBuilder);

			_RegionListBuilder.Clear();

			for(int i = 0; i < paragraph.Text.Length; ++i)
			{
				_RegionListBuilder.Add(_Clusters[_TextToCluster[i]].Region);
			}

			_Regions = new RegionCollection(_RegionListBuilder);
		}

		public float Leading
		{
			get { return _Leading; }
		}

		public Paragraph Paragraph
		{
			get { return _Paragraph; }
		}

		public Rectangle TextRegion
		{
			get { return _TextRegion; }
		}

		public Rectangle LayoutRegion
		{
			get { return _LayoutRegion; }
		}

		public Size BaselineOffset
		{
			get { return new Size(0.0f, _BaselineOffset); }
		}

		public LineCollection Lines
		{
			get { return _Lines; }
		}

		public RegionCollection Regions
		{
			get { return _Regions; }
		}

		public OutlineCollection Outlines
		{
			get { return _Outlines; }
		}

		public bool FindIndexNear(Point position, out int textIndex)
		{
			int nearestCluster = 0;

			for(int i = 0; i < _Clusters.Length; ++i)
			{
				if(_Clusters[i].Region.Contains(position))
				{
					textIndex = _Clusters[i].Characters.StartIndex;

					return true;
				}

				Point reference = _Clusters[i].Region.Center;

				double referenceX = position.X - reference.X;
				double referenceY = position.Y - reference.Y;

				referenceX *= referenceX;
				referenceY *= referenceY;

				Point nearest = _Clusters[nearestCluster].Region.Center;

				double nearestX = position.X - nearest.X;
				double nearestY = position.Y - nearest.Y;

				nearestX *= nearestX;
				nearestY *= nearestY;

				double referenceTotal = referenceX + referenceY;
				double nearestTotal = nearestX + nearestY;

				nearestCluster = referenceTotal > nearestTotal ? i : nearestCluster;
			}

			textIndex = _Clusters[nearestCluster].Characters.StartIndex;

			return false;
		}

		public bool IsRightToLeft(int textIndex)
		{
			int clusterIndex = _TextToCluster[textIndex];

			return Convert.ToBoolean(_ClusterBidiLevels[clusterIndex] & 1);
		}

		public bool IsClusterStart(int textIndex)
		{
			if(textIndex > 0)
			{
				if(_TextToCluster[textIndex] == _TextToCluster[textIndex - 1])
				{
					return false;
				}
			}

			return true;
		}

		public bool IsClusterVisible(int textIndex)
		{
			int clusterIndex = _TextToCluster[textIndex];

			if(_Clusters[clusterIndex].Display != DisplayMode.Suppressed)
			{
				if(_Clusters[clusterIndex].Display == DisplayMode.Neutral)
				{
					if(_Clusters[clusterIndex].Region.Area > 0)
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		public void ComputeRegion(IndexedRange range, out Rectangle region)
		{
			float left = float.MaxValue;
			float top = float.MaxValue;
			float right = float.MinValue;
			float bottom = float.MinValue;

			for(int i = range.StartIndex; i <= range.LastIndex; ++i)
			{
				// exclude invisible clusters from consideration
				if(IsClusterVisible(i))
				{
					int cluster = _TextToCluster[i];

					left = Math.Min(left, _Clusters[cluster].Region.Left);
					top = Math.Min(top, _Clusters[cluster].Region.Top);
					right = Math.Max(right, _Clusters[cluster].Region.Right);
					bottom = Math.Max(bottom, _Clusters[cluster].Region.Bottom);
				}
			}

			region = new Rectangle(left, top, right, bottom);
		}

		private static OutlineCollection CreateOutlineList(
			FormatterSink formattedData, TextGeometryCache geometryCache, out byte[] clusterBidiLevels)
		{
			_OutlineListBuilder.Clear();

			clusterBidiLevels = new byte[formattedData.Clusters.Count];

			int geometryClusterIndex = 0;

			for(int i = 0; i < formattedData.Runs.Count; ++i)
			{
				FormattedRun run = formattedData.Runs[i];

				FontFace face = run.Font.ResolveFace();

				float emSize = face.Metrics.Ascent + face.Metrics.Descent;

				float baseline = face.Metrics.Ascent / emSize;

				for(int j = run.Clusters.Start; j <= run.Clusters.End; ++j)
				{
					Geometry geometry = geometryCache.Retrieve(j, run.BidiLevel, run.Font, formattedData);

					_OutlineListBuilder.Add(new Outline(geometry, run.EmSize, baseline));

					clusterBidiLevels[geometryClusterIndex] = run.BidiLevel;

					geometryClusterIndex++;
				}
			}

			return new OutlineCollection(_OutlineListBuilder);
		}

		private IndexedRange ToTextRange(ClusterRange clusters)
		{
			int textLength = 0;

			for(int i = clusters.Start; i <= clusters.End; ++i)
			{
				textLength += _Clusters[i].Characters.Length;
			}

			return new IndexedRange(_Clusters[clusters.Start].Characters.StartIndex, textLength);
		}
	}
}