namespace DocGen.PdfViewer.Base;

internal class Seac : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		int lastAsInt = buildChar.Operands.GetLastAsInt();
		int lastAsInt2 = buildChar.Operands.GetLastAsInt();
		int lastAsInt3 = buildChar.Operands.GetLastAsInt();
		int lastAsInt4 = buildChar.Operands.GetLastAsInt();
		buildChar.Operands.GetLastAsInt();
		string accentedChar = PresettedEncodings.StandardEncoding[lastAsInt];
		string baseChar = PresettedEncodings.StandardEncoding[lastAsInt2];
		buildChar.CombineChars(accentedChar, baseChar, lastAsInt4, lastAsInt3);
		buildChar.Operands.Clear();
	}
}
