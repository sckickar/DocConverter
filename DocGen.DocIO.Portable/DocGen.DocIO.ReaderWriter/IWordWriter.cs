using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal interface IWordWriter : IWordWriterBase
{
	DOPDescriptor DOP { get; }

	SectionProperties SectionProperties { get; }

	void WriteDocumentHeader();

	void WriteDocumentEnd(string password, string author, ushort fibVersion, Dictionary<string, Storage> oleObjectCollection);

	IWordSubdocumentWriter GetSubdocumentWriter(WordSubdocument subDocumentType);

	void InsertPageBreak();
}
