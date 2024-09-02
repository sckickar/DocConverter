namespace DocGen.Office;

internal enum TokenType
{
	Unknown,
	DictionaryStart,
	DictionaryEnd,
	StreamStart,
	StreamEnd,
	HexStringStart,
	HexStringEnd,
	String,
	UnicodeString,
	Number,
	Real,
	Name,
	ArrayStart,
	ArrayEnd,
	Reference,
	ObjectStart,
	ObjectEnd,
	Boolean,
	HexDigit,
	Eof,
	Trailer,
	StartXRef,
	XRef,
	Null,
	ObjectType,
	HexStringWeird,
	HexStringWeirdEscape,
	WhiteSpace
}
