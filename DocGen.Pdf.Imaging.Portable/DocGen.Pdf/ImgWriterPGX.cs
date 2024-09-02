using System.IO;

namespace DocGen.Pdf;

internal class ImgWriterPGX
{
	internal int maxVal;

	internal int minVal;

	internal int levShift;

	internal bool isSigned;

	private int bitDepth;

	private FileStream out_Renamed;

	private int offset;

	private DataBlockInt db = new DataBlockInt();

	private int fb;

	private int c;

	private int packBytes;

	private byte[] buf;
}
