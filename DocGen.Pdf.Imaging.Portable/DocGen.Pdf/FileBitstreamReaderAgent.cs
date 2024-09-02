using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf;

internal class FileBitstreamReaderAgent : BitstreamReader
{
	private bool isPsotEqualsZero = true;

	internal PktDecoder pktDec;

	private JPXParameters pl;

	private JPXRandomAccessStream in_Renamed;

	private int[][] firstPackOff;

	private int[] nBytes;

	private bool printInfo;

	private int[] baknBytes;

	private int[][] tilePartLen;

	private int[] totTileLen;

	private int[] totTileHeadLen;

	private int firstTilePartHeadLen;

	private double totAllTileLen;

	private int mainHeadLen;

	private int headLen;

	private int[][] tilePartHeadLen;

	private List<object> pktHL;

	private bool isTruncMode;

	private int remainingTileParts;

	private int[] tilePartsRead;

	private int totTilePartsRead;

	private int[] tileParts;

	private int curTilePart;

	private int[][] tilePartNum;

	private bool isEOCFound;

	private HeaderInformation hi;

	private CBlkInfo[][][][][] cbI;

	private int lQuit;

	private bool usePOCQuit;

	public virtual CBlkInfo[][][][][] CBlkInfo => cbI;

	public virtual int getNumTileParts(int t)
	{
		if (firstPackOff != null)
		{
			_ = firstPackOff[t];
		}
		return firstPackOff[t].Length;
	}

	internal FileBitstreamReaderAgent(HeaderDecoder hd, JPXRandomAccessStream ehs, DecodeHelper decSpec, JPXParameters pl, bool cdstrInfo, HeaderInformation hi)
		: base(hd, decSpec)
	{
		this.pl = pl;
		printInfo = cdstrInfo;
		this.hi = hi;
		string text = "Codestream elements information in bytes (offset, total length, header length):\n\n";
		usePOCQuit = pl.getBooleanParameter("poc_quit");
		pl.getBooleanParameter("parsing");
		try
		{
			trate = pl.getFloatParameter("rate");
			if (trate == -1f)
			{
				trate = float.MaxValue;
			}
		}
		catch (FormatException)
		{
		}
		catch (ArgumentException)
		{
		}
		try
		{
			tnbytes = pl.getIntParameter("nbytes");
		}
		catch (FormatException)
		{
		}
		catch (ArgumentException)
		{
		}
		JPXParameters defaultParameterList = pl.DefaultParameterList;
		if (((float)tnbytes != defaultParameterList.getFloatParameter("nbytes")) ? true : false)
		{
			trate = (float)tnbytes * 8f / (float)hd.MaxCompImgWidth / (float)hd.MaxCompImgHeight;
		}
		else
		{
			tnbytes = (int)(trate * (float)hd.MaxCompImgWidth * (float)hd.MaxCompImgHeight) / 8;
			if (tnbytes < 0)
			{
				tnbytes = int.MaxValue;
			}
		}
		isTruncMode = !pl.getBooleanParameter("parsing");
		int num = 0;
		try
		{
			num = pl.getIntParameter("ncb_quit");
		}
		catch (FormatException)
		{
		}
		catch (ArgumentException)
		{
		}
		if (num != -1)
		{
			_ = isTruncMode;
		}
		try
		{
			lQuit = pl.getIntParameter("l_quit");
		}
		catch (FormatException)
		{
		}
		catch (ArgumentException)
		{
		}
		in_Renamed = ehs;
		pktDec = new PktDecoder(decSpec, hd, ehs, this, isTruncMode, num);
		tileParts = new int[nt];
		totTileLen = new int[nt];
		tilePartLen = new int[nt][];
		tilePartNum = new int[nt][];
		firstPackOff = new int[nt][];
		tilePartsRead = new int[nt];
		totTileHeadLen = new int[nt];
		tilePartHeadLen = new int[nt][];
		nBytes = new int[nt];
		baknBytes = new int[nt];
		hd.nTileParts = new int[nt];
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int mainHeadOff = hd.mainHeadOff;
		mainHeadLen = in_Renamed.Pos - mainHeadOff;
		headLen = mainHeadLen;
		if (num == -1)
		{
			anbytes = mainHeadLen;
		}
		else
		{
			anbytes = 0;
		}
		text = text + "Main header length    : " + mainHeadOff + ", " + mainHeadLen + ", " + mainHeadLen + "\n";
		_ = anbytes;
		_ = tnbytes;
		totAllTileLen = 0.0;
		remainingTileParts = nt;
		int num5 = nt;
		int min;
		try
		{
			while (remainingTileParts != 0)
			{
				int pos = in_Renamed.Pos;
				try
				{
					num2 = readTilePartHeader();
					if (isEOCFound)
					{
						break;
					}
					num3 = tilePartsRead[num2];
					if (isPsotEqualsZero)
					{
						tilePartLen[num2][num3] = in_Renamed.length() - 2 - pos;
					}
					goto IL_0381;
				}
				catch (EndOfStreamException ex9)
				{
					firstPackOff[num2][num3] = in_Renamed.length();
					throw ex9;
				}
				IL_0381:
				int pos2 = in_Renamed.Pos;
				if (isTruncMode && num == -1 && pos2 - mainHeadOff > tnbytes)
				{
					firstPackOff[num2][num3] = in_Renamed.length();
					break;
				}
				firstPackOff[num2][num3] = pos2;
				tilePartHeadLen[num2][num3] = pos2 - pos;
				text = text + "Tile-part " + num3 + " of tile " + num2 + " : " + pos + ", " + tilePartLen[num2][num3] + ", " + tilePartHeadLen[num2][num3] + "\n";
				totTileLen[num2] += tilePartLen[num2][num3];
				totTileHeadLen[num2] += tilePartHeadLen[num2][num3];
				totAllTileLen += tilePartLen[num2][num3];
				if (isTruncMode)
				{
					if (anbytes + tilePartLen[num2][num3] > tnbytes)
					{
						anbytes += tilePartHeadLen[num2][num3];
						headLen += tilePartHeadLen[num2][num3];
						nBytes[num2] += tnbytes - anbytes;
						break;
					}
					anbytes += tilePartHeadLen[num2][num3];
					headLen += tilePartHeadLen[num2][num3];
					nBytes[num2] += tilePartLen[num2][num3] - tilePartHeadLen[num2][num3];
				}
				else
				{
					if (anbytes + tilePartHeadLen[num2][num3] > tnbytes)
					{
						break;
					}
					anbytes += tilePartHeadLen[num2][num3];
					headLen += tilePartHeadLen[num2][num3];
				}
				if (num4 == 0)
				{
					firstTilePartHeadLen = tilePartHeadLen[num2][num3];
				}
				tilePartsRead[num2]++;
				in_Renamed.seek(pos + tilePartLen[num2][num3]);
				remainingTileParts--;
				num5--;
				num4++;
				if (isPsotEqualsZero)
				{
					break;
				}
			}
		}
		catch (EndOfStreamException)
		{
			int num6 = in_Renamed.length();
			if (num6 < tnbytes)
			{
				tnbytes = num6;
				trate = (float)tnbytes * 8f / (float)hd.MaxCompImgWidth / (float)hd.MaxCompImgHeight;
			}
			if (!isTruncMode)
			{
				allocateRate();
			}
			if (pl.getParameter("res") == null)
			{
				targetRes = decSpec.dls.Min;
			}
			else
			{
				try
				{
					targetRes = pl.getIntParameter("res");
					if (targetRes < 0)
					{
						throw new ArgumentException("Specified negative resolution level index: " + targetRes);
					}
				}
				catch (FormatException)
				{
					throw new ArgumentException("Invalid resolution level index ('-res' option) " + pl.getParameter("res"));
				}
			}
			min = decSpec.dls.Min;
			if (targetRes > min)
			{
				targetRes = min;
			}
			for (int i = 0; i < nt; i++)
			{
				baknBytes[i] = nBytes[i];
			}
			return;
		}
		remainingTileParts = 0;
		if (pl.getParameter("res") == null)
		{
			targetRes = decSpec.dls.Min;
		}
		else
		{
			try
			{
				targetRes = pl.getIntParameter("res");
				if (targetRes < 0)
				{
					throw new ArgumentException("Specified negative resolution level index: " + targetRes);
				}
			}
			catch (FormatException)
			{
				throw new ArgumentException("Invalid resolution level index ('-res' option) " + pl.getParameter("res"));
			}
		}
		min = decSpec.dls.Min;
		if (targetRes > min)
		{
			targetRes = min;
		}
		if (!isEOCFound)
		{
			_ = isPsotEqualsZero;
		}
		if (!isTruncMode)
		{
			allocateRate();
		}
		else if (in_Renamed.Pos >= tnbytes)
		{
			anbytes += 2;
		}
		for (int j = 0; j < nt; j++)
		{
			baknBytes[j] = nBytes[j];
		}
	}

