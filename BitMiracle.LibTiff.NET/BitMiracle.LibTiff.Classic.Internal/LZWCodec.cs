using System;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class LZWCodec : CodecWithPredictor
{
	private struct code_t
	{
		public int next;

		public short length;

		public byte value;

		public byte firstchar;
	}

	private struct hash_t
	{
		public int hash;

		public short code;
	}

	private bool LZW_CHECKEOS = true;

	private const short BITS_MIN = 9;

	private const short BITS_MAX = 12;

	private const short CODE_CLEAR = 256;

	private const short CODE_EOI = 257;

	private const short CODE_FIRST = 258;

	private const short CODE_MAX = 4095;

	private const short CODE_MIN = 511;

	private const int HSIZE = 9001;

	private const int HSHIFT = 5;

	private const int CSIZE = 5119;

	private const int CHECK_GAP = 10000;

	private bool m_compatDecode;

	private short m_nbits;

	private short m_maxcode;

	private short m_free_ent;

	private int m_nextdata;

	private int m_nextbits;

	private int m_rw_mode;

	private int m_dec_nbitsmask;

	private int m_dec_restart;

	private long m_dec_bitsleft;

	private bool m_oldStyleCodeFound;

	private int m_dec_codep;

	private int m_dec_oldcodep;

	private int m_dec_free_entp;

	private int m_dec_maxcodep;

	private code_t[] m_dec_codetab;

	private int m_enc_oldcode;

	private int m_enc_checkpoint;

	private int m_enc_ratio;

	private int m_enc_incount;

	private int m_enc_outcount;

	private int m_enc_rawlimit;

	private hash_t[] m_enc_hashtab;

	public override bool CanEncode => true;

	public override bool CanDecode => true;

	public LZWCodec(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
	}

	public override bool Init()
	{
		m_dec_codetab = null;
		m_oldStyleCodeFound = false;
		m_enc_hashtab = null;
		m_rw_mode = m_tif.m_mode;
		m_compatDecode = false;
		TIFFPredictorInit(null);
		return true;
	}

	public override bool PreDecode(short plane)
	{
		return LZWPreDecode(plane);
	}

	public override bool PreEncode(short plane)
	{
		return LZWPreEncode(plane);
	}

	public override bool PostEncode()
	{
		return LZWPostEncode();
	}

	public override void Cleanup()
	{
		LZWCleanup();
		m_tif.m_mode = m_rw_mode;
	}

	public override bool predictor_setupdecode()
	{
		return LZWSetupDecode();
	}

	public override bool predictor_decoderow(byte[] buffer, int offset, int count, short plane)
	{
		if (m_compatDecode)
		{
			return LZWDecodeCompat(buffer, offset, count);
		}
		return LZWDecode(buffer, offset, count);
	}

	public override bool predictor_decodestrip(byte[] buffer, int offset, int count, short plane)
	{
		if (m_compatDecode)
		{
			return LZWDecodeCompat(buffer, offset, count);
		}
		return LZWDecode(buffer, offset, count);
	}

	public override bool predictor_decodetile(byte[] buffer, int offset, int count, short plane)
	{
		if (m_compatDecode)
		{
			return LZWDecodeCompat(buffer, offset, count);
		}
		return LZWDecode(buffer, offset, count);
	}

	public override bool predictor_setupencode()
	{
		return LZWSetupEncode();
	}

	public override bool predictor_encoderow(byte[] buffer, int offset, int count, short plane)
	{
		return LZWEncode(buffer, offset, count, plane);
	}

	public override bool predictor_encodestrip(byte[] buffer, int offset, int count, short plane)
	{
		return LZWEncode(buffer, offset, count, plane);
	}

	public override bool predictor_encodetile(byte[] buffer, int offset, int count, short plane)
	{
		return LZWEncode(buffer, offset, count, plane);
	}

	private bool LZWSetupDecode()
	{
		if (m_dec_codetab == null)
		{
			m_dec_codetab = new code_t[5119];
			int num = 255;
			do
			{
				m_dec_codetab[num].value = (byte)num;
				m_dec_codetab[num].firstchar = (byte)num;
				m_dec_codetab[num].length = 1;
				m_dec_codetab[num].next = -1;
			}
			while (num-- != 0);
			Array.Clear(m_dec_codetab, 256, 2);
		}
		return true;
	}

	private bool LZWPreDecode(short s)
	{
		if (m_dec_codetab == null)
		{
			SetupDecode();
		}
		if (m_tif.m_rawdata[0] == 0 && ((uint)m_tif.m_rawdata[1] & (true ? 1u : 0u)) != 0)
		{
			if (!m_oldStyleCodeFound)
			{
				Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "Old-style LZW codes, convert file.");
				m_compatDecode = true;
				SetupDecode();
				m_oldStyleCodeFound = true;
			}
			m_maxcode = 511;
		}
		else
		{
			m_maxcode = 510;
			m_oldStyleCodeFound = false;
		}
		m_nbits = 9;
		m_nextbits = 0;
		m_nextdata = 0;
		m_dec_restart = 0;
		m_dec_nbitsmask = 511;
		m_dec_bitsleft = (long)m_tif.m_rawcc << 3;
		m_dec_free_entp = 258;
		Array.Clear(m_dec_codetab, m_dec_free_entp, 4861);
		m_dec_oldcodep = -1;
		m_dec_maxcodep = m_dec_nbitsmask - 1;
		return true;
	}

	private bool LZWDecode(byte[] buffer, int offset, int count)
	{
		return LZWDecodeImpl(buffer, offset, count, compat: false, "LZWDecode");
	}

	private bool LZWDecodeCompat(byte[] buffer, int offset, int count)
	{
		return LZWDecodeImpl(buffer, offset, count, compat: true, "LZWDecodeCompat");
	}

	private bool LZWDecodeImpl(byte[] buffer, int offset, int count, bool compat, string callerName)
	{
		if (m_dec_restart != 0)
		{
			int num = m_dec_codep;
			int num2 = m_dec_codetab[num].length - m_dec_restart;
			if (num2 > count)
			{
				m_dec_restart += count;
				do
				{
					num = m_dec_codetab[num].next;
				}
				while (--num2 > count && num != -1);
				if (compat || num != -1)
				{
					int num3 = count;
					do
					{
						num3--;
						buffer[offset + num3] = m_dec_codetab[num].value;
						num = m_dec_codetab[num].next;
					}
					while (--count != 0 && (compat || num != -1));
				}
				return true;
			}
			offset += num2;
			count -= num2;
			int num4 = 0;
			do
			{
				num4--;
				buffer[offset + num4] = m_dec_codetab[num].value;
				num = m_dec_codetab[num].next;
			}
			while (--num2 != 0 && (compat || num != -1));
			m_dec_restart = 0;
		}
		short _code = 0;
		while (count > 0)
		{
			NextCode(out _code, compat);
			if (_code == 257)
			{
				break;
			}
			if (_code == 256)
			{
				do
				{
					m_dec_free_entp = 258;
					Array.Clear(m_dec_codetab, m_dec_free_entp, 4861);
					m_nbits = 9;
					m_dec_nbitsmask = 511;
					m_dec_maxcodep = m_dec_nbitsmask - ((!compat) ? 1 : 0);
					NextCode(out _code, compat);
				}
				while (_code == 256);
				if (_code == 257)
				{
					break;
				}
				if (_code > 256)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, callerName + ": Corrupted LZW table at scanline {0}", m_tif.m_row);
					return false;
				}
				buffer[offset] = (byte)_code;
				offset++;
				count--;
				m_dec_oldcodep = _code;
				continue;
			}
			int num5 = _code;
			if (m_dec_free_entp < 0 || m_dec_free_entp >= 5119)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, callerName + ": Corrupted LZW table at scanline {0}", m_tif.m_row);
				return false;
			}
			m_dec_codetab[m_dec_free_entp].next = m_dec_oldcodep;
			if (m_dec_codetab[m_dec_free_entp].next < 0 || m_dec_codetab[m_dec_free_entp].next >= 5119)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, callerName + ": Corrupted LZW table at scanline {0}", m_tif.m_row);
				return false;
			}
			m_dec_codetab[m_dec_free_entp].firstchar = m_dec_codetab[m_dec_codetab[m_dec_free_entp].next].firstchar;
			m_dec_codetab[m_dec_free_entp].length = (short)(m_dec_codetab[m_dec_codetab[m_dec_free_entp].next].length + 1);
			m_dec_codetab[m_dec_free_entp].value = ((num5 < m_dec_free_entp) ? m_dec_codetab[num5].firstchar : m_dec_codetab[m_dec_free_entp].firstchar);
			if (++m_dec_free_entp > m_dec_maxcodep)
			{
				if (++m_nbits > 12)
				{
					m_nbits = 12;
				}
				m_dec_nbitsmask = MAXCODE(m_nbits);
				m_dec_maxcodep = m_dec_nbitsmask - ((!compat) ? 1 : 0);
			}
			m_dec_oldcodep = _code;
			if (_code >= 256)
			{
				int num6 = offset;
				if (m_dec_codetab[num5].length == 0)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, callerName + ": Wrong length of decoded string: data probably corrupted at scanline {0}", m_tif.m_row);
					return false;
				}
				if (compat)
				{
					if (m_dec_codetab[num5].length > count)
					{
						m_dec_codep = _code;
						do
						{
							num5 = m_dec_codetab[num5].next;
						}
						while (m_dec_codetab[num5].length > count);
						m_dec_restart = count;
						int num7 = count;
						do
						{
							num7--;
							buffer[offset + num7] = m_dec_codetab[num5].value;
							num5 = m_dec_codetab[num5].next;
						}
						while (--count != 0);
						break;
					}
					offset += m_dec_codetab[num5].length;
					count -= m_dec_codetab[num5].length;
					int num8 = offset;
					do
					{
						num8--;
						buffer[num8] = m_dec_codetab[num5].value;
						num5 = m_dec_codetab[num5].next;
					}
					while (num5 != -1 && num8 > num6);
					continue;
				}
				if (m_dec_codetab[num5].length > count)
				{
					m_dec_codep = _code;
					do
					{
						num5 = m_dec_codetab[num5].next;
					}
					while (num5 != -1 && m_dec_codetab[num5].length > count);
					if (num5 != -1)
					{
						m_dec_restart = count;
						int num9 = count;
						do
						{
							num9--;
							buffer[offset + num9] = m_dec_codetab[num5].value;
							num5 = m_dec_codetab[num5].next;
						}
						while (--count != 0 && num5 != -1);
						if (num5 != -1)
						{
							codeLoop();
						}
					}
					break;
				}
				int length = m_dec_codetab[num5].length;
				int num10 = length;
				do
				{
					num10--;
					int value = m_dec_codetab[num5].value;
					num5 = m_dec_codetab[num5].next;
					buffer[offset + num10] = (byte)value;
				}
				while (num5 != -1 && num10 > 0);
				if (num5 != -1)
				{
					codeLoop();
					break;
				}
				offset += length;
				count -= length;
			}
			else
			{
				buffer[offset] = (byte)_code;
				offset++;
				count--;
			}
		}
		if (count > 0)
		{
			if (_code == 257)
			{
				Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, callerName + ": Not enough data at scanline {0} (short {1} bytes)", m_tif.m_row, count);
				return true;
			}
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, callerName + ": Not enough data at scanline {0} (short {1} bytes)", m_tif.m_row, count);
			return false;
		}
		return true;
	}

	private bool LZWSetupEncode()
	{
		m_enc_hashtab = new hash_t[9001];
		return true;
	}

	private bool LZWPreEncode(short s)
	{
		if (m_enc_hashtab == null)
		{
			SetupEncode();
		}
		m_nbits = 9;
		m_maxcode = 511;
		m_free_ent = 258;
		m_nextbits = 0;
		m_nextdata = 0;
		m_enc_checkpoint = 10000;
		m_enc_ratio = 0;
		m_enc_incount = 0;
		m_enc_outcount = 0;
		m_enc_rawlimit = m_tif.m_rawdatasize - 1 - 4;
		cl_hash();
		m_enc_oldcode = -1;
		return true;
	}

	private bool LZWPostEncode()
	{
		if (m_tif.m_rawcp > m_enc_rawlimit)
		{
			m_tif.m_rawcc = m_tif.m_rawcp;
			m_tif.flushData1();
			m_tif.m_rawcp = 0;
		}
		if (m_enc_oldcode != -1)
		{
			PutNextCode(m_enc_oldcode);
			m_enc_oldcode = -1;
		}
		PutNextCode(257);
		if (m_nextbits > 0)
		{
			m_tif.m_rawdata[m_tif.m_rawcp] = (byte)(m_nextdata << 8 - m_nextbits);
			m_tif.m_rawcp++;
		}
		m_tif.m_rawcc = m_tif.m_rawcp;
		return true;
	}

	private bool LZWEncode(byte[] buffer, int offset, int count, short plane)
	{
		if (m_enc_oldcode == -1 && count > 0)
		{
			PutNextCode(256);
			m_enc_oldcode = buffer[offset];
			offset++;
			count--;
			m_enc_incount++;
		}
		while (count > 0)
		{
			int num = buffer[offset];
			offset++;
			count--;
			m_enc_incount++;
			int num2 = (num << 12) + m_enc_oldcode;
			int num3 = (num << 5) ^ m_enc_oldcode;
			if (num3 >= 9001)
			{
				num3 -= 9001;
			}
			if (m_enc_hashtab[num3].hash == num2)
			{
				m_enc_oldcode = m_enc_hashtab[num3].code;
				continue;
			}
			bool flag = false;
			if (m_enc_hashtab[num3].hash >= 0)
			{
				int num4 = 9001 - num3;
				if (num3 == 0)
				{
					num4 = 1;
				}
				do
				{
					num3 -= num4;
					if (num3 < 0)
					{
						num3 += 9001;
					}
					if (m_enc_hashtab[num3].hash == num2)
					{
						m_enc_oldcode = m_enc_hashtab[num3].code;
						flag = true;
						break;
					}
				}
				while (m_enc_hashtab[num3].hash >= 0);
			}
			if (flag)
			{
				continue;
			}
			if (m_tif.m_rawcp > m_enc_rawlimit)
			{
				m_tif.m_rawcc = m_tif.m_rawcp;
				m_tif.flushData1();
				m_tif.m_rawcp = 0;
			}
			PutNextCode(m_enc_oldcode);
			m_enc_oldcode = num;
			m_enc_hashtab[num3].code = m_free_ent;
			m_free_ent++;
			m_enc_hashtab[num3].hash = num2;
			if (m_free_ent == 4094)
			{
				cl_hash();
				m_enc_ratio = 0;
				m_enc_incount = 0;
				m_enc_outcount = 0;
				m_free_ent = 258;
				PutNextCode(256);
				m_nbits = 9;
				m_maxcode = 511;
			}
			else if (m_free_ent > m_maxcode)
			{
				m_nbits++;
				m_maxcode = (short)MAXCODE(m_nbits);
			}
			else if (m_enc_incount >= m_enc_checkpoint)
			{
				m_enc_checkpoint = m_enc_incount + 10000;
				int num5;
				if (m_enc_incount > 8388607)
				{
					num5 = m_enc_outcount >> 8;
					num5 = ((num5 == 0) ? int.MaxValue : (m_enc_incount / num5));
				}
				else
				{
					num5 = (m_enc_incount << 8) / m_enc_outcount;
				}
				if (num5 <= m_enc_ratio)
				{
					cl_hash();
					m_enc_ratio = 0;
					m_enc_incount = 0;
					m_enc_outcount = 0;
					m_free_ent = 258;
					PutNextCode(256);
					m_nbits = 9;
					m_maxcode = 511;
				}
				else
				{
					m_enc_ratio = num5;
				}
			}
		}
		return true;
	}

	private void LZWCleanup()
	{
		m_dec_codetab = null;
		m_enc_hashtab = null;
	}

	private static int MAXCODE(int n)
	{
		return (1 << n) - 1;
	}

	private void PutNextCode(int c)
	{
		m_nextdata = (m_nextdata << (int)m_nbits) | c;
		m_nextbits += m_nbits;
		m_tif.m_rawdata[m_tif.m_rawcp] = (byte)(m_nextdata >> m_nextbits - 8);
		m_tif.m_rawcp++;
		m_nextbits -= 8;
		if (m_nextbits >= 8)
		{
			m_tif.m_rawdata[m_tif.m_rawcp] = (byte)(m_nextdata >> m_nextbits - 8);
			m_tif.m_rawcp++;
			m_nextbits -= 8;
		}
		m_enc_outcount += m_nbits;
	}

	private void cl_hash()
	{
		int num = 9000;
		int num2 = 8993;
		do
		{
			num2 -= 8;
			m_enc_hashtab[num - 7].hash = -1;
			m_enc_hashtab[num - 6].hash = -1;
			m_enc_hashtab[num - 5].hash = -1;
			m_enc_hashtab[num - 4].hash = -1;
			m_enc_hashtab[num - 3].hash = -1;
			m_enc_hashtab[num - 2].hash = -1;
			m_enc_hashtab[num - 1].hash = -1;
			m_enc_hashtab[num].hash = -1;
			num -= 8;
		}
		while (num2 >= 0);
		num2 += 8;
		while (num2 > 0)
		{
			m_enc_hashtab[num].hash = -1;
			num2--;
			num--;
		}
	}

	private void NextCode(out short _code, bool compat)
	{
		if (LZW_CHECKEOS)
		{
			if (m_dec_bitsleft < m_nbits)
			{
				Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "LZWDecode: Strip {0} not terminated with EOI code", m_tif.m_curstrip);
				_code = 257;
				return;
			}
			if (compat)
			{
				GetNextCodeCompat(out _code);
			}
			else
			{
				GetNextCode(out _code);
			}
			m_dec_bitsleft -= m_nbits;
		}
		else if (compat)
		{
			GetNextCodeCompat(out _code);
		}
		else
		{
			GetNextCode(out _code);
		}
	}

	private void GetNextCode(out short code)
	{
		m_nextdata = (m_nextdata << 8) | m_tif.m_rawdata[m_tif.m_rawcp];
		m_tif.m_rawcp++;
		m_nextbits += 8;
		if (m_nextbits < m_nbits)
		{
			m_nextdata = (m_nextdata << 8) | m_tif.m_rawdata[m_tif.m_rawcp];
			m_tif.m_rawcp++;
			m_nextbits += 8;
		}
		code = (short)((m_nextdata >> m_nextbits - m_nbits) & m_dec_nbitsmask);
		m_nextbits -= m_nbits;
	}

	private void GetNextCodeCompat(out short code)
	{
		m_nextdata |= m_tif.m_rawdata[m_tif.m_rawcp] << m_nextbits;
		m_tif.m_rawcp++;
		m_nextbits += 8;
		if (m_nextbits < m_nbits)
		{
			m_nextdata |= m_tif.m_rawdata[m_tif.m_rawcp] << m_nextbits;
			m_tif.m_rawcp++;
			m_nextbits += 8;
		}
		code = (short)(m_nextdata & m_dec_nbitsmask);
		m_nextdata >>= (int)m_nbits;
		m_nextbits -= m_nbits;
	}

	private void codeLoop()
	{
		Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "LZWDecode: Bogus encoding, loop in the code table; scanline {0}", m_tif.m_row);
	}
}
