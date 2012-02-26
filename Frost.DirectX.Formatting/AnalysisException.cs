using System;

namespace Frost.DirectX.Formatting
{
	internal sealed class AnalysisException : Exception
	{
		public AnalysisException(Exception exception)
			: base("Text analysis failed!", exception)
		{
		}
	}
}