using System;
using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegCodec : TiffCodec
{
	public const int FIELD_JPEGTABLES = 66;

	public const int FIELD_RECVPARAMS = 67;

	public const int FIELD_SUBADDRESS = 68;

	public const int FIELD_RECVTIME = 69;

	public const int FIELD_FAXDCS = 70;

	internal jpeg_compress_struct m_compression;

	internal jpeg_decompress_struct m_decompression;

	internal jpeg_common_struct m_common;

	internal int m_h_sampling;

	internal int m_v_sampling;

	internal byte[] m_jpegtables;

	internal int m_jpegtables_length;

	internal int m_jpegquality;

	internal JpegColorMode m_jpegcolormode;

	internal JpegTablesMode m_jpegtablesmode;

	internal bool m_ycbcrsampling_fetched;

	internal int m_recvparams;

	internal string m_subaddress;

	internal int m_recvtime;

	internal string m_faxdcs;

	private static readonly TiffFieldInfo[] jpegFieldInfo = new TiffFieldInfo[8]
	{
		new TiffFieldInfo(TiffTag.JPEGTABLES, -3, -3, TiffType.UNDEFINED, 66, okToChange: false, passCount: true, "JPEGTables"),
		new TiffFieldInfo(TiffTag.JPEGQUALITY, 0, 0, TiffType.NOTYPE, 0, okToChange: true, passCount: false, string.Empty),
		new TiffFieldInfo(TiffTag.JPEGCOLORMODE, 0, 0, TiffType.NOTYPE, 0, okToChange: false, passCount: false, string.Empty),
		new TiffFieldInfo(TiffTag.JPEGTABLESMODE, 0, 0, TiffType.NOTYPE, 0, okToChange: false, passCount: false, string.Empty),
		new TiffFieldInfo(TiffTag.FAXRECVPARAMS, 1, 1, TiffType.LONG, 67, okToChange: true, passCount: false, "FaxRecvParams"),
		new TiffFieldInfo(TiffTag.FAXSUBADDRESS, -1, -1, TiffType.ASCII, 68, okToChange: true, passCount: false, "FaxSubAddress"),
		new TiffFieldInfo(TiffTag.FAXRECVTIME, 1, 1, TiffType.LONG, 69, okToChange: true, passCount: false, "FaxRecvTime"),
		new TiffFieldInfo(TiffTag.FAXDCS, -1, -1, TiffType.ASCII, 70, okToChange: true, passCount: false, "FaxDcs")
	};

	private bool m_rawDecode;

	private bool m_rawEncode;

	private TiffTagMethods m_tagMethods;

	private TiffTagMethods m_parentTagMethods;

	private bool m_cinfo_initialized;

	internal jpeg_error_mgr m_err;

	private Photometric m_photometric;

	private int m_bytesperline;

	private byte[][][] m_ds_buffer = new byte[10][][];

	private int m_scancount;

	private int m_samplesperclump;

	public override bool CanEncode => true;

	public override bool CanDecode => true;

	public JpegCodec(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
		m_tagMethods = new JpegCodecTagMethods();
	}

	private void cleanState()
	{
		m_compression = null;
		m_decompression = null;
		m_common = null;
		m_h_sampling = 0;
		m_v_sampling = 0;
		m_jpegtables = null;
		m_jpegtables_length = 0;
		m_jpegquality = 0;
		m_jpegcolormode = JpegColorMode.RAW;
		m_jpegtablesmode = JpegTablesMode.NONE;
		m_ycbcrsampling_fetched = false;
		m_recvparams = 0;
		m_subaddress = null;
		m_recvtime = 0;
		m_faxdcs = null;
		m_rawDecode = false;
		m_rawEncode = false;
		m_cinfo_initialized = false;
		m_err = null;
		m_photometric = Photometric.MINISWHITE;
		m_bytesperline = 0;
		m_ds_buffer = new byte[10][][];
		m_scancount = 0;
		m_samplesperclump = 0;
	}

	public override bool Init()
	{
		m_tif.MergeFieldInfo(jpegFieldInfo, jpegFieldInfo.Length);
		cleanState();
		m_err = new JpegErrorManager(this);
		m_parentTagMethods = m_tif.m_tagmethods;
		m_tif.m_tagmethods = m_tagMethods;
		m_jpegquality = 75;
		m_jpegcolormode = JpegColorMode.RGB;
		m_jpegtablesmode = JpegTablesMode.QUANT | JpegTablesMode.HUFF;
		m_tif.m_flags |= TiffFlags.NOBITREV;
		if (m_tif.m_diroff == 0L)
		{
			m_jpegtables_length = 2000;
			m_jpegtables = new byte[m_jpegtables_length];
		}
		m_tif.setFieldBit(39);
		return true;
	}

	public override bool SetupDecode()
	{
		return JPEGSetupDecode();
	}

	public override bool PreDecode(short plane)
	{
		return JPEGPreDecode(plane);
	}

	public override bool DecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		if (m_rawDecode)
		{
			return JPEGDecodeRaw(buffer, offset, count, plane);
		}
		return JPEGDecode(buffer, offset, count, plane);
	}

	public override bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		if (m_rawDecode)
		{
			return JPEGDecodeRaw(buffer, offset, count, plane);
		}
		return JPEGDecode(buffer, offset, count, plane);
	}

	public override bool DecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		if (m_rawDecode)
		{
			return JPEGDecodeRaw(buffer, offset, count, plane);
		}
		return JPEGDecode(buffer, offset, count, plane);
	}

	public override bool SetupEncode()
	{
		return JPEGSetupEncode();
	}

	public override bool PreEncode(short plane)
	{
		return JPEGPreEncode(plane);
	}

	public override bool PostEncode()
	{
		return JPEGPostEncode();
	}

	public override bool EncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		if (m_rawEncode)
		{
			return JPEGEncodeRaw(buffer, offset, count, plane);
		}
		return JPEGEncode(buffer, offset, count, plane);
	}

	public override bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		if (m_rawEncode)
		{
			return JPEGEncodeRaw(buffer, offset, count, plane);
		}
		return JPEGEncode(buffer, offset, count, plane);
	}

	public override bool EncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		if (m_rawEncode)
		{
			return JPEGEncodeRaw(buffer, offset, count, plane);
		}
		return JPEGEncode(buffer, offset, count, plane);
	}

	public override void Cleanup()
	{
		JPEGCleanup();
	}

	public override int DefStripSize(int size)
	{
		return JPEGDefaultStripSize(size);
	}

	public override void DefTileSize(ref int width, ref int height)
	{
		JPEGDefaultTileSize(ref width, ref height);
	}

	public bool InitializeLibJPEG(bool force_encode, bool force_decode)
	{
		int[] array = null;
		bool flag = true;
		if (m_cinfo_initialized)
		{
			if (force_encode && m_common.IsDecompressor)
			{
				TIFFjpeg_destroy();
			}
			else
			{
				if (!force_decode || m_common.IsDecompressor)
				{
					return true;
				}
				TIFFjpeg_destroy();
			}
			m_cinfo_initialized = false;
		}
		FieldValue[] field = m_tif.GetField(TiffTag.TILEBYTECOUNTS);
		if (m_tif.IsTiled() && field != null)
		{
			array = field[0].ToIntArray();
			if (array != null)
			{
				flag = array[0] == 0;
			}
		}
		field = m_tif.GetField(TiffTag.STRIPBYTECOUNTS);
		if (!m_tif.IsTiled() && field != null)
		{
			array = field[0].ToIntArray();
			if (array != null)
			{
				flag = array[0] == 0;
			}
		}
		if (force_decode || (!force_encode && (m_tif.m_mode == 0 || !flag)))
		{
			if (!TIFFjpeg_create_decompress())
			{
				return false;
			}
		}
		else if (!TIFFjpeg_create_compress())
		{
			return false;
		}
		m_cinfo_initialized = true;
		return true;
	}

	public Tiff GetTiff()
	{
		return m_tif;
	}

	public void JPEGResetUpsampled()
	{
		m_tif.m_flags &= ~TiffFlags.UPSAMPLED;
		if (m_tif.m_dir.td_planarconfig == PlanarConfig.CONTIG && m_tif.m_dir.td_photometric == Photometric.YCBCR && m_jpegcolormode == JpegColorMode.RGB)
		{
			m_tif.m_flags |= TiffFlags.UPSAMPLED;
		}
		if (m_tif.m_tilesize > 0)
		{
			m_tif.m_tilesize = (m_tif.IsTiled() ? m_tif.TileSize() : (-1));
		}
		if (m_tif.m_scanlinesize > 0)
		{
			m_tif.m_scanlinesize = m_tif.ScanlineSize();
		}
	}

	private bool JPEGPreEncode(short s)
	{
		int num;
		int num2;
		if (m_tif.IsTiled())
		{
			num = m_tif.m_dir.td_tilewidth;
			num2 = m_tif.m_dir.td_tilelength;
			m_bytesperline = m_tif.TileRowSize();
		}
		else
		{
			num = m_tif.m_dir.td_imagewidth;
			num2 = m_tif.m_dir.td_imagelength - m_tif.m_row;
			if (num2 > m_tif.m_dir.td_rowsperstrip)
			{
				num2 = m_tif.m_dir.td_rowsperstrip;
			}
			m_bytesperline = m_tif.oldScanlineSize();
		}
		if (m_tif.m_dir.td_planarconfig == PlanarConfig.SEPARATE && s > 0)
		{
			num = Tiff.howMany(num, m_h_sampling);
			num2 = Tiff.howMany(num2, m_v_sampling);
		}
		if (num > 65535 || num2 > 65535)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGPreEncode", "Strip/tile too large for JPEG");
			return false;
		}
		m_compression.Image_width = num;
		m_compression.Image_height = num2;
		bool flag = false;
		if (m_tif.m_dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			m_compression.Input_components = m_tif.m_dir.td_samplesperpixel;
			if (m_photometric == Photometric.YCBCR)
			{
				if (m_jpegcolormode == JpegColorMode.RGB)
				{
					m_compression.In_color_space = J_COLOR_SPACE.JCS_RGB;
				}
				else
				{
					m_compression.In_color_space = J_COLOR_SPACE.JCS_YCbCr;
					if (m_h_sampling != 1 || m_v_sampling != 1)
					{
						flag = true;
					}
				}
				if (!TIFFjpeg_set_colorspace(J_COLOR_SPACE.JCS_YCbCr))
				{
					return false;
				}
				m_compression.Component_info[0].H_samp_factor = m_h_sampling;
				m_compression.Component_info[0].V_samp_factor = m_v_sampling;
			}
			else
			{
				m_compression.In_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
				if (!TIFFjpeg_set_colorspace(J_COLOR_SPACE.JCS_UNKNOWN))
				{
					return false;
				}
			}
		}
		else
		{
			m_compression.Input_components = 1;
			m_compression.In_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
			if (!TIFFjpeg_set_colorspace(J_COLOR_SPACE.JCS_UNKNOWN))
			{
				return false;
			}
			m_compression.Component_info[0].Component_id = s;
			if (m_photometric == Photometric.YCBCR && s > 0)
			{
				m_compression.Component_info[0].Quant_tbl_no = 1;
				m_compression.Component_info[0].Dc_tbl_no = 1;
				m_compression.Component_info[0].Ac_tbl_no = 1;
			}
		}
		m_compression.Write_JFIF_header = false;
		m_compression.Write_Adobe_marker = false;
		if (!TIFFjpeg_set_quality(m_jpegquality, force_baseline: false))
		{
			return false;
		}
		if ((m_jpegtablesmode & JpegTablesMode.QUANT) == 0)
		{
			unsuppress_quant_table(0);
			unsuppress_quant_table(1);
		}
		if ((m_jpegtablesmode & JpegTablesMode.HUFF) != 0)
		{
			m_compression.Optimize_coding = false;
		}
		else
		{
			m_compression.Optimize_coding = true;
		}
		if (flag)
		{
			m_compression.Raw_data_in = true;
			m_rawEncode = true;
		}
		else
		{
			m_compression.Raw_data_in = false;
			m_rawEncode = false;
		}
		if (!TIFFjpeg_start_compress(write_all_tables: false))
		{
			return false;
		}
		if (flag && !alloc_downsampled_buffers(m_compression.Component_info, m_compression.Num_components))
		{
			return false;
		}
		m_scancount = 0;
		return true;
	}

	private bool JPEGSetupEncode()
	{
		InitializeLibJPEG(force_encode: true, force_decode: false);
		m_compression.In_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
		m_compression.Input_components = 1;
		if (!TIFFjpeg_set_defaults())
		{
			return false;
		}
		m_photometric = m_tif.m_dir.td_photometric;
		switch (m_photometric)
		{
		case Photometric.YCBCR:
			m_h_sampling = m_tif.m_dir.td_ycbcrsubsampling[0];
			m_v_sampling = m_tif.m_dir.td_ycbcrsubsampling[1];
			if (m_tif.GetField(TiffTag.REFERENCEBLACKWHITE) == null)
			{
				float[] array = new float[6];
				int num = 1 << (int)m_tif.m_dir.td_bitspersample;
				array[0] = 0f;
				array[1] = (long)num - 1L;
				array[2] = num >> 1;
				array[3] = array[1];
				array[4] = array[2];
				array[5] = array[1];
				m_tif.SetField(TiffTag.REFERENCEBLACKWHITE, array);
			}
			break;
		case Photometric.PALETTE:
		case Photometric.MASK:
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupEncode", "PhotometricInterpretation {0} not allowed for JPEG", m_photometric);
			return false;
		default:
			m_h_sampling = 1;
			m_v_sampling = 1;
			break;
		}
		if (m_tif.m_dir.td_bitspersample != 8)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupEncode", "BitsPerSample {0} not allowed for JPEG", m_tif.m_dir.td_bitspersample);
			return false;
		}
		m_compression.Data_precision = m_tif.m_dir.td_bitspersample;
		if (m_tif.IsTiled())
		{
			if (m_tif.m_dir.td_tilelength % (m_v_sampling * 8) != 0)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupEncode", "JPEG tile height must be multiple of {0}", m_v_sampling * 8);
				return false;
			}
			if (m_tif.m_dir.td_tilewidth % (m_h_sampling * 8) != 0)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupEncode", "JPEG tile width must be multiple of {0}", m_h_sampling * 8);
				return false;
			}
		}
		else if (m_tif.m_dir.td_rowsperstrip < m_tif.m_dir.td_imagelength && m_tif.m_dir.td_rowsperstrip % (m_v_sampling * 8) != 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupEncode", "RowsPerStrip must be multiple of {0} for JPEG", m_v_sampling * 8);
			return false;
		}
		if ((m_jpegtablesmode & (JpegTablesMode.QUANT | JpegTablesMode.HUFF)) != 0)
		{
			bool flag = true;
			if (m_jpegtables != null)
			{
				for (int i = 0; i < 8; i++)
				{
					if (m_jpegtables[i] != 0)
					{
						flag = false;
						break;
					}
				}
			}
			else
			{
				flag = false;
			}
			if (m_jpegtables == null || flag)
			{
				if (!prepare_JPEGTables())
				{
					return false;
				}
				m_tif.m_flags |= TiffFlags.DIRTYDIRECT;
				m_tif.setFieldBit(66);
			}
		}
		else
		{
			m_tif.clearFieldBit(66);
		}
		TIFFjpeg_data_dest();
		return true;
	}

	private bool JPEGPostEncode()
	{
		if (m_scancount > 0)
		{
			for (int i = 0; i < m_compression.Num_components; i++)
			{
				int v_samp_factor = m_compression.Component_info[i].V_samp_factor;
				int count = m_compression.Component_info[i].Width_in_blocks * 8;
				for (int j = m_scancount * v_samp_factor; j < 8 * v_samp_factor; j++)
				{
					Buffer.BlockCopy(m_ds_buffer[i][j - 1], 0, m_ds_buffer[i][j], 0, count);
				}
			}
			int num = m_compression.Max_v_samp_factor * 8;
			if (TIFFjpeg_write_raw_data(m_ds_buffer, num) != num)
			{
				return false;
			}
		}
		return TIFFjpeg_finish_compress();
	}

	private void JPEGCleanup()
	{
		m_tif.m_tagmethods = m_parentTagMethods;
		if (m_cinfo_initialized)
		{
			TIFFjpeg_destroy();
		}
	}

	private bool JPEGPreDecode(short s)
	{
		TiffDirectory dir = m_tif.m_dir;
		if (!TIFFjpeg_abort())
		{
			return false;
		}
		if (TIFFjpeg_read_header(require_image: true) != ReadResult.JPEG_HEADER_OK)
		{
			return false;
		}
		int num = dir.td_imagewidth;
		int num2 = dir.td_imagelength - m_tif.m_row;
		if (m_tif.IsTiled())
		{
			num = dir.td_tilewidth;
			num2 = dir.td_tilelength;
			m_bytesperline = m_tif.TileRowSize();
		}
		else
		{
			if (num2 > dir.td_rowsperstrip && dir.td_rowsperstrip != -1)
			{
				num2 = dir.td_rowsperstrip;
			}
			m_bytesperline = m_tif.oldScanlineSize();
		}
		if (dir.td_planarconfig == PlanarConfig.SEPARATE && s > 0)
		{
			num = Tiff.howMany(num, m_h_sampling);
			num2 = Tiff.howMany(num2, m_v_sampling);
		}
		if (m_decompression.Image_width < num || m_decompression.Image_height < num2)
		{
			Tiff.WarningExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Improper JPEG strip/tile size, expected {0}x{1}, got {2}x{3}", num, num2, m_decompression.Image_width, m_decompression.Image_height);
		}
		if (m_decompression.Image_width > num || m_decompression.Image_height > num2)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "JPEG strip/tile size exceeds expected dimensions, expected {0}x{1}, got {2}x{3}", num, num2, m_decompression.Image_width, m_decompression.Image_height);
			return false;
		}
		if (m_decompression.Num_components != ((dir.td_planarconfig != PlanarConfig.CONTIG) ? 1 : dir.td_samplesperpixel))
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Improper JPEG component count");
			return false;
		}
		if (m_decompression.Data_precision != dir.td_bitspersample)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Improper JPEG data precision");
			return false;
		}
		if (dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			if (m_decompression.Comp_info[0].H_samp_factor != m_h_sampling || m_decompression.Comp_info[0].V_samp_factor != m_v_sampling)
			{
				Tiff.WarningExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Improper JPEG sampling factors {0},{1}\nApparently should be {2},{3}.", m_decompression.Comp_info[0].H_samp_factor, m_decompression.Comp_info[0].V_samp_factor, m_h_sampling, m_v_sampling);
				if (m_tif.FindFieldInfo((TiffTag)33918, TiffType.NOTYPE) == null)
				{
					Tiff.WarningExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Decompressor will try reading with sampling {0},{1}.", m_decompression.Comp_info[0].H_samp_factor, m_decompression.Comp_info[0].V_samp_factor);
					m_h_sampling = m_decompression.Comp_info[0].H_samp_factor;
					m_v_sampling = m_decompression.Comp_info[0].V_samp_factor;
				}
			}
			for (int i = 1; i < m_decompression.Num_components; i++)
			{
				if (m_decompression.Comp_info[i].H_samp_factor != 1 || m_decompression.Comp_info[i].V_samp_factor != 1)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Improper JPEG sampling factors");
					return false;
				}
			}
		}
		else if (m_decompression.Comp_info[0].H_samp_factor != 1 || m_decompression.Comp_info[0].V_samp_factor != 1)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGPreDecode", "Improper JPEG sampling factors");
			return false;
		}
		bool flag = false;
		if (dir.td_planarconfig == PlanarConfig.CONTIG && m_photometric == Photometric.YCBCR && m_jpegcolormode == JpegColorMode.RGB)
		{
			m_decompression.Jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
			m_decompression.Out_color_space = J_COLOR_SPACE.JCS_RGB;
		}
		else
		{
			m_decompression.Jpeg_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
			m_decompression.Out_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
			if (dir.td_planarconfig == PlanarConfig.CONTIG && (m_h_sampling != 1 || m_v_sampling != 1))
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_decompression.Raw_data_out = true;
			m_rawDecode = true;
		}
		else
		{
			m_decompression.Raw_data_out = false;
			m_rawDecode = false;
		}
		if (!TIFFjpeg_start_decompress())
		{
			return false;
		}
		if (flag)
		{
			if (!alloc_downsampled_buffers(m_decompression.Comp_info, m_decompression.Num_components))
			{
				return false;
			}
			m_scancount = 8;
		}
		return true;
	}

	private bool prepare_JPEGTables()
	{
		InitializeLibJPEG(force_encode: false, force_decode: false);
		if (!TIFFjpeg_set_quality(m_jpegquality, force_baseline: false))
		{
			return false;
		}
		if (!TIFFjpeg_suppress_tables(suppress: true))
		{
			return false;
		}
		if ((m_jpegtablesmode & JpegTablesMode.QUANT) != 0)
		{
			unsuppress_quant_table(0);
			if (m_photometric == Photometric.YCBCR)
			{
				unsuppress_quant_table(1);
			}
		}
		if ((m_jpegtablesmode & JpegTablesMode.HUFF) != 0)
		{
			unsuppress_huff_table(0);
			if (m_photometric == Photometric.YCBCR)
			{
				unsuppress_huff_table(1);
			}
		}
		if (!TIFFjpeg_tables_dest())
		{
			return false;
		}
		if (!TIFFjpeg_write_tables())
		{
			return false;
		}
		return true;
	}

	private bool JPEGSetupDecode()
	{
		TiffDirectory dir = m_tif.m_dir;
		InitializeLibJPEG(force_encode: false, force_decode: true);
		if (m_tif.fieldSet(66))
		{
			m_decompression.Src = new JpegTablesSource(this);
			if (TIFFjpeg_read_header(require_image: false) != ReadResult.JPEG_HEADER_TABLES_ONLY)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupDecode", "Bogus JPEGTables field");
				return false;
			}
		}
		m_photometric = dir.td_photometric;
		if (m_photometric == Photometric.YCBCR)
		{
			m_h_sampling = dir.td_ycbcrsubsampling[0];
			m_v_sampling = dir.td_ycbcrsubsampling[1];
		}
		else
		{
			m_h_sampling = 1;
			m_v_sampling = 1;
		}
		m_decompression.Src = new JpegStdSource(this);
		m_tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmNone;
		return true;
	}

	private int TIFFjpeg_read_scanlines(byte[][] scanlines, int max_lines)
	{
		int num = 0;
		try
		{
			return m_decompression.jpeg_read_scanlines(scanlines, max_lines);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private bool JPEGDecode(byte[] buffer, int offset, int count, short plane)
	{
		int num = count / m_bytesperline;
		if (count % m_bytesperline != 0)
		{
			Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "fractional scanline not read");
		}
		if (num > m_decompression.Image_height)
		{
			num = m_decompression.Image_height;
		}
		if (num != 0)
		{
			byte[][] array = new byte[1][] { new byte[m_bytesperline] };
			do
			{
				Array.Clear(array[0], 0, m_bytesperline);
				if (TIFFjpeg_read_scanlines(array, 1) != 1)
				{
					return false;
				}
				m_tif.m_row++;
				Buffer.BlockCopy(array[0], 0, buffer, offset, m_bytesperline);
				offset += m_bytesperline;
				count -= m_bytesperline;
			}
			while (--num > 0);
		}
		if (m_decompression.Output_scanline >= m_decompression.Output_height)
		{
			return TIFFjpeg_finish_decompress();
		}
		return true;
	}

	private bool JPEGDecodeRaw(byte[] buffer, int offset, int count, short plane)
	{
		int num = m_decompression.Image_height;
		if (num != 0)
		{
			int downsampled_width = m_decompression.Comp_info[1].Downsampled_width;
			do
			{
				if (m_scancount >= 8)
				{
					int num2 = m_decompression.Max_v_samp_factor * 8;
					if (TIFFjpeg_read_raw_data(m_ds_buffer, num2) != num2)
					{
						return false;
					}
					m_scancount = 0;
				}
				int num3 = 0;
				for (int i = 0; i < m_decompression.Num_components; i++)
				{
					int h_samp_factor = m_decompression.Comp_info[i].H_samp_factor;
					int v_samp_factor = m_decompression.Comp_info[i].V_samp_factor;
					for (int j = 0; j < v_samp_factor; j++)
					{
						byte[] array = m_ds_buffer[i][m_scancount * v_samp_factor + j];
						int num4 = 0;
						int num5 = offset + num3;
						if (num5 >= buffer.Length)
						{
							break;
						}
						if (h_samp_factor == 1)
						{
							int num6 = downsampled_width;
							while (num6-- > 0)
							{
								buffer[num5] = array[num4];
								num4++;
								num5 += m_samplesperclump;
							}
						}
						else
						{
							int num7 = downsampled_width;
							while (num7-- > 0)
							{
								for (int k = 0; k < h_samp_factor; k++)
								{
									buffer[num5 + k] = array[num4];
									num4++;
								}
								num5 += m_samplesperclump;
							}
						}
						num3 += h_samp_factor;
					}
				}
				m_scancount++;
				m_tif.m_row += m_v_sampling;
				offset += m_bytesperline;
				count -= m_bytesperline;
				num -= m_v_sampling;
			}
			while (num > 0);
		}
		if (m_decompression.Output_scanline >= m_decompression.Output_height)
		{
			return TIFFjpeg_finish_decompress();
		}
		return true;
	}

	private bool JPEGEncode(byte[] buffer, int offset, int count, short plane)
	{
		int num = count / m_bytesperline;
		if (count % m_bytesperline != 0)
		{
			Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "fractional scanline discarded");
		}
		if (!m_tif.IsTiled() && m_tif.m_row + num > m_tif.m_dir.td_imagelength)
		{
			num = m_tif.m_dir.td_imagelength - m_tif.m_row;
		}
		byte[][] array = new byte[1][] { new byte[m_bytesperline] };
		while (num-- > 0)
		{
			Buffer.BlockCopy(buffer, offset, array[0], 0, m_bytesperline);
			if (TIFFjpeg_write_scanlines(array, 1) != 1)
			{
				return false;
			}
			if (num > 0)
			{
				m_tif.m_row++;
			}
			offset += m_bytesperline;
		}
		return true;
	}

	private bool JPEGEncodeRaw(byte[] buffer, int offset, int count, short plane)
	{
		int num = ((m_compression.Image_width + m_h_sampling - 1) / m_h_sampling * (m_h_sampling * m_v_sampling + 2) * m_compression.Data_precision + 7) / 8;
		int num2 = count / num * m_v_sampling;
		if (count % num != 0)
		{
			Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "fractional scanline discarded");
		}
		int downsampled_width = m_compression.Component_info[1].Downsampled_width;
		while (num2 > 0)
		{
			int num3 = 0;
			for (int i = 0; i < m_compression.Num_components; i++)
			{
				jpeg_component_info obj = m_compression.Component_info[i];
				int h_samp_factor = obj.H_samp_factor;
				int v_samp_factor = obj.V_samp_factor;
				int num4 = obj.Width_in_blocks * 8 - downsampled_width * h_samp_factor;
				for (int j = 0; j < v_samp_factor; j++)
				{
					int num5 = offset + num3;
					byte[] array = m_ds_buffer[i][m_scancount * v_samp_factor + j];
					int num6 = 0;
					if (h_samp_factor == 1)
					{
						int num7 = downsampled_width;
						while (num7-- > 0)
						{
							array[num6] = buffer[num5];
							num6++;
							num5 += m_samplesperclump;
						}
					}
					else
					{
						int num8 = downsampled_width;
						while (num8-- > 0)
						{
							for (int k = 0; k < h_samp_factor; k++)
							{
								array[num6] = buffer[num5 + k];
								num6++;
							}
							num5 += m_samplesperclump;
						}
					}
					for (int l = 0; l < num4; l++)
					{
						array[num6] = array[num6 - 1];
						num6++;
					}
					num3 += h_samp_factor;
				}
			}
			m_scancount++;
			if (m_scancount >= 8)
			{
				int num9 = m_compression.Max_v_samp_factor * 8;
				if (TIFFjpeg_write_raw_data(m_ds_buffer, num9) != num9)
				{
					return false;
				}
				m_scancount = 0;
			}
			m_tif.m_row += m_v_sampling;
			offset += m_bytesperline;
			num2 -= m_v_sampling;
		}
		return true;
	}

	private int JPEGDefaultStripSize(int s)
	{
		s = base.DefStripSize(s);
		if (s < m_tif.m_dir.td_imagelength)
		{
			s = Tiff.roundUp(s, m_tif.m_dir.td_ycbcrsubsampling[1] * 8);
		}
		return s;
	}

	private void JPEGDefaultTileSize(ref int tw, ref int th)
	{
		base.DefTileSize(ref tw, ref th);
		tw = Tiff.roundUp(tw, m_tif.m_dir.td_ycbcrsubsampling[0] * 8);
		th = Tiff.roundUp(th, m_tif.m_dir.td_ycbcrsubsampling[1] * 8);
	}

	private bool TIFFjpeg_create_compress()
	{
		try
		{
			m_compression = new jpeg_compress_struct(new JpegErrorManager(this));
			m_common = m_compression;
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_create_decompress()
	{
		try
		{
			m_decompression = new jpeg_decompress_struct(new JpegErrorManager(this));
			m_common = m_decompression;
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_set_defaults()
	{
		try
		{
			m_compression.jpeg_set_defaults();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_set_colorspace(J_COLOR_SPACE colorspace)
	{
		try
		{
			m_compression.jpeg_set_colorspace(colorspace);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_set_quality(int quality, bool force_baseline)
	{
		try
		{
			m_compression.jpeg_set_quality(quality, force_baseline);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_suppress_tables(bool suppress)
	{
		try
		{
			m_compression.jpeg_suppress_tables(suppress);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_start_compress(bool write_all_tables)
	{
		try
		{
			m_compression.jpeg_start_compress(write_all_tables);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private int TIFFjpeg_write_scanlines(byte[][] scanlines, int num_lines)
	{
		int num = 0;
		try
		{
			return m_compression.jpeg_write_scanlines(scanlines, num_lines);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private int TIFFjpeg_write_raw_data(byte[][][] data, int num_lines)
	{
		int num = 0;
		try
		{
			return m_compression.jpeg_write_raw_data(data, num_lines);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private bool TIFFjpeg_finish_compress()
	{
		try
		{
			m_compression.jpeg_finish_compress();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_write_tables()
	{
		try
		{
			m_compression.jpeg_write_tables();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private ReadResult TIFFjpeg_read_header(bool require_image)
	{
		ReadResult readResult = ReadResult.JPEG_SUSPENDED;
		try
		{
			return m_decompression.jpeg_read_header(require_image);
		}
		catch (Exception)
		{
			return ReadResult.JPEG_SUSPENDED;
		}
	}

	private bool TIFFjpeg_start_decompress()
	{
		try
		{
			m_decompression.jpeg_start_decompress();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private int TIFFjpeg_read_raw_data(byte[][][] data, int max_lines)
	{
		int num = 0;
		try
		{
			return m_decompression.jpeg_read_raw_data(data, max_lines);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private bool TIFFjpeg_finish_decompress()
	{
		bool flag = true;
		try
		{
			return m_decompression.jpeg_finish_decompress();
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool TIFFjpeg_abort()
	{
		try
		{
			m_common.jpeg_abort();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool TIFFjpeg_destroy()
	{
		try
		{
			m_common.jpeg_destroy();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private static byte[][] TIFFjpeg_alloc_sarray(int samplesperrow, int numrows)
	{
		byte[][] array = new byte[numrows][];
		for (int i = 0; i < numrows; i++)
		{
			array[i] = new byte[samplesperrow];
		}
		return array;
	}

	private bool alloc_downsampled_buffers(jpeg_component_info[] comp_info, int num_components)
	{
		int num = 0;
		for (int i = 0; i < num_components; i++)
		{
			jpeg_component_info jpeg_component_info = comp_info[i];
			num += jpeg_component_info.H_samp_factor * jpeg_component_info.V_samp_factor;
			byte[][] array = TIFFjpeg_alloc_sarray(jpeg_component_info.Width_in_blocks * 8, jpeg_component_info.V_samp_factor * 8);
			m_ds_buffer[i] = array;
		}
		m_samplesperclump = num;
		return true;
	}

	private void unsuppress_quant_table(int tblno)
	{
		JQUANT_TBL jQUANT_TBL = m_compression.Quant_tbl_ptrs[tblno];
		if (jQUANT_TBL != null)
		{
			jQUANT_TBL.Sent_table = false;
		}
	}

	private void unsuppress_huff_table(int tblno)
	{
		JHUFF_TBL jHUFF_TBL = m_compression.Dc_huff_tbl_ptrs[tblno];
		if (jHUFF_TBL != null)
		{
			jHUFF_TBL.Sent_table = false;
		}
		jHUFF_TBL = m_compression.Ac_huff_tbl_ptrs[tblno];
		if (jHUFF_TBL != null)
		{
			jHUFF_TBL.Sent_table = false;
		}
	}

	private void TIFFjpeg_data_dest()
	{
		m_compression.Dest = new JpegStdDestination(m_tif);
	}

	private bool TIFFjpeg_tables_dest()
	{
		m_jpegtables_length = 1000;
		m_jpegtables = new byte[m_jpegtables_length];
		m_compression.Dest = new JpegTablesDestination(this);
		return true;
	}
}
