using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLineAnnotation : PdfAnnotation
{
	private PdfLineEndingStyle m_beginLine;

	private PdfLineEndingStyle m_endLine;

	private LineBorder m_lineBorder = new LineBorder();

	internal PdfArray m_linePoints;

	internal PdfArray m_lineStyle;

	private int m_leaderLineExt;

	private int m_leaderLine;

	private bool m_lineCaption;

	private PdfLineIntent m_lineIntent = PdfLineIntent.LineDimension;

	public PdfLineCaptionType m_captionType;

	private int[] m_points;

	private float[] m_point;

	private float m_borderWidth;

	private bool m_isLineIntent;

	public bool LineCaption
	{
		get
		{
			return m_lineCaption;
		}
		set
		{
			m_lineCaption = value;
			NotifyPropertyChanged("LineCaption");
		}
	}

	public int LeaderLine
	{
		get
		{
			return m_leaderLine;
		}
		set
		{
			if (m_leaderLineExt != 0)
			{
				m_leaderLine = value;
			}
			NotifyPropertyChanged("LeaderLine");
		}
	}

	public int LeaderLineExt
	{
		get
		{
			return m_leaderLineExt;
		}
		set
		{
			m_leaderLineExt = value;
			NotifyPropertyChanged("LeaderLineExt");
		}
	}

	public LineBorder lineBorder
	{
		get
		{
			return m_lineBorder;
		}
		set
		{
			m_lineBorder = value;
			NotifyPropertyChanged("lineBorder");
		}
	}

	public PdfLineEndingStyle BeginLineStyle
	{
		get
		{
			return m_beginLine;
		}
		set
		{
			if (m_beginLine != value)
			{
				m_beginLine = value;
			}
			NotifyPropertyChanged("BeginLineStyle");
		}
	}

	public PdfLineEndingStyle EndLineStyle
	{
		get
		{
			return m_endLine;
		}
		set
		{
			if (m_endLine != value)
			{
				m_endLine = value;
			}
			NotifyPropertyChanged("EndLineStyle");
		}
	}

	public PdfLineCaptionType CaptionType
	{
		get
		{
			return m_captionType;
		}
		set
		{
			m_captionType = value;
			NotifyPropertyChanged("CaptionType");
		}
	}

	public PdfLineIntent LineIntent
	{
		get
		{
			return m_lineIntent;
		}
		set
		{
			m_isLineIntent = true;
			m_lineIntent = value;
			NotifyPropertyChanged("LineIntent");
		}
	}

	public PdfColor InnerLineColor
	{
		get
		{
			return InnerColor;
		}
		set
		{
			InnerColor = value;
		}
	}

	public PdfColor BackColor
	{
		get
		{
			return Color;
		}
		set
		{
			Color = value;
		}
	}

	public int[] LinePoints
	{
		get
		{
			return m_points;
		}
		set
		{
			m_points = value;
			m_linePoints = new PdfArray(m_points);
			NotifyPropertyChanged("LinePoints");
		}
	}

	internal float[] LinePoint
	{
		get
		{
			return m_point;
		}
		set
		{
			m_point = value;
			m_linePoints = new PdfArray(m_point);
			NotifyPropertyChanged("LinePoints");
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

	public PdfLineAnnotation(int[] linePoints)
	{
		m_linePoints = new PdfArray(linePoints);
		m_points = linePoints;
	}

	public PdfLineAnnotation(int[] linePoints, string text)
	{
		m_linePoints = new PdfArray(linePoints);
		Text = text;
		m_points = linePoints;
	}

	internal PdfLineAnnotation(float[] linePoints, string text)
	{
		m_linePoints = new PdfArray(linePoints);
		Text = text;
		m_point = linePoints;
	}

	public PdfLineAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Line"));
	}

	private float[] ObtainLinePoints()
	{
		float[] array = null;
		if (m_linePoints != null)
		{
			array = new float[m_linePoints.Count];
			int num = 0;
			foreach (PdfNumber linePoint in m_linePoints)
			{
				array[num] = linePoint.FloatValue;
				num++;
			}
		}
		return array;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (lineBorder.BorderWidth == 1)
		{
			m_borderWidth = lineBorder.BorderLineWidth;
		}
		else
		{
			m_borderWidth = lineBorder.BorderWidth;
		}
		if (base.Flatten || base.SetAppearanceDictionary)
		{
			PdfTemplate pdfTemplate = CreateAppearance();
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
			SavePdfLineDictionary();
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

	private RectangleF ObtainLineBounds()
	{
		RectangleF result = Bounds;
		if ((m_points != null && m_points.Length == 4) || (m_point != null && m_point.Length == 4))
		{
			new PdfPath();
			float[] array = ObtainLinePoints();
			if (array != null && array.Length == 4)
			{
				PdfArray pdfArray = new PdfArray();
				pdfArray.Insert(0, new PdfName(BeginLineStyle));
				pdfArray.Insert(1, new PdfName(EndLineStyle));
				result = CalculateLineBounds(array, m_leaderLineExt, m_leaderLine, 0, pdfArray, m_borderWidth);
				result = new RectangleF(result.X - 8f, result.Y - 8f, result.Width + 16f, result.Height + 16f);
			}
		}
		return result;
	}

	private void SavePdfLineDictionary()
	{
		base.Save();
		m_lineStyle = new PdfArray();
		m_lineStyle.Insert(0, new PdfName(BeginLineStyle));
		m_lineStyle.Insert(1, new PdfName(EndLineStyle));
		base.Dictionary.SetProperty("LE", m_lineStyle);
		if (m_linePoints == null)
		{
			throw new PdfException("LinePoints cannot be null");
		}
		PdfPageBase page = ((base.Page == null) ? ((PdfPageBase)base.LoadedPage) : ((PdfPageBase)base.Page));
		PdfArray linePoints = m_linePoints;
		PdfMargins pdfMargins = ObtainMargin();
		PdfArray cropOrMediaBox = null;
		cropOrMediaBox = GetCropOrMediaBox(page, cropOrMediaBox);
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && linePoints.Count > 3)
		{
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				(linePoints[0] as PdfNumber).FloatValue = (linePoints[0] as PdfNumber).FloatValue + pdfNumber.FloatValue - pdfMargins.Left;
				(linePoints[1] as PdfNumber).FloatValue = (linePoints[1] as PdfNumber).FloatValue + pdfNumber2.FloatValue + pdfMargins.Top;
				(linePoints[2] as PdfNumber).FloatValue = (linePoints[2] as PdfNumber).FloatValue + pdfNumber.FloatValue - pdfMargins.Left;
				(linePoints[3] as PdfNumber).FloatValue = (linePoints[3] as PdfNumber).FloatValue + pdfNumber2.FloatValue + pdfMargins.Top;
			}
		}
		base.Dictionary.SetProperty("L", linePoints);
		if (m_lineBorder.DashArray == 0)
		{
			if (m_lineBorder.BorderStyle == PdfBorderStyle.Dashed)
			{
				m_lineBorder.DashArray = 4;
			}
			else if (m_lineBorder.BorderStyle == PdfBorderStyle.Dot)
			{
				m_lineBorder.DashArray = 2;
			}
		}
		base.Dictionary.SetProperty("BS", m_lineBorder);
		if (InnerLineColor.A != 0)
		{
			base.Dictionary.SetProperty("IC", InnerColor.ToArray());
		}
		base.Dictionary["C"] = BackColor.ToArray();
		if (m_isLineIntent)
		{
			base.Dictionary.SetProperty("IT", new PdfName(m_lineIntent));
		}
		base.Dictionary.SetProperty("LLE", new PdfNumber(m_leaderLineExt));
		base.Dictionary.SetProperty("LL", new PdfNumber(m_leaderLine));
		base.Dictionary.SetProperty("CP", new PdfName(m_captionType));
		base.Dictionary.SetProperty("Cap", new PdfBoolean(m_lineCaption));
		base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(ObtainLineBounds()));
	}

	private PdfTemplate CreateAppearance()
	{
		RectangleF rect = ObtainLineBounds();
		PdfTemplate pdfTemplate = new PdfTemplate(rect);
		SetMatrix(pdfTemplate.m_content);
		PaintParams paintParams = new PaintParams();
		pdfTemplate.m_writeTransformation = false;
		PdfGraphics graphics = pdfTemplate.Graphics;
		PdfPen pdfPen = new PdfPen(BackColor, m_borderWidth);
		if (lineBorder.BorderStyle == PdfBorderStyle.Dashed)
		{
			pdfPen.DashStyle = PdfDashStyle.Dash;
		}
		else if (lineBorder.BorderStyle == PdfBorderStyle.Dot)
		{
			pdfPen.DashStyle = PdfDashStyle.Dot;
		}
		PdfBrush pdfBrush = new PdfSolidBrush(InnerLineColor);
		paintParams.BorderPen = pdfPen;
		paintParams.ForeBrush = new PdfSolidBrush(Color);
		PdfMargins pdfMargins = ObtainMargin();
		PdfFont pdfFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9f, PdfFontStyle.Regular);
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Center;
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
		float width = pdfFont.MeasureString(Text, pdfStringFormat).Width;
		float[] array = ObtainLinePoints();
		if (array != null && array.Length == 4)
		{
			float num = array[0];
			float num2 = array[1];
			float num3 = array[2];
			float num4 = array[3];
			double num5 = 0.0;
			num5 = ((num3 - num != 0f) ? GetAngle(num, num2, num3, num4) : ((!(num4 > num2)) ? 270.0 : 90.0));
			int num6 = 0;
			double num7 = 0.0;
			if (LeaderLine < 0)
			{
				num6 = -m_leaderLine;
				num7 = num5 + 180.0;
			}
			else
			{
				num6 = m_leaderLine;
				num7 = num5;
			}
			float[] value = new float[2] { num, num2 };
			float[] value2 = new float[2] { num3, num4 };
			float[] axisValue = GetAxisValue(value, num7 + 90.0, num6);
			float[] axisValue2 = GetAxisValue(value2, num7 + 90.0, num6);
			double num8 = Math.Sqrt(Math.Pow(axisValue2[0] - axisValue[0], 2.0) + Math.Pow(axisValue2[1] - axisValue[1], 2.0));
			double length = num8 / 2.0 - (double)(width / 2f + m_borderWidth);
			float[] axisValue3 = GetAxisValue(axisValue, num5, length);
			float[] axisValue4 = GetAxisValue(axisValue2, num5 + 180.0, length);
			string text = m_captionType.ToString();
			float[] array2 = ((BeginLineStyle != PdfLineEndingStyle.OpenArrow && BeginLineStyle != PdfLineEndingStyle.ClosedArrow) ? axisValue : GetAxisValue(axisValue, num5, m_borderWidth));
			float[] array3 = ((EndLineStyle != PdfLineEndingStyle.OpenArrow && EndLineStyle != PdfLineEndingStyle.ClosedArrow) ? axisValue2 : GetAxisValue(axisValue2, num5, 0f - m_borderWidth));
			if (Opacity < 1f)
			{
				PdfGraphicsState state = graphics.Save();
				graphics.SetTransparency(Opacity);
				if (string.IsNullOrEmpty(Text) || text == "Top" || (!LineCaption && text == "Inline"))
				{
					graphics.DrawLine(pdfPen, array2[0], 0f - array2[1], array3[0], 0f - array3[1]);
				}
				else
				{
					graphics.DrawLine(pdfPen, array2[0], 0f - array2[1], axisValue3[0], 0f - axisValue3[1]);
					graphics.DrawLine(pdfPen, array3[0], 0f - array3[1], axisValue4[0], 0f - axisValue4[1]);
				}
				graphics.Restore(state);
			}
			else if (string.IsNullOrEmpty(Text) || text == "Top" || (!LineCaption && text == "Inline"))
			{
				graphics.DrawLine(pdfPen, array2[0], 0f - array2[1], array3[0], 0f - array3[1]);
			}
			else
			{
				graphics.DrawLine(pdfPen, array2[0], 0f - array2[1], axisValue3[0], 0f - axisValue3[1]);
				graphics.DrawLine(pdfPen, array3[0], 0f - array3[1], axisValue4[0], 0f - axisValue4[1]);
			}
			PdfArray pdfArray = new PdfArray();
			pdfArray.Insert(0, new PdfName(BeginLineStyle));
			pdfArray.Insert(1, new PdfName(EndLineStyle));
			double borderLength = m_borderWidth;
			float[] axisValue5 = GetAxisValue(axisValue, num7 + 90.0, m_leaderLineExt);
			float[] axisValue6 = GetAxisValue(axisValue2, num7 + 90.0, m_leaderLineExt);
			float[] axisValue7 = GetAxisValue(axisValue, num7 - 90.0, num6);
			float[] axisValue8 = GetAxisValue(axisValue2, num7 - 90.0, num6);
			PdfGraphicsState state2 = null;
			if (Opacity < 1f)
			{
				state2 = graphics.Save();
				graphics.SetTransparency(Opacity);
			}
			SetLineEndingStyles(axisValue, axisValue2, graphics, num5, pdfPen, pdfBrush, pdfArray, borderLength);
			graphics.DrawLine(pdfPen, axisValue[0], 0f - axisValue[1], axisValue5[0], 0f - axisValue5[1]);
			graphics.DrawLine(pdfPen, axisValue2[0], 0f - axisValue2[1], axisValue6[0], 0f - axisValue6[1]);
			graphics.DrawLine(pdfPen, axisValue[0], 0f - axisValue[1], axisValue7[0], 0f - axisValue7[1]);
			graphics.DrawLine(pdfPen, axisValue2[0], 0f - axisValue2[1], axisValue8[0], 0f - axisValue8[1]);
			if (Opacity < 1f)
			{
				graphics.Restore(state2);
			}
			double length2 = num8 / 2.0;
			float[] axisValue9 = GetAxisValue(axisValue, num5, length2);
			float[] array4 = new float[2];
			array4 = ((!(text == "Top")) ? GetAxisValue(axisValue9, num5 + 90.0, pdfFont.Height / 2f) : GetAxisValue(axisValue9, num5 + 90.0, pdfFont.Height));
			graphics.TranslateTransform(array4[0], 0f - array4[1]);
			graphics.RotateTransform((float)(0.0 - num5));
			if (LineCaption)
			{
				graphics.DrawString(Text, pdfFont, pdfBrush, new PointF((0f - width) / 2f, 0f));
			}
			graphics.Restore();
		}
		if (base.Flatten)
		{
			float num9 = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
			if (base.Page != null)
			{
				Bounds = new RectangleF(rect.X - pdfMargins.Left, num9 - (rect.Y + rect.Height) - pdfMargins.Top, rect.Width, rect.Height);
			}
			else
			{
				Bounds = new RectangleF(rect.X, num9 - (rect.Y + rect.Height), rect.Width, rect.Height);
			}
		}
		return pdfTemplate;
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
}
