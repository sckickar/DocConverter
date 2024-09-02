namespace DocGen.Pdf.Parsing;

internal class SystemFontVMoveTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		SystemFontOperator.ReadWidth(interpreter, 1);
		int firstAsInt = interpreter.Operands.GetFirstAsInt();
		SystemFontOperator.MoveTo(interpreter, 0, firstAsInt);
		interpreter.Operands.Clear();
	}
}
