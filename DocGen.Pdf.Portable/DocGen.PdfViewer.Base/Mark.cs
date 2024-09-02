namespace DocGen.PdfViewer.Base;

internal class Mark : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.AddLast(this);
	}
}
