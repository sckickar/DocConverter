namespace BitMiracle.LibJpeg.Classic.Internal;

internal class jpeg_decomp_master
{
	private jpeg_decompress_struct m_cinfo;

	private int m_pass_number;

	private bool m_is_dummy_pass;

	private bool m_using_merged_upsample;

	private jpeg_color_quantizer m_quantizer_1pass;

	private jpeg_color_quantizer m_quantizer_2pass;

	public jpeg_decomp_master(jpeg_decompress_struct cinfo)
	{
		m_cinfo = cinfo;
		master_selection();
	}

	public void prepare_for_output_pass()
	{
		if (m_is_dummy_pass)
		{
			m_is_dummy_pass = false;
			m_cinfo.m_cquantize.start_pass(is_pre_scan: false);
			m_cinfo.m_post.start_pass(J_BUF_MODE.JBUF_CRANK_DEST);
			m_cinfo.m_main.start_pass(J_BUF_MODE.JBUF_CRANK_DEST);
		}
		else
		{
			if (m_cinfo.m_quantize_colors && m_cinfo.m_colormap == null)
			{
				if (m_cinfo.m_two_pass_quantize && m_cinfo.m_enable_2pass_quant)
				{
					m_cinfo.m_cquantize = m_quantizer_2pass;
					m_is_dummy_pass = true;
				}
				else if (m_cinfo.m_enable_1pass_quant)
				{
					m_cinfo.m_cquantize = m_quantizer_1pass;
				}
				else
				{
					m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_MODE_CHANGE);
				}
			}
			m_cinfo.m_idct.start_pass();
			m_cinfo.m_coef.start_output_pass();
			if (!m_cinfo.m_raw_data_out)
			{
				m_cinfo.m_upsample.start_pass();
				if (m_cinfo.m_quantize_colors)
				{
					m_cinfo.m_cquantize.start_pass(m_is_dummy_pass);
				}
				m_cinfo.m_post.start_pass(m_is_dummy_pass ? J_BUF_MODE.JBUF_SAVE_AND_PASS : J_BUF_MODE.JBUF_PASS_THRU);
				m_cinfo.m_main.start_pass(J_BUF_MODE.JBUF_PASS_THRU);
			}
		}
		if (m_cinfo.m_progress != null)
		{
			m_cinfo.m_progress.Completed_passes = m_pass_number;
			m_cinfo.m_progress.Total_passes = m_pass_number + ((!m_is_dummy_pass) ? 1 : 2);
			if (m_cinfo.m_buffered_image && !m_cinfo.m_inputctl.EOIReached())
			{
				m_cinfo.m_progress.Total_passes += ((!m_cinfo.m_enable_2pass_quant) ? 1 : 2);
			}
		}
	}

	public void finish_output_pass()
	{
		if (m_cinfo.m_quantize_colors)
		{
			m_cinfo.m_cquantize.finish_pass();
		}
		m_pass_number++;
	}

	public bool IsDummyPass()
	{
		return m_is_dummy_pass;
	}

	private void master_selection()
	{
		if (m_cinfo.m_data_precision != 8)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_PRECISION, m_cinfo.m_data_precision);
		}
		m_cinfo.jpeg_calc_output_dimensions();
		prepare_range_limit_table();
		if (m_cinfo.m_output_height <= 0 || m_cinfo.m_output_width <= 0 || m_cinfo.m_out_color_components <= 0)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_EMPTY_IMAGE);
		}
		long num = m_cinfo.m_output_width * m_cinfo.m_out_color_components;
		if ((int)num != num)
		{
			m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_WIDTH_OVERFLOW);
		}
		m_pass_number = 0;
		m_using_merged_upsample = m_cinfo.use_merged_upsample();
		m_quantizer_1pass = null;
		m_quantizer_2pass = null;
		if (!m_cinfo.m_quantize_colors || !m_cinfo.m_buffered_image)
		{
			m_cinfo.m_enable_1pass_quant = false;
			m_cinfo.m_enable_external_quant = false;
			m_cinfo.m_enable_2pass_quant = false;
		}
		if (m_cinfo.m_quantize_colors)
		{
			if (m_cinfo.m_raw_data_out)
			{
				m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOTIMPL);
			}
			if (m_cinfo.m_out_color_components != 3)
			{
				m_cinfo.m_enable_1pass_quant = true;
				m_cinfo.m_enable_external_quant = false;
				m_cinfo.m_enable_2pass_quant = false;
				m_cinfo.m_colormap = null;
			}
			else if (m_cinfo.m_colormap != null)
			{
				m_cinfo.m_enable_external_quant = true;
			}
			else if (m_cinfo.m_two_pass_quantize)
			{
				m_cinfo.m_enable_2pass_quant = true;
			}
			else
			{
				m_cinfo.m_enable_1pass_quant = true;
			}
			if (m_cinfo.m_enable_1pass_quant)
			{
				m_cinfo.m_cquantize = new my_1pass_cquantizer(m_cinfo);
				m_quantizer_1pass = m_cinfo.m_cquantize;
			}
			if (m_cinfo.m_enable_2pass_quant || m_cinfo.m_enable_external_quant)
			{
				m_cinfo.m_cquantize = new my_2pass_cquantizer(m_cinfo);
				m_quantizer_2pass = m_cinfo.m_cquantize;
			}
		}
		if (!m_cinfo.m_raw_data_out)
		{
			if (m_using_merged_upsample)
			{
				m_cinfo.m_upsample = new my_merged_upsampler(m_cinfo);
			}
			else
			{
				m_cinfo.m_cconvert = new jpeg_color_deconverter(m_cinfo);
				m_cinfo.m_upsample = new my_upsampler(m_cinfo);
			}
			m_cinfo.m_post = new jpeg_d_post_controller(m_cinfo, m_cinfo.m_enable_2pass_quant);
		}
		m_cinfo.m_idct = new jpeg_inverse_dct(m_cinfo);
		if (m_cinfo.arith_code)
		{
			m_cinfo.m_entropy = new arith_entropy_decoder(m_cinfo);
		}
		else
		{
			m_cinfo.m_entropy = new huff_entropy_decoder(m_cinfo);
		}
		bool need_full_buffer = m_cinfo.m_inputctl.HasMultipleScans() || m_cinfo.m_buffered_image;
		m_cinfo.m_coef = new jpeg_d_coef_controller(m_cinfo, need_full_buffer);
		if (!m_cinfo.m_raw_data_out)
		{
			m_cinfo.m_main = new jpeg_d_main_controller(m_cinfo);
		}
		m_cinfo.m_inputctl.start_input_pass();
		if (m_cinfo.m_progress != null && !m_cinfo.m_buffered_image && m_cinfo.m_inputctl.HasMultipleScans())
		{
			int num2 = ((!m_cinfo.m_progressive_mode) ? m_cinfo.m_num_components : (2 + 3 * m_cinfo.m_num_components));
			m_cinfo.m_progress.Pass_counter = 0;
			m_cinfo.m_progress.Pass_limit = m_cinfo.m_total_iMCU_rows * num2;
			m_cinfo.m_progress.Completed_passes = 0;
			m_cinfo.m_progress.Total_passes = (m_cinfo.m_enable_2pass_quant ? 3 : 2);
			m_pass_number++;
		}
	}

	private void prepare_range_limit_table()
	{
		byte[] array = new byte[1280];
		int num = 512;
		m_cinfo.m_sample_range_limit = array;
		m_cinfo.m_sampleRangeLimitOffset = num;
		int i;
		for (i = 0; i <= 255; i++)
		{
			array[num + i] = (byte)i;
		}
		for (; i < 768; i++)
		{
			array[num + i] = byte.MaxValue;
		}
	}
}
