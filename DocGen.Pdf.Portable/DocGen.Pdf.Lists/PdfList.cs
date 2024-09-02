using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public abstract class PdfList : PdfLayoutElement
{
	protected static readonly char[] c_splitChars = new char[1] { '\n' };

	private PdfListItemCollection m_items;

	private float m_indent = 10f;

	private float m_textIndent = 5f;

	private PdfFont m_font;

	private PdfPen m_pen;

	private PdfBrush m_brush;

	private PdfStringFormat m_format;

	public PdfListItemCollection Items
	{
		get
		{
			if (m_items == null)
			{
				m_items = new PdfListItemCollection();
			}
			return m_items;
		}
	}

	public float Indent
	{
		get
		{
			return m_indent;
		}
		set
		{
			m_indent = value;
		}
	}

	public float TextIndent
	{
		get
		{
			return m_textIndent;
		}
		set
		{
			m_textIndent = value;
		}
	}

	public PdfFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("font");
			}
			m_font = value;
		}
	}

	public PdfBrush Brush
	{
		get
		{
			return m_brush;
		}
		set
		{
			m_brush = value;
		}
	}

	public PdfPen Pen
	{
		get
		{
			return m_pen;
		}
		set
		{
			m_pen = value;
		}
	}

	public PdfStringFormat StringFormat
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	internal bool RiseBeginItemLayout => this.BeginItemLayout != null;

	internal bool RiseEndItemLayout => this.EndItemLayout != null;

	public event BeginItemLayoutEventHandler BeginItemLayout;

	public event EndItemLayoutEventHandler EndItemLayout;

	protected static PdfListItemCollection CreateItems(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new PdfListItemCollection(text.Split(c_splitChars));
	}

	internal PdfList()
	{
	}

	internal PdfList(PdfListItemCollection items)
	{
		if (items == null)
		{
			throw new ArgumentException("Items collection can't be null", "items");
		}
		m_items = items;
	}

	internal PdfList(PdfFont font)
	{
		Font = font;
	}

	public override void Draw(PdfGraphics graphics, float x, float y)
	{
		new PdfListLayouter(this).Layout(graphics, x, y);
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		new PdfListLayouter(this).Layout(graphics, PointF.Empty);
	}

	protected override PdfLayoutResult Layout(PdfLayoutParams param)
	{
		return new PdfListLayouter(this).Layout(param);
	}

	internal void OnBeginItemLayout(BeginItemLayoutEventArgs args)
	{
		if (RiseBeginItemLayout)
		{
			this.BeginItemLayout(this, args);
		}
	}

	internal void OnEndItemLayout(EndItemLayoutEventArgs args)
	{
		if (RiseEndItemLayout)
		{
			this.EndItemLayout(this, args);
		}
	}
}
