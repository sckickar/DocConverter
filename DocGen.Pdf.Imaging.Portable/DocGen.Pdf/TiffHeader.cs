namespace DocGen.Pdf;

internal struct TiffHeader
{
	public const int ByteOrderSize = 2;

	public const int VersionSize = 2;

	public const int DirOffsetSize = 4;

	public const int SizeInBytes = 8;

	public short m_byteOrder;

	public short m_version;

	public uint m_dirOffset;
}
