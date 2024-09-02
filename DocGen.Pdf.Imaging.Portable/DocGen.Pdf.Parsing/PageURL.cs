using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal class PageURL
{
	private Matrix m_transformPoints;

	private string m_uri;

	private PointF m_currentLocation;

	private float m_textElementWidth;

	private float m_fontSize;

	internal float FontSize => m_fontSize;

	internal string URI => m_uri;

	internal PointF CurrentLocation => m_currentLocation;

	internal Matrix TransformPoints => m_transformPoints;

	internal float TextElementWidth => m_textElementWidth;

	public PageURL(Matrix transformPoints, string URI, PointF CurrentLocation, float TextElementWidth, float fontSize)
	{
		m_transformPoints = transformPoints;
		m_uri = URI;
		m_currentLocation = CurrentLocation;
		m_textElementWidth = TextElementWidth;
		m_fontSize = fontSize;
	}
}
