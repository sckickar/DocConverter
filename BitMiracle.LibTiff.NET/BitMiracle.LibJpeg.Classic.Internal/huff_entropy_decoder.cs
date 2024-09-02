using System;

namespace BitMiracle.LibJpeg.Classic.Internal;

internal class huff_entropy_decoder : jpeg_entropy_decoder
{
	private class savable_state
	{
		public uint EOBRUN;

		public int[] last_dc_val = new int[4];

		public void Assign(savable_state ss)
		{
			EOBRUN = ss.EOBRUN;
			for (int i = 0; i < last_dc_val.Length; i++)
			{
				last_dc_val[i] = ss.last_dc_val[i];
			}
		}
	}

	private const int BIT_BUF_SIZE = 32;

	private const int MIN_GET_BITS = 25;

	private static readonly int[] bmask = new int[16]
	{
		0, 1, 3, 7, 15, 31, 63, 127, 255, 511,
		1023, 2047, 4095, 8191, 16383, 32767
	};

	private static readonly int[][] jpeg_zigzag_order = new int[8][]
	{
		new int[8] { 0, 1, 5, 6, 14, 15, 27, 28 },
		new int[8] { 2, 4, 7, 13, 16, 26, 29, 42 },
		new int[8] { 3, 8, 12, 17, 25, 30, 41, 43 },
		new int[8] { 9, 11, 18, 24, 31, 40, 44, 53 },
		new int[8] { 10, 19, 23, 32, 39, 45, 52, 54 },
		new int[8] { 20, 22, 33, 38, 46, 51, 55, 60 },
		new int[8] { 21, 34, 37, 47, 50, 56, 59, 61 },
		new int[8] { 35, 36, 48, 49, 57, 58, 62, 63 }
	};

	private static readonly int[][] jpeg_zigzag_order7 = new int[7][]
	{
		new int[7] { 0, 1, 5, 6, 14, 15, 27 },
		new int[7] { 2, 4, 7, 13, 16, 26, 28 },
		new int[7] { 3, 8, 12, 17, 25, 29, 38 },
		new int[7] { 9, 11, 18, 24, 30, 37, 39 },
		new int[7] { 10, 19, 23, 31, 36, 40, 45 },
		new int[7] { 20, 22, 32, 35, 41, 44, 46 },
		new int[7] { 21, 33, 34, 42, 43, 47, 48 }
	};

	private static readonly int[][] jpeg_zigzag_order6 = new int[6][]
	{
		new int[6] { 0, 1, 5, 6, 14, 15 },
		new int[6] { 2, 4, 7, 13, 16, 25 },
		new int[6] { 3, 8, 12, 17, 24, 26 },
		new int[6] { 9, 11, 18, 23, 27, 32 },
		new int[6] { 10, 19, 22, 28, 31, 33 },
		new int[6] { 20, 21, 29, 30, 34, 35 }
	};

	private static readonly int[][] jpeg_zigzag_order5 = new int[5][]
	{
		new int[5] { 0, 1, 5, 6, 14 },
		new int[5] { 2, 4, 7, 13, 15 },
		new int[5] { 3, 8, 12, 16, 21 },
		new int[5] { 9, 11, 17, 20, 22 },
		new int[5] { 10, 18, 19, 23, 24 }
	};

	private static readonly int[][] jpeg_zigzag_order4 = new int[4][]
	{
		new int[4] { 0, 1, 5, 6 },
		new int[4] { 2, 4, 7, 12 },
		new int[4] { 3, 8, 11, 13 },
		new int[4] { 9, 10, 14, 15 }
	};

	private static readonly int[][] jpeg_zigzag_order3 = new int[3][]
	{
		new int[3] { 0, 1, 5 },
		new int[3] { 2, 4, 6 },
		new int[3] { 3, 7, 8 }
	};

