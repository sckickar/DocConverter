namespace DocGen.PdfViewer.Base;

internal class CurrentDict : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.AddLast(interpreter.CurrentDictionary);
	}
}
