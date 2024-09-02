using System;

namespace BitMiracle.LibJpeg.Classic.Internal;

internal class jpeg_inverse_dct
{
	private delegate void inverse_method(int component_index, short[] coef_block, int output_row, int output_col);

	private class multiplier_table
	{
		public int[] int_array = new int[64];

		public float[] float_array = new float[64];
	}

	private const int IFAST_SCALE_BITS = 2;

	private const int RANGE_MASK = 1023;

	private const int RANGE_CENTER = 512;

	private const int RANGE_SUBSET = 384;

	private const int SLOW_INTEGER_CONST_BITS = 13;

	private const int SLOW_INTEGER_PASS1_BITS = 2;

	private const int SLOW_INTEGER_PASS1_PLUS3_BITS = 18;

	private const int SLOW_INTEGER_FIX_0_298631336 = 2446;

	private const int SLOW_INTEGER_FIX_0_390180644 = 3196;

	private const int SLOW_INTEGER_FIX_0_541196100 = 4433;

	private const int SLOW_INTEGER_FIX_0_765366865 = 6270;

	private const int SLOW_INTEGER_FIX_0_899976223 = 7373;

	private const int SLOW_INTEGER_FIX_1_175875602 = 9633;

	private const int SLOW_INTEGER_FIX_1_501321110 = 12299;

	private const int SLOW_INTEGER_FIX_1_847759065 = 15137;

	private const int SLOW_INTEGER_FIX_1_961570560 = 16069;

	private const int SLOW_INTEGER_FIX_2_053119869 = 16819;

	private const int SLOW_INTEGER_FIX_2_562915447 = 20995;

	private const int SLOW_INTEGER_FIX_3_072711026 = 25172;

	private const int FAST_INTEGER_CONST_BITS = 8;

	private const int FAST_INTEGER_PASS1_BITS = 2;

	private const int FAST_INTEGER_FIX_1_082392200 = 277;

	private const int FAST_INTEGER_FIX_1_414213562 = 362;

	private const int FAST_INTEGER_FIX_1_847759065 = 473;

	private const int FAST_INTEGER_FIX_2_613125930 = 669;

	private const int REDUCED_CONST_BITS = 13;

	private const int REDUCED_PASS1_BITS = 2;

	private const int REDUCED_FIX_0_211164243 = 1730;

	private const int REDUCED_FIX_0_509795579 = 4176;

	private const int REDUCED_FIX_0_601344887 = 4926;

	private const int REDUCED_FIX_0_720959822 = 5906;

	private const int REDUCED_FIX_0_765366865 = 6270;

	private const int REDUCED_FIX_0_850430095 = 6967;

	private const int REDUCED_FIX_0_899976223 = 7373;

	private const int REDUCED_FIX_1_061594337 = 8697;

	private const int REDUCED_FIX_1_272758580 = 10426;

	private const int REDUCED_FIX_1_451774981 = 11893;

	private const int REDUCED_FIX_1_847759065 = 15137;

	private const int REDUCED_FIX_2_172734803 = 17799;

	private const int REDUCED_FIX_2_562915447 = 20995;

	private const int REDUCED_FIX_3_624509785 = 29692;

	private static readonly short[] aanscales = new short[64]
	{
		16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 22725, 31521,
		29692, 26722, 22725, 17855, 12299, 6270, 21407, 29692, 27969, 25172,
		21407, 16819, 11585, 5906, 19266, 26722, 25172, 22654, 19266, 15137,
		10426, 5315, 16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520,
		12873, 17855, 16819, 15137, 12873, 10114, 6967, 3552, 8867, 12299,
		11585, 10426, 8867, 6967, 4799, 2446, 4520, 6270, 5906, 5315,
		4520, 3552, 2446, 1247
	};

	private const int CONST_BITS = 14;

	private static readonly double[] aanscalefactor = new double[8] { 1.0, 1.387039845, 1.306562965, 1.175875602, 1.0, 0.785694958, 0.5411961, 0.275899379 };

	private inverse_method[] m_inverse_DCT_method = new inverse_method[10];

	private multiplier_table[] m_dctTables;

	private jpeg_decompress_struct m_cinfo;

	private int[] m_cur_method = new int[10];

	private ComponentBuffer m_componentBuffer;

	private int[] m_workspace = new int[Math.Max(64, 128)];

	private float[] m_fworkspace = new float[Math.Max(64, 128)];

	public jpeg_inverse_dct(jpeg_decompress_struct cinfo)
	{
		m_cinfo = cinfo;
		m_dctTables = new multiplier_table[cinfo.m_num_components];
		for (int i = 0; i < cinfo.m_num_components; i++)
		{
			m_dctTables[i] = new multiplier_table();
			m_cur_method[i] = -1;
		}
	}

