using System;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfCjkStandardFont : PdfFont
{
	private const int c_charOffset = 32;

	private PdfCjkFontFamily m_fontFamily;

	public PdfCjkFontFamily FontFamily => m_fontFamily;

	public PdfCjkStandardFont(PdfCjkFontFamily fontFamily, float size, PdfFontStyle style)
		: base(size, style)
	{
		m_fontFamily = fontFamily;
		CheckStyle();
		InitializeInternals();
	}

	public PdfCjkStandardFont(PdfCjkFontFamily fontFamily, float size)
		: this(fontFamily, size, PdfFontStyle.Regular)
	{
	}

	public PdfCjkStandardFont(PdfCjkStandardFont prototype, float size)
		: this(prototype.FontFamily, size, prototype.Style)
	{
	}

	public PdfCjkStandardFont(PdfCjkStandardFont prototype, float size, PdfFontStyle style)
		: this(prototype.FontFamily, size, style)
	{
	}

	protected override bool EqualsToFont(PdfFont font)
	{
		bool result = false;
		if (font is PdfCjkStandardFont pdfCjkStandardFont)
		{
			bool num = FontFamily == pdfCjkStandardFont.FontFamily;
			bool flag = (base.Style & ~(PdfFontStyle.Underline | PdfFontStyle.Strikeout)) == (pdfCjkStandardFont.Style & ~(PdfFontStyle.Underline | PdfFontStyle.Strikeout));
			result = num && flag;
		}
		return result;
	}

	protected internal override float GetCharWidth(char charCode, PdfStringFormat format)
	{
		float charWidthInternal = GetCharWidthInternal(charCode, format);
		float size = base.Metrics.GetSize(format);
		return charWidthInternal * (0.001f * size);
	}

	protected internal override float GetLineWidth(string line, PdfStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		float num = 0f;
		int i = 0;
		for (int length = line.Length; i < length; i++)
		{
			char charCode = line[i];
			float charWidthInternal = GetCharWidthInternal(charCode, format);
			num += charWidthInternal;
		}
		float size = base.Metrics.GetSize(format);
		num *= 0.001f * size;
		return ApplyFormatSettings(line, format, num);
	}

	private void InitializeInternals()
	{
		lock (PdfFont.s_syncObject)
		{
			IPdfCache pdfCache = null;
			if (PdfDocument.EnableCache)
			{
				pdfCache = PdfDocument.Cache.Search(this);
			}
			IPdfPrimitive pdfPrimitive = null;
			if (pdfCache == null)
			{
				PdfFontMetrics metrics = PdfCjkStandardFontMetricsFactory.GetMetrics(m_fontFamily, base.Style, base.Size);
				base.Metrics = metrics;
				pdfPrimitive = CreateInternals();
			}
			else
			{
				pdfPrimitive = pdfCache.GetInternals();
				PdfFontMetrics metrics2 = ((PdfFont)pdfCache).Metrics;
				metrics2 = (PdfFontMetrics)metrics2.Clone();
				metrics2.Size = base.Size;
				base.Metrics = metrics2;
			}
			((IPdfCache)this).SetInternals(pdfPrimitive);
		}
	}

	private PdfDictionary CreateInternals()
	{
		return new PdfDictionary
		{
			["Type"] = new PdfName("Font"),
			["Subtype"] = new PdfName("Type0"),
			["BaseFont"] = new PdfName(base.Metrics.PostScriptName),
			["Encoding"] = GetEncoding(m_fontFamily),
			["DescendantFonts"] = GetDescendantFont()
		};
	}

	private PdfArray GetDescendantFont()
	{
		PdfArray pdfArray = new PdfArray();
		PdfCidFont element = new PdfCidFont(m_fontFamily, base.Style, base.Metrics);
		pdfArray.Add(element);
		return pdfArray;
	}

	private static PdfName GetEncoding(PdfCjkFontFamily fontFamily)
	{
		string text = "Unknown";
		switch (fontFamily)
		{
		case PdfCjkFontFamily.HanyangSystemsGothicMedium:
		case PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium:
			text = "UniKS-UCS2-H";
			break;
		case PdfCjkFontFamily.HeiseiKakuGothicW5:
		case PdfCjkFontFamily.HeiseiMinchoW3:
			text = "UniJIS-UCS2-H";
			break;
		case PdfCjkFontFamily.MonotypeHeiMedium:
		case PdfCjkFontFamily.MonotypeSungLight:
			text = "UniCNS-UCS2-H";
			break;
		case PdfCjkFontFamily.SinoTypeSongLight:
			text = "UniGB-UCS2-H";
			break;
		default:
			throw new ArgumentException("Unsupported font face.", "fontFamily");
		}
		return new PdfName(text);
	}

	private void CheckStyle()
	{
		PdfFontStyle style = base.Style;
		SetStyle(style);
	}

	private float GetCharWidthInternal(char charCode, PdfStringFormat format)
	{
		int num = charCode;
		num = ((num >= 0) ? num : 0);
		return base.Metrics.WidthTable[num];
	}
}
