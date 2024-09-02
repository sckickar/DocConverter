using System;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class DumpModeCodec : TiffCodec
{
	public override bool CanEncode => true;

	public override bool CanDecode => true;

	public DumpModeCodec(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
	}

	public override bool Init()
	{
		return true;
	}

	public override bool DecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return DumpModeDecode(buffer, offset, count, plane);
	}

	public override bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return DumpModeDecode(buffer, offset, count, plane);
	}

	public override bool DecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return DumpModeDecode(buffer, offset, count, plane);
	}

	public override bool EncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return DumpModeEncode(buffer, offset, count, plane);
	}

	public override bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return DumpModeEncode(buffer, offset, count, plane);
	}

	public override bool EncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return DumpModeEncode(buffer, offset, count, plane);
	}

	public override bool Seek(int row)
	{
		m_tif.m_rawcp += row * m_tif.m_scanlinesize;
		m_tif.m_rawcc -= row * m_tif.m_scanlinesize;
		return true;
	}

	private bool DumpModeEncode(byte[] buffer, int offset, int count, short plane)
	{
		while (count > 0)
		{
			int num = count;
			if (m_tif.m_rawcc + num > m_tif.m_rawdatasize)
			{
				num = m_tif.m_rawdatasize - m_tif.m_rawcc;
			}
			Buffer.BlockCopy(buffer, offset, m_tif.m_rawdata, m_tif.m_rawcp, num);
			m_tif.m_rawcp += num;
			m_tif.m_rawcc += num;
			offset += num;
			count -= num;
			if (m_tif.m_rawcc >= m_tif.m_rawdatasize && !m_tif.flushData1())
			{
				return false;
			}
		}
		return true;
	}

	private bool DumpModeDecode(byte[] buffer, int offset, int count, short plane)
	{
		if (m_tif.m_rawcc < count)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "DumpModeDecode: Not enough data for scanline {0}", m_tif.m_row);
			return false;
		}
		Buffer.BlockCopy(m_tif.m_rawdata, m_tif.m_rawcp, buffer, offset, count);
		m_tif.m_rawcp += count;
		m_tif.m_rawcc -= count;
		return true;
	}
}
