namespace DocGen.PdfViewer.Base;

internal enum Token
{
	Operator,
	Integer,
	Real,
	Name,
	ArrayStart,
	ArrayEnd,
	Unknown,
	Keyword,
	String,
	Boolean,
	DictionaryStart,
	Null,
	EndOfFile,
	DictionaryEnd,
	StartXRef,
	XRef,
	StreamStart,
	StreamEnd,
	IndirectObjectStart,
	IndirectObjectEnd,
	IndirectReference,
	Trailer
}
