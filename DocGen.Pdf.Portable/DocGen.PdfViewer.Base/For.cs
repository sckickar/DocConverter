namespace DocGen.PdfViewer.Base;

internal class For : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptArray lastAs = interpreter.Operands.GetLastAs<PostScriptArray>();
		double lastAsReal = interpreter.Operands.GetLastAsReal();
		double lastAsReal2 = interpreter.Operands.GetLastAsReal();
		for (double num = interpreter.Operands.GetLastAsReal(); (lastAsReal > 0.0) ? (num < lastAsReal) : (num > lastAsReal); num += lastAsReal2)
		{
			interpreter.Operands.AddLast(num);
			interpreter.ExecuteProcedure(lastAs);
		}
	}
}
