namespace DocGen.PdfViewer.Base;

internal class Flex : Operator
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
		int firstAsInt10 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt11 = buildChar.Operands.GetFirstAsInt();
		int firstAsInt12 = buildChar.Operands.GetFirstAsInt();
		buildChar.Operands.Clear();
		Operator.CurveTo(buildChar, firstAsInt, firstAsInt2, firstAsInt3, firstAsInt4, firstAsInt5, firstAsInt6);
		Operator.CurveTo(buildChar, firstAsInt7, firstAsInt8, firstAsInt9, firstAsInt10, firstAsInt11, firstAsInt12);
	}
}
