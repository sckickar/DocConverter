namespace DocGen.Pdf.Parsing;

internal class SystemFontHVCurveTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		SystemFontOperandsCollection operands = interpreter.Operands;
		if (operands.Count % 8 != 0 && operands.Count % 8 != 1)
		{
			int firstAsInt = operands.GetFirstAsInt();
			int firstAsInt2 = operands.GetFirstAsInt();
			int firstAsInt3 = operands.GetFirstAsInt();
			int firstAsInt4 = operands.GetFirstAsInt();
			int dxc = ((operands.Count == 1) ? operands.GetFirstAsInt() : 0);
			SystemFontOperator.CurveTo(interpreter, firstAsInt, 0, firstAsInt2, firstAsInt3, dxc, firstAsInt4);
			while (operands.Count / 8 > 0)
			{
				int firstAsInt5 = operands.GetFirstAsInt();
				int firstAsInt6 = operands.GetFirstAsInt();
				int firstAsInt7 = operands.GetFirstAsInt();
				int firstAsInt8 = operands.GetFirstAsInt();
				int firstAsInt9 = operands.GetFirstAsInt();
				int firstAsInt10 = operands.GetFirstAsInt();
				int firstAsInt11 = operands.GetFirstAsInt();
				int firstAsInt12 = operands.GetFirstAsInt();
				dxc = ((operands.Count == 1) ? operands.GetFirstAsInt() : 0);
				SystemFontOperator.CurveTo(interpreter, 0, firstAsInt5, firstAsInt6, firstAsInt7, firstAsInt8, 0);
				SystemFontOperator.CurveTo(interpreter, firstAsInt9, 0, firstAsInt10, firstAsInt11, dxc, firstAsInt12);
			}
			operands.Clear();
		}
		else
		{
			while (operands.Count / 8 > 0)
			{
				int firstAsInt13 = operands.GetFirstAsInt();
				int firstAsInt14 = operands.GetFirstAsInt();
				int firstAsInt15 = operands.GetFirstAsInt();
				int firstAsInt16 = operands.GetFirstAsInt();
				int firstAsInt17 = operands.GetFirstAsInt();
				int firstAsInt18 = operands.GetFirstAsInt();
				int firstAsInt19 = operands.GetFirstAsInt();
				int firstAsInt20 = operands.GetFirstAsInt();
				int dyc = ((operands.Count == 1) ? operands.GetFirstAsInt() : 0);
				SystemFontOperator.CurveTo(interpreter, firstAsInt13, 0, firstAsInt14, firstAsInt15, 0, firstAsInt16);
				SystemFontOperator.CurveTo(interpreter, 0, firstAsInt17, firstAsInt18, firstAsInt19, firstAsInt20, dyc);
			}
			operands.Clear();
		}
	}
}
