﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Formatting.Contracts;

namespace Frost.Formatting
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IFormatter))] internal abstract class IFormatterContract : IFormatter
		{
			public LineBreaker LineBreaker
			{
				get
				{
					Contract.Ensures(Contract.Result<LineBreaker>() != null);

					throw new NotSupportedException();
				}
			}

			public abstract FontMetrics MeasureFont(
				string family, FontWeight weight, FontStyle style, FontStretch stretch);

			public ITextMetrics MeasureLayout(Paragraph paragraph)
			{
				Contract.Requires(paragraph != null);

				throw new NotSupportedException();
			}

			public ITextMetrics MeasureLayout(Paragraph paragraph, Point location)
			{
				Contract.Requires(paragraph != null);

				throw new NotSupportedException();
			}

			public ITextMetrics MeasureLayout(
				Paragraph paragraph, Rectangle region, params Rectangle[] obstructions)
			{
				Contract.Requires(paragraph != null);

				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(IFormatterContract))] public interface IFormatter
	{
		LineBreaker LineBreaker { get; }

		// Aggregator
		// Shaper
		// Typesetter
		// Layouter

		//TODO: adds the collater? aggregator? what to call it?

		FontMetrics MeasureFont(
			string family, FontWeight weight, FontStyle style, FontStretch stretch);

		ITextMetrics MeasureLayout(Paragraph paragraph);

		ITextMetrics MeasureLayout(Paragraph paragraph, Point location);

		ITextMetrics MeasureLayout(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions);
	}
}