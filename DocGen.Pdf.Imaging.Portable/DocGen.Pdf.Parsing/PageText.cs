using System.Drawing;
using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal class PageText
{
	private Matrix m_transformPoints;

	private string m_txt;

	private System.Drawing.PointF m_currentLocation;

	private float m_textElementWidth;

	private float m_fontSize;

	private Font m_textFont;

	internal float FontSize => m_fontSize;

	internal string Text => m_txt;

	internal System.Drawing.PointF CurrentLocation => m_currentLocation;

	internal Matrix TransformPoints => m_transformPoints;

	internal float TextElementWidth => m_textElementWidth;

	internal Font TextFont => m_textFont;

	public PageText(Matrix transformPoints, string txt, System.Drawing.PointF CurrentLocation, float TextElementWidth, float fontSize, Font font)
	{
		m_transformPoints = transformPoints;
		m_txt = txt;
		m_currentLocation = CurrentLocation;
		m_textElementWidth = TextElementWidth;
		m_fontSize = fontSize;
		m_textFont = font;
	}
}
