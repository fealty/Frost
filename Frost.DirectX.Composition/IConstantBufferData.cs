// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.DirectX.Composition.Contracts;

using SharpDX;

using SDX = SharpDX;

namespace Frost.DirectX.Composition
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IConstantBufferData))] internal abstract class
			IConstantBufferDataContract : IConstantBufferData
		{
			public int ByteSize
			{
				get
				{
					Contract.Ensures(Contract.Result<int>() >= 0);

					throw new NotSupportedException();
				}
			}

			public void Serialize(DataStream stream)
			{
				Contract.Requires(stream != null);
			}
		}
	}

	[ContractClass(typeof(IConstantBufferDataContract))] public interface IConstantBufferData
	{
		int ByteSize { get; }

		void Serialize(DataStream stream);
	}
}