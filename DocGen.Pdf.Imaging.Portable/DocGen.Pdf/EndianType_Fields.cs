using System.Runtime.InteropServices;

namespace DocGen.Pdf;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct EndianType_Fields
{
	public const int BIG_ENDIAN = 0;

	public const int LITTLE_ENDIAN = 1;
}
