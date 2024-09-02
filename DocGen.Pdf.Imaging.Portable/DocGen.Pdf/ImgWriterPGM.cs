using System.IO;

namespace DocGen.Pdf;

internal class ImgWriterPGM
{
	private int levShift;

	private FileStream out_Renamed;

	private int c;

	private int fb;

	private DataBlockInt db = new DataBlockInt();

	private int offset;

	private byte[] buf;
}
