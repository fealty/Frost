// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Frost.Formatting
{
	public sealed class Paragraph
	{
		private static readonly CultureInfo _DefaultCulture;
		private static readonly string _DefaultFamily;
		private static readonly float _DefaultPointSize;
		private static readonly float _DefaultIndentation;
		private static readonly float _DefaultLeading;
		private static readonly float _DefaultSpacing;
		private static readonly float _DefaultTracking;

		[ThreadStatic] private static Builder _Builder;

		private readonly Alignment _Alignment;

		private readonly float _Indentation;
		private readonly float _Leading;
		private readonly TextRunCollection _Runs;

		private readonly float _Spacing;

		private readonly string _Text;
		private readonly float _Tracking;

		static Paragraph()
		{
			_DefaultCulture = CultureInfo.InvariantCulture;
			_DefaultFamily = "arial";
			_DefaultPointSize = 10.0f;
			_DefaultIndentation = 0.0f;
			_DefaultLeading = 0.0f;
			_DefaultSpacing = 1.0f / 3.0f;
			_DefaultTracking = 1.0f / 50.0f;
		}

		private Paragraph(
			string text,
			Alignment alignment,
			float indentation,
			float leading,
			float spacing,
			float tracking,
			TextRunCollection runs)
		{
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(this._Runs != null);
			Contract.Invariant(this._Text != null);
			Contract.Invariant(Check.IsPositive(this._Indentation));
			Contract.Invariant(Check.IsPositive(this._Leading));
			Contract.Invariant(Check.IsPositive(this._Spacing));
			Contract.Invariant(Check.IsPositive(this._Tracking));
		}

		public sealed class Builder
		{
		}
	}
}