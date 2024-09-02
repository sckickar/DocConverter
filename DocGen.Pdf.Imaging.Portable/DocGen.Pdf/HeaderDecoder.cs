using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf;

internal class HeaderDecoder
{
	public const char OPT_PREFIX = 'H';

	private static readonly string[][] pinfo;

	private HeaderInformation hi;

	private string hdStr = "";

	private int nTiles;

	public int[] nTileParts;

	private int nfMarkSeg;

	private int nCOCMarkSeg;

	private int nQCCMarkSeg;

	private int nCOMMarkSeg;

	private int nRGNMarkSeg;

	private int nPPMMarkSeg;

	private int[][] nPPTMarkSeg;

	private const int SIZ_FOUND = 1;

	private const int COD_FOUND = 2;

	private const int COC_FOUND = 4;

	private const int QCD_FOUND = 8;

	private const int TLM_FOUND = 16;

	private const int PLM_FOUND = 32;

	private const int SOT_FOUND = 64;

	private const int PLT_FOUND = 128;

	private const int QCC_FOUND = 256;

	private const int RGN_FOUND = 512;

	private const int POC_FOUND = 1024;

	private const int COM_FOUND = 2048;

	public const int SOD_FOUND = 8192;

	public const int PPM_FOUND = 16384;

	public const int PPT_FOUND = 32768;

	public const int CRG_FOUND = 65536;

	private Dictionary<object, object> ht;

	private int nComp;

	private int cb0x = -1;

	private int cb0y = -1;

	private DecodeHelper decSpec;

	internal bool precinctPartitionIsUsed;

	public int mainHeadOff;

	private List<object> tileOfTileParts;

	private byte[][] pPMMarkerData;

	private byte[][][][] tilePartPkdPktHeaders;

	private MemoryStream[] pkdPktHeaders;

	public virtual int MaxCompImgHeight => hi.sizValue.MaxCompHeight;

	public virtual int MaxCompImgWidth => hi.sizValue.MaxCompWidth;

	public virtual int ImgWidth => hi.sizValue.xsiz - hi.sizValue.x0siz;

	public virtual int ImgHeight => hi.sizValue.ysiz - hi.sizValue.y0siz;

	public virtual int ImgULX => hi.sizValue.x0siz;

	public virtual int ImgULY => hi.sizValue.y0siz;

	public virtual int NomTileWidth => hi.sizValue.xtsiz;

	public virtual int NomTileHeight => hi.sizValue.ytsiz;

	public virtual int NumComps => nComp;

	public virtual int CbULX => cb0x;

	public virtual int CbULY => cb0y;

	public virtual DecodeHelper DecoderHelper => decSpec;

	public static string[][] ParameterInfo => pinfo;

	public virtual int NumTiles => nTiles;

	public virtual int TileOfTileParts
	{
		set
		{
			if (nPPMMarkSeg != 0)
			{
				tileOfTileParts.Add(value);
			}
		}
	}

	public virtual int NumFoundMarkSeg => nfMarkSeg;

