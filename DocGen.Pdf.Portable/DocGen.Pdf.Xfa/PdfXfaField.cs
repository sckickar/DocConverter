using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public abstract class PdfXfaField
{
	private string m_name = string.Empty;

	private PdfMargins m_margins = new PdfMargins();

	private PdfXfaVisibility m_visibility;

	internal string LSFN;

	internal bool m_isRendered;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public PdfMargins Margins
	{
		get
		{
			return m_margins;
		}
		set
		{
			m_margins = value;
		}
	}

	public PdfXfaVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
		}
	}

	internal PdfTextAlignment ConvertToPdfTextAlignment(PdfXfaHorizontalAlignment align)
	{
		PdfTextAlignment result = PdfTextAlignment.Center;
		switch (align)
		{
		case PdfXfaHorizontalAlignment.Center:
			result = PdfTextAlignment.Center;
			break;
		case PdfXfaHorizontalAlignment.Justify:
		case PdfXfaHorizontalAlignment.JustifyAll:
			result = PdfTextAlignment.Justify;
			break;
		case PdfXfaHorizontalAlignment.Left:
			result = PdfTextAlignment.Left;
			break;
		case PdfXfaHorizontalAlignment.Right:
			result = PdfTextAlignment.Right;
			break;
		}
		return result;
	}

	internal void SetFont(PdfDocumentBase doc, PdfFont font)
	{
		if (font == null || !(font is PdfTrueTypeFont))
		{
			return;
		}
		PdfForm pdfForm = doc.ObtainForm();
		if (pdfForm.Dictionary == null)
		{
			return;
		}
		PdfDictionary dictionary = pdfForm.Dictionary;
		if (dictionary.ContainsKey("DR"))
		{
			if (!(dictionary.Items[new PdfName("DR")] is PdfResources pdfResources))
			{
				return;
			}
			if (pdfResources.Items[new PdfName("Font")] is PdfDictionary pdfDictionary)
			{
				if (!pdfDictionary.ContainsKey(new PdfName(font.Metrics.PostScriptName)))
				{
					pdfDictionary.Items.Add(new PdfName(font.Metrics.PostScriptName), new PdfReferenceHolder(font));
				}
				return;
			}
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			if (!pdfDictionary2.ContainsKey(new PdfName(font.Metrics.PostScriptName)))
			{
				pdfDictionary2.Items.Add(new PdfName(font.Metrics.PostScriptName), new PdfReferenceHolder(font));
			}
			pdfResources.Items.Add(new PdfName("Font"), pdfDictionary2);
		}
		else
		{
			PdfResources pdfResources2 = new PdfResources();
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			pdfDictionary3.Items.Add(new PdfName(font.Metrics.PostScriptName), new PdfReferenceHolder(font));
			pdfResources2.SetProperty(new PdfName("Font"), pdfDictionary3);
			dictionary.Items.Add(new PdfName("DR"), pdfResources2);
		}
	}

	internal RectangleF GetBounds(RectangleF bounds, PdfXfaRotateAngle rotate, PdfXfaCaption caption)
	{
		RectangleF result = bounds;
		if (caption != null)
		{
			switch (rotate)
			{
			case PdfXfaRotateAngle.RotateAngle0:
				result = ((caption.Position != PdfXfaPosition.Top) ? ((caption.Position != PdfXfaPosition.Bottom) ? ((caption.Position != 0) ? new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height)) : new RectangleF(new PointF(result.Location.X + caption.Width, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height))) : new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width, result.Size.Height - caption.Width))) : new RectangleF(new PointF(result.Location.X, result.Location.Y + caption.Width), new SizeF(result.Size.Width, result.Size.Height - caption.Width)));
				break;
			case PdfXfaRotateAngle.RotateAngle180:
				result = ((caption.Position != PdfXfaPosition.Top) ? ((caption.Position != PdfXfaPosition.Bottom) ? ((caption.Position != 0) ? new RectangleF(new PointF(result.Location.X + caption.Width, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height)) : new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height))) : new RectangleF(new PointF(result.Location.X, result.Location.Y + caption.Width), new SizeF(result.Size.Width, result.Size.Height - caption.Width))) : new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width, result.Size.Height - caption.Width)));
				break;
			case PdfXfaRotateAngle.RotateAngle90:
				result = ((caption.Position != PdfXfaPosition.Top) ? ((caption.Position != PdfXfaPosition.Bottom) ? ((caption.Position != 0) ? new RectangleF(new PointF(result.Location.X, result.Location.Y + caption.Width), new SizeF(result.Size.Width, result.Size.Height - caption.Width)) : new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width, result.Size.Height - caption.Width))) : new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height))) : new RectangleF(new PointF(result.Location.X + caption.Width, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height)));
				break;
			case PdfXfaRotateAngle.RotateAngle270:
				result = ((caption.Position != PdfXfaPosition.Top) ? ((caption.Position != PdfXfaPosition.Bottom) ? ((caption.Position != 0) ? new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width, result.Size.Height - caption.Width)) : new RectangleF(new PointF(result.Location.X, result.Location.Y + caption.Width), new SizeF(result.Size.Width, result.Size.Height - caption.Width))) : new RectangleF(new PointF(result.Location.X + caption.Width, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height))) : new RectangleF(new PointF(result.Location.X, result.Location.Y), new SizeF(result.Size.Width - caption.Width, result.Size.Height)));
				break;
			}
		}
		return result;
	}

	internal string GetPattern(string patternText)
	{
		string text = patternText;
		if (patternText.Contains("|"))
		{
			text = patternText.Substring(0, patternText.IndexOf("|"));
		}
		text = text.Replace("date{", string.Empty);
		text = text.Replace("}", string.Empty);
		text = text.Replace('Y', 'y');
		text = text.Replace('D', 'd');
		text = text.Replace("time{", string.Empty);
		return text.Replace("}", string.Empty);
	}

	internal string TrimDatePattern(string patternText)
	{
		string text = patternText;
		if (patternText.Contains("|"))
		{
			text = patternText.Substring(0, patternText.IndexOf("|"));
		}
		text = text.TrimEnd();
		text = text.TrimStart();
		if (text.Contains("date.short{}"))
		{
			return "date.short{}";
		}
		if (text.Contains("date.medium{}"))
		{
			return "date.medium{}";
		}
		if (text.Contains("date.long{}"))
		{
			return "date.long{}";
		}
		if (text.Contains("date.full{}"))
		{
			return "date.full{}";
		}
		if (text.Contains("date{"))
		{
			int startIndex = text.IndexOf("date{");
			int length = text.IndexOf("}", startIndex);
			text = text.Substring(startIndex, length);
		}
		else if (text.Contains("h"))
		{
			text = text.Substring(0, text.IndexOf("h"));
		}
		else if (text.Contains("H"))
		{
			text = text.Substring(0, text.IndexOf("H"));
		}
		text = text.Replace("date{", string.Empty);
		text = text.Replace("}", string.Empty);
		return text.TrimEnd();
	}

	internal string TrimTimePattern(string patternText)
	{
		string text = patternText;
		if (patternText.Contains("|"))
		{
			text = patternText.Substring(0, patternText.IndexOf("|"));
		}
		text = text.TrimEnd();
		text = text.TrimStart();
		if (text.Contains("time.short{}"))
		{
			return "time.short{}";
		}
		if (text.Contains("time.medium{}"))
		{
			return "time.medium{}";
		}
		if (text.Contains("time.long{}"))
		{
			return "time.long{}";
		}
		if (text.Contains("time.full{}"))
		{
			return "time.full{}";
		}
		if (text.Contains("time{"))
		{
			int num = text.IndexOf("time{");
			int num2 = text.IndexOf("}", num);
			text = text.Substring(num, num2 - num);
		}
		else if (text.Contains("h"))
		{
			text = text[text.IndexOf("h")..];
		}
		else if (text.Contains("H"))
		{
			text = text[text.IndexOf("H")..];
		}
		text = text.Replace("time{", string.Empty);
		text = text.Replace("}", string.Empty);
		return text.TrimEnd();
	}
}
