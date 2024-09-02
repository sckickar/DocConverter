namespace DocGen.PdfViewer.Base;

internal class VHCurveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		OperandCollector operands = interpreter.Operands;
		if (operands.Count % 8 != 0 && operands.Count % 8 != 1)
		{
			int firstAsInt = operands.GetFirstAsInt();
			int firstAsInt2 = operands.GetFirstAsInt();
			int firstAsInt3 = operands.GetFirstAsInt();
			int firstAsInt4 = operands.GetFirstAsInt();
			int dyc = ((operands.Count == 1) ? operands.GetFirstAsInt() : 0);
			Operator.CurveTo(interpreter, 0, firstAsInt, firstAsInt2, firstAsInt3, firstAsInt4, dyc);
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
				dyc = ((operands.Count == 1) ? operands.GetFirstAsInt() : 0);
				Operator.CurveTo(interpreter, firstAsInt5, 0, firstAsInt6, firstAsInt7, 0, firstAsInt8);
				Operator.CurveTo(interpreter, 0, firstAsInt9, firstAsInt10, firstAsInt11, firstAsInt12, dyc);
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
				int dxc = ((operands.Count == 1) ? operands.GetFirstAsInt() : 0);
				Operator.CurveTo(interpreter, 0, firstAsInt13, firstAsInt14, firstAsInt15, firstAsInt16, 0);
				Operator.CurveTo(interpreter, firstAsInt17, 0, firstAsInt18, firstAsInt19, dxc, firstAsInt20);
			}
			operands.Clear();
		}
	}
}
