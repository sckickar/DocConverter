namespace DocGen.Pdf.Native;

internal struct XFORM
{
	public float eM11;

	public float eM12;

	public float eM21;

	public float eM22;

	public float eDx;

	public float eDy;

	public override string ToString()
	{
		return eM11 + " " + eM12 + " " + eM21 + " " + eM22 + " " + eDx + " " + eDy;
	}
}
