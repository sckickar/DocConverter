using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public abstract class PdfFont : IPdfWrapper, IPdfCache
{
	internal const float CharSizeMultiplier = 0.001f;

	protected static object s_syncObject = new object();

	private float m_size;

	private PdfFontStyle m_style;

	private PdfFontMetrics m_fontMetrics;

	private IPdfPrimitive m_fontInternals;

	private string m_internalFontName;

	private float m_ascentValue;

	public string Name => Metrics.Name;

	public float Size
	{
		get
		{
			return m_size;
		}
		internal set
		{
			if (Metrics != null)
			{
				Metrics.Size = value;
				m_size = value;
			}
		}
	}

	public float Height => Metrics.GetHeight(null);

	public PdfFontStyle Style
	{
		get
		{
			return m_style;
		}
		internal set
		{
			m_style = value;
		}
	}

	public bool Bold => (Style & PdfFontStyle.Bold) > PdfFontStyle.Regular;

	public bool Italic => (Style & PdfFontStyle.Italic) > PdfFontStyle.Regular;

	public bool Strikeout => (Style & PdfFontStyle.Strikeout) > PdfFontStyle.Regular;

	public bool Underline => (Style & PdfFontStyle.Underline) > PdfFontStyle.Regular;

	internal PdfFontMetrics Metrics
	{
		get
		{
			return m_fontMetrics;
		}
		set
		{
			m_fontMetrics = value;
		}
	}

	internal string InternalFontName
	{
		get
		{
			return m_internalFontName;
		}
		set
		{
			m_internalFontName = value;
		}
	}

	internal IPdfPrimitive FontInternal
	{
		get
		{
			return m_fontInternals;
		}
		set
		{
			m_fontInternals = value;
		}
	}

	internal float Ascent
	{
		get
		{
			return m_ascentValue;
		}
		set
		{
			m_ascentValue = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_fontInternals;

	protected PdfFont(float size)
	{
		m_size = size;
	}

	protected PdfFont(float size, PdfFontStyle style)
		: this(size)
	{
		SetStyle(style);
	}

	public SizeF MeasureString(string text)
	{
		return MeasureString(text, null);
	}

	public SizeF MeasureString(string text, PdfStringFormat format)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, PdfStringFormat format, out int charactersFitted, out int linesFilled)
	{
		return MeasureString(text, 0f, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, float width)
	{
		return MeasureString(text, width, null);
	}

	public SizeF MeasureString(string text, float width, PdfStringFormat format)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, width, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, float width, PdfStringFormat format, out int charactersFitted, out int linesFilled)
	{
		SizeF layoutArea = new SizeF(width, 0f);
		return MeasureString(text, layoutArea, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, SizeF layoutArea)
	{
		return MeasureString(text, layoutArea, null);
	}

	public SizeF MeasureString(string text, SizeF layoutArea, PdfStringFormat format)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, layoutArea, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, SizeF layoutArea, PdfStringFormat format, out int charactersFitted, out int linesFilled)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		PdfStringLayoutResult pdfStringLayoutResult = new PdfStringLayouter().Layout(text, this, format, layoutArea);
		charactersFitted = ((pdfStringLayoutResult.Remainder == null) ? text.Length : (text.Length - pdfStringLayoutResult.Remainder.Length));
		linesFilled = ((!pdfStringLayoutResult.Empty) ? pdfStringLayoutResult.Lines.Length : 0);
		return pdfStringLayoutResult.ActualSize;
	}

	internal SizeF MeasureString(string text, SizeF layoutArea, PdfStringFormat format, out PdfStringLayoutResult result)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		result = pdfStringLayouter.Layout(text, this, format, layoutArea);
		return result.ActualSize;
	}

	bool IPdfCache.EqualsTo(IPdfCache obj)
	{
		return EqualsToFont(obj as PdfFont);
	}

	IPdfPrimitive IPdfCache.GetInternals()
	{
		return m_fontInternals;
	}

	void IPdfCache.SetInternals(IPdfPrimitive internals)
	{
		if (internals == null)
		{
			throw new ArgumentNullException("internals");
		}
		m_fontInternals = internals;
	}

	protected abstract bool EqualsToFont(PdfFont font);

	protected internal abstract float GetCharWidth(char charCode, PdfStringFormat format);

	protected internal abstract float GetLineWidth(string line, PdfStringFormat format);

	protected void SetStyle(PdfFontStyle style)
	{
		m_style = style;
	}

	protected float ApplyFormatSettings(string line, PdfStringFormat format, float width)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		float num = width;
		if (format != null && width > 0f)
		{
			if (format.CharacterSpacing != 0f)
			{
				num += (float)(line.Length - 1) * format.CharacterSpacing;
			}
			if (format.WordSpacing != 0f)
			{
				char[] spaces = StringTokenizer.Spaces;
				int charsCount = StringTokenizer.GetCharsCount(line, spaces);
				num += (float)charsCount * format.WordSpacing;
			}
		}
		return num;
	}
}
