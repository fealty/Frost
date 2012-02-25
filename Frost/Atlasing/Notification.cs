// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

namespace Frost.Atlasing
{
	public sealed class Notification
	{
		private readonly ISurfaceAtlas _Atlas;

		private volatile bool _Value;

		public Notification(ISurfaceAtlas atlas)
		{
			Contract.Requires(atlas != null);

			_Value = true;
			_Atlas = atlas;
		}

		public ISurfaceAtlas Atlas
		{
			get
			{
				Contract.Ensures(Contract.Result<ISurfaceAtlas>() != null);

				return _Atlas;
			}
		}

		public bool Value
		{
			get { return _Value; }
		}

		public void Invalidate()
		{
			_Value = false;
		}
	}
}