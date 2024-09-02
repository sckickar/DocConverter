using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf;

internal class PktDecoder
{
	private BitstreamReader src;

	private bool pph;

	private MemoryStream pphbais;

	private DecodeHelper decSpec;

	private HeaderDecoder hd;

	private int INIT_LBLOCK = 3;

	private PktHeaderBitReader bin;

	private JPXRandomAccessStream ehs;

	private JPXImageCoordinates[][] numPrec;

	private int tIdx;

	private PrecInfo[][][] ppinfo;

	private int[][][][][] lblock;

	private TagTreeDecoder[][][][] ttIncl;

	private TagTreeDecoder[][][][] ttMaxBP;

	private int nl;

	private int nc;

	private bool sopUsed;

	private bool ephUsed;

	private int pktIdx;

	private List<object>[] cblks;

	private int ncb;

	private int maxCB;

	private bool ncbQuit;

	private int tQuit;

	private int cQuit;

	private int sQuit;

	private int rQuit;

	private int xQuit;

	private int yQuit;

	private bool isTruncMode;

	internal PktDecoder(DecodeHelper decSpec, HeaderDecoder hd, JPXRandomAccessStream ehs, BitstreamReader src, bool isTruncMode, int maxCB)
	{
		this.decSpec = decSpec;
		this.hd = hd;
		this.ehs = ehs;
		this.isTruncMode = isTruncMode;
		bin = new PktHeaderBitReader(ehs);
		this.src = src;
		ncb = 0;
		ncbQuit = false;
		this.maxCB = maxCB;
	}

	internal virtual CBlkInfo[][][][][] restart(int nc, int[] mdl, int nl, CBlkInfo[][][][][] cbI, bool pph, MemoryStream pphbais)
	{
		this.nc = nc;
		this.nl = nl;
		tIdx = src.TileIdx;
		this.pph = pph;
		this.pphbais = pphbais;
		sopUsed = (bool)decSpec.sops.getTileDef(tIdx);
		pktIdx = 0;
		ephUsed = (bool)decSpec.ephs.getTileDef(tIdx);
		cbI = new CBlkInfo[nc][][][][];
		lblock = new int[nc][][][][];
		ttIncl = new TagTreeDecoder[nc][][][];
		ttMaxBP = new TagTreeDecoder[nc][][][];
		numPrec = new JPXImageCoordinates[nc][];
		ppinfo = new PrecInfo[nc][][];
		JPXImageCoordinates jPXImageCoordinates = null;
		int cbULX = src.CbULX;
		int cbULY = src.CbULY;
		for (int i = 0; i < nc; i++)
		{
			cbI[i] = new CBlkInfo[mdl[i] + 1][][][];
			lblock[i] = new int[mdl[i] + 1][][][];
			ttIncl[i] = new TagTreeDecoder[mdl[i] + 1][][];
			ttMaxBP[i] = new TagTreeDecoder[mdl[i] + 1][][];
			numPrec[i] = new JPXImageCoordinates[mdl[i] + 1];
			ppinfo[i] = new PrecInfo[mdl[i] + 1][];
			int resULX = src.getResULX(i, mdl[i]);
			int resULY = src.getResULY(i, mdl[i]);
			int num = resULX + src.getTileCompWidth(tIdx, i, mdl[i]);
			int num2 = resULY + src.getTileCompHeight(tIdx, i, mdl[i]);
			for (int j = 0; j <= mdl[i]; j++)
			{
				int num3 = (int)Math.Ceiling((double)resULX / (double)(1 << mdl[i] - j));
				int num4 = (int)Math.Ceiling((double)resULY / (double)(1 << mdl[i] - j));
				int num5 = (int)Math.Ceiling((double)num / (double)(1 << mdl[i] - j));
				int num6 = (int)Math.Ceiling((double)num2 / (double)(1 << mdl[i] - j));
				double num7 = getPPX(tIdx, i, j);
				double num8 = getPPY(tIdx, i, j);
				numPrec[i][j] = new JPXImageCoordinates();
				if (num5 > num3)
				{
					numPrec[i][j].x = (int)Math.Ceiling((double)(num5 - cbULX) / num7) - (int)Math.Floor((double)(num3 - cbULX) / num7);
				}
				else
				{
					numPrec[i][j].x = 0;
				}
				if (num6 > num4)
				{
					numPrec[i][j].y = (int)Math.Ceiling((double)(num6 - cbULY) / num8) - (int)Math.Floor((double)(num4 - cbULY) / num8);
				}
				else
				{
					numPrec[i][j].y = 0;
				}
				int num9 = ((j != 0) ? 1 : 0);
				int num10 = ((j == 0) ? 1 : 4);
				int num11 = numPrec[i][j].x * numPrec[i][j].y;
				ttIncl[i][j] = new TagTreeDecoder[num11][];
				for (int k = 0; k < num11; k++)
				{
					ttIncl[i][j][k] = new TagTreeDecoder[num10 + 1];
				}
				ttMaxBP[i][j] = new TagTreeDecoder[num11][];
				for (int l = 0; l < num11; l++)
				{
					ttMaxBP[i][j][l] = new TagTreeDecoder[num10 + 1];
				}
				cbI[i][j] = new CBlkInfo[num10 + 1][][];
				lblock[i][j] = new int[num10 + 1][][];
				ppinfo[i][j] = new PrecInfo[num11];
				fillPrecInfo(i, j, mdl[i]);
				SubbandSyn synSubbandTree = src.getSynSubbandTree(tIdx, i);
				for (int m = num9; m < num10; m++)
				{
					jPXImageCoordinates = ((SubbandSyn)synSubbandTree.getSubbandByIdx(j, m)).numCb;
					cbI[i][j][m] = new CBlkInfo[jPXImageCoordinates.y][];
					for (int n = 0; n < jPXImageCoordinates.y; n++)
					{
						cbI[i][j][m][n] = new CBlkInfo[jPXImageCoordinates.x];
					}
					lblock[i][j][m] = new int[jPXImageCoordinates.y][];
					for (int num12 = 0; num12 < jPXImageCoordinates.y; num12++)
					{
						lblock[i][j][m][num12] = new int[jPXImageCoordinates.x];
					}
					for (int num13 = jPXImageCoordinates.y - 1; num13 >= 0; num13--)
					{
						ArrayUtil.intArraySet(lblock[i][j][m][num13], INIT_LBLOCK);
					}
				}
			}
		}
		return cbI;
	}

