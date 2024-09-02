using System.Globalization;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class TiffDirEntry
{
	public TiffTag tdir_tag;

	public TiffType tdir_type;

	public int tdir_count;

	public ulong tdir_offset;

	public new string ToString()
	{
		return tdir_tag.ToString() + ", " + tdir_type.ToString() + " " + tdir_offset.ToString(CultureInfo.InvariantCulture);
	}

	public static int SizeInBytes(bool isBigTiff)
	{
		if (isBigTiff)
		{
			return 20;
		}
		return 12;
	}
}
