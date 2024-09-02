using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class ContinueRecordBuilder
{
	protected BiffContinueRecordRaw m_parent;

	protected int m_iPos;

	private int m_iContinuePos = -1;

	private int m_iContinueSize;

	private int m_iTotal;

	protected int m_iMax;

	private TBIFFRecord m_firstContinueType = TBIFFRecord.Continue;

	private TBIFFRecord m_continueType = TBIFFRecord.Continue;

	private int m_iContinueCount;

	public int FreeSpace => m_iMax - m_iContinueSize;

	public int Total
	{
		get
		{
			return m_iTotal;
		}
		set
		{
			m_iTotal = value;
		}
	}

	public int Position
	{
		get
		{
			return m_iPos;
		}
		set
		{
			m_iPos = value;
		}
	}

	public int Max => m_iMax;

	public TBIFFRecord FirstContinueType
	{
		get
		{
			return m_firstContinueType;
		}
		set
		{
			m_firstContinueType = value;
		}
	}

	public TBIFFRecord ContinueType
	{
		get
		{
			return m_continueType;
		}
		set
		{
			m_continueType = value;
		}
	}

	public virtual int MaximumSize => 8224 - HeaderFooterImageRecord.DEF_DATA_OFFSET;

	public event EventHandler OnFirstContinue;

	public ContinueRecordBuilder(BiffContinueRecordRaw parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_parent = parent;
		m_iMax = m_parent.MaximumRecordSize;
		m_iContinueSize = m_parent.Length;
		m_iPos = m_iContinueSize;
		m_iTotal = m_iContinueSize;
	}

	public void AppendByte(byte value)
	{
		if (CheckIfSpaceNeeded(1))
		{
			UpdateContinueRecordSize();
			StartContinueRecord();
		}
		m_parent.SetByte(m_iPos, value);
		UpdateCounters(1);
	}

	public virtual int AppendBytes(byte[] data, int start, int length)
	{
		int num = 0;
		if (CheckIfSpaceNeeded(length))
		{
			int num2 = start + length;
			for (int i = start; i < num2; i += m_iMax)
			{
				UpdateContinueRecordSize();
				StartContinueRecord();
				num++;
				int num3 = ((num2 - i < m_iMax) ? (num2 - i) : m_iMax);
				m_parent.SetBytes(m_iPos, data, i, num3);
				UpdateCounters(num3);
			}
		}
		else
		{
			m_parent.SetBytes(m_iPos, data, start, length);
			UpdateCounters(length);
		}
		UpdateContinueRecordSize();
		return num;
	}

	public void AppendUInt16(ushort value)
	{
		if (CheckIfSpaceNeeded(2))
		{
			UpdateContinueRecordSize();
			StartContinueRecord();
		}
		m_parent.SetUInt16(m_iPos, value);
		UpdateCounters(2);
	}

	public bool CheckIfSpaceNeeded(int length)
	{
		return m_iContinueSize + length > m_iMax;
	}

	public void StartContinueRecord()
	{
		if (this.OnFirstContinue != null)
		{
			this.OnFirstContinue(this, EventArgs.Empty);
		}
		m_iContinueCount++;
		m_parent.m_arrContinuePos.Add(m_iPos);
		TBIFFRecord tBIFFRecord = ((m_iContinueCount == 1) ? FirstContinueType : ContinueType);
		m_parent.SetUInt16(m_iPos, (ushort)tBIFFRecord);
		m_iPos += 2;
		m_iContinuePos = m_iPos;
		m_iContinueSize = 0;
		m_parent.SetUInt16(m_iPos, (ushort)m_iContinueSize);
		m_iPos += 2;
		m_iTotal += 4;
		m_iMax = MaximumSize;
	}

	public void UpdateContinueRecordSize()
	{
		if (m_iContinuePos >= 0)
		{
			m_parent.SetUInt16(m_iContinuePos, (ushort)m_iContinueSize);
		}
	}

	protected void UpdateCounters(int iLen)
	{
		m_iPos += iLen;
		m_iTotal += iLen;
		m_iContinueSize += iLen;
	}
}
