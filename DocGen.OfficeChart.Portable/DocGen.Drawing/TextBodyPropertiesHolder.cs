using System.Xml;
using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class TextBodyPropertiesHolder
{
	private TextVertOverflowType m_textVertOverflowType;

	private TextHorzOverflowType m_textHorzOverflowType;

	private TextDirection m_textDirection;

	private double m_leftMarginPt = 7.2;

	private double m_topMarginPt = 3.6;

	private double m_rightMarginPt = 7.2;

	private double m_bottomMarginPt = 3.6;

	private bool m_wrapTextInShape = true;

	private TextFrameColumns m_columns;

	private OfficeVerticalAlignment m_verticalAlignment;

	private OfficeHorizontalAlignment m_horizontalAlignment;

	private bool m_isAutoSize;

	private bool m_isAutoMargins = true;

	private bool m_presetWrapTextInShape;

	internal TextVertOverflowType TextVertOverflowType
	{
		get
		{
			return m_textVertOverflowType;
		}
		set
		{
			m_textVertOverflowType = value;
		}
	}

	internal TextHorzOverflowType TextHorzOverflowType
	{
		get
		{
			return m_textHorzOverflowType;
		}
		set
		{
			m_textHorzOverflowType = value;
		}
	}

	internal TextDirection TextDirection
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

	internal double LeftMarginPt
	{
		get
		{
			return m_leftMarginPt;
		}
		set
		{
			m_leftMarginPt = value;
		}
	}

	internal double TopMarginPt
	{
		get
		{
			return m_topMarginPt;
		}
		set
		{
			m_topMarginPt = value;
		}
	}

	internal double RightMarginPt
	{
		get
		{
			return m_rightMarginPt;
		}
		set
		{
			m_rightMarginPt = value;
		}
	}

	internal double BottomMarginPt
	{
		get
		{
			return m_bottomMarginPt;
		}
		set
		{
			m_bottomMarginPt = value;
		}
	}

	internal bool WrapTextInShape
	{
		get
		{
			return m_wrapTextInShape;
		}
		set
		{
			m_wrapTextInShape = value;
		}
	}

	internal OfficeVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignment;
		}
		set
		{
			m_verticalAlignment = value;
		}
	}

	internal OfficeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_horizontalAlignment;
		}
		set
		{
			m_horizontalAlignment = value;
		}
	}

	internal bool PresetWrapTextInShape
	{
		get
		{
			return m_presetWrapTextInShape;
		}
		set
		{
			m_presetWrapTextInShape = value;
		}
	}

	internal bool IsAutoSize
	{
		get
		{
			return m_isAutoSize;
		}
		set
		{
			m_isAutoSize = value;
		}
	}

	internal bool IsAutoMargins
	{
		get
		{
			return m_isAutoMargins;
		}
		set
		{
			m_isAutoMargins = value;
		}
	}

	internal int Number
	{
		get
		{
			return m_columns.Number;
		}
		set
		{
			m_columns.Number = value;
		}
	}

	internal int SpacingPt
	{
		get
		{
			return m_columns.SpacingPt;
		}
		set
		{
			m_columns.SpacingPt = value;
		}
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
		return (int)(m_leftMarginPt * 12700.0 + 0.5);
	}

	internal void SetLeftMargin(int value)
	{
		m_leftMarginPt = (int)((double)value / 12700.0);
	}

	internal int GetTopMargin()
	{
		return (int)(m_topMarginPt * 12700.0 + 0.5);
	}

	internal void SetTopMargin(int value)
	{
		m_topMarginPt = (int)((double)value / 12700.0);
	}

	internal int GetRightMargin()
	{
		return (int)(m_rightMarginPt * 12700.0 + 0.5);
	}

	internal void SetRightMargin(int value)
	{
		m_rightMarginPt = (int)((double)value / 12700.0);
	}

	internal int GetBottomMargin()
	{
		return (int)(m_bottomMarginPt * 12700.0 + 0.5);
	}

	internal void SetBottomMargin(int value)
	{
		m_bottomMarginPt = (int)((double)value / 12700.0);
	}

	internal bool GetAnchorPosition(out string anchor)
	{
		return GetAnchorPosition(m_textDirection, m_verticalAlignment, m_horizontalAlignment, out anchor);
	}

	internal void SerialzieTextBodyProperties(XmlWriter xmlTextWriter, string prefix, string nameSpace)
	{
		xmlTextWriter.WriteStartElement(prefix, "bodyPr", nameSpace);
		if (TextVertOverflowType != 0)
		{
			xmlTextWriter.WriteAttributeString("vertOverflow", Helper.GetVerticalFlowType(TextVertOverflowType));
		}
		if (TextHorzOverflowType != 0)
		{
			xmlTextWriter.WriteAttributeString("horzOverflow", Helper.GetHorizontalFlowType(TextHorzOverflowType));
		}
		string value = "square";
		if (!WrapTextInShape)
		{
			value = "none";
		}
		xmlTextWriter.WriteAttributeString("wrap", value);
		if (!IsAutoMargins)
		{
			xmlTextWriter.WriteAttributeString("lIns", Helper.ToString(GetLeftMargin()));
			xmlTextWriter.WriteAttributeString("tIns", Helper.ToString(GetTopMargin()));
			xmlTextWriter.WriteAttributeString("rIns", Helper.ToString(GetRightMargin()));
			xmlTextWriter.WriteAttributeString("bIns", Helper.ToString(GetBottomMargin()));
		}
		string anchor = "t";
		bool anchorPosition = GetAnchorPosition(out anchor);
		if (TextDirection != 0)
		{
			string textDirection = GetTextDirection(TextDirection);
			if (textDirection != null)
			{
				xmlTextWriter.WriteAttributeString("vert", textDirection);
			}
		}
		xmlTextWriter.WriteAttributeString("anchor", anchor);
		if (anchorPosition)
		{
			xmlTextWriter.WriteAttributeString("anchorCtr", "1");
		}
		else
		{
			xmlTextWriter.WriteAttributeString("anchorCtr", "0");
		}
		if (IsAutoSize)
		{
			xmlTextWriter.WriteElementString("a", "spAutoFit", "http://schemas.openxmlformats.org/drawingml/2006/main", null);
		}
		if (Number > 0)
		{
			xmlTextWriter.WriteAttributeString("numCol", Helper.ToString(Number));
		}
		int num = (int)((double)SpacingPt * 12700.0 + 0.5);
		if (num > 0)
		{
			xmlTextWriter.WriteAttributeString("spcCol", Helper.ToString(num));
		}
		xmlTextWriter.WriteEndElement();
	}
}
