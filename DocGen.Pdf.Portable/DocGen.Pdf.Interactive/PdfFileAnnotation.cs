using DocGen.Drawing;

namespace DocGen.Pdf.Interactive;

public abstract class PdfFileAnnotation : PdfAnnotation
{
	private new PdfAppearance m_appearance;

	public abstract string FileName { get; set; }

	public new PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
			}
			return m_appearance;
		}
		set
		{
			if (m_appearance != value)
			{
				m_appearance = value;
			}
		}
	}

	protected PdfFileAnnotation()
	{
	}

	protected PdfFileAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Save()
	{
		base.Save();
		if (m_appearance != null && m_appearance.Normal != null)
		{
			base.Dictionary.SetProperty("AP", m_appearance);
		}
	}
}
