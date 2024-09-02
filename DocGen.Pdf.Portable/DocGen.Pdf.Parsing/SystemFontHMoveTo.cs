namespace DocGen.Pdf.Parsing;

internal class SystemFontHMoveTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		SystemFontOperator.ReadWidth(interpreter, 1);
		int firstAsInt = interpreter.Operands.GetFirstAsInt();
		SystemFontOperator.MoveTo(interpreter, firstAsInt, 0);
		interpreter.Operands.Clear();
	}
}
