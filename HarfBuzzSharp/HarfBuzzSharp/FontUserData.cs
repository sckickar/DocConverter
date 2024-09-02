namespace HarfBuzzSharp;

internal class FontUserData
{
	public Font Font { get; }

	public object FontData { get; }

	public FontUserData(Font font, object fontData)
	{
		Font = font;
		FontData = fontData;
	}
}
