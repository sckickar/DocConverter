using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfCircleMeasurementAnnotation : PdfAnnotation
{
	private LineBorder m_border = new LineBorder();

	private PdfMeasurementUnit m_measurementUnit;

	private PdfCircleMeasurementType m_type;

	private PdfFont m_font;

	private string m_unitString;

	private float m_borderWidth;

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

	public PdfMeasurementUnit Unit
	{
		get
		{
			return m_measurementUnit;
		}
		set
		{
			m_measurementUnit = value;
			NotifyPropertyChanged("Unit");
		}
	}

	public PdfCircleMeasurementType MeasurementType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
			NotifyPropertyChanged("MeasurementType");
		}
	}

	public PdfFont Font
	{
		get
		{
			if (m_font == null)
			{
				m_font = new PdfStandardFont(PdfFontFamily.Helvetica, 8f, PdfFontStyle.Regular);
			}
			return m_font;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Font");
			}
			m_font = value;
			NotifyPropertyChanged("Font");
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

	public PdfCircleMeasurementAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Circle"));
	}

	private float ConvertToUnit()
	{
		float num = 0f;
		num = new PdfUnitConvertor().ConvertUnits(Bounds.Width / 2f, PdfGraphicsUnit.Point, GetEqualPdfGraphicsUnit(Unit, out m_unitString));
		if (MeasurementType == PdfCircleMeasurementType.Diameter)
		{
			num = 2f * num;
		}
		return num;
	}

	protected override void Save()
	{
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		if (base.Dictionary.Items.ContainsKey(new PdfName("AP")))
		{
			return;
		}
		float num = ConvertToUnit();
		SizeF sizeF = Font.MeasureString(num.ToString("0.00") + " " + m_unitString);
		PdfSection pdfSection = null;
		if (base.Page != null)
		{
			pdfSection = base.Page.Section;
		}
		PdfPen pdfPen = new PdfPen(Color, m_borderWidth);
		PdfBrush pdfBrush = null;
		if (InnerColor.A != 0)
		{
			pdfBrush = new PdfSolidBrush(InnerColor);
		}
		PdfBrush brush = new PdfSolidBrush(Color);
		RectangleF rect = new RectangleF(Bounds.X, Bounds.Bottom, Bounds.Width, Bounds.Height);
		if (base.Page != null)
		{
			rect.Location = pdfSection.PointToNativePdf(base.Page, Bounds.Location);
		}
		if (!base.Flatten)
		{
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Dictionary["AP"] != null)
			{
				rect.Y -= rect.Height;
				base.Appearance.Normal = new PdfTemplate(rect);
				PdfTemplate normal = base.Appearance.Normal;
				PaintParams paintParams = new PaintParams();
				normal.m_writeTransformation = false;
				PdfGraphics graphics = base.Appearance.Normal.Graphics;
				float num2 = (float)Border.BorderWidth / 2f;
				paintParams.BorderPen = pdfPen;
				paintParams.BackBrush = pdfBrush;
				paintParams.ForeBrush = new PdfSolidBrush(Color);
				RectangleF rectangleF = new RectangleF(rect.X, 0f - rect.Y - rect.Height, rect.Width, rect.Height);
				paintParams.Bounds = new RectangleF(rect.X, 0f - rect.Y, rect.Width, 0f - rect.Height);
				rectangleF = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
				graphics.Save();
				if (Opacity < 1f)
				{
					graphics.SetTransparency(Opacity);
				}
				FieldPainter.DrawEllipseAnnotation(graphics, paintParams, rectangleF.X + num2, rectangleF.Y + num2, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
				if (MeasurementType == PdfCircleMeasurementType.Diameter)
				{
					graphics.Save();
					graphics.TranslateTransform(rect.X, 0f - rect.Y);
					float offsetX = rect.Width / 2f - sizeF.Width / 2f;
					graphics.DrawLine(paintParams.BorderPen, new PointF(0f, (0f - rect.Height) / 2f), new PointF(rect.Right, (0f - rect.Height) / 2f));
					graphics.TranslateTransform(offsetX, 0f - rect.Height / 2f - Font.Height);
					graphics.DrawString(num.ToString("0.00") + " " + m_unitString, Font, paintParams.ForeBrush, 0f, 0f);
					graphics.Restore();
				}
				else
				{
					graphics.Save();
					graphics.TranslateTransform(rect.X, 0f - rect.Y);
					float offsetX2 = rect.Width / 2f + (rect.Width / 4f - sizeF.Width / 2f);
					_ = rect.Height / 2f;
					_ = sizeF.Height / 2f;
					graphics.DrawLine(paintParams.BorderPen, new PointF(rect.Width / 2f, (0f - rect.Height) / 2f), new PointF(rect.Right, (0f - rect.Height) / 2f));
					graphics.TranslateTransform(offsetX2, 0f - rect.Height / 2f - Font.Height);
					graphics.DrawString(num.ToString("0.00") + " " + m_unitString, Font, paintParams.ForeBrush, 0f, 0f);
					graphics.Restore();
				}
				graphics.Restore();
			}
			base.Save();
			base.Dictionary.SetProperty("BS", m_border);
			if (!base.Dictionary.Items.ContainsKey(new PdfName("Measure")))
			{
				PdfDictionary obj = CreateMeasureDictioanry(m_unitString);
				base.Dictionary.Items.Add(new PdfName("Measure"), new PdfReferenceHolder(obj));
			}
			if (Text == string.Empty)
			{
				string value = num.ToString("0.00") + " " + m_unitString;
				base.Dictionary.SetProperty(new PdfName("Contents"), new PdfString(value));
			}
			else
			{
				base.Dictionary.SetProperty(new PdfName("Contents"), new PdfString(Text));
			}
			base.Dictionary.SetProperty("DS", new PdfString($"font:{Font.Name} {Font.Size}pt; color:{ColorToHex(Color)}"));
			return;
		}
		if (base.Page != null)
		{
			PdfGraphicsState state = base.Page.Graphics.Save();
			base.Page.Graphics.SetTransparency(Opacity);
			base.Page.Graphics.DrawEllipse(pdfPen, pdfBrush, Bounds);
			PointF pointF = new PointF(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
			if (MeasurementType == PdfCircleMeasurementType.Radius)
			{
				base.Page.Graphics.DrawLine(pdfPen, new PointF(pointF.X, pointF.Y), new PointF(Bounds.Right, pointF.Y));
				pointF = new PointF(pointF.X + Bounds.Width / 4f, pointF.Y);
			}
			else
			{
				base.Page.Graphics.DrawLine(pdfPen, new PointF(Bounds.X, pointF.Y), new PointF(Bounds.Right, pointF.Y));
			}
			base.Page.Graphics.DrawString(num.ToString("0.00") + " " + m_unitString, Font, brush, new PointF(pointF.X - sizeF.Width / 2f, pointF.Y - sizeF.Height));
			base.Page.Graphics.Restore(state);
		}
		else if (base.LoadedPage != null)
		{
			PdfGraphicsState state2 = base.LoadedPage.Graphics.Save();
			base.LoadedPage.Graphics.SetTransparency(Opacity);
			base.LoadedPage.Graphics.DrawEllipse(pdfPen, pdfBrush, Bounds);
			PointF pointF2 = new PointF(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
			if (MeasurementType == PdfCircleMeasurementType.Radius)
			{
				base.LoadedPage.Graphics.DrawLine(pdfPen, new PointF(pointF2.X, pointF2.Y), new PointF(Bounds.Right, pointF2.Y));
				pointF2 = new PointF(pointF2.X + Bounds.Width / 4f, pointF2.Y);
			}
			else
			{
				base.LoadedPage.Graphics.DrawLine(pdfPen, new PointF(Bounds.X, pointF2.Y), new PointF(Bounds.Right, pointF2.Y));
			}
			base.LoadedPage.Graphics.DrawString(num.ToString("0.00") + " " + m_unitString, Font, brush, new PointF(pointF2.X - sizeF.Width / 2f, pointF2.Y - sizeF.Height));
			base.LoadedPage.Graphics.Restore(state2);
		}
		RemoveAnnoationFromPage(base.Page, this);
	}
}
