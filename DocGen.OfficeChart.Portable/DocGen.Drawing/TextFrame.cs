using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class TextFrame : ITextFrame
{
	private bool wrapTextInShape = true;

	private bool isAutoMargins = true;

	private TextBodyPropertiesHolder m_textBodyProperties;

	private bool isTextOverFlow;

	private int marginLeftPt;

	private int topMarginPt;

	private int rightMarginPt;

	private int bottomMarginPt;

	private TextDirection textDirection;

	private OfficeVerticalAlignment verticalAlignment;

	private OfficeHorizontalAlignment horizontalAlignment;

	private TextVertOverflowType textVertOverflowType;

	private TextHorzOverflowType textHorzOverflowType;

	private ShapeImplExt shape;

	private bool isAutoSize;

	private TextRange m_textRange;

	internal TextFrameColumns Columns;

	public bool IsTextOverFlow
	{
		get
		{
			return isTextOverFlow;
		}
		set
		{
			isTextOverFlow = value;
			SetVisible();
		}
	}

	public bool WrapTextInShape
	{
		get
		{
			return wrapTextInShape;
		}
		set
		{
			wrapTextInShape = value;
			SetVisible();
		}
	}

	public int MarginLeftPt
	{
		get
		{
			return marginLeftPt;
		}
		set
		{
			marginLeftPt = value;
		}
	}

	public int TopMarginPt
	{
		get
		{
			return topMarginPt;
		}
		set
		{
			topMarginPt = value;
		}
	}

	public int RightMarginPt
	{
		get
		{
			return rightMarginPt;
		}
		set
		{
			rightMarginPt = value;
		}
	}

	public int BottomMarginPt
	{
		get
		{
			return bottomMarginPt;
		}
		set
		{
			bottomMarginPt = value;
		}
	}

	public bool IsAutoMargins
	{
		get
		{
			return isAutoMargins;
		}
		set
		{
			isAutoMargins = value;
			SetVisible();
		}
	}

	public bool IsAutoSize
	{
		get
		{
			return isAutoSize;
		}
		set
		{
			isAutoSize = value;
			SetVisible();
		}
	}

	public TextVertOverflowType TextVertOverflowType
	{
		get
		{
			return textVertOverflowType;
		}
		set
		{
			textVertOverflowType = value;
			SetVisible();
		}
	}

	public TextHorzOverflowType TextHorzOverflowType
	{
		get
		{
			return textHorzOverflowType;
		}
		set
		{
			textHorzOverflowType = value;
			SetVisible();
		}
	}

	public OfficeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return horizontalAlignment;
		}
		set
		{
			horizontalAlignment = value;
			SetVisible();
		}
	}

	public OfficeVerticalAlignment VerticalAlignment
	{
		get
		{
			return verticalAlignment;
		}
		set
		{
			verticalAlignment = value;
			SetVisible();
		}
	}

	public TextDirection TextDirection
	{
		get
		{
			return textDirection;
		}
		set
		{
			textDirection = value;
			SetVisible();
		}
	}

	public ITextRange TextRange
	{
		get
		{
			if (m_textRange == null)
			{
				m_textRange = new TextRange(this, shape.Logger);
			}
			return m_textRange;
		}
	}

	internal TextBodyPropertiesHolder TextBodyProperties
	{
		get
		{
			return m_textBodyProperties;
		}
		set
		{
			m_textBodyProperties = value;
		}
	}

	internal TextFrame(ShapeImplExt shape)
	{
		this.shape = shape;
		m_textBodyProperties = new TextBodyPropertiesHolder();
	}

	internal TextFrame Clone(object parent)
	{
		TextFrame textFrame = (TextFrame)MemberwiseClone();
		shape = (ShapeImplExt)parent;
		if (m_textRange != null)
		{
			textFrame.m_textRange = m_textRange.Clone(textFrame);
		}
		return textFrame;
	}

	internal bool GetAnchorPosition(TextDirection textDirection, OfficeVerticalAlignment verticalAlignment, OfficeHorizontalAlignment horizontalAlignment, out string anchor)
	{
		anchor = "t";
		switch (textDirection)
		{
		case TextDirection.Horizontal:
			switch (verticalAlignment)
			{
			case OfficeVerticalAlignment.Top:
				anchor = "t";
				return false;
			case OfficeVerticalAlignment.Middle:
				anchor = "ctr";
				return false;
			case OfficeVerticalAlignment.Bottom:
				anchor = "b";
				return false;
			case OfficeVerticalAlignment.TopCentered:
				anchor = "t";
				return true;
			case OfficeVerticalAlignment.MiddleCentered:
				anchor = "ctr";
				return true;
			case OfficeVerticalAlignment.BottomCentered:
				anchor = "b";
				return true;
			}
			break;
		case TextDirection.RotateAllText90:
		case TextDirection.StackedRightToLeft:
			switch (horizontalAlignment)
			{
			case OfficeHorizontalAlignment.Right:
				anchor = "t";
				return false;
			case OfficeHorizontalAlignment.Center:
				anchor = "ctr";
				return false;
			case OfficeHorizontalAlignment.Left:
				anchor = "b";
				return false;
			case OfficeHorizontalAlignment.RightMiddle:
				anchor = "t";
				return true;
			case OfficeHorizontalAlignment.CenterMiddle:
				anchor = "ctr";
				return true;
			case OfficeHorizontalAlignment.LeftMiddle:
				anchor = "b";
				return true;
			}
			break;
		case TextDirection.RotateAllText270:
		case TextDirection.StackedLeftToRight:
			switch (horizontalAlignment)
			{
			case OfficeHorizontalAlignment.Left:
				anchor = "t";
				return false;
			case OfficeHorizontalAlignment.Center:
				anchor = "ctr";
				return false;
			case OfficeHorizontalAlignment.Right:
				anchor = "b";
				return false;
			case OfficeHorizontalAlignment.LeftMiddle:
				anchor = "t";
				return true;
			case OfficeHorizontalAlignment.CenterMiddle:
				anchor = "ctr";
				return true;
			case OfficeHorizontalAlignment.RightMiddle:
				anchor = "b";
				return true;
			}
			break;
		}
		return false;
	}

	internal string GetTextDirection(TextDirection textDirection)
	{
		return textDirection switch
		{
			TextDirection.Horizontal => "horz", 
			TextDirection.RotateAllText90 => "vert", 
			TextDirection.RotateAllText270 => "vert270", 
			TextDirection.StackedLeftToRight => "wordArtVert", 
			TextDirection.StackedRightToLeft => "wordArtVertRtl", 
			_ => "horz", 
		};
	}

	internal int GetLeftMargin()
	{
		return (int)((double)marginLeftPt * 12700.0 + 0.5);
	}

	internal void SetLeftMargin(int value)
	{
		marginLeftPt = (int)((double)value / 12700.0);
	}

	internal int GetTopMargin()
	{
		return (int)((double)topMarginPt * 12700.0 + 0.5);
	}

	internal void SetTopMargin(int value)
	{
		topMarginPt = (int)((double)value / 12700.0);
	}

	internal int GetRightMargin()
	{
		return (int)((double)rightMarginPt * 12700.0 + 0.5);
	}

	internal void SetRightMargin(int value)
	{
		rightMarginPt = (int)((double)value / 12700.0);
	}

	internal int GetBottomMargin()
	{
		return (int)((double)bottomMarginPt * 12700.0 + 0.5);
	}

	internal void SetBottomMargin(int value)
	{
		bottomMarginPt = (int)((double)value / 12700.0);
	}

	internal bool GetAnchorPosition(out string anchor)
	{
		return GetAnchorPosition(textDirection, verticalAlignment, horizontalAlignment, out anchor);
	}

	internal IWorkbook GetWorkbook()
	{
		if (shape.Worksheet != null)
		{
			return shape.Worksheet.Workbook;
		}
		return shape.ParentSheet.Workbook;
	}

	internal void SetVisible()
	{
		shape.Logger.SetFlag(PreservedFlag.RichText);
	}
}
