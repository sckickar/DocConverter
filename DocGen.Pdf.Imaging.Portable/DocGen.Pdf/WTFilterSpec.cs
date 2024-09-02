namespace DocGen.Pdf;

internal abstract class WTFilterSpec
{
	public const byte FILTER_SPEC_MAIN_DEF = 0;

	public const byte FILTER_SPEC_COMP_DEF = 1;

	public const byte FILTER_SPEC_TILE_DEF = 2;

	public const byte FILTER_SPEC_TILE_COMP = 3;

	internal byte[] specValType;

	public abstract int WTDataType { get; }

	internal WTFilterSpec(int nc)
	{
		specValType = new byte[nc];
	}

	public virtual byte getKerSpecType(int n)
	{
		return specValType[n];
	}
}
