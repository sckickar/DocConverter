using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfLinkAnnotation : PdfAnnotation
{
	private PdfHighlightMode m_highlightMode;

	public PdfHighlightMode HighlightMode
	{
		get
		{
			return m_highlightMode;
		}
		set
		{
			m_highlightMode = value;
			string highlightMode = GetHighlightMode(m_highlightMode);
			base.Dictionary.SetName("H", highlightMode);
			NotifyPropertyChanged("HighlightMode");
		}
	}

	public PdfLinkAnnotation()
	{
	}

	public PdfLinkAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Link"));
	}

	private string GetHighlightMode(PdfHighlightMode mode)
	{
		string result = null;
		switch (mode)
		{
		case PdfHighlightMode.Invert:
			result = "I";
			break;
		case PdfHighlightMode.NoHighlighting:
			result = "N";
			break;
		case PdfHighlightMode.Outline:
			result = "O";
			break;
		case PdfHighlightMode.Push:
			result = "P";
			break;
		}
		return result;
	}
}
