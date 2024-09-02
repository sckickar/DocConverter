using System;
using DocGen.Drawing;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableCellProperties : BorderProperties
{
	private int m_rotationAngle;

	private bool m_wrap;

	private float m_borderLineWidth;

	private float m_borderLineWidthTop;

	private float m_borderLineWidthBottom;

	private float m_borderLineWidthLeft;

	private float m_borderLineWidthRight;

	private bool m_shrinkToFit;

	private VerticalAlign? m_verticalAlign;

	private Color m_backColor;

	private float m_paddingTop;

	private float m_paddingBottom;

	private float m_paddingLeft;

	private float m_paddingRight;

	private bool m_repeatContent;

	private PageOrder m_direction;

	internal ushort tableCellFlags;

	internal const byte RotationAngleKey = 0;

	internal const byte WrapKey = 1;

	internal const byte BorderLineWidthKey = 2;

	internal const byte BorderLineWidthTopKey = 3;

	internal const byte BorderLineWidthBottomKey = 4;

	internal const byte BorderLineWidthLeftKey = 5;

	internal const byte BorderLineWidthRightKey = 6;

	internal const byte ShrinkToFitKey = 7;

	internal const byte BackColorKey = 8;

	internal const byte VerticalAlignKey = 9;

	internal const byte PaddingRightKey = 10;

	internal const byte paddingLeftKey = 11;

	internal const byte PaddingBottomKey = 12;

	internal const byte PaddingTopKey = 13;

	internal const byte RepeatContentKey = 14;

	internal const byte DirectionKey = 15;

	internal int RotationAngle
	{
		get
		{
			return m_rotationAngle;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFFEu) | 1u);
			m_rotationAngle = value;
		}
	}

	internal bool Wrap
	{
		get
		{
			return m_wrap;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFFDu) | 2u);
			m_wrap = value;
		}
	}

	internal float BorderLineWidth
	{
		get
		{
			return m_borderLineWidth;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFFBu) | 4u);
			m_borderLineWidth = value;
		}
	}

	internal float BorderLineWidthTop
	{
		get
		{
			return m_borderLineWidthTop;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFF7u) | 8u);
			m_borderLineWidthTop = value;
		}
	}

	internal float BorderLineWidthBottom
	{
		get
		{
			return m_borderLineWidthBottom;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFEFu) | 0x10u);
			m_borderLineWidthBottom = value;
		}
	}

	internal float BorderLineWidthLeft
	{
		get
		{
			return m_borderLineWidthLeft;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFDFu) | 0x20u);
			m_borderLineWidthLeft = value;
		}
	}

	internal float BorderLineWidthRight
	{
		get
		{
			return m_borderLineWidthRight;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFFBFu) | 0x40u);
			m_borderLineWidthRight = value;
		}
	}

	internal bool ShrinkToFit
	{
		get
		{
			return m_shrinkToFit;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFF7Fu) | 0x80u);
			m_shrinkToFit = value;
		}
	}

	internal Color BackColor
	{
		get
		{
			return m_backColor;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFEFFu) | 0x100u);
			m_backColor = value;
		}
	}

	internal VerticalAlign? VerticalAlign
	{
		get
		{
			return m_verticalAlign;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFDFFu) | 0x200u);
			m_verticalAlign = value;
		}
	}

	internal float PaddingRight
	{
		get
		{
			return m_paddingRight;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xFBFFu) | 0x400u);
			m_paddingRight = value;
		}
	}

	internal float PaddingLeft
	{
		get
		{
			return m_paddingLeft;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xF7FFu) | 0x800u);
			m_paddingLeft = value;
		}
	}

	internal float PaddingBottom
	{
		get
		{
			return m_paddingBottom;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xEFFFu) | 0x1000u);
			m_paddingBottom = value;
		}
	}

	internal float PaddingTop
	{
		get
		{
			return m_paddingTop;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xDFFFu) | 0x2000u);
			m_paddingTop = value;
		}
	}

	internal bool RepeatContent
	{
		get
		{
			return m_repeatContent;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0xBFFFu) | 0x4000u);
			m_repeatContent = value;
		}
	}

	internal PageOrder Direction
	{
		get
		{
			return m_direction;
		}
		set
		{
			tableCellFlags = (ushort)((tableCellFlags & 0x7FFFu) | 0x8000u);
			m_direction = value;
		}
	}

	internal bool HasKey(int propertyKey, int flagname)
	{
		return (flagname & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	public override bool Equals(object obj)
	{
		OTableCellProperties oTableCellProperties = obj as OTableCellProperties;
		bool flag = false;
		if (oTableCellProperties == null)
		{
			return false;
		}
		if (HasKey(0, tableCellFlags) && oTableCellProperties.HasKey(0, oTableCellProperties.tableCellFlags))
		{
			flag = RotationAngle.Equals(oTableCellProperties.RotationAngle);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(1, tableCellFlags) && oTableCellProperties.HasKey(1, oTableCellProperties.tableCellFlags))
		{
			flag = Wrap.Equals(oTableCellProperties.Wrap);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(9, tableCellFlags) && oTableCellProperties.HasKey(9, oTableCellProperties.tableCellFlags))
		{
			flag = VerticalAlign.Equals(oTableCellProperties.VerticalAlign);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(8, tableCellFlags) && oTableCellProperties.HasKey(8, oTableCellProperties.tableCellFlags))
		{
			flag = BackColor.Equals(oTableCellProperties.BackColor);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(7, tableCellFlags) && oTableCellProperties.HasKey(7, oTableCellProperties.tableCellFlags))
		{
			flag = BackColor.Equals(oTableCellProperties.BackColor);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(0, borderFlags) && oTableCellProperties.HasKey(0, oTableCellProperties.borderFlags))
		{
			flag = base.Border.Equals(oTableCellProperties.Border);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(1, borderFlags) && oTableCellProperties.HasKey(1, oTableCellProperties.borderFlags))
		{
			flag = base.BorderTop.Equals(oTableCellProperties.BorderTop);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(2, borderFlags) && oTableCellProperties.HasKey(2, oTableCellProperties.borderFlags))
		{
			flag = base.BorderBottom.Equals(oTableCellProperties.BorderBottom);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(3, borderFlags) && oTableCellProperties.HasKey(3, oTableCellProperties.borderFlags))
		{
			flag = base.BorderLeft.Equals(oTableCellProperties.BorderLeft);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(4, borderFlags) && oTableCellProperties.HasKey(4, oTableCellProperties.borderFlags))
		{
			flag = base.BorderRight.Equals(oTableCellProperties.BorderRight);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(11, tableCellFlags) && oTableCellProperties.HasKey(11, oTableCellProperties.tableCellFlags))
		{
			flag = PaddingLeft.Equals(oTableCellProperties.PaddingLeft);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(10, tableCellFlags) && oTableCellProperties.HasKey(10, oTableCellProperties.tableCellFlags))
		{
			flag = PaddingRight.Equals(oTableCellProperties.PaddingRight);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(13, tableCellFlags) && oTableCellProperties.HasKey(13, oTableCellProperties.tableCellFlags))
		{
			flag = PaddingTop.Equals(oTableCellProperties.PaddingTop);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(12, tableCellFlags) && oTableCellProperties.HasKey(12, oTableCellProperties.tableCellFlags))
		{
			flag = PaddingBottom.Equals(oTableCellProperties.PaddingBottom);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(14, tableCellFlags) && oTableCellProperties.HasKey(14, oTableCellProperties.tableCellFlags))
		{
			flag = RepeatContent.Equals(oTableCellProperties.RepeatContent);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(15, tableCellFlags) && oTableCellProperties.HasKey(15, oTableCellProperties.tableCellFlags))
		{
			return Direction.Equals(oTableCellProperties.Direction);
		}
		return flag;
	}
}
