namespace DocGen.PdfViewer.Base;

internal class ND : PostScriptOperators
{
	public static bool IsNDOperator(string name)
	{
		if (!(name == "ND"))
		{
			return name == "|-";
		}
		return true;
	}

	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.ExecuteProcedure(interpreter.ND);
	}
}
