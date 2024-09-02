namespace DocGen.PdfViewer.Base;

internal class HHCurveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		OperandCollector operands = interpreter.Operands;
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
			Operator.CurveTo(interpreter, firstAsInt, dya, firstAsInt2, firstAsInt3, firstAsInt4, 0);
			dya = 0;
		}
		operands.Clear();
	}
}
