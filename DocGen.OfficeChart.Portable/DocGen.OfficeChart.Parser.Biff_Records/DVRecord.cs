using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DV)]
[CLSCompliant(false)]
internal class DVRecord : BiffRecordRawWithArray, ICloneable
{
	public const uint DataTypeBitMask = 15u;

	public const uint ErrorStyleBitMask = 112u;

	public const uint ConditionBitMask = 15728640u;

	public const int ErrorStyleStartBit = 4;

	public const int ConditionStartBit = 20;

	public const string StringEmpty = "\0";

	private const int DEF_FIXED_PART_SIZE = 14;

	[BiffRecordPos(0, 4)]
	private uint m_uiOptions;

	[BiffRecordPos(0, 7, TFieldType.Bit)]
	private bool m_bStrListExplicit;

	[BiffRecordPos(1, 0, TFieldType.Bit)]
	private bool m_bEmptyCell = true;

	[BiffRecordPos(1, 1, TFieldType.Bit)]
	private bool m_bSuppressArrow;

	[BiffRecordPos(2, 2, TFieldType.Bit)]
	private bool m_bShowPromptBox = true;

	[BiffRecordPos(2, 3, TFieldType.Bit)]
	private bool m_bShowErrorBox = true;

	private string m_strPromtBoxTitle = string.Empty;

	private bool m_bPromptBoxShort;

	private string m_strErrorBoxTitle = string.Empty;

	private bool m_bErrorBoxShort;

	private string m_strPromtBoxText = string.Empty;

	private bool m_bPromptBoxTextShort;

	private string m_strErrorBoxText = string.Empty;

	private bool m_bErrorBoxTextShort;

	private ushort m_usAddrListSize;

	private List<TAddr> m_arrAddrList = new List<TAddr>();

	private Ptg[] m_arrFirstFormulaTokens;

	private Ptg[] m_arrSecondFormulaTokens;

	public uint Options => m_uiOptions;

	public bool IsStrListExplicit
	{
		get
		{
			return m_bStrListExplicit;
		}
		set
		{
			m_bStrListExplicit = value;
		}
	}

	public bool IsEmptyCell
	{
		get
		{
			return m_bEmptyCell;
		}
		set
		{
			m_bEmptyCell = value;
		}
	}

	public bool IsSuppressArrow
	{
		get
		{
			return m_bSuppressArrow;
		}
		set
		{
			m_bSuppressArrow = value;
		}
	}

	public bool IsShowPromptBox
	{
		get
		{
			return m_bShowPromptBox;
		}
		set
		{
			m_bShowPromptBox = value;
		}
	}

	public bool IsShowErrorBox
	{
		get
		{
			return m_bShowErrorBox;
		}
		set
		{
			m_bShowErrorBox = value;
		}
	}

