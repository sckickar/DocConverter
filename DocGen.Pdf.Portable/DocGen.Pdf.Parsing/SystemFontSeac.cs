namespace DocGen.Pdf.Parsing;

internal class SystemFontSeac : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		int lastAsInt = buildChar.Operands.GetLastAsInt();
		int lastAsInt2 = buildChar.Operands.GetLastAsInt();
		int lastAsInt3 = buildChar.Operands.GetLastAsInt();
		int lastAsInt4 = buildChar.Operands.GetLastAsInt();
		buildChar.Operands.GetLastAsInt();
		string accentedChar = SystemFontPredefinedEncodings.StandardEncoding[lastAsInt];
		string baseChar = SystemFontPredefinedEncodings.StandardEncoding[lastAsInt2];
		buildChar.CombineChars(accentedChar, baseChar, lastAsInt4, lastAsInt3);
		buildChar.Operands.Clear();
	}
}
