using System;
using System.Diagnostics.Contracts;

namespace Frost.Diagnostics
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IDeviceCounter))]
		internal abstract class IDeviceCounterContract : IDeviceCounter
		{
			public string Name
			{
				get
				{
					Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

					throw new NotSupportedException();
				}
			}

			public string Category
			{
				get
				{
					Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

					throw new NotSupportedException();
				}
			}

			public object Value
			{
				get
				{
					Contract.Ensures(Contract.Result<object>() != null);

					throw new NotSupportedException();
				}
			}

			public object Average
			{
				get
				{
					Contract.Ensures(Contract.Result<object>() != null);

					throw new NotSupportedException();
				}
			}

			public object Minimum
			{
				get
				{
					Contract.Ensures(Contract.Result<object>() != null);

					throw new NotSupportedException();
				}
			}

			public object Maximum
			{
				get
				{
					Contract.Ensures(Contract.Result<object>() != null);

					throw new NotSupportedException();
				}
			}
		}
	}

	[ContractClass(typeof(Contracts.IDeviceCounterContract))]
	public interface IDeviceCounter
	{
		string Name { get; }
		string Category { get; }

		object Value { get; }
		object Average { get; }
		object Minimum { get; }
		object Maximum { get; }
	}

	public interface IDeviceCounter<T> : IDeviceCounter
	{
		new T Value { get; }
		new T Average { get; }
		new T Minimum { get; }
		new T Maximum { get; }
	}
}