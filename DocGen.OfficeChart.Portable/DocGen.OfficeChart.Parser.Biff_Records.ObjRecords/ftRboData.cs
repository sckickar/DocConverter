using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftRboData : ObjSubRecord
{
	private byte m_isFirstButton;

	private byte m_nextButton;

	private byte m_ft;

	private byte m_cb;

	public bool IsFirstButton
	{
		get
		{
			return m_isFirstButton == 1;
		}
		set
		{
			m_isFirstButton = (value ? ((byte)1) : ((byte)0));
		}
	}

	public byte NextButton
	{
		get
		{
			return m_nextButton;
		}
		set
		{
			m_nextButton = value;
		}
	}

	public ftRboData()
		: base(TObjSubRecordType.ftRboData)
	{
	}

	public ftRboData(ushort length, byte[] buffer)
		: base(TObjSubRecordType.ftRboData, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		m_nextButton = buffer[0];
		m_ft = buffer[1];
		m_isFirstButton = buffer[2];
		m_cb = buffer[3];
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		short value = (short)(GetStoreSize(OfficeVersion.Excel97to2003) - 4);
		provider.WriteInt16(iOffset, value);
		iOffset += 2;
		provider.WriteByte(iOffset, m_nextButton);
		iOffset++;
		provider.WriteByte(iOffset, m_ft);
		iOffset++;
		provider.WriteByte(iOffset, m_isFirstButton);
		iOffset++;
		provider.WriteByte(iOffset, m_cb);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
