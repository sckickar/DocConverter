namespace DocGen.Pdf.Parsing;

internal class SystemFontHintOperator : SystemFontOperator
{
	private static void ReadWidth(SystemFontBuildChar interpreter)
	{
		if (!interpreter.Width.HasValue)
		{
			if (interpreter.Operands.Count % 2 == 1)
			{
				interpreter.Width = interpreter.Operands.GetFirstAsInt();
			}
			else
			{
				interpreter.Width = 0;
			}
		}
	}

	public override void Execute(SystemFontBuildChar interpreter)
	{
		ReadWidth(interpreter);
		interpreter.Operands.Clear();
	}

	internal void Execute(SystemFontBuildChar interpreter, out int count)
	{
		count = interpreter.Operands.Count / 2;
		Execute(interpreter);
	}
}
