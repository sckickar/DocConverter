using System.IO;

namespace DocGen.Pdf.Graphics;

internal interface IImageMetadataParser
{
	MemoryStream GetMetadata();
}
