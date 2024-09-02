using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftSbs : ObjSubRecord
{
	private const int DEF_RECORD_SIZE = 24;

	private static readonly byte[] DEF_SAMPLE_RECORD_DATA = new byte[20]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		1, 0, 8, 0, 0, 0, 16, 0, 0, 0
	};

	private byte[] m_data;

	private int m_iValue;

	private int m_iMinimum;

	private int m_iMaximum;

	private int m_iIncrement;

	private int m_iPage;

	private int m_iHorizontal;

	private int m_iScrollBarWidth;

	private short m_sOptions;

	public byte[] Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public int Value
	{
		get
		{
			return m_iValue;
		}
		set
		{
			m_iValue = value;
		}
	}

	public int Minimum
	{
		get
		{
			return m_iMinimum;
		}
		set
		{
			m_iMinimum = value;
		}
	}

	public int Maximum
	{
		get
		{
			return m_iMaximum;
		}
		set
		{
			m_iMaximum = value;
		}
	}

	public int Increment
	{
		get
		{
			return m_iIncrement;
		}
		set
		{
			m_iIncrement = value;
		}
	}

	public int Page
	{
		get
		{
			return m_iPage;
		}
		set
		{
			m_iPage = value;
		}
	}

	public int Horizontal
	{
		get
		{
			return m_iHorizontal;
		}
		set
		{
			m_iHorizontal = value;
		}
	}

	public int ScrollBarWidth
	{
		get
		{
			return m_iScrollBarWidth;
		}
		set
		{
			m_iScrollBarWidth = value;
		}
	}

	[CLSCompliant(false)]
	public ftSbs()
		: base(TObjSubRecordType.ftSbs, 20, DEF_SAMPLE_RECORD_DATA)
	{
	}

	public ftSbs(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		m_data = (byte[])buffer.Clone();
		m_iValue = BitConverter.ToInt16(m_data, 4);
		m_iMinimum = BitConverter.ToInt16(m_data, 6);
		m_iMaximum = BitConverter.ToInt16(m_data, 8);
		m_iIncrement = BitConverter.ToInt16(m_data, 10);
		m_iPage = BitConverter.ToInt16(m_data, 12);
		m_iHorizontal = BitConverter.ToInt16(m_data, 14);
		m_iScrollBarWidth = BitConverter.ToInt16(m_data, 16);
		m_sOptions = BitConverter.ToInt16(m_data, 18);
	}

	protected override void Serialize(DataProvider provider, int iOffset)
	{
		provider.WriteInt32(iOffset, 0);
		iOffset += 4;
		provider.WriteInt16(iOffset, (short)m_iValue);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iMinimum);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iMaximum);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iIncrement);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iPage);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iHorizontal);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iScrollBarWidth);
		iOffset += 2;
		provider.WriteInt16(iOffset, m_sOptions);
	}

	public override object Clone()
	{
		ftSbs obj = (ftSbs)base.Clone();
		obj.m_data = CloneUtils.CloneByteArray(m_data);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 24;
	}
}
