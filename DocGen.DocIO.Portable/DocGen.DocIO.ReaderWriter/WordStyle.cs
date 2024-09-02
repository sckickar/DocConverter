using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordStyle
{
	private string m_strName;

	private readonly WordStyleSheet m_styleSheet;

	private int m_baseStyleIndex = 4095;

	private int m_nextStyleIndex = 4095;

	private int m_linkStyleIndex;

	private int m_id = -1;

	internal static readonly WordStyle Empty = new WordStyle();

	private WordStyleType m_typeCode;

	private byte[] m_tapx;

	private byte m_bFlags;

	private CharacterPropertyException m_chpx;

	private ParagraphPropertyException m_papx;

	internal byte[] TableStyleData
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

	internal WordStyleType TypeCode
	{
		get
		{
			return m_typeCode;
		}
		set
		{
			m_typeCode = value;
		}
	}

	internal int BaseStyleIndex
	{
		get
		{
			return m_baseStyleIndex;
		}
		set
		{
			m_baseStyleIndex = value;
		}
	}

	internal int ID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	internal bool IsCharacterStyle
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal string Name
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

	internal bool HasUpe
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal int NextStyleIndex
	{
		get
		{
			return m_nextStyleIndex;
		}
		set
		{
			m_nextStyleIndex = value;
		}
	}

	internal int LinkStyleIndex
	{
		get
		{
			return m_linkStyleIndex;
		}
		set
		{
			m_linkStyleIndex = value;
		}
	}

	internal bool IsPrimary
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsSemiHidden
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool UnhideWhenUsed
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal WordStyleSheet StyleSheet => m_styleSheet;

	internal CharacterPropertyException CHPX
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

	internal ParagraphPropertyException PAPX
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

	internal WordStyle()
	{
	}

	internal WordStyle(WordStyleSheet styleSheet, string name)
	{
		m_strName = name;
		m_styleSheet = styleSheet;
		m_chpx = new CharacterPropertyException();
		m_papx = new ParagraphPropertyException();
	}

	internal WordStyle(WordStyleSheet styleSheet, string name, bool isCharacterStyle)
	{
		m_strName = name;
		m_styleSheet = styleSheet;
		IsCharacterStyle = isCharacterStyle;
		m_chpx = new CharacterPropertyException();
		if (!IsCharacterStyle)
		{
			m_papx = new ParagraphPropertyException();
		}
	}

	internal void UpdateName(string name)
	{
		m_strName = name;
	}
}
