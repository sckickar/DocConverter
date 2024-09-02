namespace DocGen.Pdf.Parsing;

internal class SystemFontRLineTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		while (interpreter.Operands.Count / 2 > 0)
		{
			int firstAsInt = interpreter.Operands.GetFirstAsInt();
			int firstAsInt2 = interpreter.Operands.GetFirstAsInt();
			SystemFontOperator.LineTo(interpreter, firstAsInt, firstAsInt2);
		}
		interpreter.Operands.Clear();
	}
}
