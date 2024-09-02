namespace DocGen.Pdf.Parsing;

internal class SystemFontCallSubr : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		interpreter.ExecuteSubr(interpreter.Operands.GetLastAsInt());
	}
}
