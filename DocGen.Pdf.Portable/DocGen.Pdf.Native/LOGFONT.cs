namespace DocGen.Pdf.Native;

internal class LOGFONT
{
	public int lfHeight;

	public int lfWidth;

	public int lfEscapement;

	public int lfOrientation;

	public FW_FONT_WEIGHT lfWeight = FW_FONT_WEIGHT.FW_NORMAL;

	public bool lfItalic;

	public bool lfUnderline;

	public bool lfStrikeOut;

	public byte lfCharSet;

	public byte lfOutPrecision;

	public byte lfClipPrecision;

	public byte lfQuality;

	public byte lfPitchAndFamily;

	public string lfFaceName;
}
