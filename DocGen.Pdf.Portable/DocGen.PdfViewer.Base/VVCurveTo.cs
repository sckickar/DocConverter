namespace DocGen.PdfViewer.Base;

internal class VVCurveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		OperandCollector operands = interpreter.Operands;
		int dxa = 0;
		if (operands.Count % 2 != 0)
		{
			dxa = operands.GetFirstAsInt();
		}
		int firstAsInt = operands.GetFirstAsInt();
		int firstAsInt2 = operands.GetFirstAsInt();
		int firstAsInt3 = operands.GetFirstAsInt();
		int firstAsInt4 = operands.GetFirstAsInt();
		Operator.CurveTo(interpreter, dxa, firstAsInt, firstAsInt2, firstAsInt3, 0, firstAsInt4);
		while (operands.Count / 4 > 0)
		{
			firstAsInt = operands.GetFirstAsInt();
			firstAsInt2 = operands.GetFirstAsInt();
			firstAsInt3 = operands.GetFirstAsInt();
			firstAsInt4 = operands.GetFirstAsInt();
			Operator.CurveTo(interpreter, 0, firstAsInt, firstAsInt2, firstAsInt3, 0, firstAsInt4);
		}
		operands.Clear();
	}
}
