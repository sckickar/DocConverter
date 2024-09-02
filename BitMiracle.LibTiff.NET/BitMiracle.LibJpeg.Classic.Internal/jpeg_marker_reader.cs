using System;

namespace BitMiracle.LibJpeg.Classic.Internal;

internal class jpeg_marker_reader
{
	private const int APP0_DATA_LEN = 14;

	private const int APP14_DATA_LEN = 12;

	private const int APPN_DATA_LEN = 14;

	private jpeg_decompress_struct m_cinfo;

	private jpeg_decompress_struct.jpeg_marker_parser_method m_process_COM;

	private jpeg_decompress_struct.jpeg_marker_parser_method[] m_process_APPn = new jpeg_decompress_struct.jpeg_marker_parser_method[16];

	private int m_length_limit_COM;

	private int[] m_length_limit_APPn = new int[16];

	private bool m_saw_SOI;

	private bool m_saw_SOF;

	private int m_next_restart_num;

	private int m_discarded_bytes;

	private jpeg_marker_struct m_cur_marker;

	private int m_bytes_read;

	public jpeg_marker_reader(jpeg_decompress_struct cinfo)
	{
		m_cinfo = cinfo;
		m_process_COM = skip_variable;
		for (int i = 0; i < 16; i++)
		{
			m_process_APPn[i] = skip_variable;
			m_length_limit_APPn[i] = 0;
		}
		m_process_APPn[0] = get_interesting_appn;
		m_process_APPn[14] = get_interesting_appn;
		reset_marker_reader();
	}

	public void reset_marker_reader()
	{
		m_cinfo.Comp_info = null;
		m_cinfo.m_input_scan_number = 0;
		m_cinfo.m_unread_marker = 0;
		m_saw_SOI = false;
		m_saw_SOF = false;
		m_discarded_bytes = 0;
		m_cur_marker = null;
	}

