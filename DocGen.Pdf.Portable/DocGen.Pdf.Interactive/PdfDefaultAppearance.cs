using System.Globalization;
using System.Text;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

internal class PdfDefaultAppearance
{
	private PdfColor m_foreColor = new PdfColor(0, 0, 0);

	private string m_fontName = string.Empty;

	private float m_fontSize;

	public string FontName
	{
		get
		{
			return m_fontName;
		}
		set
		{
			if (m_fontName != value)
			{
				m_fontName = value;
			}
		}
	}

	public float FontSize
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			if (m_fontSize != value)
			{
				m_fontSize = value;
			}
		}
	}

	public PdfColor ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			if (m_foreColor != value)
			{
				m_foreColor = value;
			}
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("/");
		stringBuilder.Append(FontName);
		stringBuilder.Append(" ");
		stringBuilder.Append(m_fontSize.ToString(CultureInfo.InvariantCulture));
		stringBuilder.Append(" ");
		stringBuilder.Append("Tf");
		stringBuilder.Append(" ");
		stringBuilder.Append(m_foreColor.ToString(PdfColorSpace.RGB, stroke: false));
		return stringBuilder.ToString();
	}
}
