using ComponentAce.Compression.Libs.zlib;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class DeflateCodec : CodecWithPredictor
{
	public const int ZSTATE_INIT_DECODE = 1;

	public const int ZSTATE_INIT_ENCODE = 2;

	public ZStream m_stream = new ZStream();

	public int m_zipquality;

	public int m_state;

	private static readonly TiffFieldInfo[] zipFieldInfo = new TiffFieldInfo[1]
	{
		new TiffFieldInfo(TiffTag.ZIPQUALITY, 0, 0, TiffType.NOTYPE, 0, okToChange: true, passCount: false, string.Empty)
	};

	private TiffTagMethods m_tagMethods;

	public override bool CanEncode => true;

	public override bool CanDecode => true;

	public DeflateCodec(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
		m_tagMethods = new DeflateCodecTagMethods();
	}

	public override bool Init()
	{
		m_tif.MergeFieldInfo(zipFieldInfo, zipFieldInfo.Length);
		m_zipquality = -1;
		m_state = 0;
		TIFFPredictorInit(m_tagMethods);
		return true;
	}

	public override bool PreDecode(short plane)
	{
		return ZIPPreDecode(plane);
	}

	public override bool PreEncode(short plane)
	{
		return ZIPPreEncode(plane);
	}

	public override bool PostEncode()
	{
		return ZIPPostEncode();
	}

	public override void Cleanup()
	{
		ZIPCleanup();
	}

	public override bool predictor_setupdecode()
	{
		return ZIPSetupDecode();
	}

	public override bool predictor_decoderow(byte[] buffer, int offset, int count, short plane)
	{
		return ZIPDecode(buffer, offset, count, plane);
	}

	public override bool predictor_decodestrip(byte[] buffer, int offset, int count, short plane)
	{
		return ZIPDecode(buffer, offset, count, plane);
	}

	public override bool predictor_decodetile(byte[] buffer, int offset, int count, short plane)
	{
		return ZIPDecode(buffer, offset, count, plane);
	}

	public override bool predictor_setupencode()
	{
		return ZIPSetupEncode();
	}

	public override bool predictor_encoderow(byte[] buffer, int offset, int count, short plane)
	{
		return ZIPEncode(buffer, offset, count, plane);
	}

	public override bool predictor_encodestrip(byte[] buffer, int offset, int count, short plane)
	{
		return ZIPEncode(buffer, offset, count, plane);
	}

	public override bool predictor_encodetile(byte[] buffer, int offset, int count, short plane)
	{
		return ZIPEncode(buffer, offset, count, plane);
	}

	private void ZIPCleanup()
	{
		TIFFPredictorCleanup();
		if (((uint)m_state & 2u) != 0)
		{
			m_stream.deflateEnd();
			m_state = 0;
		}
		else if (((uint)m_state & (true ? 1u : 0u)) != 0)
		{
			m_stream.inflateEnd();
			m_state = 0;
		}
	}

	private bool ZIPDecode(byte[] buffer, int offset, int count, short plane)
	{
		m_stream.next_out = buffer;
		m_stream.next_out_index = offset;
		m_stream.avail_out = count;
		do
		{
			switch (m_stream.inflate(1))
			{
			case -3:
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPDecode", "{0}: Decoding error at scanline {1}, {2}", m_tif.m_name, m_tif.m_row, m_stream.msg);
				if (m_stream.inflateSync() != 0)
				{
					return false;
				}
				continue;
			default:
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPDecode", "{0}: zlib error: {1}", m_tif.m_name, m_stream.msg);
				return false;
			case 0:
				continue;
			case 1:
				break;
			}
			break;
		}
		while (m_stream.avail_out > 0);
		if (m_stream.avail_out != 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPDecode", "{0}: Not enough data at scanline {1} (short {2} bytes)", m_tif.m_name, m_tif.m_row, m_stream.avail_out);
			return false;
		}
		return true;
	}

	private bool ZIPEncode(byte[] buffer, int offset, int count, short plane)
	{
		m_stream.next_in = buffer;
		m_stream.next_in_index = offset;
		m_stream.avail_in = count;
		do
		{
			if (m_stream.deflate(0) != 0)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPEncode", "{0}: Encoder error: {1}", m_tif.m_name, m_stream.msg);
				return false;
			}
			if (m_stream.avail_out == 0)
			{
				m_tif.m_rawcc = m_tif.m_rawdatasize;
				m_tif.flushData1();
				m_stream.next_out = m_tif.m_rawdata;
				m_stream.next_out_index = 0;
				m_stream.avail_out = m_tif.m_rawdatasize;
			}
		}
		while (m_stream.avail_in > 0);
		return true;
	}

	private bool ZIPPostEncode()
	{
		m_stream.avail_in = 0;
		int num;
		do
		{
			num = m_stream.deflate(4);
			if ((uint)num <= 1u)
			{
				if (m_stream.avail_out != m_tif.m_rawdatasize)
				{
					m_tif.m_rawcc = m_tif.m_rawdatasize - m_stream.avail_out;
					m_tif.flushData1();
					m_stream.next_out = m_tif.m_rawdata;
					m_stream.next_out_index = 0;
					m_stream.avail_out = m_tif.m_rawdatasize;
				}
				continue;
			}
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPPostEncode", "{0}: zlib error: {1}", m_tif.m_name, m_stream.msg);
			return false;
		}
		while (num != 1);
		return true;
	}

	private bool ZIPPreDecode(short s)
	{
		if ((m_state & 1) == 0)
		{
			SetupDecode();
		}
		m_stream.next_in = m_tif.m_rawdata;
		m_stream.next_in_index = 0;
		m_stream.avail_in = m_tif.m_rawcc;
		return m_stream.inflateInit() == 0;
	}

	private bool ZIPPreEncode(short s)
	{
		if (m_state != 2)
		{
			SetupEncode();
		}
		m_stream.next_out = m_tif.m_rawdata;
		m_stream.next_out_index = 0;
		m_stream.avail_out = m_tif.m_rawdatasize;
		return m_stream.deflateInit(m_zipquality) == 0;
	}

	private bool ZIPSetupDecode()
	{
		if (((uint)m_state & 2u) != 0)
		{
			m_stream.deflateEnd();
			m_state = 0;
		}
		if (m_stream.inflateInit() != 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPSetupDecode", "{0}: {1}", m_tif.m_name, m_stream.msg);
			return false;
		}
		m_state |= 1;
		return true;
	}

	private bool ZIPSetupEncode()
	{
		if (((uint)m_state & (true ? 1u : 0u)) != 0)
		{
			m_stream.inflateEnd();
			m_state = 0;
		}
		if (m_stream.deflateInit(m_zipquality) != 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "ZIPSetupEncode", "{0}: {1}", m_tif.m_name, m_stream.msg);
			return false;
		}
		m_state |= 2;
		return true;
	}
}
