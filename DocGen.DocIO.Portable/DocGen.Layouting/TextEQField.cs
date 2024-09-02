using DocGen.Drawing;

namespace DocGen.Layouting;

internal class TextEQField : LayoutedEQFields
{
	private string m_text;

	private Font m_font;

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

	internal Font Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}
}
