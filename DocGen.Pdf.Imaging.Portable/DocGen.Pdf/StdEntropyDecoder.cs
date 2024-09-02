using System;

namespace DocGen.Pdf;

internal class StdEntropyDecoder : EntropyDecoder
{
	private const bool DO_TIMING = false;

	private ByteToBitInput bin;

	private MQDecoder mq;

	private DecodeHelper decSpec;

	private int options;

	private bool doer;

	private bool verber;

	private const int ZC_LUT_BITS = 8;

	private static readonly int[] ZC_LUT_LH;

	private static readonly int[] ZC_LUT_HL;

	private static readonly int[] ZC_LUT_HH;

	private const int SC_LUT_BITS = 9;

	private static readonly int[] SC_LUT;

	private const int SC_LUT_MASK = 15;

	private const int SC_SPRED_SHIFT = 31;

	private const int INT_SIGN_BIT = int.MinValue;

	private const int MR_LUT_BITS = 9;

	private static readonly int[] MR_LUT;

	private const int NUM_CTXTS = 19;

	private const int RLC_CTXT = 1;

	private const int UNIF_CTXT = 0;

	private static readonly int[] MQ_INIT;

	private const int SEG_SYMBOL = 10;

	private int[] state;

	private const int STATE_SEP = 16;

	private const int STATE_SIG_R1 = 32768;

	private const int STATE_VISITED_R1 = 16384;

	private const int STATE_NZ_CTXT_R1 = 8192;

	private const int STATE_H_L_SIGN_R1 = 4096;

	private const int STATE_H_R_SIGN_R1 = 2048;

	private const int STATE_V_U_SIGN_R1 = 1024;

	private const int STATE_V_D_SIGN_R1 = 512;

	private const int STATE_PREV_MR_R1 = 256;

	private const int STATE_H_L_R1 = 128;

	private const int STATE_H_R_R1 = 64;

	private const int STATE_V_U_R1 = 32;

	private const int STATE_V_D_R1 = 16;

	private const int STATE_D_UL_R1 = 8;

	private const int STATE_D_UR_R1 = 4;

	private const int STATE_D_DL_R1 = 2;

	private const int STATE_D_DR_R1 = 1;

	private static readonly int STATE_SIG_R2;

	private static readonly int STATE_VISITED_R2;

	private static readonly int STATE_NZ_CTXT_R2;

	private static readonly int STATE_H_L_SIGN_R2;

	private static readonly int STATE_H_R_SIGN_R2;

	private static readonly int STATE_V_U_SIGN_R2;

	private static readonly int STATE_V_D_SIGN_R2;

	private static readonly int STATE_PREV_MR_R2;

	private static readonly int STATE_H_L_R2;

	private static readonly int STATE_H_R_R2;

	private static readonly int STATE_V_U_R2;

	private static readonly int STATE_V_D_R2;

	private static readonly int STATE_D_UL_R2;

	private static readonly int STATE_D_UR_R2;

	private static readonly int STATE_D_DL_R2;

	private static readonly int STATE_D_DR_R2;

	private static readonly int SIG_MASK_R1R2;

	private static readonly int VSTD_MASK_R1R2;

	private static readonly int RLC_MASK_R1R2;

	private const int ZC_MASK = 255;

	private const int SC_SHIFT_R1 = 4;

	private static readonly int SC_SHIFT_R2;

	private static readonly int SC_MASK;

	private const int MR_MASK = 511;

	private DecLyrdCBlk srcblk;

	private int mQuit;

	internal StdEntropyDecoder(CodedCBlkDataSrcDec src, DecodeHelper decSpec, bool doer, bool verber, int mQuit)
		: base(src)
	{
		this.decSpec = decSpec;
		this.doer = doer;
		this.verber = verber;
		this.mQuit = mQuit;
		state = new int[(decSpec.cblks.MaxCBlkWidth + 2) * ((decSpec.cblks.MaxCBlkHeight + 1) / 2 + 2)];
	}

