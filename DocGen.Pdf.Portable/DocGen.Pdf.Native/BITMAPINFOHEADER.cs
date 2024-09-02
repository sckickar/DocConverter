namespace DocGen.Pdf.Native;

internal struct BITMAPINFOHEADER
{
	public int biSize;

	public int biWidth;

	public int biHeight;

	public short biPlanes;

	public short biBitCount;

	public DIB_COMPRESSION biCompression;

	public uint biSizeImage;

	public int biXPelsPerMeter;

	public int biYPelsPerMeter;

	public int biClrUsed;

	public int biClrImportant;
}
