using DocGen.Pdf.Graphics.Fonts;

namespace DocGen.Pdf.Graphics;

public class LineInfo
{
	internal string m_text;

	internal float m_width;

	internal LineType m_lineType;

	internal OtfGlyphInfoList OpenTypeGlyphList;

	private byte[] m_bidiLevls;

	private int m_trimCount;

	public LineType LineType
	{
		get
		{
			return m_lineType;
		}
		internal set
		{
			m_lineType = value;
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		internal set
		{
			m_text = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		internal set
		{
			m_width = value;
		}
	}

	internal byte[] BidiLevels
	{
		get
		{
			return m_bidiLevls;
		}
		set
		{
			m_bidiLevls = value;
		}
	}

	internal int TrimCount
	{
		get
		{
			return m_trimCount;
		}
		set
		{
			m_trimCount = value;
		}
	}
}
