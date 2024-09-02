using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

internal class FKPStructure : IDataStructure
{
	internal const int DEF_RECORD_SIZE = 512;

	private byte[] m_arrPageData = new byte[511];

	private byte m_btLength;

	internal byte[] PageData => m_arrPageData;

	internal byte Count
	{
		get
		{
			return m_btLength;
		}
		set
		{
			if (m_btLength != value)
			{
				m_btLength = value;
			}
		}
	}

	public int Length => 512;

	internal FKPStructure()
	{
	}

	internal FKPStructure(Stream stream)
	{
		stream.Read(m_arrPageData, 0, 511);
		m_btLength = (byte)stream.ReadByte();
	}

	public void Parse(byte[] arrData, int iOffset)
	{
		m_arrPageData = ByteConverter.ReadBytes(arrData, m_arrPageData.Length, ref iOffset);
		m_btLength = arrData[iOffset];
		iOffset++;
	}

	public int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset + 512 > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		m_arrPageData.CopyTo(arrData, iOffset);
		iOffset += m_arrPageData.Length;
		arrData[iOffset] = m_btLength;
		return ++iOffset;
	}

	internal int Save(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		stream.Write(m_arrPageData, 0, m_arrPageData.Length);
		stream.WriteByte(m_btLength);
		return (int)stream.Position;
	}

	internal void Close()
	{
		if (m_arrPageData != null)
		{
			m_arrPageData = null;
		}
	}
}
