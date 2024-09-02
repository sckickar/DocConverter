namespace DocGen.PdfViewer.Base;

internal class Def : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last = interpreter.Operands.GetLast();
		string lastAs = interpreter.Operands.GetLastAs<string>();
		interpreter.CurrentDictionary[lastAs] = last;
		if (DocGen.PdfViewer.Base.RD.IsRDOperator(lastAs))
		{
			interpreter.RD = (PostScriptArray)last;
		}
		else if (DocGen.PdfViewer.Base.ND.IsNDOperator(lastAs))
		{
			interpreter.ND = (PostScriptArray)last;
		}
		else if (DocGen.PdfViewer.Base.NP.IsNPOperator(lastAs))
		{
			interpreter.NP = (PostScriptArray)last;
		}
	}
}