	private void fillPrecInfo(int c, int r, int mdl)
	{
		if (ppinfo[c][r].Length == 0)
		{
			return;
		}
		JPXImageCoordinates tile = src.getTile(null);
		JPXImageCoordinates numTiles = src.getNumTiles(null);
		int tilePartULX = src.TilePartULX;
		int tilePartULY = src.TilePartULY;
		int nomTileWidth = src.NomTileWidth;
		int nomTileHeight = src.NomTileHeight;
		int imgULX = hd.ImgULX;
		int imgULY = hd.ImgULY;
		int imgWidth = hd.ImgWidth;
		int imgHeight = hd.ImgHeight;
		int num = ((tile.x == 0) ? imgULX : (tilePartULX + tile.x * nomTileWidth));
		int num2 = ((tile.y == 0) ? imgULY : (tilePartULY + tile.y * nomTileHeight));
		if (tile.x != numTiles.x - 1)
		{
			_ = tile.x;
		}
		if (tile.y != numTiles.y - 1)
		{
			_ = tile.y;
		}
		int compSubsX = hd.getCompSubsX(c);
		int compSubsY = hd.getCompSubsY(c);
		int resULX = src.getResULX(c, mdl);
		int resULY = src.getResULY(c, mdl);
		int num3 = resULX + src.getTileCompWidth(tIdx, c, mdl);
		int num4 = resULY + src.getTileCompHeight(tIdx, c, mdl);
		int num5 = mdl - r;
		int num6 = (int)Math.Ceiling((double)resULX / (double)(1 << num5));
		int num7 = (int)Math.Ceiling((double)resULY / (double)(1 << num5));
		int num8 = (int)Math.Ceiling((double)num3 / (double)(1 << num5));
		int num9 = (int)Math.Ceiling((double)num4 / (double)(1 << num5));
		int cbULX = src.CbULX;
		int cbULY = src.CbULY;
		double num10 = getPPX(tIdx, c, r);
		double num11 = getPPY(tIdx, c, r);
		int num12 = (int)(num10 / 2.0);
		int num13 = (int)(num11 / 2.0);
		_ = ppinfo[c][r].Length;
		int num14 = 0;
		int num15 = (int)Math.Floor((double)(num7 - cbULY) / num11);
		int num16 = (int)Math.Floor((double)(num9 - 1 - cbULY) / num11);
		int num17 = (int)Math.Floor((double)(num6 - cbULX) / num10);
		int num18 = (int)Math.Floor((double)(num8 - 1 - cbULX) / num10);
		SubbandSyn synSubbandTree = src.getSynSubbandTree(tIdx, c);
		SubbandSyn subbandSyn = null;
		int rgw = (int)num10 << num5;
		int rgh = (int)num11 << num5;
		for (int i = num15; i <= num16; i++)
		{
			int num19 = num17;
			while (num19 <= num18)
			{
				int rgulx = ((num19 != num17 || (num6 - cbULX) % (compSubsX * (int)num10) == 0) ? (cbULX + num19 * compSubsX * ((int)num10 << num5)) : num);
				int rguly = ((i != num15 || (num7 - cbULY) % (compSubsY * (int)num11) == 0) ? (cbULY + i * compSubsY * ((int)num11 << num5)) : num2);
				ppinfo[c][r][num14] = new PrecInfo(r, (int)((double)cbULX + (double)num19 * num10), (int)((double)cbULY + (double)i * num11), (int)num10, (int)num11, rgulx, rguly, rgw, rgh);
				if (r == 0)
				{
					int num20 = cbULX;
					int num21 = cbULY;
					int num22 = num20 + num19 * (int)num10;
					int num23 = num22 + (int)num10;
					int num24 = num21 + i * (int)num11;
					int num25 = num24 + (int)num11;
					subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(0, 0);
					int num26 = ((num22 < subbandSyn.ulcx) ? subbandSyn.ulcx : num22);
					int num27 = ((num23 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num23);
					int num28 = ((num24 < subbandSyn.ulcy) ? subbandSyn.ulcy : num24);
					int num29 = ((num25 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num25);
					int nomCBlkW = subbandSyn.nomCBlkW;
					int nomCBlkH = subbandSyn.nomCBlkH;
					int num30 = (int)Math.Floor((double)(subbandSyn.ulcy - num21) / (double)nomCBlkH);
					int num31 = (int)Math.Floor((double)(num28 - num21) / (double)nomCBlkH);
					int num32 = (int)Math.Floor((double)(num29 - 1 - num21) / (double)nomCBlkH);
					int num33 = (int)Math.Floor((double)(subbandSyn.ulcx - num20) / (double)nomCBlkW);
					int num34 = (int)Math.Floor((double)(num26 - num20) / (double)nomCBlkW);
					int num35 = (int)Math.Floor((double)(num27 - 1 - num20) / (double)nomCBlkW);
					if (num27 - num26 <= 0 || num29 - num28 <= 0)
					{
						ppinfo[c][r][num14].nblk[0] = 0;
						ttIncl[c][r][num14][0] = new TagTreeDecoder(0, 0);
						ttMaxBP[c][r][num14][0] = new TagTreeDecoder(0, 0);
					}
					else
					{
						ttIncl[c][r][num14][0] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						ttMaxBP[c][r][num14][0] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						CBlkCoordInfo[][] array = new CBlkCoordInfo[num32 - num31 + 1][];
						for (int j = 0; j < num32 - num31 + 1; j++)
						{
							array[j] = new CBlkCoordInfo[num35 - num34 + 1];
						}
						ppinfo[c][r][num14].cblk[0] = array;
						ppinfo[c][r][num14].nblk[0] = (num32 - num31 + 1) * (num35 - num34 + 1);
						for (int k = num31; k <= num32; k++)
						{
							for (int l = num34; l <= num35; l++)
							{
								CBlkCoordInfo cBlkCoordInfo = new CBlkCoordInfo(k - num30, l - num33);
								if (l == num33)
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx;
								}
								else
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx + l * nomCBlkW - (subbandSyn.ulcx - num20);
								}
								if (k == num30)
								{
									cBlkCoordInfo.uly = subbandSyn.uly;
								}
								else
								{
									cBlkCoordInfo.uly = subbandSyn.uly + k * nomCBlkH - (subbandSyn.ulcy - num21);
								}
								int num36 = num20 + l * nomCBlkW;
								num36 = ((num36 > subbandSyn.ulcx) ? num36 : subbandSyn.ulcx);
								int num37 = num20 + (l + 1) * nomCBlkW;
								num37 = ((num37 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num37);
								cBlkCoordInfo.w = num37 - num36;
								num36 = num21 + k * nomCBlkH;
								num36 = ((num36 > subbandSyn.ulcy) ? num36 : subbandSyn.ulcy);
								num37 = num21 + (k + 1) * nomCBlkH;
								num37 = ((num37 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num37);
								cBlkCoordInfo.h = num37 - num36;
								ppinfo[c][r][num14].cblk[0][k - num31][l - num34] = cBlkCoordInfo;
							}
						}
					}
				}
				else
				{
					int num20 = 0;
					int num21 = cbULY;
					int num22 = num20 + num19 * num12;
					int num23 = num22 + num12;
					int num24 = num21 + i * num13;
					int num25 = num24 + num13;
					subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(r, 1);
					int num26 = ((num22 < subbandSyn.ulcx) ? subbandSyn.ulcx : num22);
					int num38 = ((num23 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num23);
					int num28 = ((num24 < subbandSyn.ulcy) ? subbandSyn.ulcy : num24);
					int num29 = ((num25 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num25);
					int nomCBlkW = subbandSyn.nomCBlkW;
					int nomCBlkH = subbandSyn.nomCBlkH;
					int num30 = (int)Math.Floor((double)(subbandSyn.ulcy - num21) / (double)nomCBlkH);
					int num31 = (int)Math.Floor((double)(num28 - num21) / (double)nomCBlkH);
					int num32 = (int)Math.Floor((double)(num29 - 1 - num21) / (double)nomCBlkH);
					int num33 = (int)Math.Floor((double)(subbandSyn.ulcx - num20) / (double)nomCBlkW);
					int num34 = (int)Math.Floor((double)(num26 - num20) / (double)nomCBlkW);
					int num35 = (int)Math.Floor((double)(num38 - 1 - num20) / (double)nomCBlkW);
					if (num38 - num26 <= 0 || num29 - num28 <= 0)
					{
						ppinfo[c][r][num14].nblk[1] = 0;
						ttIncl[c][r][num14][1] = new TagTreeDecoder(0, 0);
						ttMaxBP[c][r][num14][1] = new TagTreeDecoder(0, 0);
					}
					else
					{
						ttIncl[c][r][num14][1] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						ttMaxBP[c][r][num14][1] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						CBlkCoordInfo[][] array2 = new CBlkCoordInfo[num32 - num31 + 1][];
						for (int m = 0; m < num32 - num31 + 1; m++)
						{
							array2[m] = new CBlkCoordInfo[num35 - num34 + 1];
						}
						ppinfo[c][r][num14].cblk[1] = array2;
						ppinfo[c][r][num14].nblk[1] = (num32 - num31 + 1) * (num35 - num34 + 1);
						for (int n = num31; n <= num32; n++)
						{
							for (int num39 = num34; num39 <= num35; num39++)
							{
								CBlkCoordInfo cBlkCoordInfo = new CBlkCoordInfo(n - num30, num39 - num33);
								if (num39 == num33)
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx;
								}
								else
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx + num39 * nomCBlkW - (subbandSyn.ulcx - num20);
								}
								if (n == num30)
								{
									cBlkCoordInfo.uly = subbandSyn.uly;
								}
								else
								{
									cBlkCoordInfo.uly = subbandSyn.uly + n * nomCBlkH - (subbandSyn.ulcy - num21);
								}
								int num36 = num20 + num39 * nomCBlkW;
								num36 = ((num36 > subbandSyn.ulcx) ? num36 : subbandSyn.ulcx);
								int num37 = num20 + (num39 + 1) * nomCBlkW;
								num37 = ((num37 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num37);
								cBlkCoordInfo.w = num37 - num36;
								num36 = num21 + n * nomCBlkH;
								num36 = ((num36 > subbandSyn.ulcy) ? num36 : subbandSyn.ulcy);
								num37 = num21 + (n + 1) * nomCBlkH;
								num37 = ((num37 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num37);
								cBlkCoordInfo.h = num37 - num36;
								ppinfo[c][r][num14].cblk[1][n - num31][num39 - num34] = cBlkCoordInfo;
							}
						}
					}
					num20 = cbULX;
					num21 = 0;
					num22 = num20 + num19 * num12;
					num23 = num22 + num12;
					num24 = num21 + i * num13;
					num25 = num24 + num13;
					subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(r, 2);
					num26 = ((num22 < subbandSyn.ulcx) ? subbandSyn.ulcx : num22);
					int num40 = ((num23 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num23);
					num28 = ((num24 < subbandSyn.ulcy) ? subbandSyn.ulcy : num24);
					num29 = ((num25 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num25);
					nomCBlkW = subbandSyn.nomCBlkW;
					nomCBlkH = subbandSyn.nomCBlkH;
					num30 = (int)Math.Floor((double)(subbandSyn.ulcy - num21) / (double)nomCBlkH);
					num31 = (int)Math.Floor((double)(num28 - num21) / (double)nomCBlkH);
					num32 = (int)Math.Floor((double)(num29 - 1 - num21) / (double)nomCBlkH);
					num33 = (int)Math.Floor((double)(subbandSyn.ulcx - num20) / (double)nomCBlkW);
					num34 = (int)Math.Floor((double)(num26 - num20) / (double)nomCBlkW);
					num35 = (int)Math.Floor((double)(num40 - 1 - num20) / (double)nomCBlkW);
					if (num40 - num26 <= 0 || num29 - num28 <= 0)
					{
						ppinfo[c][r][num14].nblk[2] = 0;
						ttIncl[c][r][num14][2] = new TagTreeDecoder(0, 0);
						ttMaxBP[c][r][num14][2] = new TagTreeDecoder(0, 0);
					}
					else
					{
						ttIncl[c][r][num14][2] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						ttMaxBP[c][r][num14][2] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						CBlkCoordInfo[][] array3 = new CBlkCoordInfo[num32 - num31 + 1][];
						for (int num41 = 0; num41 < num32 - num31 + 1; num41++)
						{
							array3[num41] = new CBlkCoordInfo[num35 - num34 + 1];
						}
						ppinfo[c][r][num14].cblk[2] = array3;
						ppinfo[c][r][num14].nblk[2] = (num32 - num31 + 1) * (num35 - num34 + 1);
						for (int num42 = num31; num42 <= num32; num42++)
						{
							for (int num43 = num34; num43 <= num35; num43++)
							{
								CBlkCoordInfo cBlkCoordInfo = new CBlkCoordInfo(num42 - num30, num43 - num33);
								if (num43 == num33)
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx;
								}
								else
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx + num43 * nomCBlkW - (subbandSyn.ulcx - num20);
								}
								if (num42 == num30)
								{
									cBlkCoordInfo.uly = subbandSyn.uly;
								}
								else
								{
									cBlkCoordInfo.uly = subbandSyn.uly + num42 * nomCBlkH - (subbandSyn.ulcy - num21);
								}
								int num36 = num20 + num43 * nomCBlkW;
								num36 = ((num36 > subbandSyn.ulcx) ? num36 : subbandSyn.ulcx);
								int num37 = num20 + (num43 + 1) * nomCBlkW;
								num37 = ((num37 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num37);
								cBlkCoordInfo.w = num37 - num36;
								num36 = num21 + num42 * nomCBlkH;
								num36 = ((num36 > subbandSyn.ulcy) ? num36 : subbandSyn.ulcy);
								num37 = num21 + (num42 + 1) * nomCBlkH;
								num37 = ((num37 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num37);
								cBlkCoordInfo.h = num37 - num36;
								ppinfo[c][r][num14].cblk[2][num42 - num31][num43 - num34] = cBlkCoordInfo;
							}
						}
					}
					num20 = 0;
					num21 = 0;
					num22 = num20 + num19 * num12;
					num23 = num22 + num12;
					num24 = num21 + i * num13;
					num25 = num24 + num13;
					subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(r, 3);
					num26 = ((num22 < subbandSyn.ulcx) ? subbandSyn.ulcx : num22);
					int num44 = ((num23 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num23);
					num28 = ((num24 < subbandSyn.ulcy) ? subbandSyn.ulcy : num24);
					num29 = ((num25 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num25);
					nomCBlkW = subbandSyn.nomCBlkW;
					nomCBlkH = subbandSyn.nomCBlkH;
					num30 = (int)Math.Floor((double)(subbandSyn.ulcy - num21) / (double)nomCBlkH);
					num31 = (int)Math.Floor((double)(num28 - num21) / (double)nomCBlkH);
					num32 = (int)Math.Floor((double)(num29 - 1 - num21) / (double)nomCBlkH);
					num33 = (int)Math.Floor((double)(subbandSyn.ulcx - num20) / (double)nomCBlkW);
					num34 = (int)Math.Floor((double)(num26 - num20) / (double)nomCBlkW);
					num35 = (int)Math.Floor((double)(num44 - 1 - num20) / (double)nomCBlkW);
					if (num44 - num26 <= 0 || num29 - num28 <= 0)
					{
						ppinfo[c][r][num14].nblk[3] = 0;
						ttIncl[c][r][num14][3] = new TagTreeDecoder(0, 0);
						ttMaxBP[c][r][num14][3] = new TagTreeDecoder(0, 0);
					}
					else
					{
						ttIncl[c][r][num14][3] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						ttMaxBP[c][r][num14][3] = new TagTreeDecoder(num32 - num31 + 1, num35 - num34 + 1);
						CBlkCoordInfo[][] array4 = new CBlkCoordInfo[num32 - num31 + 1][];
						for (int num45 = 0; num45 < num32 - num31 + 1; num45++)
						{
							array4[num45] = new CBlkCoordInfo[num35 - num34 + 1];
						}
						ppinfo[c][r][num14].cblk[3] = array4;
						ppinfo[c][r][num14].nblk[3] = (num32 - num31 + 1) * (num35 - num34 + 1);
						for (int num46 = num31; num46 <= num32; num46++)
						{
							for (int num47 = num34; num47 <= num35; num47++)
							{
								CBlkCoordInfo cBlkCoordInfo = new CBlkCoordInfo(num46 - num30, num47 - num33);
								if (num47 == num33)
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx;
								}
								else
								{
									cBlkCoordInfo.ulx = subbandSyn.ulx + num47 * nomCBlkW - (subbandSyn.ulcx - num20);
								}
								if (num46 == num30)
								{
									cBlkCoordInfo.uly = subbandSyn.uly;
								}
								else
								{
									cBlkCoordInfo.uly = subbandSyn.uly + num46 * nomCBlkH - (subbandSyn.ulcy - num21);
								}
								int num36 = num20 + num47 * nomCBlkW;
								num36 = ((num36 > subbandSyn.ulcx) ? num36 : subbandSyn.ulcx);
								int num37 = num20 + (num47 + 1) * nomCBlkW;
								num37 = ((num37 > subbandSyn.ulcx + subbandSyn.w) ? (subbandSyn.ulcx + subbandSyn.w) : num37);
								cBlkCoordInfo.w = num37 - num36;
								num36 = num21 + num46 * nomCBlkH;
								num36 = ((num36 > subbandSyn.ulcy) ? num36 : subbandSyn.ulcy);
								num37 = num21 + (num46 + 1) * nomCBlkH;
								num37 = ((num37 > subbandSyn.ulcy + subbandSyn.h) ? (subbandSyn.ulcy + subbandSyn.h) : num37);
								cBlkCoordInfo.h = num37 - num36;
								ppinfo[c][r][num14].cblk[3][num46 - num31][num47 - num34] = cBlkCoordInfo;
							}
						}
					}
				}
				num19++;
				num14++;
			}
		}
	}

	public virtual int getNumPrecinct(int c, int r)
	{
		return numPrec[c][r].x * numPrec[c][r].y;
	}

	public virtual bool readPktHead(int l, int r, int c, int p, CBlkInfo[][][] cbI, int[] nb)
	{
		int num = 0;
		int pos = ehs.Pos;
		if (pos >= ehs.length())
		{
			return true;
		}
		int tileIdx = src.TileIdx;
		SubbandSyn synSubbandTree = src.getSynSubbandTree(tileIdx, c);
		PktHeaderBitReader pktHeaderBitReader = ((!pph) ? bin : new PktHeaderBitReader(pphbais));
		int num2 = ((r != 0) ? 1 : 0);
		int num3 = ((r == 0) ? 1 : 4);
		bool flag = false;
		for (int i = num2; i < num3; i++)
		{
			if (p < ppinfo[c][r].Length)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		PrecInfo precInfo = ppinfo[c][r][p];
		pktHeaderBitReader.sync();
		if (pktHeaderBitReader.readBit() == 0)
		{
			cblks = new List<object>[num3 + 1];
			for (int j = num2; j < num3; j++)
			{
				cblks[j] = new List<object>(10);
			}
			pktIdx++;
			if (isTruncMode && maxCB == -1)
			{
				int num4 = ehs.Pos - pos;
				if (num4 > nb[tileIdx])
				{
					nb[tileIdx] = 0;
					return true;
				}
				nb[tileIdx] -= num4;
			}
			if (ephUsed)
			{
				readEPHMarker(pktHeaderBitReader);
			}
			return false;
		}
		if (cblks == null || cblks.Length < num3 + 1)
		{
			cblks = new List<object>[num3 + 1];
		}
		for (int k = num2; k < num3; k++)
		{
			if (cblks[k] == null)
			{
				cblks[k] = new List<object>(10);
			}
			else
			{
				cblks[k].Clear();
			}
			SubbandSyn subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(r, k);
			if (precInfo.nblk[k] == 0)
			{
				continue;
			}
			TagTreeDecoder tagTreeDecoder = ttIncl[c][r][p][k];
			TagTreeDecoder tagTreeDecoder2 = ttMaxBP[c][r][p][k];
			int num5 = ((precInfo.cblk[k] != null) ? precInfo.cblk[k].Length : 0);
			for (int m = 0; m < num5; m++)
			{
				int num6 = ((precInfo.cblk[k][m] != null) ? precInfo.cblk[k][m].Length : 0);
				for (int n = 0; n < num6; n++)
				{
					JPXImageCoordinates idx = precInfo.cblk[k][m][n].idx;
					_ = idx.x;
					_ = idx.y;
					_ = subbandSyn.numCb.x;
					CBlkInfo cBlkInfo = cbI[k][idx.y][idx.x];
					try
					{
						int num8;
						int num4;
						if (cBlkInfo == null || cBlkInfo.ctp == 0)
						{
							if (cBlkInfo == null)
							{
								cBlkInfo = (cbI[k][idx.y][idx.x] = new CBlkInfo(precInfo.cblk[k][m][n].ulx, precInfo.cblk[k][m][n].uly, precInfo.cblk[k][m][n].w, precInfo.cblk[k][m][n].h, nl));
							}
							cBlkInfo.pktIdx[l] = pktIdx;
							num4 = tagTreeDecoder.update(m, n, l + 1, pktHeaderBitReader);
							if (num4 <= l)
							{
								num4 = 1;
								int num7;
								for (num7 = 1; num4 >= num7; num7++)
								{
									num4 = tagTreeDecoder2.update(m, n, num7, pktHeaderBitReader);
								}
								cBlkInfo.msbSkipped = num7 - 2;
								num8 = 1;
								cBlkInfo.addNTP(l, 0);
								ncb++;
								if (maxCB != -1 && !ncbQuit && ncb == maxCB)
								{
									ncbQuit = true;
									tQuit = tileIdx;
									cQuit = c;
									sQuit = k;
									rQuit = r;
									xQuit = idx.x;
									yQuit = idx.y;
								}
								goto IL_0402;
							}
						}
						else
						{
							cBlkInfo.pktIdx[l] = pktIdx;
							if (pktHeaderBitReader.readBit() == 1)
							{
								num8 = 1;
								goto IL_0402;
							}
						}
						goto end_IL_0285;
						IL_0402:
						if (pktHeaderBitReader.readBit() == 1)
						{
							num8++;
							if (pktHeaderBitReader.readBit() == 1)
							{
								num8++;
								num4 = pktHeaderBitReader.readBits(2);
								num8 += num4;
								if (num4 == 3)
								{
									num4 = pktHeaderBitReader.readBits(5);
									num8 += num4;
									if (num4 == 31)
									{
										num8 += pktHeaderBitReader.readBits(7);
									}
								}
							}
						}
						cBlkInfo.addNTP(l, num8);
						num += num8;
						cblks[k].Add(precInfo.cblk[k][m][n]);
						int num9 = (int)decSpec.ecopts.getTileCompVal(tileIdx, c);
						int num10;
						if ((num9 & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
						{
							num10 = num8;
						}
						else if ((num9 & StdEntropyCoderOptions.OPT_BYPASS) != 0)
						{
							if (cBlkInfo.ctp <= StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX)
							{
								num10 = 1;
							}
							else
							{
								num10 = 1;
								for (int num11 = cBlkInfo.ctp - num8; num11 < cBlkInfo.ctp - 1; num11++)
								{
									if (num11 >= StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1)
									{
										int num12 = (num11 + StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % StdEntropyCoderOptions.NUM_PASSES;
										if (num12 == 1 || num12 == 2)
										{
											num10++;
										}
									}
								}
							}
						}
						else
						{
							num10 = 1;
						}
						while (pktHeaderBitReader.readBit() != 0)
						{
							lblock[c][r][k][idx.y][idx.x]++;
						}
						int num13;
						if (num10 == 1)
						{
							num13 = pktHeaderBitReader.readBits(lblock[c][r][k][idx.y][idx.x] + MathUtil.log2(num8));
						}
						else
						{
							cBlkInfo.segLen[l] = new int[num10];
							num13 = 0;
							if ((num9 & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
							{
								int num11 = cBlkInfo.ctp - num8;
								int num14 = 0;
								while (num11 < cBlkInfo.ctp)
								{
									int n2 = lblock[c][r][k][idx.y][idx.x];
									num4 = pktHeaderBitReader.readBits(n2);
									cBlkInfo.segLen[l][num14] = num4;
									num13 += num4;
									num11++;
									num14++;
								}
							}
							else
							{
								int num15 = cBlkInfo.ctp - num8 - 1;
								int num11 = cBlkInfo.ctp - num8;
								int num14 = 0;
								int n2;
								for (; num11 < cBlkInfo.ctp - 1; num11++)
								{
									if (num11 >= StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1 && (num11 + StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % StdEntropyCoderOptions.NUM_PASSES != 0)
									{
										n2 = lblock[c][r][k][idx.y][idx.x];
										num4 = pktHeaderBitReader.readBits(n2 + MathUtil.log2(num11 - num15));
										cBlkInfo.segLen[l][num14] = num4;
										num13 += num4;
										num15 = num11;
										num14++;
									}
								}
								n2 = lblock[c][r][k][idx.y][idx.x];
								num4 = pktHeaderBitReader.readBits(n2 + MathUtil.log2(num11 - num15));
								num13 += num4;
								cBlkInfo.segLen[l][num14] = num4;
							}
						}
						cBlkInfo.len[l] = num13;
						if (!isTruncMode || maxCB != -1)
						{
							continue;
						}
						num4 = ehs.Pos - pos;
						if (num4 > nb[tileIdx])
						{
							nb[tileIdx] = 0;
							if (l == 0)
							{
								cbI[k][idx.y][idx.x] = null;
							}
							else
							{
								cBlkInfo.off[l] = (cBlkInfo.len[l] = 0);
								cBlkInfo.ctp -= cBlkInfo.ntp[l];
								cBlkInfo.ntp[l] = 0;
								cBlkInfo.pktIdx[l] = -1;
							}
							return true;
						}
						end_IL_0285:;
					}
					catch (EndOfStreamException)
					{
						if (l == 0)
						{
							cbI[k][idx.y][idx.x] = null;
						}
						else
						{
							cBlkInfo.off[l] = (cBlkInfo.len[l] = 0);
							cBlkInfo.ctp -= cBlkInfo.ntp[l];
							cBlkInfo.ntp[l] = 0;
							cBlkInfo.pktIdx[l] = -1;
						}
						return true;
					}
				}
			}
		}
		if (ephUsed)
		{
			readEPHMarker(pktHeaderBitReader);
		}
		pktIdx++;
		if (isTruncMode && maxCB == -1)
		{
			int num4 = ehs.Pos - pos;
			if (num4 > nb[tileIdx])
			{
				nb[tileIdx] = 0;
				return true;
			}
			nb[tileIdx] -= num4;
		}
		return false;
	}

	public virtual bool readPktBody(int l, int r, int c, int p, CBlkInfo[][][] cbI, int[] nb)
	{
		int num = ehs.Pos;
		bool flag = false;
		int tileIdx = src.TileIdx;
		bool flag2 = false;
		int num2 = ((r != 0) ? 1 : 0);
		int num3 = ((r == 0) ? 1 : 4);
		for (int i = num2; i < num3; i++)
		{
			if (p < ppinfo[c][r].Length)
			{
				flag2 = true;
			}
		}
		if (!flag2)
		{
			return false;
		}
		for (int j = num2; j < num3; j++)
		{
			for (int k = 0; k < cblks[j].Count; k++)
			{
				JPXImageCoordinates idx = ((CBlkCoordInfo)cblks[j][k]).idx;
				CBlkInfo cBlkInfo = cbI[j][idx.y][idx.x];
				cBlkInfo.off[l] = num;
				num += cBlkInfo.len[l];
				try
				{
					ehs.seek(num);
				}
				catch (EndOfStreamException)
				{
					if (l == 0)
					{
						cbI[j][idx.y][idx.x] = null;
					}
					else
					{
						cBlkInfo.off[l] = (cBlkInfo.len[l] = 0);
						cBlkInfo.ctp -= cBlkInfo.ntp[l];
						cBlkInfo.ntp[l] = 0;
						cBlkInfo.pktIdx[l] = -1;
					}
					throw new EndOfStreamException();
				}
				if (isTruncMode)
				{
					if (flag || cBlkInfo.len[l] > nb[tileIdx])
					{
						if (l == 0)
						{
							cbI[j][idx.y][idx.x] = null;
						}
						else
						{
							cBlkInfo.off[l] = (cBlkInfo.len[l] = 0);
							cBlkInfo.ctp -= cBlkInfo.ntp[l];
							cBlkInfo.ntp[l] = 0;
							cBlkInfo.pktIdx[l] = -1;
						}
						flag = true;
					}
					if (!flag)
					{
						nb[tileIdx] -= cBlkInfo.len[l];
					}
				}
				if (ncbQuit && r == rQuit && j == sQuit && idx.x == xQuit && idx.y == yQuit && tileIdx == tQuit && c == cQuit)
				{
					cbI[j][idx.y][idx.x] = null;
					flag = true;
				}
			}
		}
		ehs.seek(num);
		if (flag)
		{
			return true;
		}
		return false;
	}

	public int getPPX(int t, int c, int r)
	{
		return decSpec.pss.getPPX(t, c, r);
	}

	public int getPPY(int t, int c, int rl)
	{
		return decSpec.pss.getPPY(t, c, rl);
	}

	public virtual bool readSOPMarker(int[] nBytes, int p, int c, int r)
	{
		byte[] array = new byte[6];
		int tileIdx = src.TileIdx;
		bool num = r != 0;
		int num2 = ((r == 0) ? 1 : 4);
		bool flag = false;
		for (int i = (num ? 1 : 0); i < num2; i++)
		{
			if (p < ppinfo[c][r].Length)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!sopUsed)
		{
			return false;
		}
		int pos = ehs.Pos;
		if ((short)((ehs.read() << 8) | ehs.read()) != -111)
		{
			ehs.seek(pos);
			return false;
		}
		ehs.seek(pos);
		if (nBytes[tileIdx] < 6)
		{
			return true;
		}
		nBytes[tileIdx] -= 6;
		ehs.readFully(array, 0, 6);
		int num3 = array[0];
		num3 <<= 8;
		num3 |= array[1];
		_ = -111;
		num3 = array[2] & 0xFF;
		num3 <<= 8;
		num3 |= array[3] & 0xFF;
		_ = 4;
		num3 = array[4] & 0xFF;
		num3 <<= 8;
		num3 |= array[5] & 0xFF;
		if (!pph)
		{
			_ = pktIdx;
		}
		if (pph)
		{
			_ = pktIdx - 1;
		}
		return false;
	}

	public virtual void readEPHMarker(PktHeaderBitReader bin)
	{
		byte[] array = new byte[2];
		if (bin.usebais)
		{
			bin.bais.Read(array, 0, 2);
		}
		else
		{
			bin.in_Renamed.readFully(array, 0, 2);
		}
		_ = (array[0] << 8) | array[1];
		_ = -110;
	}

	public virtual PrecInfo getPrecInfo(int c, int r, int p)
	{
		return ppinfo[c][r][p];
	}
}
