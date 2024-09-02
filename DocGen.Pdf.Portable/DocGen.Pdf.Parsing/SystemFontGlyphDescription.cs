namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyphDescription
{
	internal const int ARG_1_AND_2_ARE_WORDS = 0;

	internal const int ARGS_ARE_XY_VALUES = 1;

	internal const int ROUND_XY_TO_GRID = 2;

	internal const int WE_HAVE_A_SCALE = 3;

	internal const int MORE_COMPONENTS = 5;

	internal const int WE_HAVE_AN_X_AND_Y_SCALE = 6;

	internal const int WE_HAVE_A_TWO_BY_TWO = 7;

	internal const int WE_HAVE_INSTRUCTIONS = 8;

	internal const int USE_MY_METRICS = 9;

	internal const int OVERLAP_COMPOUND = 10;

	public ushort Flags { get; private set; }

	public ushort GlyphIndex { get; private set; }

	public SystemFontMatrix Transform { get; private set; }

	internal bool CheckFlag(byte bit)
	{
		return SystemFontBitsHelper.GetBit(Flags, bit);
	}

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		Flags = reader.ReadUShort();
		GlyphIndex = reader.ReadUShort();
		int num = 0;
		int num2 = 0;
		if (CheckFlag(0))
		{
			if (CheckFlag(1))
			{
				num = reader.ReadShort();
				num2 = reader.ReadShort();
			}
			else
			{
				reader.ReadUShort();
				reader.ReadUShort();
			}
		}
		else if (CheckFlag(1))
		{
			num = reader.ReadChar();
			num2 = reader.ReadChar();
		}
		else
		{
			reader.ReadChar();
			reader.ReadChar();
		}
		float num3 = 1f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 1f;
		if (CheckFlag(3))
		{
			num6 = (num3 = reader.Read2Dot14());
		}
		else if (CheckFlag(6))
		{
			num3 = reader.Read2Dot14();
			num6 = reader.Read2Dot14();
		}
		else if (CheckFlag(7))
		{
			num3 = reader.Read2Dot14();
			num4 = reader.Read2Dot14();
			num5 = reader.Read2Dot14();
			num6 = reader.Read2Dot14();
		}
		Transform = new SystemFontMatrix(num3, num4, num5, num6, num, num2);
	}
}
