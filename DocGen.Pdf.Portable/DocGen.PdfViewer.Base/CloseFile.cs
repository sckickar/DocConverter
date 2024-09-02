namespace DocGen.PdfViewer.Base;

internal class CloseFile : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.GetLast();
		interpreter.Reader.PopEncryption();
	}
}
