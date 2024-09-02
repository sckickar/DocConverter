using System;

namespace DocGen.Pdf;

internal abstract class ForwardWT : ImgDataAdapter, ForwWT, WaveletTransform, ImageData, ForwWTDataProps, CBlkWTDataSrc
{
	public const int WT_DECOMP_DYADIC = 0;

	public const char OPT_PREFIX = 'W';

	private static readonly string[][] pinfo = new string[3][]
	{
		new string[4] { "Wlev", "<number of decomposition levels>", "Specifies the number of wavelet decomposition levels to apply to the image. If 0 no wavelet transform is performed. All components and all tiles have the same number of decomposition levels.", "5" },
		new string[4] { "Wwt", "[full]", "Specifies the wavelet transform to be used. Possible value is: 'full' (full page). The value 'full' performs a normal DWT.", "full" },
		new string[4] { "Wcboff", "<x y>", "Code-blocks partition offset in the reference grid. Allowed for <x> and <y> are 0 and 1.\nNote: This option is defined in JPEG 2000 part 2 and may not be supported by all JPEG 2000 decoders.", "0 0" }
	};

	public static string[][] ParameterInfo => pinfo;

	public abstract int CbULY { get; }

	public abstract int CbULX { get; }

	internal ForwardWT(ImageData src)
		: base(src)
	{
	}

	internal static ForwardWT createInstance(BlockImageDataSource src, JPXParameters pl, EncoderSpecs encSpec)
	{
		pl.checkList('W', JPXParameters.toNameArray(pinfo));
		_ = (int)encSpec.dls.getDefault();
		string text = "";
		pl.getParameter("Wcboff");
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(pl.getParameter("Wcboff"));
		if (tokenizer.Count != 2)
		{
			throw new ArgumentException("'-Wcboff' option needs two arguments. See usage with the '-u' option.");
		}
		int num = 0;
		text = tokenizer.NextToken();
		try
		{
			num = int.Parse(text);
		}
		catch (FormatException)
		{
			throw new ArgumentException("Bad first parameter for the '-Wcboff' option: " + text);
		}
		if (num < 0 || num > 1)
		{
			throw new ArgumentException("Invalid horizontal code-block partition origin.");
		}
		int num2 = 0;
		text = tokenizer.NextToken();
		try
		{
			num2 = int.Parse(text);
		}
		catch (FormatException)
		{
			throw new ArgumentException("Bad second parameter for the '-Wcboff' option: " + text);
		}
		if (num2 < 0 || num2 > 1)
		{
			throw new ArgumentException("Invalid vertical code-block partition origin.");
		}
		if (num == 0)
		{
		}
		return new ForwWTFull(src, encSpec, num, num2);
	}

	public abstract bool isReversible(int param1, int param2);

	public abstract CBlkWTData getNextInternCodeBlock(int param1, CBlkWTData param2);

	public abstract int getFixedPoint(int param1);

	public abstract AnWTFilter[] getHorAnWaveletFilters(int param1, int param2);

	public abstract AnWTFilter[] getVertAnWaveletFilters(int param1, int param2);

	public abstract SubbandAn getAnSubbandTree(int param1, int param2);

	public abstract int getDecompLevels(int param1, int param2);

	public abstract CBlkWTData getNextCodeBlock(int param1, CBlkWTData param2);

	public abstract int getImplementationType(int param1);

	public abstract int getDataType(int param1, int param2);

	public abstract int getDecomp(int param1, int param2);
}
