namespace DocGen.Pdf.Parsing;

internal class SystemFontCallGSubr : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		interpreter.ExecuteGlobalSubr(interpreter.Operands.GetLastAsInt());
	}
}
