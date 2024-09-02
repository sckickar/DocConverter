using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class CSSStyleItem
{
	internal enum CssStyleType
	{
		None,
		ElementSelector,
		IdSelector,
		ClassSelector,
		GroupingSelector,
		DescendantSelector,
		ChildSelector,
		AdjacentSiblingSelector,
		GeneralSiblingSelector
	}

	internal enum TextFormatKey
	{
		FontSize,
		FontFamily,
		Bold,
		Underline,
		Italic,
		Strike,
		FontColor,
		BackColor,
		LineHeight,
		LineHeightNormal,
		TextAlign,
		TopMargin,
		LeftMargin,
		BottomMargin,
		RightMargin,
		TextIndent,
		SubSuperScript,
		PageBreakBefore,
		PageBreakAfter,
		LetterSpacing,
		AllCaps,
		WhiteSpace,
		WordWrap,
		Display,
		ContentAlign,
		ItemsAlign,
		SelfAlign,
		Animation,
		AniamtionDelay,
		AnimationDirection,
		AnimationDuration,
		AnimationFillMode,
		AnimationIterationCount,
		AnimationName,
		AnimationPlayState,
		AnimationTimingFunction,
		BackfaceVisibility,
		Background,
		BackgroundAttachment,
		BackgroundClip,
		BackgroundImage,
		BackgroundOrigin,
		BackgroundPosition,
		BackgroundRepeat,
		BackgroundSize,
		Border,
		BorderColor,
		BorderStyle,
		BorderWidth,
		LeftBorder,
		RightBorder,
		TopBorder,
		BottomBorder,
		BorderBottomColor,
		BorderBottomStyle,
		BorderBottomWidth,
		BorderLeftColor,
		BorderLeftStyle,
		BorderLeftWidth,
		BorderRightColor,
		BorderRightStyle,
		BorderRightWidth,
		BorderTopColor,
		BorderTopStyle,
		BorderTopWidth,
		BorderBottomLeftRadius,
		BorderBottomRightRadius,
		BorderCollapse,
		BorderImage,
		BorderImageOutset,
		BorderImageRepeat,
		BorderImageSlice,
		BorderImageSource,
		BorderImageWidth,
		BorderRadius,
		BorderSpacing,
		BorderTopLeftRadius,
		BorderTopRightRadius,
		Bottom,
		BoxShadow,
		BoxSizing,
		CaptionSide,
		Clear,
		Clip,
		ColumnCount,
		ColumnFill,
		ColumnGap,
		ColumnRule,
		ColumnRuleColor,
		ColumnRuleStyle,
		ColumnRuleWidth,
		ColumnSpan,
		ColumnWidth,
		Columns,
		Content,
		CounterIncrement,
		CounterReset,
		Cursor,
		Direction,
		EmptyCells,
		Flex,
		FlexBasis,
		FlexDirection,
		FlexFlow,
		FlexGrow,
		FlexShrink,
		FlexWrap,
		Float,
		Font,
		FontFace,
		FontSizeAdjust,
		FontStretch,
		FontStyleKey,
		FontVariant,
		FontWeight,
		HangingPunctuation,
		Height,
		Icon,
		JustifyContent,
		KeyFrames,
		Left,
		ListStyle,
		listStyleImage,
		listStylePosition,
		ListStyleType,
		Margin,
		MaxHeight,
		MaxWidth,
		MinHeight,
		MinWidth,
		NavDown,
		NavIndex,
		NavLeft,
		NavRight,
		NavUp,
		Opacity,
		Order,
		Outline,
		OutlineColor,
		OutlineOffset,
		OutlineStyle,
		OutlineWidth,
		OverFlow,
		Overflow_X,
		Overflow_Y,
		Padding,
		PaddingBottom,
		PaddingLeft,
		PaddingRight,
		PaddingTop,
		PageBreakInside,
		Perspective,
		PerspectiveOrigin,
		Position,
		Quotes,
		Resize,
		Right,
		TabSize,
		TableLayout,
		TextAlignLast,
		TextDecoration,
		TextDecorationColor,
		TextDecorationLine,
		TextJustify,
		TextOverflow,
		TextShadow,
		Top,
		Transform,
		TransformOrigin,
		TransformStyle,
		Transition,
		TransitionDelay,
		TransitionDuration,
		TransitionProperty,
		TransitionTimingFunction,
		UnicodeBidi,
		VerticalAlign,
		Visibility,
		Width,
		WordBreak,
		WordSpacing,
		Zindex,
		Capitalize,
		Lowercase
	}

	internal enum TextFormatImportantKey
	{
		FontSize = 0,
		FontFamily = 1,
		Bold = 2,
		Underline = 3,
		Italic = 4,
		Strike = 5,
		FontColor = 6,
		BackColor = 7,
		LineHeight = 8,
		LineHeightNormal = 9,
		TextAlign = 10,
		TopMargin = 11,
		LeftMargin = 12,
		BottomMargin = 13,
		RightMargin = 14,
		TextIndent = 15,
		SubSuperScript = 16,
		PageBreakBefore = 17,
		PageBreakAfter = 18,
		LetterSpacing = 19,
		AllCaps = 20,
		WhiteSpace = 21,
		WordWrap = 22,
		Display = 23,
		BackgroundAttachment = 24,
		BackgroundImage = 25,
		Background = 26,
		BackgroundPosition = 27,
		BackgroundRepeat = 28,
		BackgroundSize = 29,
		BorderBottomColor = 30,
		BorderBottom = 32,
		BorderBottomStyle = 33,
		BorderBottomWidth = 34,
		BorderCollapse = 35,
		BorderColor = 36,
		Border = 37,
		BorderLeftColor = 38,
		BorderLeftStyle = 39,
		BorderLeftWidth = 40,
		BorderRightColor = 41,
		BorderRightStyle = 42,
		BorderRightWidth = 43,
		BorderSpacing = 43,
		BorderStyle = 44,
		BorderTopColor = 45,
		BorderTopStyle = 46,
		BorderTopWidth = 47,
		BorderWidth = 48,
		CaptionSide = 49,
		Cursor = 50,
		Direction = 51,
		EmptyCells = 52,
		Font = 53,
		FontSizeAdjust = 54,
		FontStretch = 55,
		FontStyle = 56,
		FontVariant = 57,
		FontWeight = 58,
		Height = 59,
		ListStyleImage = 60,
		ListStyle = 61,
		ListStylePosition = 62,
		ListStyleType = 63,
		MarginBottom = 64,
		Margin = 65,
		MarginLeft = 66,
		MarginRight = 67,
		MarginTop = 68,
		MarkerOffset = 69,
		MozOpacity = 70,
		PaddingBottom = 71,
		Padding = 72,
		PaddingLeft = 73,
		PaddingRight = 74,
		PaddingTop = 75,
		TableLayout = 76,
		TextDecoration = 77,
		TextShadow = 78,
		Width = 79,
		WordSpacing = 80
	}

	private string m_styleName;

	private Dictionary<TextFormatKey, object> m_propertiesHash;

	private Dictionary<TextFormatImportantKey, object> m_importantPropertiesHash;

	private CssStyleType m_styleType;

	internal string StyleName
	{
		get
		{
			return m_styleName;
		}
		set
		{
			m_styleName = value;
		}
	}

	internal CssStyleType StyleType
	{
		get
		{
			return m_styleType;
		}
		set
		{
			m_styleType = value;
		}
	}

	internal Dictionary<TextFormatKey, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<TextFormatKey, object>();
			}
			return m_propertiesHash;
		}
	}

	internal Dictionary<TextFormatImportantKey, object> ImportantPropertiesHash
	{
		get
		{
			if (m_importantPropertiesHash == null)
			{
				m_importantPropertiesHash = new Dictionary<TextFormatImportantKey, object>();
			}
			return m_importantPropertiesHash;
		}
	}

	internal object this[TextFormatKey key]
	{
		get
		{
			if (!PropertiesHash.ContainsKey(key))
			{
				return null;
			}
			return PropertiesHash[key];
		}
		set
		{
			if (PropertiesHash.ContainsKey(key))
			{
				PropertiesHash[key] = value;
			}
			else
			{
				PropertiesHash.Add(key, value);
			}
		}
	}

	internal object this[TextFormatImportantKey key]
	{
		get
		{
			if (!ImportantPropertiesHash.ContainsKey(key))
			{
				return null;
			}
			return ImportantPropertiesHash[key];
		}
		set
		{
			if (ImportantPropertiesHash.ContainsKey(key))
			{
				ImportantPropertiesHash[key] = value;
			}
			else
			{
				ImportantPropertiesHash.Add(key, value);
			}
		}
	}

	internal void Close()
	{
		if (m_importantPropertiesHash != null)
		{
			m_importantPropertiesHash.Clear();
			m_importantPropertiesHash = null;
		}
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
	}
}
