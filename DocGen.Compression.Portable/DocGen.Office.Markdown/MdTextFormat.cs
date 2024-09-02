namespace DocGen.Office.Markdown;

internal class MdTextFormat
{
	private byte m_bFlags = 1;

	private MdSubSuperScript m_SubSuperScriptType;

	internal bool Bold
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool Italic
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

	internal bool CodeSpan
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

	internal bool StrikeThrough
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

	internal bool IsHidden
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

	internal MdSubSuperScript SubSuperScriptType
	{
		get
		{
			return m_SubSuperScriptType;
		}
		set
		{
			m_SubSuperScriptType = value;
		}
	}

	internal MdTextFormat Clone()
	{
		return new MdTextFormat
		{
			Bold = Bold,
			Italic = Italic,
			StrikeThrough = StrikeThrough,
			CodeSpan = CodeSpan,
			IsHidden = IsHidden,
			SubSuperScriptType = SubSuperScriptType
		};
	}
}
