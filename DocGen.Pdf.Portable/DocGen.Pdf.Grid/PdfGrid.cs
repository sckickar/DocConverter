using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Tables;

namespace DocGen.Pdf.Grid;

public class PdfGrid : PdfLayoutElement
{
	private PdfGridHeaderCollection m_headers;

	private PdfGridRowCollection m_rows;

	private object m_dataSource;

	private string m_dataMember;

	private PdfDataSource m_dsParser;

	private PdfGridStyle m_style;

	private PdfGridColumnCollection m_columns;

	private bool m_bRepeatHeader;

	private SizeF m_size = SizeF.Empty;

	private bool m_breakRow = true;

	private bool m_isChildGrid;

	private PdfGridCell m_parentCell;

	private float m_initialWidth;

	private PdfLayoutFormat m_layoutFormat;

	internal bool isComplete;

	internal bool isWidthSet;

	internal int parentCellIndex;

	internal bool isDrawn;

	internal float m_rowLayoutBoundswidth;

	internal List<int> m_listOfNavigatePages = new List<int>();

	internal bool m_isRearranged;

	private bool m_headerRow = true;

	private bool m_bandedRow = true;

	private bool m_bandedColumn;

	private bool m_totalRow;

	private bool m_firstColumn;

	private bool m_lastColumn;

	internal RectangleF m_gridLocation;

	private bool m_isPageWidth;

	internal bool isBuiltinStyle;

	internal PdfGridBuiltinStyle m_gridBuiltinStyle;

	internal bool isSignleGrid = true;

	internal bool m_hasColumnSpan;

	internal bool m_hasRowSpanSpan;

	internal bool m_hasHTMLText;

	internal float parentGridWidth;

	public PdfGridHeaderCollection Headers
	{
		get
		{
			if (m_headers == null)
			{
				m_headers = new PdfGridHeaderCollection(this);
			}
			return m_headers;
		}
	}

	public PdfGridRowCollection Rows
	{
		get
		{
			if (m_rows == null)
			{
				m_rows = new PdfGridRowCollection(this);
			}
			return m_rows;
		}
	}

	public object DataSource
	{
		get
		{
			return m_dataSource;
		}
		set
		{
			if (value != null && value != m_dataSource)
			{
				m_dataSource = value;
				Columns.Clear();
				SetDataSource();
			}
		}
	}

	public string DataMember
	{
		get
		{
			return m_dataMember;
		}
		set
		{
			if (value != null || m_dataMember != value)
			{
				m_dataMember = value;
				Columns.Clear();
				SetDataSource();
			}
		}
	}

	public PdfGridStyle Style
	{
		get
		{
			if (m_style == null)
			{
				m_style = new PdfGridStyle();
			}
			return m_style;
		}
		set
		{
			m_style = value;
		}
	}

	internal PdfGridRow LastRow
	{
		get
		{
			if (Rows.Count > 0)
			{
				return Rows[Rows.Count - 1];
			}
			return null;
		}
	}

	public PdfGridColumnCollection Columns
	{
		get
		{
			if (m_columns == null)
			{
				m_columns = new PdfGridColumnCollection(this);
			}
			return m_columns;
		}
	}

	public bool RepeatHeader
	{
		get
		{
			return m_bRepeatHeader;
		}
		set
		{
			m_bRepeatHeader = value;
		}
	}

	internal SizeF Size
	{
		get
		{
			if (m_size == SizeF.Empty)
			{
				m_size = Measure();
			}
			return m_size;
		}
	}

	public bool AllowRowBreakAcrossPages
	{
		get
		{
			return m_breakRow;
		}
		set
		{
			m_breakRow = value;
		}
	}

	internal bool IsChildGrid
	{
		get
		{
			return m_isChildGrid;
		}
		set
		{
			m_isChildGrid = value;
		}
	}

	internal PdfGridCell ParentCell
	{
		get
		{
			return m_parentCell;
		}
		set
		{
			m_parentCell = value;
		}
	}

	internal PdfLayoutFormat LayoutFormat
	{
		get
		{
			return m_layoutFormat;
		}
		set
		{
			m_layoutFormat = value;
		}
	}

	internal bool RaiseBeginCellLayout => this.BeginCellLayout != null;

	internal bool RaiseEndCellLayout => this.EndCellLayout != null;

	internal bool IsPageWidth
	{
		get
		{
			return m_isPageWidth;
		}
		set
		{
			m_isPageWidth = value;
		}
	}

	internal float InitialWidth
	{
		get
		{
			return m_initialWidth;
		}
		set
		{
			m_initialWidth = value;
		}
	}

	public event PdfGridBeginCellLayoutEventHandler BeginCellLayout;

	public event PdfGridEndCellLayoutEventHandler EndCellLayout;

	public void Draw(PdfGraphics graphics, PointF location, float width)
	{
		Draw(graphics, location.X, location.Y, width);
	}

	public void Draw(PdfGraphics graphics, float x, float y, float width)
	{
		InitialWidth = width;
		isWidthSet = true;
		RectangleF bounds = new RectangleF(x, y, width, 0f);
		Draw(graphics, bounds);
	}

	internal PdfGrid Clone(PdfGrid grid)
	{
		return (PdfGrid)grid.MemberwiseClone();
	}

	public void Draw(PdfGraphics graphics, RectangleF bounds)
	{
		SetSpan();
		InitialWidth = bounds.Width;
		isWidthSet = true;
		new PdfGridLayouter(this).Layout(graphics, bounds);
		isComplete = true;
	}

	public new PdfGridLayoutResult Draw(PdfPage page, PointF location)
	{
		InitialWidth = page.Graphics.ClientSize.Width;
		PdfLayoutResult pdfLayoutResult = base.Draw(page, location);
		isComplete = true;
		return (PdfGridLayoutResult)pdfLayoutResult;
	}

