namespace DocGen.Pdf;

internal abstract class EntropyDecoder : MultiResImgDataAdapter, CBlkQuantDataSrcDec, InvWTData, MultiResImgData
{
	public const char OPT_PREFIX = 'C';

	private static readonly string[][] pinfo = new string[2][]
	{
		new string[4] { "Cverber", "[on|off]", "Specifies if the entropy decoder should be verbose about detected errors. If 'on' a message is printed whenever an error is detected.", "on" },
		new string[4] { "Cer", "[on|off]", "Specifies if error detection should be performed by the entropy decoder engine. If errors are detected they will be concealed and the resulting distortion will be less important. Note that errors can only be detected if the encoder that generated the data included error resilience information.", "on" }
	};

	internal CodedCBlkDataSrcDec src;

	public virtual int CbULX => src.CbULX;

	public virtual int CbULY => src.CbULY;

	public static string[][] ParameterInfo => pinfo;

	internal EntropyDecoder(CodedCBlkDataSrcDec src)
		: base(src)
	{
		this.src = src;
	}

	public override SubbandSyn getSynSubbandTree(int t, int c)
	{
		return src.getSynSubbandTree(t, c);
	}

	public abstract DataBlock getCodeBlock(int param1, int param2, int param3, SubbandSyn param4, DataBlock param5);

	public abstract DataBlock getInternCodeBlock(int param1, int param2, int param3, SubbandSyn param4, DataBlock param5);
}
