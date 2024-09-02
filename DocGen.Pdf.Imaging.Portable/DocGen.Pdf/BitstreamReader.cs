using System;

namespace DocGen.Pdf;

internal abstract class BitstreamReader : CodedCBlkDataSrcDec, InvWTData, MultiResImgData
{
	internal DecodeHelper decSpec;

	internal bool[] derived;

	internal int[] gb;

	internal StdDequantizerParams[] params_Renamed;

	public const char OPT_PREFIX = 'B';

	private static readonly string[][] pinfo;

	internal int[] mdl;

	internal int nc;

	internal int targetRes;

	internal SubbandSyn[] subbTrees;

	internal int imgW;

	internal int imgH;

	internal int ax;

	internal int ay;

	internal int px;

	internal int py;

	internal int[] offX;

	internal int[] offY;

	internal int[] culx;

	internal int[] culy;

	internal int ntW;

	internal int ntH;

	internal int ntX;

	internal int ntY;

	internal int nt;

	internal int ctX;

	internal int ctY;

	internal HeaderDecoder hd;

	internal int tnbytes;

	internal int anbytes;

	internal float trate;

	internal float arate;

	public virtual int CbULX => hd.CbULX;

	public virtual int CbULY => hd.CbULY;

	public virtual int NumComps => nc;

	public virtual int TileIdx => ctY * ntX + ctX;

	public static string[][] ParameterInfo => pinfo;

	public virtual int ImgRes => targetRes;

	public virtual float TargetRate => trate;

	public virtual float ActualRate
	{
		get
		{
			arate = (float)anbytes * 8f / (float)hd.MaxCompImgWidth / (float)hd.MaxCompImgHeight;
			return arate;
		}
	}

	public virtual int TargetNbytes => tnbytes;

	public virtual int ActualNbytes => anbytes;

	public virtual int TilePartULX => hd.getTilingOrigin(null).x;

	public virtual int TilePartULY => hd.getTilingOrigin(null).y;

	public virtual int NomTileWidth => hd.NomTileWidth;

	public virtual int NomTileHeight => hd.NomTileHeight;

	internal BitstreamReader(HeaderDecoder hd, DecodeHelper decSpec)
	{
		this.decSpec = decSpec;
		this.hd = hd;
		nc = hd.NumComps;
		offX = new int[nc];
		offY = new int[nc];
		culx = new int[nc];
		culy = new int[nc];
		imgW = hd.ImgWidth;
		imgH = hd.ImgHeight;
		ax = hd.ImgULX;
		ay = hd.ImgULY;
		JPXImageCoordinates tilingOrigin = hd.getTilingOrigin(null);
		px = tilingOrigin.x;
		py = tilingOrigin.y;
		ntW = hd.NomTileWidth;
		ntH = hd.NomTileHeight;
		ntX = (ax + imgW - px + ntW - 1) / ntW;
		ntY = (ay + imgH - py + ntH - 1) / ntH;
		nt = ntX * ntY;
	}

	public int getCompSubsX(int c)
	{
		return hd.getCompSubsX(c);
	}

	public virtual int getCompSubsY(int c)
	{
		return hd.getCompSubsY(c);
	}

