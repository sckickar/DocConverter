using System;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Label)]
[CLSCompliant(false)]
internal class LabelRecord : CellPositionBase, IStringValue, IValueHolder
{
	private const int DEF_FIXED_PART = 9;

	private string m_strLabel = string.Empty;

	public string Label
	{
		get
		{
			return m_strLabel;
		}
		set
		{
			m_strLabel = value;
		}
	}

	public override int MinimumRecordSize => 8;

	string IStringValue.StringValue => Label;

	public object Value
	{
		get
		{
			return Label;
		}
		set
		{
			Label = (string)value;
		}
	}

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_strLabel = provider.ReadString16Bit(iOffset, out var _);
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = iOffset;
		provider.WriteString16BitUpdateOffset(ref m_iLength, m_strLabel);
		m_iLength -= iOffset;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_strLabel == null)
		{
			m_strLabel = string.Empty;
		}
		int num = 9 + Encoding.Unicode.GetByteCount(m_strLabel);
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}
}
