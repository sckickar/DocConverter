namespace DocGen.Pdf.Graphics;

internal struct CHARFORMAT
{
	public int cbSize;

	public uint dwMask;

	public uint dwEffects;

	public int yHeight;

	public int yOffset;

	public int crTextColor;

	public byte bCharSet;

	public byte bPitchAndFamily;

	public char[] szFaceName;

	public short wWeight;

	public short sSpacing;

	public int crBackColor;

	public uint lcid;

	public uint dwReserved;

	public short sStyle;

	public short wKerning;

	public byte bUnderlineType;

	public byte bAnimation;

	public byte bRevAuthor;

	public byte bReserved1;
}
