using System;
using System.Collections.Generic;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartLegend : ChartDockControl, IChartLegend, IDisposable
{
	private ChartControl chart;

	private Size m_measuredSize = Size.Empty;

	private bool m_floatingAutoSize = true;

	private ChartLegendItem[] m_allItems;

	private bool m_needRefreshItems = true;

	private ChartLegendItemStyle m_baseItemStyle = new ChartLegendItemStyle();

	private ChartLegendItem[] m_customItems = new ChartLegendItem[0];

	private StringAlignment m_textAlignment = StringAlignment.Center;

	private static readonly StringFormat c_stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);

	private Color m_foreColor = Color.Black;

	public const string DefaultName = "";

	internal bool IsLegendOverlapping { get; set; }

	public BrushInfo BackInterior { get; set; }

	public string Text { get; set; }

	public ChartLegendItem[] Items
	{
		get
		{
			if (m_allItems == null || m_needRefreshItems)
			{
				if (m_allItems != null)
				{
					ChartLegendItem[] allItems = m_allItems;
					for (int i = 0; i < allItems.Length; i++)
					{
						allItems[i]?.SetLegend(null);
					}
				}
				m_allItems = GetAllItems();
				if (m_allItems != null)
				{
					ChartLegendItem[] allItems = m_allItems;
					for (int i = 0; i < allItems.Length; i++)
					{
						allItems[i]?.SetLegend(this);
					}
				}
				m_needRefreshItems = false;
			}
			return m_allItems;
		}
	}

	public ChartLegendItem[] CustomItems
	{
		get
		{
			return m_customItems;
		}
		set
		{
			if (m_customItems != value)
			{
				m_customItems = value;
				m_needRefreshItems = true;
			}
		}
	}

	private int VisibleItemsCount
	{
		get
		{
			int num = 0;
			ChartLegendItem[] items = Items;
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].Visible)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool ShowBorder { get; set; }

	public string Name { get; set; }

	public VerticalAlignment ItemsTextAligment { get; set; }

	public StringAlignment ItemsAlignment { get; set; }

	public ChartLegendRepresentationType RepresentationType { get; set; }

	public SizeF ItemsSize { get; set; }

	public Size ItemsShadowOffset { get; set; }

	public Color ItemsShadowColor { get; set; }

	public int Spacing { get; set; }

	public int RowsCount { get; set; }

	public int ColumnsCount { get; set; }

	public bool ShowSymbol { get; set; }

	public bool OnlyColumnsForFloating { get; set; }

	public bool FloatingAutoSize { get; set; }

	public bool ShowItemsShadow { get; set; }

	public bool SetDefSizeForCustom { get; set; }

	public Font Font { get; set; }

	public Color ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			m_foreColor = value;
		}
	}

	public LineInfo Border { get; set; }

	public override ChartOrientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value)
			{
				m_orientation = value;
			}
		}
	}

	public event LegendFilterItemsEventHandler FilterItems;

	private void WireSeriesCollection()
	{
		chart.Series.Changed += SeriesChanged;
	}

	private ChartLegendItem[] GetAllItems()
	{
		ChartLegendItemsCollection chartLegendItemsCollection = new ChartLegendItemsCollection();
		if (chart != null)
		{
			for (int i = 0; i < chart.Series.Count; i++)
			{
				ChartSeries chartSeries = chart.Series[i];
				if (!(chartSeries.LegendName == Name) || !chartSeries.Compatible)
				{
					continue;
				}
				if (chartSeries.BaseType == ChartSeriesBaseType.Single && !chart.ChartArea.DivideArea)
				{
					if (chartSeries.LegendItem.Children.Count > 0)
					{
						if (chartSeries.SortPoints)
						{
							int count = chartSeries.Points.Count;
							ChartSeriesLegendItem[] array = new ChartSeriesLegendItem[chartSeries.Points.Count];
							double[] array2 = new double[count];
							double[] array3 = new double[count];
							string[] array4 = new string[count];
							for (int j = 0; j < count; j++)
							{
								array[j] = (ChartSeriesLegendItem)chartSeries.LegendItem.Children[j];
								if (chartSeries.SortBy == ChartSeriesSortingType.Y)
								{
									array2[j] = chartSeries.Points[j].YValues[0];
								}
								if (chartSeries.ActualXAxis.ValueType == ChartValueType.Category)
								{
									array4[j] = chartSeries.Points[j].Category;
								}
								else
								{
									array3[j] = chartSeries.Points[j].X;
								}
							}
							if (chartSeries.SortBy == ChartSeriesSortingType.X)
							{
								if (chartSeries.ActualXAxis.ValueType != ChartValueType.Category)
								{
									Array.Sort(array3, array);
									if (chartSeries.SortOrder == ChartSeriesSortingOrder.Descending)
									{
										Array.Reverse(array);
									}
								}
								else
								{
									Array.Sort(array4, array);
									if (chartSeries.SortOrder == ChartSeriesSortingOrder.Descending)
									{
										Array.Reverse(array);
									}
								}
							}
							else
							{
								Array.Sort(array2, array);
								if (chartSeries.SortOrder == ChartSeriesSortingOrder.Descending)
								{
									Array.Reverse(array);
								}
							}
							foreach (ChartSeriesLegendItem chartSeriesLegendItem in array)
							{
								chartSeriesLegendItem.ItemStyle.BaseStyle = m_baseItemStyle;
								chartSeriesLegendItem.DrawSeriesIcon = RepresentationType == ChartLegendRepresentationType.SeriesType;
								chartLegendItemsCollection.Add(chartSeriesLegendItem);
							}
							break;
						}
						foreach (ChartSeriesLegendItem child in chartSeries.LegendItem.Children)
						{
							child.ItemStyle.BaseStyle = m_baseItemStyle;
							child.DrawSeriesIcon = RepresentationType == ChartLegendRepresentationType.SeriesType;
							chartLegendItemsCollection.Add(child);
						}
					}
					else
					{
						chartSeries.LegendItem.ItemStyle.BaseStyle = m_baseItemStyle;
						chartSeries.LegendItem.DrawSeriesIcon = RepresentationType == ChartLegendRepresentationType.SeriesType;
						chartLegendItemsCollection.Add(chartSeries.LegendItem);
					}
					break;
				}
				chartSeries.LegendItem.ItemStyle.BaseStyle = m_baseItemStyle;
				chartSeries.LegendItem.DrawSeriesIcon = RepresentationType == ChartLegendRepresentationType.SeriesType;
				chartLegendItemsCollection.Add(chartSeries.LegendItem);
			}
			for (int l = 0; l < chart.Series.Count; l++)
			{
				ChartSeries chartSeries2 = chart.Series[l];
				for (int m = 0; m < chartSeries2.Trendlines.Count; m++)
				{
					if (chartSeries2.BaseType == ChartSeriesBaseType.Circular)
					{
						break;
					}
					if (chartSeries2.BaseType == ChartSeriesBaseType.Single)
					{
						break;
					}
					if (chart.Series3D)
					{
						break;
					}
					ChartTrendlineLegendItem chartTrendlineLegendItem = new ChartTrendlineLegendItem(chartSeries2, chartSeries2.Trendlines[m]);
					chartTrendlineLegendItem.Refresh(useSeriesStyle: false);
					chartTrendlineLegendItem.ItemStyle.BaseStyle = m_baseItemStyle;
					chartTrendlineLegendItem.Interior = ((chartSeries2.Trendlines[m].Color == Color.Black) ? new BrushInfo(GradientStyle.None, Color.Black, Color.Black) : new BrushInfo(GradientStyle.None, chartSeries2.Trendlines[m].Color, chartSeries2.Trendlines[m].Color));
					chartTrendlineLegendItem.Text = ((chartSeries2.Trendlines[m].Name == "Trendline") ? ("Trendline" + m) : chartSeries2.Trendlines[m].Name);
					chartTrendlineLegendItem.Type = ChartLegendItemType.StraightLine;
					chartLegendItemsCollection.Add(chartTrendlineLegendItem);
				}
			}
		}
		if (m_customItems != null)
		{
			for (int n = 0; n < m_customItems.Length; n++)
			{
				m_customItems[n].ItemStyle.BaseStyle = m_baseItemStyle;
				chartLegendItemsCollection.Add(m_customItems[n]);
			}
		}
		ChartLegendItem[] array5 = chartLegendItemsCollection.ToArray();
		ChartLegendItem[] array6 = array5;
		for (int num = 0; num < array6.Length; num++)
		{
			array6[num].ItemStyle.BaseStyle = m_baseItemStyle;
		}
		return array5;
	}

	public override void Render(Graphics g)
	{
		if (base.Visible)
		{
			GraphicsState state = g.Save();
			g.TranslateTransform(base.Location.X, base.Location.Y);
			Draw(g);
			g.ResetTransform();
			g.Restore(state);
		}
	}

	internal void Draw(Graphics g)
	{
		int visibleItemsCount = VisibleItemsCount;
		base.ClientRectangle = new Rectangle(0, 0, base.Width, base.Height);
		BrushPaint.FillRectangle(g, base.ClientRectangle, BackInterior);
		SizeF sizeF = g.MeasureString(Text, Font, base.Width);
		RectangleF rectangle = new RectangleF(0f, 0f, base.Width, sizeF.Height);
		RectangleF contentRect = new RectangleF(0f, rectangle.Bottom, base.Width, (float)base.Height - sizeF.Height);
		ChartLegendItem[] array = Items;
		if (visibleItemsCount > 0)
		{
			int num = 0;
			bool flag = Position == ChartDock.Floating && OnlyColumnsForFloating;
			bool flag2 = Orientation == ChartOrientation.Vertical;
			int num2 = Math.Min(visibleItemsCount, ColumnsCount);
			int num3 = Math.Min(visibleItemsCount, flag ? ColumnsCount : RowsCount);
			if (flag2)
			{
				num3 = (int)Math.Ceiling((float)visibleItemsCount / (float)num2);
			}
			else
			{
				num2 = (int)Math.Ceiling((float)visibleItemsCount / (float)num3);
			}
			contentRect.Inflate(-0.5f * (float)Spacing, -0.5f * (float)Spacing);
			SizeF cellSize = new SizeF(contentRect.Width / (float)num2, contentRect.Height / (float)num3);
			bool flag3 = false;
			bool flag4 = false;
			ChartSeriesType chartSeriesType = ((chart.Series.VisibleList.Count > 0) ? (chart.Series.VisibleList[0] as ChartSeries).Type : ChartSeriesType.Area);
			Dictionary<int, int> dictionary = new Dictionary<int, int>(array.Length);
			int num4 = 0;
			int key = 0;
			foreach (ChartSeries visible in chart.Series.VisibleList)
			{
				flag3 = ((chart.IsRadar || visible.Type == ChartSeriesType.Bar || visible.Type == ChartSeriesType.Scatter || visible.Type == ChartSeriesType.Line || visible.Type == ChartSeriesType.Spline || (visible.ParentChart != null && !visible.ParentChart.Series3D && visible.Type == ChartSeriesType.Area && (Position == ChartDock.Bottom || Position == ChartDock.Top)) || ((visible.Type == ChartSeriesType.StackingColumn || visible.Type == ChartSeriesType.StackingColumn100 || visible.Type == ChartSeriesType.StackingArea || visible.Type == ChartSeriesType.StackingArea100) && (Position == ChartDock.Left || Position == ChartDock.Right))) ? true : false);
				if (!flag4 && chartSeriesType != visible.Type)
				{
					flag4 = true;
				}
				if (chartSeriesType == visible.Type)
				{
					if (!dictionary.ContainsKey(key))
					{
						key = num4;
						dictionary.Add(key, num4);
					}
					dictionary[key] = num4;
				}
				else if (!dictionary.ContainsKey(num4))
				{
					key = num4;
					dictionary.Add(key, num4);
				}
				num4++;
				chartSeriesType = visible.Type;
			}
			if (flag4)
			{
				if (!flag3)
				{
					ChartLegendItem[] array2 = new ChartLegendItem[array.Length];
					int num5 = 0;
					foreach (KeyValuePair<int, int> item in dictionary)
					{
						int num6 = item.Value;
						while (num6 >= item.Key)
						{
							if (num5 < array.Length && num6 < array.Length)
							{
								array2[num5] = array[num6];
								num5++;
								num6--;
								continue;
							}
							array2 = null;
							break;
						}
					}
					if (array2 != null)
					{
						array = array2;
					}
				}
				Array.Reverse(array);
			}
			else if (flag3)
			{
				Array.Reverse(array);
			}
			RectangleF itemRect = RectangleF.Empty;
			float width = 0f;
			float itemSizeWidth = ItemsSize.Width + (float)Spacing;
			ChartLegendItem[] array3 = array;
			foreach (ChartLegendItem chartLegendItem in array3)
			{
				if (!chartLegendItem.Visible)
				{
					continue;
				}
				if (chartLegendItem.Text.Contains("\n") && chartLegendItem.Text.Split('\n').Length > 1)
				{
					string[] array4 = chartLegendItem.Text.Split('\n');
					num3 += array4.Length - 1;
					int num7 = num;
					for (int j = 0; j < array4.Length; j++)
					{
						int ci = num / num3;
						int ri = num % num3;
						SizeF itemSize = chartLegendItem.Measure(g, array4[j]);
						itemRect = CalculateLegendRectangle(itemRect, cellSize, ci, ri, num3, contentRect, width, itemSize, itemSizeWidth, chartLegendItem);
						bool isDrawIcon = j == 0;
						chartLegendItem.Draw(g, array4[j], isDrawIcon);
						width = itemRect.Right;
						num++;
						if (j == array4.Length - 1 && !flag2)
						{
							num = ++num7;
						}
					}
				}
				else
				{
					int ci2 = (flag2 ? (num / num3) : (num % num2));
					int ri2 = (flag2 ? (num % num3) : (num / num2));
					SizeF itemSize2 = chartLegendItem.Measure(g);
					itemRect = CalculateLegendRectangle(itemRect, cellSize, ci2, ri2, num3, contentRect, width, itemSize2, itemSizeWidth, chartLegendItem);
					chartLegendItem.Draw(g);
					width = itemRect.Right;
					num++;
				}
			}
		}
		if (Text != null && Text != string.Empty)
		{
			StringFormat stringFormat = new StringFormat(StringFormatFlags.NoClip);
			stringFormat.Alignment = m_textAlignment;
			using (SolidBrush brush = new SolidBrush(ForeColor))
			{
				g.DrawString(Text, Font, brush, rectangle, stringFormat);
			}
			if (ShowBorder)
			{
				g.DrawLine(Border.Pen, 0f, sizeF.Height, base.Width, sizeF.Height);
			}
		}
		if (ShowBorder)
		{
			g.DrawRectangle(Border.Pen, 0f, 0f, base.Width - 1, base.Height - 1);
		}
	}

	internal RectangleF CalculateLegendRectangle(RectangleF itemRect, SizeF cellSize, int ci, int ri, int rows, RectangleF contentRect, float width, SizeF itemSize, float itemSizeWidth, ChartLegendItem item)
	{
		if (rows == 1)
		{
			itemRect = new RectangleF(contentRect.Left + width + 0.5f * (float)Spacing, contentRect.Top + (float)ri * cellSize.Height, itemSize.Width, cellSize.Height);
			itemRect.Inflate(0.5f * (float)Spacing, 0.5f * (float)Spacing);
		}
		else if (RowsCount > 1)
		{
			itemRect = new RectangleF(contentRect.Left + (float)ci * itemSizeWidth, contentRect.Top + (float)ri * ItemsSize.Height, itemSizeWidth, ItemsSize.Height);
			itemRect.Inflate(-0.5f * (float)Spacing, -0.5f * (float)Spacing);
		}
		else
		{
			itemRect = new RectangleF(contentRect.Left + (float)ci * cellSize.Width, contentRect.Top + (float)ri * cellSize.Height, cellSize.Width, cellSize.Height);
			itemRect.Inflate(-0.5f * (float)Spacing, -0.5f * (float)Spacing);
		}
		switch (ItemsAlignment)
		{
		case StringAlignment.Center:
			itemRect.X += (itemRect.Width - itemSize.Width) / 2f;
			break;
		case StringAlignment.Far:
			itemRect.X += itemRect.Width - itemSize.Width;
			break;
		}
		item.Arrange(itemRect);
		return itemRect;
	}

	private void UnWireSeriesCollection()
	{
		if (chart != null)
		{
			chart.Series.Changed -= SeriesChanged;
		}
	}

	private void SeriesChanged(object sender, ChartSeriesCollectionChangedEventArgs e)
	{
		m_needRefreshItems = true;
	}

	public override SizeF Measure(Graphics g, SizeF size)
	{
		m_measuredSize = Size.Ceiling(MeasureItems(g, size.ToSize()));
		if (m_floatingAutoSize || Position != ChartDock.Floating)
		{
			base.Size = m_measuredSize;
		}
		else
		{
			base.Size = new Size(Math.Max(m_measuredSize.Width, base.Size.Width), Math.Max(m_measuredSize.Height, base.Size.Height));
		}
		return base.Size;
	}

	private SizeF MeasureItems(Graphics g, Size maxSize)
	{
		SizeF empty = SizeF.Empty;
		SizeF sizeF = g.MeasureString(Text, Font, maxSize.Width, c_stringFormat);
		int visibleItemsCount = VisibleItemsCount;
		float num = 0f;
		ChartLegendItem[] items = Items;
		foreach (ChartLegendItem chartLegendItem in items)
		{
			if (chartLegendItem.Visible)
			{
				SizeF sizeF2 = chartLegendItem.Measure(g);
				num += sizeF2.Width + (float)Spacing;
				empty.Width = Math.Max(sizeF2.Width, empty.Width);
				empty.Height = Math.Max(sizeF2.Height, empty.Height);
			}
		}
		if (visibleItemsCount > 0)
		{
			bool flag = Position == ChartDock.Floating && OnlyColumnsForFloating;
			int num2 = Math.Min(visibleItemsCount, ColumnsCount);
			int num3 = Math.Min(visibleItemsCount, flag ? ColumnsCount : RowsCount);
			if (Orientation == ChartOrientation.Vertical)
			{
				num3 = (int)Math.Ceiling((float)visibleItemsCount / (float)num2);
			}
			else
			{
				num2 = (int)Math.Ceiling((float)visibleItemsCount / (float)num3);
			}
			empty = ((!((float)chart.Width > num) || Orientation == ChartOrientation.Vertical || num3 != 1) ? new SizeF((empty.Width + (float)Spacing) * (float)num2 + (float)Spacing, (empty.Height + 5f) * (float)num3 + 5f) : new SizeF(num + (float)Spacing, (empty.Height + 5f) * (float)num3 + 5f));
		}
		else
		{
			empty = g.MeasureString("", Font);
			empty = new SizeF(empty.Width + (float)(2 * Spacing), empty.Height + (float)(2 * Spacing));
		}
		return new SizeF(Math.Min(maxSize.Width, Math.Max(sizeF.Width, empty.Width)), Math.Min(maxSize.Height, empty.Height + sizeF.Height));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (chart != null)
		{
			UnWireSeriesCollection();
			if (chart.Legends != null)
			{
				chart.Legends.Remove(this);
			}
			chart = null;
		}
		if (m_baseItemStyle != null)
		{
			m_baseItemStyle.Clear();
			m_baseItemStyle = null;
		}
		if (m_customItems != null)
		{
			for (int i = 0; i < m_customItems.Length; i++)
			{
				m_customItems[i].Dispose();
				m_customItems[i] = null;
			}
			m_customItems = null;
		}
		if (m_allItems != null)
		{
			for (int j = 0; j < m_allItems.Length; j++)
			{
				m_allItems[j].Dispose();
				m_allItems[j] = null;
			}
			m_allItems = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public ChartLegend(ChartControl chart)
	{
		this.chart = chart;
		base.Visible = true;
		Behavior = ChartDockingFlags.Dockable;
		Alignment = ChartAlignment.Center;
		Position = ChartDock.Left;
		base.BackColor = Color.Transparent;
		Font = new Font("Verdana", 8f);
		Border = new LineInfo();
		Text = "";
		ColumnsCount = 1;
		RowsCount = 1;
		Spacing = 5;
		if (chart != null)
		{
			WireSeriesCollection();
		}
	}
}