	public PdfGridLayoutResult Draw(PdfPage page, PointF location, PdfGridLayoutFormat format)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		InitialWidth = page.Graphics.ClientSize.Width;
		PdfLayoutResult pdfLayoutResult = Draw(page, location, (PdfLayoutFormat)format);
		isComplete = true;
		return (PdfGridLayoutResult)pdfLayoutResult;
	}

	public new PdfGridLayoutResult Draw(PdfPage page, RectangleF bounds)
	{
		InitialWidth = bounds.Width;
		isWidthSet = true;
		PdfLayoutResult pdfLayoutResult = base.Draw(page, bounds);
		isComplete = true;
		return (PdfGridLayoutResult)pdfLayoutResult;
	}

	public PdfGridLayoutResult Draw(PdfPage page, RectangleF bounds, PdfGridLayoutFormat format)
	{
		InitialWidth = bounds.Width;
		isWidthSet = true;
		PdfLayoutResult pdfLayoutResult = Draw(page, bounds, (PdfLayoutFormat)format);
		isComplete = true;
		return (PdfGridLayoutResult)pdfLayoutResult;
	}

	public new PdfGridLayoutResult Draw(PdfPage page, float x, float y)
	{
		InitialWidth = page.Graphics.ClientSize.Width;
		PdfLayoutResult pdfLayoutResult = base.Draw(page, x, y);
		isComplete = true;
		return (PdfGridLayoutResult)pdfLayoutResult;
	}

	public PdfGridLayoutResult Draw(PdfPage page, float x, float y, PdfGridLayoutFormat format)
	{
		InitialWidth = page.Graphics.ClientSize.Width;
		PdfLayoutResult pdfLayoutResult = Draw(page, x, y, (PdfLayoutFormat)format);
		isComplete = true;
		return (PdfGridLayoutResult)pdfLayoutResult;
	}

	public PdfGridLayoutResult Draw(PdfPage page, float x, float y, float width)
	{
		return Draw(page, x, y, width, null);
	}

	public PdfGridLayoutResult Draw(PdfPage page, float x, float y, float width, PdfGridLayoutFormat format)
	{
		RectangleF layoutRectangle = new RectangleF(x, y, width, 0f);
		InitialWidth = layoutRectangle.Width;
		isWidthSet = true;
		return (PdfGridLayoutResult)Draw(page, layoutRectangle, (PdfLayoutFormat)format);
	}

	protected override PdfLayoutResult Layout(PdfLayoutParams param)
	{
		if (param.Bounds.Width < 0f)
		{
			throw new ArgumentOutOfRangeException("Width");
		}
		SetSpan();
		m_layoutFormat = param.Format;
		m_gridLocation = param.Bounds;
		PdfGridLayoutResult result = (PdfGridLayoutResult)new PdfGridLayouter(this).Layout(param);
		Dispose();
		return result;
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		SetSpan();
		new PdfGridLayouter(this).Layout(graphics, PointF.Empty);
		isComplete = true;
	}

	internal void SetSpan()
	{
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int i = 0;
		for (int count = Headers.Count; i < count; i++)
		{
			PdfGridRow pdfGridRow = Headers[i];
			num3 = 0;
			int j = 0;
			for (int count2 = pdfGridRow.Cells.Count; j < count2; j++)
			{
				PdfGridCell pdfGridCell = pdfGridRow.Cells[j];
				num3 = Math.Max(num3, pdfGridCell.RowSpan);
				if (pdfGridCell.IsCellMergeContinue || pdfGridCell.IsRowMergeContinue || (pdfGridCell.ColumnSpan <= 1 && pdfGridCell.RowSpan <= 1))
				{
					continue;
				}
				if (pdfGridCell.ColumnSpan + j > pdfGridRow.Cells.Count)
				{
					throw new ArgumentException($"Invalid span specified at row {j.ToString()} column {i.ToString()}");
				}
				if (pdfGridCell.RowSpan + i > Headers.Count)
				{
					throw new ArgumentException($"Invalid span specified at Header {j.ToString()} column {i.ToString()}");
				}
				if (pdfGridCell.ColumnSpan > 1 && pdfGridCell.RowSpan > 1)
				{
					int num4 = pdfGridCell.ColumnSpan;
					num = pdfGridCell.RowSpan;
					int num5 = j;
					num2 = i;
					pdfGridCell.IsCellMergeStart = true;
					pdfGridCell.IsRowMergeStart = true;
					while (num4 > 1)
					{
						num5++;
						pdfGridRow.Cells[num5].IsCellMergeContinue = true;
						pdfGridRow.Cells[num5].IsRowMergeContinue = true;
						pdfGridRow.Cells[num5].RowSpan = num;
						num4--;
					}
					num5 = j;
					num4 = pdfGridCell.ColumnSpan;
					while (num > 1)
					{
						num2++;
						Headers[num2].Cells[j].IsRowMergeContinue = true;
						Headers[num2].Cells[num5].IsRowMergeContinue = true;
						num--;
						while (num4 > 1)
						{
							num5++;
							Headers[num2].Cells[num5].IsCellMergeContinue = true;
							Headers[num2].Cells[num5].IsRowMergeContinue = true;
							num4--;
						}
						num4 = pdfGridCell.ColumnSpan;
						num5 = j;
					}
				}
				else if (pdfGridCell.ColumnSpan > 1 && pdfGridCell.RowSpan == 1)
				{
					int num4 = pdfGridCell.ColumnSpan;
					int num5 = j;
					pdfGridCell.IsCellMergeStart = true;
					while (num4 > 1)
					{
						num5++;
						pdfGridRow.Cells[num5].IsCellMergeContinue = true;
						num4--;
					}
				}
				else if (pdfGridCell.ColumnSpan == 1 && pdfGridCell.RowSpan > 1)
				{
					num = pdfGridCell.RowSpan;
					num2 = i;
					while (num > 1)
					{
						num2++;
						Headers[num2].Cells[j].IsRowMergeContinue = true;
						num--;
					}
				}
			}
			pdfGridRow.maximumRowSpan = num3;
		}
		num = 1;
		num2 = 0;
		if (!m_hasColumnSpan && !m_hasRowSpanSpan)
		{
			return;
		}
		int k = 0;
		for (int count3 = Rows.Count; k < count3; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k];
			num3 = 0;
			int l = 0;
			for (int count4 = pdfGridRow2.Cells.Count; l < count4; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l];
				num3 = Math.Max(num3, pdfGridCell2.RowSpan);
				if (pdfGridCell2.IsCellMergeContinue || pdfGridCell2.IsRowMergeContinue || (pdfGridCell2.ColumnSpan <= 1 && pdfGridCell2.RowSpan <= 1))
				{
					continue;
				}
				if (pdfGridCell2.ColumnSpan + l > pdfGridRow2.Cells.Count)
				{
					throw new ArgumentException($"Invalid span specified at row {l.ToString()} column {k.ToString()}");
				}
				if (pdfGridCell2.RowSpan + k > Rows.Count)
				{
					throw new ArgumentException($"Invalid span specified at row {l.ToString()} column {k.ToString()}");
				}
				if (pdfGridCell2.ColumnSpan > 1 && pdfGridCell2.RowSpan > 1)
				{
					int num4 = pdfGridCell2.ColumnSpan;
					num = pdfGridCell2.RowSpan;
					int num5 = l;
					num2 = k;
					pdfGridCell2.IsCellMergeStart = true;
					pdfGridCell2.IsRowMergeStart = true;
					while (num4 > 1)
					{
						num5++;
						pdfGridRow2.Cells[num5].IsCellMergeContinue = true;
						pdfGridRow2.Cells[num5].IsRowMergeContinue = true;
						num4--;
					}
					num5 = l;
					num4 = pdfGridCell2.ColumnSpan;
					while (num > 1)
					{
						num2++;
						Rows[num2].Cells[l].IsRowMergeContinue = true;
						Rows[num2].Cells[num5].IsRowMergeContinue = true;
						num--;
						while (num4 > 1)
						{
							num5++;
							Rows[num2].Cells[num5].IsCellMergeContinue = true;
							Rows[num2].Cells[num5].IsRowMergeContinue = true;
							num4--;
						}
						num4 = pdfGridCell2.ColumnSpan;
						num5 = l;
					}
				}
				else if (pdfGridCell2.ColumnSpan > 1 && pdfGridCell2.RowSpan == 1)
				{
					int num4 = pdfGridCell2.ColumnSpan;
					int num5 = l;
					pdfGridCell2.IsCellMergeStart = true;
					while (num4 > 1)
					{
						num5++;
						pdfGridRow2.Cells[num5].IsCellMergeContinue = true;
						num4--;
					}
				}
				else if (pdfGridCell2.ColumnSpan == 1 && pdfGridCell2.RowSpan > 1)
				{
					num = pdfGridCell2.RowSpan;
					num2 = k;
					while (num > 1)
					{
						num2++;
						Rows[num2].Cells[l].IsRowMergeContinue = true;
						num--;
					}
				}
			}
			pdfGridRow2.maximumRowSpan = num3;
		}
	}

	private SizeF Measure()
	{
		float num = 0f;
		float width = Columns.Width;
		foreach (PdfGridRow header in Headers)
		{
			num += header.Height;
		}
		foreach (PdfGridRow row in Rows)
		{
			num += row.Height;
		}
		return new SizeF(width, num);
	}

	internal void OnBeginCellLayout(PdfGridBeginCellLayoutEventArgs args)
	{
		if (RaiseBeginCellLayout)
		{
			this.BeginCellLayout(this, args);
		}
	}

	internal void OnEndCellLayout(PdfGridEndCellLayoutEventArgs args)
	{
		if (RaiseEndCellLayout)
		{
			this.EndCellLayout(this, args);
		}
	}

	private void Dispose()
	{
		foreach (PdfGridRow header in Headers)
		{
			if (header.RowBreakHeight > 0f)
			{
				foreach (PdfGridCell cell in header.Cells)
				{
					cell.FinishedDrawingCell = true;
					cell.RemainingString = null;
				}
			}
			header.RowBreakHeight = 0f;
			header.rowBreakHeight = 0f;
			header.isRowBreaksNextPage = false;
			header.isPageBreakRowSpanApplied = false;
		}
		foreach (PdfGridRow row in Rows)
		{
			foreach (PdfGridCell cell2 in row.Cells)
			{
				cell2.FinishedDrawingCell = true;
				cell2.RemainingString = null;
			}
			row.RowBreakHeight = 0f;
			row.rowBreakHeight = 0f;
			row.isRowBreaksNextPage = false;
			row.isPageBreakRowSpanApplied = false;
		}
	}

	public void ApplyBuiltinStyle(PdfGridBuiltinStyle gridStyle)
	{
		isBuiltinStyle = true;
		m_gridBuiltinStyle = gridStyle;
	}

	internal void ApplyBuiltinStyles(PdfGridBuiltinStyle gridStyle)
	{
		switch (gridStyle)
		{
		case PdfGridBuiltinStyle.TableGrid:
			ApplyTableGridLight(Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.TableGridLight:
			ApplyTableGridLight(Color.FromArgb(255, 191, 191, 191));
			break;
		case PdfGridBuiltinStyle.PlainTable1:
			ApplyPlainTable1(Color.FromArgb(255, 191, 191, 191), Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfGridBuiltinStyle.PlainTable2:
			ApplyPlainTable2(Color.FromArgb(255, 127, 127, 127));
			break;
		case PdfGridBuiltinStyle.PlainTable3:
			ApplyPlainTable3(Color.FromArgb(255, 127, 127, 127), Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfGridBuiltinStyle.PlainTable4:
			ApplyPlainTable4(Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfGridBuiltinStyle.PlainTable5:
			ApplyPlainTable5(Color.FromArgb(255, 127, 127, 127), Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfGridBuiltinStyle.GridTable1Light:
			ApplyGridTable1Light(Color.FromArgb(255, 153, 153, 153), Color.FromArgb(255, 102, 102, 102));
			break;
		case PdfGridBuiltinStyle.GridTable1LightAccent1:
			ApplyGridTable1Light(Color.FromArgb(255, 189, 214, 238), Color.FromArgb(255, 156, 194, 229));
			break;
		case PdfGridBuiltinStyle.GridTable1LightAccent2:
			ApplyGridTable1Light(Color.FromArgb(255, 247, 202, 172), Color.FromArgb(255, 244, 176, 131));
			break;
		case PdfGridBuiltinStyle.GridTable1LightAccent3:
			ApplyGridTable1Light(Color.FromArgb(255, 219, 219, 219), Color.FromArgb(255, 201, 201, 201));
			break;
		case PdfGridBuiltinStyle.GridTable1LightAccent4:
			ApplyGridTable1Light(Color.FromArgb(255, 255, 229, 153), Color.FromArgb(255, 255, 217, 102));
			break;
		case PdfGridBuiltinStyle.GridTable1LightAccent5:
			ApplyGridTable1Light(Color.FromArgb(255, 180, 198, 231), Color.FromArgb(255, 142, 170, 219));
			break;
		case PdfGridBuiltinStyle.GridTable1LightAccent6:
			ApplyGridTable1Light(Color.FromArgb(255, 192, 224, 179), Color.FromArgb(255, 168, 208, 141));
			break;
		case PdfGridBuiltinStyle.GridTable2:
			ApplyGridTable2(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfGridBuiltinStyle.GridTable2Accent1:
			ApplyGridTable2(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfGridBuiltinStyle.GridTable2Accent2:
			ApplyGridTable2(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfGridBuiltinStyle.GridTable2Accent3:
			ApplyGridTable2(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfGridBuiltinStyle.GridTable2Accent4:
			ApplyGridTable2(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfGridBuiltinStyle.GridTable2Accent5:
			ApplyGridTable2(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfGridBuiltinStyle.GridTable2Accent6:
			ApplyGridTable2(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfGridBuiltinStyle.GridTable3:
			ApplyGridTable3(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfGridBuiltinStyle.GridTable3Accent1:
			ApplyGridTable3(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfGridBuiltinStyle.GridTable3Accent2:
			ApplyGridTable3(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfGridBuiltinStyle.GridTable3Accent3:
			ApplyGridTable3(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfGridBuiltinStyle.GridTable3Accent4:
			ApplyGridTable3(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfGridBuiltinStyle.GridTable3Accent5:
			ApplyGridTable3(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfGridBuiltinStyle.GridTable3Accent6:
			ApplyGridTable3(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfGridBuiltinStyle.GridTable4:
			ApplyGridTable4(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.GridTable4Accent1:
			ApplyGridTable4(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 91, 155, 213));
			break;
		case PdfGridBuiltinStyle.GridTable4Accent2:
			ApplyGridTable4(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 237, 125, 49));
			break;
		case PdfGridBuiltinStyle.GridTable4Accent3:
			ApplyGridTable4(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 165, 165, 165));
			break;
		case PdfGridBuiltinStyle.GridTable4Accent4:
			ApplyGridTable4(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 255, 192, 0));
			break;
		case PdfGridBuiltinStyle.GridTable4Accent5:
			ApplyGridTable4(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 68, 114, 196));
			break;
		case PdfGridBuiltinStyle.GridTable4Accent6:
			ApplyGridTable4(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 112, 173, 71));
			break;
		case PdfGridBuiltinStyle.GridTable5Dark:
			ApplyGridTable5Dark(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 153, 153, 153), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfGridBuiltinStyle.GridTable5DarkAccent1:
			ApplyGridTable5Dark(Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 189, 214, 238), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfGridBuiltinStyle.GridTable5DarkAccent2:
			ApplyGridTable5Dark(Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 247, 202, 172), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfGridBuiltinStyle.GridTable5DarkAccent3:
			ApplyGridTable5Dark(Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 219, 219, 219), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfGridBuiltinStyle.GridTable5DarkAccent4:
			ApplyGridTable5Dark(Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 229, 153), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfGridBuiltinStyle.GridTable5DarkAccent5:
			ApplyGridTable5Dark(Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 180, 198, 231), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfGridBuiltinStyle.GridTable5DarkAccent6:
			ApplyGridTable5Dark(Color.FromArgb(255, 112, 171, 71), Color.FromArgb(255, 197, 224, 179), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfGridBuiltinStyle.GridTable6Colorful:
			ApplyGridTable6Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.GridTable6ColorfulAccent1:
			ApplyGridTable6Colorful(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfGridBuiltinStyle.GridTable6ColorfulAccent2:
			ApplyGridTable6Colorful(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfGridBuiltinStyle.GridTable6ColorfulAccent3:
			ApplyGridTable6Colorful(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfGridBuiltinStyle.GridTable6ColorfulAccent4:
			ApplyGridTable6Colorful(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfGridBuiltinStyle.GridTable6ColorfulAccent5:
			ApplyGridTable6Colorful(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfGridBuiltinStyle.GridTable6ColorfulAccent6:
			ApplyGridTable6Colorful(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		case PdfGridBuiltinStyle.GridTable7Colorful:
			ApplyGridTable7Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.GridTable7ColorfulAccent1:
			ApplyGridTable7Colorful(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfGridBuiltinStyle.GridTable7ColorfulAccent2:
			ApplyGridTable7Colorful(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfGridBuiltinStyle.GridTable7ColorfulAccent3:
			ApplyGridTable7Colorful(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfGridBuiltinStyle.GridTable7ColorfulAccent4:
			ApplyGridTable7Colorful(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfGridBuiltinStyle.GridTable7ColorfulAccent5:
			ApplyGridTable7Colorful(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfGridBuiltinStyle.GridTable7ColorfulAccent6:
			ApplyGridTable7Colorful(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		case PdfGridBuiltinStyle.ListTable1Light:
			ApplyListTable1Light(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfGridBuiltinStyle.ListTable1LightAccent1:
			ApplyListTable1Light(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfGridBuiltinStyle.ListTable1LightAccent2:
			ApplyListTable1Light(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfGridBuiltinStyle.ListTable1LightAccent3:
			ApplyListTable1Light(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfGridBuiltinStyle.ListTable1LightAccent4:
			ApplyListTable1Light(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfGridBuiltinStyle.ListTable1LightAccent5:
			ApplyListTable1Light(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfGridBuiltinStyle.ListTable1LightAccent6:
			ApplyListTable1Light(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfGridBuiltinStyle.ListTable2:
			ApplyListTable2(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfGridBuiltinStyle.ListTable2Accent1:
			ApplyListTable2(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfGridBuiltinStyle.ListTable2Accent2:
			ApplyListTable2(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfGridBuiltinStyle.ListTable2Accent3:
			ApplyListTable2(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfGridBuiltinStyle.ListTable2Accent4:
			ApplyListTable2(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfGridBuiltinStyle.ListTable2Accent5:
			ApplyListTable2(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfGridBuiltinStyle.ListTable2Accent6:
			ApplyListTable2(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfGridBuiltinStyle.ListTable3:
			ApplyListTable3(Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.ListTable3Accent1:
			ApplyListTable3(Color.FromArgb(255, 91, 155, 213));
			break;
		case PdfGridBuiltinStyle.ListTable3Accent2:
			ApplyListTable3(Color.FromArgb(255, 237, 125, 49));
			break;
		case PdfGridBuiltinStyle.ListTable3Accent3:
			ApplyListTable3(Color.FromArgb(255, 165, 165, 165));
			break;
		case PdfGridBuiltinStyle.ListTable3Accent4:
			ApplyListTable3(Color.FromArgb(255, 255, 192, 0));
			break;
		case PdfGridBuiltinStyle.ListTable3Accent5:
			ApplyListTable3(Color.FromArgb(255, 68, 114, 196));
			break;
		case PdfGridBuiltinStyle.ListTable3Accent6:
			ApplyListTable3(Color.FromArgb(255, 112, 171, 71));
			break;
		case PdfGridBuiltinStyle.ListTable4:
			ApplyListTable4(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfGridBuiltinStyle.ListTable4Accent1:
			ApplyListTable4(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfGridBuiltinStyle.ListTable4Accent2:
			ApplyListTable4(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfGridBuiltinStyle.ListTable4Accent3:
			ApplyListTable4(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfGridBuiltinStyle.ListTable4Accent4:
			ApplyListTable4(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfGridBuiltinStyle.ListTable4Accent5:
			ApplyListTable4(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfGridBuiltinStyle.ListTable4Accent6:
			ApplyListTable4(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 112, 173, 71), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfGridBuiltinStyle.ListTable5Dark:
			ApplyListTable5Dark(Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.ListTable5DarkAccent1:
			ApplyListTable5Dark(Color.FromArgb(255, 91, 155, 213));
			break;
		case PdfGridBuiltinStyle.ListTable5DarkAccent2:
			ApplyListTable5Dark(Color.FromArgb(255, 237, 125, 49));
			break;
		case PdfGridBuiltinStyle.ListTable5DarkAccent3:
			ApplyListTable5Dark(Color.FromArgb(255, 165, 165, 165));
			break;
		case PdfGridBuiltinStyle.ListTable5DarkAccent4:
			ApplyListTable5Dark(Color.FromArgb(255, 255, 192, 0));
			break;
		case PdfGridBuiltinStyle.ListTable5DarkAccent5:
			ApplyListTable5Dark(Color.FromArgb(255, 68, 114, 196));
			break;
		case PdfGridBuiltinStyle.ListTable5DarkAccent6:
			ApplyListTable5Dark(Color.FromArgb(255, 112, 173, 71));
			break;
		case PdfGridBuiltinStyle.ListTable6Colorful:
			ApplyListTable6Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.ListTable6ColorfulAccent1:
			ApplyListTable6Colorful(Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfGridBuiltinStyle.ListTable6ColorfulAccent2:
			ApplyListTable6Colorful(Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfGridBuiltinStyle.ListTable6ColorfulAccent3:
			ApplyListTable6Colorful(Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfGridBuiltinStyle.ListTable6ColorfulAccent4:
			ApplyListTable6Colorful(Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfGridBuiltinStyle.ListTable6ColorfulAccent5:
			ApplyListTable6Colorful(Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfGridBuiltinStyle.ListTable6ColorfulAccent6:
			ApplyListTable6Colorful(Color.FromArgb(255, 112, 173, 71), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		case PdfGridBuiltinStyle.ListTable7Colorful:
			ApplyListTable7Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfGridBuiltinStyle.ListTable7ColorfulAccent1:
			ApplyListTable7Colorful(Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfGridBuiltinStyle.ListTable7ColorfulAccent2:
			ApplyListTable7Colorful(Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfGridBuiltinStyle.ListTable7ColorfulAccent3:
			ApplyListTable7Colorful(Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfGridBuiltinStyle.ListTable7ColorfulAccent4:
			ApplyListTable7Colorful(Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfGridBuiltinStyle.ListTable7ColorfulAccent5:
			ApplyListTable7Colorful(Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfGridBuiltinStyle.ListTable7ColorfulAccent6:
			ApplyListTable7Colorful(Color.FromArgb(255, 112, 173, 71), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		}
	}

	public void ApplyBuiltinStyle(PdfGridBuiltinStyle gridStyle, PdfGridBuiltinStyleSettings gridStyleSetting)
	{
		m_headerRow = gridStyleSetting.ApplyStyleForHeaderRow;
		m_totalRow = gridStyleSetting.ApplyStyleForLastRow;
		m_firstColumn = gridStyleSetting.ApplyStyleForFirstColumn;
		m_lastColumn = gridStyleSetting.ApplyStyleForLastColumn;
		m_bandedColumn = gridStyleSetting.ApplyStyleForBandedColumns;
		m_bandedRow = gridStyleSetting.ApplyStyleForBandedRows;
		ApplyBuiltinStyle(gridStyle);
	}

	private PdfFont CreateBoldFont(PdfFont font)
	{
		if (font is PdfStandardFont)
		{
			new PdfStandardFont(font as PdfStandardFont, font.Size, PdfFontStyle.Bold);
		}
		return font;
	}

	private PdfFont CreateRegularFont(PdfFont font)
	{
		if (font is PdfStandardFont)
		{
			new PdfStandardFont(font as PdfStandardFont, font.Size, PdfFontStyle.Regular);
		}
		return font;
	}

	private PdfFont CreateItalicFont(PdfFont font)
	{
		if (font is PdfStandardFont)
		{
			new PdfStandardFont(font as PdfStandardFont, font.Size, PdfFontStyle.Italic);
		}
		return font;
	}

	private PdfFont ChangeFontStyle(PdfFont font)
	{
		PdfFont result = null;
		if (font.Style == PdfFontStyle.Regular)
		{
			result = CreateBoldFont(font);
		}
		else if (font.Style == PdfFontStyle.Bold)
		{
			result = CreateRegularFont(font);
		}
		return result;
	}

	private PdfBrush ApplyBandedColStyle(bool firstColumn, Color backColor, int cellIndex)
	{
		PdfBrush result = null;
		if (firstColumn)
		{
			if (cellIndex % 2 == 0)
			{
				result = new PdfSolidBrush(backColor);
			}
		}
		else if (cellIndex % 2 != 0)
		{
			result = new PdfSolidBrush(backColor);
		}
		return result;
	}

	private PdfBrush ApplyBandedRowStyle(bool headerRow, Color backColor, int rowIndex)
	{
		PdfBrush result = null;
		if (headerRow)
		{
			if (rowIndex % 2 != 0)
			{
				result = new PdfSolidBrush(backColor);
			}
		}
		else if (rowIndex % 2 == 0)
		{
			result = new PdfSolidBrush(backColor);
		}
		return result;
	}

	private void ApplyTableGridLight(Color borderColor)
	{
		PdfPen all = new PdfPen(borderColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					pdfGridRow.Cells[j - 1].Style.Borders.All = all;
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				pdfGridRow2.Cells[l - 1].Style.Borders.All = all;
			}
		}
	}

	private void ApplyPlainTable1(Color borderColor, Color backColor)
	{
		PdfPen all = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = all;
						if (m_bandedColumn)
						{
							pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						}
						if (m_lastColumn && j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.BackgroundBrush = null;
						}
						continue;
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						if (m_bandedRow && i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
				}
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
					pdfGridCell2.Style.Borders.Top = new PdfPen(borderColor);
					if (m_bandedColumn && (!m_lastColumn || l != pdfGridRow2.Cells.Count))
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
				}
			}
		}
	}

	private void ApplyPlainTable2(Color borderColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen pdfPen2 = new PdfPen(Color.Empty);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = pdfPen2;
					pdfGridCell.Style.Borders.Top = pdfPen;
					if (m_bandedColumn)
					{
						pdfGridCell.Style.Borders.Left = pdfPen;
						pdfGridCell.Style.Borders.Right = pdfPen;
					}
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.Bottom = pdfPen;
						if (m_firstColumn && j == 1)
						{
							pdfGridCell.Style.Borders.Left = pdfPen2;
						}
						if (m_lastColumn && j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.Borders.Right = pdfPen2;
						}
						continue;
					}
					if (m_bandedRow)
					{
						pdfGridCell.Style.Borders.Top = pdfPen;
						pdfGridCell.Style.Borders.Bottom = pdfPen;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
						pdfGridCell.Style.Borders.Left = pdfPen2;
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
						pdfGridCell.Style.Borders.Right = pdfPen2;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = pdfPen2;
				if (k == Rows.Count)
				{
					pdfGridCell2.Style.Borders.Bottom = pdfPen;
				}
				if (m_bandedColumn)
				{
					pdfGridCell2.Style.Borders.Left = pdfPen;
					pdfGridCell2.Style.Borders.Right = pdfPen;
				}
				if (m_bandedRow)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
					pdfGridCell2.Style.Borders.Bottom = pdfPen;
				}
				if (k == Rows.Count && m_totalRow)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.Borders.Top = pdfPen;
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.Borders.Left = pdfPen;
						pdfGridCell2.Style.Borders.Right = pdfPen;
					}
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font5);
						pdfGridCell2.Style.Borders.Right = pdfPen2;
					}
					else if (m_bandedColumn)
					{
						pdfGridCell2.Style.Borders.Right = pdfPen2;
					}
				}
				if (m_firstColumn && l == 1)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font6);
						pdfGridCell2.Style.Borders.Left = pdfPen2;
					}
					else if (m_bandedColumn)
					{
						pdfGridCell2.Style.Borders.Left = pdfPen2;
					}
				}
			}
		}
	}

	private void ApplyPlainTable3(Color borderColor, Color backColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		new PdfPen(backColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
					}
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						if (pdfGridCell.Value is string)
						{
							string text = pdfGridCell.Value as string;
							pdfGridCell.Value = text.ToUpper();
						}
						if (i == 1)
						{
							pdfGridCell.Style.Borders.Bottom = pdfPen;
						}
						if (m_lastColumn && j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.BackgroundBrush = null;
						}
						continue;
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						if (m_firstColumn && j == 2)
						{
							pdfGridCell.Style.Borders.Left = pdfPen;
						}
						if (m_headerRow && i == 1)
						{
							pdfGridCell.Style.Borders.Top = pdfPen;
						}
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
						if (pdfGridCell.Value is string)
						{
							string text2 = pdfGridCell.Value as string;
							pdfGridCell.Value = text2.ToUpper();
						}
						pdfGridCell.Style.Borders.Right = pdfPen;
					}
					if (!m_lastColumn || j != pdfGridRow.Cells.Count)
					{
						continue;
					}
					PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell.Style.Font = ChangeFontStyle(font3);
					pdfGridCell.Style.Borders.All = all;
					if (pdfGridCell.Value is string)
					{
						string text3 = pdfGridCell.Value as string;
						pdfGridCell.Value = text3.ToUpper();
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						if (m_bandedRow && i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_bandedRow && m_bandedColumn)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					if (m_firstColumn && l == 2)
					{
						pdfGridCell2.Style.Borders.Left = pdfPen;
					}
					if (m_headerRow && k == 1)
					{
						pdfGridCell2.Style.Borders.Top = pdfPen;
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							if (m_firstColumn && l == 2)
							{
								pdfGridCell2.Style.Borders.Left = pdfPen;
							}
							if (m_headerRow && k == 1)
							{
								pdfGridCell2.Style.Borders.Top = pdfPen;
							}
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (m_firstColumn && l == 2)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen;
						}
						if (m_headerRow && k == 1)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen;
						}
					}
				}
				if (k == Rows.Count && m_totalRow)
				{
					if (m_bandedRow)
					{
						pdfGridCell2.Style.Borders.All = all;
					}
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					if (pdfGridCell2.Value is string)
					{
						string text4 = pdfGridCell2.Value as string;
						pdfGridCell2.Value = text4.ToUpper();
					}
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null && m_firstColumn && l == 2)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen;
						}
					}
				}
				if (m_firstColumn && l == 1)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font5);
						if (pdfGridCell2.Value is string)
						{
							string text5 = pdfGridCell2.Value as string;
							pdfGridCell2.Value = text5.ToUpper();
						}
					}
					pdfGridCell2.Style.Borders.Right = pdfPen;
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font6);
						if (pdfGridCell2.Value is string)
						{
							string text6 = pdfGridCell2.Value as string;
							pdfGridCell2.Value = text6.ToUpper();
						}
						pdfGridCell2.Style.BackgroundBrush = null;
						if (m_bandedRow)
						{
							pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						}
						pdfGridCell2.Style.Borders.All = all;
					}
					else if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
					}
				}
				if (m_headerRow && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
			}
		}
	}

	private void ApplyPlainTable4(Color backColor)
	{
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all2 = new PdfPen(backColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						if (m_bandedColumn)
						{
							pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
							if (pdfGridCell.Style.BackgroundBrush != null)
							{
								pdfGridCell.Style.Borders.All = all2;
							}
						}
						if (m_lastColumn && j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.BackgroundBrush = null;
							pdfGridCell.Style.Borders.All = all;
						}
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
						pdfGridCell.Style.BackgroundBrush = null;
						if (m_bandedRow && i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
				}
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
				}
				if (k == Rows.Count && m_totalRow)
				{
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.BackgroundBrush = null;
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (!m_lastColumn || l != pdfGridRow2.Cells.Count)
				{
					continue;
				}
				if (!m_totalRow || k != Rows.Count)
				{
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
					pdfGridCell2.Style.BackgroundBrush = null;
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else if (m_bandedColumn)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
				}
			}
		}
	}

	private void ApplyPlainTable5(Color borderColor, Color backColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all2 = new PdfPen(backColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_headerRow)
					{
						PdfFont pdfFont = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						if (pdfFont.Style != PdfFontStyle.Italic)
						{
							pdfGridCell.Style.Font = CreateItalicFont(pdfFont);
						}
						if (i == 1)
						{
							pdfGridCell.Style.Borders.Bottom = pdfPen;
						}
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							if (m_firstColumn && j == 2)
							{
								pdfGridCell.Style.Borders.Left = pdfPen;
							}
							else
							{
								pdfGridCell.Style.Borders.All = all2;
							}
						}
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						if (m_firstColumn && j == 2)
						{
							pdfGridCell.Style.Borders.Left = pdfPen;
						}
						else
						{
							pdfGridCell.Style.Borders.All = all2;
						}
					}
					if (m_firstColumn && j == 1)
					{
						pdfGridCell.Style.Borders.All = all;
						pdfGridCell.Style.BackgroundBrush = null;
						PdfFont pdfFont2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						if (pdfFont2.Style != PdfFontStyle.Italic)
						{
							pdfGridCell.Style.Font = CreateItalicFont(pdfFont2);
						}
						pdfGridCell.Style.Borders.Right = pdfPen;
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font);
						pdfGridCell.Style.Borders.All = all;
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_bandedRow && m_bandedColumn)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						if (m_firstColumn && l == 2)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen;
						}
						else
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							if (m_firstColumn && l == 2)
							{
								pdfGridCell2.Style.Borders.Left = pdfPen;
							}
							else
							{
								pdfGridCell2.Style.Borders.All = all2;
							}
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (m_firstColumn && l == 2)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen;
						}
						else
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.Borders.All = new PdfPen(Color.Empty);
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.Top = pdfPen;
					PdfFont pdfFont3 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					if (pdfFont3.Style != PdfFontStyle.Italic)
					{
						pdfGridCell2.Style.Font = CreateItalicFont(pdfFont3);
					}
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont pdfFont4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					if (pdfFont4.Style != PdfFontStyle.Italic)
					{
						pdfGridCell2.Style.Font = CreateItalicFont(pdfFont4);
					}
					pdfGridCell2.Style.Borders.Right = pdfPen;
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font2 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = CreateItalicFont(font2);
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				if (m_headerRow && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
			}
		}
	}

	private void ApplyGridTable1Light(Color borderColor, Color headerBottomColor)
	{
		PdfPen all = new PdfPen(borderColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						continue;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_headerRow && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = new PdfPen(headerBottomColor);
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.Borders.Top = new PdfPen(headerBottomColor);
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
				}
			}
		}
	}

	private void ApplyGridTable2(Color borderColor, Color backColor)
	{
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all = new PdfPen(borderColor, 0.25f);
		PdfPen pdfPen = new PdfPen(backColor, 0.25f);
		PdfPen pdfPen2 = new PdfPen(Color.Empty);
		PdfPen pdfPen3 = new PdfPen(borderColor);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (j == 1)
					{
						pdfGridCell.Style.Borders.Left = pdfPen2;
					}
					else if (j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.Borders.Right = pdfPen2;
					}
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = new PdfPen(Color.Empty);
						if (pdfGridCell.Row.Grid.Style.CellSpacing > 0f)
						{
							pdfGridCell.Style.Borders.Bottom = pdfPen3;
						}
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							if (j == 1)
							{
								pdfGridCell.Style.Borders.Left = pdfPen;
							}
							else if (pdfGridRow.Cells.Count % 2 != 0 && j == pdfGridRow.Cells.Count)
							{
								pdfGridCell.Style.Borders.Right = pdfPen;
							}
						}
					}
					if (m_bandedRow)
					{
						if (i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							if (j == 1)
							{
								pdfGridCell.Style.Borders.Left = pdfPen;
							}
							else if (j == pdfGridRow.Cells.Count)
							{
								pdfGridCell.Style.Borders.Right = pdfPen;
							}
						}
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (!m_lastColumn || j != pdfGridRow.Cells.Count)
					{
						continue;
					}
					pdfGridCell.Style.BackgroundBrush = null;
					if (m_bandedRow)
					{
						if (i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							if (j == 1)
							{
								pdfGridCell.Style.Borders.Left = pdfPen;
							}
							else if (j == pdfGridRow.Cells.Count)
							{
								pdfGridCell.Style.Borders.Right = pdfPen;
							}
						}
					}
					PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell.Style.Font = ChangeFontStyle(font3);
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (l == 1)
				{
					pdfGridCell2.Style.Borders.Left = pdfPen2;
				}
				else if (l == pdfGridRow2.Cells.Count)
				{
					pdfGridCell2.Style.Borders.Right = pdfPen2;
				}
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						if (l == 1)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen;
						}
						else if (l == pdfGridRow2.Cells.Count)
						{
							pdfGridCell2.Style.Borders.Right = pdfPen;
						}
					}
				}
				else
				{
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							if (l == 1)
							{
								pdfGridCell2.Style.Borders.Left = pdfPen;
							}
							else if (l == pdfGridRow2.Cells.Count)
							{
								pdfGridCell2.Style.Borders.Right = pdfPen;
							}
						}
					}
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							if (l == 1)
							{
								pdfGridCell2.Style.Borders.Left = pdfPen;
							}
							else if (pdfGridRow2.Cells.Count % 2 != 0 && l == pdfGridRow2.Cells.Count)
							{
								pdfGridCell2.Style.Borders.Right = pdfPen;
							}
						}
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = pdfPen2;
					pdfGridCell2.Style.Borders.Top = pdfPen3;
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font6);
						pdfGridCell2.Style.BackgroundBrush = null;
						if (m_bandedRow)
						{
							pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
							if (pdfGridCell2.Style.BackgroundBrush == null)
							{
								pdfGridCell2.Style.Borders.Right = pdfPen2;
							}
						}
					}
					else if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
					}
				}
				if (m_headerRow && m_headers.Count > 0 && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen3;
				}
			}
		}
	}

	private void ApplyGridTable3(Color borderColor, Color backColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all = new PdfPen(Color.Empty);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = pdfPen;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = all;
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = all;
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = all;
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = pdfPen;
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.Borders.All = new PdfPen(Color.Empty);
					pdfGridCell2.Style.BackgroundBrush = null;
				}
				if (m_firstColumn && l == 1)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
						pdfGridCell2.Style.Borders.All = all;
						PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = CreateItalicFont(font5);
						if (k == 1 && m_headerRow)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen;
						}
					}
					else
					{
						pdfGridCell2.Style.Borders.Top = pdfPen;
					}
				}
				if (!m_lastColumn || l != pdfGridRow2.Cells.Count)
				{
					continue;
				}
				if (!m_totalRow || k != Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = all;
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = CreateItalicFont(font6);
					if (k == 1 && m_headerRow)
					{
						pdfGridCell2.Style.Borders.Top = pdfPen;
					}
				}
				else
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
			}
		}
	}

	private void ApplyGridTable4(Color borderColor, Color backColor, Color headerBackColor)
	{
		PdfPen all = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen top = new PdfPen(headerBackColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = new PdfPen(headerBackColor);
						pdfGridCell.Style.TextBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
						pdfGridCell.Style.BackgroundBrush = new PdfSolidBrush(headerBackColor);
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						if (m_bandedColumn)
						{
							pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						}
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
				}
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
					pdfGridCell2.Style.Borders.Top = new PdfPen(borderColor);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
						if (m_bandedRow)
						{
							pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						}
						PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font6);
					}
					else if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
					}
				}
				if (m_headerRow && m_headers.Count > 0 && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = top;
				}
			}
		}
	}

	private void ApplyGridTable5Dark(Color headerBackColor, Color oddRowBackColor, Color evenRowBackColor)
	{
		new PdfPen(oddRowBackColor, 0.5f);
		new PdfPen(evenRowBackColor, 0.5f);
		PdfPen pdfPen = new PdfPen(Color.FromArgb(255, 255, 255, 255), 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(evenRowBackColor);
		PdfBrush backgroundBrush2 = new PdfSolidBrush(oddRowBackColor);
		PdfBrush backgroundBrush3 = new PdfSolidBrush(headerBackColor);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = pdfPen;
					pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.TextBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
						pdfGridCell.Style.BackgroundBrush = backgroundBrush3;
						pdfGridCell.Style.Borders.All = new PdfPen(Color.Empty, 0.5f);
						if (j == 1)
						{
							pdfGridCell.Style.Borders.Left = pdfPen;
						}
						else if (j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.Borders.Right = pdfPen;
						}
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, oddRowBackColor, j);
						if (pdfGridCell.Style.BackgroundBrush == null)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush2;
					}
					if ((m_firstColumn && j == 1) || (m_lastColumn && j == pdfGridRow.Cells.Count))
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush3;
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
						pdfGridCell.Style.TextBrush = textBrush;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = pdfPen;
				pdfGridCell2.Style.BackgroundBrush = backgroundBrush;
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, oddRowBackColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, oddRowBackColor, k);
						if (pdfGridCell2.Style.BackgroundBrush == null)
						{
							pdfGridCell2.Style.BackgroundBrush = backgroundBrush;
						}
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, oddRowBackColor, l);
						if (pdfGridCell2.Style.BackgroundBrush == null)
						{
							pdfGridCell2.Style.BackgroundBrush = backgroundBrush;
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, oddRowBackColor, k);
						if (pdfGridCell2.Style.BackgroundBrush == null)
						{
							pdfGridCell2.Style.BackgroundBrush = backgroundBrush;
						}
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					PdfFont font3 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font3);
					pdfGridCell2.Style.TextBrush = textBrush;
					pdfGridCell2.Style.BackgroundBrush = new PdfSolidBrush(headerBackColor);
					pdfGridCell2.Style.Borders.All = pdfPen;
					if (l == 1)
					{
						pdfGridCell2.Style.Borders.Left = pdfPen;
					}
					else if (l == pdfGridRow2.Cells.Count)
					{
						pdfGridCell2.Style.Borders.Right = pdfPen;
					}
				}
				if (((m_firstColumn && l == 1) || (m_lastColumn && l == pdfGridRow2.Cells.Count)) && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = backgroundBrush3;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.TextBrush = textBrush;
				}
			}
		}
	}

	private void ApplyGridTable6Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfPen all = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen top = new PdfPen(borderColor);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					pdfGridCell.Style.TextBrush = textBrush;
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
					}
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						if (m_lastColumn && j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.BackgroundBrush = null;
						}
						continue;
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						if (i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				pdfGridCell2.Style.TextBrush = textBrush;
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					if (m_bandedColumn && (!m_lastColumn || l != pdfGridRow2.Cells.Count))
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.Borders.Top = new PdfPen(borderColor);
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
				}
				if (m_headerRow && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = top;
				}
			}
		}
	}

	private void ApplyGridTable7Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		new PdfPen(borderColor);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		PdfPen all = new PdfPen(Color.Empty, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.TextBrush = textBrush;
					pdfGridCell.Style.Borders.All = pdfPen;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = new PdfPen(Color.FromArgb(255, 255, 255, 255));
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = all;
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = all;
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = pdfPen;
				pdfGridCell2.Style.TextBrush = textBrush;
				if (m_bandedRow && m_bandedColumn)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.Borders.All = new PdfPen(Color.Empty);
				}
				if (m_firstColumn && l == 1)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
						pdfGridCell2.Style.Borders.All = all;
						if (k == 1 && m_headerRow)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen;
						}
						PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = CreateItalicFont(font5);
					}
					else
					{
						pdfGridCell2.Style.Borders.Top = pdfPen;
					}
				}
				if (!m_lastColumn || l != pdfGridRow2.Cells.Count)
				{
					continue;
				}
				if (!m_totalRow || k != Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.Borders.Left = pdfPen;
					if (k == 1 && m_headerRow)
					{
						pdfGridCell2.Style.Borders.Top = pdfPen;
					}
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = CreateItalicFont(font6);
				}
				else
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
			}
		}
	}

	private void ApplyListTable1Light(Color borderColor, Color backColor)
	{
		PdfPen top = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all2 = new PdfPen(backColor, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						if (i == 1)
						{
							pdfGridCell.Style.Borders.Bottom = new PdfPen(borderColor);
						}
						if (m_bandedColumn)
						{
							if (m_lastColumn && j == Rows.Count)
							{
								pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
							}
							if (pdfGridCell.Style.BackgroundBrush != null)
							{
								pdfGridCell.Style.Borders.All = all2;
							}
							if (m_lastColumn && j == pdfGridRow.Cells.Count)
							{
								pdfGridCell.Style.Borders.All = all;
								pdfGridCell.Style.BackgroundBrush = null;
							}
						}
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							pdfGridCell.Style.Borders.All = all2;
						}
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						pdfGridCell.Style.Borders.All = all2;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = all;
						if (m_bandedRow && i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						pdfGridCell2.Style.Borders.All = all2;
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
				}
				if (m_firstColumn && l == 1)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					}
					else
					{
						pdfGridCell2.Style.Borders.Top = top;
					}
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = all;
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = all;
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
					if (m_bandedColumn)
					{
						if (!m_lastColumn || l != pdfGridRow2.Cells.Count)
						{
							pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						}
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
					pdfGridCell2.Style.Borders.Top = new PdfPen(borderColor);
				}
				if (m_headerRow && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = top;
				}
			}
		}
	}

	private void ApplyListTable2(Color borderColor, Color backColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen pdfPen2 = new PdfPen(backColor, 0.5f);
		PdfPen pdfPen3 = new PdfPen(Color.Empty, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = pdfPen3;
					pdfGridCell.Style.Borders.Bottom = pdfPen;
					pdfGridCell.Style.Borders.Top = pdfPen;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						if (m_bandedColumn)
						{
							pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
							if (m_lastColumn && j == pdfGridRow.Cells.Count)
							{
								pdfGridCell.Style.BackgroundBrush = null;
							}
							if (pdfGridCell.Style.BackgroundBrush != null)
							{
								pdfGridCell.Style.Borders.Right = pdfPen2;
								pdfGridCell.Style.Borders.Left = pdfPen2;
							}
						}
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							pdfGridCell.Style.Borders.Right = pdfPen2;
							pdfGridCell.Style.Borders.Left = pdfPen2;
						}
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.Borders.Left = pdfPen2;
						pdfGridCell.Style.Borders.Right = pdfPen2;
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (!m_lastColumn || j != pdfGridRow.Cells.Count)
					{
						continue;
					}
					pdfGridCell.Style.BackgroundBrush = null;
					pdfGridCell.Style.Borders.Left = pdfPen3;
					pdfGridCell.Style.Borders.Right = pdfPen3;
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							pdfGridCell.Style.Borders.Left = pdfPen2;
							pdfGridCell.Style.Borders.Right = pdfPen2;
						}
					}
					PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell.Style.Font = ChangeFontStyle(font3);
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = pdfPen3;
				pdfGridCell2.Style.Borders.Bottom = pdfPen;
				pdfGridCell2.Style.Borders.Top = pdfPen;
				if (m_bandedRow && m_bandedColumn)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						pdfGridCell2.Style.Borders.Right = pdfPen2;
						pdfGridCell2.Style.Borders.Left = pdfPen2;
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.Right = pdfPen2;
							pdfGridCell2.Style.Borders.Left = pdfPen2;
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen2;
							pdfGridCell2.Style.Borders.Right = pdfPen2;
						}
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = pdfPen3;
					pdfGridCell2.Style.Borders.Top = pdfPen;
					pdfGridCell2.Style.Borders.Bottom = pdfPen;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					if (m_bandedColumn)
					{
						if (l != pdfGridRow2.Cells.Count || !m_lastColumn)
						{
							pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						}
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.Right = pdfPen2;
							pdfGridCell2.Style.Borders.Left = pdfPen2;
						}
					}
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (!m_lastColumn || l != pdfGridRow2.Cells.Count || (m_totalRow && k == Rows.Count))
				{
					continue;
				}
				pdfGridCell2.Style.BackgroundBrush = null;
				pdfGridCell2.Style.Borders.Left = pdfPen3;
				pdfGridCell2.Style.Borders.Right = pdfPen3;
				if (m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						pdfGridCell2.Style.Borders.Right = pdfPen2;
					}
				}
				PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
				pdfGridCell2.Style.Font = ChangeFontStyle(font6);
			}
		}
	}

	private void ApplyListTable3(Color backColor)
	{
		PdfPen pdfPen = new PdfPen(backColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all = new PdfPen(Color.Empty, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					pdfGridCell.Style.Borders.Top = pdfPen;
					if (j == 1)
					{
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
					else if (j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.Borders.Right = pdfPen;
					}
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = pdfPen;
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						pdfGridCell.Style.TextBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
					}
					else
					{
						if (m_firstColumn && j == 1)
						{
							PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
							pdfGridCell.Style.Font = ChangeFontStyle(font2);
						}
						if (m_lastColumn && j == pdfGridRow.Cells.Count)
						{
							PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
							pdfGridCell.Style.Font = ChangeFontStyle(font3);
						}
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				if (Headers.Count == 0 && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
				if (l == 1)
				{
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				else if (l == pdfGridRow2.Cells.Count)
				{
					pdfGridCell2.Style.Borders.Right = pdfPen;
				}
				if (k == Rows.Count)
				{
					pdfGridCell2.Style.Borders.Bottom = pdfPen;
				}
				if (m_bandedColumn)
				{
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				if (m_bandedRow)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
				if (m_totalRow && k == Rows.Count)
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.Borders.Top = new PdfPen(backColor);
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
				}
			}
		}
	}

	private void ApplyListTable4(Color borderColor, Color headerBackColor, Color bandRowColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(headerBackColor);
		PdfBrush backgroundBrush2 = new PdfSolidBrush(bandRowColor);
		new PdfPen(bandRowColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty, 0.5f);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					pdfGridCell.Style.Borders.Top = pdfPen;
					if (j == 1)
					{
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
					else if (j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.Borders.Right = pdfPen;
					}
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.All = new PdfPen(headerBackColor, 0.5f);
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						pdfGridCell.Style.TextBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, bandRowColor, j);
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush2;
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						if (m_bandedRow && i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush2;
						}
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				pdfGridCell2.Style.Borders.Top = pdfPen;
				if (l == 1)
				{
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				else if (l == pdfGridRow2.Cells.Count)
				{
					pdfGridCell2.Style.Borders.Right = pdfPen;
				}
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, bandRowColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, bandRowColor, k);
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, bandRowColor, l);
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, bandRowColor, k);
					}
				}
				if (m_totalRow && k == Rows.Count)
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
					pdfGridCell2.Style.Borders.Top = new PdfPen(borderColor);
					if (m_bandedColumn && (!m_lastColumn || l != Rows.Count))
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, bandRowColor, l);
					}
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, bandRowColor, k);
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
				}
				if (k == Rows.Count)
				{
					pdfGridCell2.Style.Borders.Bottom = pdfPen;
				}
			}
		}
	}

	private void ApplyListTable5Dark(Color backColor)
	{
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		new PdfPen(backColor, 0.5f);
		PdfPen pdfPen = new PdfPen(Color.FromArgb(255, 255, 255, 255), 0.5f);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		PdfPen pdfPen2 = new PdfPen(Color.Empty);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = pdfPen2;
					pdfGridCell.Style.TextBrush = textBrush;
					pdfGridCell.Style.BackgroundBrush = backgroundBrush;
					if (m_headerRow)
					{
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						pdfGridCell.Style.TextBrush = textBrush;
						pdfGridCell.Style.Borders.Bottom = new PdfPen(Color.FromArgb(255, 255, 255, 255), 2f);
						if (m_bandedColumn && j > 1)
						{
							pdfGridCell.Style.Borders.Left = pdfPen;
						}
						continue;
					}
					if (m_firstColumn)
					{
						switch (j)
						{
						case 1:
						{
							PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
							pdfGridCell.Style.Font = ChangeFontStyle(font2);
							break;
						}
						case 2:
							pdfGridCell.Style.Borders.Left = pdfPen;
							break;
						}
					}
					if (m_bandedColumn && j > 1)
					{
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
					if (m_bandedRow)
					{
						pdfGridCell.Style.Borders.Top = pdfPen;
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = pdfPen2;
				pdfGridCell2.Style.TextBrush = textBrush;
				pdfGridCell2.Style.BackgroundBrush = backgroundBrush;
				if (m_firstColumn && (!m_totalRow || k != Rows.Count))
				{
					switch (l)
					{
					case 1:
					{
						PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font4);
						break;
					}
					case 2:
						pdfGridCell2.Style.Borders.Left = pdfPen;
						break;
					}
				}
				if (m_bandedColumn && l > 1)
				{
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				if (m_bandedRow)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
				if (m_totalRow && k == Rows.Count)
				{
					PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font5);
					pdfGridCell2.Style.Borders.Top = pdfPen;
					if (m_headerRow)
					{
						if (m_firstColumn && l == 1)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen2;
						}
						if (m_lastColumn && l == pdfGridRow2.Cells.Count)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen2;
						}
					}
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font6);
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
			}
		}
	}

	private void ApplyListTable6Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen pdfPen2 = new PdfPen(backColor, 0.5f);
		PdfPen pdfPen3 = new PdfPen(Color.Empty, 0.5f);
		new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = pdfPen3;
					pdfGridCell.Style.Borders.Top = pdfPen;
					pdfGridCell.Style.TextBrush = textBrush;
					if (m_headerRow)
					{
						if (m_bandedColumn)
						{
							if (!m_lastColumn || j != pdfGridRow.Cells.Count)
							{
								pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
							}
							if (pdfGridCell.Style.BackgroundBrush != null)
							{
								pdfGridCell.Style.Borders.Left = pdfPen2;
								pdfGridCell.Style.Borders.Right = pdfPen2;
							}
						}
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font);
						pdfGridCell.Style.Borders.Bottom = pdfPen;
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						if (pdfGridCell.Style.BackgroundBrush != null && i == 1 && m_headerRow)
						{
							pdfGridCell.Style.Borders.Top = pdfPen;
						}
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						if (j == 1)
						{
							pdfGridCell.Style.Borders.Left = pdfPen2;
						}
						else if (j == pdfGridRow.Cells.Count)
						{
							pdfGridCell.Style.Borders.Right = pdfPen2;
						}
						if (i == 1)
						{
							pdfGridCell.Style.Borders.Top = pdfPen;
						}
					}
					if (m_firstColumn && j == 1)
					{
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font2);
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = pdfPen3;
						pdfGridCell.Style.Borders.Top = pdfPen;
						if (m_bandedRow && i % 2 != 0)
						{
							pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						}
						PdfFont font3 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = ChangeFontStyle(font3);
						pdfGridCell.Style.Borders.Left = pdfPen3;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.TextBrush = textBrush;
				pdfGridCell2.Style.Borders.All = pdfPen3;
				if (Headers.Count == 0 && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
				if (k == Rows.Count)
				{
					pdfGridCell2.Style.Borders.Bottom = pdfPen;
				}
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						if (l == 1)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen2;
						}
						else if (l == pdfGridRow2.Cells.Count)
						{
							pdfGridCell2.Style.Borders.Right = pdfPen2;
						}
					}
					if (k == 1 && m_headerRow)
					{
						pdfGridCell2.Style.Borders.Top = pdfPen;
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen2;
							pdfGridCell2.Style.Borders.Right = pdfPen2;
							if (k == 1 && m_headerRow)
							{
								pdfGridCell2.Style.Borders.Top = pdfPen;
							}
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (l == 1)
						{
							pdfGridCell2.Style.Borders.Left = pdfPen2;
						}
						else if (l == pdfGridRow2.Cells.Count)
						{
							pdfGridCell2.Style.Borders.Right = pdfPen2;
						}
						if (k == 1 && m_headerRow)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen;
						}
					}
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = ChangeFontStyle(font4);
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count)
				{
					if (!m_totalRow || k != Rows.Count)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
						pdfGridCell2.Style.Borders.Left = pdfPen3;
						pdfGridCell2.Style.Borders.Right = pdfPen3;
						if (m_bandedRow)
						{
							pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
							if (pdfGridCell2.Style.BackgroundBrush != null)
							{
								pdfGridCell2.Style.Borders.Right = pdfPen2;
							}
						}
						PdfFont font5 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell2.Style.Font = ChangeFontStyle(font5);
						if (k == 1 && m_headerRow)
						{
							pdfGridCell2.Style.Borders.Top = pdfPen;
						}
					}
					else if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
					}
				}
				if (!m_totalRow || k != Rows.Count)
				{
					continue;
				}
				pdfGridCell2.Style.BackgroundBrush = null;
				pdfGridCell2.Style.Borders.Left = pdfPen3;
				pdfGridCell2.Style.Borders.Right = pdfPen3;
				PdfFont font6 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
				pdfGridCell2.Style.Font = ChangeFontStyle(font6);
				pdfGridCell2.Style.Borders.Top = new PdfPen(borderColor);
				if (m_bandedColumn)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (m_lastColumn && l == pdfGridRow2.Cells.Count)
					{
						pdfGridCell2.Style.BackgroundBrush = null;
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						pdfGridCell2.Style.Borders.Left = pdfPen2;
						pdfGridCell2.Style.Borders.Right = pdfPen2;
					}
				}
			}
		}
	}

	private void ApplyListTable7Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all2 = new PdfPen(backColor, 0.5f);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		new PdfPen(borderColor);
		if (Headers.Count > 0)
		{
			for (int i = 1; i <= Headers.Count; i++)
			{
				PdfGridRow pdfGridRow = Headers[i - 1];
				for (int j = 1; j <= pdfGridRow.Cells.Count; j++)
				{
					PdfGridCell pdfGridCell = pdfGridRow.Cells[j - 1];
					pdfGridCell.Style.Borders.All = all;
					pdfGridCell.Style.TextBrush = textBrush;
					if (m_headerRow)
					{
						PdfFont pdfFont = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						if (pdfFont.Style != PdfFontStyle.Italic)
						{
							pdfGridCell.Style.Font = CreateItalicFont(pdfFont);
						}
						pdfGridCell.Style.Borders.Bottom = pdfPen;
						continue;
					}
					if (m_bandedColumn)
					{
						pdfGridCell.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, j);
						if (pdfGridCell.Style.BackgroundBrush != null)
						{
							pdfGridCell.Style.Borders.All = all2;
						}
					}
					if (m_bandedRow && i % 2 != 0)
					{
						pdfGridCell.Style.BackgroundBrush = backgroundBrush;
						pdfGridCell.Style.Borders.All = all2;
					}
					if (m_firstColumn && j == 1)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						pdfGridCell.Style.Borders.All = all;
						PdfFont font = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font);
						pdfGridCell.Style.Borders.Right = pdfPen;
					}
					if (m_lastColumn && j == pdfGridRow.Cells.Count)
					{
						pdfGridCell.Style.BackgroundBrush = null;
						PdfFont font2 = pdfGridCell.Style.Font ?? pdfGridRow.Style.Font ?? pdfGridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
						pdfGridCell.Style.Font = CreateItalicFont(font2);
						pdfGridCell.Style.Borders.All = all;
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
					if (m_firstColumn && j == 2)
					{
						pdfGridCell.Style.Borders.Left = pdfPen;
					}
				}
			}
		}
		for (int k = 1; k <= Rows.Count; k++)
		{
			PdfGridRow pdfGridRow2 = Rows[k - 1];
			for (int l = 1; l <= pdfGridRow2.Cells.Count; l++)
			{
				PdfGridCell pdfGridCell2 = pdfGridRow2.Cells[l - 1];
				pdfGridCell2.Style.Borders.All = all;
				pdfGridCell2.Style.TextBrush = textBrush;
				if (m_bandedColumn && m_bandedRow)
				{
					pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
					if (pdfGridCell2.Style.BackgroundBrush == null)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
					}
					if (pdfGridCell2.Style.BackgroundBrush != null)
					{
						pdfGridCell2.Style.Borders.All = all2;
					}
				}
				else
				{
					if (m_bandedColumn)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedColStyle(m_firstColumn, backColor, l);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
					if (m_bandedRow)
					{
						pdfGridCell2.Style.BackgroundBrush = ApplyBandedRowStyle(m_headerRow, backColor, k);
						if (pdfGridCell2.Style.BackgroundBrush != null)
						{
							pdfGridCell2.Style.Borders.All = all2;
						}
					}
				}
				if (m_firstColumn && l == 1 && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					pdfGridCell2.Style.Borders.All = all;
					PdfFont font3 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = CreateItalicFont(font3);
					pdfGridCell2.Style.Borders.Right = pdfPen;
				}
				if (m_firstColumn && l == 2)
				{
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				if (m_totalRow && k == Rows.Count)
				{
					PdfFont pdfFont2 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					if (pdfFont2.Style != PdfFontStyle.Italic)
					{
						pdfGridCell2.Style.Font = CreateItalicFont(pdfFont2);
					}
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.Borders.Top = pdfPen;
					pdfGridCell2.Style.BackgroundBrush = null;
				}
				if (m_lastColumn && l == pdfGridRow2.Cells.Count && (!m_totalRow || k != Rows.Count))
				{
					pdfGridCell2.Style.BackgroundBrush = null;
					PdfFont font4 = pdfGridCell2.Style.Font ?? pdfGridRow2.Style.Font ?? pdfGridRow2.Grid.Style.Font ?? PdfDocument.DefaultFont;
					pdfGridCell2.Style.Font = CreateItalicFont(font4);
					pdfGridCell2.Style.Borders.All = all;
					pdfGridCell2.Style.Borders.Left = pdfPen;
				}
				if (m_headerRow && k == 1)
				{
					pdfGridCell2.Style.Borders.Top = pdfPen;
				}
			}
		}
	}

	private void SetDataSource()
	{
		PopulateDataGrid();
	}

	private void PopulateDataGrid()
	{
		Array array = m_dataSource as Array;
		DataSet dataSet = m_dataSource as DataSet;
		DataColumn dataColumn = m_dataSource as DataColumn;
		DataTable dataTable = m_dataSource as DataTable;
		DataView dataView = m_dataSource as DataView;
		PdfDataSource pdfDataSource = null;
		if (array != null)
		{
			pdfDataSource = new PdfDataSource(array);
			if (array != null && array.Length > 0)
			{
				int index = 0;
				string[] row = pdfDataSource.GetRow(ref index);
				if (array.GetType().FullName.TrimEnd(']').TrimEnd('[') == row[0])
				{
					pdfDataSource = null;
				}
			}
		}
		else if (dataColumn != null)
		{
			pdfDataSource = new PdfDataSource(dataColumn);
		}
		else if (dataTable != null)
		{
			pdfDataSource = new PdfDataSource(dataTable);
		}
		else if (dataView != null)
		{
			pdfDataSource = new PdfDataSource(dataView);
		}
		else if (dataSet != null)
		{
			pdfDataSource = new PdfDataSource(dataSet, m_dataMember);
		}
		m_dsParser = pdfDataSource;
		if (pdfDataSource == null)
		{
			PopulateIEnumerableGrid();
			return;
		}
		PopulateHeader();
		PopulateGrid();
	}

	private void PopulateIEnumerableGrid()
	{
		if (!(m_dataSource is IEnumerable))
		{
			return;
		}
		PropertyInfo[] array = null;
		foreach (object item in m_dataSource as IEnumerable)
		{
			if (item != null)
			{
				array = new List<PropertyInfo>(item.GetType().GetRuntimeProperties()).ToArray();
				Columns.Add(array.Length);
				PdfGridRow pdfGridRow = new PdfGridRow(this);
				PropertyInfo[] array2 = array;
				foreach (PropertyInfo propertyInfo in array2)
				{
					PdfGridCell pdfGridCell = new PdfGridCell();
					pdfGridCell.Value = propertyInfo.Name;
					pdfGridRow.Cells.Add(pdfGridCell);
				}
				Headers.Add(pdfGridRow);
				break;
			}
		}
		foreach (object item2 in m_dataSource as IEnumerable)
		{
			PdfGridRow pdfGridRow = new PdfGridRow(this);
			PropertyInfo[] array2 = array;
			foreach (PropertyInfo propertyInfo2 in array2)
			{
				PropertyInfo runtimeProperty = item2.GetType().GetRuntimeProperty(propertyInfo2.Name);
				PdfGridCell pdfGridCell2 = new PdfGridCell(pdfGridRow);
				pdfGridCell2.Value = Convert.ToString(runtimeProperty.GetValue(item2, null));
				pdfGridRow.Cells.Add(pdfGridCell2);
			}
			Rows.Add(pdfGridRow);
		}
	}

	private void PopulateGrid()
	{
		if (m_dsParser != null)
		{
			int index = 0;
			Rows.Clear();
			while (index < m_dsParser.RowCount)
			{
				PdfGridRow pdfGridRow = new PdfGridRow(this);
				string[] row = m_dsParser.GetRow(ref index);
				for (int i = 0; i < m_dsParser.ColumnCount; i++)
				{
					PdfGridCell pdfGridCell = new PdfGridCell(pdfGridRow);
					pdfGridCell.Value = row[i];
					pdfGridRow.Cells.Add(pdfGridCell);
				}
				Rows.Add(pdfGridRow);
			}
		}
		for (int j = 0; j < m_dsParser.ColumnCount; j++)
		{
			Columns.Add(new PdfGridColumn(this));
		}
	}

	private void PopulateHeader()
	{
		Headers.Clear();
		string[] columnCaptions = m_dsParser.ColumnCaptions;
		if (columnCaptions != null)
		{
			PdfGridRow pdfGridRow = new PdfGridRow(this);
			for (int i = 0; i < m_dsParser.ColumnCount; i++)
			{
				PdfGridCell pdfGridCell = new PdfGridCell(pdfGridRow);
				pdfGridCell.Value = columnCaptions[i];
				pdfGridRow.Cells.Add(pdfGridCell);
			}
			Headers.Add(pdfGridRow);
		}
	}

	internal void MeasureColumnsWidth()
	{
		float[] array = new float[Columns.Count];
		float num = 0f;
		if (Headers.Count > 0)
		{
			int i = 0;
			for (int count = Headers[0].Cells.Count; i < count; i++)
			{
				int j = 0;
				for (int count2 = Headers.Count; j < count2; j++)
				{
					float val = (((double)InitialWidth > 0.0) ? Math.Min(InitialWidth, Headers[j].Cells[i].Width) : Headers[j].Cells[i].Width);
					num = Math.Max(num, val);
				}
				array[i] = num;
			}
		}
		num = 0f;
		int k = 0;
		for (int count3 = Columns.Count; k < count3; k++)
		{
			int l = 0;
			for (int count4 = Rows.Count; l < count4; l++)
			{
				if ((Rows[l].Cells[k].ColumnSpan != 1 || Rows[l].Cells[k].IsCellMergeContinue) && !(Rows[l].Cells[k].Value is PdfGrid))
				{
					continue;
				}
				if (Rows[l].Cells[k].Value is PdfGrid && !Rows[l].Grid.Style.AllowHorizontalOverflow)
				{
					float num2 = Rows[l].Grid.Style.CellPadding.Right + Rows[l].Grid.Style.CellPadding.Left + Rows[l].Cells[k].Style.Borders.Left.Width / 2f + m_gridLocation.X;
					if (InitialWidth != 0f)
					{
						(Rows[l].Cells[k].Value as PdfGrid).InitialWidth = InitialWidth - num2;
					}
				}
				float num3 = 0f;
				num3 = (((double)InitialWidth > 0.0) ? Math.Min(InitialWidth, Rows[l].Cells[k].Width) : Rows[l].Cells[k].Width);
				num = Math.Max(array[k], Math.Max(num, num3));
				num = Math.Max(Columns[k].Width, num);
			}
			if (Rows.Count != 0)
			{
				array[k] = num;
			}
			num = 0f;
		}
		int m = 0;
		for (int count5 = Rows.Count; m < count5; m++)
		{
			int n = 0;
			for (int count6 = Columns.Count; n < count6; n++)
			{
				if (Rows[m].Cells[n].ColumnSpan <= 1)
				{
					continue;
				}
				float num4 = array[n];
				for (int num5 = 1; num5 < Rows[m].Cells[n].ColumnSpan; num5++)
				{
					num4 += array[n + num5];
				}
				if (Rows[m].Cells[n].Width > num4)
				{
					float num6 = Rows[m].Cells[n].Width - num4;
					num6 /= (float)Rows[m].Cells[n].ColumnSpan;
					for (int num7 = n; num7 < n + Rows[m].Cells[n].ColumnSpan; num7++)
					{
						array[num7] += num6;
					}
				}
			}
		}
		if (IsChildGrid && InitialWidth != 0f)
		{
			array = Columns.GetDefaultWidths(InitialWidth);
		}
		int num8 = 0;
		for (int count7 = Columns.Count; num8 < count7; num8++)
		{
			if (Columns[num8].Width < 0f)
			{
				Columns[num8].m_width = array[num8];
			}
			else if ((Columns[num8].Width > 0f) & !Columns[num8].isCustomWidth)
			{
				Columns[num8].m_width = array[num8];
			}
		}
	}

	internal void MeasureColumnsWidth(RectangleF bounds)
	{
		float[] defaultWidths = Columns.GetDefaultWidths(bounds.Width - bounds.X);
		int i = 0;
		for (int count = Columns.Count; i < count; i++)
		{
			if (Columns[i].Width < 0f)
			{
				Columns[i].m_width = defaultWidths[i];
			}
			else if (Columns[i].Width > 0f && !Columns[i].isCustomWidth && defaultWidths[i] > 0f && isComplete)
			{
				Columns[i].m_width = defaultWidths[i];
			}
		}
		if (ParentCell != null && !Style.AllowHorizontalOverflow && !ParentCell.Row.Grid.Style.AllowHorizontalOverflow)
		{
			float num = 0f;
			float num2 = 0f;
			int num3 = Columns.Count;
			if (ParentCell.Style.CellPadding != null)
			{
				num += ParentCell.Style.CellPadding.Left + ParentCell.Style.CellPadding.Right;
			}
			else if (ParentCell.Row.Grid.Style.CellPadding != null)
			{
				num += ParentCell.Row.Grid.Style.CellPadding.Left + ParentCell.Row.Grid.Style.CellPadding.Right;
			}
			for (int j = 0; j < ParentCell.ColumnSpan; j++)
			{
				num2 += ParentCell.Row.Grid.Columns[parentCellIndex + j].Width;
			}
			for (int k = 0; k < Columns.Count; k++)
			{
				if (m_columns[k].Width > 0f && m_columns[k].isCustomWidth)
				{
					num2 -= m_columns[k].Width;
					num3--;
				}
			}
			if (num2 > num)
			{
				float num4 = 0f;
				num4 = ((!(num2 - num > bounds.Width) || ParentCell.Row.Grid.Columns.Count != 1 || !ParentCell.Row.Grid.Columns[0].isCustomWidth) ? ((num2 - num) / (float)num3) : (bounds.Width / (float)num3));
				if (ParentCell != null && ParentCell.StringFormat.Alignment != PdfTextAlignment.Right)
				{
					for (int l = 0; l < Columns.Count; l++)
					{
						if (!Columns[l].isCustomWidth)
						{
							if (num4 > bounds.Width && IsChildGrid && Style != null && Style.CellPadding != null)
							{
								Columns[l].m_width = bounds.Width;
							}
							else
							{
								Columns[l].m_width = num4;
							}
						}
					}
				}
			}
		}
		if (ParentCell != null && ParentCell.Row.Width > 0f && IsChildGrid && m_size.Width > ParentCell.Row.Width)
		{
			defaultWidths = Columns.GetDefaultWidths(bounds.Width);
			for (int m = 0; m < Columns.Count; m++)
			{
				Columns[m].Width = defaultWidths[m];
			}
		}
	}
}
