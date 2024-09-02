namespace DocGen.PdfViewer.Base;

internal class End : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.DictionaryStack.Pop();
	}
}
