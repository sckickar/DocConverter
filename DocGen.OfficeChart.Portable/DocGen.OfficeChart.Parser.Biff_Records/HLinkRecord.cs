using System;
using System.IO;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.HLink)]
[CLSCompliant(false)]
internal class HLinkRecord : BiffRecordRawWithArray
{
	public static readonly Guid GUID_STDLINK = new Guid("79EAC9D0-BAF9-11CE-8C82-00AA004BA90B");

	public static readonly Guid GUID_URLMONIKER = new Guid("79EAC9E0-BAF9-11CE-8C82-00AA004BA90B");

	public static readonly Guid GUID_FILEMONIKER = new Guid("00000303-0000-0000-C000-000000000046");

	public static readonly byte[] GUID_STDLINK_BYTES = GUID_STDLINK.ToByteArray();

	public static readonly byte[] GUID_URLMONIKER_BYTES = GUID_URLMONIKER.ToByteArray();

	public static readonly byte[] GUID_FILEMONIKER_BYTES = GUID_FILEMONIKER.ToByteArray();

	public static readonly byte[] FILE_UNKNOWN = new byte[24]
	{
		255, 255, 173, 222, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0
	};

	public static readonly byte[] FILE_UNKNOWN2 = new byte[2] { 3, 0 };

	public const int GUID_LENGTH = 16;

	public const int STDLINK_START_BYTE = 8;

	public const int URLMONIKER_START_BYTE = 0;

	public const int FILEMONIKER_START_BYTE = 0;

	[BiffRecordPos(0, 2)]
	private uint m_usFirstRow;

	[BiffRecordPos(2, 2)]
	private uint m_usLastRow;

	[BiffRecordPos(4, 2)]
	private uint m_usFirstColumn;

	[BiffRecordPos(6, 2)]
	private uint m_usLastColumn;

	[BiffRecordPos(24, 4)]
	private uint m_uiUnknown = 2u;

	[BiffRecordPos(28, 4)]
	private uint m_uiOptions;

	[BiffRecordPos(28, 0, TFieldType.Bit)]
	private bool m_bFileOrUrl;

	[BiffRecordPos(28, 1, TFieldType.Bit)]
	private bool m_bAbsolutePathOrUrl;

	[BiffRecordPos(28, 2, TFieldType.Bit)]
	private bool m_bDescription1;

	[BiffRecordPos(28, 3, TFieldType.Bit)]
	private bool m_bTextMark;

	[BiffRecordPos(28, 4, TFieldType.Bit)]
	private bool m_bDescription2;

	[BiffRecordPos(28, 7, TFieldType.Bit)]
	private bool m_bTargetFrame;

	[BiffRecordPos(29, 0, TFieldType.Bit)]
	private bool m_bUncPath;

	private uint m_uiDescriptionLen;

	private string m_strDescription = string.Empty;

	private uint m_uiTargetFrameLen;

	private string m_strTargetFrame = string.Empty;

	private uint m_uiTextMarkLen;

	private string m_strTextMark = string.Empty;

	private ExcelHyperLinkType m_LinkType;

	private uint m_uiUrlLen;

	private string m_strUrl = string.Empty;

	private ushort m_usDirUpLevel;

	private uint m_uiFileNameLen;

	private string m_strFileName = string.Empty;

	private uint m_uiFollowSize;

	private uint m_uiXFilePathLen;

	private string m_strXFilePath;

	private uint m_uiUncLen;

	private string m_strUnc;

	public uint FirstRow
	{
		get
		{
			return m_usFirstRow;
		}
		set
		{
			m_usFirstRow = value;
		}
	}

	public uint FirstColumn
	{
		get
		{
			return m_usFirstColumn;
		}
		set
		{
			m_usFirstColumn = value;
		}
	}

	public uint LastRow
	{
		get
		{
			return m_usLastRow;
		}
		set
		{
			m_usLastRow = value;
		}
	}

	public uint LastColumn
	{
		get
		{
			return m_usLastColumn;
		}
		set
		{
			m_usLastColumn = value;
		}
	}

	public uint Unknown => m_uiUnknown;

	public uint Options => m_uiOptions;

	public bool IsFileOrUrl
	{
		get
		{
			return m_bFileOrUrl;
		}
		set
		{
			m_bFileOrUrl = value;
		}
	}

