namespace DocGen.PdfViewer.Base;

internal abstract class ByteEncodingBase
{
	private readonly PdfRangeCalculator range;

	public static ByteEncodingCollectionBase DictByteEncodings { get; private set; }

	public static ByteEncodingCollectionBase CharStringByteEncodings { get; private set; }

	private static void InitializeDictByteEncodings()
	{
		DictByteEncodings = new ByteEncodingCollectionBase();
		DictByteEncodings.Add(new SingleByteIntegerEncodingBase());
		DictByteEncodings.Add(new TwoByteIntegerEncodingType1Base());
		DictByteEncodings.Add(new TwoByteIntegerEncodingType2Base());
		DictByteEncodings.Add(new ThreeByteIntegerEncodingBase());
		DictByteEncodings.Add(new FiveByteIntegerEncodingBase());
		DictByteEncodings.Add(new RealByteEncodingBase());
	}

	private static void InitializeCharStringByteEncodings()
	{
		CharStringByteEncodings = new ByteEncodingCollectionBase();
		CharStringByteEncodings.Add(new ThreeByteIntegerEncodingBase());
		CharStringByteEncodings.Add(new SingleByteIntegerEncodingBase());
		CharStringByteEncodings.Add(new TwoByteIntegerEncodingType1Base());
		CharStringByteEncodings.Add(new TwoByteIntegerEncodingType2Base());
		CharStringByteEncodings.Add(new FiveByteFixedEncodingBase());
	}

	static ByteEncodingBase()
	{
		InitializeDictByteEncodings();
		InitializeCharStringByteEncodings();
	}

	public ByteEncodingBase(byte start, byte end)
	{
		range = new PdfRangeCalculator(start, end);
	}

	public abstract object Read(EncodedDataParser reader);

	public bool IsInRange(byte b0)
	{
		return range.IsInRange(b0);
	}
}
