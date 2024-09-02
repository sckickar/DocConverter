namespace DocGen.PdfViewer.Base;

internal class ClearToMark : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last;
		do
		{
			last = interpreter.Operands.GetLast();
		}
		while (last == null || !(last is Mark));
	}
}