	private void allocateRate()
	{
		int num = tnbytes;
		anbytes += 2;
		_ = anbytes;
		int num2 = num - anbytes;
		int num3 = num2;
		for (int num4 = nt - 1; num4 > 0; num4--)
		{
			num2 -= (nBytes[num4] = (int)((double)num3 * ((double)totTileLen[num4] / totAllTileLen)));
		}
		nBytes[0] = num2;
	}

	private int readTilePartHeader()
	{
		HeaderInformation.SOT newSOT = hi.NewSOT;
		switch (in_Renamed.readShort())
		{
		case -39:
			isEOCFound = true;
			return -1;
		default:
			throw new Exception();
		case -112:
		{
			isEOCFound = false;
			if ((newSOT.lsot = in_Renamed.readUnsignedShort()) != 10)
			{
				throw new Exception();
			}
			int num = (newSOT.isot = in_Renamed.readUnsignedShort());
			if (num > 65534)
			{
				throw new Exception();
			}
			int num2 = (newSOT.psot = in_Renamed.readInt());
			isPsotEqualsZero = num2 == 0;
			if (num2 < 0)
			{
				throw new ArgumentException("Maximum tile length exceeded");
			}
			int num3 = (newSOT.tpsot = in_Renamed.read());
			if (num3 != tilePartsRead[num] || num3 < 0 || num3 > 254)
			{
				throw new Exception();
			}
			int num4 = (newSOT.tnsot = in_Renamed.read());
			hi.sotValue["t" + num + "_tp" + num3] = newSOT;
			if (num4 == 0)
			{
				int num5;
				if (tileParts[num] == 0 || tileParts[num] == tilePartLen.Length)
				{
					num5 = 2;
					remainingTileParts++;
				}
				else
				{
					num5 = 1;
				}
				tileParts[num] += num5;
				num4 = tileParts[num];
				int[] array = tilePartLen[num];
				tilePartLen[num] = new int[num4];
				for (int i = 0; i < num4 - num5; i++)
				{
					tilePartLen[num][i] = array[i];
				}
				array = tilePartNum[num];
				tilePartNum[num] = new int[num4];
				for (int j = 0; j < num4 - num5; j++)
				{
					tilePartNum[num][j] = array[j];
				}
				array = firstPackOff[num];
				firstPackOff[num] = new int[num4];
				for (int k = 0; k < num4 - num5; k++)
				{
					firstPackOff[num][k] = array[k];
				}
				array = tilePartHeadLen[num];
				tilePartHeadLen[num] = new int[num4];
				for (int l = 0; l < num4 - num5; l++)
				{
					tilePartHeadLen[num][l] = array[l];
				}
			}
			else if (tileParts[num] == 0)
			{
				remainingTileParts += num4 - 1;
				tileParts[num] = num4;
				tilePartLen[num] = new int[num4];
				tilePartNum[num] = new int[num4];
				firstPackOff[num] = new int[num4];
				tilePartHeadLen[num] = new int[num4];
			}
			else
			{
				if (tileParts[num] > num4)
				{
					throw new Exception();
				}
				remainingTileParts += num4 - tileParts[num];
				if (tileParts[num] != num4)
				{
					int[] array2 = tilePartLen[num];
					tilePartLen[num] = new int[num4];
					for (int m = 0; m < tileParts[num] - 1; m++)
					{
						tilePartLen[num][m] = array2[m];
					}
					array2 = tilePartNum[num];
					tilePartNum[num] = new int[num4];
					for (int n = 0; n < tileParts[num] - 1; n++)
					{
						tilePartNum[num][n] = array2[n];
					}
					array2 = firstPackOff[num];
					firstPackOff[num] = new int[num4];
					for (int num6 = 0; num6 < tileParts[num] - 1; num6++)
					{
						firstPackOff[num][num6] = array2[num6];
					}
					array2 = tilePartHeadLen[num];
					tilePartHeadLen[num] = new int[num4];
					for (int num7 = 0; num7 < tileParts[num] - 1; num7++)
					{
						tilePartHeadLen[num][num7] = array2[num7];
					}
				}
			}
			hd.resetHeaderMarkers();
			hd.nTileParts[num] = num4;
			do
			{
				hd.extractTilePartMarkSeg(in_Renamed.readShort(), in_Renamed, num, num3);
			}
			while ((hd.NumFoundMarkSeg & 0x2000) == 0);
			hd.readFoundTilePartMarkSeg(num, num3);
			tilePartLen[num][num3] = num2;
			tilePartNum[num][num3] = totTilePartsRead;
			totTilePartsRead++;
			hd.TileOfTileParts = num;
			return num;
		}
		}
	}

