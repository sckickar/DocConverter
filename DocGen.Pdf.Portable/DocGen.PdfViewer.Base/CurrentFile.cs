namespace DocGen.PdfViewer.Base;

internal class CurrentFile : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.AddLast(null);
	}
}
