using System;
using System.Globalization;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedLineAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfColor m_backcolor;

	private LineBorder m_lineborder;

	private PdfArray m_linePoints;

	private int[] m_points;

	private float[] m_point;

	internal PdfFont m_font;

	private PdfRecordCollection readTextCollection;

	private ContentParser parser;

	private bool m_isfontAPStream;

	private MemoryStream freeTextStream;

	private float m_borderWidth;

	public PdfLoadedPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory == null)
			{
				m_reviewHistory = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: true);
			}
			return m_reviewHistory;
		}
	}

	public PdfLoadedPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments == null)
			{
				m_comments = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: false);
			}
			return m_comments;
		}
	}

	public PdfColor BackColor
	{
		get
		{
			return ObtainBackColor();
		}
		set
		{
			PdfArray pdfArray = new PdfArray();
			m_backcolor = value;
			pdfArray.Insert(0, new PdfNumber((float)(int)m_backcolor.R / 255f));
			pdfArray.Insert(1, new PdfNumber((float)(int)m_backcolor.G / 255f));
			pdfArray.Insert(2, new PdfNumber((float)(int)m_backcolor.B / 255f));
			base.Dictionary.SetProperty("C", pdfArray);
			NotifyPropertyChanged("BackColor");
		}
	}

	public PdfLineEndingStyle BeginLineStyle
	{
		get
		{
			return GetLineStyle(0);
		}
		set
		{
			PdfArray pdfArray = GetLineStyle();
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
				pdfArray.Insert(1, new PdfName(PdfLineEndingStyle.None));
			}
			else
			{
				pdfArray.RemoveAt(0);
			}
			pdfArray.Insert(0, new PdfName(GetLineStyle(value.ToString())));
			base.Dictionary.SetProperty("LE", pdfArray);
			NotifyPropertyChanged("BeginLineStyle");
		}
	}

	public PdfLineCaptionType CaptionType
	{
		get
		{
			return ObtainCaptionType();
		}
		set
		{
			base.Dictionary.SetProperty("CP", new PdfName(GetCaptionType(value.ToString())));
			NotifyPropertyChanged("CaptionType");
		}
	}

	public PdfLineEndingStyle EndLineStyle
	{
		get
		{
			return GetLineStyle(1);
		}
		set
		{
			PdfArray pdfArray = GetLineStyle();
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
				pdfArray.Insert(0, new PdfName(PdfLineEndingStyle.None));
			}
			else
			{
				pdfArray.RemoveAt(1);
			}
			pdfArray.Insert(1, new PdfName(GetLineStyle(value.ToString())));
			base.Dictionary.SetProperty("LE", pdfArray);
			NotifyPropertyChanged("EndLineStyle");
		}
	}

	public PdfColor InnerLineColor
	{
		get
		{
			return ObtainInnerLineColor();
		}
		set
		{
			m_backcolor = value;
			if (value.A != 0)
			{
				base.Dictionary.SetProperty("IC", value.ToArray());
			}
			else if (base.Dictionary.ContainsKey("IC"))
			{
				base.Dictionary.Remove("IC");
			}
			NotifyPropertyChanged("InnerLineColor");
		}
	}

	public int LeaderLine
	{
		get
		{
			return ObtainLeaderLine();
		}
		set
		{
			base.Dictionary.SetNumber("LL", value);
			NotifyPropertyChanged("LeaderLine");
		}
	}

	public int LeaderExt
	{
		get
		{
			return ObtainLeaderExt();
		}
		set
		{
			base.Dictionary.SetNumber("LLE", value);
			NotifyPropertyChanged("LeaderExt");
		}
	}

	public int LeaderOffset
	{
		get
		{
			return ObtainLeaderOffset();
		}
		set
		{
			base.Dictionary.SetNumber("LLO", value);
			NotifyPropertyChanged("LeaderOffset");
		}
	}

	public LineBorder LineBorder
	{
		get
		{
			return ObtainLineBorder();
		}
		set
		{
			m_lineborder = value;
			base.Dictionary.SetProperty("BS", m_lineborder);
			NotifyPropertyChanged("LineBorder");
		}
	}

	public int[] LinePoints
	{
		get
		{
			m_points = ObtainLinePoints();
			return m_points;
		}
		set
		{
			m_points = value;
			m_linePoints = new PdfArray(m_points);
			int[] points = m_points;
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					new PdfMargins();
					ObtainMargin();
					float floatValue = pdfNumber.FloatValue;
					float floatValue2 = pdfNumber2.FloatValue;
					points[0] += (int)floatValue;
					points[1] += (int)floatValue2;
					points[2] += (int)floatValue;
					points[3] += (int)floatValue2;
				}
			}
			base.Dictionary.SetProperty("L", new PdfArray(points));
			NotifyPropertyChanged("LinePoints");
		}
	}

	internal float[] LinePoint
	{
		get
		{
			m_point = ObtainLinePoint();
			return m_point;
		}
		set
		{
			m_point = value;
			m_linePoints = new PdfArray(m_point);
			float[] point = m_point;
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					new PdfMargins();
					ObtainMargin();
					float floatValue = pdfNumber.FloatValue;
					float floatValue2 = pdfNumber2.FloatValue;
					point[0] += floatValue;
					point[1] += floatValue2;
					point[2] += floatValue;
					point[3] += floatValue2;
				}
			}
			base.Dictionary.SetProperty("L", new PdfArray(point));
		}
	}

	public bool LineCaption
	{
		get
		{
			return ObtainLineCaption();
		}
		set
		{
			base.Dictionary.SetBoolean("Cap", value);
			NotifyPropertyChanged("LineCaption");
		}
	}

	public PdfLineIntent LineIntent
	{
		get
		{
			return ObtainLineIntent();
		}
		set
		{
			base.Dictionary.SetName("IT", value.ToString());
			NotifyPropertyChanged("LineIntent");
		}
	}

	internal PdfLoadedLineAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_text = text;
	}

	private PdfLineIntent ObtainLineIntent()
	{
		PdfLineIntent result = PdfLineIntent.LineDimension;
		if (base.Dictionary.ContainsKey("IT"))
		{
			PdfName pdfName = m_crossTable.GetObject(base.Dictionary["IT"]) as PdfName;
			result = GetLineIntentText(pdfName.Value.ToString());
		}
		return result;
	}

	private PdfArray GetLineStyle()
	{
		PdfArray result = null;
		if (base.Dictionary.ContainsKey("LE"))
		{
			result = m_crossTable.GetObject(base.Dictionary["LE"]) as PdfArray;
		}
		return result;
	}

	private PdfLineEndingStyle GetLineStyle(int Ch)
	{
		PdfLineEndingStyle result = PdfLineEndingStyle.None;
		PdfArray lineStyle = GetLineStyle();
		if (lineStyle != null)
		{
			PdfName pdfName = lineStyle[Ch] as PdfName;
			result = GetLineStyle(pdfName.Value);
		}
		return result;
	}

	private PdfLineEndingStyle GetLineStyle(string style)
	{
		PdfLineEndingStyle result = PdfLineEndingStyle.None;
		switch (style)
		{
		case "Square":
			result = PdfLineEndingStyle.Square;
			break;
		case "Circle":
			result = PdfLineEndingStyle.Circle;
			break;
		case "Diamond":
			result = PdfLineEndingStyle.Diamond;
			break;
		case "OpenArrow":
			result = PdfLineEndingStyle.OpenArrow;
			break;
		case "ClosedArrow":
			result = PdfLineEndingStyle.ClosedArrow;
			break;
		case "None":
			result = PdfLineEndingStyle.None;
			break;
		case "ROpenArrow":
			result = PdfLineEndingStyle.ROpenArrow;
			break;
		case "Butt":
			result = PdfLineEndingStyle.Butt;
			break;
		case "RClosedArrow":
			result = PdfLineEndingStyle.RClosedArrow;
			break;
		case "Slash":
			result = PdfLineEndingStyle.Slash;
			break;
		}
		return result;
	}

	private PdfColor ObtainInnerLineColor()
	{
		PdfColorSpace colorSpace = PdfColorSpace.RGB;
		PdfColor result = PdfColor.Empty;
		PdfArray pdfArray = null;
		bool flag = false;
		if (base.Dictionary.ContainsKey("IC"))
		{
			pdfArray = PdfCrossTable.Dereference(base.Dictionary["IC"]) as PdfArray;
			flag = true;
		}
		else
		{
			pdfArray = result.ToArray(colorSpace);
		}
		if (pdfArray != null && pdfArray[0] is PdfNumber && pdfArray[1] is PdfNumber && pdfArray[2] is PdfNumber)
		{
			byte red = (byte)Math.Round((pdfArray[0] as PdfNumber).FloatValue * 255f);
			byte green = (byte)Math.Round((pdfArray[1] as PdfNumber).FloatValue * 255f);
			byte blue = (byte)Math.Round((pdfArray[2] as PdfNumber).FloatValue * 255f);
			result = ((!flag) ? new PdfColor(0, red, green, blue) : new PdfColor(red, green, blue));
		}
		return result;
	}

	private PdfColor ObtainBackColor()
	{
		PdfColorSpace colorSpace = PdfColorSpace.RGB;
		PdfColor result = PdfColor.Empty;
		PdfArray pdfArray = null;
		pdfArray = ((!base.Dictionary.ContainsKey("C")) ? result.ToArray(colorSpace) : (PdfCrossTable.Dereference(base.Dictionary["C"]) as PdfArray));
		if (pdfArray != null && pdfArray[0] is PdfNumber && pdfArray[1] is PdfNumber && pdfArray[2] is PdfNumber)
		{
			byte red = (byte)Math.Round((pdfArray[0] as PdfNumber).FloatValue * 255f);
			byte green = (byte)Math.Round((pdfArray[1] as PdfNumber).FloatValue * 255f);
			byte blue = (byte)Math.Round((pdfArray[2] as PdfNumber).FloatValue * 255f);
			result = new PdfColor(red, green, blue);
		}
		return result;
	}

	private PdfLineCaptionType ObtainCaptionType()
	{
		PdfLineCaptionType result = PdfLineCaptionType.Inline;
		if (base.Dictionary.ContainsKey("CP"))
		{
			PdfName pdfName = base.Dictionary["CP"] as PdfName;
			result = GetCaptionType(pdfName.Value.ToString());
		}
		return result;
	}

	private PdfLineCaptionType GetCaptionType(string cType)
	{
		PdfLineCaptionType pdfLineCaptionType = PdfLineCaptionType.Inline;
		if (cType == "Inline")
		{
			return PdfLineCaptionType.Inline;
		}
		return PdfLineCaptionType.Top;
	}

	private bool ObtainLineCaption()
	{
		bool result = false;
		if (base.Dictionary.ContainsKey("Cap"))
		{
			result = (base.Dictionary["Cap"] as PdfBoolean).Value;
		}
		return result;
	}

	private int ObtainLeaderLine()
	{
		int result = 0;
		if (base.Dictionary.ContainsKey("LL"))
		{
			result = (base.Dictionary["LL"] as PdfNumber).IntValue;
		}
		return result;
	}

	private int ObtainLeaderExt()
	{
		int result = 0;
		if (base.Dictionary.ContainsKey("LLE"))
		{
			result = (base.Dictionary["LLE"] as PdfNumber).IntValue;
		}
		return result;
	}

	private int ObtainLeaderOffset()
	{
		int result = 0;
		if (base.Dictionary.ContainsKey("LLO"))
		{
			result = (base.Dictionary["LLO"] as PdfNumber).IntValue;
		}
		return result;
	}

	private LineBorder ObtainLineBorder()
	{
		LineBorder lineBorder = new LineBorder();
		if (base.Dictionary.ContainsKey("Border"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Border"]) is PdfArray { Count: >=2 } pdfArray && pdfArray[2] is PdfNumber)
			{
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				lineBorder.BorderWidth = (int)floatValue;
				lineBorder.BorderLineWidth = floatValue;
			}
		}
		else if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("W"))
			{
				int intValue = (pdfDictionary["W"] as PdfNumber).IntValue;
				float floatValue2 = (pdfDictionary["W"] as PdfNumber).FloatValue;
				lineBorder.BorderWidth = intValue;
				lineBorder.BorderLineWidth = floatValue2;
			}
			if (pdfDictionary.ContainsKey("S"))
			{
				PdfName pdfName = pdfDictionary["S"] as PdfName;
				lineBorder.BorderStyle = GetBorderStyle(pdfName.Value.ToString());
			}
			if (pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfArray { Count: >0 } pdfArray2)
			{
				int intValue2 = (pdfArray2[0] as PdfNumber).IntValue;
				pdfArray2.Clear();
				pdfArray2.Insert(0, new PdfNumber(intValue2));
				pdfArray2.Insert(1, new PdfNumber(intValue2));
				lineBorder.DashArray = intValue2;
			}
		}
		return lineBorder;
	}

	private int[] ObtainLinePoints()
	{
		int[] array = null;
		PdfArray cropOrMediaBox = null;
		if (base.Dictionary.ContainsKey("L"))
		{
			m_linePoints = PdfCrossTable.Dereference(base.Dictionary["L"]) as PdfArray;
			if (m_linePoints != null)
			{
				array = new int[m_linePoints.Count];
				int num = 0;
				foreach (PdfNumber linePoint in m_linePoints)
				{
					array[num] = linePoint.IntValue;
					num++;
				}
			}
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
			{
				PdfNumber pdfNumber2 = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[1] as PdfNumber;
				if (pdfNumber2 != null && pdfNumber3 != null && (pdfNumber2.FloatValue != 0f || pdfNumber3.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					_ = m_points;
					float floatValue = pdfNumber2.FloatValue;
					float floatValue2 = pdfNumber3.FloatValue;
					array[0] = (int)(0f - floatValue + (float)array[0] + pdfMargins.Left);
					array[1] = (int)((float)array[1] - floatValue2 - pdfMargins.Top);
					array[2] = (int)(0f - floatValue + (float)array[2] + pdfMargins.Left);
					array[3] = (int)((float)array[3] - floatValue2 - pdfMargins.Top);
				}
			}
		}
		return array;
	}

	private float[] ObtainLinePoint()
	{
		float[] array = null;
		PdfArray cropOrMediaBox = null;
		if (base.Dictionary.ContainsKey("L"))
		{
			m_linePoints = PdfCrossTable.Dereference(base.Dictionary["L"]) as PdfArray;
			if (m_linePoints != null)
			{
				array = new float[m_linePoints.Count];
				int num = 0;
				foreach (PdfNumber linePoint in m_linePoints)
				{
					array[num] = linePoint.FloatValue;
					num++;
				}
				cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
				if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
				{
					PdfNumber pdfNumber2 = cropOrMediaBox[0] as PdfNumber;
					PdfNumber pdfNumber3 = cropOrMediaBox[1] as PdfNumber;
					if (pdfNumber2 != null && pdfNumber3 != null && (pdfNumber2.FloatValue != 0f || pdfNumber3.FloatValue != 0f))
					{
						PdfMargins pdfMargins = new PdfMargins();
						pdfMargins = ObtainMargin();
						float floatValue = pdfNumber2.FloatValue;
						float floatValue2 = pdfNumber3.FloatValue;
						array[0] = 0f - floatValue + array[0] + pdfMargins.Left;
						array[1] = array[1] - floatValue2 - pdfMargins.Top;
						array[2] = 0f - floatValue + array[2] + pdfMargins.Left;
						array[3] = array[3] - floatValue2 - pdfMargins.Top;
					}
				}
			}
		}
		return array;
	}

	private new PdfBorderStyle GetBorderStyle(string bstyle)
	{
		PdfBorderStyle result = PdfBorderStyle.Solid;
		switch (bstyle)
		{
		case "S":
			result = PdfBorderStyle.Solid;
			break;
		case "D":
			result = PdfBorderStyle.Dashed;
			break;
		case "B":
			result = PdfBorderStyle.Beveled;
			break;
		case "I":
			result = PdfBorderStyle.Inset;
			break;
		case "U":
			result = PdfBorderStyle.Underline;
			break;
		}
		return result;
	}

	private PdfLineIntent GetLineIntentText(string lintent)
	{
		PdfLineIntent result = PdfLineIntent.LineArrow;
		if (!(lintent == "LineArrow"))
		{
			if (lintent == "LineDimension")
			{
				result = PdfLineIntent.LineDimension;
			}
		}
		else
		{
			result = PdfLineIntent.LineArrow;
		}
		return result;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		int num = 0;
		if (base.Dictionary.ContainsKey("F"))
		{
			num = (base.Dictionary["F"] as PdfNumber).IntValue;
		}
		m_borderWidth = LineBorder.BorderLineWidth;
		SaveAnnotationBorder(m_borderWidth);
		if (num != 2)
		{
			if (base.Flatten || base.Page.Annotations.Flatten || base.SetAppearanceDictionary || isExternalFlatten)
			{
				PdfTemplate pdfTemplate = CreateAppearance();
				if (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten)
				{
					FlattenAnnotation(base.Page, pdfTemplate);
				}
				else if (pdfTemplate != null)
				{
					base.Appearance.Normal = pdfTemplate;
					base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
				}
			}
		}
		else
		{
			RemoveAnnoationFromPage(base.Page, this);
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenLoadedPopup();
		}
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		if (base.Dictionary.ContainsKey("AP") && appearance == null)
		{
			if (!(PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary))
			{
				return;
			}
			if (PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2)
			{
				if (!(pdfDictionary2 is PdfStream template))
				{
					return;
				}
				appearance = new PdfTemplate(template);
				if (!pdfDictionary2.ContainsKey("Matrix"))
				{
					SetMatrix(appearance.m_content);
				}
				if (appearance != null)
				{
					bool flag = ValidateTemplateMatrix(pdfDictionary2);
					if ((flag && base.Page.Rotation != 0) || IsValidTemplateMatrix(pdfDictionary2, Bounds.Location, appearance))
					{
						FlattenAnnotationTemplate(appearance, flag);
					}
				}
			}
			else
			{
				base.SetAppearanceDictionary = true;
				appearance = CreateAppearance();
				if (appearance != null)
				{
					bool isNormalMatrix = ValidateTemplateMatrix(appearance.m_content);
					FlattenAnnotationTemplate(appearance, isNormalMatrix);
				}
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance == null)
		{
			base.SetAppearanceDictionary = true;
			appearance = CreateAppearance();
			if (appearance != null)
			{
				bool isNormalMatrix2 = ValidateTemplateMatrix(appearance.m_content);
				FlattenAnnotationTemplate(appearance, isNormalMatrix2);
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix3 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix3);
		}
		else if (base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix4 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix4);
		}
	}

	private RectangleF ObtainLineBounds()
	{
		RectangleF result = Bounds;
		if ((LinePoints != null && m_points != null) || (LinePoint != null && m_point != null))
		{
			new PdfPath();
			float[] array = ObtainLinePoint();
			if (array != null && array.Length == 4)
			{
				PdfArray pdfArray = new PdfArray();
				pdfArray.Insert(0, new PdfName(BeginLineStyle));
				pdfArray.Insert(1, new PdfName(EndLineStyle));
				result = CalculateLineBounds(array, LeaderExt, LeaderLine, LeaderOffset, pdfArray, m_borderWidth);
				result = new RectangleF(result.X - 8f, result.Y - 8f, result.Width + 16f, result.Height + 16f);
			}
		}
		return result;
	}

	private PdfTemplate CreateAppearance()
	{
		if (base.SetAppearanceDictionary)
		{
			PdfTemplate pdfTemplate = new PdfTemplate(ObtainLineBounds());
			SetMatrix(pdfTemplate.m_content);
			PaintParams paintParams = new PaintParams();
			pdfTemplate.m_writeTransformation = false;
			PdfGraphics graphics = pdfTemplate.Graphics;
			PdfPen pdfPen = new PdfPen(BackColor, m_borderWidth);
			PdfBrush backBrush = new PdfSolidBrush(InnerLineColor);
			if (LineBorder.BorderStyle == PdfBorderStyle.Dashed)
			{
				pdfPen.DashStyle = PdfDashStyle.Dash;
			}
			else if (LineBorder.BorderStyle == PdfBorderStyle.Dot)
			{
				pdfPen.DashStyle = PdfDashStyle.Dot;
			}
			paintParams.BorderPen = pdfPen;
			paintParams.ForeBrush = new PdfSolidBrush(Color);
			PdfFont pdfFont = ObtainFont();
			if (pdfFont == null)
			{
				if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
				{
					pdfFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
				}
				else
				{
					PdfLoadedDocument pdfLoadedDocument = m_crossTable.Document as PdfLoadedDocument;
					if (pdfLoadedDocument.RaisePdfFont)
					{
						PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
						pdfFontEventArgs.m_fontName = "Arial";
						pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
						pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
						pdfFont = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, 10f, string.Empty, useTrueType: true, isEnableEmbedding: true);
					}
				}
			}
			PdfStringFormat pdfStringFormat = new PdfStringFormat();
			pdfStringFormat.Alignment = PdfTextAlignment.Center;
			pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
			float width = pdfFont.MeasureString(Text, pdfStringFormat).Width;
			PdfSolidBrush brush = new PdfSolidBrush(Color);
			int[] array = ObtainLinePoints();
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					new PdfMargins();
					ObtainMargin();
					_ = m_points;
					float floatValue = pdfNumber.FloatValue;
					float floatValue2 = pdfNumber2.FloatValue;
					array[0] += (int)floatValue;
					array[1] += (int)floatValue2;
					array[2] += (int)floatValue;
					array[3] += (int)floatValue2;
				}
			}
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
					num6 = -LeaderLine;
					num7 = num5 + 180.0;
				}
				else
				{
					num6 = LeaderLine;
					num7 = num5;
				}
				float[] value = new float[2] { num, num2 };
				float[] value2 = new float[2] { num3, num4 };
				float[] axisValue = GetAxisValue(value, num7 + 90.0, num6 + LeaderOffset);
				float[] axisValue2 = GetAxisValue(value2, num7 + 90.0, num6 + LeaderOffset);
				double num8 = Math.Sqrt(Math.Pow(axisValue2[0] - axisValue[0], 2.0) + Math.Pow(axisValue2[1] - axisValue[1], 2.0));
				double length = num8 / 2.0 - (double)(width / 2f + m_borderWidth);
				float[] axisValue3 = GetAxisValue(axisValue, num5, length);
				float[] axisValue4 = GetAxisValue(axisValue2, num5 + 180.0, length);
				float[] array2 = ((BeginLineStyle != PdfLineEndingStyle.OpenArrow && BeginLineStyle != PdfLineEndingStyle.ClosedArrow) ? axisValue : GetAxisValue(axisValue, num5, m_borderWidth));
				float[] array3 = ((EndLineStyle != PdfLineEndingStyle.OpenArrow && EndLineStyle != PdfLineEndingStyle.ClosedArrow) ? axisValue2 : GetAxisValue(axisValue2, num5, 0f - m_borderWidth));
				string text = CaptionType.ToString();
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
				SetLineEndingStyles(axisValue, axisValue2, graphics, num5, pdfPen, backBrush, pdfArray, borderLength);
				float[] axisValue5 = GetAxisValue(axisValue, num7 + 90.0, LeaderExt);
				graphics.DrawLine(pdfPen, axisValue[0], 0f - axisValue[1], axisValue5[0], 0f - axisValue5[1]);
				float[] axisValue6 = GetAxisValue(axisValue2, num7 + 90.0, LeaderExt);
				graphics.DrawLine(pdfPen, axisValue2[0], 0f - axisValue2[1], axisValue6[0], 0f - axisValue6[1]);
				float[] axisValue7 = GetAxisValue(axisValue, num7 - 90.0, num6);
				graphics.DrawLine(pdfPen, axisValue[0], 0f - axisValue[1], axisValue7[0], 0f - axisValue7[1]);
				float[] axisValue8 = GetAxisValue(axisValue2, num7 - 90.0, num6);
				graphics.DrawLine(pdfPen, axisValue2[0], 0f - axisValue2[1], axisValue8[0], 0f - axisValue8[1]);
				double length2 = num8 / 2.0;
				float[] axisValue9 = GetAxisValue(axisValue, num5, length2);
				float[] array4 = new float[2];
				array4 = ((text == "Top") ? ((!base.Dictionary.Items.ContainsKey(new PdfName("Measure"))) ? GetAxisValue(axisValue9, num5 + 90.0, pdfFont.Height) : GetAxisValue(axisValue9, num5 + 90.0, 2f * pdfFont.Height)) : ((!base.Dictionary.Items.ContainsKey(new PdfName("Measure"))) ? GetAxisValue(axisValue9, num5 + 90.0, pdfFont.Height / 2f) : GetAxisValue(axisValue9, num5 + 90.0, 3f * (pdfFont.Height / 2f))));
				graphics.TranslateTransform(array4[0], 0f - array4[1]);
				graphics.RotateTransform((float)(0.0 - num5));
				if (LineCaption)
				{
					graphics.DrawString(Text, pdfFont, brush, new PointF((0f - width) / 2f, 0f));
				}
				graphics.Restore();
			}
			base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(ObtainLineBounds()));
			return pdfTemplate;
		}
		return null;
	}

	private PdfFont ObtainFont()
	{
		PdfString pdfString = null;
		string text = "";
		float result = 1f;
		if (base.Dictionary.ContainsKey("DS") || base.Dictionary.ContainsKey("DA"))
		{
			if (base.Dictionary.ContainsKey("DS"))
			{
				pdfString = PdfCrossTable.Dereference(base.Dictionary["DS"]) as PdfString;
				string[] array = pdfString.Value.ToString().Split(new char[1] { ';' });
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(new char[1] { ':' });
					if (array[i].Contains("font-family"))
					{
						text = array2[1];
					}
					else if (array[i].Contains("font-size"))
					{
						if (array2[1].EndsWith("pt"))
						{
							result = float.Parse(array2[1].Replace("pt", ""), CultureInfo.InvariantCulture);
						}
					}
					else if (array[i].Contains("font-style"))
					{
						_ = array[1];
					}
					else
					{
						if (!array[i].Contains("font"))
						{
							continue;
						}
						string obj = array2[1];
						text = string.Empty;
						string[] array3 = obj.Split(new char[1] { ' ' });
						for (int j = 0; j < array3.Length; j++)
						{
							if (array3[j] != "" && !array3[j].EndsWith("pt"))
							{
								text = text + array3[j] + " ";
							}
							if (array3[j].EndsWith("pt"))
							{
								result = float.Parse(array3[j].Replace("pt", ""), CultureInfo.InvariantCulture);
							}
						}
						text = text.TrimEnd(' ');
						if (text.Contains(","))
						{
							text = text.Split(new char[1] { ',' })[0];
						}
					}
				}
			}
			else if (base.Dictionary.ContainsKey("DA"))
			{
				pdfString = base.Dictionary["DA"] as PdfString;
				string text2 = string.Empty;
				if (pdfString != null)
				{
					text2 = pdfString.Value;
				}
				if (text2 != string.Empty && text2.Contains("Tf"))
				{
					string[] array4 = text2.ToString().Split(new char[1] { ' ' });
					for (int k = 0; k < array4.Length; k++)
					{
						if (array4[k].Contains("Tf"))
						{
							text = array4[k - 2].TrimStart('/');
							float.TryParse(array4[k - 1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out result);
						}
					}
				}
				else if (base.Dictionary["AP"] != null)
				{
					m_isfontAPStream = true;
					ObtainFromAPStream(m_isfontAPStream);
					return m_font;
				}
			}
			text = text.Trim();
			if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
			{
				switch (text)
				{
				case "Helvetica":
					m_font = new PdfStandardFont(PdfFontFamily.Helvetica, result);
					break;
				case "Courier":
					m_font = new PdfStandardFont(PdfFontFamily.Courier, result);
					break;
				case "Symbol":
					m_font = new PdfStandardFont(PdfFontFamily.Symbol, result);
					break;
				case "TimesRoman":
					m_font = new PdfStandardFont(PdfFontFamily.TimesRoman, result);
					break;
				case "ZapfDingbats":
					m_font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, result);
					break;
				case "MonotypeSungLight":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeSungLight, result);
					break;
				case "SinoTypeSongLight":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.SinoTypeSongLight, result);
					break;
				case "MonotypeHeiMedium":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeHeiMedium, result);
					break;
				case "HanyangSystemsGothicMedium":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsGothicMedium, result);
					break;
				case "HanyangSystemsShinMyeongJoMedium":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium, result);
					break;
				case "HeiseiKakuGothicW5":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiKakuGothicW5, result);
					break;
				case "HeiseiMinchoW3":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiMinchoW3, result);
					break;
				default:
					m_font = new PdfStandardFont(PdfFontFamily.Helvetica, result);
					break;
				}
			}
			else
			{
				PdfLoadedDocument pdfLoadedDocument = m_crossTable.Document as PdfLoadedDocument;
				if (pdfLoadedDocument.RaisePdfFont)
				{
					PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
					pdfFontEventArgs.m_fontName = "Arial";
					pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
					pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
					m_font = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, result, string.Empty, useTrueType: false, isEnableEmbedding: true);
				}
			}
		}
		else if (base.Dictionary["AP"] != null)
		{
			m_isfontAPStream = true;
			ObtainFromAPStream(m_isfontAPStream);
		}
		if (freeTextStream != null)
		{
			freeTextStream.Dispose();
		}
		return m_font;
	}

	private void ObtainFromAPStream(bool isfontStream)
	{
		if (!(PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary) || !(PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2))
		{
			return;
		}
		PdfDictionary pdfDictionary3 = pdfDictionary2["Resources"] as PdfDictionary;
		if (pdfDictionary3 == null)
		{
			PdfReferenceHolder pdfReferenceHolder = pdfDictionary2["Resources"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				pdfDictionary3 = pdfReferenceHolder.Object as PdfDictionary;
			}
		}
		if (pdfDictionary3 == null || !(pdfDictionary2 is PdfStream pdfStream))
		{
			return;
		}
		parser = new ContentParser(pdfStream.GetDecompressedData());
		readTextCollection = parser.ReadContent();
		if (readTextCollection == null || !isfontStream)
		{
			return;
		}
		string text = "";
		float size = 1f;
		foreach (PdfRecord item in readTextCollection)
		{
			_ = string.Empty;
			string[] operands = item.Operands;
			string operatorName = item.OperatorName;
			string text2 = null;
			if (operatorName == "Tf")
			{
				text2 = operands[0];
				PdfDictionary pdfDictionary4 = pdfDictionary3["Font"] as PdfDictionary;
				if (text2.Contains("/"))
				{
					text2 = text2.Trim('/');
					text = ((PdfCrossTable.Dereference(pdfDictionary4[text2]) as PdfDictionary)["BaseFont"] as PdfName).Value;
				}
				size = float.Parse(operands[1], CultureInfo.InvariantCulture.NumberFormat);
			}
		}
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
		{
			switch (text)
			{
			case "Helvetica":
				m_font = new PdfStandardFont(PdfFontFamily.Helvetica, size);
				break;
			case "Courier":
				m_font = new PdfStandardFont(PdfFontFamily.Courier, size);
				break;
			case "Symbol":
				m_font = new PdfStandardFont(PdfFontFamily.Symbol, size);
				break;
			case "TimesRoman":
				m_font = new PdfStandardFont(PdfFontFamily.TimesRoman, size);
				break;
			case "Times-Roman":
				m_font = new PdfStandardFont(PdfFontFamily.TimesRoman, size);
				break;
			case "ZapfDingbats":
				m_font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, size);
				break;
			case "MonotypeSungLight":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeSungLight, size);
				break;
			case "SinoTypeSongLight":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.SinoTypeSongLight, size);
				break;
			case "MonotypeHeiMedium":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeHeiMedium, size);
				break;
			case "HanyangSystemsGothicMedium":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsGothicMedium, size);
				break;
			case "HanyangSystemsShinMyeongJoMedium":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium, size);
				break;
			case "HeiseiKakuGothicW5":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiKakuGothicW5, size);
				break;
			case "HeiseiMinchoW3":
				m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiMinchoW3, size);
				break;
			default:
				m_font = new PdfStandardFont(PdfFontFamily.Helvetica, size);
				break;
			}
		}
		else
		{
			PdfLoadedDocument pdfLoadedDocument = m_crossTable.Document as PdfLoadedDocument;
			if (pdfLoadedDocument.RaisePdfFont)
			{
				PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
				pdfFontEventArgs.m_fontName = "Arial";
				pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
				pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
				m_font = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, size, string.Empty, useTrueType: true, isEnableEmbedding: true);
			}
		}
	}
}
