namespace DocGen.PdfViewer.Base;

internal class HFlex : Operator
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
		buildChar.Operands.Clear();
		Operator.CurveTo(buildChar, firstAsInt, 0, firstAsInt2, firstAsInt3, firstAsInt4, 0);
		Operator.CurveTo(buildChar, firstAsInt5, 0, firstAsInt6, 0, firstAsInt7, 0);
	}
}
