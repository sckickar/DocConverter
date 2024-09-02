using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal interface IWordSubdocumentWriter : IWordWriterBase
{
	WordSubdocument Type { get; }

	void WriteDocumentEnd();

	void WriteItemStart();

	void WriteItemEnd();
}
