namespace DocGen.Pdf.Graphics.Fonts;

public class PdfFontSettings
{
	private float m_size;

	private PdfFontStyle m_style;

	private bool m_embed;

	private bool m_subset;

	private bool m_useFloatingFactorForMeasure;

	internal float Size => m_size;

	internal bool Subset => m_subset;

	internal bool Embed => m_embed;

	internal PdfFontStyle Style => m_style;

	internal bool UseFloatingFactorForMeasure => m_useFloatingFactorForMeasure;

	public PdfFontSettings(float fontSize, PdfFontStyle style, bool embed, bool subset, bool useFloatingFactorForMeasure)
	{
		m_size = fontSize;
		m_style = style;
		m_embed = embed;
		m_subset = subset;
		m_useFloatingFactorForMeasure = useFloatingFactorForMeasure;
	}
}
