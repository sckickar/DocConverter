namespace DocGen.PdfViewer.Base;

internal class Exch : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last = interpreter.Operands.GetLast();
		object last2 = interpreter.Operands.GetLast();
		interpreter.Operands.AddLast(last);
		interpreter.Operands.AddLast(last2);
	}
}
