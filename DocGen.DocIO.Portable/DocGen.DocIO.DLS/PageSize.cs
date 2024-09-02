using System;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public sealed class PageSize
{
	public static readonly SizeF A3 = new SizeF(842f, 1190f);

	public static readonly SizeF A4 = new SizeF(595f, 842f);

	public static readonly SizeF A5 = new SizeF(421f, 595f);

	public static readonly SizeF A6 = new SizeF(297f, 421f);

	public static readonly SizeF B4 = new SizeF(709f, 1002f);

	public static readonly SizeF B5 = new SizeF(501f, 709f);

	public static readonly SizeF B6 = new SizeF(501f, 354f);

	public static readonly SizeF Letter = new SizeF(612f, 792f);

	public static readonly SizeF HalfLetter = new SizeF(396f, 612f);

	public static readonly SizeF Letter11x17 = new SizeF(792f, 1224f);

	public static readonly SizeF EnvelopeDL = new SizeF(312f, 624f);

	public static readonly SizeF Quarto = new SizeF(610f, 780f);

	public static readonly SizeF Statement = new SizeF(396f, 612f);

	public static readonly SizeF Ledger = new SizeF(1224f, 792f);

	public static readonly SizeF Tabloid = new SizeF(792f, 1224f);

	public static readonly SizeF Note = new SizeF(540f, 720f);

	public static readonly SizeF Legal = new SizeF(612f, 1008f);

	public static readonly SizeF Flsa = new SizeF(612f, 936f);

	public static readonly SizeF Executive = new SizeF(522f, 756f);

	private PageSize()
	{
		throw new NotSupportedException();
	}
}