	public ReadResult read_markers()
	{
		while (true)
		{
			if (m_cinfo.m_unread_marker == 0)
			{
				if (!m_cinfo.m_marker.m_saw_SOI)
				{
					if (!first_marker())
					{
						return ReadResult.JPEG_SUSPENDED;
					}
				}
				else if (!next_marker())
				{
					break;
				}
			}
			switch ((JPEG_MARKER)m_cinfo.m_unread_marker)
			{
			case JPEG_MARKER.SOI:
				get_soi();
				break;
			case JPEG_MARKER.SOF0:
				if (!get_sof(is_baseline: true, is_prog: false, is_arith: false))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.SOF1:
				if (!get_sof(is_baseline: false, is_prog: false, is_arith: false))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.SOF2:
				if (!get_sof(is_baseline: false, is_prog: true, is_arith: false))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.SOF9:
				if (!get_sof(is_baseline: false, is_prog: false, is_arith: true))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.SOF10:
				if (!get_sof(is_baseline: false, is_prog: true, is_arith: true))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.SOF3:
			case JPEG_MARKER.SOF5:
			case JPEG_MARKER.SOF6:
			case JPEG_MARKER.SOF7:
			case JPEG_MARKER.JPG:
			case JPEG_MARKER.SOF11:
			case JPEG_MARKER.SOF13:
			case JPEG_MARKER.SOF14:
			case JPEG_MARKER.SOF15:
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_SOF_UNSUPPORTED, m_cinfo.m_unread_marker);
				break;
			case JPEG_MARKER.SOS:
				if (!get_sos())
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				m_cinfo.m_unread_marker = 0;
				return ReadResult.JPEG_REACHED_SOS;
			case JPEG_MARKER.EOI:
				m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_EOI);
				m_cinfo.m_unread_marker = 0;
				return ReadResult.JPEG_REACHED_EOI;
			case JPEG_MARKER.DAC:
				if (!get_dac())
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.DHT:
				if (!get_dht())
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.DQT:
				if (!get_dqt())
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.DRI:
				if (!get_dri())
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.JPG8:
				if (!get_lse())
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.APP0:
			case JPEG_MARKER.APP1:
			case JPEG_MARKER.APP2:
			case JPEG_MARKER.APP3:
			case JPEG_MARKER.APP4:
			case JPEG_MARKER.APP5:
			case JPEG_MARKER.APP6:
			case JPEG_MARKER.APP7:
			case JPEG_MARKER.APP8:
			case JPEG_MARKER.APP9:
			case JPEG_MARKER.APP10:
			case JPEG_MARKER.APP11:
			case JPEG_MARKER.APP12:
			case JPEG_MARKER.APP13:
			case JPEG_MARKER.APP14:
			case JPEG_MARKER.APP15:
				if (!m_cinfo.m_marker.m_process_APPn[m_cinfo.m_unread_marker - 224](m_cinfo))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.COM:
				if (!m_cinfo.m_marker.m_process_COM(m_cinfo))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			case JPEG_MARKER.TEM:
			case JPEG_MARKER.RST0:
			case JPEG_MARKER.RST1:
			case JPEG_MARKER.RST2:
			case JPEG_MARKER.RST3:
			case JPEG_MARKER.RST4:
			case JPEG_MARKER.RST5:
			case JPEG_MARKER.RST6:
			case JPEG_MARKER.RST7:
				m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_PARMLESS_MARKER, m_cinfo.m_unread_marker);
				break;
			case JPEG_MARKER.DNL:
				if (!skip_variable(m_cinfo))
				{
					return ReadResult.JPEG_SUSPENDED;
				}
				break;
			default:
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_UNKNOWN_MARKER, m_cinfo.m_unread_marker);
				break;
			}
			m_cinfo.m_unread_marker = 0;
		}
		return ReadResult.JPEG_SUSPENDED;
	}

	public bool read_restart_marker()
	{
		if (m_cinfo.m_unread_marker == 0 && !next_marker())
		{
			return false;
		}
		if (m_cinfo.m_unread_marker == 208 + m_cinfo.m_marker.m_next_restart_num)
		{
			m_cinfo.TRACEMS(3, J_MESSAGE_CODE.JTRC_RST, m_cinfo.m_marker.m_next_restart_num);
			m_cinfo.m_unread_marker = 0;
		}
		else if (!m_cinfo.m_src.resync_to_restart(m_cinfo, m_cinfo.m_marker.m_next_restart_num))
		{
			return false;
		}
		m_cinfo.m_marker.m_next_restart_num = (m_cinfo.m_marker.m_next_restart_num + 1) & 7;
		return true;
	}

	public bool next_marker()
	{
		int V;
		while (m_cinfo.m_src.GetByte(out V))
		{
			while (V != 255)
			{
				m_cinfo.m_marker.m_discarded_bytes++;
				if (!m_cinfo.m_src.GetByte(out V))
				{
					return false;
				}
			}
			while (true)
			{
				if (!m_cinfo.m_src.GetByte(out V))
				{
					return false;
				}
				switch (V)
				{
				case 255:
					continue;
				case 0:
					goto end_IL_004c;
				}
				if (m_cinfo.m_marker.m_discarded_bytes != 0)
				{
					m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_EXTRANEOUS_DATA, m_cinfo.m_marker.m_discarded_bytes, V);
					m_cinfo.m_marker.m_discarded_bytes = 0;
				}
				m_cinfo.m_unread_marker = V;
				return true;
				continue;
				end_IL_004c:
				break;
			}
			m_cinfo.m_marker.m_discarded_bytes += 2;
		}
		return false;
	}

	public void jpeg_set_marker_processor(int marker_code, jpeg_decompress_struct.jpeg_marker_parser_method routine)
	{
		switch (marker_code)
		{
		case 254:
			m_process_COM = routine;
			break;
		case 224:
		case 225:
		case 226:
		case 227:
		case 228:
		case 229:
		case 230:
		case 231:
		case 232:
		case 233:
		case 234:
		case 235:
		case 236:
		case 237:
		case 238:
		case 239:
			m_process_APPn[marker_code - 224] = routine;
			break;
		default:
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_UNKNOWN_MARKER, marker_code);
			break;
		}
	}

	public void jpeg_save_markers(int marker_code, int length_limit)
	{
		jpeg_decompress_struct.jpeg_marker_parser_method jpeg_marker_parser_method;
		if (length_limit != 0)
		{
			jpeg_marker_parser_method = save_marker;
			if (marker_code == 224 && length_limit < 14)
			{
				length_limit = 14;
			}
			else if (marker_code == 238 && length_limit < 12)
			{
				length_limit = 12;
			}
		}
		else
		{
			jpeg_marker_parser_method = skip_variable;
			if (marker_code == 224 || marker_code == 238)
			{
				jpeg_marker_parser_method = get_interesting_appn;
			}
		}
		switch (marker_code)
		{
		case 254:
			m_process_COM = jpeg_marker_parser_method;
			m_length_limit_COM = length_limit;
			break;
		case 224:
		case 225:
		case 226:
		case 227:
		case 228:
		case 229:
		case 230:
		case 231:
		case 232:
		case 233:
		case 234:
		case 235:
		case 236:
		case 237:
		case 238:
		case 239:
			m_process_APPn[marker_code - 224] = jpeg_marker_parser_method;
			m_length_limit_APPn[marker_code - 224] = length_limit;
			break;
		default:
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_UNKNOWN_MARKER, marker_code);
			break;
		}
	}

	public bool SawSOI()
	{
		return m_saw_SOI;
	}

	public bool SawSOF()
	{
		return m_saw_SOF;
	}

	public int NextRestartNumber()
	{
		return m_next_restart_num;
	}

	public int DiscardedByteCount()
	{
		return m_discarded_bytes;
	}

	public void SkipBytes(int count)
	{
		m_discarded_bytes += count;
	}

	private static bool save_marker(jpeg_decompress_struct cinfo)
	{
		jpeg_marker_struct jpeg_marker_struct = cinfo.m_marker.m_cur_marker;
		byte[] array = null;
		int V = 0;
		int num = 0;
		int num3;
		int num4;
		if (jpeg_marker_struct == null)
		{
			if (!cinfo.m_src.GetTwoBytes(out V))
			{
				return false;
			}
			V -= 2;
			if (V >= 0)
			{
				int num2 = ((cinfo.m_unread_marker != 254) ? cinfo.m_marker.m_length_limit_APPn[cinfo.m_unread_marker - 224] : cinfo.m_marker.m_length_limit_COM);
				if (V < num2)
				{
					num2 = V;
				}
				jpeg_marker_struct = new jpeg_marker_struct((byte)cinfo.m_unread_marker, V, num2);
				array = jpeg_marker_struct.Data;
				cinfo.m_marker.m_cur_marker = jpeg_marker_struct;
				cinfo.m_marker.m_bytes_read = 0;
				num3 = 0;
				num4 = num2;
			}
			else
			{
				num3 = (num4 = 0);
				array = null;
			}
		}
		else
		{
			num3 = cinfo.m_marker.m_bytes_read;
			num4 = jpeg_marker_struct.Data.Length;
			array = jpeg_marker_struct.Data;
			num = num3;
		}
		byte[] array2 = null;
		if (num4 != 0)
		{
			array2 = new byte[array.Length];
		}
		while (num3 < num4)
		{
			cinfo.m_marker.m_bytes_read = num3;
			if (!cinfo.m_src.MakeByteAvailable())
			{
				return false;
			}
			int bytes = cinfo.m_src.GetBytes(array2, num4 - num3);
			Buffer.BlockCopy(array2, 0, array, num, bytes);
			num3 += bytes;
			num += bytes;
		}
		if (jpeg_marker_struct != null)
		{
			cinfo.m_marker_list.Add(jpeg_marker_struct);
			array = jpeg_marker_struct.Data;
			num = 0;
			V = jpeg_marker_struct.OriginalLength - num4;
		}
		cinfo.m_marker.m_cur_marker = null;
		JPEG_MARKER unread_marker = (JPEG_MARKER)cinfo.m_unread_marker;
		if (num4 != 0 && (unread_marker == JPEG_MARKER.APP0 || unread_marker == JPEG_MARKER.APP14))
		{
			array2 = new byte[array.Length];
			Buffer.BlockCopy(array, num, array2, 0, array.Length - num);
		}
		switch ((JPEG_MARKER)cinfo.m_unread_marker)
		{
		case JPEG_MARKER.APP0:
			examine_app0(cinfo, array2, num4, V);
			break;
		case JPEG_MARKER.APP14:
			examine_app14(cinfo, array2, num4, V);
			break;
		default:
			cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_MISC_MARKER, cinfo.m_unread_marker, num4 + V);
			break;
		}
		if (V > 0)
		{
			cinfo.m_src.skip_input_data(V);
		}
		return true;
	}

	private static bool skip_variable(jpeg_decompress_struct cinfo)
	{
		if (!cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		V -= 2;
		cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_MISC_MARKER, cinfo.m_unread_marker, V);
		if (V > 0)
		{
			cinfo.m_src.skip_input_data(V);
		}
		return true;
	}

	private static bool get_interesting_appn(jpeg_decompress_struct cinfo)
	{
		if (!cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		V -= 2;
		int num = 0;
		if (V >= 14)
		{
			num = 14;
		}
		else if (V > 0)
		{
			num = V;
		}
		byte[] array = new byte[14];
		for (int i = 0; i < num; i++)
		{
			int V2 = 0;
			if (!cinfo.m_src.GetByte(out V2))
			{
				return false;
			}
			array[i] = (byte)V2;
		}
		V -= num;
		switch ((JPEG_MARKER)cinfo.m_unread_marker)
		{
		case JPEG_MARKER.APP0:
			examine_app0(cinfo, array, num, V);
			break;
		case JPEG_MARKER.APP14:
			examine_app14(cinfo, array, num, V);
			break;
		default:
			cinfo.ERREXIT(J_MESSAGE_CODE.JERR_UNKNOWN_MARKER, cinfo.m_unread_marker);
			break;
		}
		if (V > 0)
		{
			cinfo.m_src.skip_input_data(V);
		}
		return true;
	}

	private static void examine_app0(jpeg_decompress_struct cinfo, byte[] data, int datalen, int remaining)
	{
		int num = datalen + remaining;
		if (datalen >= 14 && data[0] == 74 && data[1] == 70 && data[2] == 73 && data[3] == 70 && data[4] == 0)
		{
			cinfo.m_saw_JFIF_marker = true;
			cinfo.m_JFIF_major_version = data[5];
			cinfo.m_JFIF_minor_version = data[6];
			cinfo.m_density_unit = (DensityUnit)data[7];
			cinfo.m_X_density = (short)((data[8] << 8) + data[9]);
			cinfo.m_Y_density = (short)((data[10] << 8) + data[11]);
			if (cinfo.m_JFIF_major_version != 1 && cinfo.m_JFIF_major_version != 2)
			{
				cinfo.WARNMS(J_MESSAGE_CODE.JWRN_JFIF_MAJOR, cinfo.m_JFIF_major_version, cinfo.m_JFIF_minor_version);
			}
			cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_JFIF, cinfo.m_JFIF_major_version, cinfo.m_JFIF_minor_version, cinfo.m_X_density, cinfo.m_Y_density, cinfo.m_density_unit);
			if ((data[12] | data[13]) != 0)
			{
				cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_JFIF_THUMBNAIL, data[12], data[13]);
			}
			num -= 14;
			if (num != data[12] * data[13] * 3)
			{
				cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_JFIF_BADTHUMBNAILSIZE, num);
			}
		}
		else if (datalen >= 6 && data[0] == 74 && data[1] == 70 && data[2] == 88 && data[3] == 88 && data[4] == 0)
		{
			switch (data[5])
			{
			case 16:
				cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_THUMB_JPEG, num);
				break;
			case 17:
				cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_THUMB_PALETTE, num);
				break;
			case 19:
				cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_THUMB_RGB, num);
				break;
			default:
				cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_JFIF_EXTENSION, data[5], num);
				break;
			}
		}
		else
		{
			cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_APP0, num);
		}
	}

	private static void examine_app14(jpeg_decompress_struct cinfo, byte[] data, int datalen, int remaining)
	{
		if (datalen >= 12 && data[0] == 65 && data[1] == 100 && data[2] == 111 && data[3] == 98 && data[4] == 101)
		{
			int num = (data[5] << 8) + data[6];
			int num2 = (data[7] << 8) + data[8];
			int num3 = (data[9] << 8) + data[10];
			int num4 = data[11];
			cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_ADOBE, num, num2, num3, num4);
			cinfo.m_saw_Adobe_marker = true;
			cinfo.m_Adobe_transform = (byte)num4;
		}
		else
		{
			cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_APP14, datalen + remaining);
		}
	}

	private void get_soi()
	{
		m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_SOI);
		if (m_cinfo.m_marker.m_saw_SOI)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_SOI_DUPLICATE);
		}
		m_cinfo.m_restart_interval = 0;
		m_cinfo.m_jpeg_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
		m_cinfo.color_transform = J_COLOR_TRANSFORM.JCT_NONE;
		m_cinfo.m_CCIR601_sampling = false;
		m_cinfo.m_saw_JFIF_marker = false;
		m_cinfo.m_JFIF_major_version = 1;
		m_cinfo.m_JFIF_minor_version = 1;
		m_cinfo.m_density_unit = DensityUnit.Unknown;
		m_cinfo.m_X_density = 1;
		m_cinfo.m_Y_density = 1;
		m_cinfo.m_saw_Adobe_marker = false;
		m_cinfo.m_Adobe_transform = 0;
		m_cinfo.m_marker.m_saw_SOI = true;
	}

	private bool get_sof(bool is_baseline, bool is_prog, bool is_arith)
	{
		m_cinfo.is_baseline = is_baseline;
		m_cinfo.m_progressive_mode = is_prog;
		m_cinfo.arith_code = is_arith;
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		if (!m_cinfo.m_src.GetByte(out m_cinfo.m_data_precision))
		{
			return false;
		}
		int V2 = 0;
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		m_cinfo.m_image_height = V2;
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		m_cinfo.m_image_width = V2;
		if (!m_cinfo.m_src.GetByte(out m_cinfo.m_num_components))
		{
			return false;
		}
		V -= 8;
		m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_SOF, m_cinfo.m_unread_marker, m_cinfo.m_image_width, m_cinfo.m_image_height, m_cinfo.m_num_components);
		if (m_cinfo.m_marker.m_saw_SOF)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_SOF_DUPLICATE);
		}
		if (m_cinfo.m_image_height <= 0 || m_cinfo.m_image_width <= 0 || m_cinfo.m_num_components <= 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_EMPTY_IMAGE);
		}
		if (V != m_cinfo.m_num_components * 3)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		if (m_cinfo.Comp_info == null)
		{
			m_cinfo.Comp_info = jpeg_component_info.createArrayOfComponents(m_cinfo.m_num_components);
		}
		for (int i = 0; i < m_cinfo.m_num_components; i++)
		{
			if (!m_cinfo.m_src.GetByte(out var V3))
			{
				return false;
			}
			int num = 0;
			jpeg_component_info jpeg_component_info = null;
			int num2 = 0;
			while (num2 < i)
			{
				jpeg_component_info = m_cinfo.Comp_info[num];
				if (V3 == jpeg_component_info.Component_id)
				{
					num = 0;
					jpeg_component_info = m_cinfo.Comp_info[num];
					V3 = jpeg_component_info.Component_id;
					num++;
					jpeg_component_info = m_cinfo.Comp_info[num];
					num2 = 1;
					while (num2 < i)
					{
						jpeg_component_info = m_cinfo.Comp_info[num];
						if (jpeg_component_info.Component_id > V3)
						{
							V3 = jpeg_component_info.Component_id;
						}
						num2++;
						num++;
					}
					V3++;
					break;
				}
				num2++;
				num++;
			}
			jpeg_component_info = m_cinfo.Comp_info[num];
			jpeg_component_info.Component_id = V3;
			jpeg_component_info.Component_index = i;
			if (!m_cinfo.m_src.GetByte(out V3))
			{
				return false;
			}
			jpeg_component_info.H_samp_factor = (V3 >> 4) & 0xF;
			jpeg_component_info.V_samp_factor = V3 & 0xF;
			if (!m_cinfo.m_src.GetByte(out var V4))
			{
				return false;
			}
			jpeg_component_info.Quant_tbl_no = V4;
			m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_SOF_COMPONENT, jpeg_component_info.Component_id, jpeg_component_info.H_samp_factor, jpeg_component_info.V_samp_factor, jpeg_component_info.Quant_tbl_no);
		}
		m_cinfo.m_marker.m_saw_SOF = true;
		return true;
	}

	private bool get_sos()
	{
		if (!m_cinfo.m_marker.m_saw_SOF)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_SOF_BEFORE, "SOS");
		}
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		if (!m_cinfo.m_src.GetByte(out var V2))
		{
			return false;
		}
		m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_SOS, V2);
		if (V != V2 * 2 + 6 || V2 > 4 || (V2 == 0 && m_cinfo.m_progressive_mode))
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		m_cinfo.m_comps_in_scan = V2;
		for (int i = 0; i < V2; i++)
		{
			if (!m_cinfo.m_src.GetByte(out var V3))
			{
				return false;
			}
			for (int j = 0; j < i; j++)
			{
				jpeg_component_info jpeg_component_info = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[j]];
				if (V3 != jpeg_component_info.Component_id)
				{
					continue;
				}
				jpeg_component_info = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[0]];
				V3 = jpeg_component_info.Component_id;
				for (j = 1; j < i; j++)
				{
					jpeg_component_info = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[j]];
					if (jpeg_component_info.Component_id > V3)
					{
						V3 = jpeg_component_info.Component_id;
					}
				}
				V3++;
				break;
			}
			bool flag = false;
			int num = -1;
			for (int k = 0; k < m_cinfo.m_num_components; k++)
			{
				if (V3 == m_cinfo.Comp_info[k].Component_id)
				{
					num = k;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_COMPONENT_ID, V3);
			}
			m_cinfo.m_cur_comp_info[i] = num;
			if (!m_cinfo.m_src.GetByte(out V3))
			{
				return false;
			}
			m_cinfo.Comp_info[num].Dc_tbl_no = (V3 >> 4) & 0xF;
			m_cinfo.Comp_info[num].Ac_tbl_no = V3 & 0xF;
			m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_SOS_COMPONENT, m_cinfo.Comp_info[num].Component_id, m_cinfo.Comp_info[num].Dc_tbl_no, m_cinfo.Comp_info[num].Ac_tbl_no);
		}
		if (!m_cinfo.m_src.GetByte(out var V4))
		{
			return false;
		}
		m_cinfo.m_Ss = V4;
		if (!m_cinfo.m_src.GetByte(out V4))
		{
			return false;
		}
		m_cinfo.m_Se = V4;
		if (!m_cinfo.m_src.GetByte(out V4))
		{
			return false;
		}
		m_cinfo.m_Ah = (V4 >> 4) & 0xF;
		m_cinfo.m_Al = V4 & 0xF;
		m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_SOS_PARAMS, m_cinfo.m_Ss, m_cinfo.m_Se, m_cinfo.m_Ah, m_cinfo.m_Al);
		m_cinfo.m_marker.m_next_restart_num = 0;
		if (V2 != 0)
		{
			m_cinfo.m_input_scan_number++;
		}
		return true;
	}

	private bool get_dac()
	{
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		V -= 2;
		while (V > 0)
		{
			if (!m_cinfo.m_src.GetByte(out var V2))
			{
				return false;
			}
			if (!m_cinfo.m_src.GetByte(out var V3))
			{
				return false;
			}
			V -= 2;
			m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_DAC, V2, V3);
			if (V2 < 0 || V2 >= 32)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_DAC_INDEX, V2);
			}
			if (V2 >= 16)
			{
				m_cinfo.arith_ac_K[V2 - 16] = (byte)V3;
				continue;
			}
			m_cinfo.arith_dc_L[V2] = (byte)((uint)V3 & 0xFu);
			m_cinfo.arith_dc_U[V2] = (byte)(V3 >> 4);
			if (m_cinfo.arith_dc_L[V2] > m_cinfo.arith_dc_U[V2])
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_DAC_VALUE, V3);
			}
		}
		if (V != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		return true;
	}

	private bool get_dht()
	{
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		V -= 2;
		byte[] array = new byte[17];
		byte[] array2 = new byte[256];
		while (V > 16)
		{
			if (!m_cinfo.m_src.GetByte(out var V2))
			{
				return false;
			}
			m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_DHT, V2);
			array[0] = 0;
			int num = 0;
			for (int i = 1; i <= 16; i++)
			{
				int V3 = 0;
				if (!m_cinfo.m_src.GetByte(out V3))
				{
					return false;
				}
				array[i] = (byte)V3;
				num += array[i];
			}
			V -= 17;
			m_cinfo.TRACEMS(2, J_MESSAGE_CODE.JTRC_HUFFBITS, array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8]);
			m_cinfo.TRACEMS(2, J_MESSAGE_CODE.JTRC_HUFFBITS, array[9], array[10], array[11], array[12], array[13], array[14], array[15], array[16]);
			if (num > 256 || num > V)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_HUFF_TABLE);
			}
			for (int j = 0; j < num; j++)
			{
				int V4 = 0;
				if (!m_cinfo.m_src.GetByte(out V4))
				{
					return false;
				}
				array2[j] = (byte)V4;
			}
			V -= num;
			JHUFF_TBL jHUFF_TBL = null;
			if (((uint)V2 & 0x10u) != 0)
			{
				V2 -= 16;
				if (m_cinfo.m_ac_huff_tbl_ptrs[V2] == null)
				{
					m_cinfo.m_ac_huff_tbl_ptrs[V2] = new JHUFF_TBL();
				}
				jHUFF_TBL = m_cinfo.m_ac_huff_tbl_ptrs[V2];
			}
			else
			{
				if (m_cinfo.m_dc_huff_tbl_ptrs[V2] == null)
				{
					m_cinfo.m_dc_huff_tbl_ptrs[V2] = new JHUFF_TBL();
				}
				jHUFF_TBL = m_cinfo.m_dc_huff_tbl_ptrs[V2];
			}
			if (V2 < 0 || V2 >= 4)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_DHT_INDEX, V2);
			}
			Buffer.BlockCopy(array, 0, jHUFF_TBL.Bits, 0, jHUFF_TBL.Bits.Length);
			Buffer.BlockCopy(array2, 0, jHUFF_TBL.Huffval, 0, jHUFF_TBL.Huffval.Length);
		}
		if (V != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		return true;
	}

	private bool get_dqt()
	{
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		V -= 2;
		while (V > 0)
		{
			V--;
			if (!m_cinfo.m_src.GetByte(out var V2))
			{
				return false;
			}
			int num = V2 >> 4;
			V2 &= 0xF;
			m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_DQT, V2, num);
			if (V2 >= 4)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_DQT_INDEX, V2);
			}
			if (m_cinfo.m_quant_tbl_ptrs[V2] == null)
			{
				m_cinfo.m_quant_tbl_ptrs[V2] = new JQUANT_TBL();
			}
			JQUANT_TBL jQUANT_TBL = m_cinfo.m_quant_tbl_ptrs[V2];
			int num2;
			if (num != 0)
			{
				if (V < 128)
				{
					for (int i = 0; i < 64; i++)
					{
						jQUANT_TBL.quantval[i] = 1;
					}
					num2 = V >> 1;
				}
				else
				{
					num2 = 64;
				}
			}
			else if (V < 64)
			{
				for (int j = 0; j < 64; j++)
				{
					jQUANT_TBL.quantval[j] = 1;
				}
				num2 = V;
			}
			else
			{
				num2 = 64;
			}
			int[] array = num2 switch
			{
				4 => JpegUtils.jpeg_natural_order2, 
				9 => JpegUtils.jpeg_natural_order3, 
				16 => JpegUtils.jpeg_natural_order4, 
				25 => JpegUtils.jpeg_natural_order5, 
				36 => JpegUtils.jpeg_natural_order6, 
				49 => JpegUtils.jpeg_natural_order7, 
				_ => JpegUtils.jpeg_natural_order, 
			};
			for (int k = 0; k < num2; k++)
			{
				int num3;
				if (num != 0)
				{
					int V3 = 0;
					if (!m_cinfo.m_src.GetTwoBytes(out V3))
					{
						return false;
					}
					num3 = V3;
				}
				else
				{
					int V4 = 0;
					if (!m_cinfo.m_src.GetByte(out V4))
					{
						return false;
					}
					num3 = V4;
				}
				jQUANT_TBL.quantval[array[k]] = (short)num3;
			}
			if (m_cinfo.m_err.m_trace_level >= 2)
			{
				for (int l = 0; l < 64; l += 8)
				{
					m_cinfo.TRACEMS(2, J_MESSAGE_CODE.JTRC_QUANTVALS, jQUANT_TBL.quantval[l], jQUANT_TBL.quantval[l + 1], jQUANT_TBL.quantval[l + 2], jQUANT_TBL.quantval[l + 3], jQUANT_TBL.quantval[l + 4], jQUANT_TBL.quantval[l + 5], jQUANT_TBL.quantval[l + 6], jQUANT_TBL.quantval[l + 7]);
				}
			}
			V -= num2;
			if (num != 0)
			{
				V -= num2;
			}
		}
		if (V != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		return true;
	}

	private bool get_dri()
	{
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		if (V != 4)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		int V2 = 0;
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		int num = V2;
		m_cinfo.TRACEMS(1, J_MESSAGE_CODE.JTRC_DRI, num);
		m_cinfo.m_restart_interval = num;
		return true;
	}

	private bool get_lse()
	{
		if (!m_cinfo.m_marker.m_saw_SOF)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_SOF_BEFORE, "LSE");
		}
		if (m_cinfo.m_num_components < 3)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out var V))
		{
			return false;
		}
		if (V != 24)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);
		}
		if (!m_cinfo.m_src.GetByte(out var V2))
		{
			return false;
		}
		if (V2 != 13)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_UNKNOWN_MARKER, m_cinfo.m_unread_marker);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 255)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out V2))
		{
			return false;
		}
		if (V2 != 3)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out var V3))
		{
			return false;
		}
		if (V3 != m_cinfo.Comp_info[1].Component_id)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out V3))
		{
			return false;
		}
		if (V3 != m_cinfo.Comp_info[0].Component_id)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out V3))
		{
			return false;
		}
		if (V3 != m_cinfo.Comp_info[2].Component_id)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out V2))
		{
			return false;
		}
		if (V2 != 128)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out V2))
		{
			return false;
		}
		if (V2 != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 1)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetByte(out V2))
		{
			return false;
		}
		if (V2 != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 1)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		if (!m_cinfo.m_src.GetTwoBytes(out V2))
		{
			return false;
		}
		if (V2 != 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
		}
		m_cinfo.color_transform = J_COLOR_TRANSFORM.JCT_SUBTRACT_GREEN;
		return true;
	}

	private bool first_marker()
	{
		if (!m_cinfo.m_src.GetByte(out var V))
		{
			return false;
		}
		if (!m_cinfo.m_src.GetByte(out var V2))
		{
			return false;
		}
		if (V != 255 || V2 != 216)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NO_SOI, V, V2);
		}
		m_cinfo.m_unread_marker = V2;
		return true;
	}
}