	public ExcelDataType DataType
	{
		get
		{
			return (ExcelDataType)BiffRecordRaw.GetUInt32BitsByMask(m_uiOptions, 15u);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiOptions, 15u, (uint)value);
		}
	}

	public ExcelErrorStyle ErrorStyle
	{
		get
		{
			return (ExcelErrorStyle)(BiffRecordRaw.GetUInt32BitsByMask(m_uiOptions, 112u) >> 4);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiOptions, 112u, (uint)((int)value << 4));
		}
	}

	public ExcelDataValidationComparisonOperator Condition
	{
		get
		{
			return (ExcelDataValidationComparisonOperator)(BiffRecordRaw.GetUInt32BitsByMask(m_uiOptions, 15728640u) >> 20);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiOptions, 15728640u, (uint)((int)value << 20));
		}
	}

	public string PromtBoxTitle
	{
		get
		{
			return m_strPromtBoxTitle;
		}
		set
		{
			m_strPromtBoxTitle = value;
		}
	}

	public string ErrorBoxTitle
	{
		get
		{
			return m_strErrorBoxTitle;
		}
		set
		{
			m_strErrorBoxTitle = value;
		}
	}

	public string PromtBoxText
	{
		get
		{
			return m_strPromtBoxText;
		}
		set
		{
			m_strPromtBoxText = value;
		}
	}

	public string ErrorBoxText
	{
		get
		{
			return m_strErrorBoxText;
		}
		set
		{
			m_strErrorBoxText = value;
		}
	}

	public Ptg[] FirstFormulaTokens
	{
		get
		{
			return m_arrFirstFormulaTokens;
		}
		set
		{
			m_arrFirstFormulaTokens = value;
		}
	}

	public Ptg[] SecondFormulaTokens
	{
		get
		{
			return m_arrSecondFormulaTokens;
		}
		set
		{
			m_arrSecondFormulaTokens = value;
		}
	}

	public ushort AddrListSize => m_usAddrListSize;

	public TAddr[] AddrList
	{
		get
		{
			return m_arrAddrList.ToArray();
		}
		set
		{
			m_arrAddrList.Clear();
			if (value != null)
			{
				m_arrAddrList.AddRange(value);
			}
			m_usAddrListSize = (ushort)m_arrAddrList.Count;
		}
	}

	public override int MinimumRecordSize => 12;

	public DVRecord()
	{
	}

	public DVRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DVRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_uiOptions = GetUInt32(0);
		m_bStrListExplicit = GetBit(0, 7);
		m_bEmptyCell = GetBit(1, 0);
		m_bSuppressArrow = GetBit(1, 1);
		m_bShowPromptBox = GetBit(2, 2);
		m_bShowErrorBox = GetBit(2, 3);
		int offset = 4;
		PromtBoxTitle = CreateEmptyString(GetString16BitUpdateOffset(ref offset, out m_bPromptBoxShort));
		ErrorBoxTitle = CreateEmptyString(GetString16BitUpdateOffset(ref offset, out m_bErrorBoxShort));
		PromtBoxText = CreateEmptyString(GetString16BitUpdateOffset(ref offset, out m_bPromptBoxTextShort));
		ErrorBoxText = CreateEmptyString(GetString16BitUpdateOffset(ref offset, out m_bErrorBoxTextShort));
		ushort uInt = GetUInt16(offset);
		offset += 4;
		byte[] bytes = GetBytes(offset, uInt);
		offset += uInt;
		ushort uInt2 = GetUInt16(offset);
		offset += 4;
		byte[] bytes2 = GetBytes(offset, uInt2);
		offset += uInt2;
		ByteArrayDataProvider byteArrayDataProvider = new ByteArrayDataProvider(bytes);
		m_arrFirstFormulaTokens = FormulaUtil.ParseExpression(byteArrayDataProvider, uInt, OfficeVersion.Excel97to2003);
		byteArrayDataProvider.SetBuffer(bytes2);
		m_arrSecondFormulaTokens = FormulaUtil.ParseExpression(byteArrayDataProvider, uInt2, OfficeVersion.Excel97to2003);
		m_usAddrListSize = GetUInt16(offset);
		offset += 2;
		m_arrAddrList.Clear();
		int num = 0;
		while (num < m_usAddrListSize)
		{
			m_arrAddrList.Add(GetAddr(offset));
			num++;
			offset += 8;
		}
		if (offset != m_iLength)
		{
			throw new WrongBiffRecordDataException();
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		m_data = new byte[m_iLength];
		SetUInt32(0, m_uiOptions);
		SetBit(0, m_bStrListExplicit, 7);
		SetBit(1, m_bEmptyCell, 0);
		SetBit(1, m_bSuppressArrow, 1);
		SetBit(2, m_bShowPromptBox, 2);
		SetBit(2, m_bShowErrorBox, 3);
		int offset = 4;
		SetString16BitUpdateOffset(ref offset, CreateNotEmptyString(m_strPromtBoxTitle), m_bPromptBoxShort);
		SetString16BitUpdateOffset(ref offset, CreateNotEmptyString(m_strErrorBoxTitle), m_bErrorBoxShort);
		SetString16BitUpdateOffset(ref offset, CreateNotEmptyString(m_strPromtBoxText), m_bPromptBoxTextShort);
		SetString16BitUpdateOffset(ref offset, CreateNotEmptyString(m_strErrorBoxText), m_bErrorBoxTextShort);
		byte[] array = FormulaUtil.PtgArrayToByteArray(m_arrFirstFormulaTokens, version);
		ushort num = (ushort)((array != null) ? ((uint)array.Length) : 0u);
		SetUInt16(offset, num);
		offset += 2;
		SetUInt16(offset, 0);
		offset += 2;
		if (num > 0)
		{
			SetBytes(offset, array, 0, num);
			offset += num;
		}
		byte[] array2 = FormulaUtil.PtgArrayToByteArray(m_arrSecondFormulaTokens, version);
		ushort num2 = (ushort)((array2 != null) ? ((uint)array2.Length) : 0u);
		SetUInt16(offset, num2);
		offset += 2;
		SetUInt16(offset, 0);
		offset += 2;
		if (num2 > 0)
		{
			SetBytes(offset, array2, 0, num2);
			offset += num2;
		}
		SetUInt16(offset, m_usAddrListSize);
		offset += 2;
		int num3 = 0;
		while (num3 < m_usAddrListSize)
		{
			SetAddr(offset, m_arrAddrList[num3]);
			num3++;
			offset += 8;
		}
	}

	public void Add(TAddr addrToAdd)
	{
		m_arrAddrList.Add(addrToAdd);
		m_usAddrListSize++;
	}

	public void AddRange(TAddr[] addrToAdd)
	{
		m_arrAddrList.AddRange(addrToAdd);
		m_usAddrListSize = (ushort)m_arrAddrList.Count;
	}

	public void AddRange(ICollection<TAddr> addrToAdd)
	{
		m_arrAddrList.AddRange(addrToAdd);
		m_usAddrListSize = (ushort)m_arrAddrList.Count;
	}

	public void ClearAddressList()
	{
		m_arrAddrList.Clear();
	}

	public static int GetFormulaSize(Ptg[] arrTokens, OfficeVersion version, bool addAdditionalDataSize)
	{
		if (arrTokens == null)
		{
			return 0;
		}
		int num = arrTokens.Length;
		if (num == 0)
		{
			return 0;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Ptg ptg = arrTokens[i];
			num2 += ptg.GetSize(version);
			if (addAdditionalDataSize && ptg is IAdditionalData additionalData)
			{
				num2 += additionalData.AdditionalDataSize;
			}
		}
		return num2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 14 + Get16BitStringSize(CreateNotEmptyString(m_strPromtBoxTitle), m_bPromptBoxShort) + Get16BitStringSize(CreateNotEmptyString(m_strErrorBoxTitle), m_bErrorBoxShort) + Get16BitStringSize(CreateNotEmptyString(m_strPromtBoxText), m_bPromptBoxTextShort) + Get16BitStringSize(CreateNotEmptyString(m_strErrorBoxText), m_bErrorBoxTextShort) + GetFormulaSize(m_arrFirstFormulaTokens, version, addAdditionalDataSize: true) + GetFormulaSize(m_arrSecondFormulaTokens, version, addAdditionalDataSize: true) + m_usAddrListSize * 8;
	}

	private string CreateNotEmptyString(string strToModify)
	{
		if (strToModify == null || strToModify.Length == 0)
		{
			strToModify = "\0";
		}
		return strToModify;
	}

	private string CreateEmptyString(string strToModify)
	{
		if (strToModify == "\0")
		{
			strToModify = string.Empty;
		}
		return strToModify;
	}

	public new object Clone()
	{
		DVRecord dVRecord = (DVRecord)base.Clone();
		dVRecord.m_arrFirstFormulaTokens = CloneUtils.ClonePtgArray(m_arrFirstFormulaTokens);
		dVRecord.m_arrSecondFormulaTokens = CloneUtils.ClonePtgArray(m_arrSecondFormulaTokens);
		int count = m_arrAddrList.Count;
		dVRecord.m_arrAddrList = new List<TAddr>(count);
		for (int i = 0; i < count; i++)
		{
			dVRecord.m_arrAddrList.Add(m_arrAddrList[i]);
		}
		return dVRecord;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DVRecord dVRecord))
		{
			return false;
		}
		if (dVRecord.IsStrListExplicit == IsStrListExplicit && dVRecord.IsEmptyCell == IsEmptyCell && dVRecord.IsSuppressArrow == IsSuppressArrow && dVRecord.IsShowPromptBox == IsShowPromptBox && dVRecord.IsShowErrorBox == IsShowErrorBox && dVRecord.DataType == DataType && dVRecord.ErrorStyle == ErrorStyle && dVRecord.Condition == Condition && dVRecord.PromtBoxTitle == PromtBoxTitle && dVRecord.ErrorBoxTitle == ErrorBoxTitle && dVRecord.PromtBoxText == PromtBoxText && dVRecord.ErrorBoxText == ErrorBoxText && Ptg.CompareArrays(dVRecord.FirstFormulaTokens, FirstFormulaTokens))
		{
			return Ptg.CompareArrays(dVRecord.SecondFormulaTokens, SecondFormulaTokens);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = ((m_arrFirstFormulaTokens != null) ? m_arrFirstFormulaTokens.Length : 0);
		int num2 = ((m_arrSecondFormulaTokens != null) ? m_arrSecondFormulaTokens.Length : 0);
		int num3 = 0;
		for (int i = 0; i < AddrList.Length; i++)
		{
			num3 += AddrList.GetValue(i).GetHashCode();
		}
		return IsStrListExplicit.GetHashCode() ^ IsEmptyCell.GetHashCode() ^ IsSuppressArrow.GetHashCode() ^ IsShowPromptBox.GetHashCode() ^ IsShowErrorBox.GetHashCode() ^ DataType.GetHashCode() ^ ErrorStyle.GetHashCode() ^ Condition.GetHashCode() ^ PromtBoxTitle.GetHashCode() ^ ErrorBoxTitle.GetHashCode() ^ PromtBoxText.GetHashCode() ^ ErrorBoxText.GetHashCode() ^ num.GetHashCode() ^ num2.GetHashCode() ^ AddrListSize.GetHashCode() ^ num3.GetHashCode();
	}
}
