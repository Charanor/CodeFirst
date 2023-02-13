// using System.Drawing;
// using JXS.Graphics.Core;
// using OpenTK.Mathematics;
//
// using System.Drawing;
// using System.Text;
//
// namespace JXS.Gui;
//
// public sealed class DrawManager : IDisposable, IGraphicsProvider
// {
// 	// Capital M is conventionally the largest (fun fact: this is where the term "em" comes from in CSS :) )
// 	private const string WIDEST_LETTER = "M";
// 	private const string ALL_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
//
// 	private readonly Texture2D whiteRect;
//
// 	private readonly IDictionary<int, float> fontMeasurements;
//
// 	private readonly IList<Box2i> scissors;
// 	
// 	public DrawManager()
// 	{
// 		
// 		var (r, g, b, a) = Color4.White;
// 		var data = BitConverter.GetBytes(r)
// 			.Concat(BitConverter.GetBytes(g))
// 			.Concat(BitConverter.GetBytes(b))
// 			.Concat(BitConverter.GetBytes(a))
// 			.ToArray();
// 		whiteRect = new Texture2D(data, width: 1, height: 1);
//
// 		fontMeasurements = new Dictionary<int, float>();
// 		scissors = new List<Box2i>();
// 	}
//
// 	public void Begin()
// 	{
// 	}
//
// 	public void End()
// 	{
// 	}
//
// 	public void AddScissor(Box2i scissor)
// 	{
// 		if (scissors.Count == 0)
// 		{
// 			// We enabled a scissor, we need to enable the very slow scissor-enabled draw mode.
// 		}
//
// 		scissors.Add(scissor);
// 		RecalculateScissor();
// 	}
//
// 	public void RemoveScissor(Box2i scissor)
// 	{
// 		scissors.Remove(scissor);
// 		RecalculateScissor();
// 		if (scissors.Any())
// 		{
// 			return;
// 		}
//
// 		// We disabled all scissors, let's disable the very slow scissor-enabled draw mode.
// 		Begin();
// 	}
//
// 	private void RecalculateScissor()
// 	{
// 		if (scissors.Count == 0)
// 		{
// 			return;
// 		}
//
// 		var scissor = scissors.Aggregate((sum, next) => sum.Intersected(next));
// 	}
//
// 	public void DrawRect(Box2 bounds, Color4<Rgba> color)
// 	{
// 	}
//
// 	public void DrawImage(Texture2D texture, Box2 bounds)
// 	{
// 		throw new NotImplementedException();
// 	}
//
// 	public void DrawText(int fontSize, string text, Vector2 position, Color4<Rgba> color, float maxTextWidth, bool log = false)
// 	{
// 		if (!fonts.TryGetValue(fontSize, out var font))
// 		{
// 			font = fontSystem.GetFont(fontSize);
// 			fonts[fontSize] = font;
// 		}
//
// 		if (!fontMeasurments.TryGetValue(fontSize, out var estCharacterWidth))
// 		{
// 			estCharacterWidth = font.MeasureString(ALL_LETTERS).X / ALL_LETTERS.Length;
// 			fontMeasurments.Add(fontSize, estCharacterWidth);
// 		}
//
// 		var estCharactersPerLine = Math.Floor(maxWidth / estCharacterWidth);
// 		if (log)
// 		{
// 			Console.WriteLine(
// 				$"Max width: {maxWidth}, textLen: {text.Length}, eCharLine: {estCharactersPerLine}, eCharW: {estCharacterWidth}");
// 		}
//
// 		// if (maxWidth > 0 && text.Length > estCharactersPerLine)
// 		// {
// 		//     var lineLength = 0;
// 		//     var sb = new StringBuilder();
// 		//     var words = Regex.Split(text, @"\s+");
// 		//
// 		//     for (var i = 0; i < words.Length; i++)
// 		//     {
// 		//         var word = words[i];
// 		//         sb.Append(word);
// 		//
// 		//         lineLength += word.Length;
// 		//         if (lineLength >= estCharactersPerLine)
// 		//         {
// 		//             sb.Append(Environment.NewLine);
// 		//             lineLength = 0;
// 		//         }
// 		//         else if (i < words.Length - 1)
// 		//         {
// 		//             sb.Append(' ');
// 		//             lineLength += 1;
// 		//         }
// 		//     }
// 		//
// 		//     if (log)
// 		//         Console.WriteLine(sb);
// 		//     text = sb.ToString();
// 		// }
//
// 		batch.DrawString(font, text, position, color);
// 	}
//
// 	public Vector2 MeasureText(int fontSize, string text)
// 	{
// 		if (!fonts.TryGetValue(fontSize, out var font))
// 		{
// 			font = fontSystem.GetFont(fontSize);
// 			fonts[fontSize] = font;
// 		}
//
// 		return font.MeasureString(text);
// 	}
//
// 	public void Dispose()
// 	{
// 		whiteRect.Dispose();
// 	}
// }

