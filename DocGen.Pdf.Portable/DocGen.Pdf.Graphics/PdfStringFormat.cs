using System;

namespace DocGen.Pdf.Graphics;

public sealed class PdfStringFormat : ICloneable
{
	private PdfTextAlignment m_alignment;

	private PdfVerticalAlignment m_lineAlignment;

	private bool m_rightToLeft;

	private PdfTextDirection m_textDirection;

	private float m_characterSpacing;

	private float m_wordSpacing;

	private float m_leading;

	private bool m_clip;

	private PdfSubSuperScript m_subSuperScript;

	private float m_scalingFactor = 100f;

	private float m_firstLineIndent;

	internal float m_paragraphIndent;

	private bool m_lineLimit;

	private bool m_measureTrailingSpaces;

	private bool m_noClip;

	private PdfWordWrapType m_wrapType;

	internal bool isCustomRendering;

	private bool m_complexScript;

	private bool m_baseLine;

	internal bool m_isList;

	private bool m_enableNewLineIndent = true;

	private bool m_measureTiltingSpace;

	private float m_tiltingSpace;

	public PdfTextDirection TextDirection
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

	public PdfTextAlignment Alignment
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

	public PdfVerticalAlignment LineAlignment
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

	public PdfSubSuperScript SubSuperScript
	{
		get
		{
			return m_subSuperScript;
		}
		set
		{
			m_subSuperScript = value;
		}
	}

	public float ParagraphIndent
	{
		get
		{
			return FirstLineIndent;
		}
		set
		{
			FirstLineIndent = value;
			if (EnableNewLineIndent)
			{
				m_paragraphIndent = value;
			}
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

	public PdfWordWrapType WordWrap
	{
		get
		{
			return m_wrapType;
		}
		set
		{
			m_wrapType = value;
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

	public bool EnableNewLineIndent
	{
		get
		{
			return m_enableNewLineIndent;
		}
		set
		{
			m_enableNewLineIndent = value;
			if (!value && m_paragraphIndent > 0f)
			{
				m_firstLineIndent = m_paragraphIndent;
				m_paragraphIndent = 0f;
			}
			else if (m_firstLineIndent > 0f && m_paragraphIndent == 0f)
			{
				m_paragraphIndent = m_firstLineIndent;
			}
		}
	}

	public bool MeasureTiltingSpace
	{
		get
		{
			return m_measureTiltingSpace;
		}
		set
		{
			m_measureTiltingSpace = value;
		}
	}

	internal float TiltingSpace
	{
		get
		{
			return m_tiltingSpace;
		}
		set
		{
			m_tiltingSpace = value;
		}
	}

	public PdfStringFormat()
	{
		m_lineLimit = true;
		m_wrapType = PdfWordWrapType.Word;
	}

	public PdfStringFormat(PdfTextAlignment alignment)
		: this()
	{
		m_alignment = alignment;
	}

	public PdfStringFormat(string columnFormat)
		: this()
	{
	}

	public PdfStringFormat(PdfTextAlignment alignment, PdfVerticalAlignment lineAlignment)
		: this(alignment)
	{
		m_lineAlignment = lineAlignment;
	}

	public object Clone()
	{
		return (PdfStringFormat)MemberwiseClone();
	}
}
