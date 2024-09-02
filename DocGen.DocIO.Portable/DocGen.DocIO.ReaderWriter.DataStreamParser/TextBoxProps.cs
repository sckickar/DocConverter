using System;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser;

[CLSCompliant(false)]
internal class TextBoxProps : BaseProps
{
	private float m_txbxLineWidth;

	private LineDashing m_lineDashing;

	private Color m_fillColor;

	private TextBoxLineStyle m_lineStyle;

	private WrapMode m_wrapMode;

	private float m_txID;

	private bool m_noLine;

	private Color m_lineColor;

	private uint m_leftMargin;

	private uint m_rightMargin;

	private uint m_topMargin;

	private uint m_bottomMargin;

	private byte m_bFlags;

	internal bool NoLine
	{
		get
		{
			return m_noLine;
		}
		set
		{
			m_noLine = value;
		}
	}

	internal WrapMode WrapText
	{
		get
		{
			return m_wrapMode;
		}
		set
		{
			m_wrapMode = value;
		}
	}

	internal bool FitShapeToText
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal TextBoxLineStyle LineStyle
	{
		get
		{
			return m_lineStyle;
		}
		set
		{
			m_lineStyle = value;
		}
	}

	internal Color FillColor
	{
		get
		{
			return m_fillColor;
		}
		set
		{
			m_fillColor = value;
		}
	}

	internal Color LineColor
	{
		get
		{
			return m_lineColor;
		}
		set
		{
			m_lineColor = value;
		}
	}

	internal float TxbxLineWidth
	{
		get
		{
			return m_txbxLineWidth;
		}
		set
		{
			m_txbxLineWidth = value;
		}
	}

	internal LineDashing LineDashing
	{
		get
		{
			return m_lineDashing;
		}
		set
		{
			m_lineDashing = value;
		}
	}

	internal float TXID
	{
		get
		{
			return m_txID;
		}
		set
		{
			m_txID = value;
		}
	}

	internal uint LeftMargin
	{
		get
		{
			return m_leftMargin;
		}
		set
		{
			m_leftMargin = value;
		}
	}

	internal uint RightMargin
	{
		get
		{
			return m_rightMargin;
		}
		set
		{
			m_rightMargin = value;
		}
	}

	internal uint TopMargin
	{
		get
		{
			return m_topMargin;
		}
		set
		{
			m_topMargin = value;
		}
	}

	internal uint BottomMargin
	{
		get
		{
			return m_bottomMargin;
		}
		set
		{
			m_bottomMargin = value;
		}
	}

	internal TextBoxProps()
	{
		m_fillColor = Color.White;
		m_lineColor = Color.Black;
		m_txbxLineWidth = 0.75f;
		base.RelHrzPos = HorizontalOrigin.Column;
		base.RelVrtPos = VerticalOrigin.Paragraph;
		m_lineStyle = TextBoxLineStyle.Simple;
		m_lineDashing = LineDashing.Solid;
		m_wrapMode = WrapMode.None;
		m_leftMargin = uint.MaxValue;
		m_rightMargin = uint.MaxValue;
		m_topMargin = uint.MaxValue;
		m_bottomMargin = uint.MaxValue;
	}
}
