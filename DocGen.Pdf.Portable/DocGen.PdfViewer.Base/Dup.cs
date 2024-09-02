namespace DocGen.PdfViewer.Base;

internal class Dup : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last = interpreter.Operands.GetLast();
		interpreter.Operands.AddLast(last);
		interpreter.Operands.AddLast(last);
	}
}
