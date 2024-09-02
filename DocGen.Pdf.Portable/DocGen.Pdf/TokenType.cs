namespace DocGen.Pdf;

internal enum TokenType
{
	None,
	Comment,
	Integer,
	Real,
	String,
	HexString,
	UnicodeString,
	UnicodeHexString,
	Name,
	Operator,
	BeginArray,
	EndArray,
	Eof
}
