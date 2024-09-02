using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal interface IWordReaderBase
{
	bool HasTableBody { get; }

	WordStyleSheet StyleSheet { get; }

	int CurrentStyleIndex { get; }

	WordChunkType ChunkType { get; }

	string TextChunk { get; set; }

	CharacterPropertyException CHPX { get; }

	ParagraphPropertyException PAPX { get; }

	int CurrentTextPosition { get; set; }

	BookmarkInfo[] Bookmarks { get; }

	Dictionary<int, string> SttbfRMarkAuthorNames { get; }

	Stack<Dictionary<WTableRow, short>> TableRowWidthStack { get; }

	List<short> MaximumTableRowWidth { get; }

	WordChunkType ReadChunk();

	IWordImageReader GetImageReader(WordDocument doc);

	DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase GetDrawingObject();

	FormField GetFormField(FieldType fieldType);

	bool ReadWatermark(WordDocument doc, WTextBody m_textBody);
}
