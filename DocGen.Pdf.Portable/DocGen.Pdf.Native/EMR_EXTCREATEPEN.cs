namespace DocGen.Pdf.Native;

internal struct EMR_EXTCREATEPEN
{
	public int ihPen;

	public int offBmi;

	public int cbBmi;

	public int offBits;

	public int cbBits;

	public int elpPenStyle;

	public int elpWidth;

	public uint elpBrushStyle;

	public int elpColor;

	public nint elpHatch;

	public int elpNumEntries;

	public int[] elpStyleEntry;
}
