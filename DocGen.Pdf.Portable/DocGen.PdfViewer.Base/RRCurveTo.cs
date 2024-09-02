namespace DocGen.PdfViewer.Base;

internal class RRCurveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		OperandCollector operands = interpreter.Operands;
		while (operands.Count / 6 > 0)
		{
			int firstAsInt = operands.GetFirstAsInt();
			int firstAsInt2 = operands.GetFirstAsInt();
			int firstAsInt3 = operands.GetFirstAsInt();
			int firstAsInt4 = operands.GetFirstAsInt();
			int firstAsInt5 = operands.GetFirstAsInt();
			int firstAsInt6 = operands.GetFirstAsInt();
			Operator.CurveTo(interpreter, firstAsInt, firstAsInt2, firstAsInt3, firstAsInt4, firstAsInt5, firstAsInt6);
		}
		operands.Clear();
	}
}
