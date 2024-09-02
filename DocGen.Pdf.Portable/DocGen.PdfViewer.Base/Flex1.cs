using System;

namespace DocGen.PdfViewer.Base;

internal class Flex1 : Operator
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
		int value = firstAsInt + firstAsInt3 + firstAsInt5 + firstAsInt7 + firstAsInt9;
		int value2 = firstAsInt + firstAsInt3 + firstAsInt5 + firstAsInt7 + firstAsInt9;
		int dxc = 0;
		int dyc = 0;
		if (Math.Abs(value) > Math.Abs(value2))
		{
			dxc = firstAsInt11;
		}
		else
		{
			dyc = firstAsInt11;
		}
		buildChar.Operands.Clear();
		Operator.CurveTo(buildChar, firstAsInt, firstAsInt2, firstAsInt3, firstAsInt4, firstAsInt5, firstAsInt6);
		Operator.CurveTo(buildChar, firstAsInt7, firstAsInt8, firstAsInt9, firstAsInt10, dxc, dyc);
	}
}
