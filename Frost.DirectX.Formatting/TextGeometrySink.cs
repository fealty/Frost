// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

using Geometry = Frost.Shaping.Geometry;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class a cluster key to Frost geometry from the DirectWrite glyph outlines.
	/// </summary>
	internal sealed class TextGeometrySink : CallbackBase, SimplifiedGeometrySink
	{
		private Geometry.Builder _GeometryBuilder;

		void SimplifiedGeometrySink.SetFillMode(FillMode fillMode)
		{
		}

		void SimplifiedGeometrySink.SetSegmentFlags(PathSegment vertexFlags)
		{
		}

		void SimplifiedGeometrySink.BeginFigure(DrawingPointF startPoint, FigureBegin figureBegin)
		{
			_GeometryBuilder.MoveTo(startPoint.X, startPoint.Y);
		}

		void SimplifiedGeometrySink.AddLines(DrawingPointF[] ointsRef)
		{
			foreach(DrawingPointF point in ointsRef)
			{
				_GeometryBuilder.LineTo(point.X, point.Y);
			}
		}

		void SimplifiedGeometrySink.AddBeziers(BezierSegment[] beziers)
		{
			foreach(BezierSegment segment in beziers)
			{
				_GeometryBuilder.BezierCurveTo(
					segment.Point1.X,
					segment.Point1.Y,
					segment.Point2.X,
					segment.Point2.Y,
					segment.Point3.X,
					segment.Point3.Y);
			}
		}

		void SimplifiedGeometrySink.EndFigure(FigureEnd figureEnd)
		{
			if(figureEnd == FigureEnd.Closed)
			{
				_GeometryBuilder.Close();
			}
		}

		void SimplifiedGeometrySink.Close()
		{
		}

		/// <summary>
		///   This method creates a Frost cluster geometry from DirectWrite glyphs.
		/// </summary>
		/// <param name="key"> This parameter contains the cluster key information. </param>
		/// <param name="bidiLevel"> This parameter contains the bidi level for the cluster. </param>
		/// <param name="font"> This parameter references the font for the cluster. </param>
		/// <returns> This method returns the geometry for the cluster key. </returns>
		public Geometry CreateGeometry(TextGeometryKey key, byte bidiLevel, FontHandle font)
		{
			Contract.Requires(font != null);

			FontFace face = font.ResolveFace();

			_GeometryBuilder = Geometry.Create();

			Geometry result;

			try
			{
				bool isRightToLeft = Convert.ToBoolean(bidiLevel & 1);

				face.GetGlyphRunOutline(
					1.0f, key.Indices, key.Advances, key.Offsets, false, isRightToLeft, this);
			}
			finally
			{
				result = _GeometryBuilder.Build();
			}

			return result;
		}
	}
}