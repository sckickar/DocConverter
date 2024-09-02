using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

internal class PdfGridLayouter : ElementLayouter
{
	internal class RowLayoutResult
	{
		private bool m_bIsFinished;

		private RectangleF m_layoutedBounds;

		public bool IsFinish
		{
			get
			{
				return m_bIsFinished;
			}
			set
			{
				m_bIsFinished = value;
			}
		}

		public RectangleF Bounds
		{
			get
			{
				return m_layoutedBounds;
			}
			set
			{
				m_layoutedBounds = value;
			}
		}

		public RowLayoutResult()
		{
			m_layoutedBounds = default(RectangleF);
		}
	}

	private PdfGraphics m_currentGraphics;

	private PdfPage m_currentPage;

	private SizeF m_currentPageBounds;

	private RectangleF m_currentBounds;

	private List<int[]> m_columnRanges = new List<int[]>();

	private int m_cellStartIndex;

	private int m_cellEndIndex;

	private int m_currentRowIndex;

	private PointF m_startLocation;

	private float m_newheight;

	internal static int m_repeatRowIndex = -1;

	private bool isChanged;

	private PointF m_currentLocation = PointF.Empty;

	private PdfHorizontalOverflowType m_hType;

	private bool flag = true;

	private float m_childHeight;

	private List<int> m_parentCellIndexList;

	private bool isChildGrid;

	private int m_rowBreakPageHeightCellIndex;

	private bool m_isHeader;

	private bool isPaginate;

	private float userHeight;

	private RectangleF m_cellEventBounds;

	private bool m_GridPaginated;

	private PdfStringLayoutResult slr;

	private string remainderText;

	internal bool paginateWithRowBreak;

	internal bool paginateWithoutRowBreak;

	internal bool drawFinalRow;

	internal float endPageHeight;

	internal float currentRowHeight;

	internal int previousPageIndex = -1;

	internal string remainingStringValue = string.Empty;

	private bool drawPaginateHeader;

	internal bool cellInnerGrid;

	private float m_initialHeight;

	internal PdfTextLayoutResult currentHtmlLayoutResult;

	internal float m_currentDrawingRowHeignt;

	private bool isCompleteRowspanBorder;

	internal int previousRowPendingRowSpan = -1;

	private bool previousRowSpanExists;

	internal bool paginateRowSpan;

	private PdfGridCell pendingRowSpanCell;

	private int rowSpanCellIndex = -1;

	internal PdfLayoutParams pdfLayoutParams;

	internal PdfGrid Grid => base.Element as PdfGrid;

	internal PdfGridLayouter(PdfGrid grid)
		: base(grid)
	{
	}

	public void Layout(PdfGraphics graphics, PointF location)
	{
		RectangleF bounds = new RectangleF(location, SizeF.Empty);
		Layout(graphics, bounds);
	}

