namespace DocGen.PdfViewer.Base;

internal class NP : PostScriptOperators
{
	public static bool IsNPOperator(string name)
	{
		if (!(name == "NP"))
		{
			return name == "|";
		}
		return true;
	}

	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.ExecuteProcedure(interpreter.NP);
	}
}
