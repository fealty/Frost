using System;

namespace Frost.DirectX.Formatting
{
	internal sealed class ShapingException : Exception
	{
		public ShapingException(Exception exception)
			: base("Text shaping failed!", exception)
		{
		}
	}
}