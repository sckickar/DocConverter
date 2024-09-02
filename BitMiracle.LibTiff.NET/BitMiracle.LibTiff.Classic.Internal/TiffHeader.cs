namespace BitMiracle.LibTiff.Classic.Internal;

internal struct TiffHeader
{
	public const int TIFF_MAGIC_SIZE = 2;

	public const int TIFF_VERSION_SIZE = 2;

	public const int TIFF_DIROFFSET_SIZE = 4;

	public short tiff_magic;

	public short tiff_version;

	public ulong tiff_diroff;

	public short tiff_offsize;

	public short tiff_fill;

	public static int SizeInBytes(bool isBigTiff)
	{
		if (isBigTiff)
		{
			return 16;
		}
		return 8;
	}
}
