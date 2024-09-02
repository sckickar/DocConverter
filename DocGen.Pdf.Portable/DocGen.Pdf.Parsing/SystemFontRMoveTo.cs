namespace DocGen.Pdf.Parsing;

internal class SystemFontRMoveTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		SystemFontOperator.ReadWidth(interpreter, 2);
		int firstAsInt = interpreter.Operands.GetFirstAsInt();
		int firstAsInt2 = interpreter.Operands.GetFirstAsInt();
		SystemFontOperator.MoveTo(interpreter, firstAsInt, firstAsInt2);
		interpreter.Operands.Clear();
	}
}
