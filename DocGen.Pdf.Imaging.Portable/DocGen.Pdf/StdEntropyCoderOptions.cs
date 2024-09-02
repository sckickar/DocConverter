using System.Runtime.InteropServices;

namespace DocGen.Pdf;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct StdEntropyCoderOptions
{
	public static readonly int OPT_BYPASS;

	public static readonly int OPT_RESET_MQ;

	public static readonly int OPT_TERM_PASS;

	public static readonly int OPT_VERT_STR_CAUSAL;

	public static readonly int OPT_PRED_TERM;

	public static readonly int OPT_SEG_SYMBOLS;

	public static readonly int MIN_CB_DIM;

	public static readonly int MAX_CB_DIM;

	public static readonly int MAX_CB_AREA;

	public static readonly int STRIPE_HEIGHT;

	public static readonly int NUM_PASSES;

	public static readonly int NUM_NON_BYPASS_MS_BP;

	public static readonly int NUM_EMPTY_PASSES_IN_MS_BP;

	public static readonly int FIRST_BYPASS_PASS_IDX;

	static StdEntropyCoderOptions()
	{
		OPT_BYPASS = 1;
		OPT_RESET_MQ = 2;
		OPT_TERM_PASS = 4;
		OPT_VERT_STR_CAUSAL = 8;
		OPT_PRED_TERM = 16;
		OPT_SEG_SYMBOLS = 32;
		MIN_CB_DIM = 4;
		MAX_CB_DIM = 1024;
		MAX_CB_AREA = 4096;
		STRIPE_HEIGHT = 4;
		NUM_PASSES = 3;
		NUM_NON_BYPASS_MS_BP = 4;
		NUM_EMPTY_PASSES_IN_MS_BP = 2;
		FIRST_BYPASS_PASS_IDX = NUM_PASSES * NUM_NON_BYPASS_MS_BP - NUM_EMPTY_PASSES_IN_MS_BP;
	}
}
