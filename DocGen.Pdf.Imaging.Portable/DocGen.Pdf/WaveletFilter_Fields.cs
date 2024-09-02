using System.Runtime.InteropServices;

namespace DocGen.Pdf;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct WaveletFilter_Fields
{
	public static readonly int WT_FILTER_INT_LIFT = 0;

	public static readonly int WT_FILTER_FLOAT_LIFT = 1;

	public static readonly int WT_FILTER_FLOAT_CONVOL = 2;
}
