namespace BitMiracle.LibTiff.Classic.Internal;

internal class TiffDirectory
{
	public int[] td_fieldsset = new int[4];

	public int td_imagewidth;

	public int td_imagelength;

	public int td_imagedepth;

	public int td_tilewidth;

	public int td_tilelength;

	public int td_tiledepth;

	public FileType td_subfiletype;

	public short td_bitspersample;

	public SampleFormat td_sampleformat;

	public Compression td_compression;

	public Photometric td_photometric;

	public Threshold td_threshholding;

	public FillOrder td_fillorder;

	public Orientation td_orientation;

	public short td_samplesperpixel;

	public int td_rowsperstrip;

	public ushort td_minsamplevalue;

	public ushort td_maxsamplevalue;

	public double td_sminsamplevalue;

	public double td_smaxsamplevalue;

	public float td_xresolution;

	public float td_yresolution;

	public ResUnit td_resolutionunit;

	public PlanarConfig td_planarconfig;

	public float td_xposition;

	public float td_yposition;

	public short[] td_pagenumber = new short[2];

	public short[][] td_colormap = new short[3][];

	public short[] td_halftonehints = new short[2];

	public short td_extrasamples;

	public ExtraSample[] td_sampleinfo;

	public int td_stripsperimage;

	public int td_nstrips;

	public ulong[] td_stripoffset;

	public ulong[] td_stripbytecount;

	public bool td_stripbytecountsorted;

	public int td_nsubifd;

	public long[] td_subifd;

	public short[] td_ycbcrsubsampling = new short[2];

	public YCbCrPosition td_ycbcrpositioning;

	public float[] td_refblackwhite;

	public short[][] td_transferfunction = new short[3][];

	public int td_inknameslen;

	public string td_inknames;

	public int td_customValueCount;

	public TiffTagValue[] td_customValues;

	public TiffDirectory()
	{
		td_subfiletype = (FileType)0;
		td_compression = (Compression)0;
		td_photometric = Photometric.MINISWHITE;
		td_planarconfig = PlanarConfig.UNKNOWN;
		td_fillorder = FillOrder.MSB2LSB;
		td_bitspersample = 1;
		td_threshholding = Threshold.BILEVEL;
		td_orientation = Orientation.TOPLEFT;
		td_samplesperpixel = 1;
		td_rowsperstrip = -1;
		td_tiledepth = 1;
		td_stripbytecountsorted = true;
		td_resolutionunit = ResUnit.INCH;
		td_sampleformat = SampleFormat.UINT;
		td_imagedepth = 1;
		td_ycbcrsubsampling[0] = 2;
		td_ycbcrsubsampling[1] = 2;
		td_ycbcrpositioning = YCbCrPosition.CENTERED;
	}
}
