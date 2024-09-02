namespace DocGen.PdfViewer.Base;

internal class Array : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptArray obj = new PostScriptArray(interpreter.Operands.GetLastAsInt());
		interpreter.Operands.AddLast(obj);
	}
}
