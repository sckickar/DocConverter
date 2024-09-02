namespace DocGen.PdfViewer.Base;

internal class HFlex1 : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		int firstAsInt = buildChar.Operands.GetFirstAsInt();
		int firstAsInt2 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt3 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt4 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt5 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt6 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt7 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt8 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt9 = buildChar.Operands.GetFirstAsInt();
		buildChar.Operands.Clear();
		Operator.CurveTo(buildChar, firstAsInt, firstAsInt2, firstAsInt3, firstAsInt4, firstAsInt5, 0);
		Operator.CurveTo(buildChar, firstAsInt6, 0, firstAsInt7, firstAsInt8, firstAsInt9, 0);
	}
}
