using System;
using System.Globalization;
using System.IO;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedRedactionAnnotation : PdfLoadedStyledAnnotation
{
	private PdfColor borderColor;

	private LineBorder border = new LineBorder();

	private string overlayText = string.Empty;

	private PdfColor textColor;

	private bool repeat;

	private PdfFont font;

	private PdfTextAlignment alignment;

	private PdfCrossTable crossTable;

	private bool isfontAPStream;

	private bool flatten;

	private MemoryStream redactTextStream;

	private PdfRecordCollection readTextCollection;

	private ContentParser parser;

	private PdfDictionary dictionary = new PdfDictionary();

	internal bool AppearanceEnabled;

	public PdfColor TextColor
	{
		get
		{
			return ObtainTextColor();
		}
		set
		{
			textColor = value;
			if (textColor.A != 0)
			{
				base.Dictionary.SetProperty("C", textColor.ToArray());
				NotifyPropertyChanged("TextColor");
			}
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
			alignment = value;
			base.Dictionary.SetProperty("Q", new PdfNumber((int)alignment));
			NotifyPropertyChanged("TextAlignment");
		}
	}

	public new LineBorder Border
	{
		get
		{
			return border;
		}
		set
		{
			border = value;
			NotifyPropertyChanged("Border");
		}
	}

	public string OverlayText
	{
		get
		{
			return ObtainOverlayText();
		}
		set
		{
			overlayText = value;
			base.Dictionary.SetString("OverlayText", overlayText);
			NotifyPropertyChanged("OverlayText");
		}
	}

	public PdfFont Font
	{
		get
		{
			if (font == null)
			{
				return ObtainFont();
			}
			return font;
		}
		set
		{
			font = value;
			NotifyPropertyChanged("Font");
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return ObtainBorderColor();
		}
		set
		{
			borderColor = value;
			if (borderColor.A != 0)
			{
				base.Dictionary.SetProperty("OC", borderColor.ToArray());
				NotifyPropertyChanged("BorderColor");
			}
		}
	}

	public bool RepeatText
	{
		get
		{
			return ObtainTextRepeat();
		}
		set
		{
			repeat = value;
			base.Dictionary.SetBoolean("Repeat", repeat);
			NotifyPropertyChanged("RepeatText");
		}
	}

	public new bool Flatten
	{
		get
		{
			return flatten;
		}
		set
		{
			flatten = value;
			if (flatten)
			{
				ApplyRedaction();
			}
		}
	}

	internal PdfLoadedRedactionAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		this.crossTable = crossTable;
		base.Dictionary = dictionary;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		RemoveAnnoationFromPage(base.Page, this);
		if (Popup != null)
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		PdfTemplate mouseHover = CreateNormalAppearance(OverlayText, Font, RepeatText, TextColor, TextAlignment, Border);
		if (Flatten || base.Page.Annotations.Flatten)
		{
			RemoveAnnoationFromPage(base.Page, this);
		}
		else
		{
			PdfTemplate normal = CreateBorderAppearance(BorderColor, Border);
			base.Appearance.Normal = normal;
			base.Appearance.MouseHover = mouseHover;
			base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
		}
		if (Popup != null && (Flatten || base.Page.Annotations.Flatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	private void ApplyRedaction()
	{
	}

	private string ObtainOverlayText()
	{
		string result = string.Empty;
		if (base.Dictionary.ContainsKey("OverlayText") && PdfCrossTable.Dereference(base.Dictionary["OverlayText"]) is PdfString pdfString)
		{
			result = pdfString.Value.ToString();
		}
		return result;
	}

	private bool ObtainTextRepeat()
	{
		bool result = false;
		if (base.Dictionary.ContainsKey("Repeat") && PdfCrossTable.Dereference(base.Dictionary["Repeat"]) is PdfBoolean pdfBoolean)
		{
			result = pdfBoolean.Value;
		}
		return result;
	}

	private PdfColor ObtainBorderColor()
	{
		PdfColor result = PdfColor.Empty;
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("OC"))
		{
			pdfArray = PdfCrossTable.Dereference(base.Dictionary["OC"]) as PdfArray;
			result = GetColorFromArray(pdfArray);
		}
		return result;
	}

	private PdfColor ObtainTextColor()
	{
		PdfColor result = PdfColor.Empty;
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("C"))
		{
			pdfArray = PdfCrossTable.Dereference(base.Dictionary["C"]) as PdfArray;
			result = GetColorFromArray(pdfArray);
		}
		return result;
	}

	private PdfColor GetColorFromArray(PdfArray colours)
	{
		PdfColor result = PdfColor.Empty;
		if (colours != null)
		{
			if (colours.Count == 3 && colours[0] is PdfNumber && colours[1] is PdfNumber && colours[2] is PdfNumber)
			{
				byte red = (byte)Math.Round((colours[0] as PdfNumber).FloatValue * 255f);
				byte green = (byte)Math.Round((colours[1] as PdfNumber).FloatValue * 255f);
				byte blue = (byte)Math.Round((colours[2] as PdfNumber).FloatValue * 255f);
				result = new PdfColor(red, green, blue);
			}
			if (colours.Count == 4 && colours[0] is PdfNumber && colours[1] is PdfNumber && colours[2] is PdfNumber && colours[3] is PdfNumber)
			{
				byte a = (byte)Math.Round((colours[0] as PdfNumber).FloatValue * 255f);
				byte red2 = (byte)Math.Round((colours[1] as PdfNumber).FloatValue * 255f);
				byte green2 = (byte)Math.Round((colours[2] as PdfNumber).FloatValue * 255f);
				byte blue2 = (byte)Math.Round((colours[3] as PdfNumber).FloatValue * 255f);
				result = new PdfColor(a, red2, green2, blue2);
			}
			if (colours.Count == 1 && colours[0] is PdfNumber)
			{
				float gray = (float)Math.Round((colours[0] as PdfNumber).FloatValue * 255f);
				result = new PdfColor(gray);
			}
		}
		return result;
	}

	private PdfTextAlignment ObtainTextAlignment()
	{
		PdfTextAlignment result = PdfTextAlignment.Left;
		if (base.Dictionary.ContainsKey("Q") && PdfCrossTable.Dereference(base.Dictionary["Q"]) is PdfNumber pdfNumber)
		{
			result = (PdfTextAlignment)Enum.ToObject(typeof(PdfTextAlignment), pdfNumber.IntValue);
		}
		return result;
	}

	private PdfFont ObtainFont()
	{
		if (base.Dictionary["AP"] != null)
		{
			isfontAPStream = true;
			ObtainFromAPStream(isfontAPStream);
		}
		return font;
	}

	private void ObtainFromAPStream(bool isfontStream)
	{
		if (!(PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary) || !(PdfCrossTable.Dereference(pdfDictionary["R"]) is PdfDictionary pdfDictionary2))
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
				if (PdfCrossTable.Dereference(pdfDictionary3["Font"]) is PdfDictionary pdfDictionary4 && text2.Contains("/"))
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
				font = new PdfStandardFont(PdfFontFamily.Helvetica, size);
				break;
			case "Courier":
				font = new PdfStandardFont(PdfFontFamily.Courier, size);
				break;
			case "Symbol":
				font = new PdfStandardFont(PdfFontFamily.Symbol, size);
				break;
			case "TimesRoman":
				font = new PdfStandardFont(PdfFontFamily.TimesRoman, size);
				break;
			case "ZapfDingbats":
				font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, size);
				break;
			case "MonotypeSungLight":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeSungLight, size);
				break;
			case "SinoTypeSongLight":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.SinoTypeSongLight, size);
				break;
			case "MonotypeHeiMedium":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeHeiMedium, size);
				break;
			case "HanyangSystemsGothicMedium":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsGothicMedium, size);
				break;
			case "HanyangSystemsShinMyeongJoMedium":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium, size);
				break;
			case "HeiseiKakuGothicW5":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiKakuGothicW5, size);
				break;
			case "HeiseiMinchoW3":
				font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiMinchoW3, size);
				break;
			default:
				font = new PdfStandardFont(PdfFontFamily.Helvetica, size);
				break;
			}
		}
		else
		{
			PdfLoadedDocument pdfLoadedDocument = crossTable.Document as PdfLoadedDocument;
			if (pdfLoadedDocument.RaisePdfFont)
			{
				PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
				pdfFontEventArgs.m_fontName = "Arial";
				pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
				pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
				font = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, size, string.Empty, useTrueType: false, isEnableEmbedding: true);
			}
		}
	}
}