	public bool IsAbsolutePathOrUrl
	{
		get
		{
			return m_bAbsolutePathOrUrl;
		}
		set
		{
			m_bAbsolutePathOrUrl = value;
		}
	}

	public bool IsDescription
	{
		get
		{
			if (m_bDescription1)
			{
				return m_bDescription2;
			}
			return false;
		}
		set
		{
			m_bDescription1 = value;
			m_bDescription2 = value;
		}
	}

	public bool IsTextMark
	{
		get
		{
			return m_bTextMark;
		}
		set
		{
			m_bTextMark = value;
		}
	}

	public bool IsTargetFrame
	{
		get
		{
			return m_bTargetFrame;
		}
		set
		{
			m_bTargetFrame = value;
		}
	}

	public bool IsUncPath
	{
		get
		{
			return m_bUncPath;
		}
		set
		{
			m_bUncPath = value;
		}
	}

	public bool CanBeUrl
	{
		get
		{
			if (IsFileOrUrl && IsAbsolutePathOrUrl)
			{
				return !IsUncPath;
			}
			return false;
		}
		set
		{
			IsFileOrUrl = value;
			IsAbsolutePathOrUrl = value;
			IsUncPath = !value;
		}
	}

	public bool CanBeFile
	{
		get
		{
			if (IsFileOrUrl)
			{
				return !IsUncPath;
			}
			return false;
		}
		set
		{
			IsFileOrUrl = value;
			IsUncPath = !value;
		}
	}

	public bool CanBeUnc
	{
		get
		{
			if (IsFileOrUrl && IsAbsolutePathOrUrl)
			{
				return IsUncPath;
			}
			return false;
		}
		set
		{
			if (value)
			{
				IsFileOrUrl = value;
				IsAbsolutePathOrUrl = value;
				IsUncPath = value;
			}
		}
	}

	public bool CanBeWorkbook
	{
		get
		{
			if (!IsFileOrUrl && !IsAbsolutePathOrUrl && IsTextMark)
			{
				return !IsUncPath;
			}
			return false;
		}
		set
		{
			if (value)
			{
				IsFileOrUrl = false;
				IsAbsolutePathOrUrl = false;
				IsTextMark = true;
				IsUncPath = false;
			}
		}
	}

	public uint DescriptionLen => m_uiDescriptionLen;

	public string Description
	{
		get
		{
			return m_strDescription;
		}
		set
		{
			if (m_strDescription != value)
			{
				m_strDescription = ((value != null) ? value : string.Empty);
				m_uiDescriptionLen = ((value != null) ? ((uint)(m_strDescription.Length + 1)) : 0u);
				IsDescription = true;
			}
		}
	}

	public uint TargetFrameLen => m_uiTargetFrameLen;

	public string TargetFrame
	{
		get
		{
			return m_strTargetFrame;
		}
		set
		{
			if (m_strTargetFrame != value)
			{
				m_strTargetFrame = ((value != null) ? value : string.Empty);
				m_uiTargetFrameLen = ((value != null) ? ((uint)(m_strTargetFrame.Length + 1)) : 0u);
			}
		}
	}

	public uint TextMarkLen => m_uiTextMarkLen;

	public string TextMark
	{
		get
		{
			return m_strTextMark;
		}
		set
		{
			if (m_strTextMark != value)
			{
				m_strTextMark = ((value != null) ? value : string.Empty);
				m_uiTextMarkLen = ((value != null) ? ((uint)(m_strTextMark.Length + 1)) : 0u);
				m_bTextMark = true;
			}
		}
	}

	public ExcelHyperLinkType LinkType
	{
		get
		{
			return m_LinkType;
		}
		set
		{
			switch (value)
			{
			case ExcelHyperLinkType.File:
				CanBeFile = true;
				break;
			case ExcelHyperLinkType.Unc:
				CanBeUnc = true;
				break;
			case ExcelHyperLinkType.Url:
				CanBeUrl = true;
				break;
			case ExcelHyperLinkType.Workbook:
				CanBeWorkbook = true;
				break;
			}
			m_LinkType = value;
		}
	}

	public bool IsUrl
	{
		get
		{
			return m_LinkType == ExcelHyperLinkType.Url;
		}
		set
		{
			if (value)
			{
				m_LinkType = ExcelHyperLinkType.Url;
				CanBeUrl = true;
			}
		}
	}