	public JPXImageCoordinates getTilingOrigin(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = hi.sizValue.xt0siz;
			co.y = hi.sizValue.yt0siz;
			return co;
		}
		return new JPXImageCoordinates(hi.sizValue.xt0siz, hi.sizValue.yt0siz);
	}

	public bool isOriginalSigned(int c)
	{
		return hi.sizValue.isOrigSigned(c);
	}

	public int GetActualBitDepth(int c)
	{
		return hi.sizValue.getOrigBitDepth(c);
	}

	public int getCompSubsX(int c)
	{
		return hi.sizValue.xrsiz[c];
	}

	public int getCompSubsY(int c)
	{
		return hi.sizValue.yrsiz[c];
	}

	public Dequantizer createDequantizer(CBlkQuantDataSrcDec src, int[] rb, DecodeHelper decSpec2)
	{
		return new StdDequantizer(src, rb, decSpec2);
	}

	public int getPPX(int t, int c, int rl)
	{
		return decSpec.pss.getPPX(t, c, rl);
	}

	public int getPPY(int t, int c, int rl)
	{
		return decSpec.pss.getPPY(t, c, rl);
	}

	public bool precinctPartitionUsed()
	{
		return precinctPartitionIsUsed;
	}

	private SynWTFilter readFilter(BinaryReader ehs, int[] filtIdx)
	{
		int num = (filtIdx[0] = ehs.ReadByte());
		if (num >= 128)
		{
			throw new ArgumentException("Custom filters are used");
		}
		return num switch
		{
			0 => new SynWTFilterFloatLift9x7(), 
			1 => new SynWTFilterIntLift5x3(), 
			_ => throw new Exception(), 
		};
	}

	public virtual void checkMarkerLength(BinaryReader ehs, string str)
	{
		_ = ehs.BaseStream.Length;
		_ = ehs.BaseStream.Position;
	}

	private void readSIZ(BinaryReader ehs)
	{
		HeaderInformation.SIZ newSIZ = hi.NewSIZ;
		hi.sizValue = newSIZ;
		newSIZ.lsiz = ehs.ReadUInt16();
		newSIZ.rsiz = ehs.ReadUInt16();
		_ = newSIZ.rsiz;
		_ = 2;
		newSIZ.xsiz = ehs.ReadInt32();
		newSIZ.ysiz = ehs.ReadInt32();
		if (newSIZ.xsiz <= 0 || newSIZ.ysiz <= 0)
		{
			throw new IOException("JJ2000 does not support images whose width and/or height not in the range: 1 -- (2^31)-1");
		}
		newSIZ.x0siz = ehs.ReadInt32();
		newSIZ.y0siz = ehs.ReadInt32();
		if (newSIZ.x0siz < 0 || newSIZ.y0siz < 0)
		{
			throw new IOException("JJ2000 does not support images offset not in the range: 0 -- (2^31)-1");
		}
		newSIZ.xtsiz = ehs.ReadInt32();
		newSIZ.ytsiz = ehs.ReadInt32();
		if (newSIZ.xtsiz <= 0 || newSIZ.ytsiz <= 0)
		{
			throw new IOException("JJ2000 does not support tiles whose width and/or height are not in  the range: 1 -- (2^31)-1");
		}
		newSIZ.xt0siz = ehs.ReadInt32();
		newSIZ.yt0siz = ehs.ReadInt32();
		if (newSIZ.xt0siz < 0 || newSIZ.yt0siz < 0)
		{
			throw new IOException("JJ2000 does not support tiles whose offset is not in  the range: 0 -- (2^31)-1");
		}
		nComp = (newSIZ.csiz = ehs.ReadUInt16());
		if (nComp < 1 || nComp > 16384)
		{
			throw new ArgumentException("Number of component out of range 1--16384: " + nComp);
		}
		newSIZ.ssiz = new int[nComp];
		newSIZ.xrsiz = new int[nComp];
		newSIZ.yrsiz = new int[nComp];
		for (int i = 0; i < nComp; i++)
		{
			newSIZ.ssiz[i] = ehs.ReadByte();
			newSIZ.xrsiz[i] = ehs.ReadByte();
			newSIZ.yrsiz[i] = ehs.ReadByte();
		}
		checkMarkerLength(ehs, "SIZ marker");
		nTiles = newSIZ.NumTiles;
		decSpec = new DecodeHelper(nTiles, nComp);
	}

	private void readCRG(BinaryReader ehs)
	{
		HeaderInformation.CRG newCRG = hi.NewCRG;
		hi.crgValue = newCRG;
		newCRG.lcrg = ehs.ReadUInt16();
		newCRG.xcrg = new int[nComp];
		newCRG.ycrg = new int[nComp];
		for (int i = 0; i < nComp; i++)
		{
			newCRG.xcrg[i] = ehs.ReadUInt16();
			newCRG.ycrg[i] = ehs.ReadUInt16();
		}
		checkMarkerLength(ehs, "CRG marker");
	}

	private void readCOM(BinaryReader ehs, bool mainh, int tileIdx, int comIdx)
	{
		HeaderInformation.COM newCOM = hi.NewCOM;
		newCOM.lcom = ehs.ReadUInt16();
		newCOM.rcom = ehs.ReadUInt16();
		if (newCOM.rcom == 1)
		{
			newCOM.ccom = new byte[newCOM.lcom - 4];
			for (int i = 0; i < newCOM.lcom - 4; i++)
			{
				newCOM.ccom[i] = ehs.ReadByte();
			}
		}
		else
		{
			long position = ehs.BaseStream.Position;
			position = ehs.BaseStream.Seek(newCOM.lcom - 4, SeekOrigin.Current) - position;
		}
		if (mainh)
		{
			hi.comValue["main_" + comIdx] = newCOM;
		}
		else
		{
			hi.comValue["t" + tileIdx + "_" + comIdx] = newCOM;
		}
		checkMarkerLength(ehs, "COM marker");
	}

	private void readQCD(BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
	{
		float[][] array = null;
		HeaderInformation.QCD newQCD = hi.NewQCD;
		newQCD.lqcd = ehs.ReadUInt16();
		newQCD.sqcd = ehs.ReadByte();
		int numGuardBits = newQCD.NumGuardBits;
		int quantType = newQCD.QuantType;
		if (mainh)
		{
			hi.qcdValue["main"] = newQCD;
			switch (quantType)
			{
			case 0:
				decSpec.qts.setDefault("reversible");
				break;
			case 1:
				decSpec.qts.setDefault("derived");
				break;
			case 2:
				decSpec.qts.setDefault("expounded");
				break;
			}
		}
		else
		{
			hi.qcdValue["t" + tileIdx] = newQCD;
			switch (quantType)
			{
			case 0:
				decSpec.qts.setTileDef(tileIdx, "reversible");
				break;
			case 1:
				decSpec.qts.setTileDef(tileIdx, "derived");
				break;
			case 2:
				decSpec.qts.setTileDef(tileIdx, "expounded");
				break;
			}
		}
		StdDequantizerParams stdDequantizerParams = new StdDequantizerParams();
		if (quantType == 0)
		{
			int num = (mainh ? ((int)decSpec.dls.getDefault()) : ((int)decSpec.dls.getTileDef(tileIdx)));
			int[][] array2 = (stdDequantizerParams.exp = new int[num + 1][]);
			int[][] array3 = new int[num + 1][];
			for (int i = 0; i < num + 1; i++)
			{
				array3[i] = new int[4];
			}
			newQCD.spqcd = array3;
			for (int j = 0; j <= num; j++)
			{
				int num2;
				int num3;
				if (j == 0)
				{
					num2 = 0;
					num3 = 1;
				}
				else
				{
					int num4 = 1;
					num4 = ((num4 <= num - j) ? 1 : (num4 - (num - j)));
					num2 = 1 << (num4 - 1 << 1);
					num3 = 1 << (num4 << 1);
				}
				array2[j] = new int[num3];
				for (int k = num2; k < num3; k++)
				{
					int num5 = (newQCD.spqcd[j][k] = ehs.ReadByte());
					array2[j][k] = (num5 >> 3) & 0x1F;
				}
			}
		}
		else
		{
			int num6 = ((quantType != 1) ? (mainh ? ((int)decSpec.dls.getDefault()) : ((int)decSpec.dls.getTileDef(tileIdx))) : 0);
			int[][] array2 = (stdDequantizerParams.exp = new int[num6 + 1][]);
			array = (stdDequantizerParams.nStep = new float[num6 + 1][]);
			int[][] array4 = new int[num6 + 1][];
			for (int l = 0; l < num6 + 1; l++)
			{
				array4[l] = new int[4];
			}
			newQCD.spqcd = array4;
			for (int m = 0; m <= num6; m++)
			{
				int num7;
				int num8;
				if (m == 0)
				{
					num7 = 0;
					num8 = 1;
				}
				else
				{
					int num9 = 1;
					num9 = ((num9 <= num6 - m) ? 1 : (num9 - (num6 - m)));
					num7 = 1 << (num9 - 1 << 1);
					num8 = 1 << (num9 << 1);
				}
				array2[m] = new int[num8];
				array[m] = new float[num8];
				for (int n = num7; n < num8; n++)
				{
					int num10 = (newQCD.spqcd[m][n] = ehs.ReadUInt16());
					array2[m][n] = (num10 >> 11) & 0x1F;
					array[m][n] = (-1f - (float)(num10 & 0x7FF) / 2048f) / (float)(-1 << array2[m][n]);
				}
			}
		}
		if (mainh)
		{
			decSpec.qsss.setDefault(stdDequantizerParams);
			decSpec.gbs.setDefault(numGuardBits);
		}
		else
		{
			decSpec.qsss.setTileDef(tileIdx, stdDequantizerParams);
			decSpec.gbs.setTileDef(tileIdx, numGuardBits);
		}
		checkMarkerLength(ehs, "QCD marker");
	}

	private void readQCC(BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
	{
		float[][] array = null;
		HeaderInformation.QCC newQCC = hi.NewQCC;
		newQCC.lqcc = ehs.ReadUInt16();
		int num = ((nComp >= 257) ? (newQCC.cqcc = ehs.ReadUInt16()) : (newQCC.cqcc = ehs.ReadByte()));
		if (num >= nComp)
		{
			throw new Exception();
		}
		newQCC.sqcc = ehs.ReadByte();
		int numGuardBits = newQCC.NumGuardBits;
		int quantType = newQCC.QuantType;
		if (mainh)
		{
			hi.qccValue["main_c" + num] = newQCC;
			switch (quantType)
			{
			case 0:
				decSpec.qts.setCompDef(num, "reversible");
				break;
			case 1:
				decSpec.qts.setCompDef(num, "derived");
				break;
			case 2:
				decSpec.qts.setCompDef(num, "expounded");
				break;
			default:
				throw new Exception();
			}
		}
		else
		{
			hi.qccValue["t" + tileIdx + "_c" + num] = newQCC;
			switch (quantType)
			{
			case 0:
				decSpec.qts.setTileCompVal(tileIdx, num, "reversible");
				break;
			case 1:
				decSpec.qts.setTileCompVal(tileIdx, num, "derived");
				break;
			case 2:
				decSpec.qts.setTileCompVal(tileIdx, num, "expounded");
				break;
			default:
				throw new Exception();
			}
		}
		StdDequantizerParams stdDequantizerParams = new StdDequantizerParams();
		if (quantType == 0)
		{
			int num2 = (mainh ? ((int)decSpec.dls.getCompDef(num)) : ((int)decSpec.dls.getTileCompVal(tileIdx, num)));
			int[][] array2 = (stdDequantizerParams.exp = new int[num2 + 1][]);
			int[][] array3 = new int[num2 + 1][];
			for (int i = 0; i < num2 + 1; i++)
			{
				array3[i] = new int[4];
			}
			newQCC.spqcc = array3;
			for (int j = 0; j <= num2; j++)
			{
				int num3;
				int num4;
				if (j == 0)
				{
					num3 = 0;
					num4 = 1;
				}
				else
				{
					int num5 = 1;
					num5 = ((num5 <= num2 - j) ? 1 : (num5 - (num2 - j)));
					num3 = 1 << (num5 - 1 << 1);
					num4 = 1 << (num5 << 1);
				}
				array2[j] = new int[num4];
				for (int k = num3; k < num4; k++)
				{
					int num6 = (newQCC.spqcc[j][k] = ehs.ReadByte());
					array2[j][k] = (num6 >> 3) & 0x1F;
				}
			}
		}
		else
		{
			int num7 = ((quantType != 1) ? (mainh ? ((int)decSpec.dls.getCompDef(num)) : ((int)decSpec.dls.getTileCompVal(tileIdx, num))) : 0);
			array = (stdDequantizerParams.nStep = new float[num7 + 1][]);
			int[][] array2 = (stdDequantizerParams.exp = new int[num7 + 1][]);
			int[][] array4 = new int[num7 + 1][];
			for (int l = 0; l < num7 + 1; l++)
			{
				array4[l] = new int[4];
			}
			newQCC.spqcc = array4;
			for (int m = 0; m <= num7; m++)
			{
				int num8;
				int num9;
				if (m == 0)
				{
					num8 = 0;
					num9 = 1;
				}
				else
				{
					int num10 = 1;
					num10 = ((num10 <= num7 - m) ? 1 : (num10 - (num7 - m)));
					num8 = 1 << (num10 - 1 << 1);
					num9 = 1 << (num10 << 1);
				}
				array2[m] = new int[num9];
				array[m] = new float[num9];
				for (int n = num8; n < num9; n++)
				{
					int num6 = (newQCC.spqcc[m][n] = ehs.ReadUInt16());
					array2[m][n] = (num6 >> 11) & 0x1F;
					array[m][n] = (-1f - (float)(num6 & 0x7FF) / 2048f) / (float)(-1 << array2[m][n]);
				}
			}
		}
		if (mainh)
		{
			decSpec.qsss.setCompDef(num, stdDequantizerParams);
			decSpec.gbs.setCompDef(num, numGuardBits);
		}
		else
		{
			decSpec.qsss.setTileCompVal(tileIdx, num, stdDequantizerParams);
			decSpec.gbs.setTileCompVal(tileIdx, num, numGuardBits);
		}
		checkMarkerLength(ehs, "QCC marker");
	}

	private void readCOD(BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
	{
		HeaderInformation.COD newCOD = hi.NewCOD;
		newCOD.lcod = ehs.ReadUInt16();
		int num = (newCOD.scod = ehs.ReadByte());
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			precinctPartitionIsUsed = true;
			num &= -2;
		}
		else
		{
			precinctPartitionIsUsed = false;
		}
		if (mainh)
		{
			hi.codValue["main"] = newCOD;
			if (((uint)num & 2u) != 0)
			{
				decSpec.sops.setDefault("true".ToUpper().Equals("TRUE"));
				num &= -3;
			}
			else
			{
				decSpec.sops.setDefault("false".ToUpper().Equals("TRUE"));
			}
		}
		else
		{
			hi.codValue["t" + tileIdx] = newCOD;
			if (((uint)num & 2u) != 0)
			{
				decSpec.sops.setTileDef(tileIdx, "true".ToUpper().Equals("TRUE"));
				num &= -3;
			}
			else
			{
				decSpec.sops.setTileDef(tileIdx, "false".ToUpper().Equals("TRUE"));
			}
		}
		if (mainh)
		{
			if (((uint)num & 4u) != 0)
			{
				decSpec.ephs.setDefault("true".ToUpper().Equals("TRUE"));
				num &= -5;
			}
			else
			{
				decSpec.ephs.setDefault("false".ToUpper().Equals("TRUE"));
			}
		}
		else if (((uint)num & 4u) != 0)
		{
			decSpec.ephs.setTileDef(tileIdx, "true".ToUpper().Equals("TRUE"));
			num &= -5;
		}
		else
		{
			decSpec.ephs.setTileDef(tileIdx, "false".ToUpper().Equals("TRUE"));
		}
		_ = num & 0x18;
		if (((uint)num & 8u) != 0)
		{
			if (cb0x != -1 && cb0x == 0)
			{
				throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
			}
			cb0x = 1;
			num &= -9;
		}
		else
		{
			if (cb0x != -1 && cb0x == 1)
			{
				throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
			}
			cb0x = 0;
		}
		if (((uint)num & 0x10u) != 0)
		{
			if (cb0y != -1 && cb0y == 0)
			{
				throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
			}
			cb0y = 1;
			num &= -17;
		}
		else
		{
			if (cb0y != -1 && cb0y == 1)
			{
				throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
			}
			cb0y = 0;
		}
		newCOD.sgcod_po = ehs.ReadByte();
		newCOD.sgcod_nl = ehs.ReadUInt16();
		if (newCOD.sgcod_nl <= 0 || newCOD.sgcod_nl > 65535)
		{
			throw new ArgumentException("Number of layers out of range: 1--65535");
		}
		newCOD.sgcod_mct = ehs.ReadByte();
		int num2 = (newCOD.spcod_ndl = ehs.ReadByte());
		if (num2 > 32)
		{
			throw new ArgumentException("Number of decomposition levels out of range: 0--32");
		}
		int[] array = new int[2];
		newCOD.spcod_cw = ehs.ReadByte();
		array[0] = 1 << newCOD.spcod_cw + 2;
		if (array[0] < StdEntropyCoderOptions.MIN_CB_DIM || array[0] > StdEntropyCoderOptions.MAX_CB_DIM)
		{
			throw new ArgumentException("Non-valid code-block width in SPcod field, COD marker");
		}
		newCOD.spcod_ch = ehs.ReadByte();
		array[1] = 1 << newCOD.spcod_ch + 2;
		if (array[1] < StdEntropyCoderOptions.MIN_CB_DIM || array[1] > StdEntropyCoderOptions.MAX_CB_DIM)
		{
			throw new ArgumentException("Non-valid code-block height in SPcod field, COD marker");
		}
		if (array[0] * array[1] > StdEntropyCoderOptions.MAX_CB_AREA)
		{
			throw new ArgumentException("Non-valid code-block area in SPcod field, COD marker");
		}
		if (mainh)
		{
			decSpec.cblks.setDefault(array);
		}
		else
		{
			decSpec.cblks.setTileDef(tileIdx, array);
		}
		int num3 = (newCOD.spcod_cs = ehs.ReadByte());
		if ((num3 & ~(StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_RESET_MQ | StdEntropyCoderOptions.OPT_TERM_PASS | StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL | StdEntropyCoderOptions.OPT_PRED_TERM | StdEntropyCoderOptions.OPT_SEG_SYMBOLS)) != 0)
		{
			throw new ArgumentException("Unknown \"code-block style\" in SPcod field, COD marker: 0x" + Convert.ToString(num3, 16));
		}
		SynWTFilter[] array2 = new SynWTFilter[1];
		SynWTFilter[] array3 = new SynWTFilter[1];
		array2[0] = readFilter(ehs, newCOD.spcod_t);
		array3[0] = array2[0];
		SynWTFilter[][] array4 = new SynWTFilter[2][] { array2, array3 };
		List<object>[] array5 = new List<object>[2]
		{
			new List<object>(10),
			new List<object>(10)
		};
		int num4 = 65535;
		if (!precinctPartitionIsUsed)
		{
			int num5 = 1 << (num4 & 0xF);
			array5[0].Add(num5);
			int num6 = 1 << ((num4 & 0xF0) >> 4);
			array5[1].Add(num6);
		}
		else
		{
			newCOD.spcod_ps = new int[num2 + 1];
			for (int num7 = num2; num7 >= 0; num7--)
			{
				num4 = (newCOD.spcod_ps[num2 - num7] = ehs.ReadByte());
				int num8 = 1 << (num4 & 0xF);
				array5[0].Insert(0, num8);
				int num9 = 1 << ((num4 & 0xF0) >> 4);
				array5[1].Insert(0, num9);
			}
		}
		if (mainh)
		{
			decSpec.pss.setDefault(array5);
		}
		else
		{
			decSpec.pss.setTileDef(tileIdx, array5);
		}
		precinctPartitionIsUsed = true;
		checkMarkerLength(ehs, "COD marker");
		if (mainh)
		{
			decSpec.wfs.setDefault(array4);
			decSpec.dls.setDefault(num2);
			decSpec.ecopts.setDefault(num3);
			decSpec.cts.setDefault(newCOD.sgcod_mct);
			decSpec.nls.setDefault(newCOD.sgcod_nl);
			decSpec.pos.setDefault(newCOD.sgcod_po);
		}
		else
		{
			decSpec.wfs.setTileDef(tileIdx, array4);
			decSpec.dls.setTileDef(tileIdx, num2);
			decSpec.ecopts.setTileDef(tileIdx, num3);
			decSpec.cts.setTileDef(tileIdx, newCOD.sgcod_mct);
			decSpec.nls.setTileDef(tileIdx, newCOD.sgcod_nl);
			decSpec.pos.setTileDef(tileIdx, newCOD.sgcod_po);
		}
	}

	private void readCOC(BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
	{
		HeaderInformation.COC newCOC = hi.NewCOC;
		newCOC.lcoc = ehs.ReadUInt16();
		int num = ((nComp >= 257) ? (newCOC.ccoc = ehs.ReadUInt16()) : (newCOC.ccoc = ehs.ReadByte()));
		if (num >= nComp)
		{
			throw new ArgumentException("Invalid component index in QCC marker");
		}
		int num2 = (newCOC.scoc = ehs.ReadByte());
		if (((uint)num2 & (true ? 1u : 0u)) != 0)
		{
			precinctPartitionIsUsed = true;
			num2 &= -2;
		}
		else
		{
			precinctPartitionIsUsed = false;
		}
		int num3 = (newCOC.spcoc_ndl = ehs.ReadByte());
		int[] array = new int[2];
		newCOC.spcoc_cw = ehs.ReadByte();
		array[0] = 1 << newCOC.spcoc_cw + 2;
		if (array[0] < StdEntropyCoderOptions.MIN_CB_DIM || array[0] > StdEntropyCoderOptions.MAX_CB_DIM)
		{
			throw new ArgumentException("Non-valid code-block width in SPcod field, COC marker");
		}
		newCOC.spcoc_ch = ehs.ReadByte();
		array[1] = 1 << newCOC.spcoc_ch + 2;
		if (array[1] < StdEntropyCoderOptions.MIN_CB_DIM || array[1] > StdEntropyCoderOptions.MAX_CB_DIM)
		{
			throw new ArgumentException("Non-valid code-block height in SPcod field, COC marker");
		}
		if (array[0] * array[1] > StdEntropyCoderOptions.MAX_CB_AREA)
		{
			throw new ArgumentException("Non-valid code-block area in SPcod field, COC marker");
		}
		if (mainh)
		{
			decSpec.cblks.setCompDef(num, array);
		}
		else
		{
			decSpec.cblks.setTileCompVal(tileIdx, num, array);
		}
		int num4 = (newCOC.spcoc_cs = ehs.ReadByte());
		if ((num4 & ~(StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_RESET_MQ | StdEntropyCoderOptions.OPT_TERM_PASS | StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL | StdEntropyCoderOptions.OPT_PRED_TERM | StdEntropyCoderOptions.OPT_SEG_SYMBOLS)) != 0)
		{
			throw new ArgumentException("Unknown \"code-block context\" in SPcoc field, COC marker: 0x" + Convert.ToString(num4, 16));
		}
		SynWTFilter[] array2 = new SynWTFilter[1];
		SynWTFilter[] array3 = new SynWTFilter[1];
		array2[0] = readFilter(ehs, newCOC.spcoc_t);
		array3[0] = array2[0];
		SynWTFilter[][] value_Renamed = new SynWTFilter[2][] { array2, array3 };
		List<object>[] array4 = new List<object>[2]
		{
			new List<object>(10),
			new List<object>(10)
		};
		int num5 = 65535;
		if (!precinctPartitionIsUsed)
		{
			int num6 = 1 << (num5 & 0xF);
			array4[0].Add(num6);
			int num7 = 1 << ((num5 & 0xF0) >> 4);
			array4[1].Add(num7);
		}
		else
		{
			newCOC.spcoc_ps = new int[num3 + 1];
			for (int num8 = num3; num8 >= 0; num8--)
			{
				num5 = (newCOC.spcoc_ps[num8] = ehs.ReadByte());
				int num9 = 1 << (num5 & 0xF);
				array4[0].Insert(0, num9);
				int num10 = 1 << ((num5 & 0xF0) >> 4);
				array4[1].Insert(0, num10);
			}
		}
		if (mainh)
		{
			decSpec.pss.setCompDef(num, array4);
		}
		else
		{
			decSpec.pss.setTileCompVal(tileIdx, num, array4);
		}
		precinctPartitionIsUsed = true;
		checkMarkerLength(ehs, "COD marker");
		if (mainh)
		{
			hi.cocValue["main_c" + num] = newCOC;
			decSpec.wfs.setCompDef(num, value_Renamed);
			decSpec.dls.setCompDef(num, num3);
			decSpec.ecopts.setCompDef(num, num4);
		}
		else
		{
			hi.cocValue["t" + tileIdx + "_c" + num] = newCOC;
			decSpec.wfs.setTileCompVal(tileIdx, num, value_Renamed);
			decSpec.dls.setTileCompVal(tileIdx, num, num3);
			decSpec.ecopts.setTileCompVal(tileIdx, num, num4);
		}
	}

	private void readPOC(BinaryReader ehs, bool mainh, int t, int tpIdx)
	{
		bool flag = nComp >= 256;
		int num = 0;
		HeaderInformation.POC pOC;
		if (mainh || hi.pocValue["t" + t] == null)
		{
			pOC = hi.NewPOC;
		}
		else
		{
			pOC = (HeaderInformation.POC)hi.pocValue["t" + t];
			num = pOC.rspoc.Length;
		}
		pOC.lpoc = ehs.ReadUInt16();
		int num2 = (pOC.lpoc - 2) / (5 + (flag ? 4 : 2));
		int num3 = num + num2;
		int[][] array2;
		if (num != 0)
		{
			int[][] array = new int[num3][];
			for (int i = 0; i < num3; i++)
			{
				array[i] = new int[6];
			}
			array2 = array;
			int[] array3 = new int[num3];
			int[] array4 = new int[num3];
			int[] array5 = new int[num3];
			int[] array6 = new int[num3];
			int[] array7 = new int[num3];
			int[] array8 = new int[num3];
			int[][] array9 = (int[][])decSpec.pcs.getTileDef(t);
			for (int j = 0; j < num; j++)
			{
				array2[j] = array9[j];
				array3[j] = pOC.rspoc[j];
				array4[j] = pOC.cspoc[j];
				array5[j] = pOC.lyepoc[j];
				array6[j] = pOC.repoc[j];
				array7[j] = pOC.cepoc[j];
				array8[j] = pOC.ppoc[j];
			}
			pOC.rspoc = array3;
			pOC.cspoc = array4;
			pOC.lyepoc = array5;
			pOC.repoc = array6;
			pOC.cepoc = array7;
			pOC.ppoc = array8;
		}
		else
		{
			int[][] array10 = new int[num2][];
			for (int k = 0; k < num2; k++)
			{
				array10[k] = new int[6];
			}
			array2 = array10;
			pOC.rspoc = new int[num2];
			pOC.cspoc = new int[num2];
			pOC.lyepoc = new int[num2];
			pOC.repoc = new int[num2];
			pOC.cepoc = new int[num2];
			pOC.ppoc = new int[num2];
		}
		for (int l = num; l < num3; l++)
		{
			array2[l][0] = (pOC.rspoc[l] = ehs.ReadByte());
			if (flag)
			{
				array2[l][1] = (pOC.cspoc[l] = ehs.ReadUInt16());
			}
			else
			{
				array2[l][1] = (pOC.cspoc[l] = ehs.ReadByte());
			}
			array2[l][2] = (pOC.lyepoc[l] = ehs.ReadUInt16());
			if (array2[l][2] < 1)
			{
				throw new ArgumentException("LYEpoc value must be greater than 1 in POC marker segment of tile " + t + ", tile-part " + tpIdx);
			}
			array2[l][3] = (pOC.repoc[l] = ehs.ReadByte());
			if (array2[l][3] <= array2[l][0])
			{
				throw new ArgumentException("REpoc value must be greater than RSpoc in POC marker segment of tile " + t + ", tile-part " + tpIdx);
			}
			if (flag)
			{
				array2[l][4] = (pOC.cepoc[l] = ehs.ReadUInt16());
			}
			else
			{
				int num4 = (pOC.cepoc[l] = ehs.ReadByte());
				if (num4 == 0)
				{
					array2[l][4] = 0;
				}
				else
				{
					array2[l][4] = num4;
				}
			}
			if (array2[l][4] <= array2[l][1])
			{
				throw new ArgumentException("CEpoc value must be greater than CSpoc in POC marker segment of tile " + t + ", tile-part " + tpIdx);
			}
			array2[l][5] = (pOC.ppoc[l] = ehs.ReadByte());
		}
		checkMarkerLength(ehs, "POC marker");
		if (mainh)
		{
			hi.pocValue["main"] = pOC;
			decSpec.pcs.setDefault(array2);
		}
		else
		{
			hi.pocValue["t" + t] = pOC;
			decSpec.pcs.setTileDef(t, array2);
		}
	}

	private void readTLM(BinaryReader ehs)
	{
		int num = ehs.ReadUInt16();
		long position = ehs.BaseStream.Position;
		position = ehs.BaseStream.Seek(num - 2, SeekOrigin.Current) - position;
	}

	private void readPLM(BinaryReader ehs)
	{
		int num = ehs.ReadUInt16();
		long position = ehs.BaseStream.Position;
		position = ehs.BaseStream.Seek(num - 2, SeekOrigin.Current) - position;
	}

	private void readPLTFields(BinaryReader ehs)
	{
		int num = ehs.ReadUInt16();
		long position = ehs.BaseStream.Position;
		position = ehs.BaseStream.Seek(num - 2, SeekOrigin.Current) - position;
	}

	private void readRGN(BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
	{
		HeaderInformation.RGN newRGN = hi.NewRGN;
		newRGN.lrgn = ehs.ReadUInt16();
		int num = (newRGN.crgn = ((nComp < 257) ? ehs.ReadByte() : ehs.ReadUInt16()));
		if (num >= nComp)
		{
			throw new ArgumentException("Invalid component index in RGN marker" + num);
		}
		newRGN.srgn = ehs.ReadByte();
		if (newRGN.srgn != 0)
		{
			throw new ArgumentException("Unknown or unsupported Srgn parameter in ROI marker");
		}
		if (decSpec.rois == null)
		{
			decSpec.rois = new MaxShiftSpec(nTiles, nComp, 2);
		}
		newRGN.sprgn = ehs.ReadByte();
		if (mainh)
		{
			hi.rgnValue["main_c" + num] = newRGN;
			decSpec.rois.setCompDef(num, newRGN.sprgn);
		}
		else
		{
			hi.rgnValue["t" + tileIdx + "_c" + num] = newRGN;
			decSpec.rois.setTileCompVal(tileIdx, num, newRGN.sprgn);
		}
		checkMarkerLength(ehs, "RGN marker");
	}

	private void readPPM(BinaryReader ehs)
	{
		if (pPMMarkerData == null)
		{
			pPMMarkerData = new byte[nPPMMarkSeg][];
			tileOfTileParts = new List<object>(10);
			decSpec.pphs.setDefault(true);
		}
		int num = ehs.ReadUInt16() - 3;
		int num2 = ehs.ReadByte();
		pPMMarkerData[num2] = new byte[num];
		ehs.BaseStream.Read(pPMMarkerData[num2], 0, num);
		checkMarkerLength(ehs, "PPM marker");
	}

	private void readPPT(BinaryReader ehs, int tile, int tpIdx)
	{
		if (tilePartPkdPktHeaders == null)
		{
			tilePartPkdPktHeaders = new byte[nTiles][][][];
		}
		if (tilePartPkdPktHeaders[tile] == null)
		{
			tilePartPkdPktHeaders[tile] = new byte[nTileParts[tile]][][];
		}
		if (tilePartPkdPktHeaders[tile][tpIdx] == null)
		{
			tilePartPkdPktHeaders[tile][tpIdx] = new byte[nPPTMarkSeg[tile][tpIdx]][];
		}
		ushort num = ehs.ReadUInt16();
		int num2 = ehs.ReadByte();
		byte[] array = new byte[num - 3];
		ehs.BaseStream.Read(array, 0, array.Length);
		tilePartPkdPktHeaders[tile][tpIdx][num2] = array;
		checkMarkerLength(ehs, "PPT marker");
		decSpec.pphs.setTileDef(tile, true);
	}

	private void extractMainMarkSeg(short marker, JPXRandomAccessStream ehs)
	{
		if (nfMarkSeg == 0 && marker != -175)
		{
			throw new ArgumentException("First marker after SOC must be SIZ " + Convert.ToString(marker, 16));
		}
		string text = "";
		if (ht == null)
		{
			ht = new Dictionary<object, object>();
		}
		switch (marker)
		{
		case -175:
			if (((uint)nfMarkSeg & (true ? 1u : 0u)) != 0)
			{
				throw new ArgumentException("More than one SIZ marker segment found in main header");
			}
			nfMarkSeg |= 1;
			text = "SIZ";
			break;
		case -109:
			throw new ArgumentException("SOD found in main header");
		case -39:
			throw new ArgumentException("EOC found in main header");
		case -112:
			if (((uint)nfMarkSeg & 0x40u) != 0)
			{
				throw new ArgumentException("More than one SOT marker found right after main or tile header");
			}
			nfMarkSeg |= 64;
			return;
		case -174:
			if (((uint)nfMarkSeg & 2u) != 0)
			{
				throw new ArgumentException("More than one COD marker found in main header");
			}
			nfMarkSeg |= 2;
			text = "COD";
			break;
		case -173:
			nfMarkSeg |= 4;
			text = "COC" + nCOCMarkSeg++;
			break;
		case -164:
			if (((uint)nfMarkSeg & 8u) != 0)
			{
				throw new ArgumentException("More than one QCD marker found in main header");
			}
			nfMarkSeg |= 8;
			text = "QCD";
			break;
		case -163:
			nfMarkSeg |= 256;
			text = "QCC" + nQCCMarkSeg++;
			break;
		case -162:
			nfMarkSeg |= 512;
			text = "RGN" + nRGNMarkSeg++;
			break;
		case -156:
			nfMarkSeg |= 2048;
			text = "COM" + nCOMMarkSeg++;
			break;
		case -157:
			if (((uint)nfMarkSeg & 0x10000u) != 0)
			{
				throw new ArgumentException("More than one CRG marker found in main header");
			}
			nfMarkSeg |= 65536;
			text = "CRG";
			break;
		case -160:
			nfMarkSeg |= 16384;
			text = "PPM" + nPPMMarkSeg++;
			break;
		case -171:
			if (((uint)nfMarkSeg & 0x10u) != 0)
			{
				throw new ArgumentException("More than one TLM marker found in main header");
			}
			nfMarkSeg |= 16;
			break;
		case -169:
			if (((uint)nfMarkSeg & 0x20u) != 0)
			{
				throw new ArgumentException("More than one PLM marker found in main header");
			}
			nfMarkSeg |= 32;
			text = "PLM";
			break;
		case -161:
			if (((uint)nfMarkSeg & 0x400u) != 0)
			{
				throw new ArgumentException("More than one POC marker segment found in main header");
			}
			nfMarkSeg |= 1024;
			text = "POC";
			break;
		case -168:
			throw new ArgumentException("PLT found in main header");
		case -159:
			throw new ArgumentException("PPT found in main header");
		default:
			text = "UNKNOWN";
			break;
		}
		if (marker < -208 || marker > -193)
		{
			int num = ehs.readUnsignedShort();
			byte[] array = new byte[num];
			array[0] = (byte)((uint)(num >> 8) & 0xFFu);
			array[1] = (byte)((uint)num & 0xFFu);
			ehs.readFully(array, 2, num - 2);
			if (!text.Equals("UNKNOWN"))
			{
				ht[text] = array;
			}
		}
	}

	internal virtual void extractTilePartMarkSeg(short marker, JPXRandomAccessStream ehs, int tileIdx, int tilePartIdx)
	{
		string text = "";
		if (ht == null)
		{
			ht = new Dictionary<object, object>();
		}
		switch (marker)
		{
		case -112:
			throw new ArgumentException("Second SOT marker segment found in tile-part header");
		case -175:
			throw new ArgumentException("SIZ found in tile-part header");
		case -39:
			throw new ArgumentException("EOC found in tile-part header");
		case -171:
			throw new ArgumentException("TLM found in tile-part header");
		case -169:
			throw new ArgumentException("PLM found in tile-part header");
		case -160:
			throw new ArgumentException("PPM found in tile-part header");
		case -174:
			if (((uint)nfMarkSeg & 2u) != 0)
			{
				throw new ArgumentException("More than one COD marker found in tile-part header");
			}
			nfMarkSeg |= 2;
			text = "COD";
			break;
		case -173:
			nfMarkSeg |= 4;
			text = "COC" + nCOCMarkSeg++;
			break;
		case -164:
			if (((uint)nfMarkSeg & 8u) != 0)
			{
				throw new ArgumentException("More than one QCD marker found in tile-part header");
			}
			nfMarkSeg |= 8;
			text = "QCD";
			break;
		case -163:
			nfMarkSeg |= 256;
			text = "QCC" + nQCCMarkSeg++;
			break;
		case -162:
			nfMarkSeg |= 512;
			text = "RGN" + nRGNMarkSeg++;
			break;
		case -156:
			nfMarkSeg |= 2048;
			text = "COM" + nCOMMarkSeg++;
			break;
		case -157:
			throw new ArgumentException("CRG marker found in tile-part header");
		case -159:
			nfMarkSeg |= 32768;
			if (nPPTMarkSeg == null)
			{
				nPPTMarkSeg = new int[nTiles][];
			}
			if (nPPTMarkSeg[tileIdx] == null)
			{
				nPPTMarkSeg[tileIdx] = new int[nTileParts[tileIdx]];
			}
			text = "PPT" + nPPTMarkSeg[tileIdx][tilePartIdx]++;
			break;
		case -109:
			nfMarkSeg |= 8192;
			return;
		case -161:
			if (((uint)nfMarkSeg & 0x400u) != 0)
			{
				throw new ArgumentException("More than one POC marker segment found in tile-part header");
			}
			nfMarkSeg |= 1024;
			text = "POC";
			break;
		case -168:
			if (((uint)nfMarkSeg & 0x20u) != 0)
			{
				throw new ArgumentException("PLT marker found eventhough PLM marker found in main header");
			}
			text = "UNKNOWN";
			break;
		default:
			text = "UNKNOWN";
			break;
		}
		int num = ehs.readUnsignedShort();
		byte[] array = new byte[num];
		array[0] = (byte)((uint)(num >> 8) & 0xFFu);
		array[1] = (byte)((uint)num & 0xFFu);
		ehs.readFully(array, 2, num - 2);
		if (!text.Equals("UNKNOWN"))
		{
			ht[text] = array;
		}
	}

	private void readFoundMainMarkSeg()
	{
		if (((uint)nfMarkSeg & (true ? 1u : 0u)) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["SIZ"]);
			readSIZ(new EndianBinaryReader(input, bigEndian: true));
		}
		if (((uint)nfMarkSeg & 0x800u) != 0)
		{
			for (int i = 0; i < nCOMMarkSeg; i++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["COM" + i]);
				readCOM(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, i);
			}
		}
		if (((uint)nfMarkSeg & 0x10000u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["CRG"]);
			readCRG(new EndianBinaryReader(input, bigEndian: true));
		}
		if (((uint)nfMarkSeg & 2u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["COD"]);
			readCOD(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, 0);
		}
		if (((uint)nfMarkSeg & 4u) != 0)
		{
			for (int j = 0; j < nCOCMarkSeg; j++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["COC" + j]);
				readCOC(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, 0);
			}
		}
		if (((uint)nfMarkSeg & 0x200u) != 0)
		{
			for (int k = 0; k < nRGNMarkSeg; k++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["RGN" + k]);
				readRGN(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, 0);
			}
		}
		if (((uint)nfMarkSeg & 8u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["QCD"]);
			readQCD(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, 0);
		}
		if (((uint)nfMarkSeg & 0x100u) != 0)
		{
			for (int l = 0; l < nQCCMarkSeg; l++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["QCC" + l]);
				readQCC(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, 0);
			}
		}
		if (((uint)nfMarkSeg & 0x400u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["POC"]);
			readPOC(new EndianBinaryReader(input, bigEndian: true), mainh: true, 0, 0);
		}
		if (((uint)nfMarkSeg & 0x4000u) != 0)
		{
			for (int m = 0; m < nPPMMarkSeg; m++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["PPM" + m]);
				readPPM(new EndianBinaryReader(input));
			}
		}
		ht = null;
	}

	public virtual void readFoundTilePartMarkSeg(int tileIdx, int tpIdx)
	{
		if (((uint)nfMarkSeg & 2u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["COD"]);
			readCOD(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, tpIdx);
		}
		if (((uint)nfMarkSeg & 4u) != 0)
		{
			for (int i = 0; i < nCOCMarkSeg; i++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["COC" + i]);
				readCOC(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, tpIdx);
			}
		}
		if (((uint)nfMarkSeg & 0x200u) != 0)
		{
			for (int j = 0; j < nRGNMarkSeg; j++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["RGN" + j]);
				readRGN(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, tpIdx);
			}
		}
		if (((uint)nfMarkSeg & 8u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["QCD"]);
			readQCD(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, tpIdx);
		}
		if (((uint)nfMarkSeg & 0x100u) != 0)
		{
			for (int k = 0; k < nQCCMarkSeg; k++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["QCC" + k]);
				readQCC(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, tpIdx);
			}
		}
		if (((uint)nfMarkSeg & 0x400u) != 0)
		{
			MemoryStream input = new MemoryStream((byte[])ht["POC"]);
			readPOC(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, tpIdx);
		}
		if (((uint)nfMarkSeg & 0x800u) != 0)
		{
			for (int l = 0; l < nCOMMarkSeg; l++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["COM" + l]);
				readCOM(new EndianBinaryReader(input, bigEndian: true), mainh: false, tileIdx, l);
			}
		}
		if (((uint)nfMarkSeg & 0x8000u) != 0)
		{
			for (int m = 0; m < nPPTMarkSeg[tileIdx][tpIdx]; m++)
			{
				MemoryStream input = new MemoryStream((byte[])ht["PPT" + m]);
				readPPT(new EndianBinaryReader(input, bigEndian: true), tileIdx, tpIdx);
			}
		}
		ht = null;
	}

	internal HeaderDecoder(JPXRandomAccessStream ehs, JPXParameters pl, HeaderInformation hi)
	{
		this.hi = hi;
		pl.checkList('H', JPXParameters.toNameArray(pinfo));
		mainHeadOff = ehs.Pos;
		if (ehs.readShort() != -177)
		{
			throw new ArgumentException("SOC marker segment not  found at the beginning of the codestream.");
		}
		nfMarkSeg = 0;
		do
		{
			extractMainMarkSeg(ehs.readShort(), ehs);
		}
		while ((nfMarkSeg & 0x40) == 0);
		ehs.seek(ehs.Pos - 2);
		readFoundMainMarkSeg();
	}

	internal virtual EntropyDecoder createEntropyDecoder(CodedCBlkDataSrcDec src, JPXParameters pl)
	{
		pl.checkList('C', JPXParameters.toNameArray(EntropyDecoder.ParameterInfo));
		bool booleanParameter = pl.getBooleanParameter("Cer");
		bool booleanParameter2 = pl.getBooleanParameter("Cverber");
		int intParameter = pl.getIntParameter("m_quit");
		return new StdEntropyDecoder(src, decSpec, booleanParameter, booleanParameter2, intParameter);
	}

	internal virtual DeScalerROI createROIDeScaler(CBlkQuantDataSrcDec src, JPXParameters pl, DecodeHelper decSpec2)
	{
		return DeScalerROI.createInstance(src, pl, decSpec2);
	}

	public virtual void resetHeaderMarkers()
	{
		nfMarkSeg &= 16416;
		nCOCMarkSeg = 0;
		nQCCMarkSeg = 0;
		nCOMMarkSeg = 0;
		nRGNMarkSeg = 0;
	}

	public override string ToString()
	{
		return hdStr;
	}

	public virtual MemoryStream getPackedPktHead(int tile)
	{
		if (pkdPktHeaders == null)
		{
			pkdPktHeaders = new MemoryStream[nTiles];
			for (int num = nTiles - 1; num >= 0; num--)
			{
				pkdPktHeaders[num] = new MemoryStream();
			}
			if (nPPMMarkSeg != 0)
			{
				int count = tileOfTileParts.Count;
				MemoryStream memoryStream = new MemoryStream();
				for (int num = 0; num < nPPMMarkSeg; num++)
				{
					byte[] array = pPMMarkerData[num];
					memoryStream.Write(array, 0, array.Length);
				}
				MemoryStream memoryStream2 = new MemoryStream(memoryStream.ToArray());
				for (int num = 0; num < count; num++)
				{
					int num2 = (int)tileOfTileParts[num];
					byte[] array2 = new byte[(memoryStream2.ReadByte() << 24) | (memoryStream2.ReadByte() << 16) | (memoryStream2.ReadByte() << 8) | memoryStream2.ReadByte()];
					memoryStream2.Read(array2, 0, array2.Length);
					byte[] array3 = array2;
					pkdPktHeaders[num2].Write(array3, 0, array3.Length);
				}
			}
			else
			{
				for (int num2 = nTiles - 1; num2 >= 0; num2--)
				{
					for (int i = 0; i < nTileParts[num2]; i++)
					{
						for (int num = 0; num < nPPTMarkSeg[num2][i]; num++)
						{
							byte[] array4 = tilePartPkdPktHeaders[num2][i][num];
							pkdPktHeaders[num2].Write(array4, 0, array4.Length);
						}
					}
				}
			}
		}
		return new MemoryStream(pkdPktHeaders[tile].ToArray());
	}
}
