namespace DocGen.PdfViewer.Base;

internal class Known : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		string lastAs = interpreter.Operands.GetLastAs<string>();
		PostScriptDict lastAs2 = interpreter.Operands.GetLastAs<PostScriptDict>();
		interpreter.Operands.AddLast(lastAs2.ContainsKey(lastAs));
	}
}
