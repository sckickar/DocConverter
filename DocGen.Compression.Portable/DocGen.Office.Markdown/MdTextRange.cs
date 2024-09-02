namespace DocGen.Office.Markdown;

internal class MdTextRange : IMdInline
{
	private byte m_bFlags = 1;

	private string m_text;

	private MdTextFormat m_textFormat;

	internal MdTextFormat TextFormat
	{
		get
		{
			return m_textFormat;
		}
		set
		{
			m_textFormat = value;
		}
	}

	internal string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal bool IsLineBreak
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public MdTextRange()
	{
		m_textFormat = new MdTextFormat();
	}

	public void Close()
	{
	}
}
