using CodeFirst.Graphics.Core;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.G2D;

public interface IBatch : IDisposable
{
	/// <summary>
	///     If blending is enabled or not. Defaults to <c>true</c>.
	/// </summary>
	/// <remarks>
	///     Setting this between <see cref="Begin" /> and <see cref="End" /> will <see cref="Rasterize" /> the batch.
	/// </remarks>
	bool IsBlendingEnabled { get; set; }

	/// <summary>
	///     The "src" blend.
	/// </summary>
	/// <remarks>
	///     Setting this between <see cref="Begin" /> and <see cref="End" /> will <see cref="Rasterize" /> the batch.
	/// </remarks>
	BlendingFactor SourceBlendFactor { get; set; }

	/// <summary>
	///     The "dst" blend.
	/// </summary>
	/// <remarks>
	///     Setting this between <see cref="Begin" /> and <see cref="End" /> will <see cref="Rasterize" /> the batch.
	/// </remarks>
	BlendingFactor DestinationBlendFactor { get; set; }

	/// <summary>
	///     The projection matrix used for drawing. Should be called <b>before</b> <see cref="Begin" />.
	/// </summary>
	/// <example>
	///     <c>batch.ProjectionMatrix = camera.Combined;</c>
	/// </example>
	/// <exception cref="InvalidOperationException">If called between <see cref="Begin" /> and <see cref="End" />.</exception>
	Matrix4 ProjectionMatrix { get; set; }

	/// <summary>
	///     <c>true</c> if between <see cref="Begin" /> and <see cref="End" /> calls, <c>false</c> otherwise.
	/// </summary>
	bool IsBatching { get; }

	/// <summary>
	///     Prepares this batch for batching. Disabled depth buffer and enables blending.
	/// </summary>
	/// <remarks>
	///     To end batching and rasterize container call <see cref="End" />.
	/// </remarks>
	/// <example>
	///     <code>
	/// 		batch.Begin();
	/// 		batch.Draw(...);
	/// 		batch.Draw(...);
	/// 		batch.End();
	/// 		</code>
	/// </example>
	/// <exception cref="InvalidOperationException">if <see cref="IsBatching" /></exception>
	void Begin();

	/// <summary>
	///     Finalizes batching and calls <see cref="Rasterize" /> to draw to the screen.
	/// </summary>
	/// <remarks>
	///     Must only be called after <see cref="Begin" />.
	/// </remarks>
	/// <exception cref="InvalidOperationException">if not <see cref="IsBatching" /></exception>
	void End();

	/// <summary>
	///     Rasterizes any batched calls currently in the batch. Can be used to manually draw to the screen without
	///     <see cref="End" />-ing the batch.
	/// </summary>
	/// <remarks>
	///     This is automatically called by <see cref="End" />, thus manually calling <see cref="Rasterize" /> is
	///     advanced use only.
	/// </remarks>
	void Rasterize();

	/// <summary>
	///     Batches the given texture for drawing. To draw only a certain region of the texture, use the
	///     <c>source{X|Y|Width|Height}</c> parameters.
	/// </summary>
	/// <param name="texture">the texture</param>
	/// <param name="x">the X coordinate in screen space</param>
	/// <param name="y">the Y coordinate in screen space</param>
	/// <param name="width">the width in world units</param>
	/// <param name="height">the height in world units</param>
	/// <param name="sourceX">the X coordinate in pixel (texel) space</param>
	/// <param name="sourceY">the Y coordinate in pixel (texel) space</param>
	/// <param name="sourceWidth">the width in pixel (texel) space</param>
	/// <param name="sourceHeight">the height in pixel (texel) space</param>
	void Draw(Texture2D texture, float x, float y, float width = default, float height = default, int sourceX = default,
		int sourceY = default, int sourceWidth = default, int sourceHeight = default);
}