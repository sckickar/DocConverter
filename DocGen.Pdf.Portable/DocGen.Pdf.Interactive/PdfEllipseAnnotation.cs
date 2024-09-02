using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfEllipseAnnotation : PdfCircleAnnotation
{
	public new PdfPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory != null)
			{
				return m_reviewHistory;
			}
			return m_reviewHistory = new PdfPopupAnnotationCollection(this, isReview: true);
		}
	}

	public new PdfPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments != null)
			{
				return m_comments;
			}
			return m_comments = new PdfPopupAnnotationCollection(this, isReview: false);
		}
	}

	public PdfEllipseAnnotation(RectangleF rectangle, string text)
		: base(rectangle)
	{
		base.Dictionary.SetProperty("Subtype", new PdfName("Circle"));
		Text = text;
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.FlattenAnnot(flattenPopUps);
	}

	protected override void Save()
	{
		base.Save();
	}
}