	public bool IsFileName
	{
		get
		{
			return m_LinkType == ExcelHyperLinkType.File;
		}
		set
		{
			if (value)
			{
				m_LinkType = ExcelHyperLinkType.File;
				CanBeFile = true;
			}
		}
	}

	public uint UrlLen => m_uiUrlLen;

	public string Url
	{
		get
		{
			return m_strUrl;
		}
		set
		{
			if (m_strUrl != value)
			{
				m_strUrl = value;
				m_uiUrlLen = ((value != null) ? ((uint)(value.Length * 2 + 2)) : 2u);
				IsUrl = true;
			}
		}
	}

	public ushort DirUpLevel
	{
		get
		{
			return m_usDirUpLevel;
		}
		set
		{
			m_usDirUpLevel = value;
		}
	}

	public uint FileNameLen => m_uiFileNameLen;

	public string FileName
	{
		get
		{
			return m_strFileName;
		}
		set
		{
			m_strFileName = ((value != null) ? value : string.Empty);
			m_uiFileNameLen = (uint)m_strFileName.Length;
			IsFileName = true;
		}
	}

	public uint FollowSize => m_uiFollowSize;

	public uint XFilePathLen => m_uiXFilePathLen;

	public string XFilePath
	{
		get
		{
			return m_strXFilePath;
		}
		set
		{
			m_strXFilePath = ((value != null) ? value : string.Empty);
			m_uiXFilePathLen = (uint)(m_strXFilePath.Length * 2 + 2);
		}
	}

	public uint UncLen => m_uiUncLen;

	public string UncPath
	{
		get
		{
			return m_strUnc;
		}
		set
		{
			m_strUnc = value;
			m_uiUncLen = (uint)(m_strUnc.Length + 1);
		}
	}

	public HLinkRecord()
	{
	}

	public HLinkRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public HLinkRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_usFirstRow = GetUInt16(0);
		m_usLastRow = GetUInt16(2);
		m_usFirstColumn = GetUInt16(4);
		m_usLastColumn = GetUInt16(6);
		m_uiUnknown = GetUInt32(24);
		m_uiOptions = GetUInt32(28);
		m_bFileOrUrl = GetBit(28, 0);
		m_bAbsolutePathOrUrl = GetBit(28, 1);
		m_bDescription1 = GetBit(28, 2);
		m_bTextMark = GetBit(28, 3);
		m_bDescription2 = GetBit(28, 4);
		m_bTargetFrame = GetBit(28, 7);
		m_bUncPath = GetBit(29, 0);
		int iOffset = 32;
		if (IsDescription)
		{
			ParseDescription(ref iOffset);
		}
		if (IsTargetFrame)
		{
			ParseTargetFrame(ref iOffset);
		}
		ParseSpecialData(ref iOffset);
		if (IsTextMark)
		{
			ParseTextMark(ref iOffset);
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		SetOptionFlags();
		m_uiUnknown = 2u;
		AutoGrowData = true;
		SetUInt16(0, (ushort)m_usFirstRow);
		SetUInt16(2, (ushort)m_usLastRow);
		SetUInt16(4, (ushort)m_usFirstColumn);
		SetUInt16(6, (ushort)m_usLastColumn);
		SetUInt32(24, m_uiUnknown);
		SetUInt32(28, m_uiOptions);
		SetBit(28, m_bFileOrUrl, 0);
		SetBit(28, m_bAbsolutePathOrUrl, 1);
		SetBit(28, m_bDescription1, 2);
		SetBit(28, m_bTextMark, 3);
		SetBit(28, m_bDescription2, 4);
		SetBit(28, m_bTargetFrame, 7);
		SetBit(29, m_bUncPath, 0);
		m_iLength = 32;
		SetBytes(8, GUID_STDLINK_BYTES, 0, GUID_STDLINK_BYTES.Length);
		if (IsDescription)
		{
			InfillLenAndString(ref m_uiDescriptionLen, ref m_strDescription, bBytesCount: false);
		}
		if (IsTargetFrame)
		{
			InfillLenAndString(ref m_uiTargetFrameLen, ref m_strTargetFrame, bBytesCount: false);
		}
		InfillSpecialData();
		if (IsTextMark)
		{
			InfillLenAndString(ref m_uiTextMarkLen, ref m_strTextMark, bBytesCount: false);
		}
	}

