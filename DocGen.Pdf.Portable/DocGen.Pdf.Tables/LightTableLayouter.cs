using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

internal class LightTableLayouter : ElementLayouter
{
	private class PageLayoutResult
	{
		public PdfPage Page;

		public RectangleF Bounds;

		public bool Finish;

		public int FirstRowIndex;

		public int LastRowIndex;
	}

	private string[] m_row;

	private PdfStringLayoutResult[] m_latestTextResults;

	private float[] m_cellWidths;

	private PdfPage m_currentPage;

	private SizeF m_currentPageBounds;

	private PdfGraphics m_currentGraphics;

	private RectangleF m_currentBounds;

	private float m_cellSpacing;

	private int[] m_spanMap;

	private int m_dropIndex;

	private int m_startColumn;

	private int m_endColumn;

	private int m_previousRowIndex = -1;

	private int m_currentRowIndex;

	private int m_currentCellIndex;

	private int m_currentIndex;

	private bool isRemainder;

	private int currentPageIndex = -1;

	private bool isPreviousReminderText;

	private int previousRowIndex = -1;

	private int previousPageIndex = -1;

	private bool remainderText;

	public PdfLightTable Table => base.Element as PdfLightTable;

	internal int CurrentPageIndex
	{
		get
		{
			PdfSection pdfSection = null;
			if (m_currentPage != null)
			{
				pdfSection = m_currentPage.Section;
				currentPageIndex = pdfSection.IndexOf(m_currentPage);
			}
			return currentPageIndex;
		}
	}

	internal LightTableLayouter(PdfLightTable table)
		: base(table)
	{
	}

	public void Layout(PdfGraphics graphics, PointF location)
	{
		RectangleF boundaries = new RectangleF(location, SizeF.Empty);
		Layout(graphics, boundaries);
	}

