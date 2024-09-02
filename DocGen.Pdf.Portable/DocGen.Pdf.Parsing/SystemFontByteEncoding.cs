namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontByteEncoding
{
	private readonly SystemFontRange range;

	public static SystemFontByteEncodingCollection DictByteEncodings { get; private set; }

	public static SystemFontByteEncodingCollection CharStringByteEncodings { get; private set; }

	private static void InitializeDictByteEncodings()
	{
		DictByteEncodings = new SystemFontByteEncodingCollection();
		DictByteEncodings.Add(new SystemFontSingleByteIntegerEncoding());
		DictByteEncodings.Add(new SystemFontTwoByteIntegerEncodingType1());
		DictByteEncodings.Add(new SystemFontTwoByteIntegerEncodingType2());
		DictByteEncodings.Add(new SystemFontThreeByteIntegerEncoding());
		DictByteEncodings.Add(new SystemFontFiveByteIntegerEncoding());
		DictByteEncodings.Add(new SystemFontRealByteEncoding());
	}

	private static void InitializeCharStringByteEncodings()
	{
		CharStringByteEncodings = new SystemFontByteEncodingCollection();
		CharStringByteEncodings.Add(new SystemFontThreeByteIntegerEncoding());
		CharStringByteEncodings.Add(new SystemFontSingleByteIntegerEncoding());
		CharStringByteEncodings.Add(new SystemFontTwoByteIntegerEncodingType1());
		CharStringByteEncodings.Add(new SystemFontTwoByteIntegerEncodingType2());
		CharStringByteEncodings.Add(new SystemFontFiveByteFixedEncoding());
	}

	static SystemFontByteEncoding()
	{
		InitializeDictByteEncodings();
		InitializeCharStringByteEncodings();
	}

	public SystemFontByteEncoding(byte start, byte end)
	{
		range = new SystemFontRange(start, end);
	}

	public abstract object Read(SystemFontEncodedDataReader reader);

	public bool IsInRange(byte b0)
	{
		return range.IsInRange(b0);
	}
}
