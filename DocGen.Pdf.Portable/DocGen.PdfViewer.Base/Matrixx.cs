namespace DocGen.PdfViewer.Base;

internal struct Matrixx
{
	internal Matrixx Identity => new Matrixx(1.0, 0.0);

	internal double M11 { get; set; }

	internal double M12 { get; set; }

	internal Matrixx(double m11, double m12)
	{
		this = default(Matrixx);
		M11 = m11;
		M12 = m12;
	}
}
