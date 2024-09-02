using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class FFNBaseStructure : IDataStructure
{
	private const int DEF_RECORD_LENGTH = 40;

	private const int DEF_PANOSE_SIZE = 10;

	private const int DEF_FONTSIGNATURE_SIZE = 24;

	private byte m_btTotalLength;

	private byte m_btOptions;

	private short m_wWeight;

	private byte m_btCharacterSetId;

	private byte m_btAlternateFontIndex;

	private byte[] m_PANOSE = new byte[10];

	internal byte[] m_FONTSIGNATURE = new byte[24];

	internal byte TotalLengthM1
	{
		get
		{
			return m_btTotalLength;
		}
		set
		{
			if (value != m_btTotalLength)
			{
				m_btTotalLength = value;
			}
		}
	}

	internal byte Options
	{
		get
		{
			return m_btOptions;
		}
		set
		{
			if (value != m_btOptions)
			{
				m_btOptions = value;
			}
		}
	}

	internal short Weight
	{
		get
		{
			return m_wWeight;
		}
		set
		{
			if (value != m_wWeight)
			{
				m_wWeight = value;
			}
		}
	}

	internal byte CharacterSetId
	{
		get
		{
			return m_btCharacterSetId;
		}
		set
		{
			if (value != m_btCharacterSetId)
			{
				m_btCharacterSetId = value;
			}
		}
	}

	internal byte AlternateFontIndex
	{
		get
		{
			return m_btAlternateFontIndex;
		}
		set
		{
			if (value != m_btAlternateFontIndex)
			{
				m_btAlternateFontIndex = value;
			}
		}
	}

	public int Length => 40;

	internal void Close()
	{
		m_FONTSIGNATURE = null;
		m_PANOSE = null;
	}

	public void Parse(byte[] arrData, int iOffset)
	{
		m_btTotalLength = arrData[iOffset];
		iOffset++;
		m_btOptions = arrData[iOffset];
		iOffset++;
		m_wWeight = ByteConverter.ReadInt16(arrData, ref iOffset);
		m_btCharacterSetId = arrData[iOffset];
		iOffset++;
		m_btAlternateFontIndex = arrData[iOffset];
		iOffset++;
		m_PANOSE = ByteConverter.ReadBytes(arrData, 10, ref iOffset);
		m_FONTSIGNATURE = ByteConverter.ReadBytes(arrData, 24, ref iOffset);
	}

	public int Save(byte[] arrData, int iOffset)
	{
		arrData[iOffset] = m_btTotalLength;
		iOffset++;
		arrData[iOffset] = m_btOptions;
		iOffset++;
		ByteConverter.WriteInt16(arrData, ref iOffset, m_wWeight);
		arrData[iOffset] = m_btCharacterSetId;
		iOffset++;
		arrData[iOffset] = m_btAlternateFontIndex;
		iOffset++;
		ByteConverter.WriteBytes(arrData, ref iOffset, m_PANOSE);
		ByteConverter.WriteBytes(arrData, ref iOffset, m_FONTSIGNATURE);
		return 40;
	}
}
