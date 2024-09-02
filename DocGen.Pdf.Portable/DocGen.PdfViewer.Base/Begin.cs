namespace DocGen.PdfViewer.Base;

internal class Begin : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptDict lastAs = interpreter.Operands.GetLastAs<PostScriptDict>();
		interpreter.DictionaryStack.Push(lastAs);
	}
}