	public void Layout(PdfGraphics graphics, RectangleF boundaries)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (graphics.ClientSize.Height < 0f)
		{
			boundaries.Y += graphics.ClientSize.Height;
		}
		_ = graphics.ClientSize.Width;
		_ = boundaries.X;
		PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
		pdfLayoutParams.Bounds = boundaries;
		m_currentGraphics = graphics;
		LayoutInternal(pdfLayoutParams);
	}

	protected override PdfLayoutResult LayoutInternal(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		PdfLightTableLayoutFormat format = GetFormat(param.Format);
		if (format != null)
		{
			m_startColumn = format.StartColumnIndex;
			m_endColumn = format.EndColumnIndex;
		}
		if (m_endColumn == 0)
		{
			m_endColumn = Table.Columns.Count - 1;
		}
		if (m_endColumn < m_startColumn)
		{
			throw new PdfLightTableException("End column index is less than start column index.");
		}
		int count = Table.Columns.Count;
		if (m_startColumn < 0 || m_startColumn >= count || m_endColumn >= count || m_endColumn - m_startColumn > count)
		{
			throw new PdfLightTableException("The selected columns are out of the existing range.");
		}
		m_dropIndex = 0;
		m_row = null;
		m_latestTextResults = null;
		m_currentPage = param.Page;
		m_currentPageBounds = ((m_currentPage != null) ? m_currentPage.GetClientSize() : m_currentGraphics.ClientSize);
		m_currentBounds = param.Bounds;
		PdfLightTableLayoutResult pdfLightTableLayoutResult = null;
		PageLayoutResult pageLayoutResult = null;
		if (m_currentBounds.Width <= 0f)
		{
			float num = m_currentPageBounds.Width - m_currentBounds.X;
			if (num < 0f)
			{
				throw new PdfLightTableException("Can't draw table outside of the page.");
			}
			m_currentBounds.Width = num;
		}
		param.Bounds = m_currentBounds;
		PdfLightTableStyle style = Table.Style;
		m_cellSpacing = style.CellSpacing;
		int currentRow = ((style.HeaderSource == PdfHeaderSource.Rows) ? style.HeaderRowCount : 0);
		m_cellWidths = GetWidths(param.Bounds);
		bool isPageFirst = true;
		while (true)
		{
			bool flag = RaiseBeforePageLayout(m_currentPage, ref m_currentBounds, ref currentRow);
			LightTableEndPageLayoutEventArgs lightTableEndPageLayoutEventArgs = null;
			if (!flag)
			{
				pageLayoutResult = LayoutOnPage(currentRow, param, isPageFirst);
				lightTableEndPageLayoutEventArgs = RaisePageLayouted(pageLayoutResult);
				flag = lightTableEndPageLayoutEventArgs?.Cancel ?? false;
			}
			if (flag || pageLayoutResult.Finish)
			{
				break;
			}
			m_currentPage = ((lightTableEndPageLayoutEventArgs != null && lightTableEndPageLayoutEventArgs.NextPage != null) ? lightTableEndPageLayoutEventArgs.NextPage : GetNextPage(m_currentPage));
			m_currentPageBounds = ((m_currentPage != null) ? m_currentPage.GetClientSize() : m_currentGraphics.ClientSize);
			isPageFirst = false;
			currentRow = pageLayoutResult.LastRowIndex;
			m_currentBounds = GetPaginateBounds(param);
			if (m_currentBounds.Height == 0f)
			{
				m_currentBounds.Y = 0f;
			}
		}
		return GetLayoutResult(pageLayoutResult);
	}

	private PdfLightTableLayoutFormat GetFormat(PdfLayoutFormat format)
	{
		PdfLightTableLayoutFormat pdfLightTableLayoutFormat = format as PdfLightTableLayoutFormat;
		if (format != null && pdfLightTableLayoutFormat == null)
		{
			pdfLightTableLayoutFormat = new PdfLightTableLayoutFormat(format);
		}
		return pdfLightTableLayoutFormat;
	}

	private PdfLightTableLayoutResult GetLayoutResult(PageLayoutResult pageResult)
	{
		PdfPage page = ((pageResult != null) ? pageResult.Page : m_currentPage);
		RectangleF bounds = pageResult?.Bounds ?? RectangleF.Empty;
		if (pageResult == null)
		{
			pageResult = new PageLayoutResult();
		}
		return new PdfLightTableLayoutResult(page, bounds, pageResult.LastRowIndex, m_latestTextResults);
	}

	private PageLayoutResult LayoutOnPage(int startRowIndex, PdfLayoutParams param, bool isPageFirst)
	{
		int rowIndex = startRowIndex;
		RectangleF rectangleF = m_currentBounds;
		if (rectangleF.Height == 0f && m_currentPage != null)
		{
			rectangleF.Height = m_currentPageBounds.Height - rectangleF.Y;
		}
		RectangleF bounds = rectangleF;
		PdfLightTableStyle style = Table.Style;
		PdfPen borderPen = style.BorderPen;
		if (borderPen != null)
		{
			rectangleF = PreserveForBorder(rectangleF, borderPen, style.BorderOverlapStyle);
		}
		rectangleF.Height -= m_cellSpacing;
		rectangleF.Width -= m_cellSpacing;
		float rowHeight = 0f;
		PageLayoutResult pageLayoutResult = new PageLayoutResult();
		bool flag = false;
		bool flag2 = style.ShowHeader && (isPageFirst || style.RepeatHeader);
		bool flag3 = style.HeaderSource != PdfHeaderSource.Rows;
		int headerRowCount = style.HeaderRowCount;
		if (flag2 && !flag3)
		{
			if (headerRowCount > 0)
			{
				rowIndex = 0;
			}
			else
			{
				flag2 = false;
			}
		}
		string[] row = m_row;
		if (flag2)
		{
			m_row = null;
		}
		PdfGraphics pdfGraphics = ((m_currentPage != null) ? m_currentPage.Graphics : m_currentGraphics);
		while (true)
		{
			string[] array = null;
			if (flag2 && flag3)
			{
				if (!Table.isCustomDataSource)
				{
					rowIndex = -1;
					m_previousRowIndex = -2;
				}
				array = Table.GetColumnCaptions();
				if (array == null)
				{
					flag2 = false;
					rowIndex = startRowIndex;
					m_row = row;
					continue;
				}
				array = CropRow(array);
			}
			else
			{
				array = GetRow(rowIndex, param);
			}
			bool stop = array == null;
			if (array != null)
			{
				rectangleF.Y += m_cellSpacing;
				rectangleF.Height -= m_cellSpacing;
				bool flag4 = DrawRow(param, ref rowIndex, array, rectangleF, out rowHeight, flag2, out stop);
				if (!flag4)
				{
					rectangleF.Y += rowHeight;
					rectangleF.Height -= rowHeight;
				}
				else if (rectangleF.Height < rowHeight || rectangleF.Y + m_cellSpacing < bounds.Height)
				{
					rectangleF.Y -= m_cellSpacing;
					rectangleF.Height += m_cellSpacing;
				}
				stop = stop || flag4;
				flag = flag || (rowHeight <= 0f && (startRowIndex == rowIndex || flag2));
				stop = stop || flag;
			}
			else
			{
				pageLayoutResult.Finish = true;
			}
			if (stop)
			{
				break;
			}
			if (flag2 && (flag3 || rowIndex >= headerRowCount))
			{
				flag2 = false;
				if (!Table.isCustomDataSource)
				{
					rowIndex = startRowIndex;
				}
				m_row = row;
			}
		}
		if (rowHeight > 0f)
		{
			rectangleF.Y += m_cellSpacing;
		}
		if (borderPen != null)
		{
			rectangleF.Y += borderPen.Width;
		}
		if (flag2)
		{
			rowIndex = startRowIndex;
		}
		pageLayoutResult.Page = m_currentPage;
		pageLayoutResult.FirstRowIndex = startRowIndex;
		pageLayoutResult.LastRowIndex = rowIndex;
		pageLayoutResult.Bounds = bounds;
		pageLayoutResult.Bounds.Height = rectangleF.Y - bounds.Y;
		RectangleF rectangleF2 = pageLayoutResult.Bounds;
		if (borderPen != null)
		{
			if (style.BorderOverlapStyle == PdfBorderOverlapStyle.Overlap)
			{
				rectangleF2.Height -= borderPen.Width / 2f;
			}
			rectangleF2 = PreserveForBorder(rectangleF2, borderPen, PdfBorderOverlapStyle.Overlap);
		}
		if (borderPen != null && rectangleF2.Bottom < m_currentPageBounds.Height)
		{
			float transparency = (float)(int)borderPen.Color.A / 255f;
			pdfGraphics.Save();
			pdfGraphics.SetTransparency(transparency);
			pdfGraphics.DrawRectangle(borderPen, rectangleF2);
			pdfGraphics.Restore();
		}
		bool flag5 = param.Format == null || param.Format.Layout == PdfLayoutType.OnePage;
		pageLayoutResult.Finish |= flag5;
		if (param.Bounds.Y == rectangleF.Y && flag2)
		{
			flag2 = false;
		}
		if (flag || (m_row != null && flag2))
		{
			throw new PdfLightTableException("Can't draw table, because there is not enough space for it.");
		}
		return pageLayoutResult;
	}

	private string[] CropRow(string[] row)
	{
		string[] array = row;
		if (row != null && (m_endColumn != 0 || m_startColumn != 0))
		{
			int num = m_endColumn - m_startColumn + 1;
			array = new string[num];
			Array.Copy(row, m_startColumn, array, 0, num);
		}
		return array;
	}

	private static RectangleF PreserveForBorder(RectangleF bounds, PdfPen pen, PdfBorderOverlapStyle overlapStyle)
	{
		if (pen != null)
		{
			float width = pen.Width;
			switch (overlapStyle)
			{
			case PdfBorderOverlapStyle.Overlap:
			{
				float num2 = width / 2f;
				bounds.X += num2;
				bounds.Y += num2;
				bounds.Width -= width;
				bounds.Height -= width;
				break;
			}
			case PdfBorderOverlapStyle.Inside:
			{
				float num = width * 2f;
				bounds.X += width;
				bounds.Y += width;
				bounds.Width -= num;
				bounds.Height -= num;
				break;
			}
			default:
				throw new ArgumentException("Unsupported overlap style.");
			}
		}
		return bounds;
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

	private bool DrawRow(PdfLayoutParams param, ref int rowIndex, string[] row, RectangleF rowBouds, out float rowHeight, bool isHeader, out bool stop)
	{
		int num = m_cellWidths.Length;
		PdfStringLayoutResult[] results = null;
		bool hasOwnStyle;
		PdfCellStyle pdfCellStyle = GetCellStyle(rowIndex, isHeader, out hasOwnStyle);
		BeginRowLayoutEventArgs beginRowLayoutEventArgs = null;
		m_spanMap = null;
		float num2 = DetermineRowHeight(param, rowIndex, row, rowBouds, out results, pdfCellStyle);
		bool flag = true;
		PdfStringLayoutResult[] array = results;
		foreach (PdfStringLayoutResult pdfStringLayoutResult in array)
		{
			if (pdfStringLayoutResult.Empty && pdfStringLayoutResult.Lines != null && pdfStringLayoutResult.LineHeight > 0f)
			{
				flag = false;
			}
			if (pdfStringLayoutResult.LineCount > 0 && !string.IsNullOrEmpty(pdfStringLayoutResult.Remainder))
			{
				if (isPreviousReminderText)
				{
					isRemainder = true;
				}
				else
				{
					isPreviousReminderText = true;
				}
			}
			else
			{
				isPreviousReminderText = false;
				remainderText = false;
			}
		}
		if (!isRemainder && !remainderText && CurrentPageIndex != 0 && previousPageIndex != CurrentPageIndex && previousRowIndex == rowIndex)
		{
			isRemainder = true;
		}
		if (flag)
		{
			beginRowLayoutEventArgs = RaiseBeforeRowLayout(rowIndex, pdfCellStyle);
			if (beginRowLayoutEventArgs != null && isRemainder)
			{
				beginRowLayoutEventArgs.IsRowPaginated = true;
				isPreviousReminderText = false;
			}
		}
		bool flag2 = false;
		m_spanMap = null;
		rowHeight = 0f;
		if (beginRowLayoutEventArgs != null)
		{
			stop = beginRowLayoutEventArgs.Cancel;
			flag2 = beginRowLayoutEventArgs.Skip;
			m_spanMap = beginRowLayoutEventArgs.ColumnSpanMap;
			pdfCellStyle = beginRowLayoutEventArgs.CellStyle;
			ValidateSpanMap();
			rowHeight = Math.Max(beginRowLayoutEventArgs.MinimalHeight, rowHeight);
		}
		else
		{
			stop = false;
		}
		if (!stop)
		{
			if (beginRowLayoutEventArgs != null && beginRowLayoutEventArgs.isArgsPropertyModified)
			{
				num2 = DetermineRowHeight(param, rowIndex, row, rowBouds, out results, pdfCellStyle);
				beginRowLayoutEventArgs.isArgsPropertyModified = false;
			}
			if (num2 > 0f)
			{
				rowHeight = Math.Max(num2, rowHeight);
			}
			else
			{
				rowHeight = num2;
			}
			m_latestTextResults = results;
		}
		if ((rowHeight <= 0f) | stop)
		{
			return IsIncomplete(results) | (m_currentPageBounds.Height - rowBouds.Y <= 0f);
		}
		rowBouds.Height = rowHeight;
		if (rowBouds.Y + rowBouds.Height > m_currentPageBounds.Height && m_currentPage != null)
		{
			return true;
		}
		bool flag3 = false;
		RectangleF bounds = rowBouds;
		PdfGraphics graphics = ((m_currentPage != null) ? m_currentPage.Graphics : m_currentGraphics);
		int num3 = 0;
		if (!flag2)
		{
			for (int j = 0; j < num; j++)
			{
				bounds.Width = GetCellWidth(j);
				bool num4 = m_spanMap != null && m_spanMap[j] < 0;
				string value = row[j];
				bool flag4 = false;
				if (!num4)
				{
					bounds.X += m_cellSpacing;
					if (!flag2 && !flag4 && !results[j].Empty)
					{
						BeginCellLayoutEventArgs beginCellLayoutEventArgs = RaiseBeforeCellLayout(graphics, rowIndex, j, bounds, value);
						if (beginCellLayoutEventArgs != null)
						{
							flag4 = beginCellLayoutEventArgs.Skip;
						}
						if (flag4)
						{
							num3++;
						}
					}
				}
				PdfStringLayoutResult pdfStringLayoutResult2 = results[j];
				if (!flag2 && !flag4 && !pdfStringLayoutResult2.Empty)
				{
					bool ignoreColumnFormat = false;
					if (beginRowLayoutEventArgs != null)
					{
						ignoreColumnFormat = beginRowLayoutEventArgs.IgnoreColumnFormat;
					}
					if (isHeader && hasOwnStyle && Table.Columns[j] != null && ((Table.Style.HeaderStyle != null && Table.Style.HeaderStyle.StringFormat != null) || Table.Columns[j].StringFormat == null))
					{
						ignoreColumnFormat = true;
					}
					if (Table.isBuiltinStyle && Table.m_lightTableBuiltinStyle != PdfLightTableBuiltinStyle.TableGrid)
					{
						m_currentRowIndex = rowIndex;
						m_currentCellIndex = j;
						ApplyStyle(Table.m_lightTableBuiltinStyle);
						pdfCellStyle = Table.Style.DefaultStyle;
					}
					pdfStringLayoutResult2 = DrawCell(pdfStringLayoutResult2, bounds, rowIndex, j, pdfCellStyle, ignoreColumnFormat);
					if (Table.isBuiltinStyle && isHeader && Table.m_headerStyle)
					{
						if (pdfCellStyle.Font.Style == PdfFontStyle.Regular)
						{
							pdfCellStyle.Font = CreateBoldFont(pdfCellStyle.Font);
						}
						else if (pdfCellStyle.Font.Style == PdfFontStyle.Bold)
						{
							pdfCellStyle.Font = CreateRegularFont(pdfCellStyle.Font);
						}
					}
					if (previousPageIndex != CurrentPageIndex)
					{
						previousPageIndex = CurrentPageIndex;
					}
					if (previousRowIndex != rowIndex)
					{
						previousRowIndex = rowIndex;
					}
				}
				if (!num4)
				{
					RaiseAfterCellLayout(graphics, rowIndex, j, bounds, value);
				}
				string remainder = pdfStringLayoutResult2.Remainder;
				if (remainder != null && remainder != string.Empty)
				{
					flag3 = true;
				}
				row[j] = remainder;
				if (!num4)
				{
					bounds.X += bounds.Width;
				}
			}
		}
		else
		{
			rowHeight = 0f;
		}
		if (!flag3)
		{
			m_row = null;
			rowIndex++;
		}
		else if (!isHeader)
		{
			m_row = row;
		}
		stop = RaiseAfterRowLayout(rowIndex, !flag3, rowBouds);
		return flag3;
	}

	private void DrawBorder(RectangleF bounds, PdfGraphics graphics, PdfCellStyle style)
	{
		PointF point = new PointF(bounds.X, bounds.Y + bounds.Height);
		PointF location = bounds.Location;
		PdfPen pdfPen = style.Borders.Left;
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(style.Borders.Left.Color, style.Borders.Left.Width);
		}
		pdfPen.LineCap = PdfLineCap.Square;
		SetTransparency(ref graphics, pdfPen);
		graphics.DrawLine(pdfPen, point, location);
		graphics.Restore();
		point = new PointF(bounds.X + bounds.Width, bounds.Y);
		location = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
		pdfPen = style.Borders.Right;
		if (bounds.X + bounds.Width > graphics.ClientSize.Width - pdfPen.Width / 2f)
		{
			point = new PointF(graphics.ClientSize.Width - pdfPen.Width / 2f, bounds.Y);
			location = new PointF(graphics.ClientSize.Width - pdfPen.Width / 2f, bounds.Y + bounds.Height);
		}
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(style.Borders.Right.Color, style.Borders.Right.Width);
		}
		pdfPen.LineCap = PdfLineCap.Square;
		SetTransparency(ref graphics, pdfPen);
		graphics.DrawLine(pdfPen, point, location);
		graphics.Restore();
		point = bounds.Location;
		location = new PointF(bounds.X + bounds.Width, bounds.Y);
		pdfPen = style.Borders.Top;
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(style.Borders.Top.Color, style.Borders.Top.Width);
		}
		pdfPen.LineCap = PdfLineCap.Square;
		SetTransparency(ref graphics, pdfPen);
		graphics.DrawLine(pdfPen, point, location);
		graphics.Restore();
		point = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
		location = new PointF(bounds.X, bounds.Y + bounds.Height);
		pdfPen = style.Borders.Bottom;
		if (bounds.Y + bounds.Height > graphics.ClientSize.Height - pdfPen.Width / 2f)
		{
			point = new PointF(bounds.X + bounds.Width, graphics.ClientSize.Height - pdfPen.Width / 2f);
			location = new PointF(bounds.X, graphics.ClientSize.Height - pdfPen.Width / 2f);
		}
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(style.Borders.Bottom.Color, style.Borders.Bottom.Width);
		}
		pdfPen.LineCap = PdfLineCap.Square;
		SetTransparency(ref graphics, pdfPen);
		graphics.DrawLine(pdfPen, point, location);
		graphics.Restore();
	}

	private void SetTransparency(ref PdfGraphics graphics, PdfPen pen)
	{
		float transparency = (float)(int)pen.Color.A / 255f;
		graphics.Save();
		graphics.SetTransparency(transparency);
	}

	private void ValidateSpanMap()
	{
		if (m_spanMap == null)
		{
			return;
		}
		int num = m_spanMap.Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = m_spanMap[i];
			if (num2 > 1)
			{
				int num3 = num2 + i;
				for (i++; i < num3 && i < num; i++)
				{
					m_spanMap[i] = -1;
				}
				i--;
			}
			else if (num2 < 0)
			{
				throw new PdfLightTableException("Invalid span map.");
			}
		}
	}

	private bool IsIncomplete(PdfStringLayoutResult[] results)
	{
		bool result = false;
		if (results != null)
		{
			foreach (PdfStringLayoutResult pdfStringLayoutResult in results)
			{
				if (pdfStringLayoutResult.Remainder != null && pdfStringLayoutResult.Remainder != string.Empty)
				{
					result = true;
					break;
				}
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	private float DetermineRowHeight(PdfLayoutParams param, int rowIndex, string[] row, RectangleF rowBouds, out PdfStringLayoutResult[] results, PdfCellStyle cs)
	{
		int num = row.Length;
		float height = 0f;
		if (m_currentPage != null)
		{
			height = Math.Min(m_currentPageBounds.Height - rowBouds.Y, rowBouds.Height);
		}
		SizeF size = new SizeF(m_cellWidths[0], height);
		float width = cs.BorderPen.Width;
		float cellPadding = Table.Style.CellPadding;
		bool overlapped = Table.Style.BorderOverlapStyle == PdfBorderOverlapStyle.Overlap;
		height = 0f;
		size.Height = ApplyBordersToHeight(size.Height, width, overlapped);
		if (cellPadding > 0f)
		{
			size.Height = ApplyBordersToHeight(size.Height, cellPadding, overlapped: false);
		}
		results = new PdfStringLayoutResult[num];
		PdfColumnCollection columns = Table.Columns;
		for (int i = 0; i < num; i++)
		{
			PdfStringLayoutResult pdfStringLayoutResult = null;
			if (m_spanMap != null && m_spanMap[i] < 0)
			{
				pdfStringLayoutResult = new PdfStringLayoutResult();
				pdfStringLayoutResult.m_actualSize = SizeF.Empty;
			}
			else
			{
				string text = row[i];
				size.Width = GetCellWidth(i);
				size.Width = ApplyBordersToHeight(size.Width, width, overlapped);
				if (cellPadding > 0f)
				{
					size.Width = ApplyBordersToHeight(size.Width, cellPadding, overlapped: false);
				}
				if (text != null)
				{
					if (text.Equals(string.Empty))
					{
						text = " ";
					}
					if (m_previousRowIndex != rowIndex)
					{
						text = PdfGraphics.NormalizeText(cs.Font, text);
					}
				}
				else
				{
					text = " ";
				}
				PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
				PdfStringFormat stringFormat = columns[i].StringFormat;
				if (stringFormat == null)
				{
					stringFormat = cs.StringFormat;
				}
				if ((Table.m_lightTableBuiltinStyle == PdfLightTableBuiltinStyle.PlainTable3 && ((rowIndex == -1 && Table.m_headerStyle) || (rowIndex == Table.Rows.Count - 1 && Table.m_totalRowStyle))) || (((i == 0 && Table.m_firstColumnStyle) || (i == Table.Columns.Count - 1 && Table.m_lastColumnStyle)) && rowIndex != -1))
				{
					text = text.ToUpper();
				}
				pdfStringLayoutResult = pdfStringLayouter.Layout(text, cs.Font, stringFormat, size);
				bool flag = param.Format != null && param.Format.Break == PdfLayoutBreakType.FitElement;
				string remainder = pdfStringLayoutResult.Remainder;
				flag = ((!Table.AllowRowBreakAcrossPages) ? (!string.IsNullOrEmpty(remainder)) : (flag & !string.IsNullOrEmpty(remainder)));
				if (flag && m_dropIndex != rowIndex)
				{
					DropToNextPage(results, num, row);
					m_dropIndex = rowIndex;
					height = 0f;
					break;
				}
				if (size.Height > 0f || m_currentPage == null)
				{
					height = Math.Max(pdfStringLayoutResult.ActualSize.Height, height);
				}
				else
				{
					pdfStringLayoutResult = new PdfStringLayoutResult();
					pdfStringLayoutResult.m_remainder = text;
					pdfStringLayoutResult.m_actualSize = SizeF.Empty;
				}
			}
			results[i] = pdfStringLayoutResult;
		}
		m_previousRowIndex = rowIndex;
		if (height >= 0f)
		{
			if (m_currentPage != null && rowBouds.Height > 0f)
			{
				height = Math.Min(rowBouds.Height, height);
			}
			if (cellPadding > 0f)
			{
				height = ApplyBordersToHeight(height, 0f - cellPadding, overlapped: false);
			}
			height = ApplyBordersToHeight(height, 0f - width, overlapped);
		}
		return height;
	}

	private void DropToNextPage(PdfStringLayoutResult[] results, int count, string[] row)
	{
		for (int i = 0; i < count; i++)
		{
			PdfStringLayoutResult pdfStringLayoutResult = new PdfStringLayoutResult();
			pdfStringLayoutResult.m_remainder = row[i];
			pdfStringLayoutResult.m_actualSize = SizeF.Empty;
			results[i] = pdfStringLayoutResult;
		}
	}

	private float GetCellWidth(int cellIndex)
	{
		float num = m_cellWidths[cellIndex];
		if (m_spanMap != null && m_spanMap.Length == m_cellWidths.Length)
		{
			int num2 = m_spanMap[cellIndex];
			if (num2 > 1)
			{
				int num3 = m_spanMap.Length;
				int num4 = num2 + cellIndex;
				float cellSpacing = Table.Style.CellSpacing;
				for (int i = cellIndex + 1; i < num4 && i < num3; i++)
				{
					num += m_cellWidths[i] + cellSpacing;
					m_spanMap[i] = -1;
				}
			}
		}
		return num;
	}

	private static float ApplyBordersToHeight(float height, float borderWidth, bool overlapped)
	{
		height = ((!overlapped) ? (height - borderWidth * 2f) : (height - borderWidth));
		if (height < 0f)
		{
			height = 0f;
		}
		return height;
	}

	private PdfStringLayoutResult DrawCell(PdfStringLayoutResult layoutResult, RectangleF bounds, int rowIndex, int cellIndex, PdfCellStyle cs, bool ignoreColumnFormat)
	{
		PdfGraphics pdfGraphics = ((m_currentPage != null) ? m_currentPage.Graphics : m_currentGraphics);
		bool flag = Table.Style.BorderOverlapStyle == PdfBorderOverlapStyle.Overlap;
		float cellPadding = Table.Style.CellPadding;
		PdfPen borderPen = cs.BorderPen;
		PdfBrush backgroundBrush = cs.BackgroundBrush;
		PdfBorders borders = cs.Borders;
		if (m_spanMap != null && m_spanMap[cellIndex] == -1)
		{
			return new PdfStringLayoutResult();
		}
		if (!flag)
		{
			bounds = PreserveForBorder(bounds, borderPen, PdfBorderOverlapStyle.Overlap);
		}
		if (backgroundBrush != null)
		{
			float alpha = GetAlpha(backgroundBrush);
			pdfGraphics.Save();
			pdfGraphics.SetTransparency(alpha);
			pdfGraphics.DrawRectangle(null, backgroundBrush, bounds);
			pdfGraphics.Restore();
		}
		if (borders == null)
		{
			if (borderPen != null)
			{
				float transparency = (float)(int)borderPen.Color.A / 255f;
				pdfGraphics.Save();
				pdfGraphics.SetTransparency(transparency);
				pdfGraphics.DrawRectangle(borderPen, null, bounds);
				pdfGraphics.Restore();
			}
		}
		else
		{
			DrawBorder(bounds, pdfGraphics, cs);
		}
		bounds = PreserveForBorder(bounds, borderPen, PdfBorderOverlapStyle.Overlap);
		if (cellPadding > 0f)
		{
			bounds.X += cellPadding;
			bounds.Y += cellPadding;
			bounds.Width -= cellPadding * 2f;
			bounds.Height -= cellPadding * 2f;
		}
		if (!layoutResult.Empty)
		{
			PdfColumn pdfColumn = Table.Columns[cellIndex];
			PdfStringFormat pdfStringFormat = (ignoreColumnFormat ? cs.StringFormat : pdfColumn.StringFormat);
			if (pdfStringFormat == null)
			{
				pdfStringFormat = cs.StringFormat;
			}
			RectangleF layoutRectangle = bounds;
			RectangleF rectangleF = pdfGraphics.CheckCorrectLayoutRectangle(layoutResult.ActualSize, layoutRectangle.X, layoutRectangle.Y, pdfStringFormat);
			if (layoutRectangle.Width <= 0f)
			{
				layoutRectangle.X = rectangleF.X;
				layoutRectangle.Width = rectangleF.Width;
			}
			if (layoutRectangle.Height <= 0f)
			{
				layoutRectangle.Y = rectangleF.Y;
				layoutRectangle.Height = rectangleF.Height;
			}
			pdfGraphics.DrawStringLayoutResult(layoutResult, cs.Font, cs.TextPen, cs.TextBrush, layoutRectangle, pdfStringFormat);
		}
		return layoutResult;
	}

	private PdfCellStyle GetCellStyle(int rowIndex, bool isHeader, out bool hasOwnStyle)
	{
		PdfLightTableStyle style = Table.Style;
		hasOwnStyle = false;
		PdfCellStyle pdfCellStyle;
		if (!isHeader)
		{
			pdfCellStyle = (((rowIndex & 1) <= 0) ? style.DefaultStyle : style.AlternateStyle);
		}
		else
		{
			pdfCellStyle = style.HeaderStyle;
			hasOwnStyle = true;
		}
		if (pdfCellStyle == null)
		{
			pdfCellStyle = style.DefaultStyle;
			hasOwnStyle = false;
		}
		return pdfCellStyle;
	}

	private float[] GetWidths(RectangleF bounds)
	{
		int num = m_endColumn - m_startColumn + 1;
		PdfLightTableStyle style = Table.Style;
		float num2 = style.BorderPen?.Width ?? 0f;
		if (style.BorderOverlapStyle == PdfBorderOverlapStyle.Inside)
		{
			num2 *= 2f;
		}
		float totalWidth = bounds.Width - style.CellSpacing * (float)(num + 1) - num2;
		return Table.Columns.GetWidths(totalWidth, m_startColumn, m_endColumn, Table.ColumnProportionalSizing);
	}

	private string[] GetRow(int startRowIndex, PdfLayoutParams param)
	{
		if (m_row != null)
		{
			return m_row;
		}
		string[] nextRow = Table.GetNextRow(ref startRowIndex);
		return CropRow(nextRow);
	}

	private float GetAlpha(PdfBrush brush)
	{
		PdfSolidBrush pdfSolidBrush = brush as PdfSolidBrush;
		PdfLinearGradientBrush pdfLinearGradientBrush = brush as PdfLinearGradientBrush;
		float result = 1f;
		if (pdfSolidBrush != null)
		{
			result = (float)(int)pdfSolidBrush.Color.A / 255f;
		}
		else if (pdfLinearGradientBrush != null)
		{
			PdfColor pdfColor = new PdfColor(0, 0, 0);
			PdfColor pdfColor2 = new PdfColor(0, 0, 0);
			PdfColor[] linearColors = pdfLinearGradientBrush.LinearColors;
			if (linearColors != null)
			{
				pdfColor = linearColors[0];
				pdfColor2 = linearColors[1];
			}
			if ((pdfColor.IsEmpty && pdfColor2.IsEmpty) || (pdfColor.A == 0 && pdfColor2.A == 0))
			{
				pdfColor = pdfLinearGradientBrush.InterpolationColors.Colors[0];
			}
			result = (float)(int)pdfColor.A / 255f;
		}
		return result;
	}

	private bool RaiseBeforePageLayout(PdfPage currentPage, ref RectangleF currentBounds, ref int currentRow)
	{
		bool result = false;
		if (base.Element.RaiseBeginPageLayout)
		{
			LightTableBeginPageLayoutEventArgs lightTableBeginPageLayoutEventArgs = new LightTableBeginPageLayoutEventArgs(currentBounds, currentPage, currentRow);
			base.Element.OnBeginPageLayout(lightTableBeginPageLayoutEventArgs);
			result = lightTableBeginPageLayoutEventArgs.Cancel;
			currentBounds = lightTableBeginPageLayoutEventArgs.Bounds;
			currentRow = lightTableBeginPageLayoutEventArgs.StartRowIndex;
		}
		return result;
	}

	private LightTableEndPageLayoutEventArgs RaisePageLayouted(PageLayoutResult pageResult)
	{
		LightTableEndPageLayoutEventArgs lightTableEndPageLayoutEventArgs = null;
		if (base.Element.RaiseEndPageLayout)
		{
			PdfLightTableLayoutResult layoutResult = GetLayoutResult(pageResult);
			int lastRowIndex = pageResult.LastRowIndex;
			lastRowIndex--;
			lightTableEndPageLayoutEventArgs = new LightTableEndPageLayoutEventArgs(layoutResult, pageResult.FirstRowIndex, lastRowIndex);
			base.Element.OnEndPageLayout(lightTableEndPageLayoutEventArgs);
		}
		return lightTableEndPageLayoutEventArgs;
	}

	private BeginRowLayoutEventArgs RaiseBeforeRowLayout(int rowIndex, PdfCellStyle cellStyle)
	{
		BeginRowLayoutEventArgs beginRowLayoutEventArgs = null;
		if (Table.RaiseBeginRowLayout)
		{
			beginRowLayoutEventArgs = new BeginRowLayoutEventArgs(rowIndex, cellStyle);
			if (isRemainder)
			{
				beginRowLayoutEventArgs.IsRowPaginated = true;
			}
			Table.OnBeginRowLayout(beginRowLayoutEventArgs);
			m_currentIndex = rowIndex;
			isRemainder = false;
		}
		return beginRowLayoutEventArgs;
	}

	private bool RaiseAfterRowLayout(int rowIndex, bool isComplete, RectangleF rowBouds)
	{
		bool result = false;
		if (Table.RaiseEndRowLayout)
		{
			EndRowLayoutEventArgs endRowLayoutEventArgs = new EndRowLayoutEventArgs(rowIndex, isComplete, rowBouds);
			Table.OnEndRowLayout(endRowLayoutEventArgs);
			result = endRowLayoutEventArgs.Cancel;
		}
		return result;
	}

	private BeginCellLayoutEventArgs RaiseBeforeCellLayout(PdfGraphics graphics, int rowIndex, int cellIndex, RectangleF bounds, string value)
	{
		BeginCellLayoutEventArgs beginCellLayoutEventArgs = null;
		if (Table.RaiseBeginCellLayout)
		{
			beginCellLayoutEventArgs = new BeginCellLayoutEventArgs(graphics, rowIndex, cellIndex, bounds, value);
			Table.OnBeginCellLayout(beginCellLayoutEventArgs);
		}
		return beginCellLayoutEventArgs;
	}

	private void RaiseAfterCellLayout(PdfGraphics graphics, int rowIndex, int cellIndex, RectangleF bounds, string value)
	{
		if (Table.RaiseEndCellLayout)
		{
			EndCellLayoutEventArgs args = new EndCellLayoutEventArgs(graphics, rowIndex, cellIndex, bounds, value);
			Table.OnEndCellLayout(args);
		}
	}

	private void ApplyStyle(PdfLightTableBuiltinStyle tableStyle)
	{
		switch (tableStyle)
		{
		case PdfLightTableBuiltinStyle.TableGridLight:
			ApplyTableGridLight(Color.FromArgb(255, 191, 191, 191));
			break;
		case PdfLightTableBuiltinStyle.PlainTable1:
			ApplyPlainTable1(Color.FromArgb(255, 191, 191, 191), Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfLightTableBuiltinStyle.PlainTable2:
			ApplyPlainTable2(Color.FromArgb(255, 127, 127, 127));
			break;
		case PdfLightTableBuiltinStyle.PlainTable3:
			ApplyPlainTable3(Color.FromArgb(255, 127, 127, 127), Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfLightTableBuiltinStyle.PlainTable4:
			ApplyPlainTable4(Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfLightTableBuiltinStyle.PlainTable5:
			ApplyPlainTable5(Color.FromArgb(255, 127, 127, 127), Color.FromArgb(255, 242, 242, 242));
			break;
		case PdfLightTableBuiltinStyle.GridTable1Light:
			ApplyGridTable1Light(Color.FromArgb(255, 153, 153, 153), Color.FromArgb(255, 102, 102, 102));
			break;
		case PdfLightTableBuiltinStyle.GridTable1LightAccent1:
			ApplyGridTable1Light(Color.FromArgb(255, 189, 214, 238), Color.FromArgb(255, 156, 194, 229));
			break;
		case PdfLightTableBuiltinStyle.GridTable1LightAccent2:
			ApplyGridTable1Light(Color.FromArgb(255, 247, 202, 172), Color.FromArgb(255, 244, 176, 131));
			break;
		case PdfLightTableBuiltinStyle.GridTable1LightAccent3:
			ApplyGridTable1Light(Color.FromArgb(255, 219, 219, 219), Color.FromArgb(255, 201, 201, 201));
			break;
		case PdfLightTableBuiltinStyle.GridTable1LightAccent4:
			ApplyGridTable1Light(Color.FromArgb(255, 255, 229, 153), Color.FromArgb(255, 255, 217, 102));
			break;
		case PdfLightTableBuiltinStyle.GridTable1LightAccent5:
			ApplyGridTable1Light(Color.FromArgb(255, 180, 198, 231), Color.FromArgb(255, 142, 170, 219));
			break;
		case PdfLightTableBuiltinStyle.GridTable1LightAccent6:
			ApplyGridTable1Light(Color.FromArgb(255, 192, 224, 179), Color.FromArgb(255, 168, 208, 141));
			break;
		case PdfLightTableBuiltinStyle.GridTable2:
			ApplyGridTable2(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfLightTableBuiltinStyle.GridTable2Accent1:
			ApplyGridTable2(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfLightTableBuiltinStyle.GridTable2Accent2:
			ApplyGridTable2(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfLightTableBuiltinStyle.GridTable2Accent3:
			ApplyGridTable2(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfLightTableBuiltinStyle.GridTable2Accent4:
			ApplyGridTable2(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfLightTableBuiltinStyle.GridTable2Accent5:
			ApplyGridTable2(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfLightTableBuiltinStyle.GridTable2Accent6:
			ApplyGridTable2(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfLightTableBuiltinStyle.GridTable3:
			ApplyGridTable3(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfLightTableBuiltinStyle.GridTable3Accent1:
			ApplyGridTable3(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfLightTableBuiltinStyle.GridTable3Accent2:
			ApplyGridTable3(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfLightTableBuiltinStyle.GridTable3Accent3:
			ApplyGridTable3(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfLightTableBuiltinStyle.GridTable3Accent4:
			ApplyGridTable3(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfLightTableBuiltinStyle.GridTable3Accent5:
			ApplyGridTable3(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfLightTableBuiltinStyle.GridTable3Accent6:
			ApplyGridTable3(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfLightTableBuiltinStyle.GridTable4:
			ApplyGridTable4(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.GridTable4Accent1:
			ApplyGridTable4(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 91, 155, 213));
			break;
		case PdfLightTableBuiltinStyle.GridTable4Accent2:
			ApplyGridTable4(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 237, 125, 49));
			break;
		case PdfLightTableBuiltinStyle.GridTable4Accent3:
			ApplyGridTable4(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 165, 165, 165));
			break;
		case PdfLightTableBuiltinStyle.GridTable4Accent4:
			ApplyGridTable4(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 255, 192, 0));
			break;
		case PdfLightTableBuiltinStyle.GridTable4Accent5:
			ApplyGridTable4(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 68, 114, 196));
			break;
		case PdfLightTableBuiltinStyle.GridTable4Accent6:
			ApplyGridTable4(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 112, 173, 71));
			break;
		case PdfLightTableBuiltinStyle.GridTable5Dark:
			ApplyGridTable5Dark(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 153, 153, 153), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfLightTableBuiltinStyle.GridTable5DarkAccent1:
			ApplyGridTable5Dark(Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 189, 214, 238), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfLightTableBuiltinStyle.GridTable5DarkAccent2:
			ApplyGridTable5Dark(Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 247, 202, 172), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfLightTableBuiltinStyle.GridTable5DarkAccent3:
			ApplyGridTable5Dark(Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 219, 219, 219), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfLightTableBuiltinStyle.GridTable5DarkAccent4:
			ApplyGridTable5Dark(Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 229, 153), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfLightTableBuiltinStyle.GridTable5DarkAccent5:
			ApplyGridTable5Dark(Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 180, 198, 231), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfLightTableBuiltinStyle.GridTable5DarkAccent6:
			ApplyGridTable5Dark(Color.FromArgb(255, 112, 171, 71), Color.FromArgb(255, 197, 224, 179), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfLightTableBuiltinStyle.GridTable6Colorful:
			ApplyGridTable6Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.GridTable6ColorfulAccent1:
			ApplyGridTable6Colorful(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfLightTableBuiltinStyle.GridTable6ColorfulAccent2:
			ApplyGridTable6Colorful(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfLightTableBuiltinStyle.GridTable6ColorfulAccent3:
			ApplyGridTable6Colorful(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfLightTableBuiltinStyle.GridTable6ColorfulAccent4:
			ApplyGridTable6Colorful(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfLightTableBuiltinStyle.GridTable6ColorfulAccent5:
			ApplyGridTable6Colorful(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfLightTableBuiltinStyle.GridTable6ColorfulAccent6:
			ApplyGridTable6Colorful(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		case PdfLightTableBuiltinStyle.GridTable7Colorful:
			ApplyGridTable7Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.GridTable7ColorfulAccent1:
			ApplyGridTable7Colorful(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfLightTableBuiltinStyle.GridTable7ColorfulAccent2:
			ApplyGridTable7Colorful(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfLightTableBuiltinStyle.GridTable7ColorfulAccent3:
			ApplyGridTable7Colorful(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfLightTableBuiltinStyle.GridTable7ColorfulAccent4:
			ApplyGridTable7Colorful(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfLightTableBuiltinStyle.GridTable7ColorfulAccent5:
			ApplyGridTable7Colorful(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfLightTableBuiltinStyle.GridTable7ColorfulAccent6:
			ApplyGridTable7Colorful(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		case PdfLightTableBuiltinStyle.ListTable1Light:
			ApplyListTable1Light(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfLightTableBuiltinStyle.ListTable1LightAccent1:
			ApplyListTable1Light(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfLightTableBuiltinStyle.ListTable1LightAccent2:
			ApplyListTable1Light(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfLightTableBuiltinStyle.ListTable1LightAccent3:
			ApplyListTable1Light(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfLightTableBuiltinStyle.ListTable1LightAccent4:
			ApplyListTable1Light(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfLightTableBuiltinStyle.ListTable1LightAccent5:
			ApplyListTable1Light(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfLightTableBuiltinStyle.ListTable1LightAccent6:
			ApplyListTable1Light(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfLightTableBuiltinStyle.ListTable2:
			ApplyListTable2(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfLightTableBuiltinStyle.ListTable2Accent1:
			ApplyListTable2(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfLightTableBuiltinStyle.ListTable2Accent2:
			ApplyListTable2(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfLightTableBuiltinStyle.ListTable2Accent3:
			ApplyListTable2(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfLightTableBuiltinStyle.ListTable2Accent4:
			ApplyListTable2(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfLightTableBuiltinStyle.ListTable2Accent5:
			ApplyListTable2(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfLightTableBuiltinStyle.ListTable2Accent6:
			ApplyListTable2(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfLightTableBuiltinStyle.ListTable3:
			ApplyListTable3(Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable3Accent1:
			ApplyListTable3(Color.FromArgb(255, 91, 155, 213));
			break;
		case PdfLightTableBuiltinStyle.ListTable3Accent2:
			ApplyListTable3(Color.FromArgb(255, 237, 125, 49));
			break;
		case PdfLightTableBuiltinStyle.ListTable3Accent3:
			ApplyListTable3(Color.FromArgb(255, 165, 165, 165));
			break;
		case PdfLightTableBuiltinStyle.ListTable3Accent4:
			ApplyListTable3(Color.FromArgb(255, 255, 192, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable3Accent5:
			ApplyListTable3(Color.FromArgb(255, 68, 114, 196));
			break;
		case PdfLightTableBuiltinStyle.ListTable3Accent6:
			ApplyListTable3(Color.FromArgb(255, 112, 171, 71));
			break;
		case PdfLightTableBuiltinStyle.ListTable4:
			ApplyListTable4(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 204, 204, 204));
			break;
		case PdfLightTableBuiltinStyle.ListTable4Accent1:
			ApplyListTable4(Color.FromArgb(255, 156, 194, 229), Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 222, 234, 246));
			break;
		case PdfLightTableBuiltinStyle.ListTable4Accent2:
			ApplyListTable4(Color.FromArgb(255, 244, 176, 131), Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 251, 228, 213));
			break;
		case PdfLightTableBuiltinStyle.ListTable4Accent3:
			ApplyListTable4(Color.FromArgb(255, 201, 201, 201), Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 237, 237, 237));
			break;
		case PdfLightTableBuiltinStyle.ListTable4Accent4:
			ApplyListTable4(Color.FromArgb(255, 255, 217, 102), Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 242, 204));
			break;
		case PdfLightTableBuiltinStyle.ListTable4Accent5:
			ApplyListTable4(Color.FromArgb(255, 142, 170, 219), Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 217, 226, 243));
			break;
		case PdfLightTableBuiltinStyle.ListTable4Accent6:
			ApplyListTable4(Color.FromArgb(255, 168, 208, 141), Color.FromArgb(255, 112, 173, 71), Color.FromArgb(255, 226, 239, 217));
			break;
		case PdfLightTableBuiltinStyle.ListTable5Dark:
			ApplyListTable5Dark(Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable5DarkAccent1:
			ApplyListTable5Dark(Color.FromArgb(255, 91, 155, 213));
			break;
		case PdfLightTableBuiltinStyle.ListTable5DarkAccent2:
			ApplyListTable5Dark(Color.FromArgb(255, 237, 125, 49));
			break;
		case PdfLightTableBuiltinStyle.ListTable5DarkAccent3:
			ApplyListTable5Dark(Color.FromArgb(255, 165, 165, 165));
			break;
		case PdfLightTableBuiltinStyle.ListTable5DarkAccent4:
			ApplyListTable5Dark(Color.FromArgb(255, 255, 192, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable5DarkAccent5:
			ApplyListTable5Dark(Color.FromArgb(255, 68, 114, 196));
			break;
		case PdfLightTableBuiltinStyle.ListTable5DarkAccent6:
			ApplyListTable5Dark(Color.FromArgb(255, 112, 173, 71));
			break;
		case PdfLightTableBuiltinStyle.ListTable6Colorful:
			ApplyListTable6Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable6ColorfulAccent1:
			ApplyListTable6Colorful(Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfLightTableBuiltinStyle.ListTable6ColorfulAccent2:
			ApplyListTable6Colorful(Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfLightTableBuiltinStyle.ListTable6ColorfulAccent3:
			ApplyListTable6Colorful(Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfLightTableBuiltinStyle.ListTable6ColorfulAccent4:
			ApplyListTable6Colorful(Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable6ColorfulAccent5:
			ApplyListTable6Colorful(Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfLightTableBuiltinStyle.ListTable6ColorfulAccent6:
			ApplyListTable6Colorful(Color.FromArgb(255, 112, 173, 71), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		case PdfLightTableBuiltinStyle.ListTable7Colorful:
			ApplyListTable7Colorful(Color.FromArgb(255, 102, 102, 102), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 0, 0, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable7ColorfulAccent1:
			ApplyListTable7Colorful(Color.FromArgb(255, 91, 155, 213), Color.FromArgb(255, 222, 234, 246), Color.FromArgb(255, 46, 116, 181));
			break;
		case PdfLightTableBuiltinStyle.ListTable7ColorfulAccent2:
			ApplyListTable7Colorful(Color.FromArgb(255, 237, 125, 49), Color.FromArgb(255, 251, 228, 213), Color.FromArgb(255, 196, 89, 17));
			break;
		case PdfLightTableBuiltinStyle.ListTable7ColorfulAccent3:
			ApplyListTable7Colorful(Color.FromArgb(255, 165, 165, 165), Color.FromArgb(255, 237, 237, 237), Color.FromArgb(255, 123, 123, 123));
			break;
		case PdfLightTableBuiltinStyle.ListTable7ColorfulAccent4:
			ApplyListTable7Colorful(Color.FromArgb(255, 255, 192, 0), Color.FromArgb(255, 255, 242, 204), Color.FromArgb(255, 191, 143, 0));
			break;
		case PdfLightTableBuiltinStyle.ListTable7ColorfulAccent5:
			ApplyListTable7Colorful(Color.FromArgb(255, 68, 114, 196), Color.FromArgb(255, 217, 226, 243), Color.FromArgb(255, 47, 84, 150));
			break;
		case PdfLightTableBuiltinStyle.ListTable7ColorfulAccent6:
			ApplyListTable7Colorful(Color.FromArgb(255, 112, 173, 71), Color.FromArgb(255, 226, 239, 217), Color.FromArgb(255, 83, 129, 53));
			break;
		}
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
		if (!firstColumn)
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
		if (!headerRow)
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
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen all = new PdfPen(borderColor, 0.5f);
		pdfCellStyle.Borders.All = all;
	}

	private void ApplyPlainTable1(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		if (m_currentRowIndex < 0 && Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen all = (pdfCellStyle.BorderPen = new PdfPen(borderColor, 0.5f));
		pdfCellStyle.Borders.All = all;
		PdfSolidBrush backgroundBrush = new PdfSolidBrush(backColor);
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle && (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (m_currentRowIndex == Table.Rows.Count - 1 && Table.m_totalRowStyle)
		{
			pdfCellStyle.BackgroundBrush = null;
			if (Table.m_bandedColStyle && (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = new PdfPen(borderColor);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.BackgroundBrush = null;
			if (Table.m_bandedRowStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
	}

	private void ApplyPlainTable2(Color borderColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen pdfPen2 = new PdfPen(Color.Empty);
		pdfCellStyle.Borders.All = pdfPen2;
		pdfCellStyle.Borders.Top = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.All = pdfPen;
			pdfCellStyle.Borders.Left = pdfPen2;
			pdfCellStyle.Borders.Right = pdfPen2;
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.Borders.Left = pdfPen;
				pdfCellStyle.Borders.Right = pdfPen;
			}
			if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
			{
				pdfCellStyle.Borders.Left = pdfPen2;
			}
			if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1)
			{
				pdfCellStyle.Borders.Right = pdfPen2;
			}
		}
		pdfCellStyle.Borders.All = pdfPen2;
		if (Table.m_bandedRowStyle)
		{
			pdfCellStyle.Borders.Top = pdfPen;
			pdfCellStyle.Borders.Bottom = pdfPen;
		}
		if (Table.m_bandedColStyle)
		{
			pdfCellStyle.Borders.Left = pdfPen;
			pdfCellStyle.Borders.Right = pdfPen;
		}
		if (m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Borders.Bottom = pdfPen;
			if (Table.m_totalRowStyle)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Left = pdfPen2;
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Right = pdfPen2;
		}
	}

	private void ApplyPlainTable3(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all2 = new PdfPen(backColor, 0.5f);
		pdfCellStyle.Borders.All = all;
		if (m_currentRowIndex < 0)
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				if (pdfCellStyle.BackgroundBrush != null)
				{
					pdfCellStyle.Borders.All = all2;
				}
			}
			if (Table.m_headerStyle)
			{
				if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
				{
					pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
				}
				pdfCellStyle.Borders.Bottom = pdfPen;
			}
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
			{
				pdfCellStyle.Borders.Left = pdfPen;
			}
			else
			{
				pdfCellStyle.Borders.All = all;
			}
			if (Table.m_headerStyle && m_currentRowIndex == 0)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				if (pdfCellStyle.BackgroundBrush != null)
				{
					if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
					{
						pdfCellStyle.Borders.Left = pdfPen;
					}
					else
					{
						pdfCellStyle.Borders.All = all;
					}
					if (Table.m_headerStyle && m_currentRowIndex == 0)
					{
						pdfCellStyle.Borders.Top = pdfPen;
					}
				}
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
				if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
				{
					pdfCellStyle.Borders.Left = pdfPen;
				}
				else
				{
					pdfCellStyle.Borders.All = all;
				}
				if (Table.m_headerStyle && m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
		}
		if (m_currentRowIndex == Table.Rows.Count - 1 && Table.m_totalRowStyle)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			if (Table.m_bandedColStyle)
			{
				if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
					{
						pdfCellStyle.Borders.Left = pdfPen;
					}
					else
					{
						pdfCellStyle.Borders.All = all2;
					}
				}
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Right = pdfPen;
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			if (m_currentRowIndex < 0 && !Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (pdfCellStyle.BackgroundBrush != null)
			{
				pdfCellStyle.Borders.All = all2;
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			if (m_currentRowIndex == 0 && Table.m_headerStyle)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
	}

	private void ApplyPlainTable4(Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfPen all = new PdfPen(backColor, 0.5f);
		PdfPen all2 = new PdfPen(Color.Empty);
		pdfCellStyle.Borders.All = all2;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				if (pdfCellStyle.BackgroundBrush != null)
				{
					pdfCellStyle.Borders.All = all;
				}
			}
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (pdfCellStyle.BackgroundBrush != null)
			{
				pdfCellStyle.Borders.All = all;
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				if (pdfCellStyle.BackgroundBrush != null)
				{
					pdfCellStyle.Borders.All = all;
				}
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					pdfCellStyle.Borders.All = all;
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all2;
			if (Table.m_bandedColStyle)
			{
				if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					pdfCellStyle.Borders.All = all;
				}
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		pdfCellStyle.Borders.All = all2;
		if (Table.m_bandedRowStyle)
		{
			if (m_currentRowIndex < 0 && !Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (pdfCellStyle.BackgroundBrush != null)
			{
				pdfCellStyle.Borders.All = all;
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
	}

	private void ApplyPlainTable5(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		new PdfPen(backColor, 0.5f);
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty);
		pdfCellStyle.Borders.All = all;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Bottom = pdfPen;
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			if (!Table.m_headerStyle || m_currentRowIndex >= 0)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (pdfCellStyle.BackgroundBrush != null)
			{
				if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
				{
					pdfCellStyle.Borders.Left = pdfPen;
				}
				else
				{
					pdfCellStyle.Borders.All = all;
				}
				if (Table.m_headerStyle && m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				if (!Table.m_headerStyle || m_currentRowIndex >= 0)
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
					{
						pdfCellStyle.Borders.Left = pdfPen;
					}
					else
					{
						pdfCellStyle.Borders.All = all;
					}
					if (Table.m_headerStyle && m_currentRowIndex == 0)
					{
						pdfCellStyle.Borders.Top = pdfPen;
					}
				}
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
					{
						pdfCellStyle.Borders.Left = pdfPen;
					}
					else
					{
						pdfCellStyle.Borders.All = all;
					}
					if (Table.m_headerStyle && m_currentRowIndex == 0)
					{
						pdfCellStyle.Borders.Top = pdfPen;
					}
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = pdfPen;
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Borders.All = all;
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Right = pdfPen;
			if (m_currentRowIndex == 0)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Left = pdfPen;
		}
	}

	private void ApplyGridTable1Light(Color borderColor, Color headerBottomColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		pdfCellStyle.Borders.All = new PdfPen(borderColor, 0.5f);
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Bottom = new PdfPen(headerBottomColor);
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Borders.Top = new PdfPen(headerBottomColor);
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
	}

	private void ApplyGridTable2(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		pdfCellStyle.Borders.All = new PdfPen(borderColor, 0.5f);
		PdfPen pdfPen = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		if (m_currentCellIndex == 0)
		{
			pdfCellStyle.Borders.Left = pdfPen;
		}
		else if (m_currentCellIndex == Table.Columns.Count - 1)
		{
			pdfCellStyle.Borders.Right = pdfPen;
		}
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.All = pdfPen;
			pdfCellStyle.Borders.Bottom = new PdfPen(borderColor);
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			if (!Table.m_headerStyle || m_currentRowIndex >= 0)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle && (!Table.m_headerStyle || m_currentRowIndex >= 0))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.All = pdfPen;
			pdfCellStyle.Borders.Top = new PdfPen(borderColor);
			pdfCellStyle.BackgroundBrush = null;
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		pdfCellStyle.Borders.All = pdfPen;
		if (Table.m_bandedRowStyle)
		{
			if (m_currentRowIndex < 0 && !Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
	}

	private void ApplyGridTable3(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		pdfCellStyle.Borders.All = pdfPen;
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.All = all;
		}
		if (Table.m_bandedRowStyle && Table.m_bandedColStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (Table.m_headerStyle && m_currentRowIndex < 0)
			{
				pdfCellStyle.BackgroundBrush = null;
			}
		}
		else
		{
			if (Table.m_bandedColStyle && (!Table.m_headerStyle || m_currentRowIndex >= 0))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Borders.All = all;
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				if (!Table.m_headerStyle || m_currentRowIndex >= 0)
				{
					pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
				}
				pdfCellStyle.BackgroundBrush = null;
				pdfCellStyle.Borders.All = all;
				if (m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
			else
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
		{
			return;
		}
		if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			if (!Table.m_headerStyle || m_currentRowIndex >= 0)
			{
				pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			}
			if (m_currentRowIndex == 0)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		else
		{
			pdfCellStyle.Borders.Top = pdfPen;
		}
	}

	private void ApplyGridTable4(Color borderColor, Color backColor, Color headerColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		new PdfPen(Color.Empty);
		PdfPen pdfPen2 = new PdfPen(headerColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfBrush backgroundBrush2 = new PdfSolidBrush(headerColor);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		pdfCellStyle.Borders.All = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.BackgroundBrush = backgroundBrush2;
			pdfCellStyle.TextBrush = textBrush;
			pdfCellStyle.Borders.Left = pdfPen2;
			pdfCellStyle.Borders.Right = pdfPen2;
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			else
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
			if (Table.m_headerStyle && m_currentRowIndex < 0)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush2;
			}
		}
		else
		{
			if (Table.m_bandedColStyle && (!Table.m_headerStyle || m_currentRowIndex >= 0))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0)
				{
					if (Table.m_headerStyle)
					{
						pdfCellStyle.BackgroundBrush = backgroundBrush2;
					}
					else
					{
						pdfCellStyle.BackgroundBrush = backgroundBrush;
					}
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = pdfPen;
			if (Table.m_bandedColStyle)
			{
				if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					pdfCellStyle.Borders.All = pdfPen;
				}
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Left = pdfPen2;
			pdfCellStyle.Borders.Right = pdfPen2;
			pdfCellStyle.Borders.Top = new PdfPen(borderColor);
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		if (Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
		}
		if (m_currentRowIndex < 0)
		{
			if (Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush2;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
	}

	private void ApplyGridTable5Dark(Color headerColor, Color oddRowColor, Color evenRowColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(Color.FromArgb(255, 255, 255, 255), 0.5f);
		PdfPen pdfPen2 = new PdfPen(headerColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(headerColor);
		PdfBrush backgroundBrush2 = new PdfSolidBrush(oddRowColor);
		PdfBrush backgroundBrush3 = new PdfSolidBrush(evenRowColor);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		pdfCellStyle.Borders.All = pdfPen;
		pdfCellStyle.BackgroundBrush = backgroundBrush3;
		pdfCellStyle.BorderPen = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.BackgroundBrush = backgroundBrush;
			pdfCellStyle.TextBrush = textBrush;
			pdfCellStyle.Borders.Left = pdfPen2;
			pdfCellStyle.Borders.Right = pdfPen2;
		}
		if (Table.m_bandedRowStyle && Table.m_bandedColStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, oddRowColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, oddRowColor, m_currentRowIndex);
			}
			if (m_currentRowIndex < 0 && Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush3;
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, oddRowColor, m_currentCellIndex);
				if (m_currentRowIndex < 0 && Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				if (pdfCellStyle.BackgroundBrush == null)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush3;
				}
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush2;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, oddRowColor, m_currentRowIndex);
					if (m_currentRowIndex < 0 && Table.m_headerStyle)
					{
						pdfCellStyle.BackgroundBrush = backgroundBrush;
					}
					if (pdfCellStyle.BackgroundBrush == null)
					{
						pdfCellStyle.BackgroundBrush = backgroundBrush3;
					}
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.BackgroundBrush = backgroundBrush;
			pdfCellStyle.TextBrush = textBrush;
			pdfCellStyle.Borders.Left = pdfPen2;
			pdfCellStyle.Borders.Right = pdfPen2;
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.BackgroundBrush = backgroundBrush;
			pdfCellStyle.TextBrush = textBrush;
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.BackgroundBrush = backgroundBrush;
			pdfCellStyle.TextBrush = textBrush;
		}
	}

	private void ApplyGridTable6Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = pdfPen;
		pdfCellStyle.TextBrush = textBrush;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Bottom = new PdfPen(borderColor);
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				pdfCellStyle.BackgroundBrush = null;
				if (Table.m_bandedRowStyle)
				{
					if (m_currentRowIndex < 0 && !Table.m_headerStyle)
					{
						pdfCellStyle.BackgroundBrush = backgroundBrush;
					}
					else
					{
						pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
					}
				}
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
				if (m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
			else
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		pdfCellStyle.Borders.All = pdfPen;
		if (Table.m_bandedColStyle)
		{
			if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (pdfCellStyle.BackgroundBrush != null)
			{
				pdfCellStyle.Borders.All = pdfPen;
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		pdfCellStyle.Borders.Top = new PdfPen(borderColor);
	}

	private void ApplyGridTable7Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = pdfPen;
		pdfCellStyle.TextBrush = textBrush;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Bottom = new PdfPen(borderColor);
			pdfCellStyle.Borders.All = new PdfPen(Color.FromArgb(255, 255, 255, 255), 0.5f);
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			if (!Table.m_headerStyle || m_currentRowIndex >= 0)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle && (!Table.m_headerStyle || m_currentRowIndex >= 0))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.All = new PdfPen(Color.Empty);
			pdfCellStyle.BackgroundBrush = null;
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
		{
			if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
			{
				if (!Table.m_headerStyle || m_currentRowIndex >= 0)
				{
					pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
				}
				pdfCellStyle.BackgroundBrush = null;
				pdfCellStyle.Borders.All = all;
				if (m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
			else
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
		{
			return;
		}
		if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			if (!Table.m_headerStyle || m_currentRowIndex >= 0)
			{
				pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			}
			if (m_currentRowIndex == 0)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		else
		{
			pdfCellStyle.Borders.Top = pdfPen;
		}
	}

	private void ApplyListTable1Light(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(Color.Empty);
		PdfPen pdfPen2 = new PdfPen(borderColor);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = pdfPen;
		pdfCellStyle.BorderPen = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			if (Table.m_headerStyle && (m_currentCellIndex != 0 || !Table.m_firstColumnStyle) && (m_currentCellIndex != Table.Columns.Count - 1 || !Table.m_lastColumnStyle))
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			pdfCellStyle.Borders.Bottom = pdfPen2;
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
		}
		if (Table.m_bandedRowStyle && Table.m_bandedColStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = pdfPen2;
			if (Table.m_bandedColStyle && (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		if (Table.m_bandedRowStyle)
		{
			if (m_currentRowIndex < 0 && !Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
	}

	private void ApplyListTable2(Color borderColor, Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(Color.Empty);
		PdfPen all = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = all;
		pdfCellStyle.Borders.Left = pdfPen;
		pdfCellStyle.Borders.Right = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		if (Table.m_bandedRowStyle)
		{
			if (m_currentRowIndex < 0 && !Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
	}

	private void ApplyListTable3(Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(backColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		PdfPen all = new PdfPen(Color.Empty);
		pdfCellStyle.Borders.All = all;
		if (m_currentCellIndex == 0)
		{
			pdfCellStyle.Borders.Left = pdfPen;
		}
		else if (m_currentCellIndex == Table.Columns.Count - 1)
		{
			pdfCellStyle.Borders.Right = pdfPen;
		}
		if (Table.Style.ShowHeader)
		{
			if (m_currentRowIndex < 0)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		else if (m_currentRowIndex == 0)
		{
			pdfCellStyle.Borders.Top = pdfPen;
		}
		if (m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Borders.Bottom = pdfPen;
		}
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = pdfPen;
			pdfCellStyle.BackgroundBrush = backgroundBrush;
			pdfCellStyle.TextBrush = textBrush;
		}
		if (Table.m_bandedRowStyle)
		{
			pdfCellStyle.Borders.Top = pdfPen;
		}
		if (Table.m_bandedColStyle)
		{
			pdfCellStyle.Borders.Left = pdfPen;
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = new PdfPen(backColor);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			if (Table.m_headerStyle)
			{
				if (m_currentRowIndex >= 0)
				{
					pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
				}
			}
			else
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		if (Table.m_headerStyle)
		{
			if (m_currentRowIndex >= 0)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
		}
		else
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
	}

	private void ApplyListTable4(Color borderColor, Color headerBackColor, Color bandedRowColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(headerBackColor);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush backgroundBrush2 = new PdfSolidBrush(bandedRowColor);
		pdfCellStyle.Borders.All = all;
		if (m_currentCellIndex == 0)
		{
			pdfCellStyle.Borders.Left = pdfPen;
		}
		else if (m_currentCellIndex == Table.Columns.Count - 1)
		{
			pdfCellStyle.Borders.Right = pdfPen;
		}
		pdfCellStyle.Borders.Top = pdfPen;
		pdfCellStyle.Borders.Bottom = pdfPen;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.All = new PdfPen(headerBackColor, 0.5f);
			pdfCellStyle.BackgroundBrush = backgroundBrush;
			pdfCellStyle.TextBrush = textBrush;
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, bandedRowColor, m_currentCellIndex);
			}
			if (m_currentRowIndex < 0 && Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, bandedRowColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle && (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, bandedRowColor, m_currentCellIndex);
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush2;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, bandedRowColor, m_currentRowIndex);
				}
			}
			if (m_currentRowIndex < 0 && Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = new PdfPen(borderColor);
			if (Table.m_bandedColStyle && (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1))
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, bandedRowColor, m_currentCellIndex);
			}
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			if (Table.m_headerStyle)
			{
				if (m_currentRowIndex >= 0)
				{
					pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
				}
			}
			else
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		if (Table.m_headerStyle)
		{
			if (m_currentRowIndex >= 0)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
		}
		else
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
	}

	private void ApplyListTable5Dark(Color backColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		PdfBrush textBrush = new PdfSolidBrush(Color.FromArgb(255, 255, 255, 255));
		PdfPen pdfPen = new PdfPen(Color.FromArgb(255, 255, 255, 255), 0.5f);
		PdfPen top = new PdfPen(Color.Empty, 0.5f);
		pdfCellStyle.Borders.All = new PdfPen(Color.Empty);
		pdfCellStyle.BackgroundBrush = backgroundBrush;
		pdfCellStyle.TextBrush = textBrush;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Bottom = new PdfPen(Color.FromArgb(255, 255, 255, 255), 2f);
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.Borders.Left = pdfPen;
				pdfCellStyle.Borders.Right = pdfPen;
			}
		}
		if (Table.m_firstColumnStyle && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			if (m_currentCellIndex == 0)
			{
				pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			}
			else if (m_currentCellIndex == 1)
			{
				pdfCellStyle.Borders.Left = pdfPen;
			}
		}
		if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Left = pdfPen;
		}
		if (Table.m_bandedRowStyle)
		{
			pdfCellStyle.Borders.Top = pdfPen;
			pdfCellStyle.Borders.Bottom = pdfPen;
		}
		if (Table.m_bandedColStyle)
		{
			pdfCellStyle.Borders.Left = pdfPen;
			pdfCellStyle.Borders.Right = pdfPen;
		}
		if (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1)
		{
			return;
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		pdfCellStyle.Borders.Top = pdfPen;
		if (!Table.m_headerStyle)
		{
			if (Table.m_firstColumnStyle && m_currentCellIndex == 0)
			{
				pdfCellStyle.Borders.Top = top;
			}
			else if (Table.m_lastColumnStyle && m_currentCellIndex == Table.Columns.Count - 1)
			{
				pdfCellStyle.Borders.Top = top;
			}
		}
	}

	private void ApplyListTable6Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen all = new PdfPen(Color.Empty, 0.5f);
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = all;
		pdfCellStyle.TextBrush = textBrush;
		if (m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Borders.Bottom = pdfPen;
		}
		if (Table.Style.ShowHeader)
		{
			if (m_currentRowIndex < 0)
			{
				pdfCellStyle.Borders.Top = pdfPen;
			}
		}
		else if (m_currentRowIndex == 0)
		{
			pdfCellStyle.Borders.Top = pdfPen;
		}
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Bottom = pdfPen;
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
		}
		if (Table.m_bandedRowStyle && Table.m_bandedColStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				if (pdfCellStyle.BackgroundBrush != null && m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null && m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			if (Table.m_bandedColStyle)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			}
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Top = new PdfPen(borderColor);
		}
		if (Table.m_firstColumnStyle && m_currentCellIndex == 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1))
		{
			pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
		}
		if (!Table.m_lastColumnStyle || m_currentCellIndex != Table.Columns.Count - 1 || (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1))
		{
			return;
		}
		pdfCellStyle.BackgroundBrush = null;
		if (Table.m_bandedRowStyle)
		{
			if (m_currentRowIndex < 0 && !Table.m_headerStyle)
			{
				pdfCellStyle.BackgroundBrush = backgroundBrush;
			}
			else
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		pdfCellStyle.Font = ChangeFontStyle(Table.Style.DefaultStyle.Font);
	}

	private void ApplyListTable7Colorful(Color borderColor, Color backColor, Color textColor)
	{
		PdfCellStyle pdfCellStyle = new PdfCellStyle();
		pdfCellStyle.Borders = PdfBorders.Default;
		Table.Style.DefaultStyle = pdfCellStyle;
		PdfPen all = new PdfPen(Color.Empty);
		PdfBrush textBrush = new PdfSolidBrush(textColor);
		PdfPen pdfPen = new PdfPen(borderColor, 0.5f);
		PdfBrush backgroundBrush = new PdfSolidBrush(backColor);
		pdfCellStyle.Borders.All = all;
		pdfCellStyle.TextBrush = textBrush;
		if (m_currentRowIndex < 0 && Table.m_headerStyle)
		{
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Bottom = pdfPen;
		}
		if (Table.m_bandedColStyle && Table.m_bandedRowStyle)
		{
			pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
			if (pdfCellStyle.BackgroundBrush == null)
			{
				pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
			}
		}
		else
		{
			if (Table.m_bandedColStyle)
			{
				if (!Table.m_headerStyle || m_currentRowIndex >= 0)
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedColStyle(Table.m_firstColumnStyle, backColor, m_currentCellIndex);
				}
				if (pdfCellStyle.BackgroundBrush != null)
				{
					if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
					{
						pdfCellStyle.Borders.Left = pdfPen;
					}
					else
					{
						pdfCellStyle.Borders.All = all;
					}
					if (Table.m_headerStyle && m_currentRowIndex == 0)
					{
						pdfCellStyle.Borders.Top = pdfPen;
					}
				}
			}
			if (Table.m_bandedRowStyle)
			{
				if (m_currentRowIndex < 0 && !Table.m_headerStyle)
				{
					pdfCellStyle.BackgroundBrush = backgroundBrush;
				}
				else
				{
					pdfCellStyle.BackgroundBrush = ApplyBandedRowStyle(Table.m_headerStyle, backColor, m_currentRowIndex);
				}
				if (Table.m_firstColumnStyle && m_currentCellIndex == 1)
				{
					pdfCellStyle.Borders.Left = pdfPen;
				}
				else
				{
					pdfCellStyle.Borders.All = all;
				}
				if (Table.m_headerStyle && m_currentRowIndex == 0)
				{
					pdfCellStyle.Borders.Top = pdfPen;
				}
			}
		}
		if (Table.m_firstColumnStyle && m_currentRowIndex >= 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1) && m_currentCellIndex == 0)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Right = pdfPen;
		}
		if (Table.m_totalRowStyle && m_currentRowIndex == Table.Rows.Count - 1)
		{
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Borders.All = all;
			pdfCellStyle.Borders.Top = pdfPen;
		}
		if (Table.m_lastColumnStyle && m_currentRowIndex >= 0 && (!Table.m_totalRowStyle || m_currentRowIndex != Table.Rows.Count - 1) && m_currentCellIndex == Table.Columns.Count - 1)
		{
			pdfCellStyle.BackgroundBrush = null;
			pdfCellStyle.Font = CreateItalicFont(Table.Style.DefaultStyle.Font);
			pdfCellStyle.Borders.Left = pdfPen;
		}
	}
}