	public void start_pass()
	{
		for (int i = 0; i < m_cinfo.m_num_components; i++)
		{
			jpeg_component_info jpeg_component_info = m_cinfo.Comp_info[i];
			inverse_method inverse_method = null;
			int num = 0;
			switch ((jpeg_component_info.DCT_h_scaled_size << 8) + jpeg_component_info.DCT_v_scaled_size)
			{
			case 257:
				inverse_method = jpeg_idct_1x1;
				num = 0;
				break;
			case 514:
				inverse_method = jpeg_idct_2x2;
				num = 0;
				break;
			case 771:
				inverse_method = jpeg_idct_3x3;
				num = 0;
				break;
			case 1028:
				inverse_method = jpeg_idct_4x4;
				num = 0;
				break;
			case 1285:
				inverse_method = jpeg_idct_5x5;
				num = 0;
				break;
			case 1542:
				inverse_method = jpeg_idct_6x6;
				num = 0;
				break;
			case 1799:
				inverse_method = jpeg_idct_7x7;
				num = 0;
				break;
			case 2313:
				inverse_method = jpeg_idct_9x9;
				num = 0;
				break;
			case 2570:
				inverse_method = jpeg_idct_10x10;
				num = 0;
				break;
			case 2827:
				inverse_method = jpeg_idct_11x11;
				num = 0;
				break;
			case 3084:
				inverse_method = jpeg_idct_12x12;
				num = 0;
				break;
			case 3341:
				inverse_method = jpeg_idct_13x13;
				num = 0;
				break;
			case 3598:
				inverse_method = jpeg_idct_14x14;
				num = 0;
				break;
			case 3855:
				inverse_method = jpeg_idct_15x15;
				num = 0;
				break;
			case 4112:
				inverse_method = jpeg_idct_16x16;
				num = 0;
				break;
			case 4104:
				inverse_method = jpeg_idct_16x8;
				num = 0;
				break;
			case 3591:
				inverse_method = jpeg_idct_14x7;
				num = 0;
				break;
			case 3078:
				inverse_method = jpeg_idct_12x6;
				num = 0;
				break;
			case 2565:
				inverse_method = jpeg_idct_10x5;
				num = 0;
				break;
			case 2052:
				inverse_method = jpeg_idct_8x4;
				num = 0;
				break;
			case 1539:
				inverse_method = jpeg_idct_6x3;
				num = 0;
				break;
			case 1026:
				inverse_method = jpeg_idct_4x2;
				num = 0;
				break;
			case 513:
				inverse_method = jpeg_idct_2x1;
				num = 0;
				break;
			case 2064:
				inverse_method = jpeg_idct_8x16;
				num = 0;
				break;
			case 1806:
				inverse_method = jpeg_idct_7x14;
				num = 0;
				break;
			case 1548:
				inverse_method = jpeg_idct_6x12;
				num = 0;
				break;
			case 1290:
				inverse_method = jpeg_idct_5x10;
				num = 0;
				break;
			case 1032:
				inverse_method = jpeg_idct_4x8;
				num = 0;
				break;
			case 774:
				inverse_method = jpeg_idct_3x6;
				num = 0;
				break;
			case 516:
				inverse_method = jpeg_idct_2x4;
				num = 0;
				break;
			case 258:
				inverse_method = jpeg_idct_1x2;
				num = 0;
				break;
			case 2056:
				switch (m_cinfo.m_dct_method)
				{
				case J_DCT_METHOD.JDCT_ISLOW:
					inverse_method = jpeg_idct_islow;
					num = 0;
					break;
				case J_DCT_METHOD.JDCT_IFAST:
					inverse_method = jpeg_idct_ifast;
					num = 1;
					break;
				case J_DCT_METHOD.JDCT_FLOAT:
					inverse_method = jpeg_idct_float;
					num = 2;
					break;
				default:
					m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOT_COMPILED);
					break;
				}
				break;
			default:
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_DCTSIZE, jpeg_component_info.DCT_h_scaled_size, jpeg_component_info.DCT_v_scaled_size);
				break;
			}
			m_inverse_DCT_method[i] = inverse_method;
			if (!jpeg_component_info.component_needed || m_cur_method[i] == num || jpeg_component_info.quant_table == null)
			{
				continue;
			}
			m_cur_method[i] = num;
			switch ((J_DCT_METHOD)num)
			{
			case J_DCT_METHOD.JDCT_ISLOW:
			{
				int[] int_array = m_dctTables[i].int_array;
				for (int l = 0; l < 64; l++)
				{
					int_array[l] = jpeg_component_info.quant_table.quantval[l];
				}
				break;
			}
			case J_DCT_METHOD.JDCT_IFAST:
			{
				int[] int_array2 = m_dctTables[i].int_array;
				for (int m = 0; m < 64; m++)
				{
					int_array2[m] = JpegUtils.DESCALE(jpeg_component_info.quant_table.quantval[m] * aanscales[m], 12);
				}
				break;
			}
			case J_DCT_METHOD.JDCT_FLOAT:
			{
				float[] float_array = m_dctTables[i].float_array;
				int num2 = 0;
				for (int j = 0; j < 8; j++)
				{
					for (int k = 0; k < 8; k++)
					{
						float_array[num2] = (float)((double)jpeg_component_info.quant_table.quantval[num2] * aanscalefactor[j] * aanscalefactor[k] * 0.125);
						num2++;
					}
				}
				break;
			}
			default:
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOT_COMPILED);
				break;
			}
		}
	}

	public void inverse(int component_index, short[] coef_block, ComponentBuffer output_buf, int output_row, int output_col)
	{
		m_componentBuffer = output_buf;
		inverse_method inverse_method = m_inverse_DCT_method[component_index];
		if (inverse_method == null)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOT_COMPILED);
		}
		else
		{
			inverse_method(component_index, coef_block, output_row, output_col);
		}
	}

	private void jpeg_idct_islow(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int[] int_array = m_dctTables[component_index].int_array;
		int num = 8;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num > 0)
		{
			if (coef_block[num2 + 8] == 0 && coef_block[num2 + 16] == 0 && coef_block[num2 + 24] == 0 && coef_block[num2 + 32] == 0 && coef_block[num2 + 40] == 0 && coef_block[num2 + 48] == 0 && coef_block[num2 + 56] == 0)
			{
				int num5 = coef_block[num2] * int_array[num3] << 2;
				m_workspace[num4] = num5;
				m_workspace[num4 + 8] = num5;
				m_workspace[num4 + 16] = num5;
				m_workspace[num4 + 24] = num5;
				m_workspace[num4 + 32] = num5;
				m_workspace[num4 + 40] = num5;
				m_workspace[num4 + 48] = num5;
				m_workspace[num4 + 56] = num5;
			}
			else
			{
				int num6 = coef_block[num2] * int_array[num3];
				int num7 = coef_block[num2 + 32] * int_array[num3 + 32];
				num6 <<= 13;
				num7 <<= 13;
				num6 += 1024;
				int num8 = num6 + num7;
				int num9 = num6 - num7;
				num6 = coef_block[num2 + 16] * int_array[num3 + 16];
				num7 = coef_block[num2 + 48] * int_array[num3 + 48];
				int num10 = (num6 + num7) * 4433;
				int num11 = num10 + num6 * 6270;
				int num12 = num10 - num7 * 15137;
				int num13 = num8 + num11;
				int num14 = num8 - num11;
				int num15 = num9 + num12;
				int num16 = num9 - num12;
				num8 = coef_block[num2 + 56] * int_array[num3 + 56];
				num9 = coef_block[num2 + 40] * int_array[num3 + 40];
				num11 = coef_block[num2 + 24] * int_array[num3 + 24];
				num12 = coef_block[num2 + 8] * int_array[num3 + 8];
				num6 = num8 + num11;
				num7 = num9 + num12;
				num10 = (num6 + num7) * 9633;
				num6 *= -16069;
				num7 *= -3196;
				num6 += num10;
				num7 += num10;
				num10 = (num8 + num12) * -7373;
				num8 *= 2446;
				num12 *= 12299;
				num8 += num10 + num6;
				num12 += num10 + num7;
				num10 = (num9 + num11) * -20995;
				num9 *= 16819;
				num11 *= 25172;
				num9 += num10 + num7;
				num11 += num10 + num6;
				m_workspace[num4] = num13 + num12 >> 11;
				m_workspace[num4 + 56] = num13 - num12 >> 11;
				m_workspace[num4 + 8] = num15 + num11 >> 11;
				m_workspace[num4 + 48] = num15 - num11 >> 11;
				m_workspace[num4 + 16] = num16 + num9 >> 11;
				m_workspace[num4 + 40] = num16 - num9 >> 11;
				m_workspace[num4 + 24] = num14 + num8 >> 11;
				m_workspace[num4 + 32] = num14 - num8 >> 11;
			}
			num--;
			num2++;
			num3++;
			num4++;
		}
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num17 = m_cinfo.m_sampleRangeLimitOffset - 384;
		int num18 = 0;
		int num19 = 0;
		while (num18 < 8)
		{
			int num20 = m_workspace[num19] + 16384 + 16;
			int i = output_row + num18;
			byte[] array = m_componentBuffer[i];
			if (m_workspace[num19 + 1] == 0 && m_workspace[num19 + 2] == 0 && m_workspace[num19 + 3] == 0 && m_workspace[num19 + 4] == 0 && m_workspace[num19 + 5] == 0 && m_workspace[num19 + 6] == 0 && m_workspace[num19 + 7] == 0)
			{
				array[output_col + 7] = (array[output_col + 6] = (array[output_col + 5] = (array[output_col + 4] = (array[output_col + 3] = (array[output_col + 2] = (array[output_col + 1] = (array[output_col] = sample_range_limit[num17 + ((num20 >> 5) & 0x3FF)])))))));
			}
			else
			{
				int num21 = m_workspace[num19 + 4];
				int num22 = num20 + num21 << 13;
				int num23 = num20 - num21 << 13;
				num20 = m_workspace[num19 + 2];
				num21 = m_workspace[num19 + 6];
				int num24 = (num20 + num21) * 4433;
				int num25 = num24 + num20 * 6270;
				int num26 = num24 - num21 * 15137;
				int num27 = num22 + num25;
				int num28 = num22 - num25;
				int num29 = num23 + num26;
				int num30 = num23 - num26;
				num22 = m_workspace[num19 + 7];
				num23 = m_workspace[num19 + 5];
				num25 = m_workspace[num19 + 3];
				num26 = m_workspace[num19 + 1];
				num20 = num22 + num25;
				num21 = num23 + num26;
				num24 = (num20 + num21) * 9633;
				num20 *= -16069;
				num21 *= -3196;
				num20 += num24;
				num21 += num24;
				num24 = (num22 + num26) * -7373;
				num22 *= 2446;
				num26 *= 12299;
				num22 += num24 + num20;
				num26 += num24 + num21;
				num24 = (num23 + num25) * -20995;
				num23 *= 16819;
				num25 *= 25172;
				num23 += num24 + num21;
				num25 += num24 + num20;
				array[output_col] = sample_range_limit[(num17 + (num27 + num26 >> 18)) & 0x3FF];
				array[output_col + 7] = sample_range_limit[(num17 + (num27 - num26 >> 18)) & 0x3FF];
				array[output_col + 1] = sample_range_limit[(num17 + (num29 + num25 >> 18)) & 0x3FF];
				array[output_col + 6] = sample_range_limit[(num17 + (num29 - num25 >> 18)) & 0x3FF];
				array[output_col + 2] = sample_range_limit[(num17 + (num30 + num23 >> 18)) & 0x3FF];
				array[output_col + 5] = sample_range_limit[(num17 + (num30 - num23 >> 18)) & 0x3FF];
				array[output_col + 3] = sample_range_limit[(num17 + (num28 + num22 >> 18)) & 0x3FF];
				array[output_col + 4] = sample_range_limit[(num17 + (num28 - num22 >> 18)) & 0x3FF];
			}
			num18++;
			num19 += 8;
		}
	}

	private static int SLOW_INTEGER_FIX(double x)
	{
		return (int)(x * 8192.0 + 0.5);
	}

	private void jpeg_idct_ifast(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int num2 = 0;
		int[] int_array = m_dctTables[component_index].int_array;
		int num3 = 0;
		for (int num4 = 8; num4 > 0; num4--)
		{
			if (coef_block[num + 8] == 0 && coef_block[num + 16] == 0 && coef_block[num + 24] == 0 && coef_block[num + 32] == 0 && coef_block[num + 40] == 0 && coef_block[num + 48] == 0 && coef_block[num + 56] == 0)
			{
				int num5 = FAST_INTEGER_DEQUANTIZE(coef_block[num], int_array[num3]);
				m_workspace[num2] = num5;
				m_workspace[num2 + 8] = num5;
				m_workspace[num2 + 16] = num5;
				m_workspace[num2 + 24] = num5;
				m_workspace[num2 + 32] = num5;
				m_workspace[num2 + 40] = num5;
				m_workspace[num2 + 48] = num5;
				m_workspace[num2 + 56] = num5;
				num++;
				num3++;
				num2++;
			}
			else
			{
				int num6 = FAST_INTEGER_DEQUANTIZE(coef_block[num], int_array[num3]);
				int num7 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 16], int_array[num3 + 16]);
				int num8 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 32], int_array[num3 + 32]);
				int num9 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 48], int_array[num3 + 48]);
				int num10 = num6 + num8;
				int num11 = num6 - num8;
				int num12 = num7 + num9;
				int num13 = FAST_INTEGER_MULTIPLY(num7 - num9, 362) - num12;
				num6 = num10 + num12;
				num9 = num10 - num12;
				num7 = num11 + num13;
				num8 = num11 - num13;
				int num14 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 8], int_array[num3 + 8]);
				int num15 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 24], int_array[num3 + 24]);
				int num16 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 40], int_array[num3 + 40]);
				int num17 = FAST_INTEGER_DEQUANTIZE(coef_block[num + 56], int_array[num3 + 56]);
				int num18 = num16 + num15;
				int num19 = num16 - num15;
				int num20 = num14 + num17;
				int num21 = num14 - num17;
				num17 = num20 + num18;
				int num22 = FAST_INTEGER_MULTIPLY(num20 - num18, 362);
				int num23 = FAST_INTEGER_MULTIPLY(num19 + num21, 473);
				num10 = num23 - FAST_INTEGER_MULTIPLY(num21, 277);
				num13 = num23 - FAST_INTEGER_MULTIPLY(num19, 669);
				num16 = num13 - num17;
				num15 = num22 - num16;
				num14 = num10 - num15;
				m_workspace[num2] = num6 + num17;
				m_workspace[num2 + 56] = num6 - num17;
				m_workspace[num2 + 8] = num7 + num16;
				m_workspace[num2 + 48] = num7 - num16;
				m_workspace[num2 + 16] = num8 + num15;
				m_workspace[num2 + 40] = num8 - num15;
				m_workspace[num2 + 24] = num9 + num14;
				m_workspace[num2 + 32] = num9 - num14;
				num++;
				num3++;
				num2++;
			}
		}
		num2 = 0;
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num24 = m_cinfo.m_sampleRangeLimitOffset - 384;
		for (int i = 0; i < 8; i++)
		{
			int i2 = output_row + i;
			int num25 = m_workspace[num2] + 16400;
			byte[] array = m_componentBuffer[i2];
			if (m_workspace[num2 + 1] == 0 && m_workspace[num2 + 2] == 0 && m_workspace[num2 + 3] == 0 && m_workspace[num2 + 4] == 0 && m_workspace[num2 + 5] == 0 && m_workspace[num2 + 6] == 0 && m_workspace[num2 + 7] == 0)
			{
				array[output_col + 7] = (array[output_col + 6] = (array[output_col + 5] = (array[output_col + 4] = (array[output_col + 3] = (array[output_col + 2] = (array[output_col + 1] = (array[output_col] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num25, 5)) & 0x3FF])))))));
				num2 += 8;
				continue;
			}
			int num26 = num25 + m_workspace[num2 + 4];
			int num27 = num25 - m_workspace[num2 + 4];
			int num28 = m_workspace[num2 + 2] + m_workspace[num2 + 6];
			int num29 = FAST_INTEGER_MULTIPLY(m_workspace[num2 + 2] - m_workspace[num2 + 6], 362) - num28;
			int num30 = num26 + num28;
			int num31 = num26 - num28;
			int num32 = num27 + num29;
			int num33 = num27 - num29;
			int num34 = m_workspace[num2 + 5] + m_workspace[num2 + 3];
			int num35 = m_workspace[num2 + 5] - m_workspace[num2 + 3];
			int num36 = m_workspace[num2 + 1] + m_workspace[num2 + 7];
			int num37 = m_workspace[num2 + 1] - m_workspace[num2 + 7];
			int num38 = num36 + num34;
			num27 = FAST_INTEGER_MULTIPLY(num36 - num34, 362);
			num25 = FAST_INTEGER_MULTIPLY(num35 + num37, 473);
			int num39 = num25 - FAST_INTEGER_MULTIPLY(num37, 277);
			num29 = num25 - FAST_INTEGER_MULTIPLY(num35, 669);
			int num40 = num29 - num38;
			int num41 = num27 - num40;
			int num42 = num39 - num41;
			array[output_col] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num30 + num38, 5)) & 0x3FF];
			array[output_col + 7] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num30 - num38, 5)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num32 + num40, 5)) & 0x3FF];
			array[output_col + 6] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num32 - num40, 5)) & 0x3FF];
			array[output_col + 2] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num33 + num41, 5)) & 0x3FF];
			array[output_col + 5] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num33 - num41, 5)) & 0x3FF];
			array[output_col + 3] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num31 + num42, 5)) & 0x3FF];
			array[output_col + 4] = sample_range_limit[(num24 + FAST_INTEGER_IRIGHT_SHIFT(num31 - num42, 5)) & 0x3FF];
			num2 += 8;
		}
	}

	private static int FAST_INTEGER_MULTIPLY(int var, int c)
	{
		return var * c >> 8;
	}

	private static int FAST_INTEGER_DEQUANTIZE(short coef, int quantval)
	{
		return coef * quantval;
	}

	private static int FAST_INTEGER_IRIGHT_SHIFT(int x, int shft)
	{
		return x >> shft;
	}

	private void jpeg_idct_float(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int num2 = 0;
		float[] float_array = m_dctTables[component_index].float_array;
		int num3 = 0;
		for (int num4 = 8; num4 > 0; num4--)
		{
			if (coef_block[num + 8] == 0 && coef_block[num + 16] == 0 && coef_block[num + 24] == 0 && coef_block[num + 32] == 0 && coef_block[num + 40] == 0 && coef_block[num + 48] == 0 && coef_block[num + 56] == 0)
			{
				float num5 = FLOAT_DEQUANTIZE(coef_block[num], float_array[num3]);
				m_fworkspace[num2] = num5;
				m_fworkspace[num2 + 8] = num5;
				m_fworkspace[num2 + 16] = num5;
				m_fworkspace[num2 + 24] = num5;
				m_fworkspace[num2 + 32] = num5;
				m_fworkspace[num2 + 40] = num5;
				m_fworkspace[num2 + 48] = num5;
				m_fworkspace[num2 + 56] = num5;
				num++;
				num3++;
				num2++;
			}
			else
			{
				float num6 = FLOAT_DEQUANTIZE(coef_block[num], float_array[num3]);
				float num7 = FLOAT_DEQUANTIZE(coef_block[num + 16], float_array[num3 + 16]);
				float num8 = FLOAT_DEQUANTIZE(coef_block[num + 32], float_array[num3 + 32]);
				float num9 = FLOAT_DEQUANTIZE(coef_block[num + 48], float_array[num3 + 48]);
				float num10 = num6 + num8;
				float num11 = num6 - num8;
				float num12 = num7 + num9;
				float num13 = (num7 - num9) * 1.4142135f - num12;
				num6 = num10 + num12;
				num9 = num10 - num12;
				num7 = num11 + num13;
				num8 = num11 - num13;
				float num14 = FLOAT_DEQUANTIZE(coef_block[num + 8], float_array[num3 + 8]);
				float num15 = FLOAT_DEQUANTIZE(coef_block[num + 24], float_array[num3 + 24]);
				float num16 = FLOAT_DEQUANTIZE(coef_block[num + 40], float_array[num3 + 40]);
				float num17 = FLOAT_DEQUANTIZE(coef_block[num + 56], float_array[num3 + 56]);
				float num18 = num16 + num15;
				float num19 = num16 - num15;
				float num20 = num14 + num17;
				float num21 = num14 - num17;
				num17 = num20 + num18;
				float num22 = (num20 - num18) * 1.4142135f;
				float num23 = (num19 + num21) * 1.847759f;
				num10 = num23 - num21 * 1.0823922f;
				num13 = num23 - num19 * 2.613126f;
				num16 = num13 - num17;
				num15 = num22 - num16;
				num14 = num10 - num15;
				m_fworkspace[num2] = num6 + num17;
				m_fworkspace[num2 + 56] = num6 - num17;
				m_fworkspace[num2 + 8] = num7 + num16;
				m_fworkspace[num2 + 48] = num7 - num16;
				m_fworkspace[num2 + 16] = num8 + num15;
				m_fworkspace[num2 + 40] = num8 - num15;
				m_fworkspace[num2 + 24] = num9 + num14;
				m_fworkspace[num2 + 32] = num9 - num14;
				num++;
				num3++;
				num2++;
			}
		}
		num2 = 0;
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num24 = m_cinfo.m_sampleRangeLimitOffset - 384;
		for (int i = 0; i < 8; i++)
		{
			float num25 = m_fworkspace[num2] + 512.5f;
			float num26 = num25 + m_fworkspace[num2 + 4];
			float num27 = num25 - m_fworkspace[num2 + 4];
			float num28 = m_fworkspace[num2 + 2] + m_fworkspace[num2 + 6];
			float num29 = (m_fworkspace[num2 + 2] - m_fworkspace[num2 + 6]) * 1.4142135f - num28;
			float num30 = num26 + num28;
			float num31 = num26 - num28;
			float num32 = num27 + num29;
			float num33 = num27 - num29;
			float num34 = m_fworkspace[num2 + 5] + m_fworkspace[num2 + 3];
			float num35 = m_fworkspace[num2 + 5] - m_fworkspace[num2 + 3];
			float num36 = m_fworkspace[num2 + 1] + m_fworkspace[num2 + 7];
			float num37 = m_fworkspace[num2 + 1] - m_fworkspace[num2 + 7];
			float num38 = num36 + num34;
			float num39 = (num36 - num34) * 1.4142135f;
			float num40 = (num35 + num37) * 1.847759f;
			num26 = num40 - num37 * 1.0823922f;
			num29 = num40 - num35 * 2.613126f;
			float num41 = num29 - num38;
			float num42 = num39 - num41;
			float num43 = num26 - num42;
			int i2 = output_row + i;
			byte[] array = m_componentBuffer[i2];
			array[output_col] = sample_range_limit[(num24 + (int)(num30 + num38)) & 0x3FF];
			array[output_col + 7] = sample_range_limit[(num24 + (int)(num30 - num38)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num24 + (int)(num32 + num41)) & 0x3FF];
			array[output_col + 6] = sample_range_limit[(num24 + (int)(num32 - num41)) & 0x3FF];
			array[output_col + 2] = sample_range_limit[(num24 + (int)(num33 + num42)) & 0x3FF];
			array[output_col + 5] = sample_range_limit[(num24 + (int)(num33 - num42)) & 0x3FF];
			array[output_col + 3] = sample_range_limit[(num24 + (int)(num31 + num43)) & 0x3FF];
			array[output_col + 4] = sample_range_limit[(num24 + (int)(num31 - num43)) & 0x3FF];
			num2 += 8;
		}
	}

	private static float FLOAT_DEQUANTIZE(short coef, float quantval)
	{
		return (float)coef * quantval;
	}

	private void jpeg_idct_4x4(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int num2 = 0;
		int[] int_array = m_dctTables[component_index].int_array;
		int num3 = 0;
		for (int num4 = 8; num4 > 0; num4--)
		{
			if (num4 != 4)
			{
				if (coef_block[num + 8] == 0 && coef_block[num + 16] == 0 && coef_block[num + 24] == 0 && coef_block[num + 40] == 0 && coef_block[num + 48] == 0 && coef_block[num + 56] == 0)
				{
					int num5 = REDUCED_DEQUANTIZE(coef_block[num], int_array[num3]) << 2;
					m_workspace[num2] = num5;
					m_workspace[num2 + 8] = num5;
					m_workspace[num2 + 16] = num5;
					m_workspace[num2 + 24] = num5;
				}
				else
				{
					int num6 = REDUCED_DEQUANTIZE(coef_block[num], int_array[num3]);
					num6 <<= 14;
					int num7 = REDUCED_DEQUANTIZE(coef_block[num + 16], int_array[num3 + 16]);
					int num8 = REDUCED_DEQUANTIZE(coef_block[num + 48], int_array[num3 + 48]);
					int num9 = num7 * 15137 + num8 * -6270;
					int num10 = num6 + num9;
					int num11 = num6 - num9;
					int num12 = REDUCED_DEQUANTIZE(coef_block[num + 56], int_array[num3 + 56]);
					num7 = REDUCED_DEQUANTIZE(coef_block[num + 40], int_array[num3 + 40]);
					num8 = REDUCED_DEQUANTIZE(coef_block[num + 24], int_array[num3 + 24]);
					int num13 = REDUCED_DEQUANTIZE(coef_block[num + 8], int_array[num3 + 8]);
					num6 = num12 * -1730 + num7 * 11893 + num8 * -17799 + num13 * 8697;
					num9 = num12 * -4176 + num7 * -4926 + num8 * 7373 + num13 * 20995;
					m_workspace[num2] = JpegUtils.DESCALE(num10 + num9, 12);
					m_workspace[num2 + 24] = JpegUtils.DESCALE(num10 - num9, 12);
					m_workspace[num2 + 8] = JpegUtils.DESCALE(num11 + num6, 12);
					m_workspace[num2 + 16] = JpegUtils.DESCALE(num11 - num6, 12);
				}
			}
			num++;
			num3++;
			num2++;
		}
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num14 = m_cinfo.m_sampleRangeLimitOffset - 384;
		num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			int i2 = output_row + i;
			byte[] array = m_componentBuffer[i2];
			if (m_workspace[num2 + 1] == 0 && m_workspace[num2 + 2] == 0 && m_workspace[num2 + 3] == 0 && m_workspace[num2 + 5] == 0 && m_workspace[num2 + 6] == 0 && m_workspace[num2 + 7] == 0)
			{
				array[output_col + 3] = (array[output_col + 2] = (array[output_col + 1] = (array[output_col] = sample_range_limit[(num14 + JpegUtils.DESCALE(m_workspace[num2], 5)) & 0x3FF])));
				num2 += 8;
				continue;
			}
			int num15 = m_workspace[num2] << 14;
			int num16 = m_workspace[num2 + 2] * 15137 + m_workspace[num2 + 6] * -6270;
			int num17 = num15 + num16;
			int num18 = num15 - num16;
			int num19 = m_workspace[num2 + 7];
			int num20 = m_workspace[num2 + 5];
			int num21 = m_workspace[num2 + 3];
			int num22 = m_workspace[num2 + 1];
			num15 = num19 * -1730 + num20 * 11893 + num21 * -17799 + num22 * 8697;
			num16 = num19 * -4176 + num20 * -4926 + num21 * 7373 + num22 * 20995;
			array[output_col] = sample_range_limit[(num14 + JpegUtils.DESCALE(num17 + num16, 19)) & 0x3FF];
			array[output_col + 3] = sample_range_limit[(num14 + JpegUtils.DESCALE(num17 - num16, 19)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num14 + JpegUtils.DESCALE(num18 + num15, 19)) & 0x3FF];
			array[output_col + 2] = sample_range_limit[(num14 + JpegUtils.DESCALE(num18 - num15, 19)) & 0x3FF];
			num2 += 8;
		}
	}

	private void jpeg_idct_2x2(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int num2 = 0;
		int[] int_array = m_dctTables[component_index].int_array;
		int num3 = 0;
		for (int num4 = 8; num4 > 0; num4--)
		{
			if (num4 != 6 && num4 != 4 && num4 != 2)
			{
				if (coef_block[num + 8] == 0 && coef_block[num + 24] == 0 && coef_block[num + 40] == 0 && coef_block[num + 56] == 0)
				{
					int num5 = REDUCED_DEQUANTIZE(coef_block[num], int_array[num3]) << 2;
					m_workspace[num2] = num5;
					m_workspace[num2 + 8] = num5;
				}
				else
				{
					int num6 = REDUCED_DEQUANTIZE(coef_block[num], int_array[num3]);
					int num7 = num6 << 15;
					num6 = REDUCED_DEQUANTIZE(coef_block[num + 56], int_array[num3 + 56]);
					int num8 = num6 * -5906;
					num6 = REDUCED_DEQUANTIZE(coef_block[num + 40], int_array[num3 + 40]);
					num8 += num6 * 6967;
					num6 = REDUCED_DEQUANTIZE(coef_block[num + 24], int_array[num3 + 24]);
					num8 += num6 * -10426;
					num6 = REDUCED_DEQUANTIZE(coef_block[num + 8], int_array[num3 + 8]);
					num8 += num6 * 29692;
					m_workspace[num2] = JpegUtils.DESCALE(num7 + num8, 13);
					m_workspace[num2 + 8] = JpegUtils.DESCALE(num7 - num8, 13);
				}
			}
			num++;
			num3++;
			num2++;
		}
		num2 = 0;
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num9 = m_cinfo.m_sampleRangeLimitOffset - 384;
		for (int i = 0; i < 2; i++)
		{
			int i2 = output_row + i;
			byte[] array = m_componentBuffer[i2];
			if (m_workspace[num2 + 1] == 0 && m_workspace[num2 + 3] == 0 && m_workspace[num2 + 5] == 0 && m_workspace[num2 + 7] == 0)
			{
				array[output_col + 1] = (array[output_col] = sample_range_limit[(num9 + JpegUtils.DESCALE(m_workspace[num2], 5)) & 0x3FF]);
				num2 += 8;
				continue;
			}
			int num10 = m_workspace[num2] << 15;
			int num11 = m_workspace[num2 + 7] * -5906 + m_workspace[num2 + 5] * 6967 + m_workspace[num2 + 3] * -10426 + m_workspace[num2 + 1] * 29692;
			array[output_col] = sample_range_limit[(num9 + JpegUtils.DESCALE(num10 + num11, 20)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num9 + JpegUtils.DESCALE(num10 - num11, 20)) & 0x3FF];
			num2 += 8;
		}
	}

	private void jpeg_idct_1x1(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int[] int_array = m_dctTables[component_index].int_array;
		int x = REDUCED_DEQUANTIZE(coef_block[0], int_array[0]);
		x = JpegUtils.DESCALE(x, 3);
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num = m_cinfo.m_sampleRangeLimitOffset - 384;
		m_componentBuffer[output_row][output_col] = sample_range_limit[(num + x) & 0x3FF];
	}

	private static int REDUCED_DEQUANTIZE(short coef, int quantval)
	{
		return coef * quantval;
	}

	private void jpeg_idct_3x3(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_5x5(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_6x6(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_7x7(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_9x9(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_10x10(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_11x11(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_12x12(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_13x13(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_14x14(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_15x15(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_16x16(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int[] int_array = m_dctTables[component_index].int_array;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < 8; i++)
		{
			int num4 = coef_block[num] * int_array[num2];
			num4 <<= 13;
			num4 += 1024;
			int num5 = coef_block[num + 32] * int_array[num2 + 32];
			int num6 = num5 * SLOW_INTEGER_FIX(1.306562965);
			int num7 = num5 * 4433;
			int num8 = num4 + num6;
			int num9 = num4 - num6;
			int num10 = num4 + num7;
			int num11 = num4 - num7;
			num5 = coef_block[num + 16] * int_array[num2 + 16];
			int num12 = coef_block[num + 48] * int_array[num2 + 48];
			int num13 = num5 - num12;
			int num14 = num13 * SLOW_INTEGER_FIX(0.275899379);
			num13 *= SLOW_INTEGER_FIX(1.387039845);
			num4 = num13 + num12 * 20995;
			num6 = num14 + num5 * 7373;
			num7 = num13 - num5 * SLOW_INTEGER_FIX(0.601344887);
			int num15 = num14 - num12 * SLOW_INTEGER_FIX(0.509795579);
			int num16 = num8 + num4;
			int num17 = num8 - num4;
			int num18 = num10 + num6;
			int num19 = num10 - num6;
			int num20 = num11 + num7;
			int num21 = num11 - num7;
			int num22 = num9 + num15;
			int num23 = num9 - num15;
			num5 = coef_block[num + 8] * int_array[num2 + 8];
			num12 = coef_block[num + 24] * int_array[num2 + 24];
			num13 = coef_block[num + 40] * int_array[num2 + 40];
			num14 = coef_block[num + 56] * int_array[num2 + 56];
			num9 = num5 + num13;
			num6 = (num5 + num12) * SLOW_INTEGER_FIX(1.353318001);
			num7 = num9 * SLOW_INTEGER_FIX(1.247225013);
			num15 = (num5 + num14) * SLOW_INTEGER_FIX(1.093201867);
			num8 = (num5 - num14) * SLOW_INTEGER_FIX(0.897167586);
			num9 *= SLOW_INTEGER_FIX(0.666655658);
			num10 = (num5 - num12) * SLOW_INTEGER_FIX(0.410524528);
			num4 = num6 + num7 + num15 - num5 * SLOW_INTEGER_FIX(2.286341144);
			num11 = num8 + num9 + num10 - num5 * SLOW_INTEGER_FIX(1.835730603);
			num5 = (num12 + num13) * SLOW_INTEGER_FIX(0.138617169);
			num6 += num5 + num12 * SLOW_INTEGER_FIX(0.071888074);
			num7 += num5 - num13 * SLOW_INTEGER_FIX(1.125726048);
			num5 = (num13 - num12) * SLOW_INTEGER_FIX(1.407403738);
			num9 += num5 - num13 * SLOW_INTEGER_FIX(0.766367282);
			num10 += num5 + num12 * SLOW_INTEGER_FIX(1.971951411);
			num12 += num14;
			num5 = num12 * -SLOW_INTEGER_FIX(0.666655658);
			num6 += num5;
			num15 += num5 + num14 * SLOW_INTEGER_FIX(1.065388962);
			num12 *= -SLOW_INTEGER_FIX(1.247225013);
			num8 += num12 + num14 * SLOW_INTEGER_FIX(3.141271809);
			num10 += num12;
			num12 = (num13 + num14) * -SLOW_INTEGER_FIX(1.353318001);
			num7 += num12;
			num15 += num12;
			num12 = (num14 - num13) * SLOW_INTEGER_FIX(0.410524528);
			num8 += num12;
			num9 += num12;
			m_workspace[num3] = num16 + num4 >> 11;
			m_workspace[num3 + 120] = num16 - num4 >> 11;
			m_workspace[num3 + 8] = num18 + num6 >> 11;
			m_workspace[num3 + 112] = num18 - num6 >> 11;
			m_workspace[num3 + 16] = num20 + num7 >> 11;
			m_workspace[num3 + 104] = num20 - num7 >> 11;
			m_workspace[num3 + 24] = num22 + num15 >> 11;
			m_workspace[num3 + 96] = num22 - num15 >> 11;
			m_workspace[num3 + 32] = num23 + num8 >> 11;
			m_workspace[num3 + 88] = num23 - num8 >> 11;
			m_workspace[num3 + 40] = num21 + num9 >> 11;
			m_workspace[num3 + 80] = num21 - num9 >> 11;
			m_workspace[num3 + 48] = num19 + num10 >> 11;
			m_workspace[num3 + 72] = num19 - num10 >> 11;
			m_workspace[num3 + 56] = num17 + num11 >> 11;
			m_workspace[num3 + 64] = num17 - num11 >> 11;
			num++;
			num2++;
			num3++;
		}
		num3 = 0;
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num24 = m_cinfo.m_sampleRangeLimitOffset - 384;
		for (int j = 0; j < 16; j++)
		{
			int num25 = m_workspace[num3] + 16384 + 16;
			num25 <<= 13;
			int num26 = m_workspace[num3 + 4];
			int num27 = num26 * SLOW_INTEGER_FIX(1.306562965);
			int num28 = num26 * 4433;
			int num29 = num25 + num27;
			int num30 = num25 - num27;
			int num31 = num25 + num28;
			int num32 = num25 - num28;
			num26 = m_workspace[num3 + 2];
			int num33 = m_workspace[num3 + 6];
			int num34 = num26 - num33;
			int num35 = num34 * SLOW_INTEGER_FIX(0.275899379);
			num34 *= SLOW_INTEGER_FIX(1.387039845);
			num25 = num34 + num33 * 20995;
			num27 = num35 + num26 * 7373;
			num28 = num34 - num26 * SLOW_INTEGER_FIX(0.601344887);
			int num36 = num35 - num33 * SLOW_INTEGER_FIX(0.509795579);
			int num37 = num29 + num25;
			int num38 = num29 - num25;
			int num39 = num31 + num27;
			int num40 = num31 - num27;
			int num41 = num32 + num28;
			int num42 = num32 - num28;
			int num43 = num30 + num36;
			int num44 = num30 - num36;
			num26 = m_workspace[num3 + 1];
			num33 = m_workspace[num3 + 3];
			num34 = m_workspace[num3 + 5];
			num35 = m_workspace[num3 + 7];
			num30 = num26 + num34;
			num27 = (num26 + num33) * SLOW_INTEGER_FIX(1.353318001);
			num28 = num30 * SLOW_INTEGER_FIX(1.247225013);
			num36 = (num26 + num35) * SLOW_INTEGER_FIX(1.093201867);
			num29 = (num26 - num35) * SLOW_INTEGER_FIX(0.897167586);
			num30 *= SLOW_INTEGER_FIX(0.666655658);
			num31 = (num26 - num33) * SLOW_INTEGER_FIX(0.410524528);
			num25 = num27 + num28 + num36 - num26 * SLOW_INTEGER_FIX(2.286341144);
			num32 = num29 + num30 + num31 - num26 * SLOW_INTEGER_FIX(1.835730603);
			num26 = (num33 + num34) * SLOW_INTEGER_FIX(0.138617169);
			num27 += num26 + num33 * SLOW_INTEGER_FIX(0.071888074);
			num28 += num26 - num34 * SLOW_INTEGER_FIX(1.125726048);
			num26 = (num34 - num33) * SLOW_INTEGER_FIX(1.407403738);
			num30 += num26 - num34 * SLOW_INTEGER_FIX(0.766367282);
			num31 += num26 + num33 * SLOW_INTEGER_FIX(1.971951411);
			num33 += num35;
			num26 = num33 * -SLOW_INTEGER_FIX(0.666655658);
			num27 += num26;
			num36 += num26 + num35 * SLOW_INTEGER_FIX(1.065388962);
			num33 *= -SLOW_INTEGER_FIX(1.247225013);
			num29 += num33 + num35 * SLOW_INTEGER_FIX(3.141271809);
			num31 += num33;
			num33 = (num34 + num35) * -SLOW_INTEGER_FIX(1.353318001);
			num28 += num33;
			num36 += num33;
			num33 = (num35 - num34) * SLOW_INTEGER_FIX(0.410524528);
			num29 += num33;
			num30 += num33;
			int i2 = output_row + j;
			byte[] array = m_componentBuffer[i2];
			array[output_col] = sample_range_limit[(num24 + (num37 + num25 >> 18)) & 0x3FF];
			array[output_col + 15] = sample_range_limit[(num24 + (num37 - num25 >> 18)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num24 + (num39 + num27 >> 18)) & 0x3FF];
			array[output_col + 14] = sample_range_limit[(num24 + (num39 - num27 >> 18)) & 0x3FF];
			array[output_col + 2] = sample_range_limit[(num24 + (num41 + num28 >> 18)) & 0x3FF];
			array[output_col + 13] = sample_range_limit[(num24 + (num41 - num28 >> 18)) & 0x3FF];
			array[output_col + 3] = sample_range_limit[(num24 + (num43 + num36 >> 18)) & 0x3FF];
			array[output_col + 12] = sample_range_limit[(num24 + (num43 - num36 >> 18)) & 0x3FF];
			array[output_col + 4] = sample_range_limit[(num24 + (num44 + num29 >> 18)) & 0x3FF];
			array[output_col + 11] = sample_range_limit[(num24 + (num44 - num29 >> 18)) & 0x3FF];
			array[output_col + 5] = sample_range_limit[(num24 + (num42 + num30 >> 18)) & 0x3FF];
			array[output_col + 10] = sample_range_limit[(num24 + (num42 - num30 >> 18)) & 0x3FF];
			array[output_col + 6] = sample_range_limit[(num24 + (num40 + num31 >> 18)) & 0x3FF];
			array[output_col + 9] = sample_range_limit[(num24 + (num40 - num31 >> 18)) & 0x3FF];
			array[output_col + 7] = sample_range_limit[(num24 + (num38 + num32 >> 18)) & 0x3FF];
			array[output_col + 8] = sample_range_limit[(num24 + (num38 - num32 >> 18)) & 0x3FF];
			num3 += 8;
		}
	}

	private void jpeg_idct_16x8(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int[] int_array = m_dctTables[component_index].int_array;
		int num2 = 0;
		int num3 = 0;
		for (int num4 = 8; num4 > 0; num4--)
		{
			if (coef_block[num + 8] == 0 && coef_block[num + 16] == 0 && coef_block[num + 24] == 0 && coef_block[num + 32] == 0 && coef_block[num + 40] == 0 && coef_block[num + 48] == 0 && coef_block[num + 56] == 0)
			{
				int num5 = coef_block[num] * int_array[num2] << 2;
				m_workspace[num3] = num5;
				m_workspace[num3 + 8] = num5;
				m_workspace[num3 + 16] = num5;
				m_workspace[num3 + 24] = num5;
				m_workspace[num3 + 32] = num5;
				m_workspace[num3 + 40] = num5;
				m_workspace[num3 + 48] = num5;
				m_workspace[num3 + 56] = num5;
				num++;
				num2++;
				num3++;
			}
			else
			{
				int num6 = coef_block[num + 16] * int_array[num2 + 16];
				int num7 = coef_block[num + 48] * int_array[num2 + 48];
				int num8 = (num6 + num7) * 4433;
				int num9 = num8 + num6 * 6270;
				int num10 = num8 - num7 * 15137;
				num6 = coef_block[num] * int_array[num2];
				num7 = coef_block[num + 32] * int_array[num2 + 32];
				num6 <<= 13;
				num7 <<= 13;
				num6 += 1024;
				int num11 = num6 + num7;
				int num12 = num6 - num7;
				int num13 = num11 + num9;
				int num14 = num11 - num9;
				int num15 = num12 + num10;
				int num16 = num12 - num10;
				num11 = coef_block[num + 56] * int_array[num2 + 56];
				num12 = coef_block[num + 40] * int_array[num2 + 40];
				num9 = coef_block[num + 24] * int_array[num2 + 24];
				num10 = coef_block[num + 8] * int_array[num2 + 8];
				num6 = num11 + num9;
				num7 = num12 + num10;
				num8 = (num6 + num7) * 9633;
				num6 *= -16069;
				num7 *= -3196;
				num6 += num8;
				num7 += num8;
				num8 = (num11 + num10) * -7373;
				num11 *= 2446;
				num10 *= 12299;
				num11 += num8 + num6;
				num10 += num8 + num7;
				num8 = (num12 + num9) * -20995;
				num12 *= 16819;
				num9 *= 25172;
				num12 += num8 + num7;
				num9 += num8 + num6;
				m_workspace[num3] = num13 + num10 >> 11;
				m_workspace[num3 + 56] = num13 - num10 >> 11;
				m_workspace[num3 + 8] = num15 + num9 >> 11;
				m_workspace[num3 + 48] = num15 - num9 >> 11;
				m_workspace[num3 + 16] = num16 + num12 >> 11;
				m_workspace[num3 + 40] = num16 - num12 >> 11;
				m_workspace[num3 + 24] = num14 + num11 >> 11;
				m_workspace[num3 + 32] = num14 - num11 >> 11;
				num++;
				num2++;
				num3++;
			}
		}
		num3 = 0;
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num17 = m_cinfo.m_sampleRangeLimitOffset - 384;
		for (int i = 0; i < 8; i++)
		{
			int num18 = m_workspace[num3] + 16384 + 16;
			num18 <<= 13;
			int num19 = m_workspace[num3 + 4];
			int num20 = num19 * SLOW_INTEGER_FIX(1.306562965);
			int num21 = num19 * 4433;
			int num22 = num18 + num20;
			int num23 = num18 - num20;
			int num24 = num18 + num21;
			int num25 = num18 - num21;
			num19 = m_workspace[num3 + 2];
			int num26 = m_workspace[num3 + 6];
			int num27 = num19 - num26;
			int num28 = num27 * SLOW_INTEGER_FIX(0.275899379);
			num27 *= SLOW_INTEGER_FIX(1.387039845);
			num18 = num27 + num26 * 20995;
			num20 = num28 + num19 * 7373;
			num21 = num27 - num19 * SLOW_INTEGER_FIX(0.601344887);
			int num29 = num28 - num26 * SLOW_INTEGER_FIX(0.509795579);
			int num30 = num22 + num18;
			int num31 = num22 - num18;
			int num32 = num24 + num20;
			int num33 = num24 - num20;
			int num34 = num25 + num21;
			int num35 = num25 - num21;
			int num36 = num23 + num29;
			int num37 = num23 - num29;
			num19 = m_workspace[num3 + 1];
			num26 = m_workspace[num3 + 3];
			num27 = m_workspace[num3 + 5];
			num28 = m_workspace[num3 + 7];
			num23 = num19 + num27;
			num20 = (num19 + num26) * SLOW_INTEGER_FIX(1.353318001);
			num21 = num23 * SLOW_INTEGER_FIX(1.247225013);
			num29 = (num19 + num28) * SLOW_INTEGER_FIX(1.093201867);
			num22 = (num19 - num28) * SLOW_INTEGER_FIX(0.897167586);
			num23 *= SLOW_INTEGER_FIX(0.666655658);
			num24 = (num19 - num26) * SLOW_INTEGER_FIX(0.410524528);
			num18 = num20 + num21 + num29 - num19 * SLOW_INTEGER_FIX(2.286341144);
			num25 = num22 + num23 + num24 - num19 * SLOW_INTEGER_FIX(1.835730603);
			num19 = (num26 + num27) * SLOW_INTEGER_FIX(0.138617169);
			num20 += num19 + num26 * SLOW_INTEGER_FIX(0.071888074);
			num21 += num19 - num27 * SLOW_INTEGER_FIX(1.125726048);
			num19 = (num27 - num26) * SLOW_INTEGER_FIX(1.407403738);
			num23 += num19 - num27 * SLOW_INTEGER_FIX(0.766367282);
			num24 += num19 + num26 * SLOW_INTEGER_FIX(1.971951411);
			num26 += num28;
			num19 = num26 * -SLOW_INTEGER_FIX(0.666655658);
			num20 += num19;
			num29 += num19 + num28 * SLOW_INTEGER_FIX(1.065388962);
			num26 *= -SLOW_INTEGER_FIX(1.247225013);
			num22 += num26 + num28 * SLOW_INTEGER_FIX(3.141271809);
			num24 += num26;
			num26 = (num27 + num28) * -SLOW_INTEGER_FIX(1.353318001);
			num21 += num26;
			num29 += num26;
			num26 = (num28 - num27) * SLOW_INTEGER_FIX(0.410524528);
			num22 += num26;
			num23 += num26;
			int i2 = output_row + i;
			byte[] array = m_componentBuffer[i2];
			array[output_col] = sample_range_limit[(num17 + (num30 + num18 >> 18)) & 0x3FF];
			array[output_col + 15] = sample_range_limit[(num17 + (num30 - num18 >> 18)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num17 + (num32 + num20 >> 18)) & 0x3FF];
			array[output_col + 14] = sample_range_limit[(num17 + (num32 - num20 >> 18)) & 0x3FF];
			array[output_col + 2] = sample_range_limit[(num17 + (num34 + num21 >> 18)) & 0x3FF];
			array[output_col + 13] = sample_range_limit[(num17 + (num34 - num21 >> 18)) & 0x3FF];
			array[output_col + 3] = sample_range_limit[(num17 + (num36 + num29 >> 18)) & 0x3FF];
			array[output_col + 12] = sample_range_limit[(num17 + (num36 - num29 >> 18)) & 0x3FF];
			array[output_col + 4] = sample_range_limit[(num17 + (num37 + num22 >> 18)) & 0x3FF];
			array[output_col + 11] = sample_range_limit[(num17 + (num37 - num22 >> 18)) & 0x3FF];
			array[output_col + 5] = sample_range_limit[(num17 + (num35 + num23 >> 18)) & 0x3FF];
			array[output_col + 10] = sample_range_limit[(num17 + (num35 - num23 >> 18)) & 0x3FF];
			array[output_col + 6] = sample_range_limit[(num17 + (num33 + num24 >> 18)) & 0x3FF];
			array[output_col + 9] = sample_range_limit[(num17 + (num33 - num24 >> 18)) & 0x3FF];
			array[output_col + 7] = sample_range_limit[(num17 + (num31 + num25 >> 18)) & 0x3FF];
			array[output_col + 8] = sample_range_limit[(num17 + (num31 - num25 >> 18)) & 0x3FF];
			num3 += 8;
		}
	}

	private void jpeg_idct_14x7(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_12x6(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_10x5(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_8x4(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_6x3(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_4x2(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_2x1(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_8x16(int component_index, short[] coef_block, int output_row, int output_col)
	{
		int num = 0;
		int[] int_array = m_dctTables[component_index].int_array;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < 8; i++)
		{
			int num4 = coef_block[num] * int_array[num2];
			num4 <<= 13;
			num4 += 1024;
			int num5 = coef_block[num + 32] * int_array[num2 + 32];
			int num6 = num5 * SLOW_INTEGER_FIX(1.306562965);
			int num7 = num5 * 4433;
			int num8 = num4 + num6;
			int num9 = num4 - num6;
			int num10 = num4 + num7;
			int num11 = num4 - num7;
			num5 = coef_block[num + 16] * int_array[num2 + 16];
			int num12 = coef_block[num + 48] * int_array[num2 + 48];
			int num13 = num5 - num12;
			int num14 = num13 * SLOW_INTEGER_FIX(0.275899379);
			num13 *= SLOW_INTEGER_FIX(1.387039845);
			num4 = num13 + num12 * 20995;
			num6 = num14 + num5 * 7373;
			num7 = num13 - num5 * SLOW_INTEGER_FIX(0.601344887);
			int num15 = num14 - num12 * SLOW_INTEGER_FIX(0.509795579);
			int num16 = num8 + num4;
			int num17 = num8 - num4;
			int num18 = num10 + num6;
			int num19 = num10 - num6;
			int num20 = num11 + num7;
			int num21 = num11 - num7;
			int num22 = num9 + num15;
			int num23 = num9 - num15;
			num5 = coef_block[num + 8] * int_array[num2 + 8];
			num12 = coef_block[num + 24] * int_array[num2 + 24];
			num13 = coef_block[num + 40] * int_array[num2 + 40];
			num14 = coef_block[num + 56] * int_array[num2 + 56];
			num9 = num5 + num13;
			num6 = (num5 + num12) * SLOW_INTEGER_FIX(1.353318001);
			num7 = num9 * SLOW_INTEGER_FIX(1.247225013);
			num15 = (num5 + num14) * SLOW_INTEGER_FIX(1.093201867);
			num8 = (num5 - num14) * SLOW_INTEGER_FIX(0.897167586);
			num9 *= SLOW_INTEGER_FIX(0.666655658);
			num10 = (num5 - num12) * SLOW_INTEGER_FIX(0.410524528);
			num4 = num6 + num7 + num15 - num5 * SLOW_INTEGER_FIX(2.286341144);
			num11 = num8 + num9 + num10 - num5 * SLOW_INTEGER_FIX(1.835730603);
			num5 = (num12 + num13) * SLOW_INTEGER_FIX(0.138617169);
			num6 += num5 + num12 * SLOW_INTEGER_FIX(0.071888074);
			num7 += num5 - num13 * SLOW_INTEGER_FIX(1.125726048);
			num5 = (num13 - num12) * SLOW_INTEGER_FIX(1.407403738);
			num9 += num5 - num13 * SLOW_INTEGER_FIX(0.766367282);
			num10 += num5 + num12 * SLOW_INTEGER_FIX(1.971951411);
			num12 += num14;
			num5 = num12 * -SLOW_INTEGER_FIX(0.666655658);
			num6 += num5;
			num15 += num5 + num14 * SLOW_INTEGER_FIX(1.065388962);
			num12 *= -SLOW_INTEGER_FIX(1.247225013);
			num8 += num12 + num14 * SLOW_INTEGER_FIX(3.141271809);
			num10 += num12;
			num12 = (num13 + num14) * -SLOW_INTEGER_FIX(1.353318001);
			num7 += num12;
			num15 += num12;
			num12 = (num14 - num13) * SLOW_INTEGER_FIX(0.410524528);
			num8 += num12;
			num9 += num12;
			m_workspace[num3] = num16 + num4 >> 11;
			m_workspace[num3 + 120] = num16 - num4 >> 11;
			m_workspace[num3 + 8] = num18 + num6 >> 11;
			m_workspace[num3 + 112] = num18 - num6 >> 11;
			m_workspace[num3 + 16] = num20 + num7 >> 11;
			m_workspace[num3 + 104] = num20 - num7 >> 11;
			m_workspace[num3 + 24] = num22 + num15 >> 11;
			m_workspace[num3 + 96] = num22 - num15 >> 11;
			m_workspace[num3 + 32] = num23 + num8 >> 11;
			m_workspace[num3 + 88] = num23 - num8 >> 11;
			m_workspace[num3 + 40] = num21 + num9 >> 11;
			m_workspace[num3 + 80] = num21 - num9 >> 11;
			m_workspace[num3 + 48] = num19 + num10 >> 11;
			m_workspace[num3 + 72] = num19 - num10 >> 11;
			m_workspace[num3 + 56] = num17 + num11 >> 11;
			m_workspace[num3 + 64] = num17 - num11 >> 11;
			num++;
			num2++;
			num3++;
		}
		num3 = 0;
		byte[] sample_range_limit = m_cinfo.m_sample_range_limit;
		int num24 = m_cinfo.m_sampleRangeLimitOffset - 384;
		for (int j = 0; j < 16; j++)
		{
			int num25 = m_workspace[num3] + 16384 + 16;
			int num26 = m_workspace[num3 + 4];
			int num27 = num25 + num26 << 13;
			int num28 = num25 - num26 << 13;
			num25 = m_workspace[num3 + 2];
			num26 = m_workspace[num3 + 6];
			int num29 = (num25 + num26) * 4433;
			int num30 = num29 + num25 * 6270;
			int num31 = num29 - num26 * 15137;
			int num32 = num27 + num30;
			int num33 = num27 - num30;
			int num34 = num28 + num31;
			int num35 = num28 - num31;
			num27 = m_workspace[num3 + 7];
			num28 = m_workspace[num3 + 5];
			num30 = m_workspace[num3 + 3];
			num31 = m_workspace[num3 + 1];
			num25 = num27 + num30;
			num26 = num28 + num31;
			num29 = (num25 + num26) * 9633;
			num25 *= -16069;
			num26 *= -3196;
			num25 += num29;
			num26 += num29;
			num29 = (num27 + num31) * -7373;
			num27 *= 2446;
			num31 *= 12299;
			num27 += num29 + num25;
			num31 += num29 + num26;
			num29 = (num28 + num30) * -20995;
			num28 *= 16819;
			num30 *= 25172;
			num28 += num29 + num26;
			num30 += num29 + num25;
			int i2 = output_row + j;
			byte[] array = m_componentBuffer[i2];
			array[output_col] = sample_range_limit[(num24 + (num32 + num31 >> 18)) & 0x3FF];
			array[output_col + 7] = sample_range_limit[(num24 + (num32 - num31 >> 18)) & 0x3FF];
			array[output_col + 1] = sample_range_limit[(num24 + (num34 + num30 >> 18)) & 0x3FF];
			array[output_col + 6] = sample_range_limit[(num24 + (num34 - num30 >> 18)) & 0x3FF];
			array[output_col + 2] = sample_range_limit[(num24 + (num35 + num28 >> 18)) & 0x3FF];
			array[output_col + 5] = sample_range_limit[(num24 + (num35 - num28 >> 18)) & 0x3FF];
			array[output_col + 3] = sample_range_limit[(num24 + (num33 + num27 >> 18)) & 0x3FF];
			array[output_col + 4] = sample_range_limit[(num24 + (num33 - num27 >> 18)) & 0x3FF];
			num3 += 8;
		}
	}

	private void jpeg_idct_7x14(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_6x12(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_5x10(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_4x8(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_3x6(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_2x4(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}

	private void jpeg_idct_1x2(int component_index, short[] coef_block, int output_row, int output_col)
	{
		m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
	}
}
