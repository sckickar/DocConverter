using System;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class StyleDefinitionRecord : BaseWordRecord
{
	private const int DEF_START_ID = 0;

	private const int DEF_MASK_ID = 4095;

	private const int DEF_BIT_SCRATCH = 12;

	private const int DEF_BIT_INVALID_HEIGHT = 13;

	private const int DEF_BIT_HAS_UPE = 14;

	private const int DEF_BIT_MASS_COPY = 15;

	private const int DEF_MASK_TYPE_CODE = 15;

	private const int DEF_START_TYPE_CODE = 0;

	private const int DEF_MASK_BASE_STYLE = 65520;

	private const int DEF_START_BASE_STYLE = 4;

	private const int DEF_MASK_UPX_NUMBER = 15;

	private const int DEF_START_UPX_NUMBER = 0;

	private const int DEF_MASK_NEXT_STYLE = 65520;

	private const int DEF_START_NEXT_STYLE = 4;

	private const int DEF_BIT_AUTO_REDEFINE = 0;

	private const int DEF_BIT_HIDDEN = 1;

	private StyleDefinitionBase m_basePart = new StyleDefinitionBase();

	private string m_strStyleName;

	private string m_aliasesStyleName;

	private UniversalPropertyException[] m_arrUpx;

	private CharacterPropertyException m_chpx;

	private ParagraphPropertyException m_papx;

	private byte[] m_data;

	private byte[] m_data1;

	private byte[] m_data2;

	private byte[] m_data3;

	private byte[] m_tapx;

	private StyleSheetInfoRecord m_shInfo;

	internal byte[] Tapx
	{
		get
		{
			return m_tapx;
		}
		set
		{
			m_tapx = value;
		}
	}

	internal ushort StyleId
	{
		get
		{
			return m_basePart.StyleId;
		}
		set
		{
			m_basePart.StyleId = value;
		}
	}

	internal bool IsScratch
	{
		get
		{
			return m_basePart.IsScratch;
		}
		set
		{
			m_basePart.IsScratch = value;
		}
	}

	internal bool IsInvalidHeight
	{
		get
		{
			return m_basePart.IsInvalidHeight;
		}
		set
		{
			m_basePart.IsInvalidHeight = value;
		}
	}

	internal bool HasUpe
	{
		get
		{
			return m_basePart.HasUpe;
		}
		set
		{
			m_basePart.HasUpe = value;
		}
	}

	internal bool IsMassCopy
	{
		get
		{
			return m_basePart.IsMassCopy;
		}
		set
		{
			m_basePart.IsMassCopy = value;
		}
	}

	internal WordStyleType TypeCode
	{
		get
		{
			return (WordStyleType)m_basePart.TypeCode;
		}
		set
		{
			if (value == WordStyleType.ParagraphStyle)
			{
				m_papx = new ParagraphPropertyException();
			}
			else if (m_basePart.TypeCode == 1)
			{
				m_papx = null;
			}
			m_basePart.TypeCode = (ushort)value;
		}
	}

	internal ushort BaseStyle
	{
		get
		{
			return m_basePart.BaseStyle;
		}
		set
		{
			m_basePart.BaseStyle = value;
		}
	}

	internal ushort UPEOffset
	{
		get
		{
			return m_basePart.UPEOffset;
		}
		set
		{
			m_basePart.UPEOffset = value;
		}
	}

	internal ushort UpxNumber
	{
		get
		{
			return m_basePart.UpxNumber;
		}
		set
		{
			m_basePart.UpxNumber = value;
		}
	}

	internal ushort NextStyleId
	{
		get
		{
			return m_basePart.NextStyleId;
		}
		set
		{
			m_basePart.NextStyleId = value;
		}
	}

	internal bool IsAutoRedefine
	{
		get
		{
			return m_basePart.IsAutoRedefine;
		}
		set
		{
			m_basePart.IsAutoRedefine = value;
		}
	}

	internal bool IsHidden
	{
		get
		{
			return m_basePart.IsHidden;
		}
		set
		{
			m_basePart.IsHidden = value;
		}
	}

	internal string StyleName
	{
		get
		{
			return m_strStyleName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("value - string can not be empty");
			}
			m_strStyleName = value;
		}
	}

	internal string AliasesStyleName
	{
		get
		{
			return m_aliasesStyleName;
		}
		set
		{
			m_aliasesStyleName = value;
		}
	}

	internal UniversalPropertyException[] PropertyExceptions => m_arrUpx;

	internal CharacterPropertyException CharacterProperty
	{
		get
		{
			return m_chpx;
		}
		set
		{
			m_chpx = value;
		}
	}

	internal ParagraphPropertyException ParagraphProperty
	{
		get
		{
			return m_papx;
		}
		set
		{
			m_papx = value;
		}
	}

	protected override IDataStructure UnderlyingStructure => m_basePart;

	internal override int Length
	{
		get
		{
			int num = Math.Max(m_shInfo.STDBaseLength, m_basePart.Length);
			if (num % 2 != 0)
			{
				num++;
			}
			if (m_strStyleName != null)
			{
				num += Encoding.Unicode.GetByteCount(m_strStyleName);
			}
			num += 4;
			if (m_tapx != null && UpxNumber == 3)
			{
				num += m_tapx.Length;
			}
			else
			{
				if (m_arrUpx != null && m_arrUpx.Length != 0)
				{
					int i = 0;
					for (int num2 = m_arrUpx.Length; i < num2; i++)
					{
						num += m_arrUpx[i].Length;
					}
				}
				if (m_arrUpx == null)
				{
					num += UpxLength;
				}
			}
			return num;
		}
	}

	internal byte[] DBG_data => m_data;

	internal byte[] DBG_data1 => m_data1;

	internal byte[] DBG_data2 => m_data2;

	internal byte[] DBG_data3 => m_data3;

	internal ushort LinkStyleId
	{
		get
		{
			return m_basePart.LinkStyleId;
		}
		set
		{
			m_basePart.LinkStyleId = value;
		}
	}

	internal bool IsQFormat
	{
		get
		{
			return m_basePart.IsQFormat;
		}
		set
		{
			m_basePart.IsQFormat = value;
		}
	}

	internal bool UnhideWhenUsed
	{
		get
		{
			return m_basePart.UnhideWhenUsed;
		}
		set
		{
			m_basePart.UnhideWhenUsed = value;
		}
	}

	internal bool IsSemiHidden
	{
		get
		{
			return m_basePart.IsSemiHidden;
		}
		set
		{
			m_basePart.IsSemiHidden = value;
		}
	}

	private int UpxLength
	{
		get
		{
			if (m_strStyleName == "No List")
			{
				return 4;
			}
			if (UpxNumber == 3 && m_tapx != null)
			{
				return m_tapx.Length;
			}
			int num = 2;
			if (m_papx != null)
			{
				num += 2 + m_papx.Length;
				if (num % 2 != 0)
				{
					num++;
				}
			}
			if (m_chpx != null)
			{
				num += m_chpx.PropertyModifiers.Length;
			}
			if (num % 2 != 0)
			{
				num++;
			}
			return num;
		}
	}

	internal override void Close()
	{
		base.Close();
		m_basePart = null;
		if (m_arrUpx != null)
		{
			m_arrUpx = null;
		}
		if (m_chpx != null)
		{
			m_chpx = null;
		}
		if (m_papx != null)
		{
			m_papx = null;
		}
		m_data = null;
		m_data1 = null;
		m_data2 = null;
		m_data3 = null;
		m_tapx = null;
		if (m_shInfo != null)
		{
			m_shInfo = null;
		}
	}

	internal void Parse(Stream stream, int iCount, StyleSheetInfoRecord info)
	{
		Clear();
		if (iCount == 0)
		{
			return;
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		byte[] buffer = new byte[iCount];
		stream.Read(buffer, 0, iCount);
		stream.Position -= iCount;
		int sTDBaseLength = info.STDBaseLength;
		int num = Math.Max(sTDBaseLength, 12);
		buffer = new byte[num];
		stream.Read(buffer, 0, sTDBaseLength);
		Parse(buffer, 0, num);
		int num2 = iCount - sTDBaseLength;
		if (sTDBaseLength % 2 != 0)
		{
			stream.Position++;
			num2--;
		}
		byte[] array = new byte[num2];
		stream.Read(array, 0, num2);
		int iEndPos;
		string zeroTerminatedString = BaseWordRecord.GetZeroTerminatedString(array, 0, out iEndPos);
		if (zeroTerminatedString.Contains(","))
		{
			string[] array2 = zeroTerminatedString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			m_strStyleName = array2[0];
			if (array2.Length > 1)
			{
				m_aliasesStyleName = string.Join(",", array2, 1, array2.Length - 1);
			}
		}
		else
		{
			m_strStyleName = zeroTerminatedString;
		}
		ParseUpxPart(array, iEndPos);
	}

	internal void Clear()
	{
		m_basePart.Clear();
		m_arrUpx = null;
		m_strStyleName = null;
	}

	internal override int Save(Stream stream)
	{
		long position = stream.Position;
		if (m_tapx != null && TypeCode == WordStyleType.TableStyle)
		{
			m_basePart.UpxNumber = 3;
		}
		else
		{
			m_basePart.UpxNumber = ((m_chpx != null) ? ((ushort)1) : ((ushort)0));
		}
		if (m_strStyleName == "No List")
		{
			TypeCode = WordStyleType.ListStyle;
			m_basePart.UPEOffset = 40;
		}
		else
		{
			m_basePart.UPEOffset = (ushort)Length;
		}
		if (m_basePart.UpxNumber > 0)
		{
			m_basePart.UpxNumber += ((m_papx != null) ? ((ushort)1) : ((ushort)0));
		}
		byte[] array = new byte[m_basePart.Length];
		base.Save(array, 0);
		stream.Write(array, 0, array.Length);
		if (m_shInfo.STDBaseLength > array.Length)
		{
			int num = m_shInfo.STDBaseLength - array.Length;
			stream.Write(new byte[num], 0, num);
		}
		string text = m_strStyleName;
		if (!string.IsNullOrEmpty(AliasesStyleName))
		{
			text = text + "," + AliasesStyleName;
		}
		byte[] array2 = BaseWordRecord.ToZeroTerminatedArray(text);
		stream.Write(array2, 0, array2.Length);
		if (m_basePart.UpxNumber > 0)
		{
			SaveUpxPart(stream);
		}
		return (int)(stream.Position - position);
	}

	internal StyleDefinitionRecord(string styleName, ushort styleId, StyleSheetInfoRecord info)
	{
		m_strStyleName = styleName;
		m_basePart.BaseStyle = 4095;
		m_basePart.HasUpe = false;
		m_basePart.NextStyleId = 0;
		m_basePart.StyleId = styleId;
		TypeCode = WordStyleType.CharacterStyle;
		m_chpx = new CharacterPropertyException();
		m_shInfo = info;
	}

	internal StyleDefinitionRecord(byte[] arrData, int iOffset, int iCount, StyleSheetInfoRecord info)
		: base(arrData, iOffset, iCount)
	{
		m_shInfo = info;
	}

	internal StyleDefinitionRecord(Stream stream, int iCount, StyleSheetInfoRecord info)
	{
		m_shInfo = info;
		Parse(stream, iCount, info);
	}

	private void ParseUpxPart(byte[] arrVariable, int iStartPos)
	{
		m_arrUpx = new UniversalPropertyException[m_basePart.UpxNumber];
		if (UpxNumber == 3)
		{
			m_tapx = new byte[arrVariable.Length - iStartPos];
			TypeCode = WordStyleType.TableStyle;
			Buffer.BlockCopy(arrVariable, iStartPos, m_tapx, 0, arrVariable.Length - iStartPos);
			return;
		}
		for (int i = 0; i < m_basePart.UpxNumber; i++)
		{
			iStartPos = MakeEven(iStartPos);
			ushort num = BitConverter.ToUInt16(arrVariable, iStartPos);
			iStartPos += 2;
			if (num != 0)
			{
				m_arrUpx[i] = new UniversalPropertyException(arrVariable, iStartPos, num);
				if (m_basePart.UpxNumber == 1 || (UpxNumber == 2 && i == 1))
				{
					m_chpx = new CharacterPropertyException(m_arrUpx[i]);
				}
				else if (m_basePart.UpxNumber == 2 && i == 0)
				{
					m_papx = new ParagraphPropertyException(m_arrUpx[i]);
				}
				iStartPos += num;
				iStartPos = MakeEven(iStartPos);
			}
		}
	}

	private int MakeEven(int iStartPos)
	{
		if (iStartPos % 2 == 0)
		{
			return iStartPos;
		}
		return ++iStartPos;
	}

	private void SaveUpxPart(Stream stream)
	{
		long position = stream.Position;
		if (UpxNumber == 3 && m_tapx != null)
		{
			stream.Write(m_tapx, 0, m_tapx.Length);
		}
		else if (m_strStyleName == "No List")
		{
			stream.Write(new byte[2] { 2, 0 }, 0, 2);
			stream.Write(BitConverter.GetBytes(StyleId), 0, 2);
		}
		else
		{
			if (m_papx != null)
			{
				ushort num = (ushort)m_papx.Length;
				byte[] bytes = BitConverter.GetBytes(num);
				stream.Write(bytes, 0, bytes.Length);
				int num2 = m_papx.Save(stream);
				if (num != num2)
				{
					throw new StreamWriteException("Incorrect writing UPX(pap) to file");
				}
				if (num % 2 != 0)
				{
					stream.WriteByte(0);
				}
			}
			ushort num3 = (ushort)m_chpx.PropertyModifiers.Length;
			byte[] bytes2 = BitConverter.GetBytes(num3);
			stream.Write(bytes2, 0, bytes2.Length);
			int num4 = m_chpx.PropertyModifiers.Save(stream);
			if (num3 != num4)
			{
				throw new StreamWriteException("Incorrect writing UPX(chp) to file");
			}
			if (num3 % 2 != 0)
			{
				stream.WriteByte(0);
			}
		}
		if (stream.Position - position != UpxLength)
		{
			throw new StreamWriteException("Incorrect writing UPX to file, invalid UPX Length");
		}
	}
}
