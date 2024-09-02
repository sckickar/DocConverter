using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class FontFamilyNameStringTable : BaseWordRecord
{
	private ushort DEF_EXTENDED = ushort.MaxValue;

	private ushort m_extendedFlag;

	private ushort m_noStrings;

	private ushort m_extraDataLen;

	private FontFamilyNameRecord[] m_ffnRecords;

	internal override int Length
	{
		get
		{
			int num = 4;
			if (m_ffnRecords == null)
			{
				return num;
			}
			for (int i = 0; i < m_ffnRecords.Length; i++)
			{
				if (m_ffnRecords[i] != null)
				{
					num += m_ffnRecords[i].Length;
				}
			}
			return num;
		}
	}

	internal int RecordsCount
	{
		get
		{
			if (m_ffnRecords != null)
			{
				return m_ffnRecords.Length;
			}
			return 0;
		}
		set
		{
			m_ffnRecords = new FontFamilyNameRecord[value];
		}
	}

	internal FontFamilyNameRecord[] FontFamilyNameRecords => m_ffnRecords;

	internal FontFamilyNameStringTable()
	{
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (arrData.Length >= 2)
		{
			m_noStrings = (m_extendedFlag = BaseWordRecord.ReadUInt16(arrData, iOffset));
			iOffset += 2;
			if (m_extendedFlag == DEF_EXTENDED)
			{
				m_noStrings = BaseWordRecord.ReadUInt16(arrData, ref iOffset);
			}
			m_extraDataLen = BaseWordRecord.ReadUInt16(arrData, ref iOffset);
			iCount = arrData.Length - iOffset;
			m_ffnRecords = new FontFamilyNameRecord[m_noStrings];
			for (int i = 0; i < m_noStrings; i++)
			{
				int num = (m_ffnRecords[i] = new FontFamilyNameRecord()).ParseBytes(arrData, iOffset, iCount);
				iOffset += num;
				iCount -= num;
			}
		}
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		int result = 0;
		m_noStrings = (ushort)m_ffnRecords.Length;
		BaseWordRecord.WriteUInt16(arrData, m_noStrings, ref iOffset);
		BaseWordRecord.WriteUInt16(arrData, m_extraDataLen, ref iOffset);
		for (int i = 0; i < m_ffnRecords.Length; i++)
		{
			iOffset = m_ffnRecords[i].Save(arrData, iOffset);
		}
		return result;
	}

	internal override void Close()
	{
		base.Close();
		if (m_ffnRecords == null)
		{
			return;
		}
		for (int i = 0; i < m_ffnRecords.Length; i++)
		{
			if (m_ffnRecords[i] != null)
			{
				m_ffnRecords[i].Close();
			}
		}
		m_ffnRecords = null;
	}
}
