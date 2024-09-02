namespace DocGen.PdfViewer.Base;

internal class FontDirectory : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.AddLast(interpreter.SystemDict["FontDirectory"]);
	}
}
