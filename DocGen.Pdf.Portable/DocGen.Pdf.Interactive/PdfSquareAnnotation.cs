using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfSquareAnnotation : PdfAnnotation
{
	private LineBorder m_border = new LineBorder();

	private PdfBorderEffect m_borderEffect = new PdfBorderEffect();

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

	public PdfBorderEffect BorderEffect
	{
		get
		{
			return m_borderEffect;
		}
		set
		{
			m_borderEffect = value;
			NotifyPropertyChanged("BorderEffect");
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

	public PdfSquareAnnotation(RectangleF rectangle, string text)
		: base(rectangle)
	{
		Text = text;
	}

	public PdfSquareAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Square"));
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
			if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
			{
				base.Dictionary.SetProperty("BE", m_borderEffect);
				base.Dictionary.SetProperty("BS", m_border);
				if (base.Dictionary["BS"] is PdfDictionary pdfDictionary)
				{
					if (pdfDictionary.ContainsKey("S"))
					{
						pdfDictionary.Remove("S");
					}
					if (pdfDictionary.ContainsKey("D"))
					{
						pdfDictionary.Remove("D");
					}
				}
			}
			else
			{
				base.Dictionary.SetProperty("BS", m_border);
			}
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
		RectangleF rectangleF = RectangleF.Empty;
		if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
		{
			RectangleF bounds = new RectangleF(Bounds.X - BorderEffect.Intensity * 5f - m_borderWidth / 2f, Bounds.Y - BorderEffect.Intensity * 5f - m_borderWidth / 2f, Bounds.Width + BorderEffect.Intensity * 10f + m_borderWidth, Bounds.Height + BorderEffect.Intensity * 10f + m_borderWidth);
			Bounds = bounds;
		}
		rectangleF = new RectangleF(0f, 0f, Bounds.Width, Bounds.Height);
		PdfTemplate pdfTemplate = new PdfTemplate(rectangleF);
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
		RectangleF rectangle = ObtainStyle(rectangleF);
		if (Opacity < 1f)
		{
			graphics.Save();
			graphics.SetTransparency(Opacity);
		}
		if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
		{
			FieldPainter.DrawRectanglecloud(graphics, paintParams, rectangle, BorderEffect.Intensity, m_borderWidth);
		}
		else
		{
			FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangle.X + num, rectangle.Y + num, rectangle.Width - m_borderWidth, rectangle.Height - m_borderWidth);
		}
		if (Opacity < 1f)
		{
			graphics.Restore();
		}
		return pdfTemplate;
	}

	private RectangleF ObtainStyle(RectangleF rectangle)
	{
		if (!base.Flatten)
		{
			if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
			{
				float num = 0f;
				if (BorderEffect.Intensity > 0f && BorderEffect.Intensity <= 2f)
				{
					num = BorderEffect.Intensity * 5f;
				}
				float[] array = new float[4]
				{
					2f * num + m_borderWidth,
					2f * num + m_borderWidth,
					2f * num + m_borderWidth,
					2f * num + m_borderWidth
				};
				base.Dictionary.SetProperty("RD", new PdfArray(array));
				rectangle.X = rectangle.X + array[0] + m_borderWidth / 2f;
				rectangle.Y = rectangle.Y + array[1] + m_borderWidth / 2f;
				rectangle.Width = rectangle.Width - 2f * array[2] - m_borderWidth;
				rectangle.Height = rectangle.Height - 2f * array[3] - m_borderWidth;
			}
		}
		else if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
		{
			float num2 = 0f;
			if (BorderEffect.Intensity > 0f && BorderEffect.Intensity <= 2f)
			{
				num2 = BorderEffect.Intensity * 5f;
			}
			float[] array2 = new float[4]
			{
				num2 + m_borderWidth / 2f,
				num2 + m_borderWidth / 2f,
				num2 + m_borderWidth / 2f,
				num2 + m_borderWidth / 2f
			};
			base.Dictionary.SetProperty("RD", new PdfArray(array2));
			rectangle.X = rectangle.X + array2[0] + m_borderWidth / 2f;
			rectangle.Y = rectangle.Y + array2[1] + m_borderWidth / 2f;
			rectangle.Width = rectangle.Width - 2f * array2[2] - m_borderWidth;
			rectangle.Height = rectangle.Height - 2f * array2[3] - m_borderWidth;
		}
		rectangle = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		return rectangle;
	}
}
