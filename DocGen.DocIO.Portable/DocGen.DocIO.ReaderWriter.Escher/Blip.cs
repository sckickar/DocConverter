using System.IO;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal abstract class Blip : BaseWordRecord
{
	public Blip()
	{
	}

	public abstract Image Read(Stream stream, int length, bool chr);

	internal abstract void Write(Stream stream, MemoryStream image, MSOBlipType imageFormat, byte[] id);
}
