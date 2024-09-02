using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class SyncFont
{
	private byte m_bFlags;

	private string m_fontname;

	private float m_fontsize;

	internal bool Bold
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

	internal bool Underline
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

	internal bool Strikeout
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

	public SyncFont(DocGen.Drawing.Font font)
	{
		m_fontname = font.Name;
		m_fontsize = font.SizeInPoints;
		if (font.Bold)
		{
			Bold = true;
		}
		if (font.Italic)
		{
			Italic = true;
		}
		if (font.Underline)
		{
			Underline = true;
		}
		if (font.Strikeout)
		{
			Strikeout = true;
		}
	}

	public DocGen.Drawing.Font GetFont(WordDocument Document, FontScriptType scriptType)
	{
		FontStyle fontStyle = FontStyle.Regular;
		float fontSize = ((m_fontsize == 0f) ? 0.5f : m_fontsize);
		if (Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (Underline)
		{
			fontStyle |= FontStyle.Underline;
		}
		if (Strikeout)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		return Document.FontSettings.GetFont(m_fontname, fontSize, fontStyle, scriptType);
	}
}
