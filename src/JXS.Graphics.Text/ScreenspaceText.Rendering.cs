using System.Diagnostics;
using JXS.Graphics.Core;
using JXS.Graphics.Renderer2D;
using JXS.Graphics.Utils;
using OpenTK.Mathematics;

namespace JXS.Graphics.Text;

public partial class ScreenspaceText
{
	// When we can't render a character, try to render these characters in order instead
	private static readonly char[] FallbackCharacters =
	{
		'⍰',
		'?',
		'.',
		' '
	};

	/// <summary>
	///     Draws this text to the screen. Will use cached texture if possible.
	/// </summary>
	public void Draw(SpriteBatch spriteBatch)
	{
		if (CachedTexture == null)
		{
			DrawToTexture(spriteBatch);
			return;
		}

		DrawCachedTexture(spriteBatch);
	}

	private void DrawCachedTexture(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(CachedTexture!, Position, Size);
	}

	private void DrawToTexture(SpriteBatch spriteBatch)
	{
		CachedTexture = new Texture2D((int)Size.X, (int)Size.Y);
		using var _ = frameBuffer.Binding();
		DrawImmediate(spriteBatch);
	}

	/// <summary>
	///     Draws this text to the screen immediately. Will not use cached texture. This has better performance if the
	///     text changes very often (such as an FPS counter).
	/// </summary>
	public void DrawImmediate(SpriteBatch spriteBatch)
	{
		if (font.Atlas.Texture is not Texture2D atlas2D)
		{
			// TODO: Maybe throw?
			return;
		}


		var shader = font.Shader;
		shader.FontAtlas = atlas2D;
		shader.BackgroundColor = Color4.White.ToVector4();
		shader.ForegroundColor = Color.ToVector4();
		shader.DistanceFieldRange = font.Atlas.DistanceRange;

		var prevShader = spriteBatch.Shader;
		if (!spriteBatch.IsStarted)
		{
			spriteBatch.Shader = shader;
			spriteBatch.Begin();

			FontGlyph? previousGlyph = null;
			var virtualCursor = Vector2.Zero;
			for (var i = 0; i < Text.Length; i++)
			{
				var character = text[i];
				foreach (var fallbackCharacter in FallbackCharacters)
				{
					if (!Font.SupportsCharacter(fallbackCharacter))
					{
						continue;
					}

					character = fallbackCharacter;
					break;
				}

				// It might not support ANY character
				if (!Font.SupportsCharacter(character))
				{
					// Just don't render it I guess...
					continue;
				}

				Debug.Assert(Font.TryGetGlyph(character, out var glyph));
				RenderGlyph(ref virtualCursor, spriteBatch, glyph, previousGlyph);
				previousGlyph = glyph;
			}

			spriteBatch.End();
		}
		else
		{
			spriteBatch.End(); // SIC!

			spriteBatch.Shader = shader;
			spriteBatch.Begin(); // SIC!
		}

		spriteBatch.Shader = prevShader;
	}

	private void RenderGlyph(ref Vector2 virtualCursor, SpriteBatch spriteBatch, FontGlyph glyph, FontGlyph? previousGlyph)
	{
		var kerning = previousGlyph == null ? 0 : Font.GetKerningBetween(previousGlyph, glyph);

		var cursorOffset = Vector2.Zero;
		
		// spriteBatch.Draw(Font.Atlas.Texture, virtualCursor+cursorOffset, );
		
		virtualCursor += new Vector2(glyph.Advance, y: 0);
	}
}