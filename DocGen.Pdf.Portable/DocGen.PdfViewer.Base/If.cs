namespace DocGen.PdfViewer.Base;

internal class If : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptArray lastAs = interpreter.Operands.GetLastAs<PostScriptArray>();
		if (interpreter.Operands.GetLastAs<bool>())
		{
			interpreter.ExecuteProcedure(lastAs);
		}
	}
}
