namespace DocGen.PdfViewer.Base;

internal class Index : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		int lastAsInt = interpreter.Operands.GetLastAsInt();
		interpreter.Operands.AddLast(interpreter.Operands.GetElementAt(Origin.End, lastAsInt));
	}
}
