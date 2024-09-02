using DocGen.DocIO.DLS.Entities;

namespace DocGen.DocIO.ReaderWriter;

internal interface IWordImageReader
{
	Image Image { get; }

	int WidthScale { get; }

	int HeightScale { get; }

	short Width { get; }

	short Height { get; }
}
