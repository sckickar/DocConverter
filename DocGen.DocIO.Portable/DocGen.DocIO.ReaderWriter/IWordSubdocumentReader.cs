using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal interface IWordSubdocumentReader : IWordReaderBase
{
	WordSubdocument Type { get; }

	HeaderType HeaderType { get; }

	int ItemNumber { get; }

	void Reset();

	void MoveToItem(int itemIndex);
}
