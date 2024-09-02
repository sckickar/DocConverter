using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfSquareMeasurementAnnotation : PdfAnnotation
{
	private LineBorder m_border = new LineBorder();

	private string m_unitString;

	private PdfMeasurementUnit m_measurementUnit;

	private PdfFont m_font;

	private float m_borderWidth;

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

	public PdfSquareMeasurementAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Square"));
	}

	private float CalculateAreaOfSquare()
	{
		float num = 0f;
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
		if (Bounds.Width == Bounds.Height)
		{
			float num2 = pdfUnitConvertor.ConvertUnits(Bounds.Width, PdfGraphicsUnit.Point, GetEqualPdfGraphicsUnit(Unit, out m_unitString));
			return num2 * num2;
		}
		float num3 = pdfUnitConvertor.ConvertUnits(Bounds.Width, PdfGraphicsUnit.Point, GetEqualPdfGraphicsUnit(Unit, out m_unitString));
		float num4 = pdfUnitConvertor.ConvertUnits(Bounds.Height, PdfGraphicsUnit.Point, GetEqualPdfGraphicsUnit(Unit, out m_unitString));
		return num3 * num4;
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
		float num = CalculateAreaOfSquare();
		SizeF sizeF = Font.MeasureString(num.ToString("0.00") + " sq " + m_unitString);
		PaintParams paintParams = null;
		PdfPen pdfPen = new PdfPen(Color, m_borderWidth);
		if (base.Dictionary.Items.ContainsKey(new PdfName("AP")))
		{
			return;
		}
		PdfBrush pdfBrush = null;
		if (InnerColor.A != 0)
		{
			pdfBrush = new PdfSolidBrush(InnerColor);
		}
		PdfBrush brush = new PdfSolidBrush(Color);
		if (base.Page != null)
		{
			_ = base.Page.Section;
		}
		RectangleF rect = new RectangleF(Bounds.X, Bounds.Bottom, Bounds.Width, Bounds.Height);
		if (!base.Flatten)
		{
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Dictionary["AP"] != null)
			{
				rect.Y -= rect.Height;
				base.Appearance.Normal = new PdfTemplate(rect);
				PdfTemplate normal = base.Appearance.Normal;
				paintParams = new PaintParams();
				normal.m_writeTransformation = false;
				PdfGraphics graphics = base.Appearance.Normal.Graphics;
				float num2 = m_borderWidth / 2f;
				paintParams.BorderPen = pdfPen;
				paintParams.BackBrush = pdfBrush;
				paintParams.ForeBrush = new PdfSolidBrush(Color);
				RectangleF rectangleF = new RectangleF(rect.X, 0f - rect.Y - rect.Height, rect.Width, rect.Height);
				paintParams.Bounds = new RectangleF(rect.X, 0f - rect.Y, rect.Width, 0f - rect.Height);
				rectangleF = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
				if (Opacity < 1f)
				{
					PdfGraphicsState state = graphics.Save();
					graphics.SetTransparency(Opacity);
					FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangleF.X + num2, rectangleF.Y + num2, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
					graphics.TranslateTransform(rect.X, 0f - rect.Y);
					float offsetX = rect.Width / 2f - sizeF.Width / 2f;
					float num3 = rect.Height / 2f - sizeF.Height / 2f;
					graphics.TranslateTransform(offsetX, 0f - num3 - Font.Height);
					graphics.DrawString(num.ToString("0.00") + " sq " + m_unitString, Font, paintParams.ForeBrush, 0f, 0f);
					graphics.Restore(state);
				}
				else
				{
					FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangleF.X + num2, rectangleF.Y + num2, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
					graphics.Save();
					graphics.TranslateTransform(rect.X, 0f - rect.Y);
					float offsetX2 = rect.Width / 2f - sizeF.Width / 2f;
					float num4 = rect.Height / 2f - sizeF.Height / 2f;
					graphics.TranslateTransform(offsetX2, 0f - num4 - Font.Height);
					graphics.DrawString(num.ToString("0.00") + " sq " + m_unitString, Font, paintParams.ForeBrush, 0f, 0f);
					graphics.Restore();
				}
			}
			base.Save();
			if (!base.Dictionary.Items.ContainsKey(new PdfName("Measure")))
			{
				PdfDictionary obj = CreateMeasureDictioanry(m_unitString);
				base.Dictionary.Items.Add(new PdfName("Measure"), new PdfReferenceHolder(obj));
			}
			base.Dictionary.SetProperty("DS", new PdfString($"font:{Font.Name} {Font.Size}pt; color:{ColorToHex(Color)}"));
			base.Dictionary.SetProperty("BS", m_border);
			base.Dictionary.SetProperty("Contents", new PdfString(Text + "\n" + num.ToString("0.00") + " sq " + m_unitString));
			base.Dictionary.SetProperty("Subj", new PdfString("Area Measurement"));
			base.Dictionary.SetProperty("MeasurementTypes", new PdfNumber(129));
			base.Dictionary.SetProperty("Subtype", new PdfName("Polygon"));
			base.Dictionary.SetProperty("IT", new PdfName("PolygonDimension"));
			PdfArray obj2 = base.Dictionary["Rect"] as PdfArray;
			float[] array = new float[obj2.Elements.Count];
			int num5 = 0;
			foreach (PdfNumber item in obj2)
			{
				array[num5] = item.FloatValue;
				num5++;
			}
			float[] array2 = new float[array.Length * 2];
			array2[0] = array[0];
			array2[1] = array[3];
			array2[2] = array[0];
			array2[3] = array[1];
			array2[4] = array[2];
			array2[5] = array[1];
			array2[6] = array[2];
			array2[7] = array[3];
			base.Dictionary.SetProperty("Vertices", new PdfArray(array2));
		}
		else
		{
			if (base.Page != null)
			{
				PdfGraphicsState state2 = base.Page.Graphics.Save();
				base.Page.Graphics.SetTransparency(Opacity);
				base.Page.Graphics.DrawRectangle(pdfPen, pdfBrush, Bounds);
				PointF pointF = new PointF(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
				base.Page.Graphics.DrawString(num.ToString("0.00") + " sq " + m_unitString, Font, brush, new PointF(pointF.X - sizeF.Width / 2f, pointF.Y - sizeF.Height / 2f));
				base.Page.Graphics.Restore(state2);
			}
			else if (base.LoadedPage != null)
			{
				PdfGraphicsState state3 = base.LoadedPage.Graphics.Save();
				base.LoadedPage.Graphics.SetTransparency(Opacity);
				base.LoadedPage.Graphics.DrawRectangle(pdfPen, pdfBrush, Bounds);
				PointF pointF2 = new PointF(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
				base.LoadedPage.Graphics.DrawString(num.ToString("0.00") + " sq " + m_unitString, Font, brush, new PointF(pointF2.X - sizeF.Width / 2f, pointF2.Y - sizeF.Height / 2f));
				base.LoadedPage.Graphics.Restore(state3);
			}
			RemoveAnnoationFromPage(base.Page, this);
		}
	}
}
