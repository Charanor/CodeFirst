using CodeFirst.Graphics.Core;

namespace CodeFirst.Graphics.Text;

public record FontAtlas(Texture2D Texture, float CharacterPixelSize, float DistanceRange);