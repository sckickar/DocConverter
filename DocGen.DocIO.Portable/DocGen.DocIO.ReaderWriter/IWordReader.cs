using System;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal interface IWordReader : IWordReaderBase
{
	bool IsFootnote { get; }

	bool IsEndnote { get; }

	int SectionNumber { get; }

	SectionProperties SectionProperties { get; }

	DOPDescriptor DOP { get; }

	event NeedPasswordEventHandler NeedPassword;

	IWordSubdocumentReader GetSubdocumentReader(WordSubdocument subDocumentType);

	void ReadDocumentHeader(WordDocument doc);

	void ReadDocumentEnd();

	BookmarkInfo[] GetBookmarks();
}