	private void ParseDescription(ref int iOffset)
	{
		if (!IsDescription)
		{
			throw new ArgumentException("There is no description.");
		}
		m_uiDescriptionLen = GetUInt32(iOffset);
		iOffset += 4;
		if (iOffset + m_uiDescriptionLen * 2 > m_iLength)
		{
			throw new WrongBiffRecordDataException("Description");
		}
		m_strDescription = Encoding.Unicode.GetString(GetBytes(iOffset, (int)(m_uiDescriptionLen * 2)), 0, (int)(m_uiDescriptionLen * 2 - 2));
		iOffset += (int)(m_uiDescriptionLen * 2);
	}

	private void ParseTargetFrame(ref int iOffset)
	{
		if (!IsTargetFrame)
		{
			throw new ArgumentException("There is no target frame.");
		}
		m_uiTargetFrameLen = GetUInt32(iOffset);
		iOffset += 4;
		m_strTargetFrame = Encoding.Unicode.GetString(GetBytes(iOffset, (int)(m_uiTargetFrameLen * 2)), 0, (int)(m_uiTargetFrameLen * 2 - 2));
		iOffset += (int)(m_uiTargetFrameLen * 2);
	}

	private void ParseSpecialData(ref int iOffset)
	{
		if (CheckUrl(ref iOffset))
		{
			LinkType = ExcelHyperLinkType.Url;
			ParseUrl(ref iOffset);
		}
		else if (CheckLocalFile(ref iOffset))
		{
			LinkType = ExcelHyperLinkType.File;
			ParseFile(ref iOffset);
		}
		else if (CheckUnc(ref iOffset))
		{
			LinkType = ExcelHyperLinkType.Unc;
			ParseUnc(ref iOffset);
		}
		else
		{
			LinkType = ExcelHyperLinkType.Workbook;
			ParseWorkbook(ref iOffset);
		}
	}

	private void ParseTextMark(ref int iOffset)
	{
		if (!IsTextMark)
		{
			throw new ArgumentException("There is no text mark.");
		}
		if (iOffset < m_iLength)
		{
			m_uiTextMarkLen = GetUInt32(iOffset);
			iOffset += 4;
			m_strTextMark = Encoding.Unicode.GetString(m_data, iOffset, (int)((m_uiTextMarkLen - 1) * 2));
			iOffset += (int)(m_uiTextMarkLen * 2);
		}
	}

	private bool CheckUrl(ref int iOffset)
	{
		bool num = BiffRecordRaw.CompareArrays(m_data, iOffset, GUID_URLMONIKER_BYTES, 0, 16);
		if (num)
		{
			iOffset += 16;
		}
		return num;
	}

