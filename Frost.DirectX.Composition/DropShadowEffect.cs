// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using Frost.Composition;
using Frost.Effects;

namespace Frost.DirectX.Composition
{
	public sealed class DropShadowEffect : Effect<DropShadowSettings>
	{
		public override void Apply<TEnum>(
			TEnum batchedItems,
			EffectContext<DropShadowSettings> effectContext,
			Frost.Composition.Compositor compositionContext)
		{
			DropShadowSettings settings = effectContext.Options;

			Matrix3X2 sourceTransform = compositionContext.Transformation;

			compositionContext.Transformation = Matrix3X2.Identity;

			compositionContext.PushLayer();

			try
			{
				compositionContext.ResetState();

				// composite the blurred shadow
				///////////////////////////////////////////////////////////
				GaussianBlurSettings blurSettings = new GaussianBlurSettings(new Size(settings.Amount));

				compositionContext.ApplyEffect(blurSettings);

				foreach(BatchedItem item in batchedItems)
				{
					compositionContext.Blend = item.Blend;

					compositionContext.Transformation = sourceTransform;

					Matrix3X2 transform = item.Transformation;

					compositionContext.Transform(ref transform);

					float xPixel = 1.0f / item.SourceRegion.Width;
					float yPixel = 1.0f / item.SourceRegion.Height;

					Point center = item.DestinationRegion.Center;

					compositionContext.Scale(
						1.0f + (settings.Scale.Width * xPixel * 2),
						1.0f + (settings.Scale.Height * yPixel * 2),
						center.X,
						center.Y);

					Rectangle sourceRegion = item.SourceRegion;
					Rectangle destinationRegion = item.DestinationRegion;

					compositionContext.Composite(item.Canvas, sourceRegion, destinationRegion);
				}

				///////////////////////////////////////////////////////////

				compositionContext.ResetState();

				// tell the shader what color to color the shadow
				///////////////////////////////////////////////////////////
				ColorOutputSettings colorSettings = new ColorOutputSettings(
					settings.Color, ColorOperation.Modulate);

				compositionContext.ApplyEffect(colorSettings);

				// translate and color the shadow
				compositionContext.Translate(settings.Offset);
				compositionContext.Blend = BlendOperation.Copy;

				compositionContext.CompositeResult();
				///////////////////////////////////////////////////////////

				compositionContext.ResetState();

				// composite each item atop the shadow
				///////////////////////////////////////////////////////////

				compositionContext.SaveState();

				try
				{
					foreach(BatchedItem item in batchedItems)
					{
						compositionContext.Transformation = sourceTransform;

						Matrix3X2 transform = item.Transformation;

						compositionContext.Transform(ref transform);

						Rectangle sourceRegion = item.SourceRegion;
						Rectangle destinationRegion = item.DestinationRegion;

						compositionContext.Composite(item.Canvas, sourceRegion, destinationRegion);
					}
				}
				finally
				{
					compositionContext.RestoreState();
				}
			}
			finally
			{
				compositionContext.PopLayer();
			}
		}
	}
}