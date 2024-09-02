using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfCircleAnnotation : PdfAnnotation
{
	private LineBorder m_border = new LineBorder();

	private float m_borderWidth;

	public PdfPopupAnnotationCollection ReviewHistory
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

	public PdfPopupAnnotationCollection Comments
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

	public new LineBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			m_border = value;
			NotifyPropertyChanged("Border");
		}
	}

	public PdfCircleAnnotation(RectangleF rectangle, string text)
		: base(rectangle)
	{
		Text = text;
	}

	public PdfCircleAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Circle"));
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		if (!m_isStandardAppearance && !base.SetAppearanceDictionary)
		{
			base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
		}
		if (base.Flatten || base.SetAppearanceDictionary)
		{
			PdfTemplate pdfTemplate = null;
			if (!m_isStandardAppearance && base.Flatten && !base.SetAppearanceDictionary)
			{
				pdfTemplate = base.Appearance.Normal;
			}
			if (pdfTemplate == null)
			{
				pdfTemplate = CreateAppearance();
			}
			if (base.Flatten)
			{
				if (pdfTemplate != null)
				{
					if (base.Page != null)
					{
						FlattenAnnotation(base.Page, pdfTemplate);
					}
					else if (base.LoadedPage != null)
					{
						FlattenAnnotation(base.LoadedPage, pdfTemplate);
					}
				}
			}
			else if (pdfTemplate != null)
			{
				base.Appearance.Normal = pdfTemplate;
				base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
			}
		}
		if (!isExternalFlatten && !base.Flatten)
		{
			base.Save();
			if (Color.IsEmpty)
			{
				base.Dictionary.SetProperty("C", Color.ToArray());
			}
			base.Dictionary.SetProperty("BS", m_border);
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenPopup();
		}
		if (base.Page != null && base.Popup != null && base.Flatten)
		{
			RemoveAnnoationFromPage(base.Page, base.Popup);
		}
		else if (base.LoadedPage != null && base.Popup != null && base.Flatten)
		{
			RemoveAnnoationFromPage(base.LoadedPage, base.Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		PdfGraphics layerGraphics = GetLayerGraphics();
		page.Graphics.Save();
		isAnnotationCreation = true;
		RectangleF rectangleF = CalculateTemplateBounds(Bounds, page, appearance, isNormalMatrix: true);
		isAnnotationCreation = false;
		if (Opacity < 1f)
		{
			page.Graphics.SetTransparency(Opacity);
		}
		if (layerGraphics != null)
		{
			layerGraphics.DrawPdfTemplate(appearance, rectangleF.Location, rectangleF.Size);
		}
		else
		{
			page.Graphics.DrawPdfTemplate(appearance, rectangleF.Location, rectangleF.Size);
		}
		RemoveAnnoationFromPage(page, this);
		page.Graphics.Restore();
	}

	private PdfTemplate CreateAppearance()
	{
		RectangleF rect = new RectangleF(0f, 0f, Bounds.Width, Bounds.Height);
		PdfTemplate pdfTemplate = new PdfTemplate(rect);
		SetMatrix(pdfTemplate.m_content);
		PaintParams paintParams = new PaintParams();
		PdfGraphics graphics = pdfTemplate.Graphics;
		if (m_borderWidth > 0f && Color.A != 0)
		{
			PdfPen borderPen = new PdfPen(Color, m_borderWidth);
			paintParams.BorderPen = borderPen;
		}
		PdfBrush backBrush = null;
		if (InnerColor.A != 0)
		{
			backBrush = new PdfSolidBrush(InnerColor);
		}
		float num = m_borderWidth / 2f;
		paintParams.BackBrush = backBrush;
		paintParams.ForeBrush = new PdfSolidBrush(Color);
		RectangleF rectangleF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		if (Opacity < 1f)
		{
			PdfGraphicsState state = graphics.Save();
			graphics.SetTransparency(Opacity);
			FieldPainter.DrawEllipseAnnotation(graphics, paintParams, rectangleF.X + num, rectangleF.Y + num, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
			graphics.Restore(state);
		}
		else
		{
			FieldPainter.DrawEllipseAnnotation(graphics, paintParams, rectangleF.X + num, rectangleF.Y + num, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
		}
		return pdfTemplate;
	}
}
