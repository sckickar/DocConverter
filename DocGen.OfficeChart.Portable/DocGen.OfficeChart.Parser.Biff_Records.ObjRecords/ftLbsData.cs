using System;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftLbsData : ObjSubRecord
{
	public enum ExcelListSelectionType
	{
		Single,
		MultiClick,
		MultiCtrl
	}

	private const int DEF_RECORD_SIZE = 20;

	private const int DEF_COLOR_BIT_INDEX = 3;

	private const int DEF_COLOR_BYTE = 10;

	private static readonly byte[] DEF_SAMPLE_RECORD_DATA = new byte[16]
	{
		0, 0, 8, 0, 4, 0, 1, 3, 0, 0,
		2, 0, 20, 0, 162, 0
	};

	private const int TypeValidMask = 1;

	private const int TypeMask = 65280;

	private const int TypeMaskStartBit = 8;

	private const int ThreeDMask = 8;

	private const int SelectionTypeMask = 48;

	private const int SelectionTypeStartBit = 4;

	private byte[] m_data;

	private int m_iLinesNumber;

	private Ptg[] m_arrFormula;

	private int m_iSelectedIndex;

	private int m_iOptions;

	private int m_iEditId;

	private LbsDropData m_dropData = new LbsDropData();

	private bool[] m_arrSelections;

	private bool m_bShortVersion;

	private TObjType m_parentObjectType = TObjType.otComboBox;

	public byte[] Data1
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

	public bool IsSelectedColor
	{
		get
		{
			return BiffRecordRaw.GetBit(m_data, 10, 3);
		}
		set
		{
			BiffRecordRaw.SetBit(m_data, 10, value, 3);
		}
	}

	public int LinesNumber
	{
		get
		{
			return m_iLinesNumber;
		}
		set
		{
			if (m_iLinesNumber != value)
			{
				m_iLinesNumber = value;
				if (m_arrSelections != null)
				{
					bool[] arrSelections = m_arrSelections;
					m_arrSelections = new bool[value];
					Buffer.BlockCopy(arrSelections, 0, m_arrSelections, 0, arrSelections.Length);
				}
			}
		}
	}

	public Ptg[] Formula
	{
		get
		{
			return m_arrFormula;
		}
		set
		{
			m_arrFormula = value;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return m_iSelectedIndex;
		}
		set
		{
			m_iSelectedIndex = value;
		}
	}

	public int Options
	{
		get
		{
			return m_iOptions;
		}
		set
		{
			m_iOptions = value;
		}
	}

	public int EditId
	{
		get
		{
			return m_iEditId;
		}
		set
		{
			m_iEditId = value;
		}
	}

	public LbsDropData DropData => m_dropData;

	public bool ComboTypeValid
	{
		get
		{
			return (m_iOptions & 1) != 0;
		}
		set
		{
			m_iOptions = (value ? (m_iOptions | 1) : (m_iOptions & -2));
		}
	}

	public ExcelComboType ComboType
	{
		get
		{
			if (!ComboTypeValid)
			{
				return ExcelComboType.Regular;
			}
			return (ExcelComboType)((m_iOptions & 0xFF00) >> 8);
		}
		set
		{
			int num = (int)value << 8;
			m_iOptions &= -65281;
			m_iOptions |= num;
			ComboTypeValid = value != ExcelComboType.Regular;
		}
	}

	public bool NoThreeD
	{
		get
		{
			return (m_iOptions & 8) != 0;
		}
		set
		{
			if (value)
			{
				m_iOptions |= 8;
			}
			else
			{
				m_iOptions &= -9;
			}
		}
	}

	public ExcelListSelectionType SelectionType
	{
		get
		{
			return (ExcelListSelectionType)((m_iOptions & 0x30) >> 4);
		}
		set
		{
			if (value != SelectionType)
			{
				int num = (int)value << 4;
				m_iOptions &= -49;
				m_iOptions |= num;
				if (value == ExcelListSelectionType.Single)
				{
					m_arrSelections = null;
				}
				else
				{
					m_arrSelections = new bool[LinesNumber];
				}
			}
		}
	}

	public bool IsMultiSelection => SelectionType != ExcelListSelectionType.Single;

	public ftLbsData()
		: base(TObjSubRecordType.ftLbsData)
	{
	}

	public ftLbsData(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	public ftLbsData(TObjSubRecordType type, ushort length, byte[] buffer, TObjType objectType)
		: base(type)
	{
		base.Length = length;
		m_parentObjectType = objectType;
		Parse(buffer);
	}

	protected override void Parse(byte[] buffer)
	{
		m_data = (byte[])buffer.Clone();
		int num = 0;
		int num2 = BitConverter.ToInt16(buffer, num);
		num += 2;
		if (num2 > 0)
		{
			int num3 = BitConverter.ToInt16(buffer, num);
			num += 2;
			BitConverter.ToInt32(buffer, num);
			num += 4;
			byte[] array = new byte[num3];
			Buffer.BlockCopy(m_data, num, array, 0, num3);
			m_arrFormula = FormulaUtil.ParseExpression(new ByteArrayDataProvider(array), num3, OfficeVersion.Excel97to2003);
			num = num2 + 2;
		}
		m_iLinesNumber = BitConverter.ToInt16(buffer, num);
		num += 2;
		m_iSelectedIndex = BitConverter.ToInt16(buffer, num);
		num += 2;
		if (num >= buffer.Length)
		{
			m_bShortVersion = true;
			return;
		}
		m_iOptions = BitConverter.ToInt16(buffer, num);
		num += 2;
		m_iEditId = BitConverter.ToInt16(buffer, num);
		num += 2;
		if (m_parentObjectType == TObjType.otComboBox)
		{
			num = m_dropData.Parse(new ByteArrayDataProvider(buffer), num);
		}
		if (IsMultiSelection)
		{
			num = ParseMultiSelection(buffer, num);
		}
	}

	private int ParseMultiSelection(byte[] buffer, int iOffset)
	{
		m_arrSelections = new bool[m_iLinesNumber];
		int num = 0;
		while (num < m_iLinesNumber && iOffset < buffer.Length)
		{
			m_arrSelections[num] = buffer[iOffset] != 0;
			num++;
			iOffset++;
		}
		return iOffset;
	}

	private void ParseLines(byte[] buffer, int iOffset)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public override object Clone()
	{
		ftLbsData obj = (ftLbsData)base.Clone();
		obj.m_data = CloneUtils.CloneByteArray(m_data);
		obj.m_arrFormula = CloneUtils.ClonePtgArray(m_arrFormula);
		obj.m_dropData = m_dropData.Clone();
		obj.m_arrSelections = CloneUtils.CloneBoolArray(m_arrSelections);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = DVRecord.GetFormulaSize(m_arrFormula, OfficeVersion.Excel97to2003, addAdditionalDataSize: false);
		if (num % 2 != 0)
		{
			num++;
		}
		if (num > 0)
		{
			num += 8;
		}
		if (!m_bShortVersion)
		{
			num += 6;
		}
		num += 8;
		if (m_parentObjectType == TObjType.otComboBox)
		{
			num += m_dropData.GetStoreSize();
		}
		if (m_arrSelections != null)
		{
			num += m_arrSelections.Length;
		}
		return num;
	}

	protected override void Serialize(DataProvider provider, int iOffset)
	{
		byte[] array = ((m_arrFormula != null) ? FormulaUtil.PtgArrayToByteArray(m_arrFormula, OfficeVersion.Excel97to2003) : new byte[0]);
		int num = array.Length;
		int num2 = ((num > 0) ? (num + 2 + 4) : 0);
		bool flag = num2 % 2 != 0;
		if (flag)
		{
			num2++;
		}
		provider.WriteInt16(iOffset, (short)num2);
		iOffset += 2;
		if (num > 0)
		{
			provider.WriteInt16(iOffset, (short)num);
			iOffset += 2;
			provider.WriteInt32(iOffset, 0);
			iOffset += 4;
			if (num > 0)
			{
				provider.WriteBytes(iOffset, array);
				iOffset += num;
			}
			if (flag)
			{
				provider.WriteByte(iOffset, 240);
				iOffset++;
			}
		}
		provider.WriteInt16(iOffset, (short)m_iLinesNumber);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iSelectedIndex);
		iOffset += 2;
		if (m_bShortVersion)
		{
			return;
		}
		provider.WriteInt16(iOffset, (short)m_iOptions);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)m_iEditId);
		iOffset += 2;
		if (m_parentObjectType == TObjType.otComboBox)
		{
			if (ComboType == ExcelComboType.AutoFilter)
			{
				m_dropData.Options = 2;
			}
			m_dropData.Serialize(provider, iOffset);
		}
		if (m_arrSelections != null)
		{
			iOffset = SerializeMultiSelection(provider, iOffset);
		}
	}

	private int SerializeMultiSelection(DataProvider provider, int iOffset)
	{
		int num = 0;
		int num2 = m_arrSelections.Length;
		while (num < num2)
		{
			byte value = (m_arrSelections[num] ? ((byte)1) : ((byte)0));
			provider.WriteByte(iOffset, value);
			num++;
			iOffset++;
		}
		return iOffset;
	}
}