	private static readonly int[][] jpeg_zigzag_order2 = new int[2][]
	{
		new int[2] { 0, 1 },
		new int[2] { 2, 3 }
	};

	private bitread_perm_state m_bitstate;

	private readonly savable_state m_saved = new savable_state();

	private bool m_insufficient_data;

	private int m_restarts_to_go;

	private readonly d_derived_tbl[] derived_tbls = new d_derived_tbl[4];

	private d_derived_tbl ac_derived_tbl;

	private readonly d_derived_tbl[] m_dc_derived_tbls = new d_derived_tbl[4];

	private readonly d_derived_tbl[] m_ac_derived_tbls = new d_derived_tbl[4];

	private readonly d_derived_tbl[] m_dc_cur_tbls = new d_derived_tbl[10];

	private readonly d_derived_tbl[] m_ac_cur_tbls = new d_derived_tbl[10];

	private readonly int[] coef_limit = new int[10];

	private readonly jpeg_decompress_struct m_cinfo;

	private int get_buffer;

	private int bits_left;

	public huff_entropy_decoder(jpeg_decompress_struct cinfo)
	{
		m_cinfo = cinfo;
		finish_pass = finish_pass_huff;
		if (m_cinfo.m_progressive_mode)
		{
			cinfo.m_coef_bits = new int[cinfo.m_num_components][];
			for (int i = 0; i < cinfo.m_num_components; i++)
			{
				cinfo.m_coef_bits[i] = new int[64];
			}
			for (int j = 0; j < cinfo.m_num_components; j++)
			{
				for (int k = 0; k < 64; k++)
				{
					cinfo.m_coef_bits[j][k] = -1;
				}
			}
			for (int l = 0; l < 4; l++)
			{
				derived_tbls[l] = null;
			}
		}
		else
		{
			for (int m = 0; m < 4; m++)
			{
				m_dc_derived_tbls[m] = null;
				m_ac_derived_tbls[m] = null;
			}
		}
	}

