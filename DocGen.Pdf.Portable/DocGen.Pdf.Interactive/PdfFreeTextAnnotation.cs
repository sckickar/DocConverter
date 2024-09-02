using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfFreeTextAnnotation : PdfAnnotation
{
	private const string c_annotationType = "FreeText";

	private PdfLineEndingStyle m_lineEndingStyle;

	private PdfAnnotationIntent m_annotationIntent = PdfAnnotationIntent.None;

	private string m_markUpText;

	private PdfFont m_font = new PdfStandardFont(PdfFontFamily.Helvetica, 7f);

	private PointF[] m_calloutLines = new PointF[0];

	private PdfColor m_textMarkupColor;

	private WidgetAnnotation m_widgetAnnotation = new WidgetAnnotation();

	private PdfColor m_borderColor;

	private PdfMargins m_margins = new PdfMargins();

	private bool m_complexScript;

	private PdfTextAlignment alignment;

	private PdfTextDirection m_textDirection;

	private float m_lineSpacing;

	private bool isAllRotation = true;

	private float m_cropBoxValueX;

	private float m_cropBoxValueY;

	private PdfPaddings m_paddings;

	public float LineSpacing
	{
		get
		{
			return m_lineSpacing;
		}
		set
		{
			m_lineSpacing = value;
			NotifyPropertyChanged("LineSpacing");
		}
	}

	public PdfLineEndingStyle LineEndingStyle
	{
		get
		{
			return m_lineEndingStyle;
		}
		set
		{
			m_lineEndingStyle = value;
			NotifyPropertyChanged("LineEndingStyle");
		}
	}

	public PdfAnnotationIntent AnnotationIntent
	{
		get
		{
			return m_annotationIntent;
		}
		set
		{
			m_annotationIntent = value;
			NotifyPropertyChanged("AnnotationIntent");
		}
	}

	public string MarkupText
	{
		get
		{
			return m_markUpText;
		}
		set
		{
			m_markUpText = value;
			NotifyPropertyChanged("MarkupText");
		}
	}

	public PdfFont Font
	{
		get
		{
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

	public PointF[] CalloutLines
	{
		get
		{
			return m_calloutLines;
		}
		set
		{
			m_calloutLines = value;
			NotifyPropertyChanged("CalloutLines");
		}
	}

	public PdfColor TextMarkupColor
	{
		get
		{
			return m_textMarkupColor;
		}
		set
		{
			m_textMarkupColor = value;
			NotifyPropertyChanged("TextMarkupColor");
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			m_borderColor = value;
			NotifyPropertyChanged("BorderColor");
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

	public bool ComplexScript
	{
		get
		{
			return m_complexScript;
		}
		set
		{
			m_complexScript = value;
			NotifyPropertyChanged("ComplexScript");
		}
	}

	public PdfTextAlignment TextAlignment
	{
		get
		{
			return alignment;
		}
		set
		{
			if (alignment != value)
			{
				alignment = value;
				base.Dictionary.SetProperty("Q", new PdfNumber((int)alignment));
			}
			NotifyPropertyChanged("TextAlignment");
		}
	}

	public PdfTextDirection TextDirection
	{
		get
		{
			return m_textDirection;
		}
		set
		{
			m_textDirection = value;
			NotifyPropertyChanged("TextDirection");
		}
	}

	private PdfFreeTextAnnotation()
	{
	}

	public PdfFreeTextAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
		base.Initialize();
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("FreeText"));
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		UpdateCropBoxValues();
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
			SaveFreeTextDictionary();
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

	private void SaveFreeTextDictionary()
	{
		base.Dictionary.SetProperty("Subtype", new PdfName("FreeText"));
		if (Font == null)
		{
			Font = new PdfStandardFont(PdfFontFamily.Helvetica, 7f);
		}
		PdfArray pdfArray = new PdfArray();
		if (!Color.IsEmpty)
		{
			float value = (float)(int)Color.R / 255f;
			float value2 = (float)(int)Color.G / 255f;
			float value3 = (float)(int)Color.B / 255f;
			pdfArray.Insert(0, new PdfNumber(value));
			pdfArray.Insert(1, new PdfNumber(value2));
			pdfArray.Insert(2, new PdfNumber(value3));
		}
		base.Dictionary.SetProperty("C", pdfArray);
		if (MarkupText == null)
		{
			MarkupText = Text;
		}
		base.Dictionary.SetProperty("Contents", new PdfString(m_markUpText));
		base.Dictionary.SetProperty("Q", new PdfNumber((int)alignment));
		if (Subject == string.Empty && m_annotationIntent == PdfAnnotationIntent.None && m_calloutLines != null && m_calloutLines.Length == 0)
		{
			base.Dictionary.SetProperty("Subj", new PdfString("Text Box"));
		}
		else
		{
			base.Dictionary.SetProperty("IT", new PdfName(m_annotationIntent.ToString()));
		}
		string empty = string.Empty;
		empty = ((!Font.Name.Contains("Times")) ? Font.Name : Font.Name.Replace(Font.Name, "Times"));
		Color c = DocGen.Drawing.Color.FromArgb(m_textMarkupColor.R, m_textMarkupColor.G, m_textMarkupColor.B);
		base.Dictionary.SetProperty("DS", new PdfString($"font:{empty} {Font.Size}pt;style:{Font.Style}; color:{ColorTranslator.ToHtml(c)}"));
		string value4 = $"{(float)(int)m_borderColor.R / 255f} {(float)(int)m_borderColor.G / 255f} {(float)(int)m_borderColor.B / 255f} rg ";
		base.Dictionary.SetProperty("DA", new PdfString(value4));
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string text = (Font.Bold ? "bold" : "normal");
		string empty4 = string.Empty;
		string empty5 = string.Empty;
		empty4 = ColorTranslator.ToHtml(c);
		empty3 = " style = \"" + $"font:{empty} {Font.Size}pt;font-weight:{text};color:{empty4}" + "\"";
		string text2 = string.Empty;
		string text3 = "font-style:italic";
		string text4 = "font-style:bold";
		if (Font.Underline)
		{
			text2 = (Font.Strikeout ? "text-decoration:word line-through" : "text-decoration:word");
			if (Font.Italic)
			{
				text2 = text2 + ";" + text3;
			}
			else if (Font.Bold)
			{
				text2 = text2 + ";" + text4;
			}
		}
		else if (Font.Strikeout)
		{
			text2 = "text-decoration:line-through";
			if (Font.Italic)
			{
				text2 = text2 + ";" + text3;
			}
			else if (Font.Bold)
			{
				text2 = text2 + ";" + text4;
			}
		}
		else if (Font.Italic)
		{
			text2 += text3;
		}
		else if (Font.Bold)
		{
			text2 += text4;
		}
		_ = alignment;
		empty5 = "text-align:" + alignment.ToString().ToLower() + ";";
		if (text2 != string.Empty)
		{
			_ = alignment;
			empty2 = "<span style = \"" + empty5 + text2 + "\">" + GetXmlFormattedString(MarkupText) + "</span>";
		}
		else
		{
			_ = alignment;
			empty2 = "<span style = \"" + empty5 + "\">" + GetXmlFormattedString(MarkupText) + "</span>";
		}
		base.Dictionary.SetString("RC", "<?xml version=\"1.0\"?><body xmlns=\"http://www.w3.org/1999/xhtml\"" + empty3 + "><p dir=\"ltr\">" + empty2 + "</p></body>");
		if (m_annotationIntent == PdfAnnotationIntent.FreeTextCallout || m_calloutLines.Length >= 2)
		{
			base.Dictionary.SetProperty("LE", new PdfName(m_lineEndingStyle.ToString()));
			m_margins = ObtainMargin();
			float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
			PdfArray pdfArray2 = new PdfArray();
			for (int i = 0; i < m_calloutLines.Length && i < 3; i++)
			{
				pdfArray2.Add(new PdfNumber(m_calloutLines[i].X + m_margins.Left + m_cropBoxValueX));
				pdfArray2.Add(new PdfNumber(num + m_cropBoxValueY - (m_calloutLines[i].Y + m_margins.Top)));
			}
			base.Dictionary.SetProperty("CL", pdfArray2);
			if (base.SetAppearanceDictionary)
			{
				base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(ObtainAppearanceBounds()));
			}
		}
	}

	private void UpdateCropBoxValues()
	{
		PdfArray pdfArray = null;
		PdfPageBase pdfPageBase = ((base.Page != null) ? ((PdfPageBase)base.Page) : ((PdfPageBase)base.LoadedPage));
		if (pdfPageBase.Dictionary.ContainsKey("CropBox"))
		{
			pdfArray = PdfCrossTable.Dereference(pdfPageBase.Dictionary["CropBox"]) as PdfArray;
		}
		else if (pdfPageBase.Dictionary.ContainsKey("MediaBox"))
		{
			pdfArray = PdfCrossTable.Dereference(pdfPageBase.Dictionary["MediaBox"]) as PdfArray;
		}
		if (pdfArray != null)
		{
			m_cropBoxValueX = (pdfArray[0] as PdfNumber).FloatValue;
			m_cropBoxValueY = (pdfArray[1] as PdfNumber).FloatValue;
		}
	}

	private float[] ObtainLinePoints()
	{
		float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
		PointF pointF = new PointF(CalloutLines[1].X + m_margins.Left + m_cropBoxValueX, num + m_cropBoxValueY - (CalloutLines[1].Y + m_margins.Top));
		PointF pointF2 = new PointF(CalloutLines[0].X + m_margins.Left + m_cropBoxValueX, num + m_cropBoxValueY - (CalloutLines[0].Y + m_margins.Top));
		return new float[4] { pointF.X, pointF.Y, pointF2.X, pointF2.Y };
	}

	private RectangleF ObtainAppearanceBounds()
	{
		RectangleF result = RectangleF.Empty;
		m_margins = ObtainMargin();
		if (CalloutLines != null && CalloutLines.Length != 0)
		{
			PdfPath pdfPath = new PdfPath();
			float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
			PointF[] array = null;
			array = ((CalloutLines.Length != 2) ? new PointF[3] : new PointF[CalloutLines.Length]);
			if (CalloutLines.Length >= 2)
			{
				for (int i = 0; i < CalloutLines.Length && i < 3; i++)
				{
					array[i] = new PointF(CalloutLines[i].X + m_margins.Left + m_cropBoxValueX, num + m_cropBoxValueY - (CalloutLines[i].Y + m_margins.Top));
				}
			}
			if (array.Length != 0)
			{
				if (LineEndingStyle != 0)
				{
					ExpandAppearanceForEndLineStyle(array);
				}
				pdfPath.AddLines(array);
			}
			result = pdfPath.GetBounds();
			if (base.Page != null)
			{
				pdfPath.AddRectangle(new RectangleF(Bounds.X + m_margins.Left - 2f, num - (Bounds.Y + Bounds.Height + m_margins.Top) - 2f, Bounds.Width + 4f, Bounds.Height + 4f));
			}
			else if (base.LoadedPage != null)
			{
				pdfPath.AddRectangle(new RectangleF(Bounds.X + m_cropBoxValueX - 2f, num + m_cropBoxValueY - (Bounds.Y + Bounds.Height) - 2f, Bounds.Width + 4f, Bounds.Height + 4f));
			}
			result = pdfPath.GetBounds();
		}
		else if (base.Page != null)
		{
			result = new RectangleF(Bounds.X + m_margins.Left, base.Page.Size.Height - (Bounds.Y + Bounds.Height + m_margins.Top), Bounds.Width, Bounds.Height);
		}
		else if (base.LoadedPage != null)
		{
			result = new RectangleF(Bounds.X, base.LoadedPage.Size.Height - (Bounds.Y + Bounds.Height), Bounds.Width, Bounds.Height);
		}
		return result;
	}

	private void ExpandAppearanceForEndLineStyle(PointF[] pointArray)
	{
		float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
		float y = pointArray[0].Y;
		float x = pointArray[0].X;
		y = num - y - m_margins.Top;
		float num2 = x - m_margins.Left;
		if (y > Bounds.Y)
		{
			if (LineEndingStyle != PdfLineEndingStyle.OpenArrow)
			{
				pointArray[0].Y -= Border.Width * 11f;
			}
		}
		else
		{
			pointArray[0].Y += Border.Width * 11f;
		}
		if (num2 <= Bounds.X)
		{
			pointArray[0].X -= Border.Width * 11f;
		}
		else
		{
			pointArray[0].X += Border.Width * 11f;
		}
	}

	private PdfTemplate CreateAppearance()
	{
		RectangleF rect = ObtainAppearanceBounds();
		PdfTemplate pdfTemplate = null;
		if (base.RotateAngle == 90f || base.RotateAngle == 180f || base.RotateAngle == 270f || base.RotateAngle == 0f)
		{
			isAllRotation = false;
		}
		pdfTemplate = ((!(base.RotateAngle > 0f) || !isAllRotation) ? new PdfTemplate(rect) : new PdfTemplate(rect.Size));
		SetMatrix(pdfTemplate.m_content);
		PaintParams paintParams = new PaintParams();
		pdfTemplate.m_writeTransformation = false;
		PdfGraphics graphics = pdfTemplate.Graphics;
		if (Border.Width > 0f && BorderColor.A != 0)
		{
			PdfPen borderPen = new PdfPen(BorderColor, Border.Width);
			paintParams.BorderPen = borderPen;
		}
		paintParams.ForeBrush = new PdfSolidBrush(Color);
		paintParams.BackBrush = new PdfSolidBrush(TextMarkupColor);
		paintParams.m_complexScript = ComplexScript;
		paintParams.m_textDirection = TextDirection;
		paintParams.m_lineSpacing = LineSpacing;
		paintParams.BorderWidth = Border.Width;
		RectangleF rectangleF = new RectangleF(rect.X, 0f - rect.Y, rect.Width, 0f - rect.Height);
		if (MarkupText == null)
		{
			MarkupText = Text;
		}
		paintParams.Bounds = rectangleF;
		float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
		if (CalloutLines.Length >= 2)
		{
			PdfPen pdfPen = null;
			if (BorderColor.A != 0)
			{
				pdfPen = new PdfPen(BorderColor, Border.Width);
			}
			DrawCallOuts(graphics, pdfPen);
			if (LineEndingStyle != 0)
			{
				float[] array = ObtainLinePoints();
				float num2 = array[0];
				float num3 = array[1];
				float num4 = array[2];
				float num5 = array[3];
				double num6 = 0.0;
				num6 = ((num4 - num2 != 0f) ? GetAngle(num2, num3, num4, num5) : ((!(num5 > num3)) ? 270.0 : 90.0));
				int num7 = 0;
				double num8 = 0.0;
				float[] value = new float[2] { num2, num3 };
				float[] value2 = new float[2] { num4, num5 };
				float[] axisValue = GetAxisValue(value, num8 + 90.0, num7);
				float[] axisValue2 = GetAxisValue(value2, num8 + 90.0, num7);
				PdfArray pdfArray = new PdfArray();
				pdfArray.Insert(0, null);
				pdfArray.Insert(1, new PdfName(LineEndingStyle));
				double borderLength = Border.Width;
				SetLineEndingStyles(axisValue, axisValue2, graphics, num6, pdfPen, paintParams.ForeBrush, pdfArray, borderLength);
			}
			if (base.Page != null)
			{
				rectangleF = new RectangleF(Bounds.X + m_margins.Left, 0f - (base.Page.Size.Height - (Bounds.Y + Bounds.Height + m_margins.Top)), Bounds.Width, 0f - Bounds.Height);
			}
			else if (base.LoadedPage != null)
			{
				rectangleF = new RectangleF(Bounds.X, 0f - (base.LoadedPage.Size.Height - (Bounds.Y + Bounds.Height)), Bounds.Width, 0f - Bounds.Height);
			}
			rectangleF.X += m_cropBoxValueX;
			rectangleF.Y -= m_cropBoxValueY;
			SetRectangleDifferance(rectangleF);
			paintParams.Bounds = rectangleF;
		}
		else
		{
			SetRectangleDifferance(rectangleF);
		}
		if (Opacity < 1f)
		{
			graphics.Save();
			graphics.SetTransparency(Opacity);
		}
		if (base.Rotate != 0)
		{
			graphics.Save();
		}
		DrawFreeTextRectangle(graphics, paintParams, rectangleF);
		DrawFreeMarkUpText(graphics, paintParams, rectangleF);
		if (base.Rotate != 0)
		{
			graphics.Restore();
		}
		if (Opacity < 1f)
		{
			graphics.Restore();
		}
		if (base.Flatten)
		{
			if (base.Page != null)
			{
				Bounds = new RectangleF(rect.X - m_margins.Left, num - (rect.Y + rect.Height) - m_margins.Top, rect.Width, rect.Height);
			}
			else
			{
				Bounds = new RectangleF(rect.X - m_cropBoxValueX, num + m_cropBoxValueY - (rect.Y + rect.Height), rect.Width, rect.Height);
			}
		}
		return pdfTemplate;
	}

	private void DrawArrow(PaintParams paintParams, PdfGraphics graphics, PdfPen mBorderPen)
	{
		float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
		PointF startingPoint = new PointF(CalloutLines[1].X + m_margins.Left, num - (CalloutLines[1].Y + m_margins.Top));
		PointF endPoint = new PointF(CalloutLines[0].X + m_margins.Left, num - (CalloutLines[0].Y + m_margins.Top));
		PointF[] array = CalculateArrowPoints(startingPoint, endPoint);
		PointF[] array2 = new PointF[3];
		byte[] array3 = new byte[3];
		array2[0] = new PointF(array[0].X, 0f - array[0].Y);
		array2[1] = new PointF(CalloutLines[0].X + m_margins.Left, 0f - (num - (CalloutLines[0].Y + m_margins.Top)));
		array2[2] = new PointF(array[1].X, 0f - array[1].Y);
		array3[0] = 0;
		array3[1] = 1;
		array3[2] = 1;
		PdfPath path = new PdfPath(array2, array3);
		if (paintParams.BorderPen != null)
		{
			graphics.DrawPath(mBorderPen, path);
		}
	}

	private void DrawCallOuts(PdfGraphics graphics, PdfPen mBorderPen)
	{
		PdfPath pdfPath = new PdfPath();
		PointF[] array = null;
		float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
		array = ((CalloutLines.Length != 2) ? new PointF[3] : new PointF[CalloutLines.Length]);
		for (int i = 0; i < CalloutLines.Length && i < 3; i++)
		{
			array[i] = new PointF(CalloutLines[i].X + m_margins.Left + m_cropBoxValueX, 0f - (num + m_cropBoxValueY - (CalloutLines[i].Y + m_margins.Top)));
		}
		if (array.Length != 0)
		{
			pdfPath.AddLines(array);
		}
		graphics.DrawPath(mBorderPen, pdfPath);
	}

	private PointF[] CalculateArrowPoints(PointF startingPoint, PointF endPoint)
	{
		float num = endPoint.X - startingPoint.X;
		float num2 = endPoint.Y - startingPoint.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		float num4 = num / num3;
		float num5 = num2 / num3;
		float num6 = 4f * (0f - num5 - num4);
		float num7 = 4f * (num4 - num5);
		PointF pointF = new PointF(endPoint.X + num6, endPoint.Y + num7);
		PointF pointF2 = new PointF(endPoint.X - num7, endPoint.Y + num6);
		return new PointF[2] { pointF, pointF2 };
	}

	private void DrawFreeTextRectangle(PdfGraphics graphics, PaintParams paintParams, RectangleF rect)
	{
		bool isRotation = false;
		if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle90 && !isAllRotation)
		{
			graphics.RotateTransform(-90f);
			paintParams.Bounds = new RectangleF(0f - rect.Y, rect.Width + rect.X, 0f - rect.Height, 0f - rect.Width);
		}
		else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle180 && !isAllRotation)
		{
			graphics.RotateTransform(-180f);
			paintParams.Bounds = new RectangleF(0f - (rect.Width + rect.X), 0f - (rect.Height + rect.Y), rect.Width, rect.Height);
		}
		else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270 && !isAllRotation)
		{
			graphics.RotateTransform(-270f);
			paintParams.Bounds = new RectangleF(rect.Y + rect.Height, 0f - rect.X, 0f - rect.Height, 0f - rect.Width);
		}
		if (paintParams.BorderWidth > 0f && !isAllRotation)
		{
			rect = new RectangleF(paintParams.Bounds.X + paintParams.BorderWidth / 2f, paintParams.Bounds.Y - paintParams.BorderWidth / 2f, paintParams.Bounds.Width - paintParams.BorderWidth, paintParams.Bounds.Height + paintParams.BorderWidth);
			if (base.LoadedPage != null && base.LoadedPage.MediaBox.Y > 0f && rect.Y < 0f)
			{
				rect.Y = base.LoadedPage.Size.Height + rect.Y;
			}
		}
		FieldPainter.DrawFreeTextAnnotation(graphics, paintParams, "", Font, rect, isSkipDrawRectangle: false, alignment, isRotation);
	}

	private void DrawFreeMarkUpText(PdfGraphics graphics, PaintParams paintParams, RectangleF rect)
	{
		bool isRotation = false;
		if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle90 && !isAllRotation)
		{
			rect = new RectangleF(0f - rect.Y, rect.X, 0f - rect.Height, rect.Width);
		}
		else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle180 && !isAllRotation)
		{
			rect = new RectangleF(0f - (rect.Width + rect.X), 0f - rect.Y, rect.Width, 0f - rect.Height);
		}
		else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270 && !isAllRotation)
		{
			rect = new RectangleF(rect.Y + rect.Height, 0f - (rect.Width + rect.X), 0f - rect.Height, rect.Width);
		}
		else if (base.RotateAngle == 0f && !isAllRotation)
		{
			rect = new RectangleF(rect.X, rect.Y + rect.Height, rect.Width, rect.Height);
		}
		float num = base.RotateAngle;
		if (num > 0f && isAllRotation)
		{
			isRotation = true;
			SizeF sizeF = Font.MeasureString(Text);
			if (num > 0f && num <= 91f)
			{
				graphics.TranslateTransform(sizeF.Height, 0f - Bounds.Height);
			}
			else if (num > 91f && num <= 181f)
			{
				graphics.TranslateTransform(Bounds.Width - sizeF.Height, 0f - (Bounds.Height - sizeF.Height));
			}
			else if (num > 181f && num <= 271f)
			{
				graphics.TranslateTransform(Bounds.Width - sizeF.Height, 0f - sizeF.Height);
			}
			else if (num > 271f && num < 360f)
			{
				graphics.TranslateTransform(sizeF.Height, 0f - sizeF.Height);
			}
			graphics.RotateTransform(base.RotateAngle);
			paintParams.Bounds = new RectangleF(0f, 0f, paintParams.Bounds.Width, paintParams.Bounds.Height);
		}
		RectangleF rectangleF = rect;
		if (m_paddings != null)
		{
			rect = ((!(paintParams.BorderWidth > 0f)) ? new RectangleF(rect.X + m_paddings.Left, rect.Y + m_paddings.Top, rect.Width - (m_paddings.Left + m_paddings.Right), (rect.Height > 0f) ? (rect.Height - (m_paddings.Top + m_paddings.Bottom)) : (0f - rect.Height - (m_paddings.Top + m_paddings.Bottom))) : new RectangleF(rect.X + (paintParams.BorderWidth + m_paddings.Left), rect.Y + (paintParams.BorderWidth + m_paddings.Top), rect.Width - (paintParams.BorderWidth * 2f + (m_paddings.Left + m_paddings.Right)), (rect.Height > 0f) ? (rect.Height - (paintParams.BorderWidth * 2f + (m_paddings.Top + m_paddings.Bottom))) : (0f - rect.Height - (paintParams.BorderWidth * 2f + (m_paddings.Top + m_paddings.Bottom)))));
		}
		else if (paintParams.BorderWidth > 0f)
		{
			rect = new RectangleF(rect.X + paintParams.BorderWidth * 2f, rect.Y + paintParams.BorderWidth * 2f, rect.Width - paintParams.BorderWidth * 4f, (rect.Height > 0f) ? (rect.Height - paintParams.BorderWidth * 4f) : (0f - rect.Height - paintParams.BorderWidth * 4f));
		}
		bool flag = Font.Height > ((rect.Height > 0f) ? rect.Height : (0f - rect.Height)) && Font.Height <= ((rectangleF.Height > 0f) ? rectangleF.Height : (0f - rectangleF.Height));
		if (base.LoadedPage != null && base.LoadedPage.MediaBox.Y > 0f && base.LoadedPage.MediaBox.Height == 0f && rect.Y < 0f)
		{
			rect.Y = base.LoadedPage.Size.Height + rect.Y;
		}
		FieldPainter.DrawFreeTextAnnotation(graphics, paintParams, MarkupText, Font, flag ? rectangleF : rect, isSkipDrawRectangle: true, alignment, isRotation);
	}

	private void SetRectangleDifferance(RectangleF innerRectangle)
	{
		RectangleF rectangleF = ObtainAppearanceBounds();
		float[] array = new float[4]
		{
			innerRectangle.X - rectangleF.X,
			0f - innerRectangle.Y - rectangleF.Y,
			innerRectangle.Width - rectangleF.Width,
			0f - innerRectangle.Y - rectangleF.Y + (0f - innerRectangle.Height) - rectangleF.Height
		};
		for (int i = 0; i < 4; i++)
		{
			if (array[i] < 0f)
			{
				array[i] = 0f - array[i];
			}
		}
		base.Dictionary["RD"] = new PdfArray(array);
	}

	internal void SetPaddings(PdfPaddings paddings)
	{
		m_paddings = paddings;
	}
}
