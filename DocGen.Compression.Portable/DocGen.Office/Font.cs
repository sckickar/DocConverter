using System;
using DocGen.Drawing;

namespace DocGen.Office;

internal abstract class Font : ITrueTypeFontWrapper, ITrueTypeFontCache
{
	internal const float CharSizeMultiplier = 0.001f;

	protected static object s_syncObject = new object();

	private float m_size;

	private TTFFontStyle m_style;

	private TrueTypeFontMetrics m_fontMetrics;

	private ITrueTypeFontPrimitive m_fontInternals;

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

	public TTFFontStyle Style
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

	public bool Bold => (Style & TTFFontStyle.Bold) > TTFFontStyle.Regular;

	public bool Italic => (Style & TTFFontStyle.Italic) > TTFFontStyle.Regular;

	public bool Strikeout => (Style & TTFFontStyle.Strikeout) > TTFFontStyle.Regular;

	public bool Underline => (Style & TTFFontStyle.Underline) > TTFFontStyle.Regular;

	public TrueTypeFontMetrics Metrics
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

	internal ITrueTypeFontPrimitive FontInternal
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

	ITrueTypeFontPrimitive ITrueTypeFontWrapper.Element => m_fontInternals;

	protected Font(float size)
	{
		m_size = size;
	}

	protected Font(float size, TTFFontStyle style)
		: this(size)
	{
		SetStyle(style);
	}

	public SizeF MeasureString(string text)
	{
		return MeasureString(text, null);
	}

	public SizeF MeasureString(string text, TrueTypeFontStringFormat format)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, TrueTypeFontStringFormat format, out float boundWidth)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, format, out charactersFitted, out linesFilled, out boundWidth);
	}

	public SizeF MeasureString(string text, TrueTypeFontStringFormat format, out int charactersFitted, out int linesFilled)
	{
		return MeasureString(text, 0f, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, TrueTypeFontStringFormat format, out int charactersFitted, out int linesFilled, out float boundWidth)
	{
		return MeasureString(text, 0f, format, out charactersFitted, out linesFilled, out boundWidth);
	}

	public SizeF MeasureString(string text, float width)
	{
		return MeasureString(text, width, null);
	}

	public SizeF MeasureString(string text, float width, TrueTypeFontStringFormat format)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, width, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, float width, TrueTypeFontStringFormat format, out int charactersFitted, out int linesFilled)
	{
		SizeF layoutArea = new SizeF(width, 0f);
		return MeasureString(text, layoutArea, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, float width, TrueTypeFontStringFormat format, out int charactersFitted, out int linesFilled, out float boundWidth)
	{
		SizeF layoutArea = new SizeF(width, 0f);
		return MeasureString(text, layoutArea, format, out charactersFitted, out linesFilled, out boundWidth);
	}

	public SizeF MeasureString(string text, SizeF layoutArea)
	{
		return MeasureString(text, layoutArea, null);
	}

	public SizeF MeasureString(string text, SizeF layoutArea, TrueTypeFontStringFormat format)
	{
		int charactersFitted = 0;
		int linesFilled = 0;
		return MeasureString(text, layoutArea, format, out charactersFitted, out linesFilled);
	}

	public SizeF MeasureString(string text, SizeF layoutArea, TrueTypeFontStringFormat format, out int charactersFitted, out int linesFilled)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		StringLayoutResult stringLayoutResult = new StringLayouter().Layout(text, this, format, layoutArea);
		charactersFitted = ((stringLayoutResult.Remainder == null) ? text.Length : (text.Length - stringLayoutResult.Remainder.Length));
		linesFilled = ((!stringLayoutResult.Empty) ? stringLayoutResult.Lines.Length : 0);
		return stringLayoutResult.ActualSize;
	}

	public SizeF MeasureString(string text, SizeF layoutArea, TrueTypeFontStringFormat format, out int charactersFitted, out int linesFilled, out float boundWidth)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		StringLayoutResult stringLayoutResult = new StringLayouter().Layout(text, this, format, layoutArea, out boundWidth);
		charactersFitted = ((stringLayoutResult.Remainder == null) ? text.Length : (text.Length - stringLayoutResult.Remainder.Length));
		linesFilled = ((!stringLayoutResult.Empty) ? stringLayoutResult.Lines.Length : 0);
		return stringLayoutResult.ActualSize;
	}

	internal SizeF MeasureString(string text, SizeF layoutArea, TrueTypeFontStringFormat format, out StringLayoutResult result)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		StringLayouter stringLayouter = new StringLayouter();
		result = stringLayouter.Layout(text, this, format, layoutArea);
		return result.ActualSize;
	}

	bool ITrueTypeFontCache.EqualsTo(ITrueTypeFontCache obj)
	{
		return EqualsToFont(obj as Font);
	}

	ITrueTypeFontPrimitive ITrueTypeFontCache.GetInternals()
	{
		return m_fontInternals;
	}

	void ITrueTypeFontCache.SetInternals(ITrueTypeFontPrimitive internals)
	{
		m_fontInternals = internals ?? throw new ArgumentNullException("internals");
	}

	protected abstract bool EqualsToFont(Font font);

	protected internal abstract float GetCharWidth(char charCode, TrueTypeFontStringFormat format);

	protected internal abstract float GetLineWidth(string line, TrueTypeFontStringFormat format);

	protected internal abstract float GetLineWidth(string line, TrueTypeFontStringFormat format, out float boundWidth);

	protected void SetStyle(TTFFontStyle style)
	{
		m_style = style;
	}

	protected float ApplyFormatSettings(string line, TrueTypeFontStringFormat format, float width)
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
