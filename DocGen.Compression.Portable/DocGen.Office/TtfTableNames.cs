using System.Runtime.InteropServices;

namespace DocGen.Office;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct TtfTableNames
{
	public const string cmap = "cmap";

	public const string glyf = "glyf";

	public const string head = "head";

	public const string hhea = "hhea";

	public const string hmtx = "hmtx";

	public const string loca = "loca";

	public const string maxp = "maxp";

	public const string name = "name";

	public const string post = "post";

	public const string OS2 = "OS/2";

	public const string CFF = "CFF ";

	public const string cvt = "cvt ";

	public const string fpgm = "fpgm";

	public const string prep = "prep";
}
