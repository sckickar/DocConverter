using System;
using System.IO;
using System.Text;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Name)]
[CLSCompliant(false)]
internal class NameRecord : BiffRecordRaw
{
	public const ushort FunctionGroupBitMask = 4032;

	public static readonly string[] PREDEFINED_NAMES = new string[16]
	{
		"Consolidate_Area", "Auto_Open", "Auto_Close", "Extract", "Database", "Criteria", "Print_Area", "Print_Titles", "Recorder", "Data_Form",
		"Auto_Activate", "Auto_Deactivate", "Sheet_Title", "_FilterDatabase", "_xlnm.Print_Titles", "_xlnm.Print_Area"
	};

	private const int DEF_FIXED_PART_SIZE = 14;

	private const string XLNM_ExtensionName = "_xlnm.";

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bNameHidden;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bNameFunction;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bNameCommand;

	[BiffRecordPos(0, 3, TFieldType.Bit)]
	private bool m_bFCMacro;

	[BiffRecordPos(0, 4, TFieldType.Bit)]
	private bool m_bComplexFunction;

	[BiffRecordPos(0, 5, TFieldType.Bit)]
	private bool m_bBuinldInName;

	[BiffRecordPos(1, 4, TFieldType.Bit)]
	private bool m_bBinaryData;

	[BiffRecordPos(2, 1)]
	private byte m_bKeyboardShortcut;

	[BiffRecordPos(3, 1)]
	private byte m_bNameLength;

	[BiffRecordPos(4, 2)]
	private ushort m_usFormulaDataSize;

	[BiffRecordPos(6, 2)]
	private ushort m_usReserved;

	[BiffRecordPos(8, 2)]
	private ushort m_usIndexOrGlobal;

	[BiffRecordPos(10, 1)]
	private byte m_bMenuTextLength;

	[BiffRecordPos(11, 1)]
	private byte m_bDescriptionLength;

	[BiffRecordPos(12, 1)]
	private byte m_bHelpTextLength;

	[BiffRecordPos(13, 1)]
	private byte m_bStatusTextLength;

	private string m_strName = string.Empty;

	private byte[] m_arrFormulaData;

	private string m_strMenuText = string.Empty;

	private string m_strDescription = string.Empty;

	private string m_strHelpText = string.Empty;

	private string m_strStatusText = string.Empty;

	private Ptg[] m_arrToken;

	public bool IsNameHidden
	{
		get
		{
			return m_bNameHidden;
		}
		set
		{
			m_bNameHidden = value;
		}
	}

	public bool IsNameFunction
	{
		get
		{
			return m_bNameFunction;
		}
		set
		{
			m_bNameFunction = value;
		}
	}

	public bool IsNameCommand
	{
		get
		{
			return m_bNameCommand;
		}
		set
		{
			m_bNameCommand = value;
		}
	}

	public bool IsFunctionOrCommandMacro
	{
		get
		{
			return m_bFCMacro;
		}
		set
		{
			m_bFCMacro = value;
		}
	}

	public bool IsComplexFunction
	{
		get
		{
			return m_bComplexFunction;
		}
		set
		{
			m_bComplexFunction = value;
		}
	}

	public bool IsBuinldInName
	{
		get
		{
			return m_bBuinldInName;
		}
		set
		{
			m_bBuinldInName = value;
		}
	}

	public bool HasBinaryData
	{
		get
		{
			return m_bBinaryData;
		}
		set
		{
			m_bBinaryData = value;
		}
	}

