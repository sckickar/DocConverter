namespace DocGen.PdfViewer.Base;

internal class String : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		int lastAsInt = interpreter.Operands.GetLastAsInt();
		interpreter.Operands.AddLast(new PostScriptStrHelper(lastAsInt));
	}
}
