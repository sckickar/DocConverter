using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontOutlinePoint
{
	public Point Point { get; set; }

	public byte Flags { get; set; }

	public byte Instruction { get; set; }

	public bool IsOnCurve => (Flags & 1) != 0;

	public SystemFontOutlinePoint(byte flags)
	{
		Flags = flags;
	}

	public SystemFontOutlinePoint(double x, double y, byte flags)
		: this(flags)
	{
		Point = new Point(x, y);
	}
}