	public override void start_pass()
	{
		if (m_cinfo.m_progressive_mode)
		{
			bool flag = false;
			if (m_cinfo.m_Ss == 0)
			{
				if (m_cinfo.m_Se != 0)
				{
					flag = true;
				}
			}
			else
			{
				if (m_cinfo.m_Se < m_cinfo.m_Ss || m_cinfo.m_Se > m_cinfo.lim_Se)
				{
					flag = true;
				}
				if (m_cinfo.m_comps_in_scan != 1)
				{
					flag = true;
				}
			}
			if (m_cinfo.m_Ah != 0 && m_cinfo.m_Ah - 1 != m_cinfo.m_Al)
			{
				flag = true;
			}
			if (m_cinfo.m_Al > 13)
			{
				flag = true;
			}
			if (flag)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_PROGRESSION, m_cinfo.m_Ss, m_cinfo.m_Se, m_cinfo.m_Ah, m_cinfo.m_Al);
			}
			for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
			{
				int component_index = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[i]].Component_index;
				if (m_cinfo.m_Ss != 0 && m_cinfo.m_coef_bits[component_index][0] < 0)
				{
					m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_BOGUS_PROGRESSION, component_index, 0);
				}
				for (int j = m_cinfo.m_Ss; j <= m_cinfo.m_Se; j++)
				{
					int num = m_cinfo.m_coef_bits[component_index][j];
					if (num < 0)
					{
						num = 0;
					}
					if (m_cinfo.m_Ah != num)
					{
						m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_BOGUS_PROGRESSION, component_index, j);
					}
					m_cinfo.m_coef_bits[component_index][j] = m_cinfo.m_Al;
				}
			}
			if (m_cinfo.m_Ah == 0)
			{
				if (m_cinfo.m_Ss == 0)
				{
					decode_mcu = decode_mcu_DC_first;
				}
				else
				{
					decode_mcu = decode_mcu_AC_first;
				}
			}
			else if (m_cinfo.m_Ss == 0)
			{
				decode_mcu = decode_mcu_DC_refine;
			}
			else
			{
				decode_mcu = decode_mcu_AC_refine;
			}
			for (int k = 0; k < m_cinfo.m_comps_in_scan; k++)
			{
				jpeg_component_info jpeg_component_info = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[k]];
				if (m_cinfo.m_Ss == 0)
				{
					if (m_cinfo.m_Ah == 0)
					{
						int dc_tbl_no = jpeg_component_info.Dc_tbl_no;
						jpeg_make_d_derived_tbl(isDC: true, dc_tbl_no, ref derived_tbls[dc_tbl_no]);
					}
				}
				else
				{
					int ac_tbl_no = jpeg_component_info.Ac_tbl_no;
					jpeg_make_d_derived_tbl(isDC: false, ac_tbl_no, ref derived_tbls[ac_tbl_no]);
					ac_derived_tbl = derived_tbls[ac_tbl_no];
				}
				m_saved.last_dc_val[k] = 0;
			}
			m_saved.EOBRUN = 0u;
			return;
		}
		if (m_cinfo.m_Ss != 0 || m_cinfo.m_Ah != 0 || m_cinfo.m_Al != 0 || ((m_cinfo.is_baseline || m_cinfo.m_Se < 64) && m_cinfo.m_Se != m_cinfo.lim_Se))
		{
			m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_NOT_SEQUENTIAL);
		}
		if (m_cinfo.lim_Se != 63)
		{
			decode_mcu = decode_mcu_sub;
		}
		else
		{
			decode_mcu = decode_mcu_full;
		}
		for (int l = 0; l < m_cinfo.m_comps_in_scan; l++)
		{
			jpeg_component_info jpeg_component_info2 = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[l]];
			int dc_tbl_no2 = jpeg_component_info2.Dc_tbl_no;
			jpeg_make_d_derived_tbl(isDC: true, dc_tbl_no2, ref m_dc_derived_tbls[dc_tbl_no2]);
			if (m_cinfo.lim_Se != 0)
			{
				dc_tbl_no2 = jpeg_component_info2.Ac_tbl_no;
				jpeg_make_d_derived_tbl(isDC: false, dc_tbl_no2, ref m_ac_derived_tbls[dc_tbl_no2]);
			}
			m_saved.last_dc_val[l] = 0;
		}
		for (int m = 0; m < m_cinfo.m_blocks_in_MCU; m++)
		{
			int num2 = m_cinfo.m_MCU_membership[m];
			jpeg_component_info jpeg_component_info3 = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[num2]];
			m_dc_cur_tbls[m] = m_dc_derived_tbls[jpeg_component_info3.Dc_tbl_no];
			m_ac_cur_tbls[m] = m_ac_derived_tbls[jpeg_component_info3.Ac_tbl_no];
			if (jpeg_component_info3.component_needed)
			{
				num2 = jpeg_component_info3.DCT_v_scaled_size;
				int num3 = jpeg_component_info3.DCT_h_scaled_size;
				switch (m_cinfo.lim_Se)
				{
				case 0:
					coef_limit[m] = 1;
					break;
				case 3:
					if (num2 <= 0 || num2 > 2)
					{
						num2 = 2;
					}
					if (num3 <= 0 || num3 > 2)
					{
						num3 = 2;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order2[num2 - 1][num3 - 1];
					break;
				case 8:
					if (num2 <= 0 || num2 > 3)
					{
						num2 = 3;
					}
					if (num3 <= 0 || num3 > 3)
					{
						num3 = 3;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order3[num2 - 1][num3 - 1];
					break;
				case 15:
					if (num2 <= 0 || num2 > 4)
					{
						num2 = 4;
					}
					if (num3 <= 0 || num3 > 4)
					{
						num3 = 4;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order4[num2 - 1][num3 - 1];
					break;
				case 24:
					if (num2 <= 0 || num2 > 5)
					{
						num2 = 5;
					}
					if (num3 <= 0 || num3 > 5)
					{
						num3 = 5;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order5[num2 - 1][num3 - 1];
					break;
				case 35:
					if (num2 <= 0 || num2 > 6)
					{
						num2 = 6;
					}
					if (num3 <= 0 || num3 > 6)
					{
						num3 = 6;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order6[num2 - 1][num3 - 1];
					break;
				case 48:
					if (num2 <= 0 || num2 > 7)
					{
						num2 = 7;
					}
					if (num3 <= 0 || num3 > 7)
					{
						num3 = 7;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order7[num2 - 1][num3 - 1];
					break;
				default:
					if (num2 <= 0 || num2 > 8)
					{
						num2 = 8;
					}
					if (num3 <= 0 || num3 > 8)
					{
						num3 = 8;
					}
					coef_limit[m] = 1 + jpeg_zigzag_order[num2 - 1][num3 - 1];
					break;
				}
			}
			else
			{
				coef_limit[m] = 0;
			}
		}
		m_bitstate.bits_left = 0;
		m_bitstate.get_buffer = 0;
		m_insufficient_data = false;
		m_restarts_to_go = m_cinfo.m_restart_interval;
	}

	private bool decode_mcu_full(JBLOCK[] MCU_data)
	{
		if (m_cinfo.m_restart_interval != 0 && m_restarts_to_go == 0 && !process_restart())
		{
			return false;
		}
		if (!m_insufficient_data)
		{
			BITREAD_LOAD_STATE();
			savable_state savable_state = new savable_state();
			savable_state.Assign(m_saved);
			for (int i = 0; i < m_cinfo.m_blocks_in_MCU; i++)
			{
				d_derived_tbl htbl = m_dc_cur_tbls[i];
				int num = HUFF_DECODE(htbl);
				htbl = m_ac_cur_tbls[i];
				int num2 = 1;
				int num3 = coef_limit[i];
				bool flag = false;
				if (num3 != 0)
				{
					if (num != 0)
					{
						CHECK_BIT_BUFFER(num);
						bits_left -= num;
						int num4 = (get_buffer >> bits_left) & bmask[num];
						num = ((num4 <= bmask[num - 1]) ? (num4 - bmask[num]) : num4);
					}
					int num5 = m_cinfo.m_MCU_membership[i];
					num += savable_state.last_dc_val[num5];
					savable_state.last_dc_val[num5] = num;
					short[] data = MCU_data[i].data;
					data[0] = (short)num;
					while (num2 < num3)
					{
						num = HUFF_DECODE(htbl);
						int num6 = num >> 4;
						num &= 0xF;
						if (num != 0)
						{
							num2 += num6;
							CHECK_BIT_BUFFER(num);
							bits_left -= num;
							num6 = (get_buffer >> bits_left) & bmask[num];
							num = ((num6 <= bmask[num - 1]) ? (num6 - bmask[num]) : num6);
							data[JpegUtils.jpeg_natural_order[num2]] = (short)num;
						}
						else
						{
							if (num6 != 15)
							{
								flag = true;
								break;
							}
							num2 += 15;
						}
						num2++;
					}
				}
				else if (num != 0)
				{
					CHECK_BIT_BUFFER(num);
					bits_left -= num;
				}
				if (flag)
				{
					continue;
				}
				while (num2 < 64)
				{
					num = HUFF_DECODE(htbl);
					int num7 = num >> 4;
					num &= 0xF;
					if (num != 0)
					{
						num2 += num7;
						CHECK_BIT_BUFFER(num);
						bits_left -= num;
					}
					else
					{
						if (num7 != 15)
						{
							break;
						}
						num2 += 15;
					}
					num2++;
				}
			}
			BITREAD_SAVE_STATE();
			m_saved.Assign(savable_state);
		}
		m_restarts_to_go--;
		return true;
	}

	private bool decode_mcu_sub(JBLOCK[] MCU_data)
	{
		if (m_cinfo.m_restart_interval != 0 && m_restarts_to_go == 0 && !process_restart())
		{
			return false;
		}
		if (!m_insufficient_data)
		{
			int[] natural_order = m_cinfo.natural_order;
			int se = m_cinfo.m_Se;
			BITREAD_LOAD_STATE();
			savable_state savable_state = new savable_state();
			savable_state.Assign(m_saved);
			for (int i = 0; i < m_cinfo.m_blocks_in_MCU; i++)
			{
				d_derived_tbl htbl = m_dc_cur_tbls[i];
				int num = HUFF_DECODE(htbl);
				htbl = m_ac_cur_tbls[i];
				int j = 1;
				int num2 = coef_limit[i];
				if (num2 != 0)
				{
					if (num != 0)
					{
						CHECK_BIT_BUFFER(num);
						bits_left -= num;
						int num3 = (get_buffer >> bits_left) & bmask[num];
						num = ((num3 <= bmask[num - 1]) ? (num3 - bmask[num]) : num3);
					}
					int num4 = m_cinfo.m_MCU_membership[i];
					num += savable_state.last_dc_val[num4];
					savable_state.last_dc_val[num4] = num;
					short[] data = MCU_data[i].data;
					data[0] = (short)num;
					for (; j < num2; j++)
					{
						num = HUFF_DECODE(htbl);
						int num5 = num >> 4;
						num &= 0xF;
						if (num != 0)
						{
							j += num5;
							CHECK_BIT_BUFFER(num);
							bits_left -= num;
							num5 = (get_buffer >> bits_left) & bmask[num];
							num = ((num5 <= bmask[num - 1]) ? (num5 - bmask[num]) : num5);
							data[natural_order[j]] = (short)num;
						}
						else if (num5 == 15)
						{
							j += 15;
						}
					}
				}
				else if (num != 0)
				{
					CHECK_BIT_BUFFER(num);
					bits_left -= num;
				}
				while (j <= se)
				{
					num = HUFF_DECODE(htbl);
					int num6 = num >> 4;
					num &= 0xF;
					if (num != 0)
					{
						j += num6;
						CHECK_BIT_BUFFER(num);
						bits_left -= num;
					}
					else
					{
						if (num6 != 15)
						{
							break;
						}
						j += 15;
					}
					j++;
				}
			}
			BITREAD_SAVE_STATE();
			m_saved.Assign(savable_state);
		}
		m_restarts_to_go--;
		return true;
	}

	private bool decode_mcu_DC_first(JBLOCK[] MCU_data)
	{
		if (m_cinfo.m_restart_interval != 0 && m_restarts_to_go == 0 && !process_restart())
		{
			return false;
		}
		if (!m_insufficient_data)
		{
			BITREAD_LOAD_STATE();
			savable_state savable_state = new savable_state();
			savable_state.Assign(m_saved);
			for (int i = 0; i < m_cinfo.m_blocks_in_MCU; i++)
			{
				int num = m_cinfo.m_MCU_membership[i];
				int num2 = HUFF_DECODE(derived_tbls[m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[num]].Dc_tbl_no]);
				if (num2 != 0)
				{
					CHECK_BIT_BUFFER(num2);
					bits_left -= num2;
					int num3 = (get_buffer >> bits_left) & bmask[num2];
					num2 = ((num3 <= bmask[num2 - 1]) ? (num3 - bmask[num2]) : num3);
				}
				num2 += savable_state.last_dc_val[num];
				savable_state.last_dc_val[num] = num2;
				MCU_data[i][0] = (short)(num2 << m_cinfo.m_Al);
			}
			BITREAD_SAVE_STATE();
			m_saved.Assign(savable_state);
		}
		m_restarts_to_go--;
		return true;
	}

	private bool decode_mcu_AC_first(JBLOCK[] MCU_data)
	{
		if (m_cinfo.m_restart_interval != 0 && m_restarts_to_go == 0 && !process_restart())
		{
			return false;
		}
		if (!m_insufficient_data)
		{
			uint num = m_saved.EOBRUN;
			int[] natural_order = m_cinfo.natural_order;
			if (num != 0)
			{
				num--;
			}
			else
			{
				BITREAD_LOAD_STATE();
				short[] data = MCU_data[0].data;
				int num2;
				for (num2 = m_cinfo.m_Ss; num2 <= m_cinfo.m_Se; num2++)
				{
					int num3 = HUFF_DECODE(ac_derived_tbl);
					int num4 = num3 >> 4;
					num3 &= 0xF;
					if (num3 != 0)
					{
						num2 += num4;
						CHECK_BIT_BUFFER(num3);
						bits_left -= num3;
						num4 = (get_buffer >> bits_left) & bmask[num3];
						num3 = ((num4 <= bmask[num3 - 1]) ? (num4 - bmask[num3]) : num4);
						data[natural_order[num2]] = (short)(num3 << m_cinfo.m_Al);
						continue;
					}
					switch (num4)
					{
					default:
						num = (uint)(1 << num4);
						CHECK_BIT_BUFFER(num4);
						bits_left -= num4;
						num4 = (get_buffer >> bits_left) & bmask[num4];
						num += (uint)num4;
						num--;
						break;
					case 15:
						num2 += 15;
						continue;
					case 0:
						break;
					}
					break;
				}
				BITREAD_SAVE_STATE();
			}
			m_saved.EOBRUN = num;
		}
		m_restarts_to_go--;
		return true;
	}

	private bool decode_mcu_DC_refine(JBLOCK[] MCU_data)
	{
		if (m_cinfo.m_restart_interval != 0 && m_restarts_to_go == 0 && !process_restart())
		{
			return false;
		}
		BITREAD_LOAD_STATE();
		int num = 1 << m_cinfo.m_Al;
		for (int i = 0; i < m_cinfo.m_blocks_in_MCU; i++)
		{
			CHECK_BIT_BUFFER(1);
			bits_left--;
			if (((get_buffer >> bits_left) & bmask[1]) != 0)
			{
				short[] data = MCU_data[i].data;
				data[0] = (short)((ushort)data[0] | num);
			}
		}
		BITREAD_SAVE_STATE();
		m_restarts_to_go--;
		return true;
	}

	private bool decode_mcu_AC_refine(JBLOCK[] MCU_data)
	{
		if (m_cinfo.m_restart_interval != 0 && m_restarts_to_go == 0 && !process_restart())
		{
			return false;
		}
		if (!m_insufficient_data)
		{
			int num = 1 << m_cinfo.m_Al;
			int num2 = -1 << m_cinfo.m_Al;
			int[] natural_order = m_cinfo.natural_order;
			BITREAD_LOAD_STATE();
			uint num3 = m_saved.EOBRUN;
			int num4 = m_cinfo.m_Ss;
			short[] data = MCU_data[0].data;
			if (num3 == 0)
			{
				do
				{
					int num5 = HUFF_DECODE(ac_derived_tbl);
					int num6 = num5 >> 4;
					num5 &= 0xF;
					if (num5 != 0)
					{
						if (num5 != 1)
						{
							m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_HUFF_BAD_CODE);
						}
						CHECK_BIT_BUFFER(1);
						bits_left--;
						num5 = ((((get_buffer >> bits_left) & bmask[1]) == 0) ? num2 : num);
					}
					else if (num6 != 15)
					{
						num3 = (uint)(1 << num6);
						if (num6 != 0)
						{
							CHECK_BIT_BUFFER(num6);
							bits_left -= num6;
							num6 = (get_buffer >> bits_left) & bmask[num6];
							num3 += (uint)num6;
						}
						break;
					}
					do
					{
						int num7 = natural_order[num4];
						short num8 = data[num7];
						if (num8 != 0)
						{
							CHECK_BIT_BUFFER(1);
							bits_left--;
							if (((get_buffer >> bits_left) & bmask[1]) != 0 && (num8 & num) == 0)
							{
								if (num8 >= 0)
								{
									data[num7] += (short)num;
								}
								else
								{
									data[num7] += (short)num2;
								}
							}
						}
						else if (--num6 < 0)
						{
							break;
						}
						num4++;
					}
					while (num4 <= m_cinfo.m_Se);
					if (num5 != 0)
					{
						int num9 = natural_order[num4];
						data[num9] = (short)num5;
					}
					num4++;
				}
				while (num4 <= m_cinfo.m_Se);
			}
			if (num3 != 0)
			{
				do
				{
					int num10 = natural_order[num4];
					short num11 = data[num10];
					if (num11 != 0)
					{
						CHECK_BIT_BUFFER(1);
						bits_left--;
						if (((get_buffer >> bits_left) & bmask[1]) != 0 && (num11 & num) == 0)
						{
							if (num11 >= 0)
							{
								data[num10] += (short)num;
							}
							else
							{
								data[num10] += (short)num2;
							}
						}
					}
					num4++;
				}
				while (num4 <= m_cinfo.m_Se);
				num3--;
			}
			BITREAD_SAVE_STATE();
			m_saved.EOBRUN = num3;
		}
		m_restarts_to_go--;
		return true;
	}

	public void finish_pass_huff()
	{
		m_cinfo.m_marker.SkipBytes(m_bitstate.bits_left / 8);
		m_bitstate.bits_left = 0;
	}

	private bool process_restart()
	{
		finish_pass_huff();
		if (!m_cinfo.m_marker.read_restart_marker())
		{
			return false;
		}
		for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
		{
			m_saved.last_dc_val[i] = 0;
		}
		m_saved.EOBRUN = 0u;
		m_restarts_to_go = m_cinfo.m_restart_interval;
		if (m_cinfo.m_unread_marker == 0)
		{
			m_insufficient_data = false;
		}
		return true;
	}

	private void BITREAD_LOAD_STATE()
	{
		get_buffer = m_bitstate.get_buffer;
		bits_left = m_bitstate.bits_left;
	}

	private void BITREAD_SAVE_STATE()
	{
		m_bitstate.get_buffer = get_buffer;
		m_bitstate.bits_left = bits_left;
	}

	private void CHECK_BIT_BUFFER(int nbits)
	{
		if (bits_left < nbits)
		{
			jpeg_fill_bit_buffer(nbits);
		}
	}

	private int HUFF_DECODE(d_derived_tbl htbl)
	{
		int min_bits = 0;
		bool flag = false;
		if (bits_left < 8)
		{
			jpeg_fill_bit_buffer(0);
			if (bits_left < 8)
			{
				min_bits = 1;
				flag = true;
			}
		}
		if (!flag)
		{
			int num = (get_buffer >> bits_left - 8) & bmask[8];
			if ((min_bits = htbl.look_nbits[num]) != 0)
			{
				bits_left -= min_bits;
				return htbl.look_sym[num];
			}
			min_bits = 9;
		}
		return jpeg_huff_decode(htbl, min_bits);
	}

	private void jpeg_fill_bit_buffer(int nbits)
	{
		bool flag = false;
		if (m_cinfo.m_unread_marker == 0)
		{
			while (bits_left < 25)
			{
				m_cinfo.m_src.GetByte(out var V);
				if (V == 255)
				{
					do
					{
						m_cinfo.m_src.GetByte(out V);
					}
					while (V == 255);
					if (V != 0)
					{
						m_cinfo.m_unread_marker = V;
						flag = true;
						break;
					}
					V = 255;
				}
				get_buffer = (get_buffer << 8) | V;
				bits_left += 8;
			}
		}
		else
		{
			flag = true;
		}
		if (flag && nbits > bits_left)
		{
			huff_entropy_decoder huff_entropy_decoder2 = (huff_entropy_decoder)m_cinfo.m_entropy;
			if (!huff_entropy_decoder2.m_insufficient_data)
			{
				m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_HIT_MARKER);
				huff_entropy_decoder2.m_insufficient_data = true;
			}
			get_buffer <<= 25 - bits_left;
			bits_left = 25;
		}
	}

	private void jpeg_make_d_derived_tbl(bool isDC, int tblno, ref d_derived_tbl dtbl)
	{
		if (tblno < 0 || tblno >= 4)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NO_HUFF_TABLE, tblno);
		}
		JHUFF_TBL jHUFF_TBL = (isDC ? m_cinfo.m_dc_huff_tbl_ptrs[tblno] : m_cinfo.m_ac_huff_tbl_ptrs[tblno]);
		if (jHUFF_TBL == null)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NO_HUFF_TABLE, tblno);
			return;
		}
		if (dtbl == null)
		{
			dtbl = new d_derived_tbl();
		}
		dtbl.pub = jHUFF_TBL;
		int num = 0;
		char[] array = new char[257];
		for (int i = 1; i <= 16; i++)
		{
			int num2 = jHUFF_TBL.Bits[i];
			if (num + num2 > 256)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_HUFF_TABLE);
			}
			while (num2-- != 0)
			{
				array[num++] = (char)i;
			}
		}
		array[num] = '\0';
		int num3 = num;
		int num4 = 0;
		int num5 = array[0];
		int[] array2 = new int[257];
		num = 0;
		while (array[num] != 0)
		{
			while (array[num] == num5)
			{
				array2[num++] = num4;
				num4++;
			}
			if (num4 >= 1 << num5)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_HUFF_TABLE);
			}
			num4 <<= 1;
			num5++;
		}
		num = 0;
		for (int j = 1; j <= 16; j++)
		{
			if (jHUFF_TBL.Bits[j] != 0)
			{
				dtbl.valoffset[j] = num - array2[num];
				num += jHUFF_TBL.Bits[j];
				dtbl.maxcode[j] = array2[num - 1];
			}
			else
			{
				dtbl.maxcode[j] = -1;
			}
		}
		dtbl.maxcode[17] = 1048575;
		Array.Clear(dtbl.look_nbits, 0, dtbl.look_nbits.Length);
		num = 0;
		for (int k = 1; k <= 8; k++)
		{
			int num6 = 1;
			while (num6 <= jHUFF_TBL.Bits[k])
			{
				int num7 = array2[num] << 8 - k;
				for (int num8 = 1 << 8 - k; num8 > 0; num8--)
				{
					dtbl.look_nbits[num7] = k;
					dtbl.look_sym[num7] = jHUFF_TBL.Huffval[num];
					num7++;
				}
				num6++;
				num++;
			}
		}
		if (!isDC)
		{
			return;
		}
		for (int l = 0; l < num3; l++)
		{
			if (jHUFF_TBL.Huffval[l] > 15)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_HUFF_TABLE);
			}
		}
	}

	protected int jpeg_huff_decode(d_derived_tbl htbl, int min_bits)
	{
		int i = min_bits;
		CHECK_BIT_BUFFER(i);
		bits_left -= i;
		int num;
		for (num = (get_buffer >> bits_left) & bmask[i]; num > htbl.maxcode[i]; i++)
		{
			num <<= 1;
			CHECK_BIT_BUFFER(1);
			bits_left--;
			num |= (get_buffer >> bits_left) & bmask[1];
		}
		if (i > 16)
		{
			m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_HUFF_BAD_CODE);
			return 0;
		}
		return htbl.pub.Huffval[num + htbl.valoffset[i]];
	}
}
