using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedFreeTextAnnotation : PdfLoadedStyledAnnotation
{
	private PdfLineEndingStyle m_lineEndingStyle;

	private PdfFont m_font;

	private PdfColor m_textMarkupColor;

	private PdfAnnotationIntent m_annotationIntent;

	private PointF[] m_calloutLines = new PointF[0];

	private PdfColor m_borderColor;

	private PdfCrossTable m_crossTable;

	private string m_markUpText;

	private bool m_istextMarkupcolor;

	private PointF[] m_calloutsClone = new PointF[0];

	private bool m_isfontAPStream;

	private bool m_markupTextFromStream;

	private MemoryStream freeTextStream;

	private PdfRecordCollection readTextCollection;

	private ContentParser parser;

	private bool m_complexScript;

	private PdfTextAlignment alignment;

	private PdfTextDirection m_textDirection;

	private float m_lineSpacing;

	private bool isAllRotation = true;

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

	public PdfLineEndingStyle LineEndingStyle
	{
		get
		{
			return ObtainLineEndingStyle();
		}
		set
		{
			m_lineEndingStyle = value;
			base.Dictionary.SetProperty("LE", new PdfName(m_lineEndingStyle.ToString()));
			NotifyPropertyChanged("LineEndingStyle");
		}
	}

	public string MarkUpText
	{
		get
		{
			if (string.IsNullOrEmpty(m_markUpText))
			{
				return ObtainText();
			}
			return m_markUpText;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			if (m_markUpText != value)
			{
				m_markUpText = value;
				Text = value;
			}
			NotifyPropertyChanged("MarkUpText");
		}
	}

	internal bool MarkUpTextFromStream
	{
		get
		{
			return m_markupTextFromStream;
		}
		set
		{
			m_markupTextFromStream = value;
		}
	}

	public PdfAnnotationIntent AnnotationIntent
	{
		get
		{
			return ObtainAnnotationIntent();
		}
		set
		{
			m_annotationIntent = value;
			base.Dictionary.SetProperty("IT", new PdfName(m_annotationIntent.ToString()));
			NotifyPropertyChanged("AnnotationIntent");
		}
	}

	public PdfFont Font
	{
		get
		{
			if (m_font == null)
			{
				return ObtainFont();
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
			isContentUpdated = true;
			UpdateTextMarkupColor();
			NotifyPropertyChanged("Font");
		}
	}

	public PdfColor TextMarkupColor
	{
		get
		{
			m_textMarkupColor = GetTextMarkUpColor();
			UpdateTextMarkupColor();
			return m_textMarkupColor;
		}
		set
		{
			m_textMarkupColor = value;
			m_istextMarkupcolor = true;
			isContentUpdated = true;
			UpdateTextMarkupColor();
			NotifyPropertyChanged("TextMarkupColor");
		}
	}

	public PointF[] CalloutLines
	{
		get
		{
			return GetcalloutLinepoints();
		}
		set
		{
			m_calloutLines = value;
			if (m_calloutLines.Length >= 2)
			{
				PdfArray pdfArray = new PdfArray();
				for (int i = 0; i < m_calloutLines.Length; i++)
				{
					pdfArray.Add(new PdfNumber(m_calloutLines[i].X));
					pdfArray.Add(new PdfNumber(base.Page.Size.Height - m_calloutLines[i].Y));
				}
				base.Dictionary.SetProperty("CL", pdfArray);
				NotifyPropertyChanged("CalloutLines");
			}
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return ObtainColor();
		}
		set
		{
			if (m_borderColor != value)
			{
				m_borderColor = value;
				string value2 = $"{(float)(int)m_borderColor.R / 255f} {(float)(int)m_borderColor.G / 255f} {(float)(int)m_borderColor.B / 255f} rg ";
				base.Dictionary.SetProperty("DA", new PdfString(value2));
				NotifyPropertyChanged("BorderColor");
			}
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
			return ObtainTextAlignment();
		}
		set
		{
			if (alignment != value)
			{
				alignment = value;
				base.Dictionary.SetProperty("Q", new PdfNumber((int)alignment));
				NotifyPropertyChanged("TextAlignment");
			}
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

	internal PdfLoadedFreeTextAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
	}

	private PdfColor ObtainColor()
	{
		PdfColor result = default(PdfColor);
		if (base.Dictionary.ContainsKey("DA"))
		{
			if (base.Dictionary["DA"] is PdfArray)
			{
				if (PdfCrossTable.Dereference(base.Dictionary["DA"]) is PdfArray { Count: >0 } pdfArray && pdfArray[0] is PdfNumber && pdfArray[1] is PdfNumber && pdfArray[2] is PdfNumber)
				{
					float floatValue = (pdfArray[0] as PdfNumber).FloatValue;
					float floatValue2 = (pdfArray[1] as PdfNumber).FloatValue;
					float floatValue3 = (pdfArray[2] as PdfNumber).FloatValue;
					result = new PdfColor(floatValue, floatValue2, floatValue3);
				}
			}
			else if (base.Dictionary["DA"] is PdfString)
			{
				PdfString pdfString = base.Dictionary["DA"] as PdfString;
				string text = string.Empty;
				if (pdfString != null)
				{
					text = pdfString.Value.Trim();
				}
				if (text != null && text.IndexOf("ColorFound") == 0)
				{
					text = text.Remove(0, 10);
				}
				string[] array = text.Split(' ');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == "rg")
					{
						float red = float.Parse(array[i - 3].TrimStart('['), CultureInfo.InvariantCulture.NumberFormat);
						float green = float.Parse(array[i - 2], CultureInfo.InvariantCulture.NumberFormat);
						float blue = float.Parse(array[i - 1].TrimEnd(']'), CultureInfo.InvariantCulture.NumberFormat);
						result = new PdfColor(red, green, blue);
						break;
					}
					if (array[i] == "g")
					{
						string empty = string.Empty;
						empty = ((!array[i - 1].Contains("[") || !array[i - 1].Contains("]")) ? array[i - 1] : array[i - 1].Trim('[', ']'));
						float gray = float.Parse(empty, CultureInfo.InvariantCulture.NumberFormat);
						result = new PdfColor(gray);
						break;
					}
				}
			}
		}
		else if (base.Dictionary.ContainsKey("MK") && base.Dictionary["MK"] is PdfDictionary pdfDictionary && PdfCrossTable.Dereference(pdfDictionary["BC"]) is PdfArray pdfArray2 && pdfArray2.Elements.Count > 0)
		{
			PdfNumber pdfNumber = pdfArray2.Elements[0] as PdfNumber;
			PdfNumber pdfNumber2 = pdfArray2.Elements[1] as PdfNumber;
			PdfNumber pdfNumber3 = pdfArray2.Elements[2] as PdfNumber;
			result = new PdfColor(pdfNumber.FloatValue, pdfNumber2.FloatValue, pdfNumber3.FloatValue);
		}
		return result;
	}

	private PointF[] GetcalloutLinepoints()
	{
		if (base.Dictionary.ContainsKey("CL"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["CL"]) is PdfArray pdfArray)
			{
				m_calloutLines = new PointF[pdfArray.Count / 2];
				int num = 0;
				int num2 = 0;
				while (num2 < pdfArray.Count)
				{
					float floatValue = (pdfArray[num2] as PdfNumber).FloatValue;
					float y = base.Page.Size.Height - (pdfArray[num2 + 1] as PdfNumber).FloatValue;
					m_calloutLines[num] = new PointF(floatValue, y);
					num2 += 2;
					num++;
				}
			}
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten && base.Page != null && !base.Page.Annotations.Flatten)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					int num3 = 0;
					for (int i = 0; i < m_calloutLines.Length; i++)
					{
						float x = m_calloutLines[i].X;
						float y2 = m_calloutLines[i].Y;
						float floatValue2 = pdfNumber.FloatValue;
						float floatValue3 = pdfNumber2.FloatValue;
						x = 0f - floatValue2 + x + pdfMargins.Left;
						y2 = ((m_loadedPage == null || !m_loadedPage.Dictionary.ContainsKey("MediaBox") || m_loadedPage.Dictionary.ContainsKey("CropBox") || pdfNumber3.FloatValue != 0f || !(pdfNumber2.FloatValue > 0f)) ? (y2 + floatValue3 - pdfMargins.Top) : (y2 - pdfNumber3.FloatValue - pdfMargins.Top));
						m_calloutLines[num3] = new PointF(x, y2);
						num3++;
					}
				}
			}
		}
		return m_calloutLines;
	}

	private PdfColor GetTextMarkUpColor()
	{
		PdfColor textMarkupColor = default(PdfColor);
		string text = "";
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("TextColor"))
		{
			PdfColor pdfColor = new PdfColor(DocGen.Drawing.Color.Empty);
			if (PdfCrossTable.Dereference(base.Dictionary["TextColor"]) is PdfArray pdfArray2 && pdfArray2.Elements.Count > 0 && pdfArray2[0] is PdfNumber && pdfArray2[1] is PdfNumber && pdfArray2[2] is PdfNumber)
			{
				byte red = (byte)Math.Round((pdfArray2[0] as PdfNumber).FloatValue * 255f);
				byte green = (byte)Math.Round((pdfArray2[1] as PdfNumber).FloatValue * 255f);
				byte blue = (byte)Math.Round((pdfArray2[2] as PdfNumber).FloatValue * 255f);
				pdfColor = new PdfColor(red, green, blue);
				m_textMarkupColor = pdfColor;
				return m_textMarkupColor;
			}
		}
		else if (base.Dictionary["AP"] != null && !m_istextMarkupcolor)
		{
			m_isfontAPStream = false;
			ObtainFromAPStream(m_isfontAPStream);
		}
		bool flag = false;
		if (base.Dictionary.ContainsKey("DS") && m_textMarkupColor.IsEmpty)
		{
			string[] array = (base.Dictionary["DS"] as PdfString).Value.ToString().Split(new char[1] { ';' });
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Contains("color"))
				{
					text = array[i].Split(new char[1] { ':' })[1];
				}
			}
			flag = true;
			if (text != string.Empty)
			{
				m_textMarkupColor = FromHtml(text);
			}
			else
			{
				m_textMarkupColor = textMarkupColor;
			}
		}
		if (!m_istextMarkupcolor && base.Dictionary.ContainsKey("RC") && (m_textMarkupColor.IsEmpty || flag))
		{
			try
			{
				List<object> list = ParseXMLData();
				if (list != null && list.Count > 0)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j] is PdfSolidBrush)
						{
							PdfColor color = (list[j] as PdfSolidBrush).Color;
							m_textMarkupColor = new PdfColor(color);
						}
					}
					if (m_textMarkupColor.IsEmpty)
					{
						m_textMarkupColor = new PdfColor(DocGen.Drawing.Color.Black);
					}
				}
			}
			catch
			{
			}
		}
		return m_textMarkupColor;
	}

	private void UpdateTextMarkupColor()
	{
		Color c = DocGen.Drawing.Color.FromArgb(m_textMarkupColor.R, m_textMarkupColor.G, m_textMarkupColor.B);
		base.Dictionary.SetProperty("DS", new PdfString($"font:{Font.Name} {Font.Size}pt; color:{ColorTranslator.ToHtml(c)};style:{Font.Style}"));
	}

	private string ObtainText()
	{
		string text = null;
		bool flag = base.Dictionary.ContainsKey("Contents");
		if ((!MarkUpTextFromStream && flag) || (MarkUpTextFromStream && flag && !base.Dictionary.ContainsKey("AP")))
		{
			if (base.Dictionary["Contents"] is PdfString pdfString)
			{
				text = pdfString.Value.ToString();
			}
			if (!string.IsNullOrEmpty(text))
			{
				m_markUpText = text;
			}
			return text;
		}
		if (base.Dictionary.ContainsKey("RC") && !flag && text == null)
		{
			ParseXMLData();
			return m_rctext;
		}
		return string.Empty;
	}

	private PdfLineEndingStyle ObtainLineEndingStyle()
	{
		PdfLineEndingStyle result = PdfLineEndingStyle.Square;
		if (base.Dictionary.ContainsKey("LE"))
		{
			PdfName pdfName = null;
			if (base.Dictionary["LE"] is PdfArray)
			{
				if (base.Dictionary["LE"] is PdfArray { Count: >0 } pdfArray)
				{
					pdfName = pdfArray[0] as PdfName;
				}
			}
			else
			{
				pdfName = base.Dictionary["LE"] as PdfName;
			}
			if (pdfName == null)
			{
				return result;
			}
			switch (pdfName.Value.ToString())
			{
			case "Square":
				result = PdfLineEndingStyle.Square;
				break;
			case "Butt":
				result = PdfLineEndingStyle.Butt;
				break;
			case "Circle":
				result = PdfLineEndingStyle.Circle;
				break;
			case "ClosedArrow":
				result = PdfLineEndingStyle.ClosedArrow;
				break;
			case "Diamond":
				result = PdfLineEndingStyle.Diamond;
				break;
			case "None":
				result = PdfLineEndingStyle.None;
				break;
			case "OpenArrow":
				result = PdfLineEndingStyle.OpenArrow;
				break;
			case "RClosedArrow":
				result = PdfLineEndingStyle.RClosedArrow;
				break;
			case "ROpenArrow":
				result = PdfLineEndingStyle.ROpenArrow;
				break;
			case "Slash":
				result = PdfLineEndingStyle.Slash;
				break;
			}
		}
		return result;
	}

	private PdfAnnotationIntent ObtainAnnotationIntent()
	{
		PdfAnnotationIntent result = PdfAnnotationIntent.None;
		if (base.Dictionary.ContainsKey("IT"))
		{
			PdfName pdfName = base.Dictionary["IT"] as PdfName;
			if (pdfName != null)
			{
				switch (pdfName.Value.ToString())
				{
				case "FreeTextCallout":
					result = PdfAnnotationIntent.FreeTextCallout;
					break;
				case "FreeTextTypeWriter":
					result = PdfAnnotationIntent.FreeTextTypeWriter;
					break;
				case "None":
					result = PdfAnnotationIntent.None;
					break;
				}
			}
		}
		return result;
	}

	private PdfFont ObtainFont()
	{
		PdfString pdfString = null;
		string text = "";
		float result = 1f;
		string text2 = "";
		if (base.Dictionary.ContainsKey("DS") || base.Dictionary.ContainsKey("DA"))
		{
			if (base.Dictionary.ContainsKey("DS"))
			{
				pdfString = base.Dictionary["DS"] as PdfString;
				string[] array = pdfString.Value.ToString().Split(new char[1] { ';' });
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(new char[1] { ':' });
					if (array[i].Contains("font-family"))
					{
						text = (array2[1].Contains("font-family") ? array2[i + 1] : array2[1]);
					}
					else if (array[i].Contains("font-size"))
					{
						if (array2[1].EndsWith("pt"))
						{
							string text3 = array2[1].Replace("pt", "");
							if (text3.Contains(","))
							{
								text3 = text3.Replace(",", ".");
							}
							result = float.Parse(text3, CultureInfo.InvariantCulture.NumberFormat);
						}
					}
					else if (array[i].Contains("font-style"))
					{
						text2 = array[1];
					}
					else if (array[i].Contains("font"))
					{
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
								string text4 = array3[j].Replace("pt", "");
								if (text4.Contains(","))
								{
									text4 = text4.Replace(",", ".");
								}
								result = float.Parse(text4, CultureInfo.InvariantCulture.NumberFormat);
							}
						}
						text = text.TrimEnd(' ');
						if (text.Contains("'"))
						{
							text = text.TrimEnd('\'');
							text = text.TrimStart('\'');
						}
						if (text.Contains(","))
						{
							text = text.Split(new char[1] { ',' })[0];
						}
					}
					else if (array[i].Contains("style"))
					{
						text2 = array2[1];
					}
				}
			}
			else if (base.Dictionary.ContainsKey("DA"))
			{
				pdfString = base.Dictionary["DA"] as PdfString;
				string text5 = string.Empty;
				if (pdfString != null)
				{
					text5 = pdfString.Value;
				}
				if (text5 != string.Empty && text5.Contains("Tf"))
				{
					string[] array4 = text5.ToString().Split(new char[1] { ' ' });
					for (int k = 0; k < array4.Length; k++)
					{
						string obj2 = array4[k];
						if (obj2.Contains("Tf"))
						{
							text = array4[k - 2].TrimStart('/');
							float.TryParse(array4[k - 1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out result);
						}
						if (obj2.Contains("rg"))
						{
							float red = float.Parse(array4[k - 3].TrimStart('['), CultureInfo.InvariantCulture.NumberFormat);
							float green = float.Parse(array4[k - 2], CultureInfo.InvariantCulture.NumberFormat);
							float blue = float.Parse(array4[k - 1].TrimEnd(']'), CultureInfo.InvariantCulture.NumberFormat);
							PdfColor textMarkupColor = new PdfColor(red, green, blue);
							m_textMarkupColor = textMarkupColor;
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
			PdfFontStyle pdfFontStyle = PdfFontStyle.Regular;
			string[] array5 = text2.Split(',');
			for (int l = 0; l < array5.Length; l++)
			{
				switch (array5[l].Trim())
				{
				case "Bold":
					pdfFontStyle |= PdfFontStyle.Bold;
					break;
				case "Italic":
					pdfFontStyle |= PdfFontStyle.Italic;
					break;
				case "Regular":
					pdfFontStyle |= PdfFontStyle.Regular;
					break;
				case "Strikeout":
					pdfFontStyle |= PdfFontStyle.Strikeout;
					break;
				case "Underline":
					pdfFontStyle |= PdfFontStyle.Underline;
					break;
				}
			}
			if (result == 0f && base.Dictionary.ContainsKey("RC"))
			{
				List<object> list = ParseXMLData();
				if (list != null)
				{
					for (int m = 0; m < list.Count; m++)
					{
						if (list[m] is PdfFont)
						{
							m_font = list[m] as PdfFont;
							return m_font;
						}
					}
				}
			}
			text = text.Trim();
			if (text.Contains(","))
			{
				text = text.Split(',')[0];
			}
			if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
			{
				switch (text)
				{
				case "Helvetica":
					m_font = new PdfStandardFont(PdfFontFamily.Helvetica, result, pdfFontStyle);
					break;
				case "Courier":
					m_font = new PdfStandardFont(PdfFontFamily.Courier, result, pdfFontStyle);
					break;
				case "Symbol":
					m_font = new PdfStandardFont(PdfFontFamily.Symbol, result, pdfFontStyle);
					break;
				case "TimesRoman":
				case "Times":
					m_font = new PdfStandardFont(PdfFontFamily.TimesRoman, result, pdfFontStyle);
					break;
				case "ZapfDingbats":
					m_font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, result, pdfFontStyle);
					break;
				case "MonotypeSungLight":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeSungLight, result, pdfFontStyle);
					break;
				case "SinoTypeSongLight":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.SinoTypeSongLight, result, pdfFontStyle);
					break;
				case "MonotypeHeiMedium":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeHeiMedium, result, pdfFontStyle);
					break;
				case "HanyangSystemsGothicMedium":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsGothicMedium, result, pdfFontStyle);
					break;
				case "HanyangSystemsShinMyeongJoMedium":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium, result, pdfFontStyle);
					break;
				case "HeiseiKakuGothicW5":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiKakuGothicW5, result, pdfFontStyle);
					break;
				case "HeiseiMinchoW3":
					m_font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiMinchoW3, result, pdfFontStyle);
					break;
				default:
				{
					PdfLoadedDocument pdfLoadedDocument = m_crossTable.Document as PdfLoadedDocument;
					if (pdfLoadedDocument.RaisePdfFont)
					{
						PdfFont fontFromEvent = GetFontFromEvent(text, result, pdfFontStyle, pdfLoadedDocument);
						if (fontFromEvent != null)
						{
							m_font = fontFromEvent;
						}
					}
					else
					{
						m_font = new PdfStandardFont(PdfFontFamily.Helvetica, result, pdfFontStyle);
					}
					break;
				}
				}
			}
			else
			{
				PdfLoadedDocument pdfLoadedDocument2 = m_crossTable.Document as PdfLoadedDocument;
				if (pdfLoadedDocument2.RaisePdfFont)
				{
					PdfFont fontFromEvent2 = GetFontFromEvent("Arial", result, PdfFontStyle.Regular, pdfLoadedDocument2);
					if (fontFromEvent2 != null)
					{
						m_font = fontFromEvent2;
					}
				}
			}
		}
		else if (base.Dictionary["AP"] != null)
		{
			m_isfontAPStream = true;
			ObtainFromAPStream(m_isfontAPStream);
		}
		if (m_font == null)
		{
			if (result < 2f)
			{
				result = 12f;
			}
			m_font = new PdfStandardFont(PdfFontFamily.Helvetica, result);
		}
		return m_font;
	}

	private PdfFont GetFontFromEvent(string fontName, float fontSize, PdfFontStyle fontStyle, PdfLoadedDocument ldoc)
	{
		PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
		pdfFontEventArgs.m_fontName = fontName;
		pdfFontEventArgs.m_fontStyle = fontStyle;
		ldoc.PdfFontStream(pdfFontEventArgs);
		if (pdfFontEventArgs.FontStream != null)
		{
			return new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, fontSize, string.Empty, useTrueType: true, isEnableEmbedding: true);
		}
		return null;
	}

	private string ObtainTextFromAPStream(bool isfontStream)
	{
		string text = "";
		string text2 = "";
		IPdfPrimitive pdfPrimitive = base.Dictionary["AP"];
		if (pdfPrimitive != null && isfontStream && PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary pdfDictionary)
		{
			pdfPrimitive = pdfDictionary["N"];
			PdfDictionary pdfDictionary2 = null;
			if (pdfPrimitive != null)
			{
				pdfDictionary2 = PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary;
			}
			string key = null;
			if (pdfDictionary2 != null && pdfDictionary2 is PdfStream pdfStream)
			{
				parser = new ContentParser(pdfStream.GetDecompressedData());
				readTextCollection = parser.ReadContent();
				if (readTextCollection != null)
				{
					for (int i = 0; i < readTextCollection.RecordCollection.Count; i++)
					{
						string operatorName = readTextCollection.RecordCollection[i].OperatorName;
						if (operatorName.Contains("Tf"))
						{
							PdfRecord pdfRecord = readTextCollection.RecordCollection[i];
							_ = pdfRecord.OperatorName;
							key = pdfRecord.Operands[0].Replace("/", "");
						}
						if (operatorName.Contains("Tj") || operatorName.Contains("TJ"))
						{
							text = readTextCollection.RecordCollection[i].Operands[0];
							if ((text.Contains("(") || text.Contains(")")) && pdfDictionary2.ContainsKey("Resources"))
							{
								PdfDictionary pdfDictionary3 = pdfDictionary2["Resources"] as PdfDictionary;
								if (pdfDictionary3 == null)
								{
									PdfReferenceHolder pdfReferenceHolder = pdfDictionary2["Resources"] as PdfReferenceHolder;
									if (pdfReferenceHolder != null)
									{
										pdfDictionary3 = pdfReferenceHolder.Object as PdfDictionary;
									}
								}
								if (pdfDictionary3 != null && pdfDictionary3["Font"] is PdfDictionary pdfDictionary4)
								{
									PdfDictionary pdfDictionary5 = pdfDictionary4[key] as PdfDictionary;
									if (pdfDictionary5 == null)
									{
										PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary4[key] as PdfReferenceHolder;
										if (pdfReferenceHolder2 != null)
										{
											pdfDictionary5 = pdfReferenceHolder2.Object as PdfDictionary;
										}
									}
									if (pdfDictionary5 != null && pdfDictionary4.ContainsKey(key))
									{
										FontStructure fontStructure = new FontStructure(pdfDictionary5);
										if (fontStructure != null)
										{
											fontStructure.IsTextExtraction = true;
											string text3 = fontStructure.DecodeTextExtraction(text, isSameFont: true);
											text2 += text3;
										}
									}
								}
							}
						}
						if (operatorName.Contains("ET"))
						{
							text2 += "\r\n";
						}
					}
				}
			}
		}
		if (text2.Contains("\r\n"))
		{
			text2 = text2.Substring(0, text2.Length - 2);
		}
		return text2;
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
		if (readTextCollection == null)
		{
			return;
		}
		if (isfontStream)
		{
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
					PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary3["Font"]) as PdfDictionary;
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
		if (isfontStream)
		{
			return;
		}
		int textOperatorIndex = GetTextOperatorIndex(readTextCollection);
		if (textOperatorIndex <= -1)
		{
			return;
		}
		for (int num = textOperatorIndex; num > -1; num--)
		{
			PdfRecord pdfRecord = readTextCollection.RecordCollection[num];
			string empty = string.Empty;
			switch (pdfRecord.OperatorName)
			{
			case "rg":
			{
				string[] operands4 = pdfRecord.Operands;
				float red = float.Parse(operands4[0], CultureInfo.InvariantCulture.NumberFormat);
				float green = float.Parse(operands4[1], CultureInfo.InvariantCulture.NumberFormat);
				float blue = float.Parse(operands4[2], CultureInfo.InvariantCulture.NumberFormat);
				m_textMarkupColor = new PdfColor(red, green, blue);
				return;
			}
			case "k":
			{
				string[] operands3 = pdfRecord.Operands;
				float cyan = float.Parse(operands3[0], CultureInfo.InvariantCulture.NumberFormat);
				float magenta = float.Parse(operands3[1], CultureInfo.InvariantCulture.NumberFormat);
				float yellow = float.Parse(operands3[2], CultureInfo.InvariantCulture.NumberFormat);
				float black = float.Parse(operands3[3], CultureInfo.InvariantCulture.NumberFormat);
				m_textMarkupColor = new PdfColor(cyan, magenta, yellow, black);
				return;
			}
			case "g":
			{
				string[] operands2 = pdfRecord.Operands;
				m_textMarkupColor = new PdfColor(float.Parse(operands2[0], CultureInfo.InvariantCulture.NumberFormat));
				return;
			}
			case "re":
				return;
			}
		}
	}

	private int GetTextOperatorIndex(PdfRecordCollection records)
	{
		int result = -1;
		int count = records.RecordCollection.Count;
		for (int i = 0; i < count; i++)
		{
			PdfRecord pdfRecord = records.RecordCollection[i];
			string empty = string.Empty;
			switch (pdfRecord.OperatorName)
			{
			case "Tj":
			case "'":
			case "\"":
			case "TJ":
				break;
			default:
				continue;
			}
			result = i;
			break;
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
		if (base.Dictionary.ContainsKey("RC") && isContentUpdated)
		{
			string empty = string.Empty;
			_ = TextMarkupColor;
			empty = ColorTranslator.ToHtml(DocGen.Drawing.Color.FromArgb(m_textMarkupColor.R, m_textMarkupColor.G, m_textMarkupColor.B));
			string empty2 = string.Empty;
			string empty3 = string.Empty;
			string text = (Font.Bold ? "bold" : "normal");
			empty3 = " style = \"" + $"font:{Font.Name} {Font.Size}pt;font-weight:{text};color:{empty}" + "\"";
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
			empty2 = ((!(text2 != string.Empty)) ? GetXmlFormattedString(MarkUpText) : ("<span style = \"" + text2 + "\">" + GetXmlFormattedString(MarkUpText) + "</span>"));
			base.Dictionary.SetString("RC", "<?xml version=\"1.0\"?><body xmlns=\"http://www.w3.org/1999/xhtml\"" + empty3 + "><p dir=\"ltr\">" + empty2 + "</p></body>");
		}
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
			if (PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Subtype"))
			{
				if (!(pdfDictionary2 is PdfStream template))
				{
					return;
				}
				appearance = new PdfTemplate(template);
				if (appearance != null)
				{
					bool flag = ValidateTemplateMatrix(pdfDictionary2);
					if (flag && base.Page.Rotation != 0)
					{
						FlattenAnnotationTemplate(appearance, flag);
					}
					else if (IsValidTemplateMatrix(pdfDictionary2, Bounds.Location, appearance))
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

	private PdfTemplate CreateAppearance()
	{
		if (base.SetAppearanceDictionary)
		{
			float borderWidth = Border.Width / 2f;
			RectangleF rectangleF = ObtainAppearanceBounds();
			PdfTemplate pdfTemplate = null;
			if (base.RotateAngle == 90f || base.RotateAngle == 180f || base.RotateAngle == 270f || base.RotateAngle == 0f)
			{
				isAllRotation = false;
			}
			pdfTemplate = ((!(base.RotateAngle > 0f) || !isAllRotation) ? new PdfTemplate(rectangleF) : new PdfTemplate(rectangleF.Size));
			SetMatrix(pdfTemplate.m_content);
			PaintParams paintParams = new PaintParams();
			string text = Text;
			pdfTemplate.m_writeTransformation = false;
			PdfGraphics graphics = pdfTemplate.Graphics;
			alignment = TextAlignment;
			PdfPen pdfPen = new PdfPen(BorderColor, Border.Width);
			if (Border.Width > 0f)
			{
				paintParams.BorderPen = pdfPen;
			}
			RectangleF rectangleF2 = ObtainStyle(pdfPen, rectangleF, borderWidth, paintParams);
			if (base.Dictionary.ContainsKey("C"))
			{
				paintParams.ForeBrush = new PdfSolidBrush(Color);
			}
			paintParams.BackBrush = new PdfSolidBrush(TextMarkupColor);
			paintParams.BorderWidth = Border.Width;
			paintParams.m_complexScript = ComplexScript;
			paintParams.m_textDirection = TextDirection;
			paintParams.m_lineSpacing = LineSpacing;
			if (CalloutLines.Length >= 2)
			{
				DrawCallOuts(graphics, pdfPen);
				if (LineEndingStyle != 0)
				{
					float[] array = ObtainLinePoints();
					float num = array[0];
					float num2 = array[1];
					float num3 = array[2];
					float num4 = array[3];
					double num5 = 0.0;
					num5 = ((num3 - num != 0f) ? GetAngle(num, num2, num3, num4) : ((!(num4 > num2)) ? 270.0 : 90.0));
					int num6 = 0;
					double num7 = 0.0;
					float[] value = new float[2] { num, num2 };
					float[] value2 = new float[2] { num3, num4 };
					float[] axisValue = GetAxisValue(value, num7 + 90.0, num6);
					float[] axisValue2 = GetAxisValue(value2, num7 + 90.0, num6);
					PdfArray pdfArray = new PdfArray();
					pdfArray.Insert(0, null);
					pdfArray.Insert(1, new PdfName(LineEndingStyle));
					double borderLength = Border.Width;
					SetLineEndingStyles(axisValue, axisValue2, graphics, num5, pdfPen, paintParams.ForeBrush, pdfArray, borderLength);
				}
				if (!base.Dictionary.ContainsKey("RD"))
				{
					rectangleF2 = new RectangleF(Bounds.X, 0f - (base.Page.Size.Height - (Bounds.Y + Bounds.Height)), Bounds.Width, 0f - Bounds.Height);
					SetRectangleDifferance(rectangleF2);
				}
				else
				{
					rectangleF2 = new RectangleF(rectangleF2.X, 0f - rectangleF2.Y, rectangleF2.Width, 0f - rectangleF2.Height);
				}
				paintParams.Bounds = rectangleF2;
			}
			else
			{
				rectangleF2 = (paintParams.Bounds = new RectangleF(rectangleF2.X, 0f - rectangleF2.Y, rectangleF2.Width, 0f - rectangleF2.Height));
				if (base.Dictionary.ContainsKey("RD"))
				{
					SetRectangleDifferance(rectangleF2);
				}
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
			DrawFreeTextRectangle(graphics, paintParams, rectangleF2);
			DrawFreeMarkUpText(graphics, paintParams, rectangleF2, text);
			if (base.Rotate != 0)
			{
				graphics.Restore();
			}
			if (Opacity < 1f)
			{
				graphics.Restore();
			}
			base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(ObtainAppearanceBounds()));
			return pdfTemplate;
		}
		return null;
	}

	private void DrawArrow(PaintParams paintParams, PdfGraphics graphics, PdfPen mBorderPen)
	{
		if (paintParams.BorderPen != null)
		{
			paintParams.BorderPen.LineJoin = PdfLineJoin.Miter;
		}
		if (paintParams.BorderPen != null)
		{
			PointF[] array = CalculateArrowPoints(m_calloutsClone[1], m_calloutsClone[0]);
			PointF[] array2 = new PointF[3];
			byte[] array3 = new byte[3];
			array2[0] = new PointF(array[0].X, 0f - array[0].Y);
			array2[1] = new PointF(m_calloutsClone[0].X, 0f - m_calloutsClone[0].Y);
			array2[2] = new PointF(array[1].X, 0f - array[1].Y);
			array3[0] = 0;
			array3[1] = 1;
			array3[2] = 1;
			PdfPath path = new PdfPath(array2, array3);
			graphics.DrawPath(mBorderPen, path);
		}
	}

	private void DrawAppearance(RectangleF rectangle, PdfGraphics graphics, PaintParams paintParams)
	{
		PdfPath pdfPath = new PdfPath();
		pdfPath.AddRectangle(rectangle);
		if (base.Dictionary.ContainsKey("BE") && base.Dictionary[new PdfName("BE")] is PdfDictionary pdfDictionary && pdfDictionary.Items.Count != 0 && pdfDictionary.ContainsKey("I"))
		{
			float radius = (((pdfDictionary.Items[new PdfName("I")] as PdfNumber).FloatValue == 1f) ? 4 : 9);
			DrawCloudStyle(graphics, paintParams.ForeBrush, paintParams.BorderPen, radius, 0.833f, pdfPath.PathPoints, isAppearance: true);
		}
	}

	private RectangleF ObtainStyle(PdfPen mBorderPen, RectangleF rectangle, float borderWidth, PaintParams paintParams)
	{
		if (base.Dictionary.ContainsKey("BS"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["BS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary.Items[new PdfName("D")]) is PdfArray pdfArray)
			{
				float[] array = new float[pdfArray.Count];
				for (int i = 0; i < pdfArray.Count; i++)
				{
					array[i] = (pdfArray.Elements[i] as PdfNumber).FloatValue;
				}
				mBorderPen.DashStyle = PdfDashStyle.Dash;
				mBorderPen.DashPattern = array;
			}
			if (Border.Width > 0f)
			{
				paintParams.BorderPen = mBorderPen;
			}
		}
		if (!isBounds && base.Dictionary["RD"] != null)
		{
			if (PdfCrossTable.Dereference(base.Dictionary["RD"]) is PdfArray pdfArray2)
			{
				PdfNumber pdfNumber = pdfArray2.Elements[0] as PdfNumber;
				PdfNumber pdfNumber2 = pdfArray2.Elements[1] as PdfNumber;
				PdfNumber pdfNumber3 = pdfArray2.Elements[2] as PdfNumber;
				PdfNumber pdfNumber4 = pdfArray2.Elements[3] as PdfNumber;
				rectangle.X += pdfNumber.FloatValue;
				rectangle.Y = rectangle.Y + borderWidth + pdfNumber2.FloatValue;
				rectangle.Width -= pdfNumber.FloatValue + pdfNumber3.FloatValue;
				rectangle.Height -= pdfNumber2.FloatValue + pdfNumber4.FloatValue;
				paintParams.Bounds = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			}
		}
		else
		{
			rectangle.X += Border.Width / 2f;
			rectangle.Y += Border.Width / 2f;
			rectangle.Width -= Border.Width;
			rectangle.Height -= Border.Width;
			paintParams.Bounds = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}
		return rectangle;
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

	private float[] ObtainLinePoints()
	{
		float num = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
		PointF pointF = new PointF(CalloutLines[1].X, num - CalloutLines[1].Y);
		PointF pointF2 = new PointF(CalloutLines[0].X, num - CalloutLines[0].Y);
		return new float[4] { pointF.X, pointF.Y, pointF2.X, pointF2.Y };
	}

	private RectangleF ObtainAppearanceBounds()
	{
		RectangleF empty = RectangleF.Empty;
		if (CalloutLines != null && CalloutLines.Length != 0)
		{
			PdfPath pdfPath = new PdfPath();
			PointF[] array = null;
			array = ((CalloutLines.Length != 2) ? new PointF[3] : new PointF[CalloutLines.Length]);
			if (CalloutLines.Length >= 2)
			{
				ObtainCallOutsNative();
				for (int i = 0; i < CalloutLines.Length && i < 3; i++)
				{
					array[i] = new PointF(m_calloutsClone[i].X, m_calloutsClone[i].Y);
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
			pdfPath.AddRectangle(new RectangleF(Bounds.X - 2f, base.Page.Size.Height - (Bounds.Y + Bounds.Height) - 2f, Bounds.Width + 4f, Bounds.Height + 4f));
			empty = pdfPath.GetBounds();
			return pdfPath.GetBounds();
		}
		return GetBounds(base.Dictionary, base.CrossTable);
	}

	private void ExpandAppearanceForEndLineStyle(PointF[] pointArray)
	{
		if (base.Page == null)
		{
			_ = base.LoadedPage.Size.Height;
		}
		else
		{
			_ = base.Page.Size.Height;
		}
		float y = pointArray[0].Y;
		float x = pointArray[0].X;
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
		if (x <= Bounds.X)
		{
			pointArray[0].X -= Border.Width * 11f;
		}
		else
		{
			pointArray[0].X += Border.Width * 11f;
		}
	}

	private void DrawFreeTextRectangle(PdfGraphics graphics, PaintParams paintParams, RectangleF rect)
	{
		bool isRotation = false;
		if (base.Dictionary.ContainsKey("BE"))
		{
			float[] array = new float[4] { rect.X, rect.Y, rect.Width, rect.Height };
			for (int i = 0; i < 4; i++)
			{
				if (array[i] < 0f)
				{
					array[i] = 0f - array[i];
				}
			}
			rect = new RectangleF(array[0], array[1], array[2], array[3]);
			DrawAppearance(rect, graphics, paintParams);
			if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle90 && !isAllRotation)
			{
				graphics.RotateTransform(-90f);
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle180 && !isAllRotation)
			{
				graphics.RotateTransform(-180f);
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270 && !isAllRotation)
			{
				graphics.RotateTransform(-270f);
			}
		}
		else
		{
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
			FieldPainter.DrawFreeTextAnnotation(graphics, paintParams, "", Font, paintParams.Bounds, isSkipDrawRectangle: false, alignment, isRotation);
		}
	}

	private void DrawFreeMarkUpText(PdfGraphics graphics, PaintParams paintParams, RectangleF rect, string text)
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
		if (paintParams.BorderWidth > 0f)
		{
			rect = new RectangleF(rect.X + paintParams.BorderWidth * 1.5f, rect.Y + paintParams.BorderWidth * 1.5f, rect.Width - paintParams.BorderWidth * 3f, (rect.Height > 0f) ? (rect.Height - paintParams.BorderWidth * 3f) : (0f - rect.Height - paintParams.BorderWidth * 3f));
		}
		bool flag = Font.Height > ((rect.Height > 0f) ? rect.Height : (0f - rect.Height)) && Font.Height <= ((rectangleF.Height > 0f) ? rectangleF.Height : (0f - rectangleF.Height));
		FieldPainter.DrawFreeTextAnnotation(graphics, paintParams, text, Font, flag ? rectangleF : rect, isSkipDrawRectangle: true, alignment, isRotation);
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

	private void DrawCallOuts(PdfGraphics graphics, PdfPen mBorderPen)
	{
		PdfPath pdfPath = new PdfPath();
		PointF[] array = null;
		array = ((CalloutLines.Length != 2) ? new PointF[3] : new PointF[CalloutLines.Length]);
		for (int i = 0; i < CalloutLines.Length && i < 3; i++)
		{
			array[i] = new PointF(m_calloutsClone[i].X, 0f - m_calloutsClone[i].Y);
		}
		if (array.Length != 0)
		{
			pdfPath.AddLines(array);
		}
		graphics.DrawPath(mBorderPen, pdfPath);
	}

	private void ObtainCallOutsNative()
	{
		if (CalloutLines.Length != 0)
		{
			m_calloutsClone = new PointF[CalloutLines.Length];
			for (int i = 0; i < CalloutLines.Length; i++)
			{
				float x = CalloutLines[i].X;
				float y = base.Page.Size.Height - CalloutLines[i].Y;
				m_calloutsClone[i] = new PointF(x, y);
			}
		}
	}

	private PdfTextAlignment ObtainTextAlignment()
	{
		bool flag = false;
		PdfTextAlignment result = PdfTextAlignment.Left;
		if (base.Dictionary.ContainsKey("Q"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Q"]) is PdfNumber pdfNumber)
			{
				result = (PdfTextAlignment)Enum.ToObject(typeof(PdfTextAlignment), pdfNumber.IntValue);
				flag = true;
			}
		}
		else if (base.Dictionary.ContainsKey("RC"))
		{
			List<object> list = null;
			list = ParseXMLData();
			if (list != null)
			{
				result = (PdfTextAlignment)list[1];
				flag = true;
			}
		}
		if (base.Dictionary.ContainsKey("DS") && !flag)
		{
			string[] array = (base.Dictionary["DS"] as PdfString).Value.ToString().Split(new char[1] { ';' });
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Contains("text-align"))
				{
					switch (array[i].Split(new char[1] { ':' })[1])
					{
					case "left":
						result = PdfTextAlignment.Left;
						break;
					case "right":
						result = PdfTextAlignment.Right;
						break;
					case "center":
						result = PdfTextAlignment.Center;
						break;
					case "justify":
						result = PdfTextAlignment.Justify;
						break;
					}
				}
			}
		}
		return result;
	}
}
