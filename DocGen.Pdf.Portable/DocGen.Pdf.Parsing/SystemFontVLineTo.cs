namespace DocGen.Pdf.Parsing;

internal class SystemFontVLineTo : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		if (interpreter.Operands.Count % 2 == 0)
		{
			while (interpreter.Operands.Count > 0)
			{
				int firstAsInt = interpreter.Operands.GetFirstAsInt();
				int firstAsInt2 = interpreter.Operands.GetFirstAsInt();
				SystemFontOperator.VLineTo(interpreter, firstAsInt);
				SystemFontOperator.HLineTo(interpreter, firstAsInt2);
			}
		}
		else
		{
			int firstAsInt3 = interpreter.Operands.GetFirstAsInt();
			SystemFontOperator.VLineTo(interpreter, firstAsInt3);
			while (interpreter.Operands.Count > 0)
			{
				int firstAsInt4 = interpreter.Operands.GetFirstAsInt();
				int firstAsInt5 = interpreter.Operands.GetFirstAsInt();
				SystemFontOperator.HLineTo(interpreter, firstAsInt4);
				SystemFontOperator.VLineTo(interpreter, firstAsInt5);
			}
		}
		interpreter.Operands.Clear();
	}
}