	public void Layout(PdfGraphics graphics, RectangleF bounds)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		_ = graphics.ClientSize.Width;
		_ = bounds.X;
		PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
		pdfLayoutParams.Bounds = bounds;
		if (pdfLayoutParams.Format == null)
		{
			PdfGridLayoutFormat format = new PdfGridLayoutFormat();
			pdfLayoutParams.Format = format;
		}
		m_currentGraphics = graphics;
		if (graphics.Layer != null)
		{
			int num = 0;
			num = ((!(m_currentGraphics.Page is PdfPage)) ? m_currentGraphics.Page.DefaultLayerIndex : (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentGraphics.Page as PdfPage));
			if (!Grid.m_listOfNavigatePages.Contains(num))
			{
				Grid.m_listOfNavigatePages.Add(num);
			}
		}
		LayoutInternal(pdfLayoutParams);
	}

	protected override PdfLayoutResult LayoutInternal(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		PdfGridLayoutFormat format = GetFormat(param.Format);
		m_currentPage = param.Page;
		if (m_currentGraphics == null)
		{
			m_currentGraphics = param.m_graphics;
		}
		if (m_currentPage != null)
		{
			float height = m_currentPage.GetClientSize().Height;
			float width = m_currentPage.GetClientSize().Width;
			if (height > width || (param.Page.Orientation == PdfPageOrientation.Landscape && format.Break == PdfLayoutBreakType.FitPage))
			{
				m_currentPageBounds = m_currentPage.GetClientSize();
			}
			else
			{
				m_currentPageBounds = m_currentPage.Size;
			}
		}
		else
		{
			m_currentPageBounds = m_currentGraphics.ClientSize;
		}
		m_currentGraphics = ((m_currentPage != null) ? m_currentPage.Graphics : m_currentGraphics);
		if (m_currentGraphics.Layer != null)
		{
			int num = 0;
			num = ((!(m_currentGraphics.Page is PdfPage)) ? m_currentGraphics.Page.DefaultLayerIndex : (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentGraphics.Page as PdfPage));
			if (!Grid.m_listOfNavigatePages.Contains(num))
			{
				Grid.m_listOfNavigatePages.Add(num);
			}
		}
		m_startLocation = param.Bounds.Location;
		userHeight = m_startLocation.Y;
		if (!Grid.IsChildGrid)
		{
			m_hType = Grid.Style.HorizontalOverflowType;
		}
		return LayoutOnPage(param);
	}

	private PdfGridLayoutResult LayoutOnPage(PdfLayoutParams param)
	{
		pdfLayoutParams = param;
		PdfGridLayoutFormat format = GetFormat(param.Format);
		PdfGridEndPageLayoutEventArgs pdfGridEndPageLayoutEventArgs = null;
		PdfGridLayoutResult pdfGridLayoutResult = null;
		Dictionary<PdfPage, int[]> dictionary = new Dictionary<PdfPage, int[]>();
		PdfPage pdfPage = param.Page;
		bool flag = false;
		List<float> list = new List<float>();
		if ((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || Grid.PdfTag != null)
		{
			if (PdfCatalog.StructTreeRoot != null)
			{
				PdfCatalog.StructTreeRoot.isNewTable = true;
			}
			if (Grid.PdfTag == null)
			{
				Grid.PdfTag = new PdfStructureElement(PdfTagType.Table);
				if (Grid.IsChildGrid)
				{
					(Grid.PdfTag as PdfStructureElement).Parent = Grid.ParentCell.PdfTag as PdfStructureElement;
					(Grid.ParentCell.PdfTag as PdfStructureElement).Parent = null;
				}
			}
		}
		RectangleF rectangleF = default(RectangleF);
		if (Grid.IsChildGrid)
		{
			rectangleF = (m_cellEventBounds = param.Bounds);
			m_currentBounds = param.Bounds;
			m_childHeight = m_currentBounds.Height;
		}
		else
		{
			rectangleF = ((format == null || format.Break != PdfLayoutBreakType.FitColumnsToPage) ? new RectangleF(param.Bounds.Location, m_currentGraphics.ClientSize) : new RectangleF(param.Bounds.Location, new SizeF(Grid.Columns.Width, m_currentGraphics.ClientSize.Height)));
			m_currentBounds.Location = param.Bounds.Location;
			if (param.Bounds.Width > 0f)
			{
				if (param.Bounds.Width > m_currentPageBounds.Width)
				{
					param.Bounds = new RectangleF(param.Bounds.X, param.Bounds.Y, m_currentPageBounds.Width, param.Bounds.Height);
				}
				m_currentBounds.Width = param.Bounds.Width;
			}
			else
			{
				m_currentBounds.Width = rectangleF.Width - rectangleF.X;
				rectangleF.Width = m_currentBounds.Width;
			}
			if (param.Bounds.Height > 0f)
			{
				m_currentBounds.Height = param.Bounds.Height;
			}
			else
			{
				m_currentBounds.Height = rectangleF.Height;
				rectangleF.Height = m_currentBounds.Height;
			}
		}
		if (!Grid.Style.AllowHorizontalOverflow)
		{
			if (Grid.IsChildGrid)
			{
				Grid.MeasureColumnsWidth(m_currentBounds);
			}
			else
			{
				Grid.MeasureColumnsWidth(new RectangleF(m_currentBounds.X, m_currentBounds.Y, m_currentBounds.Width + m_currentBounds.X, m_currentBounds.Height));
			}
			m_columnRanges.Add(new int[2]
			{
				0,
				Grid.Columns.Count - 1
			});
		}
		else
		{
			Grid.MeasureColumnsWidth();
			DetermineColumnDrawRanges();
		}
		if (Grid.m_hasRowSpanSpan)
		{
			for (int i = 0; i < Grid.Rows.Count; i++)
			{
				_ = Grid.Rows[i].Height;
				if (!Grid.Rows[i].m_isRowHeightSet)
				{
					Grid.Rows[i].m_isRowHeightSet = true;
				}
				else
				{
					Grid.Rows[i].m_isRowSpanRowHeightSet = true;
				}
			}
		}
		foreach (int[] columnRange in m_columnRanges)
		{
			m_cellStartIndex = columnRange[0];
			m_cellEndIndex = columnRange[1];
			RectangleF currentBounds = rectangleF;
			if (Grid.IsChildGrid)
			{
				currentBounds = param.Bounds;
			}
			if (RaiseBeforePageLayout(m_currentPage, ref currentBounds, ref m_currentRowIndex))
			{
				pdfGridLayoutResult = new PdfGridLayoutResult(m_currentPage, m_currentBounds);
				break;
			}
			if (Grid.isBuiltinStyle && Grid.ParentCell == null && Grid.m_gridBuiltinStyle != PdfGridBuiltinStyle.TableGrid)
			{
				Grid.ApplyBuiltinStyles(Grid.m_gridBuiltinStyle);
			}
			foreach (PdfGridRow header in Grid.Headers)
			{
				float y = m_currentBounds.Y;
				if ((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || header.PdfTag != null)
				{
					PdfCatalog.StructTreeRoot.isNewRow = true;
					if (header.PdfTag == null)
					{
						header.PdfTag = new PdfStructureElement(PdfTagType.TableRow);
					}
					(header.PdfTag as PdfStructureElement).Parent = header.Grid.PdfTag as PdfStructureElement;
				}
				m_isHeader = true;
				if (pdfPage != m_currentPage)
				{
					for (int j = m_cellStartIndex; j <= m_cellEndIndex; j++)
					{
						if (header.Cells[j].IsCellMergeContinue)
						{
							header.Cells[j].IsCellMergeContinue = false;
							header.Cells[j].Value = "";
						}
					}
				}
				RowLayoutResult rowLayoutResult = DrawRow(header);
				bool flag2;
				if (y == m_currentBounds.Y)
				{
					flag2 = true;
					m_repeatRowIndex = Grid.Rows.IndexOf(header);
				}
				else
				{
					flag2 = false;
				}
				if (!rowLayoutResult.IsFinish && pdfPage != null && format.Layout != PdfLayoutType.OnePage && flag2)
				{
					m_startLocation.X = m_currentBounds.X;
					m_currentPage = GetNextPage(format);
					m_startLocation.Y = m_currentBounds.Y;
					if (format.PaginateBounds == RectangleF.Empty)
					{
						m_currentBounds.X += m_startLocation.X;
					}
					DrawRow(header);
				}
				m_isHeader = false;
			}
			int num = 0;
			int count = Grid.Rows.Count;
			float num2 = 0f;
			bool flag3 = true;
			if (flag)
			{
				m_cellEndIndex = (m_cellStartIndex = Grid.parentCellIndex);
				m_parentCellIndexList = new List<int>();
				m_parentCellIndexList.Add(Grid.parentCellIndex);
				Grid.ParentCell.present = true;
				PdfGrid grid = Grid.ParentCell.Row.Grid;
				while (grid.ParentCell != null)
				{
					m_parentCellIndexList.Add(grid.parentCellIndex);
					m_cellEndIndex = grid.parentCellIndex;
					m_cellStartIndex = grid.parentCellIndex;
					grid.ParentCell.present = true;
					grid = grid.ParentCell.Row.Grid;
					if (grid.ParentCell == null)
					{
						m_parentCellIndexList.RemoveAt(m_parentCellIndexList.Count - 1);
					}
				}
				int item = m_currentPage.Section.IndexOf(m_currentPage);
				if (!grid.isDrawn || !grid.m_listOfNavigatePages.Contains(item))
				{
					item = (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentPage);
					grid.isDrawn = true;
					foreach (PdfGridRow row3 in grid.Rows)
					{
						PdfGridRow pdfGridRow2 = row3.CloneGridRow();
						PdfGridCell pdfGridCell = pdfGridRow2.Cells[m_cellStartIndex].Clone(pdfGridRow2.Cells[m_cellStartIndex]);
						pdfGridCell.Value = "";
						PointF location = new PointF(m_currentBounds.X, m_currentBounds.Y);
						float num3 = grid.Columns[m_cellStartIndex].Width;
						if (num3 > m_currentGraphics.ClientSize.Width)
						{
							num3 = m_currentGraphics.ClientSize.Width - 2f * location.X;
						}
						float height = pdfGridCell.Height;
						if (pdfGridRow2.Height > pdfGridCell.Height)
						{
							height = pdfGridRow2.Height;
						}
						pdfGridCell.Draw(m_currentGraphics, new RectangleF(location, new SizeF(num3, height)), cancelSubsequentSpans: false);
						m_currentBounds.Y += height;
					}
					m_currentBounds.Y = 0f;
				}
				DrawParentGridRow(grid);
				m_cellStartIndex = columnRange[0];
				m_cellEndIndex = columnRange[1];
			}
			list.Clear();
			foreach (PdfGridRow row4 in Grid.Rows)
			{
				num++;
				if (previousRowSpanExists && previousRowPendingRowSpan > 0)
				{
					row4.RowSpanExists = previousRowSpanExists;
					row4.maximumRowSpan = previousRowPendingRowSpan;
				}
				m_currentRowIndex = num - 1;
				float y2 = m_currentBounds.Y;
				pdfPage = m_currentPage;
				m_repeatRowIndex = -1;
				if ((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || row4.PdfTag != null)
				{
					PdfCatalog.StructTreeRoot.isNewRow = true;
					if (row4.PdfTag == null)
					{
						row4.PdfTag = new PdfStructureElement(PdfTagType.TableRow);
					}
					(row4.PdfTag as PdfStructureElement).Parent = row4.Grid.PdfTag as PdfStructureElement;
				}
				if (flag3 && row4.Grid.IsChildGrid)
				{
					num2 = y2;
					flag3 = false;
				}
				if (row4.Grid.IsChildGrid && row4.Grid.ParentCell.RowSpan > 1 && (int)(num2 + m_childHeight) < (int)(m_currentBounds.Y + row4.Height) && Grid.Rows.Count > num)
				{
					foreach (PdfGridRow row5 in row4.Grid.ParentCell.Row.Grid.Rows)
					{
						if (row5.Cells[row4.Grid.parentCellIndex] == row4.Grid.ParentCell && row5.Cells[row4.Grid.parentCellIndex].Value is PdfGrid pdfGrid)
						{
							pdfGrid.Rows.RemoveRange(0, num - 1);
						}
					}
				}
				if (row4.Grid.LayoutFormat == null)
				{
					row4.Grid.LayoutFormat = param.Format;
				}
				RowLayoutResult rowLayoutResult2 = DrawRow(row4);
				list.Add(rowLayoutResult2.Bounds.Width);
				if (row4.isRowBreaksNextPage)
				{
					float num4 = 0f;
					for (int k = 0; k < row4.Cells.Count; k++)
					{
						bool flag4 = false;
						if (row4.Height == row4.Cells[k].Height)
						{
							PdfGrid pdfGrid2 = row4.Cells[k].Value as PdfGrid;
							int num5 = pdfGrid2.Rows.Count;
							while (0 < num5)
							{
								if (pdfGrid2.Rows[num5 - 1].RowBreakHeight > 0f)
								{
									flag4 = true;
									break;
								}
								if (pdfGrid2.Rows[num5 - 1].isRowBreaksNextPage)
								{
									row4.rowBreakHeight = pdfGrid2.Rows[num5 - 1].rowBreakHeight;
									break;
								}
								row4.rowBreakHeight += pdfGrid2.Rows[num5 - 1].Height;
								num5--;
							}
						}
						if (flag4)
						{
							break;
						}
					}
					for (int l = 0; l < row4.Cells.Count; l++)
					{
						if (row4.Height > row4.Cells[l].Height)
						{
							row4.Cells[l].Value = string.Empty;
							PdfPage nextPage = GetNextPage(m_currentPage);
							PdfSection section = m_currentPage.Section;
							int num6 = section.IndexOf(nextPage);
							RectangleF bounds;
							for (int m = 0; m < section.Count - 1 - num6; m++)
							{
								bounds = new RectangleF(num4, 0f, row4.Grid.Columns[l].Width, nextPage.GetClientSize().Height);
								m_repeatRowIndex = -1;
								row4.Cells[l].Draw(nextPage.Graphics, bounds, cancelSubsequentSpans: false);
								nextPage = GetNextPage(nextPage);
							}
							bounds = new RectangleF(num4, 0f, row4.Grid.Columns[l].Width, row4.rowBreakHeight);
						}
						num4 += row4.Grid.Columns[l].Width;
					}
				}
				bool flag5;
				if (y2 == m_currentBounds.Y)
				{
					flag5 = true;
					m_repeatRowIndex = Grid.Rows.IndexOf(row4);
				}
				else
				{
					flag5 = false;
					m_repeatRowIndex = -1;
				}
				while (!rowLayoutResult2.IsFinish && pdfPage != null)
				{
					PdfGridLayoutResult pdfGridLayoutResult2 = GetLayoutResult();
					if (pdfPage != m_currentPage)
					{
						if (row4.Grid.IsChildGrid && row4.Grid.ParentCell != null)
						{
							RectangleF rectangleF2 = new RectangleF(format.PaginateBounds.Location, new SizeF(param.Bounds.Width, pdfGridLayoutResult2.Bounds.Height));
							rectangleF2.X += param.Bounds.X;
							if (row4.Grid.ParentCell.Row.Grid.Style.CellPadding != null)
							{
								rectangleF2.Y += row4.Grid.ParentCell.Row.Grid.Style.CellPadding.Top;
								if (rectangleF2.Height > m_currentPageBounds.Height)
								{
									rectangleF2.Height = m_currentPageBounds.Height - rectangleF2.Y;
									rectangleF2.Height -= row4.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
								}
							}
							int n;
							for (n = 0; n < row4.Cells.Count; n++)
							{
								PdfGridCell pdfGridCell2 = row4.Cells[n];
								float num7 = 0f;
								if (pdfGridCell2.ColumnSpan > 1)
								{
									for (; n < pdfGridCell2.ColumnSpan; n++)
									{
										num7 += row4.Grid.Columns[n].Width;
									}
								}
								else
								{
									num7 = Math.Max(pdfGridCell2.Width, row4.Grid.Columns[n].Width);
								}
								rectangleF2.X += num7;
								n += pdfGridCell2.ColumnSpan - 1;
							}
						}
					}
					else if (!row4.Grid.IsChildGrid && !row4.Grid.AllowRowBreakAcrossPages && pdfGridLayoutResult2.Bounds.Width == m_currentPageBounds.Width && pdfGridLayoutResult2.Bounds.X > 0f)
					{
						RectangleF bounds2 = pdfGridLayoutResult2.Bounds;
						float num8 = 0f;
						float num9 = 0f;
						if (row4.Cells[0].Style.Borders != null)
						{
							if (row4.Cells[0].Style.Borders.Bottom != null)
							{
								num8 = row4.Cells[0].Style.Borders.Bottom.Width;
							}
							if (row4.Cells[0].Style.Borders.Right != null)
							{
								_ = row4.Cells[0].Style.Borders.Bottom.Width;
							}
							if (row4.Cells[0].Style.Borders.Top != null)
							{
								num9 = row4.Cells[0].Style.Borders.Bottom.Width;
							}
							if (row4.Cells[0].Style.Borders.Bottom != null)
							{
								_ = row4.Cells[0].Style.Borders.Bottom.Width;
							}
							if (bounds2.X != 0f)
							{
								bounds2.X -= num8;
							}
							if (bounds2.Y != 0f)
							{
								bounds2.Y -= num9;
							}
							pdfGridLayoutResult2 = new PdfGridLayoutResult(m_currentPage, bounds2);
						}
					}
					pdfGridEndPageLayoutEventArgs = RaisePageLayouted(pdfGridLayoutResult2);
					if (pdfGridEndPageLayoutEventArgs.Cancel || flag5)
					{
						break;
					}
					if (Grid.AllowRowBreakAcrossPages)
					{
						if (pdfGridEndPageLayoutEventArgs.NextPage != null)
						{
							m_currentPage = pdfGridEndPageLayoutEventArgs.NextPage;
							m_currentGraphics = m_currentPage.Graphics;
							int item2 = (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentGraphics.Page as PdfPage);
							if (!Grid.m_listOfNavigatePages.Contains(item2))
							{
								Grid.m_listOfNavigatePages.Add(item2);
							}
							m_currentBounds = new RectangleF(PointF.Empty, m_currentPage.GetClientSize());
							if (format.PaginateBounds != RectangleF.Empty)
							{
								m_currentBounds.X = format.PaginateBounds.X;
								m_currentBounds.Y = format.PaginateBounds.Y;
								m_currentBounds.Height = format.PaginateBounds.Size.Height;
							}
						}
						else
						{
							m_currentPage = GetNextPage(format);
						}
						int num10 = m_currentPage.Section.IndexOf(m_currentPage);
						if (!string.IsNullOrEmpty(remainingStringValue) && num10 != previousPageIndex)
						{
							previousPageIndex = num10;
							if (Grid.RepeatHeader)
							{
								m_isHeader = true;
								m_currentBounds.Location = param.Bounds.Location;
								foreach (PdfGridRow header2 in Grid.Headers)
								{
									DrawRow(header2);
									drawPaginateHeader = true;
								}
								m_isHeader = false;
							}
						}
						if (format != null && format.PaginateBounds != RectangleF.Empty)
						{
							m_currentBounds = format.PaginateBounds;
						}
						else
						{
							if (!drawPaginateHeader)
							{
								if (!Grid.IsChildGrid)
								{
									if (pdfLayoutParams.Bounds.Width != 0f && pdfLayoutParams.Bounds.Height != 0f)
									{
										m_currentBounds.Location = m_startLocation;
									}
									else
									{
										m_currentBounds.Location = new PointF(m_startLocation.X, 0f);
									}
								}
								else if (row4.RowIndex != -1)
								{
									RectangleF layoutBounds = row4.Grid.Rows[(row4.RowIndex > 0) ? (row4.RowIndex - 1) : 0].Cells[0].layoutBounds;
									float num11 = 0f;
									float num12 = 0f;
									for (PdfGridCell parentCell = row4.Grid.ParentCell; parentCell != null; parentCell = parentCell.Row.Grid.ParentCell)
									{
										if (parentCell.Style != null && parentCell.Style.Borders != null && parentCell.Style.Borders != null)
										{
											if (Grid.Style.CellPadding != null && parentCell.Row.Grid.Style.CellPadding.Left <= 1f)
											{
												layoutBounds.X -= parentCell.Row.Grid.Style.CellPadding.Left;
											}
											if (parentCell.Style.Borders.Top != null)
											{
												num11 += parentCell.Style.Borders.Top.Width;
											}
											if (Grid.Style.CellPadding != null && parentCell.Row.Grid.Style.CellPadding.Top > 1f && !(row4.RowBreakHeight > m_currentPageBounds.Height))
											{
												num11 += parentCell.Row.Grid.Style.CellPadding.Top;
											}
											if (parentCell.Style.Borders.Top != null && parentCell.Style.Borders.Bottom != null)
											{
												num12 += parentCell.Style.Borders.Top.Width + parentCell.Style.Borders.Bottom.Width;
											}
											num12 += parentCell.Row.Grid.Style.CellPadding.Bottom;
											if (parentCell.Style.Borders.Left != null && parentCell.Style.Borders.Right != null)
											{
												layoutBounds.Width += (parentCell.Style.Borders.Left.Width + parentCell.Style.Borders.Right.Width) / 2f;
											}
										}
									}
									m_currentBounds = new RectangleF(layoutBounds.X, num11, layoutBounds.Width, m_currentBounds.Height - num12);
								}
							}
							else
							{
								drawPaginateHeader = false;
							}
							if (!Grid.IsChildGrid && m_currentBounds.X != 0f)
							{
								m_currentBounds.Width -= m_currentBounds.X;
							}
							if (row4.Grid.ParentCell != null)
							{
								row4.Grid.ParentCell.Row.isRowBreaksNextPage = true;
								row4.Grid.ParentCell.Row.rowBreakHeight = row4.RowBreakHeight + Grid.ParentCell.Row.Grid.Style.CellPadding.Top + Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
							}
						}
						if (Grid.IsChildGrid)
						{
							m_cellEventBounds = m_currentBounds;
						}
						if (((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || row4.PdfTag != null) && param.Page != null && param.Page.Document != null && param.Page.Document.SeparateTable)
						{
							PdfCatalog.StructTreeRoot.isNewTable = true;
							PdfGridRow pdfGridRow3 = row4;
							bool flag6 = false;
							if (pdfGridRow3.Grid.IsChildGrid)
							{
								while (pdfGridRow3.Grid.IsChildGrid)
								{
									PdfGrid grid2 = pdfGridRow3.Grid;
									PdfGrid grid3 = pdfGridRow3.Grid.ParentCell.Row.Grid;
									if (!pdfGridRow3.Grid.ParentCell.Row.Grid.IsChildGrid && flag6)
									{
										PdfStructureElement parent = new PdfStructureElement(PdfTagType.Table);
										PdfStructureElement pdfStructureElement = new PdfStructureElement(PdfTagType.TableRow);
										pdfStructureElement.Parent = parent;
										PdfStructureElement pdfStructureElement2 = new PdfStructureElement(PdfTagType.TableDataCell);
										pdfStructureElement2.Parent = pdfStructureElement;
										(grid2.PdfTag as PdfStructureElement).Parent = pdfStructureElement2;
										(grid3.PdfTag as PdfStructureElement).Parent = grid2.PdfTag as PdfStructureElement;
									}
									else
									{
										PdfStructureElement pdfStructureElement3 = new PdfStructureElement(PdfTagType.Table);
										PdfStructureElement pdfStructureElement4 = new PdfStructureElement(PdfTagType.TableRow);
										pdfStructureElement4.Parent = pdfStructureElement3;
										PdfStructureElement pdfStructureElement5 = new PdfStructureElement(PdfTagType.TableDataCell);
										pdfStructureElement5.Parent = pdfStructureElement4;
										PdfStructureElement pdfStructureElement6 = new PdfStructureElement(PdfTagType.Table);
										pdfStructureElement6.Parent = pdfStructureElement5;
										PdfStructureElement pdfStructureElement7 = new PdfStructureElement(PdfTagType.TableRow);
										pdfStructureElement7.Parent = pdfStructureElement6;
										new PdfStructureElement(PdfTagType.TableDataCell).Parent = pdfStructureElement7;
										pdfGridRow3.PdfTag = pdfStructureElement7;
										grid2.PdfTag = pdfStructureElement6;
										grid3.PdfTag = pdfStructureElement3;
										flag6 = true;
									}
									pdfGridRow3 = pdfGridRow3.Grid.ParentCell.Row;
								}
								PdfCatalog.StructTreeRoot.m_SplitTable = true;
							}
						}
						if (RaiseBeforePageLayout(m_currentPage, ref currentBounds, ref m_currentRowIndex))
						{
							break;
						}
						if (row4.m_noOfPageCount > 1)
						{
							float rowBreakHeight = row4.RowBreakHeight;
							for (int num13 = 1; num13 < row4.m_noOfPageCount; num13++)
							{
								row4.RowBreakHeight = 0f;
								row4.Height = (float)(row4.m_noOfPageCount - 1) * m_currentPage.GetClientSize().Height;
								DrawRow(row4);
								m_currentPage = GetNextPage(format);
								pdfPage = m_currentPage;
							}
							row4.RowBreakHeight = rowBreakHeight;
							row4.m_noOfPageCount = 1;
							rowLayoutResult2 = DrawRow(row4);
						}
						else
						{
							rowLayoutResult2 = DrawRow(row4);
						}
					}
					else
					{
						if (!Grid.AllowRowBreakAcrossPages && num < count)
						{
							m_currentPage = GetNextPage(format);
							break;
						}
						if (num >= count)
						{
							break;
						}
					}
				}
				if (isCompleteRowspanBorder)
				{
					isCompleteRowspanBorder = false;
					previousRowPendingRowSpan = row4.maximumRowSpan;
					previousRowSpanExists = row4.RowSpanExists;
				}
				if (!rowLayoutResult2.IsFinish && pdfPage != null && format.Layout != PdfLayoutType.OnePage && flag5)
				{
					m_startLocation.X = m_currentBounds.X;
					bool flag7 = false;
					if (!Grid.isSignleGrid)
					{
						for (int num14 = 0; num14 < Grid.Rows.Count; num14++)
						{
							bool flag8 = false;
							for (int num15 = 0; num15 < Grid.Rows[num14].Cells.Count; num15++)
							{
								if (Grid.Rows[num14].Cells[num15].Width > m_currentPageBounds.Width)
								{
									flag8 = true;
								}
							}
							if (flag8 && Grid.Rows[num14].Cells[m_rowBreakPageHeightCellIndex].m_pageCount > 0)
							{
								flag7 = true;
							}
						}
					}
					if (!Grid.m_isRearranged && flag7)
					{
						PdfSection section2 = m_currentPage.Section;
						m_currentPage = section2.Add();
						m_currentGraphics = m_currentPage.Graphics;
						m_currentBounds = new RectangleF(PointF.Empty, m_currentPage.GetClientSize());
						int item3 = (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentGraphics.Page as PdfPage);
						if (!Grid.m_listOfNavigatePages.Contains(item3))
						{
							Grid.m_listOfNavigatePages.Add(item3);
						}
					}
					else if (pdfGridEndPageLayoutEventArgs.NextPage == null)
					{
						m_currentPage = GetNextPage(format);
					}
					else
					{
						m_currentPage = pdfGridEndPageLayoutEventArgs.NextPage;
						m_currentGraphics = pdfGridEndPageLayoutEventArgs.NextPage.Graphics;
						m_currentBounds = new RectangleF(PointF.Empty, pdfGridEndPageLayoutEventArgs.NextPage.GetClientSize());
					}
					_ = m_currentPage.Section;
					_ = param.Page.Section;
					if (format != null && format.PaginateBounds != RectangleF.Empty)
					{
						m_currentBounds = format.PaginateBounds;
					}
					else
					{
						if (!Grid.IsChildGrid)
						{
							m_currentBounds.Location = new PointF(m_startLocation.X, 0f);
						}
						else if (row4.RowIndex != -1)
						{
							RectangleF layoutBounds2 = row4.Grid.Rows[(row4.RowIndex > 0) ? (row4.RowIndex - 1) : 0].Cells[0].layoutBounds;
							float num16 = 0f;
							float num17 = 0f;
							PdfGridCell parentCell2 = row4.Grid.ParentCell;
							PdfGridRow pdfGridRow4 = null;
							while (parentCell2 != null)
							{
								if (parentCell2.Style != null && parentCell2.Style.Borders != null && parentCell2.Style.Borders != null)
								{
									if (Grid.Style.CellPadding != null)
									{
										layoutBounds2.X -= Grid.Style.CellPadding.Left;
									}
									if (parentCell2.Style.Borders.Top != null)
									{
										num16 += parentCell2.Style.Borders.Top.Width / 2f + Grid.Style.CellPadding.Top;
									}
									if (parentCell2.Style.Borders.Top != null && parentCell2.Style.Borders.Bottom != null)
									{
										num17 += parentCell2.Style.Borders.Top.Width + parentCell2.Style.Borders.Bottom.Width;
									}
									if (parentCell2.Style.Borders.Left != null && parentCell2.Style.Borders.Right != null)
									{
										layoutBounds2.Width += (parentCell2.Style.Borders.Left.Width + parentCell2.Style.Borders.Right.Width) / 2f;
									}
								}
								pdfGridRow4 = parentCell2.Row;
								parentCell2 = parentCell2.Row.Grid.ParentCell;
							}
							m_currentBounds = new RectangleF(layoutBounds2.X, num16, layoutBounds2.Width, m_currentBounds.Height - num17);
							if (m_currentBounds.X != m_startLocation.X)
							{
								m_currentBounds.X = m_startLocation.X;
							}
							if (row4.Cells[0].Value is PdfGrid)
							{
								m_currentBounds.Y = 0f;
								if (pdfGridRow4 != null)
								{
									pdfGridRow4.m_paginatedGridRow = true;
								}
							}
						}
						if (!Grid.IsChildGrid && m_currentBounds.X != 0f)
						{
							m_currentBounds.Width -= m_currentBounds.X;
						}
					}
					if (RaiseBeforePageLayout(m_currentPage, ref m_currentBounds, ref m_currentRowIndex))
					{
						break;
					}
					m_startLocation.Y = m_currentBounds.Y;
					if (Grid.IsChildGrid)
					{
						m_cellEventBounds = m_currentBounds;
					}
					if (((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || row4.PdfTag != null) && param.Page != null && param.Page.Document != null && param.Page.Document.SeparateTable)
					{
						PdfCatalog.StructTreeRoot.isNewTable = true;
						PdfGridRow pdfGridRow5 = row4;
						if (pdfGridRow5.Grid.IsChildGrid)
						{
							bool flag9 = false;
							while (pdfGridRow5.Grid.IsChildGrid)
							{
								PdfGrid grid4 = pdfGridRow5.Grid;
								PdfGrid grid5 = pdfGridRow5.Grid.ParentCell.Row.Grid;
								if (!pdfGridRow5.Grid.ParentCell.Row.Grid.IsChildGrid && flag9)
								{
									PdfStructureElement parent2 = new PdfStructureElement(PdfTagType.Table);
									PdfStructureElement pdfStructureElement8 = new PdfStructureElement(PdfTagType.TableRow);
									pdfStructureElement8.Parent = parent2;
									PdfStructureElement pdfStructureElement9 = new PdfStructureElement(PdfTagType.TableDataCell);
									pdfStructureElement9.Parent = pdfStructureElement8;
									(grid4.PdfTag as PdfStructureElement).Parent = pdfStructureElement9;
									(grid5.PdfTag as PdfStructureElement).Parent = grid4.PdfTag as PdfStructureElement;
								}
								else
								{
									PdfStructureElement pdfStructureElement10 = new PdfStructureElement(PdfTagType.Table);
									PdfStructureElement pdfStructureElement11 = new PdfStructureElement(PdfTagType.TableRow);
									pdfStructureElement11.Parent = pdfStructureElement10;
									PdfStructureElement pdfStructureElement12 = new PdfStructureElement(PdfTagType.TableDataCell);
									pdfStructureElement12.Parent = pdfStructureElement11;
									PdfStructureElement pdfStructureElement13 = new PdfStructureElement(PdfTagType.Table);
									pdfStructureElement13.Parent = pdfStructureElement12;
									PdfStructureElement pdfStructureElement14 = new PdfStructureElement(PdfTagType.TableRow);
									pdfStructureElement14.Parent = pdfStructureElement13;
									new PdfStructureElement(PdfTagType.TableDataCell).Parent = pdfStructureElement14;
									pdfGridRow5.PdfTag = pdfStructureElement14;
									grid4.PdfTag = pdfStructureElement13;
									grid5.PdfTag = pdfStructureElement10;
									flag9 = true;
								}
								pdfGridRow5 = pdfGridRow5.Grid.ParentCell.Row;
							}
							PdfCatalog.StructTreeRoot.m_SplitTable = true;
						}
					}
					if (Grid.RepeatHeader)
					{
						m_isHeader = true;
						foreach (PdfGridRow header3 in Grid.Headers)
						{
							if ((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || row4.PdfTag != null)
							{
								PdfCatalog.StructTreeRoot.isNewRow = true;
								if (row4.PdfTag == null)
								{
									row4.PdfTag = new PdfStructureElement(PdfTagType.TableRow);
								}
								(row4.PdfTag as PdfStructureElement).Parent = row4.Grid.PdfTag as PdfStructureElement;
							}
							DrawRow(header3);
						}
						if ((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || row4.PdfTag != null)
						{
							PdfCatalog.StructTreeRoot.isNewRow = true;
							if (row4.PdfTag == null)
							{
								row4.PdfTag = new PdfStructureElement(PdfTagType.TableRow);
							}
							(row4.PdfTag as PdfStructureElement).Parent = row4.Grid.PdfTag as PdfStructureElement;
						}
						m_isHeader = false;
					}
					if (previousRowSpanExists && previousRowPendingRowSpan > 0)
					{
						row4.RowSpanExists = previousRowSpanExists;
						row4.maximumRowSpan = previousRowPendingRowSpan;
					}
					DrawRow(row4);
					if (m_currentPage != null && !dictionary.ContainsKey(m_currentPage))
					{
						dictionary.Add(m_currentPage, columnRange);
					}
				}
				if (row4.NestedGridLayoutResult != null)
				{
					m_currentPage = row4.NestedGridLayoutResult.Page;
					m_currentGraphics = m_currentPage.Graphics;
					m_startLocation = row4.NestedGridLayoutResult.Bounds.Location;
					m_currentBounds.Y = row4.NestedGridLayoutResult.Bounds.Bottom;
					if (pdfPage == m_currentPage)
					{
						continue;
					}
					PdfSection section3 = m_currentPage.Section;
					int num18 = section3.IndexOf(pdfPage) + 1;
					int num19 = section3.IndexOf(m_currentPage);
					for (int num20 = num18; num20 < num19 + 1; num20++)
					{
						PdfGraphics graphics = section3[num20].Graphics;
						PointF location2 = format.PaginateBounds.Location;
						if (location2 == PointF.Empty && m_currentBounds.X > location2.X && !row4.Grid.IsChildGrid && row4.Grid.ParentCell == null)
						{
							location2.X = m_currentBounds.X;
						}
						float num21 = ((num20 == num19) ? (row4.NestedGridLayoutResult.Bounds.Height - param.Bounds.Y) : (m_currentBounds.Height - location2.Y));
						if (param.Bounds.Y == 0f && row4.Grid.Style != null && row4.Grid.Style.CellPadding != null && num20 == num19 && row4.NestedGridLayoutResult.Bounds.Y > 0f)
						{
							if (row4.Grid.Style.CellPadding.Bottom > 0.5f)
							{
								num21 += row4.Grid.Style.CellPadding.Bottom;
							}
							if (row4.Grid.Style.CellPadding.Top > 0.5f)
							{
								num21 += row4.Grid.Style.CellPadding.Top;
							}
						}
						if (num21 <= graphics.ClientSize.Height)
						{
							num21 += param.Bounds.Y;
						}
						if (row4.Grid.IsChildGrid && row4.Grid.ParentCell != null)
						{
							location2.X += param.Bounds.X;
						}
						location2.Y = format?.PaginateBounds.Location.Y ?? 0f;
						float num22 = 0f;
						float num23 = 0f;
						float num24 = 0f;
						for (int num25 = 0; num25 < row4.Cells.Count; num25++)
						{
							PdfGridCell pdfGridCell3 = row4.Cells[num25];
							PdfGridCell parentCell3 = row4.Grid.ParentCell;
							while (parentCell3 != null && pdfGridCell3.Value is PdfGrid)
							{
								if (parentCell3.Style != null && parentCell3.Style.Borders != null && parentCell3.Style.Borders.Top != null)
								{
									num24 += parentCell3.Style.Borders.Top.Width;
								}
								parentCell3 = parentCell3.Row.Grid.ParentCell;
							}
							if (parentCell3 == null && pdfGridCell3.Value is PdfGrid && pdfGridCell3.Style != null && pdfGridCell3.Style.Borders != null && pdfGridCell3.Style.Borders.Top != null)
							{
								num23 += pdfGridCell3.Style.Borders.Top.Width;
							}
						}
						if (num24 != 0f)
						{
							num22 = num21;
							num22 += num24;
							if (graphics.m_cellBorderMaxHeight < num22)
							{
								graphics.m_cellBorderMaxHeight = num22;
							}
						}
						else if (graphics.m_cellBorderMaxHeight > 0f)
						{
							if (graphics.m_cellBorderMaxHeight > m_currentPageBounds.Height)
							{
								m_currentBounds.Y = m_currentPageBounds.Height - row4.Cells[0].Style.Borders.Top.Width / 2f;
								num21 = m_currentPageBounds.Height - row4.Cells[0].Style.Borders.Top.Width / 2f;
							}
							else
							{
								num21 = graphics.m_cellBorderMaxHeight;
								if (m_currentBounds.Y + row4.Cells[0].Style.Borders.Top.Width < m_currentPageBounds.Height)
								{
									num21 += num23 * 2f;
								}
								if (row4.Cells.Count > 0 && row4.Cells[0].Style != null && row4.Cells[0].Style.Borders != null && row4.Cells[0].Style.Borders.Top != null)
								{
									num21 += row4.Cells[0].Style.Borders.Top.Width;
								}
								if (row4.Cells.Count > 0 && row4.Cells[0].Style != null && row4.Cells[0].Style.Borders != null && row4.Cells[0].Style.Borders.Top != null)
								{
									m_currentBounds.Y = num21 + row4.Cells[0].Style.Borders.Top.Width;
								}
								else
								{
									m_currentBounds.Y = num21;
								}
							}
						}
						else if (num21 < m_currentPageBounds.Height)
						{
							float num26 = format?.PaginateBounds.Location.Y ?? 0f;
							if (num26 == 0f && row4.Grid.isSignleGrid)
							{
								if (row4.Cells.Count > 0 && row4.Cells[0].Style != null && row4.Cells[0].Style.Borders != null && row4.Cells[0].Style.Borders.Top != null)
								{
									num21 += row4.Cells[0].Style.Borders.Top.Width;
								}
								if (row4.Cells.Count > 0 && row4.Cells[0].Style != null && row4.Cells[0].Style.Borders != null && row4.Cells[0].Style.Borders.Top != null)
								{
									m_currentBounds.Y = num21 + row4.Cells[0].Style.Borders.Top.Width;
								}
								else
								{
									m_currentBounds.Y = num21;
								}
							}
							else if (num26 == 0f && !row4.Grid.isSignleGrid)
							{
								if (row4.rowBreakHeight == 0f && row4.Cells.Count > 0 && row4.Cells[0].Style != null && row4.Cells[0].Style.Borders != null && row4.Cells[0].Style.Borders.Top != null)
								{
									num21 += row4.Cells[0].Style.Borders.Top.Width;
								}
								if (!row4.m_paginatedGridRow && (!(param.Bounds.X > 0f) || param.Bounds.Y != 0f) && row4.Cells.Count > 0 && row4.Cells[0].Style != null && row4.Cells[0].Style.Borders != null && row4.Cells[0].Style.Borders.Top != null)
								{
									m_currentBounds.Y = num21 + row4.Cells[0].Style.Borders.Top.Width;
								}
								else
								{
									m_currentBounds.Y = num21;
								}
							}
						}
						bool flag10 = false;
						int num27;
						for (num27 = 0; num27 < row4.Cells.Count; num27++)
						{
							PdfGridCell pdfGridCell4 = row4.Cells[num27];
							float num28 = 0f;
							if (pdfGridCell4.ColumnSpan <= 1)
							{
								num28 = ((!Grid.isWidthSet) ? Math.Max(pdfGridCell4.Width, row4.Grid.Columns[num27].Width) : ((Grid.LastRow == null || Grid.LastRow.Width != row4.Grid.Columns[num27].Width) ? Math.Min(pdfGridCell4.Width, row4.Grid.Columns[num27].Width) : Grid.LastRow.Width));
							}
							else
							{
								for (num27 = 0; num27 < pdfGridCell4.ColumnSpan; num27++)
								{
									num28 += row4.Grid.Columns[num27].Width;
								}
								flag10 = true;
							}
							if (num28 < Grid.LastRow.Width && flag10)
							{
								num28 = Grid.LastRow.Width;
							}
							float num29 = 0f;
							float num30 = 0f;
							float num31 = 0f;
							int index = num27;
							if (pdfGridCell4.ColumnSpan > 1)
							{
								index = pdfGridCell4.ColumnSpan - 1;
							}
							if (row4.Cells[index].Style.Borders != null)
							{
								if (row4.Cells[index].Style.Borders.Left != null)
								{
									num29 = row4.Cells[index].Style.Borders.Left.Width;
								}
								if (row4.Cells[index].Style.Borders.Right != null)
								{
									num30 = row4.Cells[index].Style.Borders.Right.Width;
								}
								if (row4.Cells[index].Style.Borders.Top != null)
								{
									num31 = row4.Cells[index].Style.Borders.Top.Width;
								}
								if (row4.Cells[index].Style.Borders.Bottom != null)
								{
									_ = row4.Cells[index].Style.Borders.Bottom.Width;
								}
							}
							if (location2.X == 0f)
							{
								location2.X += num29 / 2f;
								if (graphics.m_cellBorderMaxHeight == 0f)
								{
									num28 -= num29 / 2f;
								}
							}
							if (location2.Y == 0f)
							{
								if (pdfGridCell4.layoutBounds.Y != 0f && row4.NestedGridLayoutResult != null && pdfGridCell4.Value is PdfGrid)
								{
									location2.Y = num24 + num31 / 2f;
									if (num24 != 0f)
									{
										location2.X += num29 / 2f;
									}
									if (graphics.m_cellBorderMaxHeight > 0f)
									{
										num28 -= (num29 + num30) / 2f;
									}
									float cellBorderMaxHeight = graphics.m_cellBorderMaxHeight;
									num21 = ((!(graphics.m_cellBorderMaxHeight > 0f)) ? (num21 + num31 / 2f) : ((cellBorderMaxHeight != num21 + num24) ? (cellBorderMaxHeight + num24 + num31 / 2f) : (cellBorderMaxHeight - num31 / 2f)));
								}
								else
								{
									location2.Y = num24 + num31 / 2f;
									if (num24 != 0f)
									{
										float cellBorderMaxHeight2 = graphics.m_cellBorderMaxHeight;
										num21 = ((cellBorderMaxHeight2 != num21 + num24) ? (cellBorderMaxHeight2 + num24 + num31 / 2f) : (cellBorderMaxHeight2 - num31 / 2f));
									}
									else
									{
										num21 += num31 / 2f;
									}
								}
							}
							if (param.Bounds.Y < row4.NestedGridLayoutResult.Bounds.Y && num21 < graphics.ClientSize.Height && row4.Cells.Count == 1 && graphics.m_cellBorderMaxHeight > 0f)
							{
								num21 = ((param.Bounds.Y == 0f) ? (num21 + row4.NestedGridLayoutResult.Bounds.Y) : (num21 + param.Bounds.Y));
							}
							pdfGridCell4.DrawCellBorders(ref graphics, new RectangleF(location2, new SizeF(num28, num21)));
							location2.X += num28;
							num27 += pdfGridCell4.ColumnSpan - 1;
						}
					}
					pdfPage = m_currentPage;
				}
				else if (pdfPage != m_currentPage && !rowLayoutResult2.IsFinish && !Grid.AllowRowBreakAcrossPages)
				{
					PointF pointF = new PointF(PdfBorders.Default.Right.Width / 2f, PdfBorders.Default.Top.Width / 2f);
					if (format.PaginateBounds == RectangleF.Empty && m_startLocation == pointF)
					{
						m_currentBounds.X += m_startLocation.X;
						m_currentBounds.Y += m_startLocation.Y;
					}
				}
			}
			bool flag11 = false;
			float num32 = 0f;
			if (list.Count > 0)
			{
				num32 = list[0];
			}
			float[,] array = new float[1, 2];
			for (int num33 = 0; num33 < Grid.Rows.Count; num33++)
			{
				if (m_cellEndIndex != -1 && Grid.Rows[num33].Cells[m_cellEndIndex].Value is PdfGrid)
				{
					PdfGrid pdfGrid3 = Grid.Rows[num33].Cells[m_cellEndIndex].Value as PdfGrid;
					Grid.m_rowLayoutBoundswidth = pdfGrid3.m_rowLayoutBoundswidth;
					flag11 = true;
					if (array[0, 0] < (float)pdfGrid3.m_listOfNavigatePages.Count)
					{
						array[0, 0] = pdfGrid3.m_listOfNavigatePages.Count;
						array[0, 1] = list[num33];
					}
					else if (array[0, 0] == (float)pdfGrid3.m_listOfNavigatePages.Count && array[0, 1] < list[num33])
					{
						array[0, 1] = list[num33];
					}
				}
			}
			if (!flag11 && list.Count > 0)
			{
				for (int num34 = 0; num34 < num - 1; num34++)
				{
					if (num32 < list[num34])
					{
						num32 = list[num34];
					}
				}
				Grid.m_rowLayoutBoundswidth = num32;
			}
			else
			{
				Grid.m_rowLayoutBoundswidth = array[0, 1];
			}
			if (m_columnRanges.IndexOf(columnRange) >= m_columnRanges.Count - 1 || pdfPage == null || format.Layout == PdfLayoutType.OnePage)
			{
				continue;
			}
			flag = Grid.IsChildGrid;
			if ((int)array[0, 0] != 0)
			{
				PdfSection section4 = m_currentPage.Section;
				int num35 = section4.IndexOf(m_currentPage);
				if (section4.Count > num35 + (int)array[0, 0])
				{
					m_currentPage = section4[num35 + (int)array[0, 0]];
				}
				else
				{
					m_currentPage = section4.Add();
				}
				m_currentGraphics = m_currentPage.Graphics;
				m_currentBounds = new RectangleF(PointF.Empty, m_currentPage.GetClientSize());
				int item4 = (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentGraphics.Page as PdfPage);
				if (!Grid.m_listOfNavigatePages.Contains(item4))
				{
					Grid.m_listOfNavigatePages.Add(item4);
				}
			}
			else
			{
				m_currentPage = GetNextPage(format);
			}
			PointF pointF2 = new PointF(PdfBorders.Default.Right.Width / 2f, PdfBorders.Default.Top.Width / 2f);
			if (format.PaginateBounds == RectangleF.Empty && m_startLocation == pointF2)
			{
				m_currentBounds.X += m_startLocation.X;
				m_currentBounds.Y += m_startLocation.Y;
			}
		}
		if (!Grid.IsChildGrid)
		{
			foreach (PdfGridRow row6 in Grid.Rows)
			{
				row6.RowBreakHeight = 0f;
			}
		}
		pdfGridLayoutResult = GetLayoutResult();
		if (Grid.Style.AllowHorizontalOverflow && Grid.Style.HorizontalOverflowType == PdfHorizontalOverflowType.NextPage)
		{
			ReArrangePages(dictionary);
		}
		if (PdfCatalog.StructTreeRoot != null && PdfCatalog.StructTreeRoot.m_isChildGrid)
		{
			PdfCatalog.StructTreeRoot.m_isChildGrid = false;
			PdfCatalog.StructTreeRoot.m_isNestedGridRendered = true;
		}
		if (!Grid.IsChildGrid)
		{
			PdfGridLayoutResult result = pdfGridLayoutResult;
			RectangleF bounds3 = pdfGridLayoutResult.Bounds;
			PdfGridRow pdfGridRow6 = null;
			if (Grid.Rows.Count > 0)
			{
				pdfGridRow6 = Grid.Rows[0];
			}
			if (pdfGridRow6 != null && pdfGridRow6.NestedGridLayoutResult != null)
			{
				float num36 = 0f;
				float num37 = 0f;
				if (pdfGridRow6.Cells[0].Style.Borders != null)
				{
					if (pdfGridRow6.Cells[0].Style.Borders.Bottom != null)
					{
						num36 = pdfGridRow6.Cells[0].Style.Borders.Bottom.Width;
					}
					if (pdfGridRow6.Cells[0].Style.Borders.Right != null)
					{
						_ = pdfGridRow6.Cells[0].Style.Borders.Bottom.Width;
					}
					if (pdfGridRow6.Cells[0].Style.Borders.Top != null)
					{
						num37 = pdfGridRow6.Cells[0].Style.Borders.Bottom.Width;
					}
					if (pdfGridRow6.Cells[0].Style.Borders.Bottom != null)
					{
						_ = pdfGridRow6.Cells[0].Style.Borders.Bottom.Width;
					}
					if (bounds3.X != 0f)
					{
						bounds3.X -= num36;
					}
					if (bounds3.Y != 0f)
					{
						bounds3.Y -= num37;
					}
					result = new PdfGridLayoutResult(m_currentPage, bounds3);
				}
			}
			RaisePageLayouted(result);
		}
		else
		{
			RaisePageLayouted(pdfGridLayoutResult);
		}
		return pdfGridLayoutResult;
	}

	private bool DrawParentGridRow(PdfGrid grid)
	{
		bool result = false;
		grid.isDrawn = true;
		float num = m_currentBounds.Y;
		foreach (PdfGridRow row in grid.Rows)
		{
			PdfGridRow pdfGridRow = row.CloneGridRow();
			PdfGridCell pdfGridCell = pdfGridRow.Cells[m_cellStartIndex].Clone(pdfGridRow.Cells[m_cellStartIndex]);
			pdfGridCell.Value = "";
			PointF location = new PointF(m_currentBounds.X, m_currentBounds.Y);
			float num2 = grid.Columns[m_cellStartIndex].Width;
			if (num2 > m_currentGraphics.ClientSize.Width)
			{
				num2 = m_currentGraphics.ClientSize.Width - 2f * location.X;
			}
			float height = pdfGridCell.Height;
			if (pdfGridRow.Height > pdfGridCell.Height)
			{
				height = pdfGridRow.Height;
			}
			if (isChildGrid)
			{
				pdfGridCell.Draw(m_currentGraphics, new RectangleF(location, new SizeF(num2, height)), cancelSubsequentSpans: false);
			}
			m_currentBounds.Y += height;
		}
		for (int i = 0; i < grid.Rows.Count; i++)
		{
			if (grid.Rows[i].Cells[m_cellStartIndex].present)
			{
				result = true;
				if (grid.Rows[i].Cells[m_cellStartIndex].Value is PdfGrid)
				{
					PdfGrid pdfGrid = grid.Rows[i].Cells[m_cellStartIndex].Value as PdfGrid;
					grid.Rows[i].Cells[m_cellStartIndex].present = false;
					if (pdfGrid == Grid)
					{
						if (!isChildGrid)
						{
							m_currentBounds.Y = num;
						}
						else if (i == 0)
						{
							m_currentBounds.Y -= grid.Size.Height;
						}
						else
						{
							for (int j = i; j < grid.Rows.Count; j++)
							{
								m_currentBounds.Y -= grid.Rows[j].Height;
							}
						}
						PdfGrid pdfGrid2 = pdfGrid.Clone(pdfGrid);
						pdfGrid2.isDrawn = true;
						grid.Rows[i].Cells[m_cellStartIndex].Value = pdfGrid2;
						m_currentBounds.X += grid.Style.CellPadding.Left + grid.Style.CellPadding.Right;
						m_currentBounds.Y += grid.Style.CellPadding.Top + grid.Style.CellPadding.Bottom;
						m_currentBounds.Width -= 2f * m_currentBounds.X;
					}
					else
					{
						isChildGrid = true;
						if (m_parentCellIndexList.Count > 0)
						{
							m_cellStartIndex = m_parentCellIndexList[m_parentCellIndexList.Count - 1];
							m_parentCellIndexList.RemoveAt(m_parentCellIndexList.Count - 1);
						}
						m_currentBounds.Y = num;
						m_currentBounds.X += grid.Style.CellPadding.Left + grid.Style.CellPadding.Right;
						m_currentBounds.Y += grid.Style.CellPadding.Top + grid.Style.CellPadding.Bottom;
						if (!DrawParentGridRow(pdfGrid))
						{
							m_currentBounds.Y -= pdfGrid.Size.Height;
						}
						isChildGrid = false;
					}
					break;
				}
				num += grid.Rows[i].Height;
			}
			else
			{
				num += grid.Rows[i].Height;
			}
		}
		return result;
	}

	private void ReArrangePages(Dictionary<PdfPage, int[]> layoutedPages)
	{
		PdfDocument document = m_currentPage.Document;
		List<PdfPage> list = new List<PdfPage>();
		foreach (PdfPage key in layoutedPages.Keys)
		{
			key.Section = null;
			list.Add(key);
			document.Pages.Remove(key);
		}
		for (int i = 0; i < layoutedPages.Count; i++)
		{
			int j = i;
			for (int num = layoutedPages.Count / m_columnRanges.Count; j < layoutedPages.Count; j += num)
			{
				PdfPage page = list[j];
				if (document.Pages.IndexOf(page) == -1)
				{
					document.Pages.Add(page);
				}
			}
		}
	}

	private RowLayoutResult DrawRow(PdfGridRow row)
	{
		RowLayoutResult result = new RowLayoutResult();
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		_ = PointF.Empty;
		_ = SizeF.Empty;
		bool flag = false;
		bool flag2 = true;
		if (row.RowSpanExists)
		{
			int num4 = Grid.Rows.IndexOf(row);
			int maximumRowSpan = row.maximumRowSpan;
			if (num4 == -1)
			{
				num4 = Grid.Headers.IndexOf(row);
				if (num4 != -1)
				{
					flag = true;
				}
			}
			int num5 = 0;
			for (int i = num4; i < num4 + maximumRowSpan; i++)
			{
				num2 += (flag ? Grid.Headers[i].Height : Grid.Rows[i].Height);
				if (num5 < 2)
				{
					num3 = num2;
				}
				num5++;
			}
			currentRowHeight = num2;
			float num6 = 0f;
			for (int j = num4; j < num4 + maximumRowSpan; j++)
			{
				num6 += (flag ? Grid.Headers[j].Height : Grid.Rows[j].Height);
				if (num6 + m_currentBounds.Y > m_currentBounds.Height)
				{
					if (j - num4 >= 1)
					{
						num = j - num4;
					}
					break;
				}
			}
			for (int k = 0; k < row.Cells.Count; k++)
			{
				if (row.Cells[k].RowSpan > 1)
				{
					rowSpanCellIndex = k;
					pendingRowSpanCell = row.Cells[k];
				}
			}
			if ((num2 > m_currentBounds.Height || num2 + m_currentBounds.Y > m_currentBounds.Height) && row.RowSpanExists && (Grid.LayoutFormat.Break == PdfLayoutBreakType.FitElement || !Grid.AllowRowBreakAcrossPages))
			{
				flag2 = false;
			}
			if ((num2 > m_currentBounds.Height || num2 + m_currentBounds.Y > m_currentBounds.Height) && flag2)
			{
				num2 = 0f;
				row.isPageBreakRowSpanApplied = true;
				foreach (PdfGridCell cell in row.Cells)
				{
					maximumRowSpan = cell.RowSpan;
					for (int l = num4; l < num4 + maximumRowSpan; l++)
					{
						num2 += (flag ? Grid.Headers[l].Height : Grid.Rows[l].Height);
						PdfLayoutFormat layoutFormat = Grid.LayoutFormat;
						float num7 = m_currentPageBounds.Height;
						if (layoutFormat.UsePaginateBounds)
						{
							_ = layoutFormat.PaginateBounds;
							if (layoutFormat.PaginateBounds.Height > 0f)
							{
								float num8 = layoutFormat.PaginateBounds.Bottom;
								if (!Grid.IsChildGrid && Grid.m_listOfNavigatePages.Count == 1)
								{
									num8 += Grid.m_gridLocation.Y;
								}
								if (num8 < num7)
								{
									num7 = num8;
									m_currentPageBounds.Height = num7;
									m_currentBounds.Height = num7;
								}
							}
						}
						if (!(m_currentBounds.Y + num2 > num7))
						{
							continue;
						}
						num2 -= (flag ? Grid.Headers[l].Height : Grid.Rows[l].Height);
						for (int m = 0; m < Grid.Rows[num4].Cells.Count; m++)
						{
							int num9 = l - num4;
							if (!flag && Grid.Rows[num4].Cells[m].RowSpan == maximumRowSpan)
							{
								switch (num9)
								{
								default:
								{
									Grid.Rows[num4].Cells[m].RowSpan = ((num9 == 0) ? 1 : num9);
									Grid.Rows[num4].maximumRowSpan = ((num9 == 0) ? 1 : num9);
									Grid.Rows[l].Cells[m].RowSpan = maximumRowSpan - num9;
									if (Grid.Rows[l].maximumRowSpan < maximumRowSpan - num9)
									{
										Grid.Rows[l].maximumRowSpan = maximumRowSpan - num9;
									}
									PdfGrid pdfGrid = Grid.Rows[num4].Cells[m].Value as PdfGrid;
									Grid.Rows[l].Cells[m].StringFormat = Grid.Rows[num4].Cells[m].StringFormat;
									Grid.Rows[l].Cells[m].Style = (PdfGridCellStyle)Grid.Rows[num4].Cells[m].Style.Clone();
									Grid.Rows[l].Cells[m].Style.BackgroundImage = null;
									Grid.Rows[l].Cells[m].ColumnSpan = Grid.Rows[num4].Cells[m].ColumnSpan;
									if (pdfGrid != null && m_currentBounds.Y + pdfGrid.Size.Height + Grid.Rows[l].Height + pdfGrid.Style.CellPadding.Top + pdfGrid.Style.CellPadding.Bottom >= m_currentBounds.Height)
									{
										Grid.Rows[l].Cells[m].Value = Grid.Rows[num4].Cells[m].Value;
									}
									else if (pdfGrid == null)
									{
										Grid.Rows[l].Cells[m].Value = Grid.Rows[num4].Cells[m].Value;
									}
									if (l > 0)
									{
										Grid.Rows[l - 1].RowSpanExists = true;
									}
									Grid.Rows[l].Cells[m].IsRowMergeContinue = false;
									Grid.Rows[l].Cells[m].IsRowMergeStart = true;
									continue;
								}
								case 0:
									break;
								case 1:
									continue;
								}
							}
							if (flag && Grid.Headers[num4].Cells[m].RowSpan == maximumRowSpan)
							{
								Grid.Headers[num4].Cells[m].RowSpan = ((num9 == 0) ? 1 : num9);
								Grid.Headers[l].Cells[m].RowSpan = maximumRowSpan - num9;
								Grid.Headers[l].Cells[m].StringFormat = Grid.Headers[num4].Cells[m].StringFormat;
								Grid.Headers[l].Cells[m].Style = Grid.Headers[num4].Cells[m].Style;
								Grid.Headers[l].Cells[m].ColumnSpan = Grid.Headers[num4].Cells[m].ColumnSpan;
								Grid.Headers[l].Cells[m].Value = Grid.Headers[num4].Cells[m].Value;
								Grid.Headers[l - 1].RowSpanExists = false;
								Grid.Headers[l].Cells[m].IsRowMergeContinue = false;
								Grid.Headers[l].Cells[m].IsRowMergeStart = true;
							}
						}
						break;
					}
					num2 = 0f;
				}
			}
		}
		float num10 = 0f;
		if (!drawFinalRow)
		{
			num10 = ((row.RowBreakHeight > 0f) ? row.RowBreakHeight : row.Height);
			if (currentHtmlLayoutResult != null && currentHtmlLayoutResult.Bounds.Height > num10)
			{
				if (currentHtmlLayoutResult.LastLineBounds.Height == currentHtmlLayoutResult.Bounds.Height)
				{
					num10 = currentHtmlLayoutResult.Bounds.Width - currentHtmlLayoutResult.LastLineBounds.Width;
				}
				else if (currentHtmlLayoutResult.Bounds.Width > currentHtmlLayoutResult.Bounds.Height)
				{
					num10 = currentHtmlLayoutResult.Bounds.Y + currentHtmlLayoutResult.Bounds.Height;
				}
			}
		}
		else if (endPageHeight != 0f)
		{
			num10 = endPageHeight;
		}
		userHeight = m_startLocation.Y;
		bool flag3 = false;
		if (Grid.AllowRowBreakAcrossPages && row.Cells.Count > 0 && row.Cells[0].Value is PdfGrid)
		{
			PdfGrid pdfGrid2 = row.Cells[0].Value as PdfGrid;
			float num11 = 0f;
			using (List<PdfGridRow>.Enumerator enumerator2 = pdfGrid2.Rows.GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					PdfGridRow current = enumerator2.Current;
					num11 += current.Height;
					num11 += pdfGrid2.Style.CellSpacing;
					if (row.Cells[0].Style != null && row.Cells[0].Style.Borders != null)
					{
						num11 += row.Cells[0].Style.Borders.Bottom.Width;
					}
				}
			}
			if (num10 > num11)
			{
				flag3 = true;
			}
		}
		if (Grid.IsChildGrid && Grid.ParentCell != null)
		{
			if (num10 + Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom + Grid.ParentCell.Row.Grid.Style.CellPadding.Top > m_currentPageBounds.Height)
			{
				if (Grid.AllowRowBreakAcrossPages)
				{
					result.IsFinish = true;
					if (Grid.IsChildGrid && row.RowBreakHeight > 0f)
					{
						if (Grid.ParentCell.Row.Grid.Style.CellPadding != null)
						{
							m_currentBounds.Y += Grid.ParentCell.Row.Grid.Style.CellPadding.Top;
						}
						m_currentBounds.X = m_startLocation.X;
					}
					result.Bounds = m_currentBounds;
					DrawRowWithBreak(ref result, row, num10);
				}
				else
				{
					if (Grid.ParentCell.Row.Grid.Style.CellPadding != null)
					{
						m_currentBounds.Y += Grid.ParentCell.Row.Grid.Style.CellPadding.Top;
						num10 = m_currentBounds.Height - m_currentBounds.Y - Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
					}
					result.IsFinish = false;
					DrawRow(ref result, row, num10);
				}
			}
			else if (m_currentBounds.Y + Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom + num10 > m_currentPageBounds.Height || (!Grid.IsChildGrid && m_currentBounds.Y + Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom + num10 > m_currentBounds.Height && !Grid.IsChildGrid) || m_currentBounds.Y + Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom + num2 > m_currentPageBounds.Height)
			{
				bool flag4 = false;
				bool flag5 = false;
				if (Grid.AllowRowBreakAcrossPages && !row.ColumnSpanExists && !Grid.RepeatHeader && !row.m_isRowHeightSet && !row.RowMergeComplete)
				{
					flag4 = IsFitToCell(m_currentPageBounds.Height - m_currentBounds.Y, Grid, row);
					if (!flag4)
					{
						foreach (PdfGridCell cell2 in row.Cells)
						{
							if (cell2.Value is PdfGrid)
							{
								flag5 = true;
								break;
							}
						}
					}
				}
				if ((m_repeatRowIndex > -1 && m_repeatRowIndex == row.RowIndex) || flag4 || flag5)
				{
					if (Grid.AllowRowBreakAcrossPages)
					{
						result.IsFinish = true;
						if (Grid.IsChildGrid && row.RowBreakHeight > 0f)
						{
							if (Grid.ParentCell.Row.Grid.Style.CellPadding != null)
							{
								m_currentBounds.Y += Grid.ParentCell.Row.Grid.Style.CellPadding.Top;
							}
							m_currentBounds.X = m_startLocation.X;
						}
						result.Bounds = m_currentBounds;
						DrawRowWithBreak(ref result, row, num10);
					}
					else
					{
						result.IsFinish = false;
						DrawRow(ref result, row, num10);
					}
				}
				else
				{
					result.IsFinish = false;
				}
			}
			else if (m_currentBounds.Y + Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom + num10 > m_currentBounds.Height && row.m_isRowHeightSet && Grid.ParentCell.Row.RowBreakHeight > 0f)
			{
				if (Grid.ParentCell.Row.RowBreakHeight > 0f && Grid.ParentCell.Row.RowBreakHeight < num10)
				{
					Grid.ParentCell.Row.RowBreakHeight = 0f;
				}
				result.IsFinish = false;
			}
			else
			{
				result.IsFinish = true;
				if (Grid.IsChildGrid && row.RowBreakHeight > 0f && Grid.ParentCell.Row.Grid.Style.CellPadding != null)
				{
					num10 += Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
				}
				DrawRow(ref result, row, num10);
			}
		}
		else
		{
			float num12 = ReCalculateHeight(row, num10);
			float num13 = 0f;
			float num14 = 0f;
			float num15 = 0f;
			if (m_currentGraphics.Page is PdfPage)
			{
				PdfDocument document = (m_currentGraphics.Page as PdfPage).Document;
				if (document.Template.Top != null)
				{
					num13 = document.Template.Top.Height;
				}
				if (document.Template.Bottom != null)
				{
					num14 = document.Template.Bottom.Height;
				}
				if (row.RowIndex == -1)
				{
					m_initialHeight = m_currentBounds.Y;
				}
				if ((num13 != 0f || num14 != 0f) && m_initialHeight > 0f)
				{
					num15 = m_currentBounds.Y + m_currentBounds.Height + (float)row.Cells.Count * num12 + num13 + num14 - m_initialHeight;
				}
			}
			if (num10 > m_currentPageBounds.Height)
			{
				if (Grid.AllowRowBreakAcrossPages)
				{
					result.IsFinish = true;
					DrawRowWithBreak(ref result, row, num10);
				}
				else
				{
					result.IsFinish = false;
					DrawRow(ref result, row, num10);
				}
			}
			else if (flag3 || m_currentBounds.Y + num10 > m_currentPageBounds.Height || m_currentBounds.Y + num12 > m_currentPageBounds.Height || m_currentBounds.Y + num10 - userHeight > m_currentBounds.Height || m_currentBounds.Y + num3 > m_currentPageBounds.Height || m_currentBounds.Y + num2 > m_currentPageBounds.Height || !flag2 || (num15 > m_currentPageBounds.Height && (m_currentBounds.Height < num13 || m_currentBounds.Height < num14)))
			{
				bool flag6 = false;
				if (Grid.AllowRowBreakAcrossPages && !Grid.RepeatHeader && !row.m_isRowHeightSet && !row.RowMergeComplete)
				{
					flag6 = ((Grid.LayoutFormat == null || !(Grid.LayoutFormat.PaginateBounds.Height > 0f) || !(m_currentBounds.Y + num10 - userHeight > m_currentBounds.Height)) ? IsFitToCell(m_currentPageBounds.Height - m_currentBounds.Y, Grid, row) : IsFitToCell(m_currentBounds.Height + m_startLocation.Y - m_currentBounds.Y, Grid, row));
					if (flag6)
					{
						isPaginate = true;
					}
				}
				if ((m_repeatRowIndex > -1 && m_repeatRowIndex == row.RowIndex) || flag6 || flag3)
				{
					if (m_currentPage == null && m_currentBounds.Y + num10 > m_currentPageBounds.Height && m_currentBounds.Y + num12 > m_currentPageBounds.Height && m_currentBounds.Y + num10 - userHeight > m_currentBounds.Height && m_currentBounds.Y + num3 > m_currentPageBounds.Height)
					{
						result.IsFinish = true;
					}
					else if (Grid.AllowRowBreakAcrossPages)
					{
						result.IsFinish = true;
						DrawRowWithBreak(ref result, row, num10);
					}
					else
					{
						result.IsFinish = false;
						DrawRow(ref result, row, num10);
					}
				}
				else if (paginateWithRowBreak)
				{
					currentRowHeight = num10 - m_currentPageBounds.Height;
					result.IsFinish = false;
					paginateWithRowBreak = false;
					DrawRowWithBreak(ref result, row, m_currentPageBounds.Height);
				}
				else if (drawFinalRow)
				{
					result.IsFinish = true;
					DrawRow(ref result, row, num10);
				}
				else if (m_currentPage.Document.Template != null && m_currentPage.Document.Template.Top != null && m_currentPage.Document.Template.Bottom != null)
				{
					if (!IsRowHasBackgorundImage(row))
					{
						float num16 = m_currentPage.Document.PageSettings.Margins.Top + m_currentPage.Document.PageSettings.Margins.Bottom + m_currentPage.Document.Template.Bottom.Height;
						if (m_currentPage.Graphics.ClientSize.Height - num16 > m_currentBounds.Y + num10)
						{
							result.IsFinish = true;
							DrawRow(ref result, row, num10);
						}
					}
				}
				else if (num >= 1)
				{
					if (num10 != (float)num * row.Height)
					{
						num10 = (float)num * row.Height;
					}
					m_currentDrawingRowHeignt = num10;
					DrawRow(ref result, row, num10);
					for (int n = 0; n < row.Cells.Count; n++)
					{
						PdfGridCell pdfGridCell = row.Cells[n];
						if (pdfGridCell.RowSpan > 1)
						{
							pdfGridCell.IsRowMergeContinue = false;
							int rowSpan = pdfGridCell.RowSpan - num;
							pdfGridCell.RowSpan = rowSpan;
							pdfGridCell.Style = pendingRowSpanCell.Style;
						}
					}
					row.maximumRowSpan -= num;
					isCompleteRowspanBorder = true;
					paginateRowSpan = true;
					result.IsFinish = true;
				}
				else
				{
					result.IsFinish = false;
				}
			}
			else
			{
				result.IsFinish = true;
				if (previousRowPendingRowSpan >= 1)
				{
					row.maximumRowSpan = previousRowPendingRowSpan;
					for (int num17 = 0; num17 < row.Cells.Count; num17++)
					{
						if (rowSpanCellIndex == num17)
						{
							PdfGridCell pdfGridCell2 = row.Cells[num17];
							pdfGridCell2.Style = pendingRowSpanCell.Style;
							pdfGridCell2.RowSpan = row.maximumRowSpan;
							break;
						}
					}
					DrawRow(ref result, row, num10);
					previousRowPendingRowSpan = 0;
				}
				else
				{
					DrawRow(ref result, row, num10);
				}
			}
		}
		return result;
	}

	private bool IsRowHasBackgorundImage(PdfGridRow row)
	{
		foreach (PdfGridCell cell in row.Cells)
		{
			if (cell.Style.BackgroundImage != null)
			{
				return true;
			}
		}
		return false;
	}

	private void DrawRowWithBreak(ref RowLayoutResult result, PdfGridRow row, float height)
	{
		PointF location = m_currentBounds.Location;
		if (row.Grid.IsChildGrid && row.Grid.AllowRowBreakAcrossPages && m_startLocation.X != m_currentBounds.X)
		{
			location.X = m_startLocation.X;
		}
		result.Bounds = new RectangleF(location, SizeF.Empty);
		if (m_currentGraphics.Page is PdfPage)
		{
			PdfDocument document = (m_currentGraphics.Page as PdfPage).Document;
			float num = 0f;
			float num2 = 0f;
			if (document.Template.Top != null)
			{
				num = document.Template.Top.Height;
			}
			if (document.Template.Bottom != null)
			{
				num2 = document.Template.Bottom.Height;
			}
			if ((num != 0f || num2 != 0f) && isPaginate && (m_currentBounds.Height < num || m_currentBounds.Height < num2))
			{
				m_currentBounds.Y += m_currentBounds.Height + num + num2;
			}
		}
		if (m_currentBounds.Height < m_currentPageBounds.Height)
		{
			m_newheight = ((row.RowBreakHeight > 0f) ? m_currentBounds.Height : 0f);
		}
		else
		{
			m_newheight = ((row.RowBreakHeight > 0f) ? m_currentPageBounds.Height : 0f);
		}
		float num3 = m_currentBounds.Y + height - m_currentPageBounds.Height;
		if (row.Grid.Style.CellPadding.Top + m_currentBounds.Y + row.Grid.Style.CellPadding.Bottom < m_currentPageBounds.Height)
		{
			if (row.Grid.Headers != null && row.Grid.Headers.Count > 0 && (int)num3 == (int)height / row.Cells.Count && m_currentBounds.Y + height > m_currentPageBounds.Height)
			{
				row.RowBreakHeight = height;
				result.IsFinish = false;
				return;
			}
			row.RowBreakHeight = m_currentBounds.Y + height - m_currentPageBounds.Height;
			foreach (PdfGridCell cell in row.Cells)
			{
				float num4 = cell.MeasureHeight();
				if (num4 == height && cell.Value is PdfGrid)
				{
					row.RowBreakHeight = 0f;
				}
				else if (num4 == height && !(cell.Value is PdfGrid))
				{
					if (m_currentBounds.Height < m_currentPageBounds.Height && !row.Grid.IsChildGrid)
					{
						row.RowBreakHeight = m_currentBounds.Y + height - m_currentBounds.Height;
					}
					else
					{
						row.RowBreakHeight = m_currentBounds.Y + height - m_currentPageBounds.Height;
					}
				}
			}
			for (int i = m_cellStartIndex; i <= m_cellEndIndex; i++)
			{
				float num5 = Grid.Columns[i].Width;
				bool flag = row.Cells[i].ColumnSpan + i > m_cellEndIndex + 1 && row.Cells[i].ColumnSpan > 1;
				if (!flag)
				{
					for (int j = 1; j < row.Cells[i].ColumnSpan; j++)
					{
						row.Cells[i + j].IsCellMergeContinue = true;
						num5 += Grid.Columns[i + j].Width;
					}
				}
				if (height < m_currentPageBounds.Height)
				{
					if (m_currentBounds.Y + height < m_currentPageBounds.Height)
					{
						if (m_currentBounds.Y + height - m_startLocation.Y > m_currentBounds.Height)
						{
							row.RowBreakHeight = m_currentBounds.Y + height - m_startLocation.Y - m_currentBounds.Height;
							m_newheight = height - row.RowBreakHeight;
						}
						else
						{
							m_newheight = height;
						}
					}
					else if (!cellInnerGrid && pdfLayoutParams.Bounds.X != 0f && pdfLayoutParams.Bounds.Y != 0f)
					{
						if (Grid != null && Grid.LayoutFormat != null && Grid.LayoutFormat.PaginateBounds != RectangleF.Empty)
						{
							row.RowBreakHeight = m_currentBounds.Y + height - m_startLocation.Y - m_currentBounds.Height;
							m_newheight = height - row.RowBreakHeight;
						}
						else
						{
							m_newheight = m_currentPageBounds.Height - (m_currentBounds.Top + (m_currentPageBounds.Height - m_currentBounds.Bottom));
						}
					}
					else
					{
						m_newheight = m_currentPageBounds.Height - m_currentBounds.Y;
					}
				}
				else if (m_currentBounds.Y > 0f && m_currentBounds.Height < m_currentPageBounds.Height && row.RowBreakHeight < m_currentPageBounds.Height)
				{
					m_newheight = m_currentPageBounds.Height - m_currentBounds.Y;
				}
				if (!cellInnerGrid && m_newheight > m_currentBounds.Height)
				{
					m_newheight = m_currentPageBounds.Height - (m_currentBounds.Top + (m_currentPageBounds.Height - m_currentBounds.Bottom));
				}
				SizeF sizeF = default(SizeF);
				sizeF = ((!(m_currentBounds.Height < m_currentPageBounds.Height)) ? new SizeF(num5, ((double)m_newheight > 0.0) ? m_newheight : m_currentPageBounds.Height) : new SizeF(num5, ((double)m_newheight > 0.0) ? m_newheight : m_currentBounds.Height));
				if (sizeF.Width == 0f)
				{
					sizeF = new SizeF(row.Cells[i].Width, sizeF.Height);
				}
				if (!CheckIfDefaultFormat(Grid.Columns[i].Format) && CheckIfDefaultFormat(row.Cells[i].StringFormat))
				{
					row.Cells[i].StringFormat = Grid.Columns[i].Format;
				}
				PdfGridCellStyle style = row.Cells[i].Style;
				PdfGridBeginCellLayoutEventArgs pdfGridBeginCellLayoutEventArgs = null;
				if (!row.Cells[i].IsCellMergeContinue)
				{
					pdfGridBeginCellLayoutEventArgs = RaiseBeforeCellLayout(m_currentGraphics, m_currentRowIndex, i, new RectangleF(location, sizeF), (row.Cells[i].Value is string) ? row.Cells[i].Value.ToString() : string.Empty, ref style, row.IsHeaderRow);
				}
				row.Cells[i].Style = style;
				bool flag2 = false;
				if (pdfGridBeginCellLayoutEventArgs != null)
				{
					flag2 = pdfGridBeginCellLayoutEventArgs.Skip;
				}
				PdfStringLayoutResult pdfStringLayoutResult = null;
				if (!flag2)
				{
					if (row.PdfTag != null)
					{
						if (row.Cells[i].PdfTag == null)
						{
							row.Cells[i].PdfTag = (m_isHeader ? new PdfStructureElement(PdfTagType.TableHeader) : new PdfStructureElement(PdfTagType.TableDataCell));
						}
						(row.Cells[i].PdfTag as PdfStructureElement).Parent = row.PdfTag as PdfStructureElement;
					}
					PointF location2 = location;
					SizeF size = sizeF;
					float num6 = 0f;
					float num7 = 0f;
					float num8 = 0f;
					PdfGridCell pdfGridCell2 = row.Cells[i];
					if (pdfGridCell2.Style.Borders != null)
					{
						if (pdfGridCell2.Style.Borders.Left != null)
						{
							num6 = pdfGridCell2.Style.Borders.Left.Width;
						}
						if (pdfGridCell2.Style.Borders.Right != null)
						{
							num7 = pdfGridCell2.Style.Borders.Right.Width;
						}
						if (pdfGridCell2.Style.Borders.Top != null)
						{
							num8 = pdfGridCell2.Style.Borders.Top.Width;
						}
						if (pdfGridCell2.Style.Borders.Bottom != null)
						{
							_ = pdfGridCell2.Style.Borders.Bottom.Width;
						}
						if (location.X == 0f)
						{
							location2.X += num6 / 2f;
							size.Width -= num6 / 2f;
							row.Cells[i].cellBorderCuttOffX = true;
						}
						if (location.Y == 0f)
						{
							location2.Y += num8 / 2f;
							size.Height -= num8 / 2f;
							row.Cells[i].cellBorderCuttOffY = true;
						}
						if (pdfGridCell2.Value is PdfGrid && location.X == 0f)
						{
							row.Cells[i].cellBorderCuttOffX = true;
						}
						if (pdfGridCell2.Value is PdfGrid && location.Y == 0f)
						{
							row.Cells[i].cellBorderCuttOffY = true;
						}
						if (Grid.IsChildGrid)
						{
							if (i == 0)
							{
								location2.X += num6 / 2f;
								size.Width -= num6 / 2f;
							}
							if (location.Y == m_cellEventBounds.Y)
							{
								location2.Y += num8 / 2f;
								size.Height -= num8 / 2f;
							}
							if ((i != 0 && i == row.Cells.Count - 1) || (i == 0 && row.Cells.Count - 1 == 0))
							{
								if (location.X + size.Width + Grid.ParentCell.Style.Borders.Left.Width > m_currentPageBounds.Width)
								{
									size.Width -= Grid.ParentCell.Style.Borders.Left.Width / 2f + num7 / 2f;
								}
								else
								{
									size.Width -= num7 / 2f;
								}
							}
						}
					}
					if (row.Cells[i].pdfGridLayouter == null)
					{
						row.Cells[i].pdfGridLayouter = this;
					}
					pdfStringLayoutResult = row.Cells[i].Draw(m_currentGraphics, new RectangleF(location2, size), flag);
					if (pdfStringLayoutResult != null && !string.IsNullOrEmpty(pdfStringLayoutResult.Remainder))
					{
						remainingStringValue = pdfStringLayoutResult.Remainder;
					}
				}
				if ((double)row.RowBreakHeight > 0.0 || (row.RowBreakHeight < 0f && pdfStringLayoutResult != null && pdfStringLayoutResult.Remainder != null))
				{
					if (row.RowBreakHeight > 0f && row.Grid.AllowRowBreakAcrossPages)
					{
						row.Cells[i].FinishedDrawingCell = false;
					}
					if (pdfStringLayoutResult != null)
					{
						row.Cells[i].FinishedDrawingCell = false;
						row.Cells[i].RemainingString = ((pdfStringLayoutResult.Remainder == null) ? string.Empty : pdfStringLayoutResult.Remainder);
						if (row.Grid.IsChildGrid && row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom > 1f)
						{
							row.RowBreakHeight = height - pdfStringLayoutResult.ActualSize.Height;
						}
					}
					else if (row.Cells[i].Value is PdfImage)
					{
						row.Cells[i].FinishedDrawingCell = false;
					}
				}
				result.IsFinish = ((!result.IsFinish) ? result.IsFinish : row.Cells[i].FinishedDrawingCell);
				if (!flag && !row.Cells[i].IsCellMergeContinue)
				{
					RaiseAfterCellLayout(m_currentGraphics, m_currentRowIndex, i, new RectangleF(location, sizeF), (row.Cells[i].Value is string) ? row.Cells[i].Value.ToString() : string.Empty, row.Cells[i].Style, row.IsHeaderRow);
				}
				if (row.Cells[i].Value is PdfGrid)
				{
					PdfGrid pdfGrid = row.Cells[i].Value as PdfGrid;
					m_rowBreakPageHeightCellIndex = i;
					row.Cells[i].m_pageCount = pdfGrid.m_listOfNavigatePages.Count;
					foreach (int listOfNavigatePage in pdfGrid.m_listOfNavigatePages)
					{
						if (!Grid.m_listOfNavigatePages.Contains(listOfNavigatePage))
						{
							Grid.m_listOfNavigatePages.Add(listOfNavigatePage);
						}
					}
					if (Grid.Columns[i].Width >= m_currentGraphics.ClientSize.Width)
					{
						location.X = pdfGrid.m_rowLayoutBoundswidth;
						location.X += pdfGrid.Style.CellSpacing;
					}
					else
					{
						location.X += Grid.Columns[i].Width;
					}
				}
				else
				{
					location.X += Grid.Columns[i].Width;
				}
			}
			m_currentBounds.Y += (((double)m_newheight > 0.0) ? m_newheight : height);
			result.Bounds = new RectangleF(result.Bounds.Location, new SizeF(location.X, location.Y));
		}
		else
		{
			row.RowBreakHeight = height;
			result.IsFinish = false;
		}
	}

	private void DrawRow(ref RowLayoutResult result, PdfGridRow row, float height)
	{
		bool flag = false;
		PointF location = m_currentBounds.Location;
		result.Bounds = new RectangleF(location, SizeF.Empty);
		height = ReCalculateHeight(row, height);
		for (int i = m_cellStartIndex; i <= m_cellEndIndex; i++)
		{
			float num = Grid.Columns[i].Width;
			bool flag2 = i > m_cellEndIndex + 1 && row.Cells[i].ColumnSpan > 1;
			if (!flag2)
			{
				for (int j = 1; j < row.Cells[i].ColumnSpan; j++)
				{
					row.Cells[i + j].IsCellMergeContinue = true;
					num += Grid.Columns[i + j].Width;
				}
			}
			if (height > m_currentPageBounds.Height && !Grid.AllowRowBreakAcrossPages)
			{
				height = m_currentPageBounds.Height;
			}
			SizeF sizeF = new SizeF(num, height);
			if (sizeF.Width > m_currentGraphics.ClientSize.Width)
			{
				sizeF.Width = m_currentGraphics.ClientSize.Width;
			}
			if (Grid.IsChildGrid && Grid.Style.AllowHorizontalOverflow && sizeF.Width >= m_currentGraphics.ClientSize.Width)
			{
				sizeF.Width -= 2f * m_currentBounds.X;
			}
			if (!CheckIfDefaultFormat(Grid.Columns[i].Format) && CheckIfDefaultFormat(row.Cells[i].StringFormat))
			{
				row.Cells[i].StringFormat = Grid.Columns[i].Format;
			}
			if (previousRowPendingRowSpan > 1 && row.Cells[i].RowSpan > 1)
			{
				row.Cells[i].m_skipCellValue = true;
				sizeF.Height = (float)row.Cells[i].RowSpan * height;
			}
			PdfGridCellStyle style = row.Cells[i].Style;
			PdfGridBeginCellLayoutEventArgs pdfGridBeginCellLayoutEventArgs = null;
			if (!row.Cells[i].IsCellMergeContinue)
			{
				pdfGridBeginCellLayoutEventArgs = RaiseBeforeCellLayout(m_currentGraphics, m_currentRowIndex, i, new RectangleF(location, sizeF), (row.Cells[i].Value is string) ? row.Cells[i].Value.ToString() : string.Empty, ref style, row.IsHeaderRow);
			}
			row.Cells[i].Style = style;
			if (pdfGridBeginCellLayoutEventArgs != null)
			{
				flag = pdfGridBeginCellLayoutEventArgs.Skip;
			}
			if (!flag)
			{
				if (row.Cells[i].Value is PdfGrid)
				{
					(row.Cells[i].Value as PdfGrid).parentCellIndex = i;
				}
				if (row.PdfTag != null)
				{
					if (row.Cells[i].PdfTag == null)
					{
						row.Cells[i].PdfTag = (m_isHeader ? new PdfStructureElement(PdfTagType.TableHeader) : new PdfStructureElement(PdfTagType.TableDataCell));
					}
					(row.Cells[i].PdfTag as PdfStructureElement).Parent = row.PdfTag as PdfStructureElement;
					m_currentGraphics.customTag = true;
				}
				PointF location2 = location;
				SizeF size = sizeF;
				float num2 = 0f;
				float num3 = 0f;
				float num4 = 0f;
				PdfGridCell pdfGridCell = row.Cells[i];
				if (pdfGridCell.Style.Borders != null)
				{
					if (pdfGridCell.Style.Borders.Left != null)
					{
						num2 = pdfGridCell.Style.Borders.Left.Width;
					}
					if (pdfGridCell.Style.Borders.Right != null)
					{
						num3 = pdfGridCell.Style.Borders.Right.Width;
					}
					if (pdfGridCell.Style.Borders.Top != null)
					{
						num4 = pdfGridCell.Style.Borders.Top.Width;
					}
					if (pdfGridCell.Style.Borders.Bottom != null)
					{
						_ = pdfGridCell.Style.Borders.Bottom.Width;
					}
					if (location.X == 0f)
					{
						location2.X += num2 / 2f;
						size.Width -= num2 / 2f;
						row.Cells[i].cellBorderCuttOffX = true;
					}
					if (location.Y == 0f)
					{
						location2.Y += num4 / 2f;
						size.Height -= num4 / 2f;
						row.Cells[i].cellBorderCuttOffY = true;
					}
					if (pdfGridCell.Value is PdfGrid && location.X == 0f)
					{
						row.Cells[i].cellBorderCuttOffX = true;
					}
					if (pdfGridCell.Value is PdfGrid && location.Y == 0f)
					{
						row.Cells[i].cellBorderCuttOffY = true;
					}
					if (Grid.IsChildGrid)
					{
						if (i == 0)
						{
							location2.X += num2 / 2f;
							size.Width -= num2 / 2f;
						}
						if (location.Y == m_cellEventBounds.Y)
						{
							location2.Y += num4 / 2f;
							size.Height -= num4 / 2f;
						}
						if ((i != 0 && i == row.Cells.Count - 1) || (i == 0 && row.Cells.Count - 1 == 0))
						{
							if (location.X + size.Width + Grid.ParentCell.Style.Borders.Left.Width > m_currentPageBounds.Width)
							{
								size.Width -= Grid.ParentCell.Style.Borders.Left.Width / 2f + num3 / 2f;
							}
							else
							{
								size.Width -= num3 / 2f;
							}
						}
					}
				}
				if (row.maximumRowSpan == previousRowPendingRowSpan)
				{
					row.Cells[i].IsRowMergeContinue = false;
				}
				if (row.Cells[i].pdfGridLayouter == null)
				{
					row.Cells[i].pdfGridLayouter = this;
				}
				PdfStringLayoutResult pdfStringLayoutResult = row.Cells[i].Draw(m_currentGraphics, new RectangleF(location2, size), flag2);
				m_currentGraphics.customTag = false;
				if (pdfStringLayoutResult == null && row.m_drawCellBroders && row.RowSpanExists && row.Grid.LayoutFormat != null && row.Grid.LayoutFormat.Layout == PdfLayoutType.Paginate)
				{
					if (row.RowIndex - 1 >= 0)
					{
						row.Cells[i].Style = row.Grid.Rows[row.RowIndex - 1].Cells[i].Style;
					}
					row.Cells[i].DrawCellBorders(ref m_currentGraphics, new RectangleF(location, new SizeF(sizeF.Width, row.m_borderReminingHeight)));
				}
				if (row.Grid.Style.AllowHorizontalOverflow && (row.Cells[i].ColumnSpan > m_cellEndIndex || i + row.Cells[i].ColumnSpan > m_cellEndIndex + 1) && m_cellEndIndex < row.Cells.Count - 1)
				{
					row.RowOverflowIndex = m_cellEndIndex;
				}
				if (row.Grid.Style.AllowHorizontalOverflow && row.RowOverflowIndex > 0 && (row.Cells[i].ColumnSpan > m_cellEndIndex || i + row.Cells[i].ColumnSpan > m_cellEndIndex + 1) && row.Cells[i].ColumnSpan - m_cellEndIndex + i - 1 > 0)
				{
					row.Cells[row.RowOverflowIndex + 1].Value = pdfStringLayoutResult?.m_remainder;
					row.Cells[row.RowOverflowIndex + 1].StringFormat = row.Cells[i].StringFormat;
					row.Cells[row.RowOverflowIndex + 1].Style = row.Cells[i].Style;
					row.Cells[row.RowOverflowIndex + 1].ColumnSpan = row.Cells[i].ColumnSpan - m_cellEndIndex + i - 1;
				}
			}
			if (!flag2 && !row.Cells[i].IsCellMergeContinue)
			{
				RaiseAfterCellLayout(m_currentGraphics, m_currentRowIndex, i, new RectangleF(location, sizeF), (row.Cells[i].Value is string) ? row.Cells[i].Value.ToString() : string.Empty, row.Cells[i].Style, row.IsHeaderRow);
			}
			if (row.Cells[i].Value is PdfGrid)
			{
				PdfGrid pdfGrid = row.Cells[i].Value as PdfGrid;
				row.Cells[i].m_pageCount = pdfGrid.m_listOfNavigatePages.Count;
				m_rowBreakPageHeightCellIndex = i;
				foreach (int listOfNavigatePage in pdfGrid.m_listOfNavigatePages)
				{
					if (!Grid.m_listOfNavigatePages.Contains(listOfNavigatePage))
					{
						Grid.m_listOfNavigatePages.Add(listOfNavigatePage);
					}
				}
				if (Grid.Columns[i].Width >= m_currentGraphics.ClientSize.Width)
				{
					location.X = pdfGrid.m_rowLayoutBoundswidth;
					location.X += pdfGrid.Style.CellSpacing;
				}
				else
				{
					location.X += Grid.Columns[i].Width;
				}
			}
			else
			{
				location.X += Grid.Columns[i].Width;
			}
		}
		if (!row.RowMergeComplete || row.m_isRowHeightSet)
		{
			m_currentBounds.Y += height;
		}
		result.Bounds = new RectangleF(result.Bounds.Location, new SizeF(location.X, location.Y));
		m_currentGraphics.customTag = false;
	}

	private float ReCalculateHeight(PdfGridRow row, float height)
	{
		float num = 0f;
		for (int i = m_cellStartIndex; i <= m_cellEndIndex; i++)
		{
			if (!string.IsNullOrEmpty(row.Cells[i].RemainingString))
			{
				num = Math.Max(num, row.Cells[i].MeasureHeight());
			}
			else
			{
				if (!(row.Cells[i].Value is PdfGrid) || row.Grid.AllowRowBreakAcrossPages)
				{
					continue;
				}
				float num2 = 0f;
				float num3 = 0f;
				PdfGrid pdfGrid = row.Cells[i].Value as PdfGrid;
				for (int j = 0; j < pdfGrid.Rows.Count; j++)
				{
					PdfGridRow pdfGridRow = pdfGrid.Rows[j];
					float num4 = 0f;
					float num5 = 0f;
					float num6 = ((pdfGridRow.Cells[0].RowSpan <= 1) ? pdfGridRow.Cells[0].Height : 0f);
					foreach (PdfGridCell cell in pdfGridRow.Cells)
					{
						if (cell.m_rowSpanRemainingHeight > num4)
						{
							num4 = cell.m_rowSpanRemainingHeight;
						}
						if (cell.IsRowMergeContinue)
						{
							continue;
						}
						if (cell.RowSpan > 1)
						{
							if (num5 < cell.Height)
							{
								num5 = cell.Height;
							}
							continue;
						}
						num6 = Math.Max(num6, cell.Height);
						if (!cell.IsRowMergeContinue && cell.Value == null && j != 0 && pdfGrid.Rows[j - 1].RowSpanExists)
						{
							num3 = pdfGrid.Rows[j - 1].Height + num6;
							num6 = ((!(num3 < num2)) ? (num3 - num2) : 0f);
						}
					}
					if (num6 == 0f)
					{
						num6 = num5;
					}
					else if (num4 > 0f)
					{
						num6 += num4;
					}
					if (pdfGrid.IsChildGrid && num5 != 0f && num6 != 0f && num6 < num5)
					{
						num6 = num5;
					}
					num2 += num6;
				}
				num = num2;
			}
		}
		return Math.Max(height, num);
	}

	private bool RaiseBeforePageLayout(PdfPage currentPage, ref RectangleF currentBounds, ref int currentRow)
	{
		bool result = false;
		if (base.Element.RaiseBeginPageLayout)
		{
			PdfGridBeginPageLayoutEventArgs pdfGridBeginPageLayoutEventArgs = new PdfGridBeginPageLayoutEventArgs(currentBounds, currentPage, currentRow);
			base.Element.OnBeginPageLayout(pdfGridBeginPageLayoutEventArgs);
			if (currentBounds != pdfGridBeginPageLayoutEventArgs.Bounds)
			{
				isChanged = true;
				m_currentLocation = pdfGridBeginPageLayoutEventArgs.Bounds.Location;
				Grid.MeasureColumnsWidth(new RectangleF(pdfGridBeginPageLayoutEventArgs.Bounds.Location, new SizeF(pdfGridBeginPageLayoutEventArgs.Bounds.Width + pdfGridBeginPageLayoutEventArgs.Bounds.X, pdfGridBeginPageLayoutEventArgs.Bounds.Height)));
			}
			result = pdfGridBeginPageLayoutEventArgs.Cancel;
			currentBounds = pdfGridBeginPageLayoutEventArgs.Bounds;
			currentRow = pdfGridBeginPageLayoutEventArgs.StartRowIndex;
		}
		return result;
	}

	private PdfGridEndPageLayoutEventArgs RaisePageLayouted(PdfLayoutResult result)
	{
		PdfGridEndPageLayoutEventArgs pdfGridEndPageLayoutEventArgs = new PdfGridEndPageLayoutEventArgs(result);
		if (base.Element.RaiseEndPageLayout)
		{
			base.Element.OnEndPageLayout(pdfGridEndPageLayoutEventArgs);
		}
		return pdfGridEndPageLayoutEventArgs;
	}

	private PdfGridBeginCellLayoutEventArgs RaiseBeforeCellLayout(PdfGraphics graphics, int rowIndex, int cellIndex, RectangleF bounds, string value, ref PdfGridCellStyle style, bool isHeaderRow)
	{
		PdfGridBeginCellLayoutEventArgs pdfGridBeginCellLayoutEventArgs = null;
		if (Grid.RaiseBeginCellLayout)
		{
			pdfGridBeginCellLayoutEventArgs = new PdfGridBeginCellLayoutEventArgs(graphics, rowIndex, cellIndex, bounds, value, style, isHeaderRow);
			Grid.OnBeginCellLayout(pdfGridBeginCellLayoutEventArgs);
			style = pdfGridBeginCellLayoutEventArgs.Style;
		}
		return pdfGridBeginCellLayoutEventArgs;
	}

	private void RaiseAfterCellLayout(PdfGraphics graphics, int rowIndex, int cellIndex, RectangleF bounds, string value, PdfGridCellStyle cellstyle, bool isHeaderRow)
	{
		PdfGridEndCellLayoutEventArgs pdfGridEndCellLayoutEventArgs = null;
		if (Grid.RaiseEndCellLayout)
		{
			pdfGridEndCellLayoutEventArgs = new PdfGridEndCellLayoutEventArgs(graphics, rowIndex, cellIndex, bounds, value, cellstyle, isHeaderRow);
			Grid.OnEndCellLayout(pdfGridEndCellLayoutEventArgs);
		}
	}

	private bool CheckIfDefaultFormat(PdfStringFormat format)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		if (format.Alignment == pdfStringFormat.Alignment && format.CharacterSpacing == pdfStringFormat.CharacterSpacing && format.ClipPath == pdfStringFormat.ClipPath && format.FirstLineIndent == pdfStringFormat.FirstLineIndent && format.HorizontalScalingFactor == pdfStringFormat.HorizontalScalingFactor && format.LineAlignment == pdfStringFormat.LineAlignment && format.LineLimit == pdfStringFormat.LineLimit && format.LineSpacing == pdfStringFormat.LineSpacing && format.MeasureTrailingSpaces == pdfStringFormat.MeasureTrailingSpaces && format.NoClip == pdfStringFormat.NoClip && format.m_paragraphIndent == pdfStringFormat.m_paragraphIndent && format.RightToLeft == pdfStringFormat.RightToLeft && format.SubSuperScript == pdfStringFormat.SubSuperScript && format.WordSpacing == pdfStringFormat.WordSpacing)
		{
			return format.WordWrap == pdfStringFormat.WordWrap;
		}
		return false;
	}

	private void DetermineColumnDrawRanges()
	{
		int num = 0;
		int num2 = 0;
		float num3 = 0f;
		float num4 = m_currentGraphics.ClientSize.Width - m_currentBounds.X;
		for (int i = 0; i < Grid.Columns.Count; i++)
		{
			num3 += Grid.Columns[i].Width;
			if (!(num3 >= num4))
			{
				continue;
			}
			float num5 = 0f;
			for (int j = num; j <= i; j++)
			{
				num5 += Grid.Columns[j].Width;
				if (num5 > num4)
				{
					break;
				}
				num2 = j;
			}
			m_columnRanges.Add(new int[2] { num, num2 });
			num = num2 + 1;
			num2 = num;
			num3 = ((num2 <= i) ? Grid.Columns[i].Width : 0f);
		}
		if (num != Grid.Columns.Count)
		{
			m_columnRanges.Add(new int[2]
			{
				num,
				Grid.Columns.Count - 1
			});
		}
	}

	private void ReArrangePages(PdfPage page)
	{
		List<PdfPage> list = new List<PdfPage>();
		int count = page.Document.Pages.count;
		int num = 0;
		int num2 = m_columnRanges.Count;
		if (count <= m_columnRanges.Count)
		{
			for (int i = 0; i < m_columnRanges.Count; i++)
			{
				page.Document.Pages.Add();
				if (page.Document.Pages.count > m_columnRanges.Count + 1)
				{
					break;
				}
			}
		}
		count = page.Document.Pages.count;
		for (int j = 0; j < count; j++)
		{
			if (num < count && list.Count != count)
			{
				PdfPage item = page.Document.Pages[num];
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			if (num2 < count && list.Count != count)
			{
				PdfPage item2 = page.Document.Pages[num2];
				if (!list.Contains(item2))
				{
					list.Add(item2);
				}
			}
			if (list.Count == count)
			{
				break;
			}
			num++;
			num2++;
		}
		PdfDocument document = page.Document;
		for (int k = 0; k < list.Count; k++)
		{
			PdfPage pdfPage = list[k];
			pdfPage.Section = null;
			document.Pages.Remove(pdfPage);
		}
		for (int l = 0; l < list.Count; l++)
		{
			document.Pages.Add(list[l]);
		}
	}

	public PdfPage GetNextPage(PdfLayoutFormat format)
	{
		PdfSection section = m_currentPage.Section;
		PdfPage pdfPage = null;
		int num = section.IndexOf(m_currentPage);
		if (m_currentPage.Document.Pages.count > 1 && m_hType == PdfHorizontalOverflowType.NextPage && flag && m_columnRanges.Count > 1)
		{
			Grid.m_isRearranged = true;
			ReArrangePages(m_currentPage);
		}
		flag = false;
		pdfPage = ((num != section.Count - 1) ? section[num + 1] : section.Add());
		m_currentGraphics = pdfPage.Graphics;
		int item = (m_currentGraphics.Page as PdfPage).Section.IndexOf(m_currentGraphics.Page as PdfPage);
		if (!Grid.m_listOfNavigatePages.Contains(item))
		{
			Grid.m_listOfNavigatePages.Add(item);
		}
		m_currentBounds = new RectangleF(PointF.Empty, pdfPage.GetClientSize());
		if (format.PaginateBounds != RectangleF.Empty)
		{
			m_currentBounds.X = format.PaginateBounds.X;
			m_currentBounds.Y = format.PaginateBounds.Y;
			m_currentBounds.Height = format.PaginateBounds.Size.Height;
		}
		m_GridPaginated = true;
		return pdfPage;
	}

	private PdfGridLayoutFormat GetFormat(PdfLayoutFormat format)
	{
		PdfGridLayoutFormat pdfGridLayoutFormat = format as PdfGridLayoutFormat;
		if (format != null && pdfGridLayoutFormat == null)
		{
			pdfGridLayoutFormat = new PdfGridLayoutFormat(format);
		}
		return pdfGridLayoutFormat;
	}

	private PdfGridLayoutResult GetLayoutResult()
	{
		if (Grid.IsChildGrid && Grid.AllowRowBreakAcrossPages)
		{
			foreach (PdfGridRow row in Grid.Rows)
			{
				if (row.RowBreakHeight > 0f)
				{
					m_startLocation.Y = m_currentPage.Origin.Y;
				}
			}
		}
		return new PdfGridLayoutResult(bounds: isChanged ? new RectangleF(m_currentLocation, new SizeF(m_currentBounds.Width, m_currentBounds.Y - m_currentLocation.Y)) : ((Grid.IsChildGrid || !(m_currentBounds.Y > m_currentBounds.Height)) ? new RectangleF(m_startLocation, new SizeF(m_currentBounds.Width, m_currentBounds.Y - m_startLocation.Y)) : new RectangleF(m_startLocation, new SizeF(m_currentBounds.Width, m_currentBounds.Height))), page: m_currentPage);
	}

	private bool IsFitToCell(float currentHeight, PdfGrid grid, PdfGridRow gridRow)
	{
		bool result = false;
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		for (int i = 0; i < gridRow.Cells.Count; i++)
		{
			if ((gridRow.Cells[i].Value is string && gridRow.Cells[i].Value != null) || (gridRow.Cells[i].Value is PdfTextElement && gridRow.Cells[i].Value != null))
			{
				PdfFont font = grid.Style.Font ?? gridRow.Style.Font ?? gridRow.Grid.Style.Font ?? PdfDocument.DefaultFont;
				remainderText = gridRow.Cells[i].Value as string;
				float width = gridRow.Cells[i].Width;
				if (grid.Columns[i].isCustomWidth && gridRow.Cells[i].Width > grid.Columns[i].Width)
				{
					width = grid.Columns[i].Width;
				}
				if (gridRow.Cells[i].Value is PdfTextElement)
				{
					slr = pdfStringLayouter.Layout(remainderText = (gridRow.Cells[i].Value as PdfTextElement).Text, font, gridRow.Cells[i].StringFormat, new SizeF(width, currentHeight));
				}
				else
				{
					slr = pdfStringLayouter.Layout(gridRow.Cells[i].Value as string, font, gridRow.Cells[i].StringFormat, new SizeF(width, currentHeight));
				}
				float num = slr.ActualSize.Height;
				if (num == 0f)
				{
					result = false;
					break;
				}
				if (gridRow.Cells[i].Style != null && gridRow.Cells[i].Style.Borders != null && gridRow.Cells[i].Style.Borders.Top != null && gridRow.Cells[i].Style.Borders.Bottom != null)
				{
					num += (gridRow.Cells[i].Style.Borders.Top.Width + gridRow.Cells[i].Style.Borders.Bottom.Width) * 2f;
				}
				if (slr.LineCount > 1 && gridRow.Cells[i].StringFormat != null && gridRow.Cells[i].StringFormat.LineSpacing != 0f)
				{
					num += (float)(slr.LineCount - 1) * gridRow.Cells[i].StringFormat.LineSpacing;
				}
				num = ((gridRow.Cells[i].Style.CellPadding != null) ? (num + (gridRow.Cells[i].Style.CellPadding.Top + gridRow.Cells[i].Style.CellPadding.Bottom)) : (num + (grid.Style.CellPadding.Top + grid.Style.CellPadding.Bottom)));
				num += grid.Style.CellSpacing;
				if (currentHeight > num || slr.m_remainder != null || (slr.ActualSize.Height > 0f && slr.Remainder == null && slr.LineCount > 1))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
