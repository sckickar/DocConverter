using System;
using DocGen.Drawing;

namespace DocGen.Pdf;

public sealed class PdfPageSize
{
	public static readonly SizeF Letter = new SizeF(612f, 792f);

	public static readonly SizeF Note = new SizeF(540f, 720f);

	public static readonly SizeF Legal = new SizeF(612f, 1008f);

	public static readonly SizeF A0 = new SizeF(2380f, 3368f);

	public static readonly SizeF A1 = new SizeF(1684f, 2380f);

	public static readonly SizeF A2 = new SizeF(1190f, 1684f);

	public static readonly SizeF A3 = new SizeF(842f, 1190f);

	public static readonly SizeF A4 = new SizeF(595f, 842f);

	public static readonly SizeF A5 = new SizeF(421f, 595f);

	public static readonly SizeF A6 = new SizeF(297f, 421f);

	public static readonly SizeF A7 = new SizeF(210f, 297f);

	public static readonly SizeF A8 = new SizeF(148f, 210f);

	public static readonly SizeF A9 = new SizeF(105f, 148f);

	public static readonly SizeF A10 = new SizeF(74f, 105f);

	public static readonly SizeF B0 = new SizeF(2836f, 4008f);

	public static readonly SizeF B1 = new SizeF(2004f, 2836f);

	public static readonly SizeF B2 = new SizeF(1418f, 2004f);

	public static readonly SizeF B3 = new SizeF(1002f, 1418f);

	public static readonly SizeF B4 = new SizeF(709f, 1002f);

	public static readonly SizeF B5 = new SizeF(501f, 709f);

	public static readonly SizeF ArchE = new SizeF(2592f, 3456f);

	public static readonly SizeF ArchD = new SizeF(1728f, 2592f);

	public static readonly SizeF ArchC = new SizeF(1296f, 1728f);

	public static readonly SizeF ArchB = new SizeF(864f, 1296f);

	public static readonly SizeF ArchA = new SizeF(648f, 864f);

	public static readonly SizeF Flsa = new SizeF(612f, 936f);

	public static readonly SizeF HalfLetter = new SizeF(396f, 612f);

	public static readonly SizeF Letter11x17 = new SizeF(792f, 1224f);

	public static readonly SizeF Ledger = new SizeF(1224f, 792f);

	private PdfPageSize()
	{
		throw new NotSupportedException();
	}
}
