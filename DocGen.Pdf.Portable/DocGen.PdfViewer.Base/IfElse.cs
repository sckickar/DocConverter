namespace DocGen.PdfViewer.Base;

internal class IfElse : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptArray lastAs = interpreter.Operands.GetLastAs<PostScriptArray>();
		PostScriptArray lastAs2 = interpreter.Operands.GetLastAs<PostScriptArray>();
		if (interpreter.Operands.GetLastAs<bool>())
		{
			interpreter.ExecuteProcedure(lastAs2);
		}
		else
		{
			interpreter.ExecuteProcedure(lastAs);
		}
	}
}
