namespace DocGen.Pdf;

internal class TiffDirectoryEntry
{
	public const int SizeInBytes = 12;

	public TiffTag DirectoryTag;

	public TiffType DirectoryType;

	public int DirectoryCount;

	public uint DirectoryOffset;
}
