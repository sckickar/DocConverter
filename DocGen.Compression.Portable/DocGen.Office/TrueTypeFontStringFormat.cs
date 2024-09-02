using System;

namespace DocGen.Office;

internal sealed class TrueTypeFontStringFormat : ICloneable
{
	private TextAlignment m_alignment;

	private VerticalAlignment m_lineAlignment;

	private bool m_rightToLeft;

	private TextDirection m_textDirection;

	private float m_characterSpacing;

	private float m_wordSpacing;

	private float m_leading;

	private bool m_clip;

	private float m_scalingFactor = 100f;

	private float m_firstLineIndent;

	private float m_paragraphIndent;

	private bool m_lineLimit;

	private bool m_measureTrailingSpaces;

	private bool m_noClip;

	internal bool isCustomRendering;

	private bool m_complexScript;

	private bool m_baseLine;

	public TextDirection TextDirection
	{
		get
		{
			return m_textDirection;
		}
		set
		{
			m_textDirection = value;
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
		}
	}

	public TextAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	public VerticalAlignment LineAlignment
	{
		get
		{
			return m_lineAlignment;
		}
		set
		{
			m_lineAlignment = value;
		}
	}

	public bool EnableBaseline
	{
		get
		{
			return m_baseLine;
		}
		set
		{
			m_baseLine = value;
		}
	}

	internal bool RightToLeft
	{
		get
		{
			return m_rightToLeft;
		}
		set
		{
			m_rightToLeft = value;
		}
	}

	public float CharacterSpacing
	{
		get
		{
			return m_characterSpacing;
		}
		set
		{
			m_characterSpacing = value;
		}
	}

	public float WordSpacing
	{
		get
		{
			return m_wordSpacing;
		}
		set
		{
			m_wordSpacing = value;
		}
	}

	public float LineSpacing
	{
		get
		{
			return m_leading;
		}
		set
		{
			m_leading = value;
		}
	}

	public bool ClipPath
	{
		get
		{
			return m_clip;
		}
		set
		{
			m_clip = value;
		}
	}

	public float ParagraphIndent
	{
		get
		{
			return m_paragraphIndent;
		}
		set
		{
			m_paragraphIndent = value;
			FirstLineIndent = value;
		}
	}

	public bool LineLimit
	{
		get
		{
			return m_lineLimit;
		}
		set
		{
			m_lineLimit = value;
		}
	}

	public bool MeasureTrailingSpaces
	{
		get
		{
			return m_measureTrailingSpaces;
		}
		set
		{
			m_measureTrailingSpaces = value;
		}
	}

	public bool NoClip
	{
		get
		{
			return m_noClip;
		}
		set
		{
			m_noClip = value;
		}
	}

	internal float HorizontalScalingFactor
	{
		get
		{
			return m_scalingFactor;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException("The scaling factor can't be less of equal to zero.", "ScalingFactor");
			}
			m_scalingFactor = value;
		}
	}

	internal float FirstLineIndent
	{
		get
		{
			return m_firstLineIndent;
		}
		set
		{
			m_firstLineIndent = value;
		}
	}

	public TrueTypeFontStringFormat()
	{
		m_lineLimit = true;
	}

	public TrueTypeFontStringFormat(TextAlignment alignment)
		: this()
	{
		m_alignment = alignment;
	}

	public TrueTypeFontStringFormat(string columnFormat)
		: this()
	{
	}

	public TrueTypeFontStringFormat(TextAlignment alignment, VerticalAlignment lineAlignment)
		: this(alignment)
	{
		m_lineAlignment = lineAlignment;
	}

	public object Clone()
	{
		return (TrueTypeFontStringFormat)MemberwiseClone();
	}
}
