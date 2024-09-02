namespace DocGen.PdfViewer.Base;

internal class Pops : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.GetLast();
	}
}
