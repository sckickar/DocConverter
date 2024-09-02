using System.IO;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Parsing;

public class PdfFontEventArgs
{
	private Stream m_fontStream;

	internal string m_fontName;

	internal PdfFontStyle m_fontStyle;

	public Stream FontStream
	{
		get
		{
			return m_fontStream;
		}
		set
		{
			m_fontStream = value;
		}
	}

	public string FontName => m_fontName;

	public PdfFontStyle FontStyle => m_fontStyle;

	internal PdfFontEventArgs()
	{
		FontStream = new MemoryStream();
		m_fontName = string.Empty;
		m_fontStyle = PdfFontStyle.Regular;
	}
}
