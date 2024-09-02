using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLineMeasurementAnnotation : PdfAnnotation
{
	private LineBorder m_lineBorder = new LineBorder();

	internal PdfArray m_linePoints;

	private int m_leaderLineExt;

	private int m_leaderLine;

	private bool m_lineCaption = true;

	private PdfLineIntent m_lineIntent;

	public PdfLineCaptionType m_captionType;

	private PdfMeasurementUnit m_measurementUnit;

	private PdfFont m_font;

	private int[] m_points;

	private string m_unitString;

	internal PdfArray m_lineStyle;

	private int m_leaderOffset;

	private PdfLineEndingStyle m_beginLine;

	private PdfLineEndingStyle m_endLine;

	private float m_borderWidth;

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

	public PdfFont Font
	{
		get
		{
			if (m_font == null)
			{
				m_font = new PdfStandardFont(PdfFontFamily.Helvetica, 6f, PdfFontStyle.Regular);
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

	public int LeaderOffset
	{
		get
		{
			return m_leaderOffset;
		}
		set
		{
			m_leaderOffset = value;
			NotifyPropertyChanged("LeaderOffset");
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

	public PdfLineMeasurementAnnotation(int[] linePoints)
	{
		if (linePoints.Length > 4)
		{
			throw new PdfException("LineMeasurement annotation shoule not 2 points");
		}
		m_linePoints = new PdfArray(linePoints);
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
				array[num] = linePoint.IntValue;
				num++;
			}
		}
		return array;
	}

	private float ConvertToUnit()
	{
		float[] array = ObtainLinePoints();
		PointF[] array2 = new PointF[array.Length / 2];
		int num = 0;
		for (int i = 0; i < array.Length; i += 2)
		{
			array2[num] = new PointF(array[i], array[i + 1]);
			num++;
		}
		float value = (float)Math.Sqrt(Math.Pow(array2[1].X - array2[0].X, 2.0) + Math.Pow(array2[1].Y - array2[0].Y, 2.0));
		return new PdfUnitConvertor().ConvertUnits(value, PdfGraphicsUnit.Point, GetEqualPdfGraphicsUnit(Unit, out m_unitString));
	}

	private RectangleF ObtainLineBounds()
	{
		RectangleF result = Bounds;
		float[] array = ObtainLinePoints();
		if (array != null && array.Length == 4)
		{
			new PdfPath();
			PdfArray pdfArray = new PdfArray();
			pdfArray.Insert(0, new PdfName(BeginLineStyle));
			pdfArray.Insert(1, new PdfName(EndLineStyle));
			result = CalculateLineBounds(array, m_leaderLineExt, m_leaderLine, m_leaderOffset, pdfArray, m_borderWidth);
			result = new RectangleF(result.X - 8f, result.Y - 8f, result.Width + 16f, result.Height + 16f);
		}
		return result;
	}

	protected override void Save()
	{
		if (lineBorder.BorderWidth == 1)
		{
			m_borderWidth = lineBorder.BorderLineWidth;
		}
		else
		{
			m_borderWidth = lineBorder.BorderWidth;
		}
		if (base.Dictionary.Items.ContainsKey(new PdfName("AP")))
		{
			return;
		}
		RectangleF empty = RectangleF.Empty;
		float num = ConvertToUnit();
		float[] array = ObtainLinePoints();
		PointF[] array2 = new PointF[array.Length / 2];
		int num2 = 0;
		for (int i = 0; i < array.Length; i += 2)
		{
			array2[num2] = new PointF(array[i], array[i + 1]);
			num2++;
		}
		byte[] array3 = new byte[array2.Length];
		array3[0] = 0;
		array3[1] = 1;
		PdfPath pdfPath = new PdfPath(array2, array3);
		Bounds = pdfPath.GetBounds();
		PdfTemplate pdfTemplate = null;
		int num3 = 0;
		num3 = ((LeaderLine >= 0) ? m_leaderLine : (-m_leaderLine));
		base.Dictionary.SetProperty("AP", base.Appearance);
		if (base.Dictionary["AP"] != null)
		{
			empty = ObtainLineBounds();
			base.Appearance.Normal = new PdfTemplate(empty);
			pdfTemplate = base.Appearance.Normal;
			PaintParams paintParams = new PaintParams();
			pdfTemplate.m_writeTransformation = false;
			PdfGraphics graphics = base.Appearance.Normal.Graphics;
			PdfPen borderPen = new PdfPen(BackColor, m_borderWidth);
			PdfBrush backBrush = new PdfSolidBrush(InnerLineColor);
			paintParams.BorderPen = borderPen;
			paintParams.ForeBrush = new PdfSolidBrush(Color);
			new RectangleF(empty.X, 0f - empty.Y - empty.Height, empty.Width, empty.Height);
			float[] array4 = ObtainLinePoints();
			if (array4 != null && array4.Length == 4)
			{
				float num4 = array4[0];
				float num5 = array4[1];
				float num6 = array4[2];
				float num7 = array4[3];
				borderPen = new PdfPen(Color, m_borderWidth);
				if (lineBorder.BorderStyle == PdfBorderStyle.Dashed)
				{
					borderPen.DashStyle = PdfDashStyle.Dash;
				}
				else if (lineBorder.BorderStyle == PdfBorderStyle.Dot)
				{
					borderPen.DashStyle = PdfDashStyle.Dot;
				}
				PdfSolidBrush brush = new PdfSolidBrush(Color);
				new PdfStringFormat
				{
					Alignment = PdfTextAlignment.Center,
					LineAlignment = PdfVerticalAlignment.Middle
				};
				SizeF sizeF = Font.MeasureString(num.ToString("0.00") + " " + m_unitString);
				double num8 = 0.0;
				num8 = ((num6 - num4 != 0f) ? GetAngle(num4, num5, num6, num7) : ((!(num7 > num5)) ? 270.0 : 90.0));
				graphics.Save();
				if (Opacity < 1f)
				{
					graphics.SetTransparency(Opacity);
				}
				float[] value = new float[2] { num4, num5 };
				float[] value2 = new float[2] { num6, num7 };
				double num9 = 0.0;
				num9 = ((m_leaderLine >= 0) ? num8 : (num8 + 180.0));
				float[] axisValue = GetAxisValue(value, num9 + 90.0, num3 + m_leaderOffset);
				float[] axisValue2 = GetAxisValue(value2, num9 + 90.0, num3 + m_leaderOffset);
				double num10 = Math.Sqrt(Math.Pow(axisValue2[0] - axisValue[0], 2.0) + Math.Pow(axisValue2[1] - axisValue[1], 2.0));
				double length = num10 / 2.0 - (double)(sizeF.Width / 2f + m_borderWidth);
				float[] axisValue3 = GetAxisValue(axisValue, num8, length);
				float[] axisValue4 = GetAxisValue(axisValue2, num8 + 180.0, length);
				float[] array5 = ((BeginLineStyle != PdfLineEndingStyle.OpenArrow && BeginLineStyle != PdfLineEndingStyle.ClosedArrow) ? axisValue : GetAxisValue(axisValue, num8, m_borderWidth));
				float[] array6 = ((EndLineStyle != PdfLineEndingStyle.OpenArrow && EndLineStyle != PdfLineEndingStyle.ClosedArrow) ? axisValue2 : GetAxisValue(axisValue2, num8, 0f - m_borderWidth));
				string text = m_captionType.ToString();
				if (text == "Top")
				{
					graphics.DrawLine(borderPen, array5[0], 0f - array5[1], array6[0], 0f - array6[1]);
				}
				else
				{
					graphics.DrawLine(borderPen, array5[0], 0f - array5[1], axisValue3[0], 0f - axisValue3[1]);
					graphics.DrawLine(borderPen, array6[0], 0f - array6[1], axisValue4[0], 0f - axisValue4[1]);
				}
				PdfArray pdfArray = new PdfArray();
				pdfArray.Insert(0, new PdfName(BeginLineStyle));
				pdfArray.Insert(1, new PdfName(EndLineStyle));
				double borderLength = m_borderWidth;
				SetLineEndingStyles(axisValue, axisValue2, graphics, num8, borderPen, backBrush, pdfArray, borderLength);
				float[] axisValue5 = GetAxisValue(axisValue, num9 + 90.0, m_leaderLineExt);
				graphics.DrawLine(borderPen, axisValue[0], 0f - axisValue[1], axisValue5[0], 0f - axisValue5[1]);
				float[] axisValue6 = GetAxisValue(axisValue2, num9 + 90.0, m_leaderLineExt);
				graphics.DrawLine(borderPen, axisValue2[0], 0f - axisValue2[1], axisValue6[0], 0f - axisValue6[1]);
				float[] axisValue7 = GetAxisValue(axisValue, num9 - 90.0, num3);
				graphics.DrawLine(borderPen, axisValue[0], 0f - axisValue[1], axisValue7[0], 0f - axisValue7[1]);
				float[] axisValue8 = GetAxisValue(axisValue2, num9 - 90.0, num3);
				graphics.DrawLine(borderPen, axisValue2[0], 0f - axisValue2[1], axisValue8[0], 0f - axisValue8[1]);
				double length2 = num10 / 2.0;
				float[] axisValue9 = GetAxisValue(axisValue, num8, length2);
				float[] array7 = new float[2];
				array7 = ((!(text == "Top")) ? GetAxisValue(axisValue9, num8 + 90.0, Font.Height / 2f) : GetAxisValue(axisValue9, num8 + 90.0, Font.Height));
				graphics.TranslateTransform(array7[0], 0f - array7[1]);
				graphics.RotateTransform(CalculateAngle(new PointF(array4[0], array4[1]), new PointF(array4[2], array4[3])));
				graphics.DrawString(num.ToString("0.00") + " " + m_unitString, Font, brush, new PointF((0f - sizeF.Width) / 2f, 0f));
				graphics.Restore();
			}
		}
		base.Save();
		base.Dictionary.SetProperty("DS", new PdfString($"font:{Font.Name} {Font.Size}pt; color:{ColorToHex(Color)}"));
		PdfDictionary obj = CreateMeasureDictioanry(m_unitString);
		if (!base.Dictionary.Items.ContainsKey(new PdfName("Measure")))
		{
			base.Dictionary.Items.Add(new PdfName("Measure"), new PdfReferenceHolder(obj));
		}
		m_lineStyle = new PdfArray();
		m_lineStyle.Insert(0, new PdfName(BeginLineStyle));
		m_lineStyle.Insert(1, new PdfName(EndLineStyle));
		base.Dictionary.SetProperty("LE", m_lineStyle);
		if (m_linePoints != null)
		{
			PdfMargins pdfMargins = ObtainMargin();
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && m_linePoints.Count > 3)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					(m_linePoints[0] as PdfNumber).FloatValue = (m_linePoints[0] as PdfNumber).FloatValue + pdfNumber.FloatValue - pdfMargins.Left;
					(m_linePoints[1] as PdfNumber).FloatValue = (m_linePoints[1] as PdfNumber).FloatValue + pdfNumber2.FloatValue + pdfMargins.Top;
					(m_linePoints[2] as PdfNumber).FloatValue = (m_linePoints[2] as PdfNumber).FloatValue + pdfNumber.FloatValue - pdfMargins.Left;
					(m_linePoints[3] as PdfNumber).FloatValue = (m_linePoints[3] as PdfNumber).FloatValue + pdfNumber2.FloatValue + pdfMargins.Top;
				}
			}
			base.Dictionary.SetProperty("L", m_linePoints);
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
				base.Dictionary.SetProperty("IC", InnerLineColor.ToArray());
			}
			base.Dictionary["C"] = Color.ToArray();
			base.Dictionary.SetProperty("Contents", new PdfString(Text + "\n" + num.ToString("0.00") + " " + m_unitString));
			base.Dictionary.SetProperty("IT", new PdfName("LineDimension"));
			base.Dictionary.SetProperty("LLE", new PdfNumber(m_leaderLineExt));
			base.Dictionary.SetProperty("LLO", new PdfNumber(m_leaderOffset));
			base.Dictionary.SetProperty("LL", new PdfNumber(m_leaderLine));
			base.Dictionary.SetProperty("CP", new PdfName(m_captionType));
			base.Dictionary.SetProperty("Cap", new PdfBoolean(LineCaption));
			base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(ObtainLineBounds()));
			if (base.Flatten)
			{
				if (base.Page != null)
				{
					PdfStream content = pdfTemplate.m_content;
					base.Page.Document.SetWaterMarkResources(pdfTemplate.m_resources, base.Page.GetResources());
					content.Items.Clear();
					PdfReferenceHolder element = new PdfReferenceHolder(content as PdfStream);
					base.Page.Contents.Add(element);
					RemoveAnnoationFromPage(base.Page, this);
				}
				else
				{
					PdfStream content2 = pdfTemplate.m_content;
					base.LoadedPage.Document.SetWaterMarkResources(pdfTemplate.m_resources, base.LoadedPage.GetResources());
					content2.Items.Clear();
					PdfReferenceHolder element2 = new PdfReferenceHolder(content2 as PdfStream);
					base.LoadedPage.Contents.Add(element2);
					RemoveAnnoationFromPage(base.LoadedPage, this);
				}
			}
			return;
		}
		throw new PdfException("LinePoints cannot be null");
	}

	private float CalculateAngle(PointF startPoint, PointF endPoint)
	{
		float num = endPoint.Y - startPoint.Y;
		float num2 = endPoint.X - startPoint.X;
		return 0f - (float)(Math.Atan2(num, num2) * (180.0 / Math.PI));
	}
}
