using System.Runtime.InteropServices;

namespace DocGen.Pdf;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ProgressionType
{
	public const int LY_RES_COMP_POS_PROG = 0;

	public const int RES_LY_COMP_POS_PROG = 1;

	public const int RES_POS_COMP_LY_PROG = 2;

	public const int POS_COMP_RES_LY_PROG = 3;

	public const int COMP_POS_RES_LY_PROG = 4;
}
