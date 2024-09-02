namespace DocGen.PdfViewer.Base;

internal class SystemDict : PostScriptOperators
{
	public override void Execute(FontInterpreter interpteret)
	{
		interpteret.Operands.AddLast(interpteret.SystemDict);
	}
}