	public override DataBlock getCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk)
	{
		ByteInputBuffer byteInputBuffer = null;
		srcblk = src.getCodeBlock(c, m, n, sb, 1, -1, srcblk);
		options = (int)decSpec.ecopts.getTileCompVal(tIdx, c);
		ArrayUtil.intArraySet(state, 0);
		if (cblk == null)
		{
			cblk = new DataBlockInt();
		}
		cblk.progressive = srcblk.prog;
		cblk.ulx = srcblk.ulx;
		cblk.uly = srcblk.uly;
		cblk.w = srcblk.w;
		cblk.h = srcblk.h;
		cblk.offset = 0;
		cblk.scanw = cblk.w;
		int[] array = (int[])cblk.Data;
		if (array == null || array.Length < srcblk.w * srcblk.h)
		{
			array = new int[srcblk.w * srcblk.h];
			cblk.Data = array;
		}
		else
		{
			ArrayUtil.intArraySet(array, 0);
		}
		if (srcblk.nl <= 0 || srcblk.nTrunc <= 0)
		{
			return cblk;
		}
		int num = ((srcblk.tsLengths == null) ? srcblk.dl : srcblk.tsLengths[0]);
		int num2 = 0;
		int num3 = srcblk.nTrunc;
		if (mq == null)
		{
			byteInputBuffer = new ByteInputBuffer(srcblk.data, 0, num);
			mq = new MQDecoder(byteInputBuffer, 19, MQ_INIT);
		}
		else
		{
			mq.nextSegment(srcblk.data, 0, num);
			mq.resetCtxts();
		}
		bool flag = false;
		if ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && bin == null)
		{
			if (byteInputBuffer == null)
			{
				byteInputBuffer = mq.ByteInputBuffer;
			}
			bin = new ByteToBitInput(byteInputBuffer);
		}
		int[] zc_lut;
		switch (sb.orientation)
		{
		case 1:
			zc_lut = ZC_LUT_HL;
			break;
		case 0:
		case 2:
			zc_lut = ZC_LUT_LH;
			break;
		case 3:
			zc_lut = ZC_LUT_HH;
			break;
		default:
			throw new Exception("JJ2000 internal error");
		}
		int num4 = 30 - srcblk.skipMSBP;
		if (mQuit != -1 && mQuit * 3 - 2 < num3)
		{
			num3 = mQuit * 3 - 2;
		}
		if (num4 >= 0 && num3 > 0)
		{
			bool isterm = (options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0 || ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && 31 - StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - srcblk.skipMSBP >= num4);
			flag = cleanuppass(cblk, mq, num4, state, zc_lut, isterm);
			num3--;
			if (!flag || !doer)
			{
				num4--;
			}
		}
		if (!flag || !doer)
		{
			while (num4 >= 0 && num3 > 0)
			{
				bool isterm;
				if ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && num4 < 31 - StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - srcblk.skipMSBP)
				{
					bin.setByteArray(null, -1, srcblk.tsLengths[++num2]);
					isterm = (options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0;
					flag = rawSigProgPass(cblk, bin, num4, state, isterm);
					num3--;
					if (num3 <= 0 || (flag && doer))
					{
						break;
					}
					if ((options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
					{
						bin.setByteArray(null, -1, srcblk.tsLengths[++num2]);
					}
					isterm = (options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0 || ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && 31 - StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - srcblk.skipMSBP > num4);
					flag = rawMagRefPass(cblk, bin, num4, state, isterm);
				}
				else
				{
					if ((options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
					{
						mq.nextSegment(null, -1, srcblk.tsLengths[++num2]);
					}
					isterm = (options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0;
					flag = sigProgPass(cblk, mq, num4, state, zc_lut, isterm);
					num3--;
					if (num3 <= 0 || (flag && doer))
					{
						break;
					}
					if ((options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
					{
						mq.nextSegment(null, -1, srcblk.tsLengths[++num2]);
					}
					isterm = (options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0 || ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && 31 - StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - srcblk.skipMSBP > num4);
					flag = magRefPass(cblk, mq, num4, state, isterm);
				}
				num3--;
				if (num3 <= 0 || (flag && doer))
				{
					break;
				}
				if ((options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0 || ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && num4 < 31 - StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - srcblk.skipMSBP))
				{
					mq.nextSegment(null, -1, srcblk.tsLengths[++num2]);
				}
				isterm = (options & StdEntropyCoderOptions.OPT_TERM_PASS) != 0 || ((options & StdEntropyCoderOptions.OPT_BYPASS) != 0 && 31 - StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - srcblk.skipMSBP >= num4);
				flag = cleanuppass(cblk, mq, num4, state, zc_lut, isterm);
				num3--;
				if (flag && doer)
				{
					break;
				}
				num4--;
			}
		}
		if (flag && doer)
		{
			_ = verber;
			conceal(cblk, num4);
		}
		return cblk;
	}

	public override DataBlock getInternCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk)
	{
		return getCodeBlock(c, m, n, sb, cblk);
	}

	private bool sigProgPass(DataBlock cblk, MQDecoder mq, int bp, int[] state, int[] zc_lut, bool isterm)
	{
		int scanw = cblk.scanw;
		int num = cblk.w + 2;
		int num2 = num * StdEntropyCoderOptions.STRIPE_HEIGHT / 2 - cblk.w;
		int num3 = scanw * StdEntropyCoderOptions.STRIPE_HEIGHT - cblk.w;
		int num4 = 3 << bp >> 1;
		int[] array = (int[])cblk.Data;
		int num5 = (cblk.h + StdEntropyCoderOptions.STRIPE_HEIGHT - 1) / StdEntropyCoderOptions.STRIPE_HEIGHT;
		bool flag = (options & StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL) != 0;
		int num6 = -num - 1;
		int num7 = -num + 1;
		int num8 = num + 1;
		int num9 = num - 1;
		int i = cblk.offset;
		int j = num + 1;
		int num10 = num5 - 1;
		while (num10 >= 0)
		{
			int num11 = ((num10 != 0) ? StdEntropyCoderOptions.STRIPE_HEIGHT : (cblk.h - (num5 - 1) * StdEntropyCoderOptions.STRIPE_HEIGHT));
			for (int num12 = i + cblk.w; i < num12; i++, j++)
			{
				int num13 = j;
				int num14 = state[num13];
				int num15;
				if ((~num14 & (num14 << 2) & SIG_MASK_R1R2) != 0)
				{
					num15 = i;
					if ((num14 & 0xA000) == 8192)
					{
						if (mq.decodeSymbol(zc_lut[num14 & 0xFF]) != 0)
						{
							int num16 = SC_LUT[SupportClass.URShift(num14, 4) & SC_MASK];
							int num17 = mq.decodeSymbol(num16 & 0xF) ^ SupportClass.URShift(num16, 31);
							array[num15] = (num17 << 31) | num4;
							if (!flag)
							{
								state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
								state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
							}
							if (num17 != 0)
							{
								num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
								if (!flag)
								{
									state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
								}
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
							}
							else
							{
								num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
								if (!flag)
								{
									state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
								}
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
							}
						}
						else
						{
							num14 |= 0x4000;
						}
					}
					if (num11 < 2)
					{
						state[num13] = num14;
						continue;
					}
					if ((num14 & (STATE_SIG_R2 | STATE_NZ_CTXT_R2)) == STATE_NZ_CTXT_R2)
					{
						num15 += scanw;
						if (mq.decodeSymbol(zc_lut[SupportClass.URShift(num14, 16) & 0xFF]) != 0)
						{
							int num16 = SC_LUT[SupportClass.URShift(num14, SC_SHIFT_R2) & SC_MASK];
							int num17 = mq.decodeSymbol(num16 & 0xF) ^ SupportClass.URShift(num16, 31);
							array[num15] = (num17 << 31) | num4;
							state[num13 + num9] |= 8196;
							state[num13 + num8] |= 8200;
							if (num17 != 0)
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10 | 0x200;
								state[num13 + num] |= 9248;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
							}
							else
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10;
								state[num13 + num] |= 8224;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
							}
						}
						else
						{
							num14 |= STATE_VISITED_R2;
						}
					}
					state[num13] = num14;
				}
				if (num11 < 3)
				{
					continue;
				}
				num13 += num;
				num14 = state[num13];
				if ((~num14 & (num14 << 2) & SIG_MASK_R1R2) == 0)
				{
					continue;
				}
				num15 = i + (scanw << 1);
				if ((num14 & 0xA000) == 8192)
				{
					if (mq.decodeSymbol(zc_lut[num14 & 0xFF]) != 0)
					{
						int num16 = SC_LUT[SupportClass.URShift(num14, 4) & SC_MASK];
						int num17 = mq.decodeSymbol(num16 & 0xF) ^ SupportClass.URShift(num16, 31);
						array[num15] = (num17 << 31) | num4;
						state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
						state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
						if (num17 != 0)
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
						}
						else
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
						}
					}
					else
					{
						num14 |= 0x4000;
					}
				}
				if (num11 < 4)
				{
					state[num13] = num14;
					continue;
				}
				if ((num14 & (STATE_SIG_R2 | STATE_NZ_CTXT_R2)) == STATE_NZ_CTXT_R2)
				{
					num15 += scanw;
					if (mq.decodeSymbol(zc_lut[SupportClass.URShift(num14, 16) & 0xFF]) != 0)
					{
						int num16 = SC_LUT[SupportClass.URShift(num14, SC_SHIFT_R2) & SC_MASK];
						int num17 = mq.decodeSymbol(num16 & 0xF) ^ SupportClass.URShift(num16, 31);
						array[num15] = (num17 << 31) | num4;
						state[num13 + num9] |= 8196;
						state[num13 + num8] |= 8200;
						if (num17 != 0)
						{
							num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10 | 0x200;
							state[num13 + num] |= 9248;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
						}
						else
						{
							num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10;
							state[num13 + num] |= 8224;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
						}
					}
					else
					{
						num14 |= STATE_VISITED_R2;
					}
				}
				state[num13] = num14;
			}
			num10--;
			i += num3;
			j += num2;
		}
		bool result = false;
		if (isterm && (options & StdEntropyCoderOptions.OPT_PRED_TERM) != 0)
		{
			result = mq.checkPredTerm();
		}
		if ((options & StdEntropyCoderOptions.OPT_RESET_MQ) != 0)
		{
			mq.resetCtxts();
		}
		return result;
	}

	private bool rawSigProgPass(DataBlock cblk, ByteToBitInput bin, int bp, int[] state, bool isterm)
	{
		int scanw = cblk.scanw;
		int num = cblk.w + 2;
		int num2 = num * StdEntropyCoderOptions.STRIPE_HEIGHT / 2 - cblk.w;
		int num3 = scanw * StdEntropyCoderOptions.STRIPE_HEIGHT - cblk.w;
		int num4 = 3 << bp >> 1;
		int[] array = (int[])cblk.Data;
		int num5 = (cblk.h + StdEntropyCoderOptions.STRIPE_HEIGHT - 1) / StdEntropyCoderOptions.STRIPE_HEIGHT;
		bool flag = (options & StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL) != 0;
		int num6 = -num - 1;
		int num7 = -num + 1;
		int num8 = num + 1;
		int num9 = num - 1;
		int i = cblk.offset;
		int j = num + 1;
		int num10 = num5 - 1;
		while (num10 >= 0)
		{
			int num11 = ((num10 != 0) ? StdEntropyCoderOptions.STRIPE_HEIGHT : (cblk.h - (num5 - 1) * StdEntropyCoderOptions.STRIPE_HEIGHT));
			for (int num12 = i + cblk.w; i < num12; i++, j++)
			{
				int num13 = j;
				int num14 = state[num13];
				int num15;
				if ((~num14 & (num14 << 2) & SIG_MASK_R1R2) != 0)
				{
					num15 = i;
					if ((num14 & 0xA000) == 8192)
					{
						if (bin.readBit() != 0)
						{
							int num16 = bin.readBit();
							array[num15] = (num16 << 31) | num4;
							if (!flag)
							{
								state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
								state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
							}
							if (num16 != 0)
							{
								num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
								if (!flag)
								{
									state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
								}
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
							}
							else
							{
								num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
								if (!flag)
								{
									state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
								}
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
							}
						}
						else
						{
							num14 |= 0x4000;
						}
					}
					if (num11 < 2)
					{
						state[num13] = num14;
						continue;
					}
					if ((num14 & (STATE_SIG_R2 | STATE_NZ_CTXT_R2)) == STATE_NZ_CTXT_R2)
					{
						num15 += scanw;
						if (bin.readBit() != 0)
						{
							int num16 = bin.readBit();
							array[num15] = (num16 << 31) | num4;
							state[num13 + num9] |= 8196;
							state[num13 + num8] |= 8200;
							if (num16 != 0)
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10 | 0x200;
								state[num13 + num] |= 9248;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
							}
							else
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10;
								state[num13 + num] |= 8224;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
							}
						}
						else
						{
							num14 |= STATE_VISITED_R2;
						}
					}
					state[num13] = num14;
				}
				if (num11 < 3)
				{
					continue;
				}
				num13 += num;
				num14 = state[num13];
				if ((~num14 & (num14 << 2) & SIG_MASK_R1R2) == 0)
				{
					continue;
				}
				num15 = i + (scanw << 1);
				if ((num14 & 0xA000) == 8192)
				{
					if (bin.readBit() != 0)
					{
						int num16 = bin.readBit();
						array[num15] = (num16 << 31) | num4;
						state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
						state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
						if (num16 != 0)
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
						}
						else
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
						}
					}
					else
					{
						num14 |= 0x4000;
					}
				}
				if (num11 < 4)
				{
					state[num13] = num14;
					continue;
				}
				if ((num14 & (STATE_SIG_R2 | STATE_NZ_CTXT_R2)) == STATE_NZ_CTXT_R2)
				{
					num15 += scanw;
					if (bin.readBit() != 0)
					{
						int num16 = bin.readBit();
						array[num15] = (num16 << 31) | num4;
						state[num13 + num9] |= 8196;
						state[num13 + num8] |= 8200;
						if (num16 != 0)
						{
							num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10 | 0x200;
							state[num13 + num] |= 9248;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
						}
						else
						{
							num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10;
							state[num13 + num] |= 8224;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
						}
					}
					else
					{
						num14 |= STATE_VISITED_R2;
					}
				}
				state[num13] = num14;
			}
			num10--;
			i += num3;
			j += num2;
		}
		bool result = false;
		if (isterm && (options & StdEntropyCoderOptions.OPT_PRED_TERM) != 0)
		{
			result = bin.checkBytePadding();
		}
		return result;
	}

	private bool magRefPass(DataBlock cblk, MQDecoder mq, int bp, int[] state, bool isterm)
	{
		int scanw = cblk.scanw;
		int num = cblk.w + 2;
		int num2 = num * StdEntropyCoderOptions.STRIPE_HEIGHT / 2 - cblk.w;
		int num3 = scanw * StdEntropyCoderOptions.STRIPE_HEIGHT - cblk.w;
		int num4 = 1 << bp >> 1;
		int num5 = -1 << bp + 1;
		int[] array = (int[])cblk.Data;
		int num6 = (cblk.h + StdEntropyCoderOptions.STRIPE_HEIGHT - 1) / StdEntropyCoderOptions.STRIPE_HEIGHT;
		int i = cblk.offset;
		int j = num + 1;
		int num7 = num6 - 1;
		while (num7 >= 0)
		{
			int num8 = ((num7 != 0) ? StdEntropyCoderOptions.STRIPE_HEIGHT : (cblk.h - (num6 - 1) * StdEntropyCoderOptions.STRIPE_HEIGHT));
			for (int num9 = i + cblk.w; i < num9; i++, j++)
			{
				int num10 = j;
				int num11 = state[num10];
				int num12;
				if ((SupportClass.URShift(num11, 1) & ~num11 & VSTD_MASK_R1R2) != 0)
				{
					num12 = i;
					if ((num11 & 0xC000) == 32768)
					{
						int num13 = mq.decodeSymbol(MR_LUT[num11 & 0x1FF]);
						array[num12] &= num5;
						array[num12] |= (num13 << bp) | num4;
						num11 |= 0x100;
					}
					if (num8 < 2)
					{
						state[num10] = num11;
						continue;
					}
					if ((num11 & (STATE_SIG_R2 | STATE_VISITED_R2)) == STATE_SIG_R2)
					{
						num12 += scanw;
						int num13 = mq.decodeSymbol(MR_LUT[SupportClass.URShift(num11, 16) & 0x1FF]);
						array[num12] &= num5;
						array[num12] |= (num13 << bp) | num4;
						num11 |= STATE_PREV_MR_R2;
					}
					state[num10] = num11;
				}
				if (num8 < 3)
				{
					continue;
				}
				num10 += num;
				num11 = state[num10];
				if ((SupportClass.URShift(num11, 1) & ~num11 & VSTD_MASK_R1R2) == 0)
				{
					continue;
				}
				num12 = i + (scanw << 1);
				if ((num11 & 0xC000) == 32768)
				{
					int num13 = mq.decodeSymbol(MR_LUT[num11 & 0x1FF]);
					array[num12] &= num5;
					array[num12] |= (num13 << bp) | num4;
					num11 |= 0x100;
				}
				if (num8 < 4)
				{
					state[num10] = num11;
					continue;
				}
				if ((state[num10] & (STATE_SIG_R2 | STATE_VISITED_R2)) == STATE_SIG_R2)
				{
					num12 += scanw;
					int num13 = mq.decodeSymbol(MR_LUT[SupportClass.URShift(num11, 16) & 0x1FF]);
					array[num12] &= num5;
					array[num12] |= (num13 << bp) | num4;
					num11 |= STATE_PREV_MR_R2;
				}
				state[num10] = num11;
			}
			num7--;
			i += num3;
			j += num2;
		}
		bool result = false;
		if (isterm && (options & StdEntropyCoderOptions.OPT_PRED_TERM) != 0)
		{
			result = mq.checkPredTerm();
		}
		if ((options & StdEntropyCoderOptions.OPT_RESET_MQ) != 0)
		{
			mq.resetCtxts();
		}
		return result;
	}

	private bool rawMagRefPass(DataBlock cblk, ByteToBitInput bin, int bp, int[] state, bool isterm)
	{
		int scanw = cblk.scanw;
		int num = cblk.w + 2;
		int num2 = num * StdEntropyCoderOptions.STRIPE_HEIGHT / 2 - cblk.w;
		int num3 = scanw * StdEntropyCoderOptions.STRIPE_HEIGHT - cblk.w;
		int num4 = 1 << bp >> 1;
		int num5 = -1 << bp + 1;
		int[] array = (int[])cblk.Data;
		int num6 = (cblk.h + StdEntropyCoderOptions.STRIPE_HEIGHT - 1) / StdEntropyCoderOptions.STRIPE_HEIGHT;
		int i = cblk.offset;
		int j = num + 1;
		int num7 = num6 - 1;
		while (num7 >= 0)
		{
			int num8 = ((num7 != 0) ? StdEntropyCoderOptions.STRIPE_HEIGHT : (cblk.h - (num6 - 1) * StdEntropyCoderOptions.STRIPE_HEIGHT));
			for (int num9 = i + cblk.w; i < num9; i++, j++)
			{
				int num10 = j;
				int num11 = state[num10];
				if ((SupportClass.URShift(num11, 1) & ~num11 & VSTD_MASK_R1R2) != 0)
				{
					int num12 = i;
					if ((num11 & 0xC000) == 32768)
					{
						int num13 = bin.readBit();
						array[num12] &= num5;
						array[num12] |= (num13 << bp) | num4;
					}
					if (num8 < 2)
					{
						continue;
					}
					if ((num11 & (STATE_SIG_R2 | STATE_VISITED_R2)) == STATE_SIG_R2)
					{
						num12 += scanw;
						int num13 = bin.readBit();
						array[num12] &= num5;
						array[num12] |= (num13 << bp) | num4;
					}
				}
				if (num8 < 3)
				{
					continue;
				}
				num10 += num;
				num11 = state[num10];
				if ((SupportClass.URShift(num11, 1) & ~num11 & VSTD_MASK_R1R2) != 0)
				{
					int num12 = i + (scanw << 1);
					if ((num11 & 0xC000) == 32768)
					{
						int num13 = bin.readBit();
						array[num12] &= num5;
						array[num12] |= (num13 << bp) | num4;
					}
					if (num8 >= 4 && (state[num10] & (STATE_SIG_R2 | STATE_VISITED_R2)) == STATE_SIG_R2)
					{
						num12 += scanw;
						int num13 = bin.readBit();
						array[num12] &= num5;
						array[num12] |= (num13 << bp) | num4;
					}
				}
			}
			num7--;
			i += num3;
			j += num2;
		}
		bool result = false;
		if (isterm && (options & StdEntropyCoderOptions.OPT_PRED_TERM) != 0)
		{
			result = bin.checkBytePadding();
		}
		return result;
	}

	private bool cleanuppass(DataBlock cblk, MQDecoder mq, int bp, int[] state, int[] zc_lut, bool isterm)
	{
		int scanw = cblk.scanw;
		int num = cblk.w + 2;
		int num2 = num * StdEntropyCoderOptions.STRIPE_HEIGHT / 2 - cblk.w;
		int num3 = scanw * StdEntropyCoderOptions.STRIPE_HEIGHT - cblk.w;
		int num4 = 3 << bp >> 1;
		int[] array = (int[])cblk.Data;
		int num5 = (cblk.h + StdEntropyCoderOptions.STRIPE_HEIGHT - 1) / StdEntropyCoderOptions.STRIPE_HEIGHT;
		bool flag = (options & StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL) != 0;
		int num6 = -num - 1;
		int num7 = -num + 1;
		int num8 = num + 1;
		int num9 = num - 1;
		int i = cblk.offset;
		int j = num + 1;
		int num10 = num5 - 1;
		while (num10 >= 0)
		{
			int num11 = ((num10 != 0) ? StdEntropyCoderOptions.STRIPE_HEIGHT : (cblk.h - (num5 - 1) * StdEntropyCoderOptions.STRIPE_HEIGHT));
			for (int num12 = i + cblk.w; i < num12; i++, j++)
			{
				int num13 = j;
				int num14 = state[num13];
				if (num14 != 0 || state[num13 + num] != 0 || num11 != StdEntropyCoderOptions.STRIPE_HEIGHT)
				{
					goto IL_0448;
				}
				if (mq.decodeSymbol(1) == 0)
				{
					continue;
				}
				int num15 = mq.decodeSymbol(0) << 1;
				num15 |= mq.decodeSymbol(0);
				int num16 = i + num15 * scanw;
				if (num15 > 1)
				{
					num13 += num;
					num14 = state[num13];
				}
				if ((num15 & 1) == 0)
				{
					int num17 = SC_LUT[(num14 >> 4) & SC_MASK];
					int num18 = mq.decodeSymbol(num17 & 0xF) ^ SupportClass.URShift(num17, 31);
					array[num16] = (num18 << 31) | num4;
					if (num15 != 0 || !flag)
					{
						state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
						state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
					}
					if (num18 != 0)
					{
						num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
						if (num15 != 0 || !flag)
						{
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
						}
						state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
						state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
					}
					else
					{
						num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
						if (num15 != 0 || !flag)
						{
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
						}
						state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
						state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
					}
					if (num15 >> 1 == 0)
					{
						goto IL_0448;
					}
				}
				else
				{
					int num17 = SC_LUT[(num14 >> SC_SHIFT_R2) & SC_MASK];
					int num18 = mq.decodeSymbol(num17 & 0xF) ^ SupportClass.URShift(num17, 31);
					array[num16] = (num18 << 31) | num4;
					state[num13 + num9] |= 8196;
					state[num13 + num8] |= 8200;
					if (num18 != 0)
					{
						num14 |= STATE_SIG_R2 | 0x2000 | 0x10 | 0x200;
						state[num13 + num] |= 9248;
						state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
						state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
					}
					else
					{
						num14 |= STATE_SIG_R2 | 0x2000 | 0x10;
						state[num13 + num] |= 8224;
						state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
						state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
					}
					state[num13] = num14;
					if (num15 >> 1 != 0)
					{
						continue;
					}
					num13 += num;
					num14 = state[num13];
				}
				goto IL_07ea;
				IL_0448:
				if ((((num14 >> 1) | num14) & VSTD_MASK_R1R2) != VSTD_MASK_R1R2)
				{
					num16 = i;
					if ((num14 & 0xC000) == 0 && mq.decodeSymbol(zc_lut[num14 & 0xFF]) != 0)
					{
						int num17 = SC_LUT[SupportClass.URShift(num14, 4) & SC_MASK];
						int num18 = mq.decodeSymbol(num17 & 0xF) ^ SupportClass.URShift(num17, 31);
						array[num16] = (num18 << 31) | num4;
						if (!flag)
						{
							state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
							state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
						}
						if (num18 != 0)
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
							if (!flag)
							{
								state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
							}
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
						}
						else
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
							if (!flag)
							{
								state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
							}
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
						}
					}
					if (num11 < 2)
					{
						num14 &= ~(0x4000 | STATE_VISITED_R2);
						state[num13] = num14;
						continue;
					}
					if ((num14 & (STATE_SIG_R2 | STATE_VISITED_R2)) == 0)
					{
						num16 += scanw;
						if (mq.decodeSymbol(zc_lut[SupportClass.URShift(num14, 16) & 0xFF]) != 0)
						{
							int num17 = SC_LUT[SupportClass.URShift(num14, SC_SHIFT_R2) & SC_MASK];
							int num18 = mq.decodeSymbol(num17 & 0xF) ^ SupportClass.URShift(num17, 31);
							array[num16] = (num18 << 31) | num4;
							state[num13 + num9] |= 8196;
							state[num13 + num8] |= 8200;
							if (num18 != 0)
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10 | 0x200;
								state[num13 + num] |= 9248;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
							}
							else
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10;
								state[num13 + num] |= 8224;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
							}
						}
					}
				}
				num14 &= ~(0x4000 | STATE_VISITED_R2);
				state[num13] = num14;
				if (num11 < 3)
				{
					continue;
				}
				num13 += num;
				num14 = state[num13];
				goto IL_07ea;
				IL_07ea:
				if ((((num14 >> 1) | num14) & VSTD_MASK_R1R2) != VSTD_MASK_R1R2)
				{
					num16 = i + (scanw << 1);
					if ((num14 & 0xC000) == 0 && mq.decodeSymbol(zc_lut[num14 & 0xFF]) != 0)
					{
						int num17 = SC_LUT[(num14 >> 4) & SC_MASK];
						int num18 = mq.decodeSymbol(num17 & 0xF) ^ SupportClass.URShift(num17, 31);
						array[num16] = (num18 << 31) | num4;
						state[num13 + num6] |= STATE_NZ_CTXT_R2 | STATE_D_DR_R2;
						state[num13 + num7] |= STATE_NZ_CTXT_R2 | STATE_D_DL_R2;
						if (num18 != 0)
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2 | STATE_V_U_SIGN_R2;
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2 | STATE_V_D_SIGN_R2;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | 0x1000 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | 0x800 | STATE_D_UR_R2;
						}
						else
						{
							num14 |= 0xC000 | STATE_NZ_CTXT_R2 | STATE_V_U_R2;
							state[num13 - num] |= STATE_NZ_CTXT_R2 | STATE_V_D_R2;
							state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x80 | STATE_D_UL_R2;
							state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 0x40 | STATE_D_UR_R2;
						}
					}
					if (num11 < 4)
					{
						num14 &= ~(0x4000 | STATE_VISITED_R2);
						state[num13] = num14;
						continue;
					}
					if ((num14 & (STATE_SIG_R2 | STATE_VISITED_R2)) == 0)
					{
						num16 += scanw;
						if (mq.decodeSymbol(zc_lut[SupportClass.URShift(num14, 16) & 0xFF]) != 0)
						{
							int num17 = SC_LUT[SupportClass.URShift(num14, SC_SHIFT_R2) & SC_MASK];
							int num18 = mq.decodeSymbol(num17 & 0xF) ^ SupportClass.URShift(num17, 31);
							array[num16] = (num18 << 31) | num4;
							state[num13 + num9] |= 8196;
							state[num13 + num8] |= 8200;
							if (num18 != 0)
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10 | 0x200;
								state[num13 + num] |= 9248;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2 | STATE_H_L_SIGN_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2 | STATE_H_R_SIGN_R2;
							}
							else
							{
								num14 |= STATE_SIG_R2 | STATE_VISITED_R2 | 0x2000 | 0x10;
								state[num13 + num] |= 8224;
								state[num13 + 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 2 | STATE_H_L_R2;
								state[num13 - 1] |= 0x2000 | STATE_NZ_CTXT_R2 | 1 | STATE_H_R_R2;
							}
						}
					}
				}
				num14 &= ~(0x4000 | STATE_VISITED_R2);
				state[num13] = num14;
			}
			num10--;
			i += num3;
			j += num2;
		}
		bool result;
		if ((options & StdEntropyCoderOptions.OPT_SEG_SYMBOLS) != 0)
		{
			int num18 = mq.decodeSymbol(0) << 3;
			num18 |= mq.decodeSymbol(0) << 2;
			num18 |= mq.decodeSymbol(0) << 1;
			num18 |= mq.decodeSymbol(0);
			result = num18 != 10;
		}
		else
		{
			result = false;
		}
		if (isterm && (options & StdEntropyCoderOptions.OPT_PRED_TERM) != 0)
		{
			result = mq.checkPredTerm();
		}
		if ((options & StdEntropyCoderOptions.OPT_RESET_MQ) != 0)
		{
			mq.resetCtxts();
		}
		return result;
	}

	private void conceal(DataBlock cblk, int bp)
	{
		int num = 1 << bp;
		int num2 = -1 << bp;
		int[] array = (int[])cblk.Data;
		int num3 = cblk.h - 1;
		int i = cblk.offset;
		while (num3 >= 0)
		{
			for (int num4 = i + cblk.w; i < num4; i++)
			{
				int num5 = array[i];
				if (((uint)(num5 & num2) & 0x7FFFFFFFu) != 0)
				{
					array[i] = (num5 & num2) | num;
				}
				else
				{
					array[i] = 0;
				}
			}
			i += cblk.scanw - cblk.w;
			num3--;
		}
	}

	static StdEntropyDecoder()
	{
		ZC_LUT_LH = new int[256];
		ZC_LUT_HL = new int[256];
		ZC_LUT_HH = new int[256];
		SC_LUT = new int[512];
		MR_LUT = new int[512];
		MQ_INIT = new int[19]
		{
			46, 3, 4, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0
		};
		STATE_SIG_R2 = int.MinValue;
		STATE_VISITED_R2 = 1073741824;
		STATE_NZ_CTXT_R2 = 536870912;
		STATE_H_L_SIGN_R2 = 268435456;
		STATE_H_R_SIGN_R2 = 134217728;
		STATE_V_U_SIGN_R2 = 67108864;
		STATE_V_D_SIGN_R2 = 33554432;
		STATE_PREV_MR_R2 = 16777216;
		STATE_H_L_R2 = 8388608;
		STATE_H_R_R2 = 4194304;
		STATE_V_U_R2 = 2097152;
		STATE_V_D_R2 = 1048576;
		STATE_D_UL_R2 = 524288;
		STATE_D_UR_R2 = 262144;
		STATE_D_DL_R2 = 131072;
		STATE_D_DR_R2 = 65536;
		SIG_MASK_R1R2 = 0x8000 | STATE_SIG_R2;
		VSTD_MASK_R1R2 = 0x4000 | STATE_VISITED_R2;
		RLC_MASK_R1R2 = 0x8000 | STATE_SIG_R2 | 0x4000 | STATE_VISITED_R2 | 0x2000 | STATE_NZ_CTXT_R2;
		SC_SHIFT_R2 = 20;
		SC_MASK = 511;
		ZC_LUT_LH[0] = 2;
		int i;
		for (i = 1; i < 16; i++)
		{
			ZC_LUT_LH[i] = 4;
		}
		for (i = 0; i < 4; i++)
		{
			ZC_LUT_LH[1 << i] = 3;
		}
		for (i = 0; i < 16; i++)
		{
			ZC_LUT_LH[0x20 | i] = 5;
			ZC_LUT_LH[0x10 | i] = 5;
			ZC_LUT_LH[0x30 | i] = 6;
		}
		ZC_LUT_LH[128] = 7;
		ZC_LUT_LH[64] = 7;
		for (i = 1; i < 16; i++)
		{
			ZC_LUT_LH[0x80 | i] = 8;
			ZC_LUT_LH[0x40 | i] = 8;
		}
		for (i = 1; i < 4; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				ZC_LUT_LH[0x80 | (i << 4) | j] = 9;
				ZC_LUT_LH[0x40 | (i << 4) | j] = 9;
			}
		}
		for (i = 0; i < 64; i++)
		{
			ZC_LUT_LH[0xC0 | i] = 10;
		}
		ZC_LUT_HL[0] = 2;
		for (i = 1; i < 16; i++)
		{
			ZC_LUT_HL[i] = 4;
		}
		for (i = 0; i < 4; i++)
		{
			ZC_LUT_HL[1 << i] = 3;
		}
		for (i = 0; i < 16; i++)
		{
			ZC_LUT_HL[0x80 | i] = 5;
			ZC_LUT_HL[0x40 | i] = 5;
			ZC_LUT_HL[0xC0 | i] = 6;
		}
		ZC_LUT_HL[32] = 7;
		ZC_LUT_HL[16] = 7;
		for (i = 1; i < 16; i++)
		{
			ZC_LUT_HL[0x20 | i] = 8;
			ZC_LUT_HL[0x10 | i] = 8;
		}
		for (i = 1; i < 4; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				ZC_LUT_HL[(i << 6) | 0x20 | j] = 9;
				ZC_LUT_HL[(i << 6) | 0x10 | j] = 9;
			}
		}
		for (i = 0; i < 4; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				ZC_LUT_HL[(i << 6) | 0x20 | 0x10 | j] = 10;
			}
		}
		int[] array = new int[6] { 3, 5, 6, 9, 10, 12 };
		int[] array2 = new int[4] { 1, 2, 4, 8 };
		int[] array3 = new int[11]
		{
			3, 5, 6, 7, 9, 10, 11, 12, 13, 14,
			15
		};
		int[] array4 = new int[5] { 7, 11, 13, 14, 15 };
		ZC_LUT_HH[0] = 2;
		for (i = 0; i < array2.Length; i++)
		{
			ZC_LUT_HH[array2[i] << 4] = 3;
		}
		for (i = 0; i < array3.Length; i++)
		{
			ZC_LUT_HH[array3[i] << 4] = 4;
		}
		for (i = 0; i < array2.Length; i++)
		{
			ZC_LUT_HH[array2[i]] = 5;
		}
		for (i = 0; i < array2.Length; i++)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				ZC_LUT_HH[(array2[i] << 4) | array2[j]] = 6;
			}
		}
		for (i = 0; i < array3.Length; i++)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				ZC_LUT_HH[(array3[i] << 4) | array2[j]] = 7;
			}
		}
		for (i = 0; i < array.Length; i++)
		{
			ZC_LUT_HH[array[i]] = 8;
		}
		for (int j = 0; j < array.Length; j++)
		{
			for (i = 1; i < 16; i++)
			{
				ZC_LUT_HH[(i << 4) | array[j]] = 9;
			}
		}
		for (i = 0; i < 16; i++)
		{
			for (int j = 0; j < array4.Length; j++)
			{
				ZC_LUT_HH[(i << 4) | array4[j]] = 10;
			}
		}
		int[] array5 = new int[36];
		array5[18] = 15;
		array5[17] = 14;
		array5[16] = 13;
		array5[10] = 12;
		array5[9] = 11;
		array5[8] = -2147483636;
		array5[2] = -2147483635;
		array5[1] = -2147483634;
		array5[0] = -2147483633;
		for (i = 0; i < 511; i++)
		{
			int num = i & 1;
			int num2 = (i >> 1) & 1;
			int num3 = (i >> 2) & 1;
			int num4 = (i >> 3) & 1;
			int num5 = (i >> 5) & 1;
			int num6 = (i >> 6) & 1;
			int num7 = (i >> 7) & 1;
			int num8 = (i >> 8) & 1;
			int num9 = num4 * (1 - 2 * num8) + num3 * (1 - 2 * num7);
			num9 = ((num9 >= -1) ? num9 : (-1));
			num9 = ((num9 > 1) ? 1 : num9);
			int num10 = num2 * (1 - 2 * num6) + num * (1 - 2 * num5);
			num10 = ((num10 >= -1) ? num10 : (-1));
			num10 = ((num10 > 1) ? 1 : num10);
			SC_LUT[i] = array5[(num9 + 1 << 3) | (num10 + 1)];
		}
		array5 = null;
		MR_LUT[0] = 16;
		for (i = 1; i < 256; i++)
		{
			MR_LUT[i] = 17;
		}
		for (; i < 512; i++)
		{
			MR_LUT[i] = 18;
		}
	}
}
