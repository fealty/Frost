using System.Diagnostics;
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

			Trace.Assert(atlas != null);

			this._Value = true;
			this._Atlas = atlas;
		}

		public void Invalidate()
		{
			this._Value = false;
		}

		public ISurfaceAtlas Atlas
		{
			get
			{
				Contract.Ensures(Contract.Result<ISurfaceAtlas>() != null);
				
				return this._Atlas;
			}
		}

		public bool Value
		{
			get { return this._Value; }
		}
	}
}