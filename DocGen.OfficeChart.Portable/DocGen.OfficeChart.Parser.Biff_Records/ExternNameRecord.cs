using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExternName)]
[CLSCompliant(false)]
internal class ExternNameRecord : BiffRecordRawWithArray
{
	[Flags]
	private enum OptionFlags
	{
		BuiltIn = 1,
		WantAdvise = 2,
		WantPicture = 4,
		Ole = 8,
		OleLink = 0x10
	}

	[BiffRecordPos(0, 2)]
	private OptionFlags m_options;

	[BiffRecordPos(2, 2)]
	private ushort m_usSheetId;

	[BiffRecordPos(4, 2)]
	private ushort m_usWord2;

	[BiffRecordPos(6, TFieldType.String)]
	private string m_strName = string.Empty;

	private ushort m_usFormulaSize;

	private byte[] m_arrFormulaData;

	private bool m_isAddIn;

	public ushort Options
	{
		get
		{
			return (ushort)m_options;
		}
		set
		{
			m_options = (OptionFlags)value;
		}
	}

	public ushort SheetId => m_usSheetId;

	public ushort Word2 => m_usWord2;

	public byte[] FormulaData => m_arrFormulaData;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public override int MinimumRecordSize => 0;

	public ushort FormulaSize
	{
		get
		{
			return m_usFormulaSize;
		}
		set
		{
			m_usFormulaSize = value;
		}
	}

	public override bool NeedDataArray
	{
		get
		{
			if (m_options != 0)
			{
				return m_options != OptionFlags.BuiltIn;
			}
			return false;
		}
	}

	public bool BuiltIn
	{
		get
		{
			return (m_options & OptionFlags.BuiltIn) != 0;
		}
		set
		{
			SetFlag(OptionFlags.BuiltIn, value);
		}
	}

	public bool WantAdvise
	{
		get
		{
			return (m_options & OptionFlags.WantAdvise) != 0;
		}
		set
		{
			SetFlag(OptionFlags.WantAdvise, value);
		}
	}

	public bool WantPicture
	{
		get
		{
			return (m_options & OptionFlags.WantPicture) != 0;
		}
		set
		{
			SetFlag(OptionFlags.WantPicture, value);
		}
	}

	public bool Ole
	{
		get
		{
			return (m_options & OptionFlags.Ole) != 0;
		}
		set
		{
			SetFlag(OptionFlags.Ole, value);
		}
	}

	public bool OleLink
	{
		get
		{
			return (m_options & OptionFlags.OleLink) != 0;
		}
		set
		{
			SetFlag(OptionFlags.OleLink, value);
		}
	}

	public bool IsAddIn
	{
		get
		{
			return m_isAddIn;
		}
		set
		{
			m_isAddIn = value;
		}
	}

	private void SetFlag(OptionFlags flag, bool value)
	{
		if (value)
		{
			m_options |= flag;
		}
		else
		{
			m_options &= ~flag;
		}
	}

	public ExternNameRecord()
	{
	}

	public ExternNameRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ExternNameRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		int num = 0;
		m_options = (OptionFlags)GetUInt16(num);
		num += 2;
		m_usSheetId = GetUInt16(num);
		num += 2;
		m_usWord2 = GetUInt16(num);
		num += 2;
		m_strName = GetStringByteLen(num, out var iBytes);
		num += iBytes + 2;
		if (BuiltIn)
		{
			m_usFormulaSize = GetUInt16(num);
			num += 2;
		}
		else if (!OleLink)
		{
			m_usFormulaSize = (ushort)(m_iLength - num);
		}
		m_arrFormulaData = new byte[m_usFormulaSize];
		Buffer.BlockCopy(m_data, num, m_arrFormulaData, 0, m_usFormulaSize);
		num += m_usFormulaSize;
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		if (!OleLink && !BuiltIn)
		{
			InfillDDELink();
		}
		else
		{
			if (m_options != 0 && (m_options != OptionFlags.BuiltIn || m_usFormulaSize != 0))
			{
				return;
			}
			bool autoGrowData = AutoGrowData;
			AutoGrowData = true;
			SetUInt16(0, (ushort)m_options);
			SetUInt16(2, m_usSheetId);
			SetUInt16(4, m_usWord2);
			m_iLength = 6;
			SetString16BitUpdateOffset(ref m_iLength, m_strName);
			if (m_options == (OptionFlags)0 || m_options == OptionFlags.BuiltIn)
			{
				SetUInt16(m_iLength, m_usFormulaSize);
				m_iLength += 2;
				if (m_usFormulaSize > 0)
				{
					SetBytes(m_iLength, m_arrFormulaData, 0, m_usFormulaSize);
					m_iLength += m_usFormulaSize;
				}
			}
			AutoGrowData = autoGrowData;
		}
	}

	private void InfillDDELink()
	{
		bool autoGrowData = AutoGrowData;
		AutoGrowData = true;
		m_iLength = 0;
		SetUInt16(m_iLength, (ushort)m_options);
		m_iLength += 2;
		SetInt32(m_iLength, 0);
		m_iLength += 4;
		m_iLength += SetStringByteLen(m_iLength, m_strName);
		if (m_arrFormulaData != null && m_arrFormulaData.Length != 0)
		{
			SetBytes(m_iLength, m_arrFormulaData);
			m_iLength += m_arrFormulaData.Length;
		}
		else if (m_isAddIn)
		{
			m_arrFormulaData = new byte[4] { 2, 0, 28, 23 };
			SetBytes(m_iLength, m_arrFormulaData);
			m_iLength += m_arrFormulaData.Length;
		}
		AutoGrowData = autoGrowData;
	}
}
