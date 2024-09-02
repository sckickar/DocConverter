namespace BitMiracle.LibTiff.Classic;

public static class FieldBit
{
	internal const int SetLongs = 4;

	internal const short ImageDimensions = 1;

	internal const short TileDimensions = 2;

	internal const short Resolution = 3;

	internal const short Position = 4;

	internal const short SubFileType = 5;

	internal const short BitsPerSample = 6;

	internal const short Compression = 7;

	internal const short Photometric = 8;

	internal const short Thresholding = 9;

	internal const short FillOrder = 10;

	internal const short Orientation = 15;

	internal const short SamplesPerPixel = 16;

	internal const short RowsPerStrip = 17;

	internal const short MinSampleValue = 18;

	internal const short MaxSampleValue = 19;

	internal const short PlanarConfig = 20;

	internal const short ResolutionUnit = 22;

	internal const short PageNumber = 23;

	internal const short StripByteCounts = 24;

	internal const short StripOffsets = 25;

	internal const short ColorMap = 26;

	internal const short ExtraSamples = 31;

	internal const short SampleFormat = 32;

	internal const short SMinSampleValue = 33;

	internal const short SMaxSampleValue = 34;

	internal const short ImageDepth = 35;

	internal const short TileDepth = 36;

	internal const short HalftoneHints = 37;

	internal const short YCbCrSubsampling = 39;

	internal const short YCbCrPositioning = 40;

	internal const short RefBlackWhite = 41;

	internal const short TransferFunction = 44;

	internal const short InkNames = 46;

	internal const short SubIFD = 49;

	public const short Ignore = 0;

	public const short Pseudo = 0;

	public const short Custom = 65;

	public const short Codec = 66;

	public const short Last = 127;
}