	public virtual int getTileWidth(int rl)
	{
		int minInTile = decSpec.dls.getMinInTile(TileIdx);
		if (rl > minInTile)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one component in tile: " + ctX + "x" + ctY);
		}
		int num = minInTile - rl;
		int num2 = ((ctX == 0) ? ax : (px + ctX * ntW));
		return (((ctX < ntX - 1) ? (px + (ctX + 1) * ntW) : (ax + imgW)) + (1 << num) - 1) / (1 << num) - (num2 + (1 << num) - 1) / (1 << num);
	}

	public virtual int getTileHeight(int rl)
	{
		int minInTile = decSpec.dls.getMinInTile(TileIdx);
		if (rl > minInTile)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one component in tile: " + ctX + "x" + ctY);
		}
		int num = minInTile - rl;
		int num2 = ((ctY == 0) ? ay : (py + ctY * ntH));
		return (((ctY < ntY - 1) ? (py + (ctY + 1) * ntH) : (ay + imgH)) + (1 << num) - 1) / (1 << num) - (num2 + (1 << num) - 1) / (1 << num);
	}

	public virtual int getImgWidth(int rl)
	{
		int min = decSpec.dls.Min;
		if (rl > min)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one tile-component");
		}
		int num = min - rl;
		return (ax + imgW + (1 << num) - 1) / (1 << num) - (ax + (1 << num) - 1) / (1 << num);
	}

	public virtual int getImgHeight(int rl)
	{
		int min = decSpec.dls.Min;
		if (rl > min)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one tile-component");
		}
		int num = min - rl;
		return (ay + imgH + (1 << num) - 1) / (1 << num) - (ay + (1 << num) - 1) / (1 << num);
	}

	public virtual int getImgULX(int rl)
	{
		int min = decSpec.dls.Min;
		if (rl > min)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one tile-component");
		}
		int num = min - rl;
		return (ax + (1 << num) - 1) / (1 << num);
	}

	public virtual int getImgULY(int rl)
	{
		int min = decSpec.dls.Min;
		if (rl > min)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one tile-component");
		}
		int num = min - rl;
		return (ay + (1 << num) - 1) / (1 << num);
	}

	public int getTileCompWidth(int t, int c, int rl)
	{
		int tileIdx = TileIdx;
		int num = mdl[c] - rl;
		return ((((ctX < ntX - 1) ? (px + (ctX + 1) * ntW) : (ax + imgW)) + hd.getCompSubsX(c) - 1) / hd.getCompSubsX(c) + (1 << num) - 1) / (1 << num) - (culx[c] + (1 << num) - 1) / (1 << num);
	}

	public int getTileCompHeight(int t, int c, int rl)
	{
		int tileIdx = TileIdx;
		int num = mdl[c] - rl;
		return ((((ctY < ntY - 1) ? (py + (ctY + 1) * ntH) : (ay + imgH)) + hd.getCompSubsY(c) - 1) / hd.getCompSubsY(c) + (1 << num) - 1) / (1 << num) - (culy[c] + (1 << num) - 1) / (1 << num);
	}

	public int getCompImgWidth(int c, int rl)
	{
		int num = decSpec.dls.getMinInComp(c) - rl;
		int num2 = (ax + hd.getCompSubsX(c) - 1) / hd.getCompSubsX(c);
		return ((ax + imgW + hd.getCompSubsX(c) - 1) / hd.getCompSubsX(c) + (1 << num) - 1) / (1 << num) - (num2 + (1 << num) - 1) / (1 << num);
	}

	public int getCompImgHeight(int c, int rl)
	{
		int num = decSpec.dls.getMinInComp(c) - rl;
		int num2 = (ay + hd.getCompSubsY(c) - 1) / hd.getCompSubsY(c);
		return ((ay + imgH + hd.getCompSubsY(c) - 1) / hd.getCompSubsY(c) + (1 << num) - 1) / (1 << num) - (num2 + (1 << num) - 1) / (1 << num);
	}

	public abstract void setTile(int x, int y);

	public abstract void nextTile();

	public JPXImageCoordinates getTile(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = ctX;
			co.y = ctY;
			return co;
		}
		return new JPXImageCoordinates(ctX, ctY);
	}

	public int getResULX(int c, int rl)
	{
		int num = mdl[c] - rl;
		if (num < 0)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one component in tile: " + ctX + "x" + ctY);
		}
		return (int)Math.Ceiling((double)(int)Math.Ceiling((double)Math.Max(px + ctX * ntW, ax) / (double)getCompSubsX(c)) / (double)(1 << num));
	}

	public int getResULY(int c, int rl)
	{
		int num = mdl[c] - rl;
		if (num < 0)
		{
			throw new ArgumentException("Requested resolution level is not available for, at least, one component in tile: " + ctX + "x" + ctY);
		}
		return (int)Math.Ceiling((double)(int)Math.Ceiling((double)Math.Max(py + ctY * ntH, ay) / (double)getCompSubsY(c)) / (double)(1 << num));
	}

	public JPXImageCoordinates getNumTiles(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = ntX;
			co.y = ntY;
			return co;
		}
		return new JPXImageCoordinates(ntX, ntY);
	}

	public int getNumTiles()
	{
		return ntX * ntY;
	}

	public SubbandSyn getSynSubbandTree(int t, int c)
	{
		if (t != TileIdx)
		{
			throw new ArgumentException("Can not request subband tree of a different tile than the current one");
		}
		if (c < 0 || c >= nc)
		{
			throw new ArgumentException("Component index out of range");
		}
		return subbTrees[c];
	}

	internal static BitstreamReader createInstance(JPXRandomAccessStream in_Renamed, HeaderDecoder hd, JPXParameters pl, DecodeHelper decSpec, bool cdstrInfo, HeaderInformation hi)
	{
		pl.checkList('B', JPXParameters.toNameArray(ParameterInfo));
		return new FileBitstreamReaderAgent(hd, in_Renamed, decSpec, pl, cdstrInfo, hi);
	}

	public int getPPX(int t, int c, int rl)
	{
		return decSpec.pss.getPPX(t, c, rl);
	}

	public int getPPY(int t, int c, int rl)
	{
		return decSpec.pss.getPPY(t, c, rl);
	}

	internal virtual void initSubbandsFields(int c, SubbandSyn sb)
	{
		int tileIdx = TileIdx;
		int resLvl = sb.resLvl;
		int cBlkWidth = decSpec.cblks.getCBlkWidth(3, tileIdx, c);
		int cBlkHeight = decSpec.cblks.getCBlkHeight(3, tileIdx, c);
		if (!sb.isNode)
		{
			if (hd.precinctPartitionUsed())
			{
				int num = MathUtil.log2(getPPX(tileIdx, c, resLvl));
				int num2 = MathUtil.log2(getPPY(tileIdx, c, resLvl));
				int num3 = MathUtil.log2(cBlkWidth);
				int num4 = MathUtil.log2(cBlkHeight);
				if (sb.resLvl == 0)
				{
					sb.nomCBlkW = ((num3 < num) ? (1 << num3) : (1 << num));
					sb.nomCBlkH = ((num4 < num2) ? (1 << num4) : (1 << num2));
				}
				else
				{
					sb.nomCBlkW = ((num3 < num - 1) ? (1 << num3) : (1 << num - 1));
					sb.nomCBlkH = ((num4 < num2 - 1) ? (1 << num4) : (1 << num2 - 1));
				}
			}
			else
			{
				sb.nomCBlkW = cBlkWidth;
				sb.nomCBlkH = cBlkHeight;
			}
			if (sb.numCb == null)
			{
				sb.numCb = new JPXImageCoordinates();
			}
			if (sb.w == 0 || sb.h == 0)
			{
				sb.numCb.x = 0;
				sb.numCb.y = 0;
			}
			else
			{
				int cbULX = CbULX;
				int cbULY = CbULY;
				int num5 = cbULX;
				int num6 = cbULY;
				switch (sb.sbandIdx)
				{
				case 1:
					num5 = 0;
					break;
				case 2:
					num6 = 0;
					break;
				case 3:
					num5 = 0;
					num6 = 0;
					break;
				}
				if (sb.ulcx - num5 < 0 || sb.ulcy - num6 < 0)
				{
					throw new ArgumentException("Invalid code-blocks partition origin or image offset in the reference grid.");
				}
				int num7 = sb.ulcx - num5 + sb.nomCBlkW;
				sb.numCb.x = (num7 + sb.w - 1) / sb.nomCBlkW - (num7 / sb.nomCBlkW - 1);
				num7 = sb.ulcy - num6 + sb.nomCBlkH;
				sb.numCb.y = (num7 + sb.h - 1) / sb.nomCBlkH - (num7 / sb.nomCBlkH - 1);
			}
			if (derived[c])
			{
				sb.magbits = gb[c] + (params_Renamed[c].exp[0][0] - (mdl[c] - sb.level)) - 1;
			}
			else
			{
				sb.magbits = gb[c] + params_Renamed[c].exp[sb.resLvl][sb.sbandIdx] - 1;
			}
		}
		else
		{
			initSubbandsFields(c, (SubbandSyn)sb.LL);
			initSubbandsFields(c, (SubbandSyn)sb.HL);
			initSubbandsFields(c, (SubbandSyn)sb.LH);
			initSubbandsFields(c, (SubbandSyn)sb.HH);
		}
	}

	public abstract DecLyrdCBlk getCodeBlock(int param1, int param2, int param3, SubbandSyn param4, int param5, int param6, DecLyrdCBlk param7);
}
