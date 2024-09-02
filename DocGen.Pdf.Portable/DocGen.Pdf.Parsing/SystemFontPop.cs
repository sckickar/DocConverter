namespace DocGen.Pdf.Parsing;

internal class SystemFontPop : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		buildChar.Operands.AddLast(buildChar.PostScriptStack.GetLast());
	}
}