	private bool readLyResCompPos(int[][] lys, int lye, int ress, int rese, int comps, int compe)
	{
		int num = 10000;
		for (int i = comps; i < compe; i++)
		{
			if (i >= mdl.Length)
			{
				continue;
			}
			for (int j = ress; j < rese; j++)
			{
				if (lys[i] != null && j < lys[i].Length && lys[i][j] < num)
				{
					num = lys[i][j];
				}
			}
		}
		int tileIdx = TileIdx;
		bool flag = false;
		int num2 = firstPackOff[tileIdx][curTilePart] + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
		int num3 = (int)decSpec.nls.getTileDef(tileIdx);
		int num4 = 1;
		string text = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n";
		bool flag2 = false;
		if ((bool)decSpec.pphs.getTileDef(tileIdx))
		{
			flag2 = true;
		}
		for (int k = num; k < lye; k++)
		{
			for (int l = ress; l < rese; l++)
			{
				for (int m = comps; m < compe; m++)
				{
					if (m >= mdl.Length || l >= lys[m].Length || l > mdl[m] || k < lys[m][l] || k >= num3)
					{
						continue;
					}
					num4 = pktDec.getNumPrecinct(m, l);
					for (int n = 0; n < num4; n++)
					{
						int pos = in_Renamed.Pos;
						if (flag2)
						{
							pktDec.readPktHead(k, l, m, n, cbI[m][l], nBytes);
						}
						if (pos > num2 && curTilePart < firstPackOff[tileIdx].Length - 1)
						{
							curTilePart++;
							in_Renamed.seek(firstPackOff[tileIdx][curTilePart]);
							num2 = in_Renamed.Pos + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
						}
						flag = pktDec.readSOPMarker(nBytes, n, m, l);
						if (flag)
						{
							return true;
						}
						if (!flag2)
						{
							flag = pktDec.readPktHead(k, l, m, n, cbI[m][l], nBytes);
						}
						if (flag)
						{
							return true;
						}
						int num5 = in_Renamed.Pos - pos;
						pktHL.Add(num5);
						flag = pktDec.readPktBody(k, l, m, n, cbI[m][l], nBytes);
						int num6 = in_Renamed.Pos - pos;
						text = text + " Pkt l=" + k + ",r=" + l + ",c=" + m + ",p=" + n + ": " + pos + ", " + num6 + ", " + num5 + "\n";
						if (flag)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private bool readResLyCompPos(int[][] lys, int lye, int ress, int rese, int comps, int compe)
	{
		int tileIdx = TileIdx;
		bool flag = false;
		int num = firstPackOff[tileIdx][curTilePart] + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
		int num2 = 10000;
		for (int i = comps; i < compe; i++)
		{
			if (i >= mdl.Length)
			{
				continue;
			}
			for (int j = ress; j < rese; j++)
			{
				if (j <= mdl[i] && lys[i] != null && j < lys[i].Length && lys[i][j] < num2)
				{
					num2 = lys[i][j];
				}
			}
		}
		string text = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n";
		int num3 = (int)decSpec.nls.getTileDef(tileIdx);
		bool flag2 = false;
		if ((bool)decSpec.pphs.getTileDef(tileIdx))
		{
			flag2 = true;
		}
		int num4 = 1;
		for (int k = ress; k < rese; k++)
		{
			for (int l = num2; l < lye; l++)
			{
				for (int m = comps; m < compe; m++)
				{
					if (m >= mdl.Length || k > mdl[m] || k >= lys[m].Length || l < lys[m][k] || l >= num3)
					{
						continue;
					}
					num4 = pktDec.getNumPrecinct(m, k);
					for (int n = 0; n < num4; n++)
					{
						int pos = in_Renamed.Pos;
						if (flag2)
						{
							pktDec.readPktHead(l, k, m, n, cbI[m][k], nBytes);
						}
						if (pos > num && curTilePart < firstPackOff[tileIdx].Length - 1)
						{
							curTilePart++;
							in_Renamed.seek(firstPackOff[tileIdx][curTilePart]);
							num = in_Renamed.Pos + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
						}
						flag = pktDec.readSOPMarker(nBytes, n, m, k);
						if (flag)
						{
							return true;
						}
						if (!flag2)
						{
							flag = pktDec.readPktHead(l, k, m, n, cbI[m][k], nBytes);
						}
						if (flag)
						{
							return true;
						}
						int num5 = in_Renamed.Pos - pos;
						pktHL.Add(num5);
						flag = pktDec.readPktBody(l, k, m, n, cbI[m][k], nBytes);
						int num6 = in_Renamed.Pos - pos;
						text = text + " Pkt l=" + l + ",r=" + k + ",c=" + m + ",p=" + n + ": " + pos + ", " + num6 + ", " + num5 + "\n";
						if (flag)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private bool readResPosCompLy(int[][] lys, int lye, int ress, int rese, int comps, int compe)
	{
		JPXImageCoordinates numTiles = getNumTiles(null);
		JPXImageCoordinates tile = getTile(null);
		int imgULX = hd.ImgULX;
		int imgULY = hd.ImgULY;
		int num = imgULX + hd.ImgWidth;
		int num2 = imgULY + hd.ImgHeight;
		int tilePartULX = TilePartULX;
		int tilePartULY = TilePartULY;
		int nomTileWidth = NomTileWidth;
		int nomTileHeight = NomTileHeight;
		int num3 = ((tile.x == 0) ? imgULX : (tilePartULX + tile.x * nomTileWidth));
		int num4 = ((tile.y == 0) ? imgULY : (tilePartULY + tile.y * nomTileHeight));
		int num5 = ((tile.x != numTiles.x - 1) ? (tilePartULX + (tile.x + 1) * nomTileWidth) : num);
		int num6 = ((tile.y != numTiles.y - 1) ? (tilePartULY + (tile.y + 1) * nomTileHeight) : num2);
		int tileIdx = TileIdx;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int[][] array = new int[compe][];
		int num10 = 100000;
		int num11 = num5;
		int num12 = num6;
		int num13 = num3;
		int num14 = num4;
		for (int i = comps; i < compe; i++)
		{
			for (int j = ress; j < rese; j++)
			{
				if (i >= mdl.Length || j > mdl[i])
				{
					continue;
				}
				array[i] = new int[mdl[i] + 1];
				if (lys[i] != null && j < lys[i].Length && lys[i][j] < num10)
				{
					num10 = lys[i][j];
				}
				for (int num15 = pktDec.getNumPrecinct(i, j) - 1; num15 >= 0; num15--)
				{
					PrecInfo precInfo = pktDec.getPrecInfo(i, j, num15);
					if (precInfo.rgulx != num3)
					{
						if (precInfo.rgulx < num11)
						{
							num11 = precInfo.rgulx;
						}
						if (precInfo.rgulx > num13)
						{
							num13 = precInfo.rgulx;
						}
					}
					if (precInfo.rguly != num4)
					{
						if (precInfo.rguly < num12)
						{
							num12 = precInfo.rguly;
						}
						if (precInfo.rguly > num14)
						{
							num14 = precInfo.rguly;
						}
					}
					if (num9 == 0)
					{
						num7 = precInfo.rgw;
						num8 = precInfo.rgh;
					}
					else
					{
						num7 = MathUtil.gcd(num7, precInfo.rgw);
						num8 = MathUtil.gcd(num8, precInfo.rgh);
					}
					num9++;
				}
			}
		}
		int num16 = (num14 - num12) / num8 + 1;
		int num17 = (num13 - num11) / num7 + 1;
		bool flag = false;
		int num18 = firstPackOff[tileIdx][curTilePart] + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
		int num19 = (int)decSpec.nls.getTileDef(tileIdx);
		string text = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n";
		bool flag2 = false;
		if ((bool)decSpec.pphs.getTileDef(tileIdx))
		{
			flag2 = true;
		}
		for (int k = ress; k < rese; k++)
		{
			int num20 = num4;
			int num21 = num3;
			for (int l = 0; l <= num16; l++)
			{
				for (int m = 0; m <= num17; m++)
				{
					for (int n = comps; n < compe; n++)
					{
						if (n >= mdl.Length || k > mdl[n] || array[n][k] >= pktDec.getNumPrecinct(n, k))
						{
							continue;
						}
						PrecInfo precInfo = pktDec.getPrecInfo(n, k, array[n][k]);
						if (precInfo.rgulx != num21 || precInfo.rguly != num20)
						{
							continue;
						}
						for (int num22 = num10; num22 < lye; num22++)
						{
							if (k < lys[n].Length && num22 >= lys[n][k] && num22 < num19)
							{
								int pos = in_Renamed.Pos;
								if (flag2)
								{
									pktDec.readPktHead(num22, k, n, array[n][k], cbI[n][k], nBytes);
								}
								if (pos > num18 && curTilePart < firstPackOff[tileIdx].Length - 1)
								{
									curTilePart++;
									in_Renamed.seek(firstPackOff[tileIdx][curTilePart]);
									num18 = in_Renamed.Pos + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
								}
								flag = pktDec.readSOPMarker(nBytes, array[n][k], n, k);
								if (flag)
								{
									return true;
								}
								if (!flag2)
								{
									flag = pktDec.readPktHead(num22, k, n, array[n][k], cbI[n][k], nBytes);
								}
								if (flag)
								{
									return true;
								}
								int num23 = in_Renamed.Pos - pos;
								pktHL.Add(num23);
								flag = pktDec.readPktBody(num22, k, n, array[n][k], cbI[n][k], nBytes);
								int num24 = in_Renamed.Pos - pos;
								text = text + " Pkt l=" + num22 + ",r=" + k + ",c=" + n + ",p=" + array[n][k] + ": " + pos + ", " + num24 + ", " + num23 + "\n";
								if (flag)
								{
									return true;
								}
							}
						}
						array[n][k]++;
					}
					num21 = ((m == num17) ? num3 : (num11 + m * num7));
				}
				num20 = ((l == num16) ? num4 : (num12 + l * num8));
			}
		}
		return false;
	}

	private bool readPosCompResLy(int[][] lys, int lye, int ress, int rese, int comps, int compe)
	{
		JPXImageCoordinates numTiles = getNumTiles(null);
		JPXImageCoordinates tile = getTile(null);
		int imgULX = hd.ImgULX;
		int imgULY = hd.ImgULY;
		int num = imgULX + hd.ImgWidth;
		int num2 = imgULY + hd.ImgHeight;
		int tilePartULX = TilePartULX;
		int tilePartULY = TilePartULY;
		int nomTileWidth = NomTileWidth;
		int nomTileHeight = NomTileHeight;
		int num3 = ((tile.x == 0) ? imgULX : (tilePartULX + tile.x * nomTileWidth));
		int num4 = ((tile.y == 0) ? imgULY : (tilePartULY + tile.y * nomTileHeight));
		int num5 = ((tile.x != numTiles.x - 1) ? (tilePartULX + (tile.x + 1) * nomTileWidth) : num);
		int num6 = ((tile.y != numTiles.y - 1) ? (tilePartULY + (tile.y + 1) * nomTileHeight) : num2);
		int tileIdx = TileIdx;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int[][] array = new int[compe][];
		int num10 = 100000;
		int num11 = num5;
		int num12 = num6;
		int num13 = num3;
		int num14 = num4;
		for (int i = comps; i < compe; i++)
		{
			for (int j = ress; j < rese; j++)
			{
				if (i >= mdl.Length || j > mdl[i])
				{
					continue;
				}
				array[i] = new int[mdl[i] + 1];
				if (lys[i] != null && j < lys[i].Length && lys[i][j] < num10)
				{
					num10 = lys[i][j];
				}
				for (int num15 = pktDec.getNumPrecinct(i, j) - 1; num15 >= 0; num15--)
				{
					PrecInfo precInfo = pktDec.getPrecInfo(i, j, num15);
					if (precInfo.rgulx != num3)
					{
						if (precInfo.rgulx < num11)
						{
							num11 = precInfo.rgulx;
						}
						if (precInfo.rgulx > num13)
						{
							num13 = precInfo.rgulx;
						}
					}
					if (precInfo.rguly != num4)
					{
						if (precInfo.rguly < num12)
						{
							num12 = precInfo.rguly;
						}
						if (precInfo.rguly > num14)
						{
							num14 = precInfo.rguly;
						}
					}
					if (num9 == 0)
					{
						num7 = precInfo.rgw;
						num8 = precInfo.rgh;
					}
					else
					{
						num7 = MathUtil.gcd(num7, precInfo.rgw);
						num8 = MathUtil.gcd(num8, precInfo.rgh);
					}
					num9++;
				}
			}
		}
		int num16 = (num14 - num12) / num8 + 1;
		int num17 = (num13 - num11) / num7 + 1;
		bool flag = false;
		int num18 = firstPackOff[tileIdx][curTilePart] + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
		int num19 = (int)decSpec.nls.getTileDef(tileIdx);
		string text = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n";
		bool flag2 = false;
		if ((bool)decSpec.pphs.getTileDef(tileIdx))
		{
			flag2 = true;
		}
		int num20 = num4;
		int num21 = num3;
		for (int k = 0; k <= num16; k++)
		{
			for (int l = 0; l <= num17; l++)
			{
				for (int m = comps; m < compe; m++)
				{
					if (m >= mdl.Length)
					{
						continue;
					}
					for (int n = ress; n < rese; n++)
					{
						if (n > mdl[m] || array[m][n] >= pktDec.getNumPrecinct(m, n))
						{
							continue;
						}
						PrecInfo precInfo = pktDec.getPrecInfo(m, n, array[m][n]);
						if (precInfo.rgulx != num21 || precInfo.rguly != num20)
						{
							continue;
						}
						for (int num22 = num10; num22 < lye; num22++)
						{
							if (n < lys[m].Length && num22 >= lys[m][n] && num22 < num19)
							{
								int pos = in_Renamed.Pos;
								if (flag2)
								{
									pktDec.readPktHead(num22, n, m, array[m][n], cbI[m][n], nBytes);
								}
								if (pos > num18 && curTilePart < firstPackOff[tileIdx].Length - 1)
								{
									curTilePart++;
									in_Renamed.seek(firstPackOff[tileIdx][curTilePart]);
									num18 = in_Renamed.Pos + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
								}
								flag = pktDec.readSOPMarker(nBytes, array[m][n], m, n);
								if (flag)
								{
									return true;
								}
								if (!flag2)
								{
									flag = pktDec.readPktHead(num22, n, m, array[m][n], cbI[m][n], nBytes);
								}
								if (flag)
								{
									return true;
								}
								int num23 = in_Renamed.Pos - pos;
								pktHL.Add(num23);
								flag = pktDec.readPktBody(num22, n, m, array[m][n], cbI[m][n], nBytes);
								int num24 = in_Renamed.Pos - pos;
								text = text + " Pkt l=" + num22 + ",r=" + n + ",c=" + m + ",p=" + array[m][n] + ": " + pos + ", " + num24 + ", " + num23 + "\n";
								if (flag)
								{
									return true;
								}
							}
						}
						array[m][n]++;
					}
				}
				num21 = ((l == num17) ? num3 : (num11 + l * num7));
			}
			num20 = ((k == num16) ? num4 : (num12 + k * num8));
		}
		return false;
	}

	private bool readCompPosResLy(int[][] lys, int lye, int ress, int rese, int comps, int compe)
	{
		JPXImageCoordinates numTiles = getNumTiles(null);
		JPXImageCoordinates tile = getTile(null);
		int imgULX = hd.ImgULX;
		int imgULY = hd.ImgULY;
		int num = imgULX + hd.ImgWidth;
		int num2 = imgULY + hd.ImgHeight;
		int tilePartULX = TilePartULX;
		int tilePartULY = TilePartULY;
		int nomTileWidth = NomTileWidth;
		int nomTileHeight = NomTileHeight;
		int num3 = ((tile.x == 0) ? imgULX : (tilePartULX + tile.x * nomTileWidth));
		int num4 = ((tile.y == 0) ? imgULY : (tilePartULY + tile.y * nomTileHeight));
		int num5 = ((tile.x != numTiles.x - 1) ? (tilePartULX + (tile.x + 1) * nomTileWidth) : num);
		int num6 = ((tile.y != numTiles.y - 1) ? (tilePartULY + (tile.y + 1) * nomTileHeight) : num2);
		int tileIdx = TileIdx;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int[][] array = new int[compe][];
		int num10 = 100000;
		int num11 = num5;
		int num12 = num6;
		int num13 = num3;
		int num14 = num4;
		for (int i = comps; i < compe; i++)
		{
			for (int j = ress; j < rese; j++)
			{
				if (i >= mdl.Length || j > mdl[i])
				{
					continue;
				}
				array[i] = new int[mdl[i] + 1];
				if (lys[i] != null && j < lys[i].Length && lys[i][j] < num10)
				{
					num10 = lys[i][j];
				}
				for (int num15 = pktDec.getNumPrecinct(i, j) - 1; num15 >= 0; num15--)
				{
					PrecInfo precInfo = pktDec.getPrecInfo(i, j, num15);
					if (precInfo.rgulx != num3)
					{
						if (precInfo.rgulx < num11)
						{
							num11 = precInfo.rgulx;
						}
						if (precInfo.rgulx > num13)
						{
							num13 = precInfo.rgulx;
						}
					}
					if (precInfo.rguly != num4)
					{
						if (precInfo.rguly < num12)
						{
							num12 = precInfo.rguly;
						}
						if (precInfo.rguly > num14)
						{
							num14 = precInfo.rguly;
						}
					}
					if (num9 == 0)
					{
						num7 = precInfo.rgw;
						num8 = precInfo.rgh;
					}
					else
					{
						num7 = MathUtil.gcd(num7, precInfo.rgw);
						num8 = MathUtil.gcd(num8, precInfo.rgh);
					}
					num9++;
				}
			}
		}
		int num16 = (num14 - num12) / num8 + 1;
		int num17 = (num13 - num11) / num7 + 1;
		bool flag = false;
		int num18 = firstPackOff[tileIdx][curTilePart] + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
		_ = (int)decSpec.nls.getTileDef(tileIdx);
		string text = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n";
		bool flag2 = false;
		if ((bool)decSpec.pphs.getTileDef(tileIdx))
		{
			flag2 = true;
		}
		for (int k = comps; k < compe; k++)
		{
			if (k >= mdl.Length)
			{
				continue;
			}
			int num19 = num4;
			int num20 = num3;
			for (int l = 0; l <= num16; l++)
			{
				for (int m = 0; m <= num17; m++)
				{
					for (int n = ress; n < rese; n++)
					{
						if (n > mdl[k] || array[k][n] >= pktDec.getNumPrecinct(k, n))
						{
							continue;
						}
						PrecInfo precInfo = pktDec.getPrecInfo(k, n, array[k][n]);
						if (precInfo.rgulx != num20 || precInfo.rguly != num19)
						{
							continue;
						}
						for (int num21 = num10; num21 < lye; num21++)
						{
							if (n < lys[k].Length && num21 >= lys[k][n])
							{
								int pos = in_Renamed.Pos;
								if (flag2)
								{
									pktDec.readPktHead(num21, n, k, array[k][n], cbI[k][n], nBytes);
								}
								if (pos > num18 && curTilePart < firstPackOff[tileIdx].Length - 1)
								{
									curTilePart++;
									in_Renamed.seek(firstPackOff[tileIdx][curTilePart]);
									num18 = in_Renamed.Pos + tilePartLen[tileIdx][curTilePart] - 1 - tilePartHeadLen[tileIdx][curTilePart];
								}
								flag = pktDec.readSOPMarker(nBytes, array[k][n], k, n);
								if (flag)
								{
									return true;
								}
								if (!flag2)
								{
									flag = pktDec.readPktHead(num21, n, k, array[k][n], cbI[k][n], nBytes);
								}
								if (flag)
								{
									return true;
								}
								int num22 = in_Renamed.Pos - pos;
								pktHL.Add(num22);
								flag = pktDec.readPktBody(num21, n, k, array[k][n], cbI[k][n], nBytes);
								int num23 = in_Renamed.Pos - pos;
								text = text + " Pkt l=" + num21 + ",r=" + n + ",c=" + k + ",p=" + array[k][n] + ": " + pos + ", " + num23 + ", " + num22 + "\n";
								if (flag)
								{
									return true;
								}
							}
						}
						array[k][n]++;
					}
					num20 = ((m == num17) ? num3 : (num11 + m * num7));
				}
				num19 = ((l == num16) ? num4 : (num12 + l * num8));
			}
		}
		return false;
	}

	private void readTilePkts(int t)
	{
		pktHL = new List<object>(new List<object>(10));
		int num = (int)decSpec.nls.getTileDef(t);
		if ((bool)decSpec.pphs.getTileDef(t))
		{
			MemoryStream packedPktHead = hd.getPackedPktHead(t);
			cbI = pktDec.restart(nc, mdl, num, cbI, pph: true, packedPktHead);
		}
		else
		{
			cbI = pktDec.restart(nc, mdl, num, cbI, pph: false, null);
		}
		int[][] array = (int[][])decSpec.pcs.getTileDef(t);
		int num2 = ((array == null) ? 1 : array.Length);
		int[][] array2 = new int[num2][];
		for (int i = 0; i < num2; i++)
		{
			array2[i] = new int[6];
		}
		int num3 = 0;
		array2[0][1] = 0;
		if (array == null)
		{
			array2[num3][0] = (int)decSpec.pos.getTileDef(t);
			array2[num3][1] = num;
			array2[num3][2] = 0;
			array2[num3][3] = decSpec.dls.getMaxInTile(t) + 1;
			array2[num3][4] = 0;
			array2[num3][5] = nc;
		}
		else
		{
			for (num3 = 0; num3 < num2; num3++)
			{
				array2[num3][0] = array[num3][5];
				array2[num3][1] = array[num3][2];
				array2[num3][2] = array[num3][0];
				array2[num3][3] = array[num3][3];
				array2[num3][4] = array[num3][1];
				array2[num3][5] = array[num3][4];
			}
		}
		try
		{
			if ((isTruncMode && firstPackOff == null) || firstPackOff[t] == null)
			{
				return;
			}
			in_Renamed.seek(firstPackOff[t][0]);
		}
		catch (EndOfStreamException)
		{
			return;
		}
		curTilePart = 0;
		bool flag = false;
		int num4 = nBytes[t];
		int[][] array3 = new int[nc][];
		for (int j = 0; j < nc; j++)
		{
			array3[j] = new int[(int)decSpec.dls.getTileCompVal(t, j) + 1];
		}
		try
		{
			for (int k = 0; k < num2; k++)
			{
				int num5 = array2[k][1];
				int num6 = array2[k][2];
				int num7 = array2[k][3];
				int num8 = array2[k][4];
				int num9 = array2[k][5];
				flag = array2[k][0] switch
				{
					0 => readLyResCompPos(array3, num5, num6, num7, num8, num9), 
					1 => readResLyCompPos(array3, num5, num6, num7, num8, num9), 
					2 => readResPosCompLy(array3, num5, num6, num7, num8, num9), 
					3 => readPosCompResLy(array3, num5, num6, num7, num8, num9), 
					4 => readCompPosResLy(array3, num5, num6, num7, num8, num9), 
					_ => throw new ArgumentException("Not recognized progression type"), 
				};
				for (int l = num8; l < num9; l++)
				{
					if (l >= array3.Length)
					{
						continue;
					}
					for (int m = num6; m < num7; m++)
					{
						if (m < array3[l].Length)
						{
							array3[l][m] = num5;
						}
					}
				}
				if (flag || usePOCQuit)
				{
					break;
				}
			}
		}
		catch (EndOfStreamException ex2)
		{
			throw ex2;
		}
		if (isTruncMode)
		{
			anbytes += num4 - nBytes[t];
			if (flag)
			{
				nBytes[t] = 0;
			}
		}
		else if (nBytes[t] < totTileLen[t] - totTileHeadLen[t])
		{
			bool flag2 = false;
			int[] array4 = new int[pktHL.Count];
			for (int num10 = pktHL.Count - 1; num10 >= 0; num10--)
			{
				array4[num10] = (int)pktHL[num10];
			}
			bool flag3 = false;
			for (int n = 0; n < num; n++)
			{
				if (cbI == null)
				{
					continue;
				}
				int num11 = cbI.Length;
				int num12 = 0;
				for (int num13 = 0; num13 < num11; num13++)
				{
					if (cbI[num13] != null && cbI[num13].Length > num12)
					{
						num12 = cbI[num13].Length;
					}
				}
				for (int num14 = 0; num14 < num12; num14++)
				{
					int num15 = 0;
					for (int num16 = 0; num16 < num11; num16++)
					{
						if (cbI[num16] != null && cbI[num16][num14] != null && cbI[num16][num14].Length > num15)
						{
							num15 = cbI[num16][num14].Length;
						}
					}
					for (int num17 = 0; num17 < num15; num17++)
					{
						if ((num14 == 0 && num17 != 0) || (num14 != 0 && num17 == 0))
						{
							continue;
						}
						int num18 = 0;
						for (int num19 = 0; num19 < num11; num19++)
						{
							if (cbI[num19] != null && cbI[num19][num14] != null && cbI[num19][num14][num17] != null && cbI[num19][num14][num17].Length > num18)
							{
								num18 = cbI[num19][num14][num17].Length;
							}
						}
						for (int num20 = 0; num20 < num18; num20++)
						{
							int num21 = 0;
							for (int num22 = 0; num22 < num11; num22++)
							{
								if (cbI[num22] != null && cbI[num22][num14] != null && cbI[num22][num14][num17] != null && cbI[num22][num14][num17][num20] != null && cbI[num22][num14][num17][num20].Length > num21)
								{
									num21 = cbI[num22][num14][num17][num20].Length;
								}
							}
							for (int num23 = 0; num23 < num21; num23++)
							{
								for (int num24 = 0; num24 < num11; num24++)
								{
									if (cbI[num24] == null || cbI[num24][num14] == null || cbI[num24][num14][num17] == null || cbI[num24][num14][num17][num20] == null || cbI[num24][num14][num17][num20][num23] == null)
									{
										continue;
									}
									CBlkInfo cBlkInfo = cbI[num24][num14][num17][num20][num23];
									if (!flag3)
									{
										if (nBytes[t] < array4[cBlkInfo.pktIdx[n]])
										{
											flag2 = true;
											flag3 = true;
										}
										else if (!flag2)
										{
											nBytes[t] -= array4[cBlkInfo.pktIdx[n]];
											anbytes += array4[cBlkInfo.pktIdx[n]];
											array4[cBlkInfo.pktIdx[n]] = 0;
										}
									}
									if (cBlkInfo.len[n] != 0)
									{
										if (cBlkInfo.len[n] < nBytes[t] && !flag3)
										{
											nBytes[t] -= cBlkInfo.len[n];
											anbytes += cBlkInfo.len[n];
										}
										else
										{
											cBlkInfo.len[n] = (cBlkInfo.off[n] = (cBlkInfo.ntp[n] = 0));
											flag3 = true;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		else
		{
			anbytes += totTileLen[t] - totTileHeadLen[t];
			if (t < getNumTiles() - 1)
			{
				nBytes[t + 1] += nBytes[t] - (totTileLen[t] - totTileHeadLen[t]);
			}
		}
	}

	public override void setTile(int x, int y)
	{
		if (x < 0 || y < 0 || x >= ntX || y >= ntY)
		{
			throw new ArgumentException();
		}
		int num = y * ntX + x;
		if (num == 0)
		{
			anbytes = headLen;
			if (!isTruncMode)
			{
				anbytes += 2;
			}
			for (int i = 0; i < nt; i++)
			{
				nBytes[i] = baknBytes[i];
			}
		}
		ctX = x;
		ctY = y;
		int num2 = ((x == 0) ? ax : (px + x * ntW));
		int num3 = ((y == 0) ? ay : (py + y * ntH));
		for (int num4 = nc - 1; num4 >= 0; num4--)
		{
			culx[num4] = (num2 + hd.getCompSubsX(num4) - 1) / hd.getCompSubsX(num4);
			culy[num4] = (num3 + hd.getCompSubsY(num4) - 1) / hd.getCompSubsY(num4);
			offX[num4] = (px + x * ntW + hd.getCompSubsX(num4) - 1) / hd.getCompSubsX(num4);
			offY[num4] = (py + y * ntH + hd.getCompSubsY(num4) - 1) / hd.getCompSubsY(num4);
		}
		subbTrees = new SubbandSyn[nc];
		mdl = new int[nc];
		derived = new bool[nc];
		params_Renamed = new StdDequantizerParams[nc];
		gb = new int[nc];
		for (int j = 0; j < nc; j++)
		{
			derived[j] = decSpec.qts.isDerived(num, j);
			params_Renamed[j] = (StdDequantizerParams)decSpec.qsss.getTileCompVal(num, j);
			gb[j] = (int)decSpec.gbs.getTileCompVal(num, j);
			mdl[j] = (int)decSpec.dls.getTileCompVal(num, j);
			SubbandSyn[] array = subbTrees;
			int num5 = j;
			int tileCompWidth = getTileCompWidth(num, j, mdl[j]);
			int tileCompHeight = getTileCompHeight(num, j, mdl[j]);
			int resULX = getResULX(j, mdl[j]);
			int resULY = getResULY(j, mdl[j]);
			int lvls = mdl[j];
			WaveletFilter[] hFilters = decSpec.wfs.getHFilters(num, j);
			WaveletFilter[] hfilters = hFilters;
			hFilters = decSpec.wfs.getVFilters(num, j);
			array[num5] = new SubbandSyn(tileCompWidth, tileCompHeight, resULX, resULY, lvls, hfilters, hFilters);
			initSubbandsFields(j, subbTrees[j]);
		}
		try
		{
			readTilePkts(num);
		}
		catch (IOException)
		{
		}
	}

	public override void nextTile()
	{
		if (ctX == ntX - 1 && ctY == ntY - 1)
		{
			throw new Exception();
		}
		if (ctX < ntX - 1)
		{
			setTile(ctX + 1, ctY);
		}
		else
		{
			setTile(0, ctY + 1);
		}
	}

	public override DecLyrdCBlk getCodeBlock(int c, int m, int n, SubbandSyn sb, int fl, int nl, DecLyrdCBlk ccb)
	{
		int tileIdx = TileIdx;
		int resLvl = sb.resLvl;
		int sbandIdx = sb.sbandIdx;
		int num = (int)decSpec.nls.getTileDef(tileIdx);
		int num2 = (int)decSpec.ecopts.getTileCompVal(tileIdx, c);
		if (nl < 0)
		{
			nl = num - fl + 1;
		}
		if (lQuit != -1 && fl + nl > lQuit)
		{
			nl = lQuit - fl;
		}
		int resLvl2 = getSynSubbandTree(tileIdx, c).resLvl;
		_ = targetRes + resLvl2 - decSpec.dls.Min;
		CBlkInfo cBlkInfo;
		try
		{
			cBlkInfo = cbI[c][resLvl][sbandIdx][m][n];
			if (fl < 1 || fl > num || fl + nl - 1 > num)
			{
				throw new ArgumentException();
			}
		}
		catch (IndexOutOfRangeException)
		{
			string[] obj = new string[13]
			{
				"Code-block (t:",
				tileIdx.ToString(),
				", c:",
				c.ToString(),
				", r:",
				resLvl.ToString(),
				", s:",
				sbandIdx.ToString(),
				", ",
				m.ToString(),
				"x",
				null,
				null
			};
			int num3 = n;
			obj[11] = num3.ToString();
			obj[12] = ") not found in codestream";
			throw new ArgumentException(string.Concat(obj));
		}
		catch (NullReferenceException)
		{
			throw new ArgumentException("Code-block (t:" + tileIdx + ", c:" + c + ", r:" + resLvl + ", s:" + sbandIdx + ", " + m + "x" + n + ") not found in bit stream");
		}
		if (ccb == null)
		{
			ccb = new DecLyrdCBlk();
		}
		ccb.m = m;
		ccb.n = n;
		ccb.nl = 0;
		ccb.dl = 0;
		ccb.nTrunc = 0;
		if (cBlkInfo == null)
		{
			ccb.skipMSBP = 0;
			ccb.prog = false;
			ccb.w = (ccb.h = (ccb.ulx = (ccb.uly = 0)));
			return ccb;
		}
		ccb.skipMSBP = cBlkInfo.msbSkipped;
		ccb.ulx = cBlkInfo.ulx;
		ccb.uly = cBlkInfo.uly;
		ccb.w = cBlkInfo.w;
		ccb.h = cBlkInfo.h;
		ccb.ftpIdx = 0;
		for (int i = 0; i < cBlkInfo.len.Length && cBlkInfo.len[i] == 0; i++)
		{
			ccb.ftpIdx += cBlkInfo.ntp[i];
		}
		for (int i = fl - 1; i < fl + nl - 1; i++)
		{
			ccb.nl++;
			ccb.dl += cBlkInfo.len[i];
			ccb.nTrunc += cBlkInfo.ntp[i];
		}
		int num4;
		int j;
		if ((num2 & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
		{
			num4 = ccb.nTrunc - ccb.ftpIdx;
		}
		else if ((num2 & StdEntropyCoderOptions.OPT_BYPASS) != 0)
		{
			if (ccb.nTrunc <= StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX)
			{
				num4 = 1;
			}
			else
			{
				num4 = 1;
				for (j = ccb.ftpIdx; j < ccb.nTrunc; j++)
				{
					if (j >= StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1)
					{
						int num5 = (j + StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % StdEntropyCoderOptions.NUM_PASSES;
						if (num5 == 1 || num5 == 2)
						{
							num4++;
						}
					}
				}
			}
		}
		else
		{
			num4 = 1;
		}
		if (ccb.data == null || ccb.data.Length < ccb.dl)
		{
			ccb.data = new byte[ccb.dl];
		}
		if (num4 > 1 && (ccb.tsLengths == null || ccb.tsLengths.Length < num4))
		{
			ccb.tsLengths = new int[num4];
		}
		else if (num4 > 1 && (num2 & (StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_TERM_PASS)) == StdEntropyCoderOptions.OPT_BYPASS)
		{
			ArrayUtil.intArraySet(ccb.tsLengths, 0);
		}
		int num6 = -1;
		j = ccb.ftpIdx;
		int num7 = ccb.ftpIdx;
		int num8 = 0;
		for (int i = fl - 1; i < fl + nl - 1; i++)
		{
			num7 += cBlkInfo.ntp[i];
			if (cBlkInfo.len[i] == 0)
			{
				continue;
			}
			try
			{
				in_Renamed.seek(cBlkInfo.off[i]);
				in_Renamed.readFully(ccb.data, num6 + 1, cBlkInfo.len[i]);
				num6 += cBlkInfo.len[i];
			}
			catch (IOException)
			{
			}
			if (num4 == 1)
			{
				continue;
			}
			int num9;
			if ((num2 & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
			{
				num9 = 0;
				for (; j < num7; j++)
				{
					if (cBlkInfo.segLen[i] != null)
					{
						ccb.tsLengths[num8++] = cBlkInfo.segLen[i][num9];
					}
					else
					{
						ccb.tsLengths[num8++] = cBlkInfo.len[i];
					}
					num9++;
				}
				continue;
			}
			num9 = 0;
			for (; j < num7; j++)
			{
				if (j >= StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1 && (j + StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % StdEntropyCoderOptions.NUM_PASSES != 0)
				{
					if (cBlkInfo.segLen[i] != null)
					{
						ccb.tsLengths[num8++] += cBlkInfo.segLen[i][num9++];
						cBlkInfo.len[i] -= cBlkInfo.segLen[i][num9 - 1];
					}
					else
					{
						ccb.tsLengths[num8++] += cBlkInfo.len[i];
						cBlkInfo.len[i] = 0;
					}
				}
			}
			if (cBlkInfo.segLen[i] != null && num9 < cBlkInfo.segLen[i].Length)
			{
				ccb.tsLengths[num8] += cBlkInfo.segLen[i][num9];
				cBlkInfo.len[i] -= cBlkInfo.segLen[i][num9];
			}
			else if (num8 < num4)
			{
				ccb.tsLengths[num8] += cBlkInfo.len[i];
				cBlkInfo.len[i] = 0;
			}
		}
		if (num4 == 1 && ccb.tsLengths != null)
		{
			ccb.tsLengths[0] = ccb.dl;
		}
		int num10 = fl + nl - 1;
		if (num10 < num - 1)
		{
			for (int i = num10 + 1; i < num; i++)
			{
				if (cBlkInfo.len[i] != 0)
				{
					ccb.prog = true;
				}
			}
		}
		return ccb;
	}
}
