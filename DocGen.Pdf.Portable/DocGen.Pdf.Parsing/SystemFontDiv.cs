namespace DocGen.Pdf.Parsing;

internal class SystemFontDiv : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		double lastAsReal = buildChar.Operands.GetLastAsReal();
		double num = buildChar.Operands.GetFirstAsInt();
		buildChar.Operands.AddLast(num / lastAsReal);
	}
}
