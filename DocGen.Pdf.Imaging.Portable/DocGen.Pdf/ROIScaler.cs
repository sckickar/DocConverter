namespace DocGen.Pdf;

internal class ROIScaler
{
	public const char OPT_PREFIX = 'R';

	private static readonly string[][] pinfo = new string[4][]
	{
		new string[4] { "Rroi", "[<component idx>] R <left> <top> <width> <height> or [<component idx>] C <centre column> <centre row> <radius> or [<component idx>] A <filename>", "Specifies ROIs shape and location. The shape can be either rectangular 'R', or circular 'C' or arbitrary 'A'. Each new occurrence of an 'R', a 'C' or an 'A' is a new ROI. For circular and rectangular ROIs, all values are given as their pixel values relative to the canvas origin. Arbitrary shapes must be included in a PGM file where non 0 values correspond to ROI coefficients. The PGM file must have the size as the image. The component idx specifies which components contain the ROI. The component index is specified as described by points 3 and 4 in the general comment on tile-component idx. If this option is used, the codestream is layer progressive by default unless it is overridden by the 'Aptype' option.", null },
		new string[4] { "Ralign", "[on|off]", "By specifying this argument, the ROI mask will be limited to covering only entire code-blocks. The ROI coding can then be performed without any actual scaling of the coefficients but by instead scaling the distortion estimates.", "off" },
		new string[4] { "Rstart_level", "<level>", "This argument forces the lowest <level> resolution levels to belong to the ROI. By doing this, it is possible to avoid only getting information for the ROI at an early stage of transmission.<level> = 0 means the lowest resolution level belongs to the ROI, 1 means the two lowest etc. (-1 deactivates the option)", "-1" },
		new string[4] { "Rno_rect", "[on|off]", "This argument makes sure that the ROI mask generation is not done using the fast ROI mask generation for rectangular ROIs regardless of whether the specified ROIs are rectangular or not", "off" }
	};

	private int[][] maxMagBits;

	private bool roi;

	private bool blockAligned;

	private int useStartLevel;

	private ROIMaskGenerator mg;

	private DataBlockInt roiMask;

	private Quantizer src;

	public virtual int CbULX => src.CbULX;

	public virtual int CbULY => src.CbULY;

	public virtual ROIMaskGenerator ROIMaskGenerator => mg;

	public virtual bool BlockAligned => blockAligned;

	public static string[][] ParameterInfo => pinfo;

	public virtual bool isReversible(int t, int c)
	{
		return src.isReversible(t, c);
	}

	public virtual SubbandAn getAnSubbandTree(int t, int c)
	{
		return src.getAnSubbandTree(t, c);
	}

	public virtual bool useRoi()
	{
		return roi;
	}

	private void calcMaxMagBits(EncoderSpecs encSpec)
	{
		MaxShiftSpec rois = encSpec.rois;
		int numTiles = src.getNumTiles();
		int numComps = src.NumComps;
		maxMagBits = new int[numTiles][];
		for (int i = 0; i < numTiles; i++)
		{
			maxMagBits[i] = new int[numComps];
		}
		src.setTile(0, 0);
		for (int j = 0; j < numTiles; j++)
		{
			for (int num = numComps - 1; num >= 0; num--)
			{
				int num2 = src.getMaxMagBits(num);
				maxMagBits[j][num] = num2;
				rois.setTileCompVal(j, num, num2);
			}
			if (j < numTiles - 1)
			{
				src.nextTile();
			}
		}
		src.setTile(0, 0);
	}
}
