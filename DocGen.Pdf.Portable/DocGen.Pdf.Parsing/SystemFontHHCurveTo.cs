namespace DocGen.Pdf.Parsing;

internal class SystemFontHHCurveTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		SystemFontOperandsCollection operands = interpreter.Operands;
		int dya = 0;
		if (operands.Count % 2 != 0)
		{
			dya = operands.GetFirstAsInt();
		}
		while (operands.Count / 4 > 0)
		{
			int firstAsInt = operands.GetFirstAsInt();
			int firstAsInt2 = operands.GetFirstAsInt();
			int firstAsInt3 = operands.GetFirstAsInt();
			int firstAsInt4 = operands.GetFirstAsInt();
			SystemFontOperator.CurveTo(interpreter, firstAsInt, dya, firstAsInt2, firstAsInt3, firstAsInt4, 0);
			dya = 0;
		}
		operands.Clear();
	}
}
