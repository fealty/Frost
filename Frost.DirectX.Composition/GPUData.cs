// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Frost.DirectX.Composition
{
	public static class GPUData
	{
		public static int OffsetOf<T>(Expression<Func<T, object>> expression) where T : struct
		{
			Contract.Requires(expression != null);
			Contract.Requires(expression.NodeType == ExpressionType.Lambda);
			Contract.Requires(expression.Body is UnaryExpression);

			UnaryExpression unaExp = (UnaryExpression)expression.Body;

			MemberExpression memExp = (MemberExpression)unaExp.Operand;

			return (int)Marshal.OffsetOf(typeof(T), memExp.Member.Name);
		}

		public static int SizeOf<T>() where T : struct
		{
			return Marshal.SizeOf(typeof(T));
		}
	}
}