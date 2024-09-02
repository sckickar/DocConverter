using System;
using System.ComponentModel;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartLegendItem : IDisposable
{
	private Font c_defaultFont = new Font("Verdana", 10f);

	private bool m_isLegendTextColor;

	protected ChartLegendItemsCollection m_children;

	protected string m_text = "";

	protected ChartLegendItemStyle m_style = new ChartLegendItemStyle();

	protected bool m_visible = true;

	protected bool m_isChecked;

	protected RectangleF m_bounds = Rectangle.Empty;

	protected RectangleF m_iconRect = Rectangle.Empty;

	protected RectangleF m_textRect = Rectangle.Empty;

	protected bool m_isDrawingShadow;

	private IChartLegend m_legend;

	private ChartLegendItem m_owner;

	private DocGen.Drawing.Image m_iconImage;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public LegendDrawItemTextEventHandler m_textHandler;

	internal bool IsLegendTextColor
	{
		get
		{
			return m_isLegendTextColor;
		}
		set
		{
			m_isLegendTextColor = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartLegendItemsCollection Children => m_children;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartLegendItemStyle ItemStyle => m_style;

	public Color BorderColor
	{
		get
		{
			return m_style.BorderColor;
		}
		set
		{
			if (m_style.BorderColor != value)
			{
				m_style.BorderColor = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public Font Font
	{
		get
		{
			if (m_style.Font == null)
			{
				if (m_owner != null)
				{
					return m_owner.Font;
				}
				if (m_legend != null)
				{
					return m_legend.Font;
				}
				return c_defaultFont;
			}
			return m_style.Font;
		}
		set
		{
			if (m_style.Font != value)
			{
				m_style.Font = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public int ImageIndex
	{
		get
		{
			return m_style.ImageIndex;
		}
		set
		{
			if (m_style.ImageIndex != value)
			{
				m_style.ImageIndex = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	[DefaultValue(null)]
	public ChartImageCollection ImageList
	{
		get
		{
			return m_style.ImageList;
		}
		set
		{
			if (m_style.ImageList != value)
			{
				m_style.ImageList = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public BrushInfo Interior
	{
		get
		{
			return m_style.Interior;
		}
		set
		{
			if (m_style.Interior != value)
			{
				m_style.Interior = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public Size RepresentationSize
	{
		get
		{
			return m_style.RepresentationSize;
		}
		set
		{
			if (m_style.RepresentationSize != value)
			{
				m_style.RepresentationSize = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool ShowSymbol
	{
		get
		{
			return m_style.ShowSymbol;
		}
		set
		{
			if (m_style.ShowSymbol != value)
			{
				m_style.ShowSymbol = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	[DefaultValue(0)]
	public int Spacing
	{
		get
		{
			return m_style.Spacing;
		}
		set
		{
			if (m_style.Spacing != value)
			{
				m_style.Spacing = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public ChartSymbolInfo Symbol
	{
		get
		{
			return m_style.Symbol;
		}
		set
		{
			if (m_style.Symbol != value)
			{
				m_style.Symbol = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public ChartLineInfo Border
	{
		get
		{
			return m_style.Border;
		}
		set
		{
			if (m_style.Border != value)
			{
				m_style.Border = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (m_text != value)
			{
				m_text = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public Color TextColor
	{
		get
		{
			if (m_style.TextColor.IsEmpty)
			{
				if (m_owner != null)
				{
					return m_owner.TextColor;
				}
				if (m_legend != null)
				{
					return m_legend.ForeColor;
				}
				return Color.Black;
			}
			return m_style.TextColor;
		}
		set
		{
			if (m_style.TextColor != value)
			{
				m_style.IsStyleChanged = true;
				m_style.TextColor = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public ChartLegendItemType Type
	{
		get
		{
			return m_style.Type;
		}
		set
		{
			if (m_style.Type != value)
			{
				m_style.Type = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool ShowIcon
	{
		get
		{
			return m_style.ShowIcon;
		}
		set
		{
			if (m_style.ShowIcon != value)
			{
				m_style.ShowIcon = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public LeftRightAlignment IconAlignment
	{
		get
		{
			return m_style.IconAlignment;
		}
		set
		{
			if (m_style.IconAlignment != value)
			{
				m_style.IconAlignment = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public VerticalAlignment TextAligment
	{
		get
		{
			return m_style.TextAlignment;
		}
		set
		{
			if (m_style.TextAlignment != value)
			{
				m_style.TextAlignment = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool VisibleCheckBox
	{
		get
		{
			return m_style.VisibleCheckBox;
		}
		set
		{
			if (m_style.VisibleCheckBox != value)
			{
				m_style.VisibleCheckBox = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool ShowShadow
	{
		get
		{
			return m_style.ShowShadow;
		}
		set
		{
			if (m_style.ShowShadow != value)
			{
				m_style.ShowShadow = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public Size ShadowOffset
	{
		get
		{
			return m_style.ShadowOffset;
		}
		set
		{
			if (m_style.ShadowOffset != value)
			{
				m_style.ShadowOffset = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public Color ShadowColor
	{
		get
		{
			return m_style.ShadowColor;
		}
		set
		{
			if (m_style.ShadowColor != value)
			{
				m_style.ShadowColor = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			if (m_visible != value)
			{
				m_visible = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			if (m_isChecked != value)
			{
				m_isChecked = value;
				OnCheckedChanged(EventArgs.Empty);
			}
		}
	}

	public string[] TextLines
	{
		get
		{
			return Text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
		}
		set
		{
			Text = string.Join(Environment.NewLine, value);
		}
	}

	public RectangleF Bounds => m_bounds;

	public DocGen.Drawing.Image Image
	{
		get
		{
			if (m_iconImage == null && m_style.ImageList != null && m_style.ImageIndex > -1 && m_style.ImageIndex < m_style.ImageList.Count)
			{
				return m_style.ImageList[m_style.ImageIndex];
			}
			return m_iconImage;
		}
		set
		{
			if (m_iconImage != value)
			{
				m_iconImage = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler Changed;

	public event EventHandler CheckedChanged;

	public ChartLegendItem()
	{
		m_children = new ChartLegendItemsCollection();
		m_children.Changed += OnChildrenChanged;
	}

	public ChartLegendItem(string text)
	{
		m_text = text;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public void SetLegend(IChartLegend legend)
	{
		m_legend = legend;
	}

	private void SetOwner(ChartLegendItem owner)
	{
		m_owner = owner;
	}

	public bool IsHit(int x, int y)
	{
		return m_iconRect.Contains(x, y);
	}

	public SizeF Measure(Graphics g)
	{
		float num = m_style.Spacing;
		SizeF result = g.MeasureString(m_text, Font, new PointF(0f, 0f), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
		if (m_style.ShowIcon)
		{
			Size representationSize = m_style.RepresentationSize;
			result.Width += num + (float)representationSize.Width;
			result.Height = Math.Max(result.Height, representationSize.Height);
		}
		return result;
	}

	internal SizeF Measure(Graphics g, string text)
	{
		float num = m_style.Spacing;
		SizeF result = g.MeasureString(text, Font, new PointF(0f, 0f), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
		if (m_style.ShowIcon)
		{
			Size representationSize = m_style.RepresentationSize;
			result.Width += num + (float)representationSize.Width;
			result.Height = Math.Max(result.Height, representationSize.Height);
		}
		return result;
	}

	public SizeF Measure(Graphics g, Rectangle bounds)
	{
		float num = m_style.Spacing;
		SizeF result = (m_style.ShowIcon ? new SizeF((float)m_style.RepresentationSize.Width + num, m_style.RepresentationSize.Height) : ((SizeF)Size.Empty));
		SizeF sizeF = g.MeasureString(m_text, Font, (int)((float)bounds.Width - result.Width));
		result.Width += sizeF.Width;
		result.Height = Math.Max(result.Height, sizeF.Height);
		return result;
	}

	public void Arrange(RectangleF rect)
	{
		float num = m_style.Spacing;
		SizeF sizeF = new SizeF(m_style.RepresentationSize.Width, m_style.RepresentationSize.Width);
		if (m_style.IconAlignment == LeftRightAlignment.Right)
		{
			m_iconRect = new RectangleF(rect.Right - sizeF.Width, rect.Top + (rect.Height - sizeF.Height) / 2f, sizeF.Width, sizeF.Height);
			float num2 = m_iconRect.Width - m_iconRect.Width * 0.75f;
			m_iconRect = new RectangleF(m_iconRect.X, m_iconRect.Y + num2, m_iconRect.Width * 0.75f, m_iconRect.Height * 0.75f);
			m_textRect = (m_style.ShowIcon ? new RectangleF(rect.Left, rect.Top, rect.Width - num - sizeF.Width, rect.Height) : new RectangleF(rect.Left, rect.Top, rect.Width + num, rect.Height));
		}
		else
		{
			m_iconRect = new RectangleF(rect.Left, rect.Top + (rect.Height - sizeF.Height) / 2f, sizeF.Width, sizeF.Height);
			float num3 = m_iconRect.Width - m_iconRect.Width * 0.75f;
			m_iconRect = new RectangleF(m_iconRect.X, m_iconRect.Y + num3, m_iconRect.Width * 0.75f, m_iconRect.Height * 0.75f);
			m_textRect = (m_style.ShowIcon ? new RectangleF(rect.Left + num + sizeF.Width, rect.Top, rect.Width - num - sizeF.Width, rect.Height) : new RectangleF(rect.Left + num + sizeF.Width, rect.Top, rect.Width + num, rect.Height));
		}
		m_bounds = rect;
	}

	public void Draw(Graphics g)
	{
		if (m_style.ShowShadow)
		{
			GraphicsContainer cont = DrawingHelper.BeginTransform(g);
			m_isDrawingShadow = true;
			g.TranslateTransform(m_style.ShadowOffset.Width, m_style.ShadowOffset.Height);
			DrawInternal(g);
			m_isDrawingShadow = false;
			DrawingHelper.EndTransform(g, cont);
		}
		DrawInternal(g);
	}

	internal void Draw(Graphics g, string text, bool isDrawIcon)
	{
		if (m_style.ShowShadow)
		{
			GraphicsContainer cont = DrawingHelper.BeginTransform(g);
			m_isDrawingShadow = true;
			g.TranslateTransform(m_style.ShadowOffset.Width, m_style.ShadowOffset.Height);
			DrawInternal(g, text, isDrawIcon);
			m_isDrawingShadow = false;
			DrawingHelper.EndTransform(g, cont);
		}
		DrawInternal(g, text, isDrawIcon);
	}

	public void Draw(Graphics g, Pen borderPen, RectangleF bounds, StringFormat format)
	{
		if (borderPen != null)
		{
			g.DrawRectangle(borderPen, Rectangle.Round(bounds));
			bounds.Inflate(0f - borderPen.Width, 0f - borderPen.Width);
		}
		RectangleF rectangleF = new RectangleF(new PointF(bounds.X, bounds.Y + (bounds.Height - (float)m_style.RepresentationSize.Height) / 2f), m_style.RepresentationSize);
		RectangleF rectangle = new RectangleF(bounds.X + rectangleF.Width + (float)m_style.Spacing, bounds.Y, bounds.Width - rectangleF.Width - (float)m_style.Spacing, bounds.Height);
		SizeF sizeF = g.MeasureString(m_text, Font, (int)rectangle.Width, format);
		rectangle.Y = ((rectangle.Height > sizeF.Height) ? (bounds.Y + bounds.Height / 2f - sizeF.Height / 2f) : rectangle.Y);
		switch (m_style.TextAlignment)
		{
		case VerticalAlignment.Top:
			format.LineAlignment = StringAlignment.Near;
			break;
		case VerticalAlignment.Center:
			format.LineAlignment = StringAlignment.Center;
			break;
		case VerticalAlignment.Bottom:
			format.LineAlignment = StringAlignment.Far;
			break;
		}
		if (m_style.ShowShadow)
		{
			GraphicsContainer cont = DrawingHelper.BeginTransform(g);
			m_isDrawingShadow = true;
			g.TranslateTransform(m_style.ShadowOffset.Width, m_style.ShadowOffset.Height);
			if (m_style.ShowIcon)
			{
				DrawIcon(g, rectangleF, m_style.Type);
			}
			using (Brush brush = GetTextBrush())
			{
				g.DrawString(m_text, Font, brush, rectangle, format);
			}
			m_isDrawingShadow = false;
			DrawingHelper.EndTransform(g, cont);
		}
		if (m_style.IconAlignment == LeftRightAlignment.Right)
		{
			rectangleF.X += rectangle.Width + (float)Spacing;
			rectangle.X -= rectangleF.Width + (float)Spacing;
		}
		using (Brush brush2 = GetTextBrush())
		{
			g.DrawString(m_text, Font, brush2, rectangle, format);
		}
		if (!m_style.ShowIcon)
		{
			return;
		}
		DrawIcon(g, rectangleF, m_style.Type);
		if (m_isDrawingShadow || !m_style.ShowSymbol)
		{
			return;
		}
		PointF center = ChartMath.GetCenter(rectangleF);
		using SolidBrush brush3 = new SolidBrush(Symbol.Color);
		using Pen pen = GetPen();
		RenderingHelper.DrawPointSymbol(g, Symbol.Shape, Symbol.Marker, Symbol.Size, Symbol.Offset, Symbol.ImageIndex, brush3, pen, m_style.ImageList, center, drawMarker: false);
	}

	public void DisposeLegendItem()
	{
		Border.Dispose();
		m_style.Dispose();
		m_style = null;
		if (c_defaultFont != null)
		{
			c_defaultFont.Dispose();
			c_defaultFont = null;
		}
	}

	public void Dispose()
	{
		m_legend = null;
		m_owner = null;
		if (m_children != null)
		{
			m_children.Changed -= OnChildrenChanged;
			m_children.Clear();
			m_children = null;
		}
	}

	private void RaiseDrawItemText(ChartLegendDrawItemTextEventArgs e)
	{
		if (m_textHandler != null)
		{
			m_textHandler(this, e);
		}
	}

	private void DrawInternal(Graphics g)
	{
		DrawInternal(g, m_text, isDrawIcon: true);
	}

	internal void DrawInternal(Graphics g, string text, bool isDrawIcon)
	{
		if (text != string.Empty)
		{
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags |= StringFormatFlags.NoClip;
			switch (m_style.TextAlignment)
			{
			case VerticalAlignment.Top:
				stringFormat.LineAlignment = StringAlignment.Near;
				break;
			case VerticalAlignment.Center:
				stringFormat.LineAlignment = StringAlignment.Center;
				break;
			case VerticalAlignment.Bottom:
				stringFormat.LineAlignment = StringAlignment.Far;
				break;
			}
			ChartLegendDrawItemTextEventArgs chartLegendDrawItemTextEventArgs = new ChartLegendDrawItemTextEventArgs(g, text, m_textRect);
			RaiseDrawItemText(chartLegendDrawItemTextEventArgs);
			if (!chartLegendDrawItemTextEventArgs.Handled)
			{
				using Brush brush = GetLegendTextBrush();
				g.DrawString(text, Font, brush, m_textRect, stringFormat);
			}
		}
		if (!(m_style.ShowIcon && isDrawIcon))
		{
			return;
		}
		DrawIcon(g, m_iconRect, m_style.Type);
		if (m_isDrawingShadow || !m_style.ShowSymbol)
		{
			return;
		}
		PointF center = ChartMath.GetCenter(m_iconRect);
		ChartSymbolShape chartSymbolShape = Symbol.Shape;
		Color color = Symbol.Color;
		if (chartSymbolShape == ChartSymbolShape.None && this is ChartSeriesLegendItem && ((ChartSeriesLegendItem)this).Series.Type == ChartSeriesType.Scatter)
		{
			color = ((m_style.Interior == null) ? color : m_style.Interior.BackColor);
			chartSymbolShape = ChartSymbolShape.Circle;
		}
		using SolidBrush brush2 = new SolidBrush(color);
		using Pen pen = GetPen();
		RenderingHelper.DrawPointSymbol(g, chartSymbolShape, Symbol.Marker, Symbol.Size, Symbol.Offset, Symbol.ImageIndex, brush2, pen, m_style.ImageList, center, drawMarker: false);
	}

	protected virtual BrushInfo GetBrushInfo()
	{
		if (!m_isDrawingShadow)
		{
			return m_style.Interior;
		}
		return new BrushInfo(m_style.ShadowColor);
	}

	protected virtual Pen GetPen()
	{
		if (!m_isDrawingShadow)
		{
			return m_style.Border.GdipPen.Clone() as Pen;
		}
		return new Pen(m_style.ShadowColor);
	}

	protected virtual Pen GetLinePen()
	{
		if (!m_isDrawingShadow)
		{
			return new Pen(m_style.Interior.BackColor, m_style.Border.Width);
		}
		return new Pen(m_style.ShadowColor, m_style.Border.Width);
	}

	protected virtual Brush GetTextBrush()
	{
		if (!m_isDrawingShadow)
		{
			return new SolidBrush(m_legend.ForeColor);
		}
		return new SolidBrush(m_style.ShadowColor);
	}

	protected virtual Brush GetLegendTextBrush()
	{
		if (!m_isDrawingShadow)
		{
			if (!m_isLegendTextColor)
			{
				return new SolidBrush(m_legend.ForeColor);
			}
			return new SolidBrush(TextColor);
		}
		return new SolidBrush(m_style.ShadowColor);
	}

	protected virtual void OnCheckedChanged(EventArgs args)
	{
		RaiseCheckedChanged(this, args);
	}

	private void OnChildrenChanged(ChartBaseList list, ChartListChangeArgs args)
	{
		if (args.NewItems != null)
		{
			object[] newItems = args.NewItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				ChartLegendItem obj = (ChartLegendItem)newItems[i];
				obj.ItemStyle.SetToLowerLevel(m_style);
				obj.ItemStyle.VisibleCheckBox = false;
				obj.SetLegend(m_legend);
				obj.SetOwner(m_owner);
			}
		}
		if (args.OldItems != null)
		{
			object[] newItems = args.OldItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				ChartLegendItem chartLegendItem = (ChartLegendItem)newItems[i];
				chartLegendItem.ItemStyle.BaseStyle = chartLegendItem.ItemStyle.BaseStyle.BaseStyle;
				chartLegendItem.SetLegend(null);
				chartLegendItem.SetOwner(null);
			}
		}
		RaiseChanged(this, EventArgs.Empty);
	}

	protected void RaiseChanged(object sender, EventArgs e)
	{
		if (this.Changed != null)
		{
			this.Changed(sender, e);
		}
	}

	protected void RaiseCheckedChanged(object sender, EventArgs args)
	{
		if (this.CheckedChanged != null)
		{
			this.CheckedChanged(sender, args);
		}
	}

	protected virtual void DrawIcon(Graphics g, RectangleF bounds, ChartLegendItemType shape)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		GraphicsPath graphicsPath = new GraphicsPath();
		switch (Type)
		{
		case ChartLegendItemType.None:
			flag = false;
			flag2 = false;
			flag3 = false;
			break;
		case ChartLegendItemType.Line:
			graphicsPath.AddLines(new PointF[4]
			{
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.X + bounds.Width / 3f, bounds.Top),
				new PointF(bounds.X + 2f * bounds.Width / 3f, bounds.Bottom),
				new PointF(bounds.Right, bounds.Top)
			});
			flag = true;
			break;
		case ChartLegendItemType.Rectangle:
			graphicsPath.AddRectangle(bounds);
			flag2 = true;
			break;
		case ChartLegendItemType.Spline:
			graphicsPath.AddCurve(new PointF[5]
			{
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.X + bounds.Width / 3f, bounds.Top),
				new PointF(bounds.X + 2f * bounds.Width / 3f, bounds.Top + bounds.Height / 2f),
				new PointF(bounds.Right, bounds.Top),
				new PointF(bounds.Right, bounds.Bottom)
			});
			flag = false;
			break;
		case ChartLegendItemType.Area:
			graphicsPath.AddPolygon(new PointF[5]
			{
				new PointF(bounds.Left, bounds.Bottom),
				new PointF(bounds.Left + bounds.Width / 3f, bounds.Top),
				new PointF(bounds.Left - bounds.Width / 3f, bounds.Top + bounds.Height / 2f),
				new PointF(bounds.Right, bounds.Top),
				new PointF(bounds.Right, bounds.Bottom)
			});
			flag2 = true;
			break;
		case ChartLegendItemType.PieSlice:
			graphicsPath.AddPie(bounds.X, bounds.Y, 2f * bounds.Width, 2f * bounds.Height, -180f, 90f);
			break;
		case ChartLegendItemType.Image:
			flag3 = true;
			break;
		case ChartLegendItemType.Circle:
			graphicsPath.AddEllipse(bounds);
			flag2 = true;
			break;
		case ChartLegendItemType.Diamond:
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
				new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom),
				new PointF(bounds.X, bounds.Y + bounds.Height / 2f)
			});
			flag2 = true;
			break;
		case ChartLegendItemType.Hexagon:
			graphicsPath.AddPolygon(new PointF[6]
			{
				new PointF(bounds.X + bounds.Width / 4f, bounds.Y),
				new PointF(bounds.X + bounds.Width * 0.75f, bounds.Y),
				new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width * 0.75f, bounds.Bottom),
				new PointF(bounds.X + bounds.Width / 4f, bounds.Bottom),
				new PointF(bounds.X, bounds.Y + bounds.Height / 2f)
			});
			flag2 = true;
			break;
		case ChartLegendItemType.Pentagon:
			graphicsPath.AddPolygon(new PointF[5]
			{
				new PointF(bounds.X + bounds.Width / 5f, bounds.Y),
				new PointF(bounds.Right - bounds.Width / 5f, bounds.Y),
				new PointF(bounds.Right, bounds.Y + bounds.Height * 0.6f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom),
				new PointF(bounds.X, bounds.Y + bounds.Height * 0.6f)
			});
			flag2 = true;
			break;
		case ChartLegendItemType.Triangle:
			graphicsPath.AddPolygon(new PointF[3]
			{
				new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.Right, bounds.Bottom)
			});
			flag2 = true;
			break;
		case ChartLegendItemType.InvertedTriangle:
			graphicsPath.AddPolygon(new PointF[3]
			{
				new PointF(bounds.X, bounds.Y),
				new PointF(bounds.Right, bounds.Y),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom)
			});
			flag2 = true;
			break;
		case ChartLegendItemType.Cross:
			graphicsPath.AddLine(bounds.Left, bounds.Top + bounds.Height / 2f, bounds.Right, bounds.Top + bounds.Height / 2f);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(bounds.Left + bounds.Width / 2f, bounds.Top, bounds.Left + bounds.Width / 2f, bounds.Bottom);
			graphicsPath.CloseFigure();
			flag = true;
			break;
		case ChartLegendItemType.SplineArea:
			graphicsPath.AddCurve(new PointF[5]
			{
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.X + bounds.Width / 3f, bounds.Top),
				new PointF(bounds.X + 2f * bounds.Width / 3f, bounds.Top + bounds.Height / 2f),
				new PointF(bounds.Right, bounds.Top),
				new PointF(bounds.Right, bounds.Bottom)
			});
			graphicsPath.CloseFigure();
			flag2 = true;
			break;
		case ChartLegendItemType.StraightLine:
			graphicsPath.AddLine(bounds.Left + bounds.Width / 8f, bounds.Y + bounds.Height / 2f, bounds.Right - bounds.Width / 8f, bounds.Y + bounds.Height / 2f);
			flag = true;
			break;
		}
		if (flag3 && Image != null)
		{
			g.DrawImage(Image, bounds);
		}
		if (flag2)
		{
			BrushPaint.FillPath(g, graphicsPath, GetBrushInfo());
			using Pen pen = GetPen();
			g.DrawPath(pen, graphicsPath);
		}
		if (flag)
		{
			using (Pen pen2 = GetLinePen())
			{
				g.DrawPath(pen2, graphicsPath);
			}
		}
	}
}