	private bool CheckLocalFile(ref int iOffset)
	{
		int num;
		if (CanBeFile)
		{
			num = (BiffRecordRaw.CompareArrays(m_data, iOffset, GUID_FILEMONIKER_BYTES, 0, 16) ? 1 : 0);
			if (num != 0)
			{
				iOffset += 16;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool CheckUnc(ref int iOffset)
	{
		return CanBeUnc;
	}

	private void ParseUrl(ref int iOffset)
	{
		m_uiUrlLen = GetUInt32(iOffset);
		iOffset += 4;
		m_strUrl = Encoding.Unicode.GetString(m_data, iOffset, (int)m_uiUrlLen);
		int num = m_strUrl.IndexOf('\0');
		if (num != -1)
		{
			m_strUrl = m_strUrl.Substring(0, num);
		}
		iOffset += (int)m_uiUrlLen;
	}

	private void ParseFile(ref int iOffset)
	{
		m_usDirUpLevel = GetUInt16(iOffset);
		iOffset += 2;
		m_uiFileNameLen = GetUInt32(iOffset);
		iOffset += 4;
		m_strFileName = BiffRecordRaw.LatinEncoding.GetString(m_data, iOffset, (int)(m_uiFileNameLen - 1));
		iOffset += (int)m_uiFileNameLen;
		m_uiFileNameLen--;
		iOffset += FILE_UNKNOWN.Length;
		m_uiFollowSize = GetUInt32(iOffset);
		iOffset += 4;
		if (m_uiFollowSize != 0)
		{
			int @int = GetInt32(iOffset);
			iOffset += 4;
			iOffset += 2;
			m_strXFilePath = Encoding.Unicode.GetString(m_data, iOffset, @int);
			iOffset += @int;
		}
	}

	private void ParseUnc(ref int iOffset)
	{
		m_uiUncLen = GetUInt32(iOffset);
		iOffset += 4;
		m_strUnc = Encoding.Unicode.GetString(m_data, iOffset, (int)(m_uiUncLen * 2 - 2));
		iOffset += (int)(m_uiUncLen * 2);
	}

	private void ParseWorkbook(ref int iOffset)
	{
	}

	private void InfillLenAndString(ref uint uiLen, ref string strValue, bool bBytesCount)
	{
		if (strValue == null || strValue.Length == 0)
		{
			strValue = "\0";
		}
		uiLen = (uint)strValue.Length;
		if (strValue[(int)(uiLen - 1)] != 0)
		{
			strValue += "\0";
			uiLen++;
		}
		if (bBytesCount)
		{
			uiLen *= 2u;
		}
		SetUInt32(m_iLength, uiLen);
		m_iLength += 4;
		byte[] bytes = Encoding.Unicode.GetBytes(strValue);
		SetBytes(m_iLength, bytes);
		m_iLength += bytes.Length;
	}

	private void InfillSpecialData()
	{
		switch (LinkType)
		{
		case ExcelHyperLinkType.File:
			InfillFileSpecialData();
			break;
		case ExcelHyperLinkType.Unc:
			InfillUncSpecialData();
			break;
		case ExcelHyperLinkType.Url:
			InfillUrlSpecialData();
			break;
		case ExcelHyperLinkType.Workbook:
			InfillWorkbookSpecialData();
			break;
		}
	}

	private void InfillFileSpecialData()
	{
		SetBytes(m_iLength, GUID_FILEMONIKER_BYTES);
		m_iLength += GUID_FILEMONIKER_BYTES.Length;
		SetUInt16(m_iLength, m_usDirUpLevel);
		m_iLength += 2;
		if (m_strFileName[(int)(m_uiFileNameLen - 1)] != 0)
		{
			m_uiFileNameLen++;
			m_strFileName += "\0";
		}
		SetUInt32(m_iLength, m_uiFileNameLen);
		m_iLength += 4;
		byte[] bytes = BiffRecordRaw.LatinEncoding.GetBytes(m_strFileName);
		SetBytes(m_iLength, bytes);
		m_iLength += bytes.Length;
		SetBytes(m_iLength, FILE_UNKNOWN);
		m_iLength += FILE_UNKNOWN.Length;
		if (m_strXFilePath != null && m_strXFilePath.Length != 0)
		{
			m_uiFollowSize = (uint)(6 + m_strXFilePath.Length * 2);
		}
		else
		{
			m_uiFollowSize = 0u;
		}
		SetUInt32(m_iLength, m_uiFollowSize);
		m_iLength += 4;
		if (m_uiFollowSize != 0)
		{
			m_uiXFilePathLen = (uint)(m_strXFilePath.Length * 2);
			SetUInt32(m_iLength, m_uiXFilePathLen);
			m_iLength += 4;
			SetBytes(m_iLength, FILE_UNKNOWN2);
			m_iLength += FILE_UNKNOWN2.Length;
			byte[] bytes2 = Encoding.Unicode.GetBytes(m_strXFilePath);
			SetBytes(m_iLength, bytes2);
			m_iLength += bytes2.Length;
		}
	}

	private void InfillUncSpecialData()
	{
		CanBeUnc = true;
		InfillLenAndString(ref m_uiUncLen, ref m_strUnc, bBytesCount: false);
	}

	private void InfillUrlSpecialData()
	{
		CanBeUrl = true;
		SetBytes(m_iLength, GUID_URLMONIKER_BYTES);
		m_iLength += GUID_URLMONIKER_BYTES.Length;
		InfillLenAndString(ref m_uiUrlLen, ref m_strUrl, bBytesCount: true);
	}

	private void InfillWorkbookSpecialData()
	{
		CanBeWorkbook = true;
	}

	private void SetOptionFlags()
	{
		switch (LinkType)
		{
		case ExcelHyperLinkType.File:
			CanBeFile = true;
			break;
		case ExcelHyperLinkType.Unc:
			CanBeUnc = true;
			break;
		case ExcelHyperLinkType.Url:
			CanBeUrl = true;
			break;
		case ExcelHyperLinkType.Workbook:
			CanBeWorkbook = true;
			break;
		}
	}
}
