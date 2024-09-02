using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedWidgetAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfAnnotationFlags m_flags;

	private PdfExtendedAppearance m_extendedAppearance;

	private WidgetBorder m_border = new WidgetBorder();

	private WidgetAppearance m_widgetAppearance = new WidgetAppearance();

	private PdfHighlightMode m_highlightMode = PdfHighlightMode.Invert;

	private PdfDefaultAppearance m_defaultAppearance;

	private PdfAnnotationActions m_actions;

	private new PdfAppearance m_appearance;

	private PdfTextAlignment m_alignment;

	private string m_appearanceState;

	internal PdfFont m_font;

	private PdfRecordCollection readTextCollection;

	private ContentParser parser;

	private bool m_isfontAPStream;

	private MemoryStream freeTextStream;

	public PdfExtendedAppearance ExtendedAppearance
	{
		get
		{
			if (m_extendedAppearance == null)
			{
				m_extendedAppearance = new PdfExtendedAppearance();
			}
			return m_extendedAppearance;
		}
		set
		{
			m_extendedAppearance = value;
			if (m_extendedAppearance != null)
			{
				base.Dictionary.SetProperty("AP", m_extendedAppearance);
				base.Dictionary.SetProperty("MK", (IPdfPrimitive)null);
			}
			else
			{
				if (m_appearance != null && m_appearance.GetNormalTemplate() != null)
				{
					base.Dictionary.SetProperty("AP", m_appearance);
				}
				else
				{
					base.Dictionary.SetProperty("AP", (IPdfPrimitive)null);
				}
				base.Dictionary.SetProperty("MK", m_widgetAppearance);
				base.Dictionary.SetProperty("AS", (IPdfPrimitive)null);
			}
			NotifyPropertyChanged("ExtendedAppearance");
		}
	}

	public PdfHighlightMode HighlightMode
	{
		get
		{
			return m_highlightMode;
		}
		set
		{
			base.Dictionary.SetName("H", HighlightModeToString(m_highlightMode));
			base.Dictionary.Modify();
			NotifyPropertyChanged("HighlightMode");
		}
	}

	public PdfTextAlignment TextAlignment
	{
		get
		{
			if (base.Dictionary.ContainsKey("Q") && PdfCrossTable.Dereference(base.Dictionary["Q"]) is PdfNumber pdfNumber)
			{
				m_alignment = (PdfTextAlignment)Enum.ToObject(typeof(PdfTextAlignment), pdfNumber.IntValue);
			}
			return m_alignment;
		}
		set
		{
			if (m_alignment != value)
			{
				m_alignment = value;
				base.Dictionary.SetProperty("Q", new PdfNumber((int)m_alignment));
				base.Dictionary.Modify();
			}
			NotifyPropertyChanged("TextAlignment");
		}
	}

	public PdfAnnotationActions Actions
	{
		get
		{
			if (m_actions == null)
			{
				m_actions = new PdfAnnotationActions();
				base.Dictionary.Remove("AA");
				base.Dictionary.SetProperty("AA", m_actions);
				base.Dictionary.Modify();
			}
			return m_actions;
		}
	}

	public new PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
			}
			return m_appearance;
		}
		set
		{
			if (m_appearance != value)
			{
				m_appearance = value;
			}
			NotifyPropertyChanged("Appearance");
		}
	}

	internal PdfFont Font
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
			if (m_font != value)
			{
				m_font = value;
				GetWidgetAnnotation(base.Dictionary, base.CrossTable)["DA"] = new PdfString(new PdfDefaultAppearance
				{
					FontName = m_font.Name.Replace(" ", string.Empty),
					FontSize = m_font.Size,
					ForeColor = ForeColor
				}.ToString());
			}
			NotifyPropertyChanged("Font");
		}
	}

	internal PdfColor ForeColor
	{
		get
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfColor result = new PdfColor(0, 0, 0);
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfString pdfString = base.CrossTable.GetObject(widgetAnnotation["DA"]) as PdfString;
				return GetForeColour(pdfString.Value);
			}
			if (widgetAnnotation != null && widgetAnnotation.GetValue(base.CrossTable, "DA", "Parent") is PdfString pdfString2)
			{
				return GetForeColour(pdfString2.Value);
			}
			return result;
		}
		set
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			float height = 0f;
			string text = null;
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfString pdfString = widgetAnnotation["DA"] as PdfString;
				text = FontName(pdfString.Value, out height);
			}
			if (!string.IsNullOrEmpty(text))
			{
				PdfDefaultAppearance pdfDefaultAppearance = new PdfDefaultAppearance();
				pdfDefaultAppearance.FontName = text;
				pdfDefaultAppearance.FontSize = height;
				pdfDefaultAppearance.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance.ToString());
			}
			else
			{
				PdfDefaultAppearance pdfDefaultAppearance2 = new PdfDefaultAppearance();
				pdfDefaultAppearance2.FontName = Font.Name;
				pdfDefaultAppearance2.FontSize = Font.Size;
				pdfDefaultAppearance2.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance2.ToString());
			}
			NotifyPropertyChanged("ForeColor");
		}
	}

	internal string AppearanceState
	{
		get
		{
			return m_appearanceState;
		}
		set
		{
			if (m_appearanceState != value)
			{
				m_appearanceState = value;
				base.Dictionary.SetName("AS", value);
			}
			NotifyPropertyChanged("AppearanceState");
		}
	}

	internal PdfLoadedWidgetAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle)
		: base(dictionary, crossTable)
	{
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
	}

	private string HighlightModeToString(PdfHighlightMode m_highlightingMode)
	{
		return m_highlightingMode switch
		{
			PdfHighlightMode.NoHighlighting => "N", 
			PdfHighlightMode.Outline => "O", 
			PdfHighlightMode.Push => "P", 
			_ => "I", 
		};
	}

	internal PdfColor GetForeColour(string defaultAppearance)
	{
		PdfColor result = new PdfColor(0, 0, 0);
		if (defaultAppearance == null || defaultAppearance == string.Empty)
		{
			result = new PdfColor(0, 0, 0);
		}
		else
		{
			PdfReader pdfReader = new PdfReader(new MemoryStream(Encoding.UTF8.GetBytes(defaultAppearance)));
			pdfReader.Position = 0L;
			bool flag = false;
			Stack<string> stack = new Stack<string>();
			string nextToken = pdfReader.GetNextToken();
			if (nextToken == "/")
			{
				flag = true;
			}
			while (nextToken != null && nextToken != string.Empty)
			{
				if (flag)
				{
					nextToken = pdfReader.GetNextToken();
				}
				flag = true;
				switch (nextToken)
				{
				case "g":
				{
					string text3 = stack.Pop();
					float gray = ParseFloatColour(text3);
					result = new PdfColor(gray);
					break;
				}
				case "rg":
				{
					string text2 = stack.Pop();
					byte blue = (byte)(ParseFloatColour(text2) * 255f);
					text2 = stack.Pop();
					byte green = (byte)(ParseFloatColour(text2) * 255f);
					text2 = stack.Pop();
					byte red = (byte)(ParseFloatColour(text2) * 255f);
					result = new PdfColor(red, green, blue);
					break;
				}
				case "k":
				{
					string text = stack.Pop();
					float black = ParseFloatColour(text);
					text = stack.Pop();
					float yellow = ParseFloatColour(text);
					text = stack.Pop();
					float magenta = ParseFloatColour(text);
					text = stack.Pop();
					float cyan = ParseFloatColour(text);
					result = new PdfColor(cyan, magenta, yellow, black);
					break;
				}
				default:
					stack.Push(nextToken);
					break;
				}
			}
		}
		return result;
	}

	private float ParseFloatColour(string text)
	{
		return (float)double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
	}

	internal string FontName(string fontString, out float height)
	{
		if (fontString.Contains("#2C"))
		{
			StringBuilder stringBuilder = new StringBuilder(fontString);
			stringBuilder.Replace("#2C", ",");
			fontString = stringBuilder.ToString();
		}
		PdfReader pdfReader = new PdfReader(new MemoryStream(Encoding.UTF8.GetBytes(fontString)));
		pdfReader.Position = 0L;
		string text = pdfReader.GetNextToken();
		string nextToken = pdfReader.GetNextToken();
		string result = null;
		height = 0f;
		while (nextToken != null && nextToken != string.Empty)
		{
			result = text;
			text = nextToken;
			nextToken = pdfReader.GetNextToken();
			if (nextToken == "Tf")
			{
				height = (float)double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			}
		}
		return result;
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
			else if (m_crossTable.Document is PdfLoadedDocument { RaisePdfFont: not false } pdfLoadedDocument)
			{
				PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
				pdfFontEventArgs.m_fontName = "Arial";
				pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
				pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
				m_font = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, result, string.Empty, useTrueType: false, isEnableEmbedding: true);
			}
		}
		else if (base.Dictionary["AP"] != null)
		{
			m_isfontAPStream = true;
			ObtainFromAPStream(m_isfontAPStream);
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
		else if (m_crossTable.Document is PdfLoadedDocument { RaisePdfFont: not false } pdfLoadedDocument)
		{
			PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
			pdfFontEventArgs.m_fontName = "Arial";
			pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
			pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
			m_font = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, size, string.Empty, useTrueType: true, isEnableEmbedding: true);
		}
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (!(base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			return;
		}
		if (base.Dictionary["AP"] != null)
		{
			PdfName pdfName = PdfCrossTable.Dereference(base.Dictionary["AS"]) as PdfName;
			if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N"))
			{
				PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["N"]) as PdfDictionary;
				if (pdfName != null && !string.IsNullOrEmpty(pdfName.Value))
				{
					if (pdfName.Value == "Yes" && pdfDictionary2.ContainsKey("Yes"))
					{
						pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary2["Yes"]) as PdfDictionary;
					}
					else if (pdfName.Value == "Off" && pdfDictionary2.ContainsKey("Off"))
					{
						pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary2["Off"]) as PdfDictionary;
					}
				}
				if (pdfDictionary2 != null && pdfDictionary2 is PdfStream template)
				{
					PdfTemplate pdfTemplate = new PdfTemplate(template);
					if (pdfTemplate != null)
					{
						PdfGraphics pdfGraphics = ObtainlayerGraphics();
						PdfGraphicsState state = base.Page.Graphics.Save();
						if (Opacity < 1f)
						{
							base.Page.Graphics.SetTransparency(Opacity);
						}
						if (pdfGraphics != null)
						{
							pdfGraphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
						}
						else
						{
							base.Page.Graphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
						}
						base.Page.Graphics.Restore(state);
					}
				}
			}
		}
		RemoveAnnoationFromPage(base.Page, this);
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
}