	public ushort FunctionGroupIndex
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 4032) >> 6);
		}
		set
		{
			if (value > 63)
			{
				throw new ArgumentOutOfRangeException("FunctionGroupIndex too large.");
			}
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 4032, (ushort)(value << 6));
		}
	}

	public byte KeyboardShortcut
	{
		get
		{
			return m_bKeyboardShortcut;
		}
		set
		{
			m_bKeyboardShortcut = value;
		}
	}

	public byte NameLength => m_bNameLength;

	public ushort FormulaDataSize => m_usFormulaDataSize;

	public ushort IndexOrGlobal
	{
		get
		{
			return m_usIndexOrGlobal;
		}
		set
		{
			m_usIndexOrGlobal = value;
		}
	}

	public byte MenuTextLength => m_bKeyboardShortcut;

	public byte DescriptionLength => m_bDescriptionLength;

	public byte HelpTextLength => m_bHelpTextLength;

	public byte StatusTextLength => m_bStatusTextLength;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
			m_bNameLength = (byte)(IsPredefinedName(m_strName) ? 1 : ((m_strName != null) ? ((byte)m_strName.Length) : 0));
		}
	}

	public Ptg[] FormulaTokens
	{
		get
		{
			return m_arrToken;
		}
		set
		{
			m_arrToken = value;
			if (value != null)
			{
				m_arrFormulaData = FormulaUtil.PtgArrayToByteArray(value, out var formulaLen, OfficeVersion.Excel2007);
				m_usFormulaDataSize = (ushort)formulaLen;
			}
			else
			{
				m_usFormulaDataSize = 0;
				m_arrFormulaData = null;
			}
		}
	}

	public string MenuText
	{
		get
		{
			return m_strMenuText;
		}
		set
		{
			m_strMenuText = value;
			m_bMenuTextLength = (byte)((m_strMenuText != null) ? ((byte)m_strMenuText.Length) : 0);
		}
	}

	public string Description
	{
		get
		{
			return m_strDescription;
		}
		set
		{
			m_strDescription = value;
			m_bDescriptionLength = (byte)((m_strDescription != null) ? ((byte)m_strDescription.Length) : 0);
		}
	}

	public string HelpText
	{
		get
		{
			return m_strHelpText;
		}
		set
		{
			m_strHelpText = value;
			m_bHelpTextLength = (byte)((m_strHelpText != null) ? ((byte)m_strHelpText.Length) : 0);
		}
	}

	public string StatusText
	{
		get
		{
			return m_strStatusText;
		}
		set
		{
			m_strStatusText = value;
			m_bStatusTextLength = (byte)((m_strStatusText != null) ? ((byte)m_strStatusText.Length) : 0);
		}
	}

	public ushort Reserved => m_usReserved;

	public override int MinimumRecordSize => 14;

	public NameRecord()
	{
	}

	public NameRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public NameRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		ParseFixedPart(provider, iOffset);
		int offset = iOffset + 14;
		if (m_bNameLength != 0)
		{
			m_strName = provider.ReadStringUpdateOffset(ref offset, m_bNameLength);
		}
		else
		{
			offset++;
		}
		if (IsBuinldInName && m_strName.Length == 1)
		{
			m_strName = PREDEFINED_NAMES[(uint)m_strName[0]];
		}
		try
		{
			FormulaTokens = FormulaUtil.ParseExpression(provider, offset, m_usFormulaDataSize, out offset, version);
		}
		catch (Exception)
		{
			throw;
		}
		m_strMenuText = provider.ReadStringUpdateOffset(ref offset, m_bMenuTextLength);
		m_strDescription = provider.ReadStringUpdateOffset(ref offset, m_bDescriptionLength);
		m_strHelpText = provider.ReadStringUpdateOffset(ref offset, m_bHelpTextLength);
		m_strStatusText = provider.ReadStringUpdateOffset(ref offset, m_bStatusTextLength);
		if (offset != m_iLength)
		{
			throw new WrongBiffRecordDataException("NameRecord");
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (m_strName.Equals(PREDEFINED_NAMES[14]))
		{
			m_strName = m_strName.Substring("_xlnm.".Length);
		}
		bool bUnicode = !BiffRecordRawWithArray.IsAsciiString(m_strName);
		if (m_arrToken != null && m_arrToken.Length != 0)
		{
			m_arrFormulaData = FormulaUtil.PtgArrayToByteArray(m_arrToken, out var formulaLen, version);
			m_usFormulaDataSize = (ushort)formulaLen;
		}
		else
		{
			m_arrFormulaData = null;
			m_usFormulaDataSize = 0;
		}
		m_iLength = GetStoreSize(version);
		InfillFixedPart(provider, iOffset);
		iOffset += 14;
		if (IsBuinldInName)
		{
			provider.WriteByte(iOffset, 0);
			int num = PredefinedIndex(m_strName);
			if (num != -1)
			{
				byte value = ((num < 0) ? ((byte)m_strName[0]) : ((byte)num));
				provider.WriteByte(iOffset + 1, value);
				iOffset += 2;
			}
			else
			{
				provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strName, bUnicode);
			}
		}
		else
		{
			provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strName, bUnicode);
		}
		if (m_bNameLength == 0)
		{
			provider.WriteByte(iOffset, 0);
			iOffset++;
		}
		if (m_arrFormulaData != null)
		{
			provider.WriteBytes(iOffset, m_arrFormulaData, 0, m_arrFormulaData.Length);
			iOffset += m_arrFormulaData.Length;
		}
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strMenuText);
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strDescription);
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strHelpText);
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strStatusText);
	}

	private void InfillFixedPart(DataProvider provider, int iOffset)
	{
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bNameHidden, 0);
		provider.WriteBit(iOffset, m_bNameFunction, 1);
		provider.WriteBit(iOffset, m_bNameCommand, 2);
		provider.WriteBit(iOffset, m_bFCMacro, 3);
		provider.WriteBit(iOffset, m_bComplexFunction, 4);
		provider.WriteBit(iOffset, m_bBuinldInName, 5);
		iOffset++;
		provider.WriteBit(iOffset, m_bBinaryData, 4);
		iOffset++;
		provider.WriteByte(iOffset, m_bKeyboardShortcut);
		iOffset++;
		provider.WriteByte(iOffset, m_bNameLength);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usFormulaDataSize);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usReserved);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usIndexOrGlobal);
		iOffset += 2;
		provider.WriteByte(iOffset, m_bMenuTextLength);
		iOffset++;
		provider.WriteByte(iOffset, m_bDescriptionLength);
		iOffset++;
		provider.WriteByte(iOffset, m_bHelpTextLength);
		iOffset++;
		provider.WriteByte(iOffset, m_bStatusTextLength);
		iOffset++;
	}

	private void ParseFixedPart(DataProvider provider, int iOffset)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bNameHidden = provider.ReadBit(iOffset, 0);
		m_bNameFunction = provider.ReadBit(iOffset, 1);
		m_bNameCommand = provider.ReadBit(iOffset, 2);
		m_bFCMacro = provider.ReadBit(iOffset, 3);
		m_bComplexFunction = provider.ReadBit(iOffset, 4);
		m_bBuinldInName = provider.ReadBit(iOffset, 5);
		m_bBinaryData = provider.ReadBit(iOffset + 1, 4);
		m_bKeyboardShortcut = provider.ReadByte(iOffset + 2);
		m_bNameLength = provider.ReadByte(iOffset + 3);
		m_usFormulaDataSize = provider.ReadUInt16(iOffset + 4);
		m_usReserved = provider.ReadUInt16(iOffset + 6);
		m_usIndexOrGlobal = provider.ReadUInt16(iOffset + 8);
		m_bMenuTextLength = provider.ReadByte(iOffset + 10);
		m_bDescriptionLength = provider.ReadByte(iOffset + 11);
		m_bHelpTextLength = provider.ReadByte(iOffset + 12);
		m_bStatusTextLength = provider.ReadByte(iOffset + 13);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 14;
		Encoding encoding = ((!BiffRecordRawWithArray.IsAsciiString(m_strName)) ? Encoding.Unicode : Encoding.UTF8);
		num = ((!IsBuinldInName) ? (num + (encoding.GetByteCount(m_strName) + 1)) : ((Array.IndexOf(PREDEFINED_NAMES, m_strName) == -1) ? (num + (encoding.GetByteCount(m_strName) + 1)) : (num + 2)));
		num += DVRecord.GetFormulaSize(m_arrToken, version, addAdditionalDataSize: true);
		num += GetByteCount(m_strMenuText);
		num += GetByteCount(m_strDescription);
		num += GetByteCount(m_strHelpText);
		return num + GetByteCount(m_strStatusText);
	}

	private int GetByteCount(string strValue)
	{
		if (strValue == null || strValue.Length <= 0)
		{
			return 0;
		}
		return Encoding.Unicode.GetByteCount(strValue) + 1;
	}

	public static bool IsPredefinedName(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return PredefinedIndex(value) >= 0;
	}

	public static int PredefinedIndex(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int result = -1;
		if (value.Length > 0)
		{
			int i = 0;
			for (int num = PREDEFINED_NAMES.Length; i < num; i++)
			{
				string value2 = PREDEFINED_NAMES[i];
				if (value.StartsWith(value2))
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public override object Clone()
	{
		NameRecord obj = (NameRecord)base.Clone();
		obj.FormulaTokens = CloneUtils.ClonePtgArray(m_arrToken);
		return obj;
	}

	public override void ClearData()
	{
		m_arrFormulaData = null;
		m_arrToken = null;
	}

	internal void Delete()
	{
		m_usFormulaDataSize = 0;
		m_arrFormulaData = null;
		m_arrToken = null;
	}
}
