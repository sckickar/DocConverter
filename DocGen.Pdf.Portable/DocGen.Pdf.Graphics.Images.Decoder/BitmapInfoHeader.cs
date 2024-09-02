namespace DocGen.Pdf.Graphics.Images.Decoder;

internal struct BitmapInfoHeader
{
	public int Size;

	public int Width;

	public int Height;

	public short Planes;

	public short BitsPerPixel;

	public BitmapCompression Compression;

	public int SizeImage;

	public int XPelsPerMeter;

	public int YPelsPerMeter;

	public int ClrUsed;

	public int ClrImportant;
}
