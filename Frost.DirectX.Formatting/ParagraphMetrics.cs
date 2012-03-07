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
	/// <summary>
	///   This class provides an implementation of <see cref="ITextMetrics" /> .
	/// </summary>
	internal sealed class ParagraphMetrics : ITextMetrics
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

			_Paragraph = paragraph;

			_Leading = formattedData.Leading;
			_LayoutRegion = formattedData.LayoutRegion;
			_BaselineOffset = formattedData.BaselineOffset;

			_Clusters = formattedData.Clusters.ToArray();

			_ClusterBidiLevels = new byte[formattedData.Clusters.Count];

			_Outlines = CreateOutlineList(formattedData, geometryCache, out _ClusterBidiLevels);

			// create the text index to cluster index transformation table
			_TextToCluster = new int[formattedData.FullText.Length];

			for(int i = 0; i < _Clusters.Length; ++i)
			{
				foreach(int characterIndex in _Clusters[i].Characters)
				{
					_TextToCluster[characterIndex] = i;
				}
			}

			// determine the region that encompasses all formatted clusters
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

			_TextRegion = Rectangle.FromEdges(left, top, right, bottom);

			// determine the ranges of each line
			_LineListBuilder.Clear();

			int lastLine = 0;

			IndexedRange range = IndexedRange.Empty;

			for(int i = 0; i < formattedData.Runs.Count; ++i)
			{
				if(formattedData.Runs[i].LineNumber > lastLine)
				{
					_LineListBuilder.Add(ToTextRange(range));

					range = new IndexedRange(formattedData.Runs[i].Clusters.StartIndex, 0);

					lastLine = formattedData.Runs[i].LineNumber;
				}

				range = range.Extend(formattedData.Runs[i].Clusters.Length);
			}

			if(range.Length > 0)
			{
				_LineListBuilder.Add(ToTextRange(range));
			}

			_Lines = new LineCollection(_LineListBuilder);

			// build a collection of regions mapping to text indexes
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

		public bool GetLineForText(int textIndex, out int lineIndex)
		{
			for(int i = 0; i < Lines.Count; ++i)
			{
				if(Lines[i].Contains(textIndex))
				{
					lineIndex = i;

					return true;
				}
			}

			lineIndex = 0;

			return false;
		}

		public bool FindLineNear(Point position, out int lineIndex)
		{
			int textIndex;

			if(FindTextNear(position, out textIndex))
			{
				return GetLineForText(textIndex, out lineIndex);
			}

			lineIndex = 0;

			return false;
		}

		public bool FindTextNear(Point position, out int textIndex)
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

		public bool IsClusterEnd(int textIndex)
		{
			if(textIndex < _Paragraph.Text.Length - 1)
			{
				if(_TextToCluster[textIndex] == _TextToCluster[textIndex + 1])
				{
					return false;
				}
			}

			return true;
		}

		public bool IsVisible(int textIndex)
		{
			int clusterIndex = _TextToCluster[textIndex];

			if(_Clusters[clusterIndex].Display != DisplayMode.Suppressed)
			{
				if(_Clusters[clusterIndex].Display == DisplayMode.Neutral)
				{
					if(!_Clusters[clusterIndex].Region.IsEmpty)
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

		public bool IsLineStart(int textIndex)
		{
			int lineIndex;

			if(GetLineForText(textIndex, out lineIndex))
			{
				return textIndex == _Lines[lineIndex].StartIndex;
			}

			return false;
		}

		public bool IsLineEnd(int textIndex)
		{
			int lineIndex;

			if (GetLineForText(textIndex, out lineIndex))
			{
				return textIndex == _Lines[lineIndex].LastIndex;
			}

			return false;
		}

		public void ComputeRegion(IndexedRange range, out Rectangle region)
		{
			float left = float.MaxValue;
			float top = float.MaxValue;
			float right = float.MinValue;
			float bottom = float.MinValue;

			foreach(int index in range)
			{
				// exclude invisible clusters from consideration
				if(IsVisible(index))
				{
					// get the cluster index from the text index
					int cluster = _TextToCluster[index];

					left = Math.Min(left, _Clusters[cluster].Region.Left);
					top = Math.Min(top, _Clusters[cluster].Region.Top);
					right = Math.Max(right, _Clusters[cluster].Region.Right);
					bottom = Math.Max(bottom, _Clusters[cluster].Region.Bottom);
				}
			}

			region = Rectangle.FromEdges(left, top, right, bottom);
		}

		/// <summary>
		///   This method creates an outline collection.
		/// </summary>
		/// <param name="formattedData"> This parameter references the formatted input sink. </param>
		/// <param name="geometryCache"> This parameter references the text geometry cache. </param>
		/// <param name="clusterBidiLevels"> This parameter references the cluster bidi levels. </param>
		/// <returns> This method returns a new outline collection containing an outline for each formatted cluster. </returns>
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

				foreach(int clusterIndex in run.Clusters)
				{
					Geometry geometry = geometryCache.Retrieve(
						clusterIndex, run.BidiLevel, run.Font, formattedData);

					_OutlineListBuilder.Add(new Outline(geometry, run.EmSize, baseline));

					clusterBidiLevels[geometryClusterIndex] = run.BidiLevel;

					geometryClusterIndex++;
				}
			}

			return new OutlineCollection(_OutlineListBuilder);
		}

		/// <summary>
		///   This method converts a cluster range to a text range.
		/// </summary>
		/// <param name="clusters"> This parameter contains the cluster range to convert. </param>
		/// <returns> This method returns the converted text range. </returns>
		private IndexedRange ToTextRange(IndexedRange clusters)
		{
			int textLength = 0;

			foreach(int index in clusters)
			{
				textLength += _Clusters[index].Characters.Length;
			}

			int startIndex = _Clusters[clusters.StartIndex].Characters.StartIndex;

			return new IndexedRange(startIndex, textLength);
		}
	}
}