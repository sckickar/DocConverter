namespace DocGen.Pdf.Parsing;

internal class SystemFontEndChar : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		if (buildChar.Operands.Count >= 4)
		{
			int lastAsInt = buildChar.Operands.GetLastAsInt();
			int lastAsInt2 = buildChar.Operands.GetLastAsInt();
			int lastAsInt3 = buildChar.Operands.GetLastAsInt();
			int lastAsInt4 = buildChar.Operands.GetLastAsInt();
			string accentedChar = SystemFontPredefinedEncodings.StandardEncoding[lastAsInt];
			string baseChar = SystemFontPredefinedEncodings.StandardEncoding[lastAsInt2];
			buildChar.CombineChars(accentedChar, baseChar, lastAsInt4, lastAsInt3);
		}
		SystemFontOperator.ReadWidth(buildChar, 0);
		buildChar.Operands.Clear();
	}
}
