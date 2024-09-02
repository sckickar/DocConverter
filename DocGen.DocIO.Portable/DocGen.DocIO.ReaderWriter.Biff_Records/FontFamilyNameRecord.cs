using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class FontFamilyNameRecord : BaseWordRecord
{
	private const int DEF_MAX_LENGTH = 130;

	private const int DEF_PITCHREQUEST_MASK = 3;

	private const int DEF_ISTRUETYPE_MASK = 4;

	private const int DEF_ISTRUETYPE_OFFSET = 2;

	private const int DEF_FONTFAMILYID_MASK = 112;

	private const int DEF_FONTFAMILYID_OFFSET = 4;

	private byte m_bflags;

	private FFNBaseStructure m_ffnBase = new FFNBaseStructure();

	private string m_strFontName = string.Empty;

	private string m_strAltFontName = string.Empty;

	private byte[] m_dbgFontName;

	private Dictionary<string, Dictionary<string, DictionaryEntry>> m_embedFonts;

	protected override IDataStructure UnderlyingStructure => m_ffnBase;

	private byte TotalLengthM1 => m_ffnBase.TotalLengthM1;

	internal override int Length => TotalLengthM1 + 1;

	internal byte PitchRequest
	{
		get
		{
			return (byte)BaseWordRecord.GetBitsByMask(m_ffnBase.Options, 3, 0);
		}
		set
		{
			if (PitchRequest != value)
			{
				m_ffnBase.Options = (byte)BaseWordRecord.SetBitsByMask(m_ffnBase.Options, 3, value);
			}
		}
	}

	internal bool IsSubsetted
	{
		get
		{
			return (m_bflags & 4) >> 2 != 0;
		}
		set
		{
			m_bflags = (byte)((m_bflags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool TrueType
	{
		get
		{
			return BaseWordRecord.GetBit(m_ffnBase.Options, 4);
		}
		set
		{
			if (TrueType != value)
			{
				m_ffnBase.Options = (byte)BaseWordRecord.SetBit(m_ffnBase.Options, 2, value);
			}
		}
	}

	internal byte FontFamilyID
	{
		get
		{
			return (byte)BaseWordRecord.GetBitsByMask(m_ffnBase.Options, 112, 4);
		}
		set
		{
			if (FontFamilyID != value)
			{
				m_ffnBase.Options = (byte)BaseWordRecord.SetBitsByMask(m_ffnBase.Options, 112, value << 4);
			}
		}
	}

	internal short Weight
	{
		get
		{
			return m_ffnBase.Weight;
		}
		set
		{
			if (m_ffnBase.Weight != value)
			{
				m_ffnBase.Weight = value;
			}
		}
	}

	internal byte[] SigUsb0
	{
		get
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_ffnBase.m_FONTSIGNATURE[i];
			}
			return array;
		}
		set
		{
			for (int i = 0; i < 4; i++)
			{
				m_ffnBase.m_FONTSIGNATURE[i] = value[i];
			}
		}
	}

	internal byte[] SigUsb1
	{
		get
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_ffnBase.m_FONTSIGNATURE[4 + i];
			}
			return array;
		}
		set
		{
			for (int i = 0; i < 4; i++)
			{
				m_ffnBase.m_FONTSIGNATURE[4 + i] = value[i];
			}
		}
	}

	internal byte[] SigUsb2
	{
		get
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_ffnBase.m_FONTSIGNATURE[8 + i];
			}
			return array;
		}
		set
		{
			for (int i = 0; i < 4; i++)
			{
				m_ffnBase.m_FONTSIGNATURE[8 + i] = value[i];
			}
		}
	}

	internal byte[] SigUsb3
	{
		get
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_ffnBase.m_FONTSIGNATURE[12 + i];
			}
			return array;
		}
		set
		{
			for (int i = 0; i < 4; i++)
			{
				m_ffnBase.m_FONTSIGNATURE[12 + i] = value[i];
			}
		}
	}

	internal byte[] SigCsb0
	{
		get
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_ffnBase.m_FONTSIGNATURE[16 + i];
			}
			return array;
		}
		set
		{
			for (int i = 0; i < 4; i++)
			{
				m_ffnBase.m_FONTSIGNATURE[16 + i] = value[i];
			}
		}
	}

	internal byte[] SigCsb1
	{
		get
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_ffnBase.m_FONTSIGNATURE[20 + i];
			}
			return array;
		}
		set
		{
			for (int i = 0; i < 4; i++)
			{
				m_ffnBase.m_FONTSIGNATURE[20 + i] = value[i];
			}
		}
	}

	internal byte CharacterSetId
	{
		get
		{
			return m_ffnBase.CharacterSetId;
		}
		set
		{
			if (m_ffnBase.CharacterSetId != value)
			{
				m_ffnBase.CharacterSetId = value;
			}
		}
	}

	internal string FontName
	{
		get
		{
			return m_strFontName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FontName is empty or nullable!");
			}
			if (value.Length + m_strAltFontName.Length > 130)
			{
				throw new ArgumentOutOfRangeException("FontName can not be large ( 65 - " + m_strAltFontName.ToString() + " ) symbols");
			}
			if (m_strFontName != value || value == string.Empty)
			{
				Encoding unicode = Encoding.Unicode;
				m_strFontName = ((value == string.Empty) ? "\0" : value);
				int byteCount = unicode.GetByteCount(m_strFontName);
				int byteCount2 = unicode.GetByteCount(m_strAltFontName);
				int num = m_ffnBase.Length + byteCount + 1 + ((byteCount2 > 0) ? (byteCount2 + 2) : 0);
				m_ffnBase.TotalLengthM1 = (byte)num;
			}
		}
	}

	internal string AlternativeFontName
	{
		get
		{
			return m_strAltFontName;
		}
		set
		{
			m_strAltFontName = value;
		}
	}

	internal Dictionary<string, Dictionary<string, DictionaryEntry>> EmbedFonts
	{
		get
		{
			if (m_embedFonts == null)
			{
				m_embedFonts = new Dictionary<string, Dictionary<string, DictionaryEntry>>();
			}
			return m_embedFonts;
		}
		set
		{
			m_embedFonts = value;
		}
	}

	internal FontFamilyNameRecord()
	{
	}

	internal int ParseBytes(byte[] arrData, int iOffset, int iCount)
	{
		base.Parse(arrData, iOffset, iCount);
		int length = m_ffnBase.Length;
		iOffset += length;
		int num = Length - length;
		if (num > 130)
		{
			num = 130;
		}
		m_dbgFontName = new byte[num];
		Array.Copy(arrData, iOffset, m_dbgFontName, 0, num);
		m_strFontName = BaseWordRecord.ReadString(arrData, iOffset, (ushort)(num - 2));
		int length2 = Length;
		if (m_ffnBase.AlternateFontIndex != 0 && m_ffnBase.AlternateFontIndex < m_strFontName.Length)
		{
			m_strAltFontName = m_strFontName.Substring(m_ffnBase.AlternateFontIndex);
			m_strFontName = m_strFontName.Substring(0, m_ffnBase.AlternateFontIndex - 1);
			if (m_strFontName == string.Empty)
			{
				m_strFontName = m_strAltFontName;
				m_ffnBase.TotalLengthM1 += (byte)(m_strFontName.Length * 2);
			}
		}
		else
		{
			char[] separator = new char[1];
			string[] array = m_strFontName.Split(separator);
			m_strFontName = array[0];
		}
		return length2;
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		int num = iOffset;
		if (AlternativeFontName != string.Empty)
		{
			m_ffnBase.AlternateFontIndex = (byte)(m_strFontName.Length + 1);
			m_strFontName = m_strFontName + '\0' + AlternativeFontName;
		}
		base.Save(arrData, iOffset);
		iOffset += m_ffnBase.Length;
		BaseWordRecord.WriteString(arrData, m_strFontName, ref iOffset);
		arrData[iOffset++] = 0;
		if (Length % 2 == 0)
		{
			arrData[iOffset++] = 0;
		}
		if (iOffset - num != Length)
		{
			throw new Exception("Length of FFN record data is incorrect!");
		}
		return iOffset;
	}

	internal override void Close()
	{
		base.Close();
		if (m_ffnBase != null)
		{
			m_ffnBase.Close();
			m_ffnBase = null;
		}
		m_dbgFontName = null;
		if (m_embedFonts == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Dictionary<string, DictionaryEntry>> embedFont in m_embedFonts)
		{
			foreach (KeyValuePair<string, DictionaryEntry> item in embedFont.Value)
			{
				((MemoryStream)item.Value.Value).Dispose();
			}
			embedFont.Value.Clear();
		}
		m_embedFonts.Clear();
		m_embedFonts = null;
	}
}
