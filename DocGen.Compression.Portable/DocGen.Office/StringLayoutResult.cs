using DocGen.Drawing;

namespace DocGen.Office;

internal class StringLayoutResult
{
	internal LineInfo[] m_lines;

	internal string m_remainder;

	internal SizeF m_actualSize;

	internal float m_lineHeight;

	public string Remainder => m_remainder;

	public SizeF ActualSize => m_actualSize;

	public LineInfo[] Lines => m_lines;

	public float LineHeight => m_lineHeight;

	internal bool Empty
	{
		get
		{
			if (m_lines != null)
			{
				return m_lines.Length == 0;
			}
			return true;
		}
	}

	internal int LineCount
	{
		get
		{
			if (Empty)
			{
				return 0;
			}
			return m_lines.Length;
		}
	}
}
