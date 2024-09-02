using System;
using System.IO;
using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class OJpegCodec : TiffCodec
{
	private enum OJPEGStateInBufferSource
	{
		osibsNotSetYet,
		osibsJpegInterchangeFormat,
		osibsStrile,
		osibsEof
	}

	private enum OJPEGStateOutState
	{
		ososSoi,
		ososQTable0,
		ososQTable1,
		ososQTable2,
		ososQTable3,
		ososDcTable0,
		ososDcTable1,
		ososDcTable2,
		ososDcTable3,
		ososAcTable0,
		ososAcTable1,
		ososAcTable2,
		ososAcTable3,
		ososDri,
		ososSof,
		ososSos,
		ososCompressed,
		ososRst,
		ososEoi
	}

	private struct SosEnd
	{
		public bool m_log;

		public OJPEGStateInBufferSource m_in_buffer_source;

		public uint m_in_buffer_next_strile;

		public uint m_in_buffer_file_pos;

		public uint m_in_buffer_file_togo;
	}

	internal const int FIELD_OJPEG_JPEGINTERCHANGEFORMAT = 66;

	internal const int FIELD_OJPEG_JPEGINTERCHANGEFORMATLENGTH = 67;

	internal const int FIELD_OJPEG_JPEGQTABLES = 68;

	internal const int FIELD_OJPEG_JPEGDCTABLES = 69;

	internal const int FIELD_OJPEG_JPEGACTABLES = 70;

	internal const int FIELD_OJPEG_JPEGPROC = 71;

	internal const int FIELD_OJPEG_JPEGRESTARTINTERVAL = 72;

	internal const int FIELD_OJPEG_COUNT = 7;

	private const int OJPEG_BUFFER = 2048;

	private static readonly TiffFieldInfo[] ojpeg_field_info = new TiffFieldInfo[7]
	{
		new TiffFieldInfo(TiffTag.JPEGIFOFFSET, 1, 1, TiffType.LONG, 66, okToChange: true, passCount: false, "JpegInterchangeFormat"),
		new TiffFieldInfo(TiffTag.JPEGIFBYTECOUNT, 1, 1, TiffType.LONG, 67, okToChange: true, passCount: false, "JpegInterchangeFormatLength"),
		new TiffFieldInfo(TiffTag.JPEGQTABLES, -1, -1, TiffType.LONG, 68, okToChange: false, passCount: true, "JpegQTables"),
		new TiffFieldInfo(TiffTag.JPEGDCTABLES, -1, -1, TiffType.LONG, 69, okToChange: false, passCount: true, "JpegDcTables"),
		new TiffFieldInfo(TiffTag.JPEGACTABLES, -1, -1, TiffType.LONG, 70, okToChange: false, passCount: true, "JpegAcTables"),
		new TiffFieldInfo(TiffTag.JPEGPROC, 1, 1, TiffType.SHORT, 71, okToChange: false, passCount: false, "JpegProc"),
		new TiffFieldInfo(TiffTag.JPEGRESTARTINTERVAL, 1, 1, TiffType.SHORT, 72, okToChange: false, passCount: false, "JpegRestartInterval")
	};

	internal uint m_jpeg_interchange_format;

	internal uint m_jpeg_interchange_format_length;

	internal byte m_jpeg_proc;

	internal bool m_subsamplingcorrect_done;

	internal bool m_subsampling_tag;

	internal byte m_subsampling_hor;

	internal byte m_subsampling_ver;

	internal byte m_qtable_offset_count;

	internal byte m_dctable_offset_count;

	internal byte m_actable_offset_count;

	internal uint[] m_qtable_offset = new uint[3];

	internal uint[] m_dctable_offset = new uint[3];

	internal uint[] m_actable_offset = new uint[3];

	internal ushort m_restart_interval;

	internal jpeg_decompress_struct m_libjpeg_jpeg_decompress_struct;

	private TiffTagMethods m_tagMethods;

	private TiffTagMethods m_parentTagMethods;

	private uint m_file_size;

	private uint m_image_width;

	private uint m_image_length;

	private uint m_strile_width;

	private uint m_strile_length;

	private uint m_strile_length_total;

	private byte m_samples_per_pixel;

	private byte m_plane_sample_offset;

	private byte m_samples_per_pixel_per_plane;

	private bool m_subsamplingcorrect;

	private bool m_subsampling_force_desubsampling_inside_decompression;

	private byte[][] m_qtable = new byte[4][];

	private byte[][] m_dctable = new byte[4][];

	private byte[][] m_actable = new byte[4][];

	private byte m_restart_index;

	private bool m_sof_log;

	private byte m_sof_marker_id;

	private uint m_sof_x;

	private uint m_sof_y;

	private byte[] m_sof_c = new byte[3];

	private byte[] m_sof_hv = new byte[3];

	private byte[] m_sof_tq = new byte[3];

	private byte[] m_sos_cs = new byte[3];

	private byte[] m_sos_tda = new byte[3];

	private SosEnd[] m_sos_end = new SosEnd[3];

	private bool m_readheader_done;

	private bool m_writeheader_done;

	private short m_write_cursample;

	private uint m_write_curstrile;

	private bool m_libjpeg_session_active;

	private byte m_libjpeg_jpeg_query_style;

	private jpeg_error_mgr m_libjpeg_jpeg_error_mgr;

	private jpeg_source_mgr m_libjpeg_jpeg_source_mgr;

	private bool m_subsampling_convert_log;

	private uint m_subsampling_convert_ylinelen;

	private uint m_subsampling_convert_ylines;

	private uint m_subsampling_convert_clinelen;

	private uint m_subsampling_convert_clines;

	private byte[][] m_subsampling_convert_ybuf;

	private byte[][] m_subsampling_convert_cbbuf;

	private byte[][] m_subsampling_convert_crbuf;

	private byte[][][] m_subsampling_convert_ycbcrimage;

	private uint m_subsampling_convert_clinelenout;

	private uint m_subsampling_convert_state;

	private uint m_bytes_per_line;

	private uint m_lines_per_strile;

	private OJPEGStateInBufferSource m_in_buffer_source;

	private uint m_in_buffer_next_strile;

	private uint m_in_buffer_strile_count;

	private uint m_in_buffer_file_pos;

	private bool m_in_buffer_file_pos_log;

	private uint m_in_buffer_file_togo;

	private ushort m_in_buffer_togo;

	private int m_in_buffer_cur;

	private byte[] m_in_buffer = new byte[2048];

	private OJPEGStateOutState m_out_state;

	private byte[] m_out_buffer = new byte[2048];

	private byte[] m_skip_buffer;

	private bool m_forceProcessedRgbOutput;

	public override bool CanEncode => false;

	public override bool CanDecode => true;

	public OJpegCodec(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
		m_tagMethods = new OJpegCodecTagMethods();
	}

	private void cleanState()
	{
		m_jpeg_interchange_format = 0u;
		m_jpeg_interchange_format_length = 0u;
		m_jpeg_proc = 0;
		m_subsamplingcorrect_done = false;
		m_subsampling_tag = false;
		m_subsampling_hor = 0;
		m_subsampling_ver = 0;
		m_qtable_offset_count = 0;
		m_dctable_offset_count = 0;
		m_actable_offset_count = 0;
		m_qtable_offset = new uint[3];
		m_dctable_offset = new uint[3];
		m_actable_offset = new uint[3];
		m_restart_interval = 0;
		m_libjpeg_jpeg_decompress_struct = null;
		m_file_size = 0u;
		m_image_width = 0u;
		m_image_length = 0u;
		m_strile_width = 0u;
		m_strile_length = 0u;
		m_strile_length_total = 0u;
		m_samples_per_pixel = 0;
		m_plane_sample_offset = 0;
		m_samples_per_pixel_per_plane = 0;
		m_subsamplingcorrect = false;
		m_subsampling_force_desubsampling_inside_decompression = false;
		m_qtable = new byte[4][];
		m_dctable = new byte[4][];
		m_actable = new byte[4][];
		m_restart_index = 0;
		m_sof_log = false;
		m_sof_marker_id = 0;
		m_sof_x = 0u;
		m_sof_y = 0u;
		m_sof_c = new byte[3];
		m_sof_hv = new byte[3];
		m_sof_tq = new byte[3];
		m_sos_cs = new byte[3];
		m_sos_tda = new byte[3];
		m_sos_end = new SosEnd[3];
		m_readheader_done = false;
		m_writeheader_done = false;
		m_write_cursample = 0;
		m_write_curstrile = 0u;
		m_libjpeg_session_active = false;
		m_libjpeg_jpeg_query_style = 0;
		m_libjpeg_jpeg_error_mgr = null;
		m_libjpeg_jpeg_source_mgr = null;
		m_subsampling_convert_log = false;
		m_subsampling_convert_ylinelen = 0u;
		m_subsampling_convert_ylines = 0u;
		m_subsampling_convert_clinelen = 0u;
		m_subsampling_convert_clines = 0u;
		m_subsampling_convert_ybuf = null;
		m_subsampling_convert_cbbuf = null;
		m_subsampling_convert_crbuf = null;
		m_subsampling_convert_ycbcrimage = null;
		m_subsampling_convert_clinelenout = 0u;
		m_subsampling_convert_state = 0u;
		m_bytes_per_line = 0u;
		m_lines_per_strile = 0u;
		m_in_buffer_source = OJPEGStateInBufferSource.osibsNotSetYet;
		m_in_buffer_next_strile = 0u;
		m_in_buffer_strile_count = 0u;
		m_in_buffer_file_pos = 0u;
		m_in_buffer_file_pos_log = false;
		m_in_buffer_file_togo = 0u;
		m_in_buffer_togo = 0;
		m_in_buffer_cur = 0;
		m_in_buffer = new byte[2048];
		m_out_state = OJPEGStateOutState.ososSoi;
		m_out_buffer = new byte[2048];
		m_skip_buffer = null;
		m_forceProcessedRgbOutput = false;
	}

	public override bool Init()
	{
		m_tif.MergeFieldInfo(ojpeg_field_info, ojpeg_field_info.Length);
		cleanState();
		m_jpeg_proc = 1;
		m_subsampling_hor = 2;
		m_subsampling_ver = 2;
		m_tif.SetField(TiffTag.YCBCRSUBSAMPLING, 2, 2);
		m_parentTagMethods = m_tif.m_tagmethods;
		m_tif.m_tagmethods = m_tagMethods;
		m_tif.m_flags |= TiffFlags.NOREADRAW;
		return true;
	}

	public Tiff GetTiff()
	{
		return m_tif;
	}

	public override bool SetupDecode()
	{
		return OJPEGSetupDecode();
	}

	public override bool PreDecode(short plane)
	{
		return OJPEGPreDecode(plane);
	}

	public override bool DecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return OJPEGDecode(buffer, offset, count, plane);
	}

	public override bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return OJPEGDecode(buffer, offset, count, plane);
	}

	public override bool DecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return OJPEGDecode(buffer, offset, count, plane);
	}

	public override bool SetupEncode()
	{
		return OJpegEncodeIsUnsupported();
	}

	public override bool PreEncode(short plane)
	{
		return OJpegEncodeIsUnsupported();
	}

	public override bool PostEncode()
	{
		return OJpegEncodeIsUnsupported();
	}

	public override bool EncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return OJpegEncodeIsUnsupported();
	}

	public override bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return OJpegEncodeIsUnsupported();
	}

	public override bool EncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return OJpegEncodeIsUnsupported();
	}

	public override void Cleanup()
	{
		OJPEGCleanup();
	}

	internal void ForceProcessedRgbOutput(bool force)
	{
		m_forceProcessedRgbOutput = force;
		m_subsamplingcorrect_done = false;
	}

	private bool OJPEGSetupDecode()
	{
		Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSetupDecode", "Depreciated and troublesome old-style JPEG compression mode, please convert to new-style JPEG compression and notify vendor of writing software");
		return true;
	}

	private bool OJPEGPreDecode(short s)
	{
		if (!m_subsamplingcorrect_done)
		{
			OJPEGSubsamplingCorrect();
		}
		if (!m_readheader_done && !OJPEGReadHeaderInfo())
		{
			return false;
		}
		if (!m_sos_end[s].m_log && !OJPEGReadSecondarySos(s))
		{
			return false;
		}
		uint num = (uint)((!m_tif.IsTiled()) ? m_tif.m_curstrip : m_tif.m_curtile);
		if (m_writeheader_done && (m_write_cursample != s || m_write_curstrile > num))
		{
			if (m_libjpeg_session_active)
			{
				OJPEGLibjpegSessionAbort();
			}
			m_writeheader_done = false;
		}
		if (!m_writeheader_done)
		{
			m_plane_sample_offset = (byte)s;
			m_write_cursample = s;
			m_write_curstrile = (uint)(s * m_tif.m_dir.td_stripsperimage);
			if (!m_in_buffer_file_pos_log || m_in_buffer_file_pos - m_in_buffer_togo != m_sos_end[s].m_in_buffer_file_pos)
			{
				m_in_buffer_source = m_sos_end[s].m_in_buffer_source;
				m_in_buffer_next_strile = m_sos_end[s].m_in_buffer_next_strile;
				m_in_buffer_file_pos = m_sos_end[s].m_in_buffer_file_pos;
				m_in_buffer_file_pos_log = false;
				m_in_buffer_file_togo = m_sos_end[s].m_in_buffer_file_togo;
				m_in_buffer_togo = 0;
				m_in_buffer_cur = 0;
			}
			if (!OJPEGWriteHeaderInfo())
			{
				return false;
			}
		}
		while (m_write_curstrile < num)
		{
			if (m_libjpeg_jpeg_query_style == 0)
			{
				if (!OJPEGPreDecodeSkipRaw())
				{
					return false;
				}
			}
			else if (!OJPEGPreDecodeSkipScanlines())
			{
				return false;
			}
			m_write_curstrile++;
		}
		return true;
	}

	private bool OJPEGDecode(byte[] buf, int offset, int cc, short s)
	{
		if (m_libjpeg_jpeg_query_style == 0)
		{
			if (!OJPEGDecodeRaw(buf, offset, cc))
			{
				return false;
			}
		}
		else if (!OJPEGDecodeScanlines(buf, offset, cc))
		{
			return false;
		}
		return true;
	}

	private bool OJpegEncodeIsUnsupported()
	{
		Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGSetupEncode", "OJPEG encoding not supported; use new-style JPEG compression instead");
		return false;
	}

	private void OJPEGCleanup()
	{
		m_tif.m_tagmethods = m_parentTagMethods;
		if (m_libjpeg_session_active)
		{
			OJPEGLibjpegSessionAbort();
		}
	}

	private bool OJPEGPreDecodeSkipRaw()
	{
		uint num = m_lines_per_strile;
		if (m_subsampling_convert_state != 0)
		{
			if (m_subsampling_convert_clines - m_subsampling_convert_state >= num)
			{
				m_subsampling_convert_state += num;
				if (m_subsampling_convert_state == m_subsampling_convert_clines)
				{
					m_subsampling_convert_state = 0u;
				}
				return true;
			}
			num -= m_subsampling_convert_clines - m_subsampling_convert_state;
			m_subsampling_convert_state = 0u;
		}
		while (num >= m_subsampling_convert_clines)
		{
			if (jpeg_read_raw_data_encap(m_subsampling_ver * 8) == 0)
			{
				return false;
			}
			num -= m_subsampling_convert_clines;
		}
		if (num != 0)
		{
			if (jpeg_read_raw_data_encap(m_subsampling_ver * 8) == 0)
			{
				return false;
			}
			m_subsampling_convert_state = num;
		}
		return true;
	}

	private bool OJPEGPreDecodeSkipScanlines()
	{
		if (m_skip_buffer == null)
		{
			m_skip_buffer = new byte[m_bytes_per_line];
		}
		for (uint num = 0u; num < m_lines_per_strile; num++)
		{
			if (jpeg_read_scanlines_encap(m_skip_buffer, 1) == 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool OJPEGDecodeRaw(byte[] buf, int offset, int cc)
	{
		if (cc % m_bytes_per_line != 0L)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGDecodeRaw", "Fractional scanline not read");
			return false;
		}
		int num = offset;
		int num2 = cc;
		do
		{
			if (m_subsampling_convert_state == 0 && jpeg_read_raw_data_encap(m_subsampling_ver * 8) == 0)
			{
				return false;
			}
			uint num3 = m_subsampling_convert_state * m_subsampling_ver * m_subsampling_convert_ylinelen;
			uint num4 = m_subsampling_convert_state * m_subsampling_convert_clinelen;
			uint num5 = m_subsampling_convert_state * m_subsampling_convert_clinelen;
			int num6 = 0;
			int num7 = 0;
			int num8 = num;
			for (uint num9 = 0u; num9 < m_subsampling_convert_clinelenout; num9++)
			{
				uint num10 = num3;
				for (byte b = 0; b < m_subsampling_ver; b++)
				{
					for (byte b2 = 0; b2 < m_subsampling_hor; b2++)
					{
						num6 = (int)(num10 / m_subsampling_convert_ylinelen);
						num7 = (int)(num10 % m_subsampling_convert_ylinelen);
						num10++;
						buf[num8++] = m_subsampling_convert_ybuf[num6][num7];
					}
					num10 += m_subsampling_convert_ylinelen - m_subsampling_hor;
				}
				num3 += m_subsampling_hor;
				num6 = (int)(num4 / m_subsampling_convert_clinelen);
				num7 = (int)(num4 % m_subsampling_convert_clinelen);
				num4++;
				buf[num8++] = m_subsampling_convert_cbbuf[num6][num7];
				num6 = (int)(num5 / m_subsampling_convert_clinelen);
				num7 = (int)(num5 % m_subsampling_convert_clinelen);
				num5++;
				buf[num8++] = m_subsampling_convert_crbuf[num6][num7];
			}
			m_subsampling_convert_state++;
			if (m_subsampling_convert_state == m_subsampling_convert_clines)
			{
				m_subsampling_convert_state = 0u;
			}
			num += (int)m_bytes_per_line;
			num2 -= (int)m_bytes_per_line;
		}
		while (num2 > 0);
		return true;
	}

	private bool OJPEGDecodeScanlines(byte[] buf, int offset, int cc)
	{
		if (cc % m_bytes_per_line != 0L)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGDecodeScanlines", "Fractional scanline not read");
			return false;
		}
		int num = offset;
		byte[] array = new byte[m_bytes_per_line];
		int num2 = cc;
		do
		{
			if (jpeg_read_scanlines_encap(array, 1) == 0)
			{
				return false;
			}
			Buffer.BlockCopy(array, 0, buf, num, array.Length);
			num += (int)m_bytes_per_line;
			num2 -= (int)m_bytes_per_line;
		}
		while (num2 > 0);
		return true;
	}

	public void OJPEGSubsamplingCorrect()
	{
		if (m_tif.m_dir.td_samplesperpixel != 3 || (m_tif.m_dir.td_photometric != Photometric.YCBCR && m_tif.m_dir.td_photometric != Photometric.ITULAB))
		{
			if (m_subsampling_tag)
			{
				Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSubsamplingCorrect", "Subsampling tag not appropriate for this Photometric and/or SamplesPerPixel");
			}
			m_subsampling_hor = 1;
			m_subsampling_ver = 1;
			m_subsampling_force_desubsampling_inside_decompression = false;
		}
		else
		{
			m_subsamplingcorrect_done = true;
			byte subsampling_hor = m_subsampling_hor;
			byte subsampling_ver = m_subsampling_ver;
			m_subsamplingcorrect = true;
			OJPEGReadHeaderInfoSec();
			if (m_subsampling_force_desubsampling_inside_decompression)
			{
				m_subsampling_hor = 1;
				m_subsampling_ver = 1;
			}
			m_subsamplingcorrect = false;
			if ((m_subsampling_hor != subsampling_hor || m_subsampling_ver != subsampling_ver) && !m_subsampling_force_desubsampling_inside_decompression)
			{
				if (!m_subsampling_tag)
				{
					Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSubsamplingCorrect", "Subsampling tag is not set, yet subsampling inside JPEG data [{0},{1}] does not match default values [2,2]; assuming subsampling inside JPEG data is correct", m_subsampling_hor, m_subsampling_ver);
				}
				else
				{
					Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSubsamplingCorrect", "Subsampling inside JPEG data [{0},{1}] does not match subsampling tag values [{2},{3}]; assuming subsampling inside JPEG data is correct", m_subsampling_hor, m_subsampling_ver, subsampling_hor, subsampling_ver);
				}
			}
			if (m_subsampling_force_desubsampling_inside_decompression)
			{
				if (!m_subsampling_tag)
				{
					Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSubsamplingCorrect", "Subsampling tag is not set, yet subsampling inside JPEG data does not match default values [2,2] (nor any other values allowed in TIFF); assuming subsampling inside JPEG data is correct and desubsampling inside JPEG decompression");
				}
				else
				{
					Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSubsamplingCorrect", "Subsampling inside JPEG data does not match subsampling tag values [{0},{1}] (nor any other values allowed in TIFF); assuming subsampling inside JPEG data is correct and desubsampling inside JPEG decompression", subsampling_hor, subsampling_ver);
				}
			}
			if (!m_subsampling_force_desubsampling_inside_decompression && m_subsampling_hor < m_subsampling_ver)
			{
				Tiff.WarningExt(m_tif, m_tif.m_clientdata, "OJPEGSubsamplingCorrect", "Subsampling values [{0},{1}] are not allowed in TIFF", m_subsampling_hor, m_subsampling_ver);
			}
		}
		m_subsamplingcorrect_done = true;
	}

	private bool OJPEGReadHeaderInfo()
	{
		m_image_width = (uint)m_tif.m_dir.td_imagewidth;
		m_image_length = (uint)m_tif.m_dir.td_imagelength;
		if (m_tif.IsTiled())
		{
			m_strile_width = (uint)m_tif.m_dir.td_tilewidth;
			m_strile_length = (uint)m_tif.m_dir.td_tilelength;
			m_strile_length_total = (m_image_length + m_strile_length - 1) / m_strile_length * m_strile_length;
		}
		else
		{
			m_strile_width = m_image_width;
			m_strile_length = (uint)m_tif.m_dir.td_rowsperstrip;
			m_strile_length_total = m_image_length;
		}
		m_samples_per_pixel = (byte)m_tif.m_dir.td_samplesperpixel;
		if (m_samples_per_pixel == 1)
		{
			m_plane_sample_offset = 0;
			m_samples_per_pixel_per_plane = m_samples_per_pixel;
			m_subsampling_hor = 1;
			m_subsampling_ver = 1;
		}
		else
		{
			if (m_samples_per_pixel != 3)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfo", "SamplesPerPixel {0} not supported for this compression scheme", m_samples_per_pixel);
				return false;
			}
			m_plane_sample_offset = 0;
			if (m_tif.m_dir.td_planarconfig == PlanarConfig.CONTIG)
			{
				m_samples_per_pixel_per_plane = 3;
			}
			else
			{
				m_samples_per_pixel_per_plane = 1;
			}
		}
		if (m_strile_length < m_image_length)
		{
			if (m_strile_length % (m_subsampling_ver * 8) != 0L)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfo", "Incompatible vertical subsampling and image strip/tile length");
				return false;
			}
			m_restart_interval = (ushort)((m_strile_width + m_subsampling_hor * 8 - 1) / (m_subsampling_hor * 8) * (m_strile_length / (m_subsampling_ver * 8)));
		}
		if (!OJPEGReadHeaderInfoSec())
		{
			return false;
		}
		m_sos_end[0].m_log = true;
		m_sos_end[0].m_in_buffer_source = m_in_buffer_source;
		m_sos_end[0].m_in_buffer_next_strile = m_in_buffer_next_strile;
		m_sos_end[0].m_in_buffer_file_pos = m_in_buffer_file_pos - m_in_buffer_togo;
		m_sos_end[0].m_in_buffer_file_togo = m_in_buffer_file_togo + m_in_buffer_togo;
		m_readheader_done = true;
		return true;
	}

	private bool OJPEGReadSecondarySos(short s)
	{
		m_plane_sample_offset = (byte)(s - 1);
		while (!m_sos_end[m_plane_sample_offset].m_log)
		{
			m_plane_sample_offset--;
		}
		m_in_buffer_source = m_sos_end[m_plane_sample_offset].m_in_buffer_source;
		m_in_buffer_next_strile = m_sos_end[m_plane_sample_offset].m_in_buffer_next_strile;
		m_in_buffer_file_pos = m_sos_end[m_plane_sample_offset].m_in_buffer_file_pos;
		m_in_buffer_file_pos_log = false;
		m_in_buffer_file_togo = m_sos_end[m_plane_sample_offset].m_in_buffer_file_togo;
		m_in_buffer_togo = 0;
		m_in_buffer_cur = 0;
		while (m_plane_sample_offset < s)
		{
			while (true)
			{
				if (!OJPEGReadByte(out var b))
				{
					return false;
				}
				if (b != byte.MaxValue)
				{
					continue;
				}
				while (true)
				{
					if (!OJPEGReadByte(out b))
					{
						return false;
					}
					switch (b)
					{
					case byte.MaxValue:
						continue;
					case 218:
						m_plane_sample_offset++;
						if (!OJPEGReadHeaderInfoSecStreamSos())
						{
							return false;
						}
						goto end_IL_00bd;
					}
					break;
				}
				continue;
				end_IL_00bd:
				break;
			}
			m_sos_end[m_plane_sample_offset].m_log = true;
			m_sos_end[m_plane_sample_offset].m_in_buffer_source = m_in_buffer_source;
			m_sos_end[m_plane_sample_offset].m_in_buffer_next_strile = m_in_buffer_next_strile;
			m_sos_end[m_plane_sample_offset].m_in_buffer_file_pos = m_in_buffer_file_pos - m_in_buffer_togo;
			m_sos_end[m_plane_sample_offset].m_in_buffer_file_togo = m_in_buffer_file_togo + m_in_buffer_togo;
		}
		return true;
	}

	private bool OJPEGWriteHeaderInfo()
	{
		m_out_state = OJPEGStateOutState.ososSoi;
		m_restart_index = 0;
		m_libjpeg_jpeg_error_mgr = new OJpegErrorManager(this);
		if (!jpeg_create_decompress_encap())
		{
			return false;
		}
		m_libjpeg_session_active = true;
		m_libjpeg_jpeg_source_mgr = new OJpegSrcManager(this);
		m_libjpeg_jpeg_decompress_struct.Src = m_libjpeg_jpeg_source_mgr;
		if (jpeg_read_header_encap(require_image: true) == ReadResult.JPEG_SUSPENDED)
		{
			return false;
		}
		if (!m_subsampling_force_desubsampling_inside_decompression && m_samples_per_pixel_per_plane > 1)
		{
			m_libjpeg_jpeg_decompress_struct.Raw_data_out = true;
			m_libjpeg_jpeg_decompress_struct.Do_fancy_upsampling = false;
			m_libjpeg_jpeg_query_style = 0;
			if (!m_subsampling_convert_log)
			{
				m_subsampling_convert_ylinelen = (uint)((m_strile_width + m_subsampling_hor * 8 - 1) / (m_subsampling_hor * 8) * m_subsampling_hor * 8);
				m_subsampling_convert_ylines = (uint)(m_subsampling_ver * 8);
				m_subsampling_convert_clinelen = m_subsampling_convert_ylinelen / m_subsampling_hor;
				m_subsampling_convert_clines = 8u;
				m_subsampling_convert_ybuf = new byte[m_subsampling_convert_ylines][];
				for (int i = 0; i < m_subsampling_convert_ylines; i++)
				{
					m_subsampling_convert_ybuf[i] = new byte[m_subsampling_convert_ylinelen];
				}
				m_subsampling_convert_cbbuf = new byte[m_subsampling_convert_clines][];
				m_subsampling_convert_crbuf = new byte[m_subsampling_convert_clines][];
				for (int j = 0; j < m_subsampling_convert_clines; j++)
				{
					m_subsampling_convert_cbbuf[j] = new byte[m_subsampling_convert_clinelen];
					m_subsampling_convert_crbuf[j] = new byte[m_subsampling_convert_clinelen];
				}
				m_subsampling_convert_ycbcrimage = new byte[3][][];
				m_subsampling_convert_ycbcrimage[0] = new byte[m_subsampling_convert_ylines][];
				for (uint num = 0u; num < m_subsampling_convert_ylines; num++)
				{
					m_subsampling_convert_ycbcrimage[0][num] = m_subsampling_convert_ybuf[num];
				}
				m_subsampling_convert_ycbcrimage[1] = new byte[m_subsampling_convert_clines][];
				for (uint num2 = 0u; num2 < m_subsampling_convert_clines; num2++)
				{
					m_subsampling_convert_ycbcrimage[1][num2] = m_subsampling_convert_cbbuf[num2];
				}
				m_subsampling_convert_ycbcrimage[2] = new byte[m_subsampling_convert_clines][];
				for (uint num3 = 0u; num3 < m_subsampling_convert_clines; num3++)
				{
					m_subsampling_convert_ycbcrimage[2][num3] = m_subsampling_convert_crbuf[num3];
				}
				m_subsampling_convert_clinelenout = (m_strile_width + m_subsampling_hor - 1) / m_subsampling_hor;
				m_subsampling_convert_state = 0u;
				m_bytes_per_line = (uint)(m_subsampling_convert_clinelenout * (m_subsampling_ver * m_subsampling_hor + 2));
				m_lines_per_strile = (m_strile_length + m_subsampling_ver - 1) / m_subsampling_ver;
				m_subsampling_convert_log = true;
			}
		}
		else
		{
			if (m_forceProcessedRgbOutput)
			{
				m_libjpeg_jpeg_decompress_struct.Do_fancy_upsampling = false;
				m_libjpeg_jpeg_decompress_struct.Jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
				m_libjpeg_jpeg_decompress_struct.Out_color_space = J_COLOR_SPACE.JCS_RGB;
			}
			else
			{
				m_libjpeg_jpeg_decompress_struct.Jpeg_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
				m_libjpeg_jpeg_decompress_struct.Out_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
			}
			m_libjpeg_jpeg_query_style = 1;
			m_bytes_per_line = m_samples_per_pixel_per_plane * m_strile_width;
			m_lines_per_strile = m_strile_length;
		}
		if (!jpeg_start_decompress_encap())
		{
			return false;
		}
		m_writeheader_done = true;
		return true;
	}

	private void OJPEGLibjpegSessionAbort()
	{
		m_libjpeg_jpeg_decompress_struct.jpeg_destroy();
		m_libjpeg_session_active = false;
	}

	private bool OJPEGReadHeaderInfoSec()
	{
		if (m_file_size == 0)
		{
			m_file_size = (uint)m_tif.GetStream().Size(m_tif.m_clientdata);
		}
		if (m_jpeg_interchange_format != 0)
		{
			if (m_jpeg_interchange_format >= m_file_size)
			{
				m_jpeg_interchange_format = 0u;
				m_jpeg_interchange_format_length = 0u;
			}
			else if (m_jpeg_interchange_format_length == 0 || m_jpeg_interchange_format + m_jpeg_interchange_format_length > m_file_size)
			{
				m_jpeg_interchange_format_length = m_file_size - m_jpeg_interchange_format;
			}
		}
		m_in_buffer_source = OJPEGStateInBufferSource.osibsNotSetYet;
		m_in_buffer_next_strile = 0u;
		m_in_buffer_strile_count = (uint)m_tif.m_dir.td_nstrips;
		m_in_buffer_file_togo = 0u;
		m_in_buffer_togo = 0;
		byte b;
		do
		{
			if (!OJPEGReadBytePeek(out b))
			{
				return false;
			}
			if (b != byte.MaxValue)
			{
				break;
			}
			OJPEGReadByteAdvance();
			do
			{
				if (!OJPEGReadByte(out b))
				{
					return false;
				}
			}
			while (b == byte.MaxValue);
			switch ((JPEG_MARKER)b)
			{
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
			case JPEG_MARKER.COM:
			{
				if (!OJPEGReadWord(out var word))
				{
					return false;
				}
				if (word < 2)
				{
					if (!m_subsamplingcorrect)
					{
						Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSec", "Corrupt JPEG data");
					}
					return false;
				}
				if (word > 2)
				{
					OJPEGReadSkip((ushort)(word - 2));
				}
				break;
			}
			case JPEG_MARKER.DRI:
				if (!OJPEGReadHeaderInfoSecStreamDri())
				{
					return false;
				}
				break;
			case JPEG_MARKER.DQT:
				if (!OJPEGReadHeaderInfoSecStreamDqt())
				{
					return false;
				}
				break;
			case JPEG_MARKER.DHT:
				if (!OJPEGReadHeaderInfoSecStreamDht())
				{
					return false;
				}
				break;
			case JPEG_MARKER.SOF0:
			case JPEG_MARKER.SOF1:
			case JPEG_MARKER.SOF3:
				if (!OJPEGReadHeaderInfoSecStreamSof(b))
				{
					return false;
				}
				if (m_subsamplingcorrect)
				{
					return true;
				}
				break;
			case JPEG_MARKER.SOS:
				if (m_subsamplingcorrect)
				{
					return true;
				}
				if (!OJPEGReadHeaderInfoSecStreamSos())
				{
					return false;
				}
				break;
			default:
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSec", "Unknown marker type {0} in JPEG data", b);
				return false;
			case JPEG_MARKER.SOI:
				break;
			}
		}
		while (b != 218);
		if (m_subsamplingcorrect)
		{
			return true;
		}
		if (!m_sof_log)
		{
			if (!OJPEGReadHeaderInfoSecTablesQTable())
			{
				return false;
			}
			m_sof_marker_id = 192;
			for (byte b2 = 0; b2 < m_samples_per_pixel; b2++)
			{
				m_sof_c[b2] = b2;
			}
			m_sof_hv[0] = (byte)((m_subsampling_hor << 4) | m_subsampling_ver);
			for (byte b2 = 1; b2 < m_samples_per_pixel; b2++)
			{
				m_sof_hv[b2] = 17;
			}
			m_sof_x = m_strile_width;
			m_sof_y = m_strile_length_total;
			m_sof_log = true;
			if (!OJPEGReadHeaderInfoSecTablesDcTable())
			{
				return false;
			}
			if (!OJPEGReadHeaderInfoSecTablesAcTable())
			{
				return false;
			}
			for (byte b2 = 1; b2 < m_samples_per_pixel; b2++)
			{
				m_sos_cs[b2] = b2;
			}
		}
		return true;
	}

	private bool OJPEGReadHeaderInfoSecStreamDri()
	{
		if (!OJPEGReadWord(out var word))
		{
			return false;
		}
		if (word != 4)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDri", "Corrupt DRI marker in JPEG data");
			return false;
		}
		if (!OJPEGReadWord(out word))
		{
			return false;
		}
		m_restart_interval = word;
		return true;
	}

	private bool OJPEGReadHeaderInfoSecStreamDqt()
	{
		if (!OJPEGReadWord(out var word))
		{
			return false;
		}
		if (word <= 2)
		{
			if (!m_subsamplingcorrect)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDqt", "Corrupt DQT marker in JPEG data");
			}
			return false;
		}
		if (m_subsamplingcorrect)
		{
			OJPEGReadSkip((ushort)(word - 2));
		}
		else
		{
			word -= 2;
			do
			{
				if (word < 65)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDqt", "Corrupt DQT marker in JPEG data");
					return false;
				}
				byte[] array = new byte[69];
				array[0] = byte.MaxValue;
				array[1] = 219;
				array[2] = 0;
				array[3] = 67;
				if (!OJPEGReadBlock(65, array, 4))
				{
					return false;
				}
				byte b = (byte)(array[4] & 0xFu);
				if (3 < b)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDqt", "Corrupt DQT marker in JPEG data");
					return false;
				}
				m_qtable[b] = array;
				word -= 65;
			}
			while (word > 0);
		}
		return true;
	}

	private bool OJPEGReadHeaderInfoSecStreamDht()
	{
		if (!OJPEGReadWord(out var word))
		{
			return false;
		}
		if (word <= 2)
		{
			if (!m_subsamplingcorrect)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDht", "Corrupt DHT marker in JPEG data");
			}
			return false;
		}
		if (m_subsamplingcorrect)
		{
			OJPEGReadSkip((ushort)(word - 2));
		}
		else
		{
			byte[] array = new byte[2 + word];
			array[0] = byte.MaxValue;
			array[1] = 196;
			array[2] = (byte)(word >> 8);
			array[3] = (byte)(word & 0xFFu);
			if (!OJPEGReadBlock((ushort)(word - 2), array, 4))
			{
				return false;
			}
			byte b = array[4];
			if ((b & 0xF0) == 0)
			{
				if (3 < b)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDht", "Corrupt DHT marker in JPEG data");
					return false;
				}
				m_dctable[b] = array;
			}
			else
			{
				if ((b & 0xF0) != 16)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDht", "Corrupt DHT marker in JPEG data");
					return false;
				}
				b = (byte)(b & 0xFu);
				if (3 < b)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamDht", "Corrupt DHT marker in JPEG data");
					return false;
				}
				m_actable[b] = array;
			}
		}
		return true;
	}

	private bool OJPEGReadHeaderInfoSecStreamSof(byte marker_id)
	{
		if (m_sof_log)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "Corrupt JPEG data");
			return false;
		}
		if (!m_subsamplingcorrect)
		{
			m_sof_marker_id = marker_id;
		}
		if (!OJPEGReadWord(out var word))
		{
			return false;
		}
		if (word < 11)
		{
			if (!m_subsamplingcorrect)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "Corrupt SOF marker in JPEG data");
			}
			return false;
		}
		word -= 8;
		if (word % 3 != 0)
		{
			if (!m_subsamplingcorrect)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "Corrupt SOF marker in JPEG data");
			}
			return false;
		}
		ushort num = (ushort)(word / 3);
		if (!m_subsamplingcorrect && num != m_samples_per_pixel)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "JPEG compressed data indicates unexpected number of samples");
			return false;
		}
		if (!OJPEGReadByte(out var b))
		{
			return false;
		}
		if (b != 8)
		{
			if (!m_subsamplingcorrect)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "JPEG compressed data indicates unexpected number of bits per sample");
			}
			return false;
		}
		if (m_subsamplingcorrect)
		{
			OJPEGReadSkip(4);
		}
		else
		{
			if (!OJPEGReadWord(out var word2))
			{
				return false;
			}
			if (word2 < m_image_length && word2 < m_strile_length_total)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "JPEG compressed data indicates unexpected height");
				return false;
			}
			m_sof_y = word2;
			if (!OJPEGReadWord(out word2))
			{
				return false;
			}
			if (word2 < m_image_width && word2 < m_strile_width)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "JPEG compressed data indicates unexpected width");
				return false;
			}
			m_sof_x = word2;
		}
		if (!OJPEGReadByte(out b))
		{
			return false;
		}
		if (b != num)
		{
			if (!m_subsamplingcorrect)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "Corrupt SOF marker in JPEG data");
			}
			return false;
		}
		for (ushort num2 = 0; num2 < num; num2++)
		{
			if (!OJPEGReadByte(out b))
			{
				return false;
			}
			if (!m_subsamplingcorrect)
			{
				m_sof_c[num2] = b;
			}
			if (!OJPEGReadByte(out b))
			{
				return false;
			}
			if (m_subsamplingcorrect)
			{
				if (num2 == 0)
				{
					m_subsampling_hor = (byte)(b >> 4);
					m_subsampling_ver = (byte)(b & 0xFu);
					if ((m_subsampling_hor != 1 && m_subsampling_hor != 2 && m_subsampling_hor != 4) || (m_subsampling_ver != 1 && m_subsampling_ver != 2 && m_subsampling_ver != 4) || m_forceProcessedRgbOutput)
					{
						m_subsampling_force_desubsampling_inside_decompression = true;
					}
				}
				else if (b != 17)
				{
					m_subsampling_force_desubsampling_inside_decompression = true;
				}
			}
			else
			{
				m_sof_hv[num2] = b;
				if (!m_subsampling_force_desubsampling_inside_decompression)
				{
					if (num2 == 0)
					{
						if (b != ((m_subsampling_hor << 4) | m_subsampling_ver))
						{
							Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "JPEG compressed data indicates unexpected subsampling values");
							return false;
						}
					}
					else if (b != 17)
					{
						Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSof", "JPEG compressed data indicates unexpected subsampling values");
						return false;
					}
				}
			}
			if (!OJPEGReadByte(out b))
			{
				return false;
			}
			if (!m_subsamplingcorrect)
			{
				m_sof_tq[num2] = b;
			}
		}
		if (!m_subsamplingcorrect)
		{
			m_sof_log = true;
		}
		return true;
	}

	private bool OJPEGReadHeaderInfoSecStreamSos()
	{
		if (!m_sof_log)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSos", "Corrupt SOS marker in JPEG data");
			return false;
		}
		if (!OJPEGReadWord(out var word))
		{
			return false;
		}
		if (word != 6 + m_samples_per_pixel_per_plane * 2)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSos", "Corrupt SOS marker in JPEG data");
			return false;
		}
		if (!OJPEGReadByte(out var b))
		{
			return false;
		}
		if (b != m_samples_per_pixel_per_plane)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecStreamSos", "Corrupt SOS marker in JPEG data");
			return false;
		}
		for (byte b2 = 0; b2 < m_samples_per_pixel_per_plane; b2++)
		{
			if (!OJPEGReadByte(out b))
			{
				return false;
			}
			m_sos_cs[m_plane_sample_offset + b2] = b;
			if (!OJPEGReadByte(out b))
			{
				return false;
			}
			m_sos_tda[m_plane_sample_offset + b2] = b;
		}
		OJPEGReadSkip(3);
		return true;
	}

	private bool OJPEGReadHeaderInfoSecTablesQTable()
	{
		if (m_qtable_offset[0] == 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecTablesQTable", "Missing JPEG tables");
			return false;
		}
		m_in_buffer_file_pos_log = false;
		for (byte b = 0; b < m_samples_per_pixel; b++)
		{
			if (m_qtable_offset[b] != 0 && (b == 0 || m_qtable_offset[b] != m_qtable_offset[b - 1]))
			{
				for (byte b2 = 0; b2 < b - 1; b2++)
				{
					if (m_qtable_offset[b] == m_qtable_offset[b2])
					{
						Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecTablesQTable", "Corrupt JpegQTables tag value");
						return false;
					}
				}
				byte[] array = new byte[69];
				array[0] = byte.MaxValue;
				array[1] = 219;
				array[2] = 0;
				array[3] = 67;
				array[4] = b;
				TiffStream stream = m_tif.GetStream();
				stream.Seek(m_tif.m_clientdata, m_qtable_offset[b], SeekOrigin.Begin);
				if (stream.Read(m_tif.m_clientdata, array, 5, 64) != 64)
				{
					return false;
				}
				m_qtable[b] = array;
				m_sof_tq[b] = b;
			}
			else
			{
				m_sof_tq[b] = m_sof_tq[b - 1];
			}
		}
		return true;
	}

	private bool OJPEGReadHeaderInfoSecTablesDcTable()
	{
		byte[] array = new byte[16];
		if (m_dctable_offset[0] == 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecTablesDcTable", "Missing JPEG tables");
			return false;
		}
		m_in_buffer_file_pos_log = false;
		for (byte b = 0; b < m_samples_per_pixel; b++)
		{
			if (m_dctable_offset[b] != 0 && (b == 0 || m_dctable_offset[b] != m_dctable_offset[b - 1]))
			{
				for (byte b2 = 0; b2 < b - 1; b2++)
				{
					if (m_dctable_offset[b] == m_dctable_offset[b2])
					{
						Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecTablesDcTable", "Corrupt JpegDcTables tag value");
						return false;
					}
				}
				TiffStream stream = m_tif.GetStream();
				stream.Seek(m_tif.m_clientdata, m_dctable_offset[b], SeekOrigin.Begin);
				if (stream.Read(m_tif.m_clientdata, array, 0, 16) != 16)
				{
					return false;
				}
				uint num = 0u;
				for (byte b2 = 0; b2 < 16; b2++)
				{
					num += array[b2];
				}
				byte[] array2 = new byte[21 + num];
				array2[0] = byte.MaxValue;
				array2[1] = 196;
				array2[2] = (byte)(19 + num >> 8);
				array2[3] = (byte)((19 + num) & 0xFFu);
				array2[4] = b;
				for (byte b2 = 0; b2 < 16; b2++)
				{
					array2[5 + b2] = array[b2];
				}
				if (stream.Read(m_tif.m_clientdata, array2, 21, (int)num) != (int)num)
				{
					return false;
				}
				m_dctable[b] = array2;
				m_sos_tda[b] = (byte)(b << 4);
			}
			else
			{
				m_sos_tda[b] = m_sos_tda[b - 1];
			}
		}
		return true;
	}

	private bool OJPEGReadHeaderInfoSecTablesAcTable()
	{
		byte[] array = new byte[16];
		if (m_actable_offset[0] == 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecTablesAcTable", "Missing JPEG tables");
			return false;
		}
		m_in_buffer_file_pos_log = false;
		for (byte b = 0; b < m_samples_per_pixel; b++)
		{
			if (m_actable_offset[b] != 0 && (b == 0 || m_actable_offset[b] != m_actable_offset[b - 1]))
			{
				for (byte b2 = 0; b2 < b - 1; b2++)
				{
					if (m_actable_offset[b] == m_actable_offset[b2])
					{
						Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "OJPEGReadHeaderInfoSecTablesAcTable", "Corrupt JpegAcTables tag value");
						return false;
					}
				}
				TiffStream stream = m_tif.GetStream();
				stream.Seek(m_tif.m_clientdata, m_actable_offset[b], SeekOrigin.Begin);
				if (stream.Read(m_tif.m_clientdata, array, 0, 16) != 16)
				{
					return false;
				}
				uint num = 0u;
				for (byte b2 = 0; b2 < 16; b2++)
				{
					num += array[b2];
				}
				byte[] array2 = new byte[21 + num];
				array2[0] = byte.MaxValue;
				array2[1] = 196;
				array2[2] = (byte)(19 + num >> 8);
				array2[3] = (byte)((19 + num) & 0xFFu);
				array2[4] = (byte)(0x10u | b);
				for (byte b2 = 0; b2 < 16; b2++)
				{
					array2[5 + b2] = array[b2];
				}
				if (stream.Read(m_tif.m_clientdata, array2, 21, (int)num) != (int)num)
				{
					return false;
				}
				m_actable[b] = array2;
				m_sos_tda[b] |= b;
			}
			else
			{
				m_sos_tda[b] = (byte)(m_sos_tda[b] | (m_sos_tda[b - 1] & 0xFu));
			}
		}
		return true;
	}

	private bool OJPEGReadBufferFill()
	{
		while (m_in_buffer_file_togo == 0)
		{
			m_in_buffer_file_pos_log = false;
			switch (m_in_buffer_source)
			{
			case OJPEGStateInBufferSource.osibsNotSetYet:
				if (m_jpeg_interchange_format != 0)
				{
					m_in_buffer_file_pos = m_jpeg_interchange_format;
					m_in_buffer_file_togo = m_jpeg_interchange_format_length;
				}
				m_in_buffer_source = OJPEGStateInBufferSource.osibsJpegInterchangeFormat;
				break;
			case OJPEGStateInBufferSource.osibsJpegInterchangeFormat:
				m_in_buffer_source = OJPEGStateInBufferSource.osibsStrile;
				goto case OJPEGStateInBufferSource.osibsStrile;
			case OJPEGStateInBufferSource.osibsStrile:
				if (m_in_buffer_next_strile == m_in_buffer_strile_count)
				{
					m_in_buffer_source = OJPEGStateInBufferSource.osibsEof;
					break;
				}
				if (m_tif.m_dir.td_stripoffset == null)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "Strip offsets are missing");
					return false;
				}
				m_in_buffer_file_pos = (uint)m_tif.m_dir.td_stripoffset[m_in_buffer_next_strile];
				if (m_in_buffer_file_pos != 0)
				{
					if (m_in_buffer_file_pos >= m_file_size)
					{
						m_in_buffer_file_pos = 0u;
					}
					else
					{
						m_in_buffer_file_togo = (uint)m_tif.m_dir.td_stripbytecount[m_in_buffer_next_strile];
						if (m_in_buffer_file_togo == 0)
						{
							m_in_buffer_file_pos = 0u;
						}
						else if (m_in_buffer_file_pos + m_in_buffer_file_togo > m_file_size)
						{
							m_in_buffer_file_togo = m_file_size - m_in_buffer_file_pos;
						}
					}
				}
				m_in_buffer_next_strile++;
				break;
			default:
				return false;
			}
		}
		TiffStream stream = m_tif.GetStream();
		if (!m_in_buffer_file_pos_log)
		{
			stream.Seek(m_tif.m_clientdata, m_in_buffer_file_pos, SeekOrigin.Begin);
			m_in_buffer_file_pos_log = true;
		}
		ushort num = 2048;
		if (num > m_in_buffer_file_togo)
		{
			num = (ushort)m_in_buffer_file_togo;
		}
		int num2 = stream.Read(m_tif.m_clientdata, m_in_buffer, 0, num);
		if (num2 == 0)
		{
			return false;
		}
		num = (m_in_buffer_togo = (ushort)num2);
		m_in_buffer_cur = 0;
		m_in_buffer_file_togo -= num;
		m_in_buffer_file_pos += num;
		return true;
	}

	private bool OJPEGReadByte(out byte b)
	{
		if (m_in_buffer_togo == 0 && !OJPEGReadBufferFill())
		{
			b = 0;
			return false;
		}
		b = m_in_buffer[m_in_buffer_cur];
		m_in_buffer_cur++;
		m_in_buffer_togo--;
		return true;
	}

	public bool OJPEGReadBytePeek(out byte b)
	{
		if (m_in_buffer_togo == 0 && !OJPEGReadBufferFill())
		{
			b = 0;
			return false;
		}
		b = m_in_buffer[m_in_buffer_cur];
		return true;
	}

	private void OJPEGReadByteAdvance()
	{
		m_in_buffer_cur++;
		m_in_buffer_togo--;
	}

	private bool OJPEGReadWord(out ushort word)
	{
		word = 0;
		if (!OJPEGReadByte(out var b))
		{
			return false;
		}
		word = (ushort)(b << 8);
		if (!OJPEGReadByte(out b))
		{
			return false;
		}
		word |= b;
		return true;
	}

	public bool OJPEGReadBlock(ushort len, byte[] mem, int offset)
	{
		ushort num = len;
		int num2 = offset;
		do
		{
			if (m_in_buffer_togo == 0 && !OJPEGReadBufferFill())
			{
				return false;
			}
			ushort num3 = num;
			if (num3 > m_in_buffer_togo)
			{
				num3 = m_in_buffer_togo;
			}
			Buffer.BlockCopy(m_in_buffer, m_in_buffer_cur, mem, num2, num3);
			m_in_buffer_cur += num3;
			m_in_buffer_togo -= num3;
			num -= num3;
			num2 += num3;
		}
		while (num > 0);
		return true;
	}

	private void OJPEGReadSkip(ushort len)
	{
		ushort num = len;
		ushort num2 = num;
		if (num2 > m_in_buffer_togo)
		{
			num2 = m_in_buffer_togo;
		}
		m_in_buffer_cur += num2;
		m_in_buffer_togo -= num2;
		num -= num2;
		if (num > 0)
		{
			num2 = num;
			if (num2 > m_in_buffer_file_togo)
			{
				num2 = (ushort)m_in_buffer_file_togo;
			}
			m_in_buffer_file_pos += num2;
			m_in_buffer_file_togo -= num2;
			m_in_buffer_file_pos_log = false;
		}
	}

	internal bool OJPEGWriteStream(out byte[] mem, out uint len)
	{
		mem = null;
		len = 0u;
		do
		{
			switch (m_out_state)
			{
			case OJPEGStateOutState.ososSoi:
				OJPEGWriteStreamSoi(out mem, out len);
				break;
			case OJPEGStateOutState.ososQTable0:
				OJPEGWriteStreamQTable(0, out mem, out len);
				break;
			case OJPEGStateOutState.ososQTable1:
				OJPEGWriteStreamQTable(1, out mem, out len);
				break;
			case OJPEGStateOutState.ososQTable2:
				OJPEGWriteStreamQTable(2, out mem, out len);
				break;
			case OJPEGStateOutState.ososQTable3:
				OJPEGWriteStreamQTable(3, out mem, out len);
				break;
			case OJPEGStateOutState.ososDcTable0:
				OJPEGWriteStreamDcTable(0, out mem, out len);
				break;
			case OJPEGStateOutState.ososDcTable1:
				OJPEGWriteStreamDcTable(1, out mem, out len);
				break;
			case OJPEGStateOutState.ososDcTable2:
				OJPEGWriteStreamDcTable(2, out mem, out len);
				break;
			case OJPEGStateOutState.ososDcTable3:
				OJPEGWriteStreamDcTable(3, out mem, out len);
				break;
			case OJPEGStateOutState.ososAcTable0:
				OJPEGWriteStreamAcTable(0, out mem, out len);
				break;
			case OJPEGStateOutState.ososAcTable1:
				OJPEGWriteStreamAcTable(1, out mem, out len);
				break;
			case OJPEGStateOutState.ososAcTable2:
				OJPEGWriteStreamAcTable(2, out mem, out len);
				break;
			case OJPEGStateOutState.ososAcTable3:
				OJPEGWriteStreamAcTable(3, out mem, out len);
				break;
			case OJPEGStateOutState.ososDri:
				OJPEGWriteStreamDri(out mem, out len);
				break;
			case OJPEGStateOutState.ososSof:
				OJPEGWriteStreamSof(out mem, out len);
				break;
			case OJPEGStateOutState.ososSos:
				OJPEGWriteStreamSos(out mem, out len);
				break;
			case OJPEGStateOutState.ososCompressed:
				if (!OJPEGWriteStreamCompressed(out mem, out len))
				{
					return false;
				}
				break;
			case OJPEGStateOutState.ososRst:
				OJPEGWriteStreamRst(out mem, out len);
				break;
			case OJPEGStateOutState.ososEoi:
				OJPEGWriteStreamEoi(out mem, out len);
				break;
			}
		}
		while (len == 0);
		return true;
	}

	private void OJPEGWriteStreamSoi(out byte[] mem, out uint len)
	{
		m_out_buffer[0] = byte.MaxValue;
		m_out_buffer[1] = 216;
		len = 2u;
		mem = m_out_buffer;
		m_out_state++;
	}

	private void OJPEGWriteStreamQTable(byte table_index, out byte[] mem, out uint len)
	{
		mem = null;
		len = 0u;
		if (m_qtable[table_index] != null)
		{
			mem = m_qtable[table_index];
			len = (uint)m_qtable[table_index].Length;
		}
		m_out_state++;
	}

	private void OJPEGWriteStreamDcTable(byte table_index, out byte[] mem, out uint len)
	{
		mem = null;
		len = 0u;
		if (m_dctable[table_index] != null)
		{
			mem = m_dctable[table_index];
			len = (uint)m_dctable[table_index].Length;
		}
		m_out_state++;
	}

	private void OJPEGWriteStreamAcTable(byte table_index, out byte[] mem, out uint len)
	{
		mem = null;
		len = 0u;
		if (m_actable[table_index] != null)
		{
			mem = m_actable[table_index];
			len = (uint)m_actable[table_index].Length;
		}
		m_out_state++;
	}

	private void OJPEGWriteStreamDri(out byte[] mem, out uint len)
	{
		mem = null;
		len = 0u;
		if (m_restart_interval != 0)
		{
			m_out_buffer[0] = byte.MaxValue;
			m_out_buffer[1] = 221;
			m_out_buffer[2] = 0;
			m_out_buffer[3] = 4;
			m_out_buffer[4] = (byte)(m_restart_interval >> 8);
			m_out_buffer[5] = (byte)(m_restart_interval & 0xFFu);
			len = 6u;
			mem = m_out_buffer;
		}
		m_out_state++;
	}

	private void OJPEGWriteStreamSof(out byte[] mem, out uint len)
	{
		m_out_buffer[0] = byte.MaxValue;
		m_out_buffer[1] = m_sof_marker_id;
		m_out_buffer[2] = 0;
		m_out_buffer[3] = (byte)(8 + m_samples_per_pixel_per_plane * 3);
		m_out_buffer[4] = 8;
		m_out_buffer[5] = (byte)(m_sof_y >> 8);
		m_out_buffer[6] = (byte)(m_sof_y & 0xFFu);
		m_out_buffer[7] = (byte)(m_sof_x >> 8);
		m_out_buffer[8] = (byte)(m_sof_x & 0xFFu);
		m_out_buffer[9] = m_samples_per_pixel_per_plane;
		for (byte b = 0; b < m_samples_per_pixel_per_plane; b++)
		{
			m_out_buffer[10 + b * 3] = m_sof_c[m_plane_sample_offset + b];
			m_out_buffer[10 + b * 3 + 1] = m_sof_hv[m_plane_sample_offset + b];
			m_out_buffer[10 + b * 3 + 2] = m_sof_tq[m_plane_sample_offset + b];
		}
		len = (uint)(10 + m_samples_per_pixel_per_plane * 3);
		mem = m_out_buffer;
		m_out_state++;
	}

	private void OJPEGWriteStreamSos(out byte[] mem, out uint len)
	{
		m_out_buffer[0] = byte.MaxValue;
		m_out_buffer[1] = 218;
		m_out_buffer[2] = 0;
		m_out_buffer[3] = (byte)(6 + m_samples_per_pixel_per_plane * 2);
		m_out_buffer[4] = m_samples_per_pixel_per_plane;
		for (byte b = 0; b < m_samples_per_pixel_per_plane; b++)
		{
			m_out_buffer[5 + b * 2] = m_sos_cs[m_plane_sample_offset + b];
			m_out_buffer[5 + b * 2 + 1] = m_sos_tda[m_plane_sample_offset + b];
		}
		m_out_buffer[5 + m_samples_per_pixel_per_plane * 2] = 0;
		m_out_buffer[5 + m_samples_per_pixel_per_plane * 2 + 1] = 63;
		m_out_buffer[5 + m_samples_per_pixel_per_plane * 2 + 2] = 0;
		len = (uint)(8 + m_samples_per_pixel_per_plane * 2);
		mem = m_out_buffer;
		m_out_state++;
	}

	private bool OJPEGWriteStreamCompressed(out byte[] mem, out uint len)
	{
		mem = null;
		len = 0u;
		if (m_in_buffer_togo == 0 && !OJPEGReadBufferFill())
		{
			return false;
		}
		len = m_in_buffer_togo;
		if (m_in_buffer_cur == 0)
		{
			mem = m_in_buffer;
		}
		else
		{
			mem = new byte[len];
			Buffer.BlockCopy(m_in_buffer, m_in_buffer_cur, mem, 0, (int)len);
		}
		m_in_buffer_togo = 0;
		if (m_in_buffer_file_togo == 0)
		{
			switch (m_in_buffer_source)
			{
			case OJPEGStateInBufferSource.osibsStrile:
				if (m_in_buffer_next_strile < m_in_buffer_strile_count)
				{
					m_out_state = OJPEGStateOutState.ososRst;
				}
				else
				{
					m_out_state = OJPEGStateOutState.ososEoi;
				}
				break;
			case OJPEGStateInBufferSource.osibsEof:
				m_out_state = OJPEGStateOutState.ososEoi;
				break;
			}
		}
		return true;
	}

	private void OJPEGWriteStreamRst(out byte[] mem, out uint len)
	{
		m_out_buffer[0] = byte.MaxValue;
		m_out_buffer[1] = (byte)(208 + m_restart_index);
		m_restart_index++;
		if (m_restart_index == 8)
		{
			m_restart_index = 0;
		}
		len = 2u;
		mem = m_out_buffer;
		m_out_state = OJPEGStateOutState.ososCompressed;
	}

	private void OJPEGWriteStreamEoi(out byte[] mem, out uint len)
	{
		m_out_buffer[0] = byte.MaxValue;
		m_out_buffer[1] = 217;
		len = 2u;
		mem = m_out_buffer;
	}

	private bool jpeg_create_decompress_encap()
	{
		try
		{
			m_libjpeg_jpeg_decompress_struct = new jpeg_decompress_struct(m_libjpeg_jpeg_error_mgr);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private ReadResult jpeg_read_header_encap(bool require_image)
	{
		ReadResult readResult = ReadResult.JPEG_SUSPENDED;
		try
		{
			return m_libjpeg_jpeg_decompress_struct.jpeg_read_header(require_image);
		}
		catch (Exception)
		{
			return ReadResult.JPEG_SUSPENDED;
		}
	}

	private bool jpeg_start_decompress_encap()
	{
		try
		{
			m_libjpeg_jpeg_decompress_struct.jpeg_start_decompress();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private int jpeg_read_scanlines_encap(byte[] scanlines, int max_lines)
	{
		int num = 0;
		try
		{
			byte[][] scanlines2 = new byte[1][] { scanlines };
			return m_libjpeg_jpeg_decompress_struct.jpeg_read_scanlines(scanlines2, max_lines);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	private int jpeg_read_raw_data_encap(int max_lines)
	{
		int num = 0;
		try
		{
			return m_libjpeg_jpeg_decompress_struct.jpeg_read_raw_data(m_subsampling_convert_ycbcrimage, max_lines);
		}
		catch (Exception)
		{
			return 0;
		}
	}
}
