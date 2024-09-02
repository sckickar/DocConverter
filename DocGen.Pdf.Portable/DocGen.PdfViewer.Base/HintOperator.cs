namespace DocGen.PdfViewer.Base;

internal class HintOperator : Operator
{
	private static void ReadWidth(CharacterBuilder interpreter)
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

	public override void Execute(CharacterBuilder interpreter)
	{
		ReadWidth(interpreter);
		interpreter.Operands.Clear();
	}

	internal void Execute(CharacterBuilder interpreter, out int count)
	{
		count = interpreter.Operands.Count / 2;
		Execute(interpreter);
	}
}
