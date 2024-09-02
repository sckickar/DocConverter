using System.Runtime.InteropServices;

namespace DocGen.Pdf;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct Markers
{
	public const short SOC = -177;

	public const short SOT = -112;

	public const short SOD = -109;

	public const short EOC = -39;

	public const short SIZ = -175;

	public const int RSIZ_BASELINE = 0;

	public const int RSIZ_ER_FLAG = 1;

	public const int RSIZ_ROI = 2;

	public const int SSIZ_DEPTH_BITS = 7;

	public const int MAX_COMP_BITDEPTH = 38;

	public const short COD = -174;

	public const short COC = -173;

	public const int SCOX_PRECINCT_PARTITION = 1;

	public const int SCOX_USE_SOP = 2;

	public const int SCOX_USE_EPH = 4;

	public const int SCOX_HOR_CB_PART = 8;

	public const int SCOX_VER_CB_PART = 16;

	public const int PRECINCT_PARTITION_DEF_SIZE = 65535;

	public const short RGN = -162;

	public const int SRGN_IMPLICIT = 0;

	public const short QCD = -164;

	public const short QCC = -163;

	public const int SQCX_GB_SHIFT = 5;

	public const int SQCX_GB_MSK = 7;

	public const int SQCX_NO_QUANTIZATION = 0;

	public const int SQCX_SCALAR_DERIVED = 1;

	public const int SQCX_SCALAR_EXPOUNDED = 2;

	public const int SQCX_EXP_SHIFT = 3;

	public const int SQCX_EXP_MASK = 31;

	public const int ERS_SOP = 1;

	public const int ERS_SEG_SYMBOLS = 2;

	public const short POC = -161;

	public const short TLM = -171;

	public const short PLM = -169;

	public const short PLT = -168;

	public const short PPM = -160;

	public const short PPT = -159;

	public const int MAX_LPPT = 65535;

	public const int MAX_LPPM = 65535;

	public const short SOP = -111;

	public const short SOP_LENGTH = 6;

	public const short EPH = -110;

	public const short EPH_LENGTH = 2;

	public const short CRG = -157;

	public const short COM = -156;

	public const short RCOM_GEN_USE = 1;
}
