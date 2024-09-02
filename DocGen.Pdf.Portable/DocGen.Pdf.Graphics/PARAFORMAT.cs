namespace DocGen.Pdf.Graphics;

internal struct PARAFORMAT
{
	public int cbSize;

	public uint dwMask;

	public short wNumbering;

	public short wReserved;

	public int dxStartIndent;

	public int dxRightIndent;

	public int dxOffset;

	public short wAlignment;

	public short cTabCount;

	public int[] rgxTabs;

	public int dySpaceBefore;

	public int dySpaceAfter;

	public int dyLineSpacing;

	public short sStyle;

	public byte bLineSpacingRule;

	public byte bOutlineLevel;

	public short wShadingWeight;

	public short wShadingStyle;

	public short wNumberingStart;

	public short wNumberingStyle;

	public short wNumberingTab;

	public short wBorderSpace;

	public short wBorderWidth;

	public short wBorders;
}
