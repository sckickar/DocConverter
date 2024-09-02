namespace DocGen.PdfViewer.Base;

internal class Dict : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptDict obj = new PostScriptDict(interpreter.Operands.GetLastAsInt());
		interpreter.Operands.AddLast(obj);
	}
}
