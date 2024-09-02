using System;
using DocGen.Drawing;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODFBorder
{
	private Color m_borderColor;

	private BorderLineStyle m_lineStyle;

	private string m_lineWidth;

	internal byte styleFlags;

	private const byte LineColorKey = 0;

	private const byte LineStyleKey = 1;

	private const byte LineWidthKey = 2;

	internal Color LineColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			styleFlags = (byte)((styleFlags & 0xFEu) | 1u);
			m_borderColor = value;
		}
	}

	internal BorderLineStyle LineStyle
	{
		get
		{
			return m_lineStyle;
		}
		set
		{
			styleFlags = (byte)((styleFlags & 0xFDu) | 2u);
			m_lineStyle = value;
		}
	}

	internal string LineWidth
	{
		get
		{
			return m_lineWidth;
		}
		set
		{
			styleFlags = (byte)((styleFlags & 0xFBu) | 4u);
			m_lineWidth = value;
		}
	}

	internal bool HasKey(int propertyKey)
	{
		return (styleFlags & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	public override bool Equals(object obj)
	{
		ODFBorder oDFBorder = obj as ODFBorder;
		bool flag = false;
		if (oDFBorder == null)
		{
			return false;
		}
		if (HasKey(0) && oDFBorder.HasKey(0))
		{
			flag = LineColor.Equals(oDFBorder.LineColor);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(1) && oDFBorder.HasKey(1))
		{
			flag = LineStyle.Equals(oDFBorder.LineStyle);
			if (!flag)
			{
				return flag;
			}
		}
		if (HasKey(2) && oDFBorder.HasKey(2))
		{
			return LineWidth.Equals(oDFBorder.LineWidth);
		}
		return flag;
	}
}
