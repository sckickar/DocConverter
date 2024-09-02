using System;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class PackBitsCodec : TiffCodec
{
	private enum EncodingState
	{
		BASE,
		LITERAL,
		RUN,
		LITERAL_RUN
	}

	private int m_rowsize;

	public override bool CanEncode => true;

	public override bool CanDecode => true;

	public PackBitsCodec(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
	}

	public override bool Init()
	{
		return true;
	}

	public override bool DecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return PackBitsDecode(buffer, offset, count, plane);
	}

	public override bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return PackBitsDecode(buffer, offset, count, plane);
	}

	public override bool DecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return PackBitsDecode(buffer, offset, count, plane);
	}

	public override bool PreEncode(short plane)
	{
		return PackBitsPreEncode(plane);
	}

	public override bool EncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return PackBitsEncode(buffer, offset, count, plane);
	}

	public override bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return PackBitsEncodeChunk(buffer, offset, count, plane);
	}

	public override bool EncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return PackBitsEncodeChunk(buffer, offset, count, plane);
	}

	private bool PackBitsPreEncode(short s)
	{
		if (m_tif.IsTiled())
		{
			m_rowsize = m_tif.TileRowSize();
		}
		else
		{
			m_rowsize = m_tif.ScanlineSize();
		}
		return true;
	}

	private bool PackBitsEncode(byte[] buf, int offset, int cc, short s)
	{
		int num = m_tif.m_rawcp;
		EncodingState encodingState = EncodingState.BASE;
		int num2 = 0;
		int num3 = offset;
		while (cc > 0)
		{
			int num4 = buf[num3];
			num3++;
			cc--;
			int num5 = 1;
			while (cc > 0 && num4 == buf[num3])
			{
				num5++;
				cc--;
				num3++;
			}
			bool flag = false;
			while (!flag)
			{
				if (num + 2 >= m_tif.m_rawdatasize)
				{
					if (encodingState == EncodingState.LITERAL || encodingState == EncodingState.LITERAL_RUN)
					{
						int num6 = num - num2;
						m_tif.m_rawcc += num2 - m_tif.m_rawcp;
						if (!m_tif.flushData1())
						{
							return false;
						}
						num = m_tif.m_rawcp;
						while (num6-- > 0)
						{
							m_tif.m_rawdata[num] = m_tif.m_rawdata[num2];
							num2++;
							num++;
						}
						num2 = m_tif.m_rawcp;
					}
					else
					{
						m_tif.m_rawcc += num - m_tif.m_rawcp;
						if (!m_tif.flushData1())
						{
							return false;
						}
						num = m_tif.m_rawcp;
					}
				}
				switch (encodingState)
				{
				case EncodingState.BASE:
					if (num5 > 1)
					{
						encodingState = EncodingState.RUN;
						if (num5 > 128)
						{
							int num10 = -127;
							m_tif.m_rawdata[num] = (byte)num10;
							num++;
							m_tif.m_rawdata[num] = (byte)num4;
							num++;
							num5 -= 128;
							break;
						}
						m_tif.m_rawdata[num] = (byte)(-num5 + 1);
						num++;
						m_tif.m_rawdata[num] = (byte)num4;
						num++;
					}
					else
					{
						num2 = num;
						m_tif.m_rawdata[num] = 0;
						num++;
						m_tif.m_rawdata[num] = (byte)num4;
						num++;
						encodingState = EncodingState.LITERAL;
					}
					flag = true;
					break;
				case EncodingState.LITERAL:
					if (num5 > 1)
					{
						encodingState = EncodingState.LITERAL_RUN;
						if (num5 > 128)
						{
							int num8 = -127;
							m_tif.m_rawdata[num] = (byte)num8;
							num++;
							m_tif.m_rawdata[num] = (byte)num4;
							num++;
							num5 -= 128;
							break;
						}
						m_tif.m_rawdata[num] = (byte)(-num5 + 1);
						num++;
						m_tif.m_rawdata[num] = (byte)num4;
						num++;
					}
					else
					{
						m_tif.m_rawdata[num2]++;
						if (m_tif.m_rawdata[num2] == 127)
						{
							encodingState = EncodingState.BASE;
						}
						m_tif.m_rawdata[num] = (byte)num4;
						num++;
					}
					flag = true;
					break;
				case EncodingState.RUN:
					if (num5 > 1)
					{
						if (num5 > 128)
						{
							int num9 = -127;
							m_tif.m_rawdata[num] = (byte)num9;
							num++;
							m_tif.m_rawdata[num] = (byte)num4;
							num++;
							num5 -= 128;
							break;
						}
						m_tif.m_rawdata[num] = (byte)(-num5 + 1);
						num++;
						m_tif.m_rawdata[num] = (byte)num4;
						num++;
					}
					else
					{
						num2 = num;
						m_tif.m_rawdata[num] = 0;
						num++;
						m_tif.m_rawdata[num] = (byte)num4;
						num++;
						encodingState = EncodingState.LITERAL;
					}
					flag = true;
					break;
				case EncodingState.LITERAL_RUN:
				{
					int num7 = -1;
					if (num5 == 1 && m_tif.m_rawdata[num - 2] == (byte)num7 && m_tif.m_rawdata[num2] < 126)
					{
						m_tif.m_rawdata[num2] += 2;
						encodingState = ((m_tif.m_rawdata[num2] != 127) ? EncodingState.LITERAL : EncodingState.BASE);
						m_tif.m_rawdata[num - 2] = m_tif.m_rawdata[num - 1];
					}
					else
					{
						encodingState = EncodingState.RUN;
					}
					break;
				}
				}
			}
		}
		m_tif.m_rawcc += num - m_tif.m_rawcp;
		m_tif.m_rawcp = num;
		return true;
	}

	private bool PackBitsEncodeChunk(byte[] buffer, int offset, int count, short plane)
	{
		while (count > 0)
		{
			int num = m_rowsize;
			if (count < num)
			{
				num = count;
			}
			if (!PackBitsEncode(buffer, offset, num, plane))
			{
				return false;
			}
			offset += num;
			count -= num;
		}
		return true;
	}

	private bool PackBitsDecode(byte[] buffer, int offset, int count, short plane)
	{
		int num = m_tif.m_rawcp;
		int num2 = m_tif.m_rawcc;
		while (num2 > 0 && count > 0)
		{
			int num3 = m_tif.m_rawdata[num];
			num++;
			num2--;
			if (num3 >= 128)
			{
				num3 -= 256;
			}
			if (num3 < 0)
			{
				if (num3 != -128)
				{
					num3 = -num3 + 1;
					if (count < num3)
					{
						Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "PackBitsDecode: discarding {0} bytes to avoid buffer overrun", num3 - count);
						num3 = count;
					}
					count -= num3;
					int num4 = m_tif.m_rawdata[num];
					num++;
					num2--;
					while (num3-- > 0)
					{
						buffer[offset] = (byte)num4;
						offset++;
					}
				}
			}
			else
			{
				if (count < num3 + 1)
				{
					Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "PackBitsDecode: discarding {0} bytes to avoid buffer overrun", num3 - count + 1);
					num3 = count - 1;
				}
				Buffer.BlockCopy(m_tif.m_rawdata, num, buffer, offset, ++num3);
				offset += num3;
				count -= num3;
				num += num3;
				num2 -= num3;
			}
		}
		m_tif.m_rawcp = num;
		m_tif.m_rawcc = num2;
		if (count > 0)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "PackBitsDecode: Not enough data for scanline {0}", m_tif.m_row);
			return false;
		}
		return true;
	}
}
