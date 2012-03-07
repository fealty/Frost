// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Formatting.Contracts;

namespace Frost.Formatting
{
	namespace Contracts
	{
		[ContractClassFor(typeof(ITextMetrics))] internal abstract class ITextMetricsContract
			: ITextMetrics
		{
			public Paragraph Paragraph
			{
				get
				{
					Contract.Ensures(Contract.Result<Paragraph>() != null);

					throw new NotSupportedException();
				}
			}

			public abstract Rectangle TextRegion { get; }
			public abstract Rectangle LayoutRegion { get; }

			public float Leading
			{
				get
				{
					Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

					throw new NotSupportedException();
				}
			}

			public abstract Size BaselineOffset { get; }

			public LineCollection Lines
			{
				get
				{
					Contract.Ensures(Contract.Result<LineCollection>() != null);

					throw new NotSupportedException();
				}
			}

			public RegionCollection Regions
			{
				get
				{
					Contract.Ensures(Contract.Result<RegionCollection>() != null);

					throw new NotSupportedException();
				}
			}

			public OutlineCollection Outlines
			{
				get
				{
					Contract.Ensures(Contract.Result<OutlineCollection>() != null);

					throw new NotSupportedException();
				}
			}

			public bool GetLineForText(int textIndex, out int lineIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);
				Contract.Ensures(Contract.ValueAtReturn(out lineIndex) >= 0);
				Contract.Ensures(Contract.ValueAtReturn(out lineIndex) < Lines.Count);

				throw new NotSupportedException();
			}

			public bool FindLineNear(Point position, out int lineIndex)
			{
				Contract.Ensures(Contract.ValueAtReturn(out lineIndex) >= 0);
				Contract.Ensures(Contract.ValueAtReturn(out lineIndex) < Lines.Count);

				throw new NotSupportedException();
			}

			public bool FindTextNear(Point position, out int textIndex)
			{
				Contract.Ensures(Contract.ValueAtReturn(out textIndex) >= 0);
				Contract.Ensures(Contract.ValueAtReturn(out textIndex) < Regions.Count);

				throw new NotSupportedException();
			}

			public bool IsRightToLeft(int textIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);

				throw new NotSupportedException();
			}

			public bool IsClusterStart(int textIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);

				throw new NotSupportedException();
			}

			public bool IsClusterEnd(int textIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);

				throw new NotSupportedException();
			}

			public bool IsVisible(int textIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);

				throw new NotSupportedException();
			}

			public bool IsLineStart(int textIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);

				throw new NotSupportedException();
			}

			public bool IsLineEnd(int textIndex)
			{
				Contract.Requires(textIndex >= 0);
				Contract.Requires(textIndex < Regions.Count);

				throw new NotSupportedException();
			}

			public void ComputeRegion(IndexedRange textRange, out Rectangle result)
			{
				Contract.Requires(textRange.StartIndex < Regions.Count);
				Contract.Requires(textRange.LastIndex < Regions.Count);

				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(ITextMetricsContract))] public interface ITextMetrics
	{
		Paragraph Paragraph { get; }

		Rectangle TextRegion { get; }

		Rectangle LayoutRegion { get; }

		float Leading { get; }

		Size BaselineOffset { get; }

		LineCollection Lines { get; }

		RegionCollection Regions { get; }

		OutlineCollection Outlines { get; }

		bool GetLineForText(int textIndex, out int lineIndex);

		bool FindLineNear(Point position, out int lineIndex);

		bool FindTextNear(Point position, out int textIndex);

		bool IsRightToLeft(int textIndex);

		bool IsClusterStart(int textIndex);

		bool IsClusterEnd(int textIndex);

		bool IsVisible(int textIndex);

		bool IsLineStart(int textIndex);

		bool IsLineEnd(int textIndex);

		void ComputeRegion(IndexedRange textRange, out Rectangle result);
	}
}