namespace DocGen.PdfViewer.Base;

internal interface IPostScriptParser
{
	bool EndOfFile { get; }

	byte Peek(int skip);

	byte Read();
}
