namespace DocGen.DocIO.ReaderWriter;

public enum WordChunkType
{
	Text,
	ParagraphEnd,
	SectionEnd,
	PageBreak,
	ColumnBreak,
	DocumentEnd,
	Image,
	Shape,
	Table,
	TableRow,
	TableCell,
	Footnote,
	FieldBeginMark,
	FieldSeparator,
	FieldEndMark,
	Tab,
	Annotation,
	LineBreak,
	Symbol,
	CurrentPageNumber,
	EndOfSubdocText
}
