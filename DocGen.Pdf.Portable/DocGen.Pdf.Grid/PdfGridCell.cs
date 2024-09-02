using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Grid;

public class PdfGridCell
{
	private float m_width = float.MinValue;

	private float m_height = float.MinValue;

	private int m_rowSpan;

	private int m_colSpan;

	private PdfGridRow m_row;

	private PdfGridCellStyle m_style;

	private object m_value;

	private PdfStringFormat m_format;

	private bool m_bIsCellMergeStart;

	private bool m_bIsCellMergeContinue;

	private bool m_bIsRowMergeStart;

	private bool m_bIsRowMergeContinue;

	private bool m_finsh = true;

	private string m_remainingString;

	internal bool present;

	private PdfGridCell m_parent;

	private float rowSpanRemainingHeight;

	private bool isHtmlText;

	internal int m_pageCount;

	private bool m_isImageDrawn;

	private float m_outerCellWidth = float.MinValue;

	private PdfGridImagePosition m_imagePosition = PdfGridImagePosition.Stretch;

	private PdfGridStretchOption m_pdfGridStretchOption = PdfGridStretchOption.None;

	internal float m_rowSpanRemainingHeight;

	internal RectangleF layoutBounds;

	internal PdfLayoutResult layoutResult;

	internal float tempHeight;

	private PdfTag m_tag;

	internal bool cellBorderCuttOffX;

	internal bool cellBorderCuttOffY;

	internal RectangleF parentLayoutBounds;

	internal bool m_isCellHeightSet;

	internal PdfGridLayouter pdfGridLayouter;

	internal bool m_skipCellValue;

	public float Width
	{
		get
		{
			if (m_width == float.MinValue || Row.Grid.isComplete)
			{
				m_width = MeasureWidth();
			}
			return (float)Math.Round(m_width, 4);
		}
		internal set
		{
			m_width = value;
		}
	}

	public float Height
	{
		get
		{
			if (m_height == float.MinValue || !m_isCellHeightSet)
			{
				m_height = MeasureHeight();
				m_isCellHeightSet = true;
			}
			return m_height;
		}
		internal set
		{
			m_height = value;
			m_isCellHeightSet = true;
		}
	}

	public int RowSpan
	{
		get
		{
			return m_rowSpan;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Invalid span specified, must be greater than or equal to 1");
			}
			if (value > 1)
			{
				m_rowSpan = value;
				Row.RowSpanExists = true;
				Row.Grid.m_hasRowSpanSpan = true;
			}
		}
	}

	public int ColumnSpan
	{
		get
		{
			return m_colSpan;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Invalid span specified, must be greater than or equal to 1");
			}
			if (value > 1)
			{
				m_colSpan = value;
				Row.ColumnSpanExists = true;
				Row.Grid.m_hasColumnSpan = true;
			}
		}
	}

	public PdfGridCellStyle Style
	{
		get
		{
			if (m_style == null)
			{
				m_style = new PdfGridCellStyle();
			}
			return m_style;
		}
		set
		{
			m_style = value;
		}
	}

	public bool IsHtmlText
	{
		get
		{
			return isHtmlText;
		}
		set
		{
			isHtmlText = value;
			Row.Grid.m_hasHTMLText = value;
		}
	}

	public object Value
	{
		get
		{
			if (m_value != null && IsHtmlText && !string.IsNullOrEmpty(m_value.ToString()) && !(m_value is PdfHTMLTextElement))
			{
				PdfFont font = ((m_style.Font == null) ? PdfDocument.DefaultFont : m_style.Font);
				PdfBrush brush = ((m_style.TextBrush == null) ? PdfBrushes.Black : m_style.TextBrush);
				PdfHTMLTextElement value = new PdfHTMLTextElement(m_value.ToString(), font, brush);
				m_value = value;
			}
			return m_value;
		}
		set
		{
			m_value = value;
			if (!(m_value is PdfGrid))
			{
				return;
			}
			Row.Grid.isSignleGrid = false;
			m_isCellHeightSet = false;
			PdfGrid obj = m_value as PdfGrid;
			obj.ParentCell = this;
			(m_value as PdfGrid).IsChildGrid = true;
			foreach (PdfGridRow row in obj.Rows)
			{
				foreach (PdfGridCell cell in row.Cells)
				{
					cell.m_parent = this;
				}
			}
		}
	}

	public PdfStringFormat StringFormat
	{
		get
		{
			if (m_format == null)
			{
				m_format = new PdfStringFormat();
			}
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	internal PdfGridRow Row
	{
		get
		{
			return m_row;
		}
		set
		{
			m_row = value;
		}
	}

	internal bool IsCellMergeContinue
	{
		get
		{
			return m_bIsCellMergeContinue;
		}
		set
		{
			m_bIsCellMergeContinue = value;
		}
	}

	internal bool IsCellMergeStart
	{
		get
		{
			return m_bIsCellMergeStart;
		}
		set
		{
			m_bIsCellMergeStart = value;
		}
	}

	internal bool IsRowMergeStart
	{
		get
		{
			return m_bIsRowMergeStart;
		}
		set
		{
			m_bIsRowMergeStart = value;
		}
	}

	internal bool IsRowMergeContinue
	{
		get
		{
			return m_bIsRowMergeContinue;
		}
		set
		{
			m_bIsRowMergeContinue = value;
		}
	}

	internal PdfGridCell NextCell => ObtainNextCell();

	internal string RemainingString
	{
		get
		{
			return m_remainingString;
		}
		set
		{
			m_remainingString = value;
		}
	}

	internal bool FinishedDrawingCell
	{
		get
		{
			return m_finsh;
		}
		set
		{
			m_finsh = value;
		}
	}

	public PdfGridImagePosition ImagePosition
	{
		get
		{
			return m_imagePosition;
		}
		set
		{
			m_imagePosition = value;
		}
	}

	internal PdfGridStretchOption StretchOption
	{
		get
		{
			return m_pdfGridStretchOption;
		}
		set
		{
			m_pdfGridStretchOption = value;
		}
	}

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public PdfGridCell()
	{
		m_rowSpan = 1;
		m_colSpan = 1;
	}

	public PdfGridCell(PdfGridRow row)
		: this()
	{
		m_row = row;
	}

	internal PdfStringLayoutResult Draw(PdfGraphics graphics, RectangleF bounds, bool cancelSubsequentSpans)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetName("O", "Table");
		pdfDictionary.SetNumber("RowSpan", RowSpan);
		pdfDictionary.SetNumber("ColSpan", ColumnSpan);
		graphics.TableSpan = pdfDictionary;
		bool flag = false;
		if (!Row.Grid.isSignleGrid)
		{
			if (m_remainingString != null || PdfGridLayouter.m_repeatRowIndex != -1)
			{
				DrawParentCells(graphics, bounds, b: true);
			}
			else if (Row.Grid.Rows.Count > 1)
			{
				for (int i = 0; i < Row.Grid.Rows.Count; i++)
				{
					if (Row == Row.Grid.Rows[i])
					{
						if (Row.Grid.Rows[i].RowBreakHeight > 0f)
						{
							flag = true;
						}
						if (i > 0 && flag)
						{
							DrawParentCells(graphics, bounds, b: false);
						}
					}
				}
			}
		}
		PdfStringLayoutResult pdfStringLayoutResult = null;
		if (cancelSubsequentSpans)
		{
			int num = Row.Cells.IndexOf(this);
			for (int j = num + 1; j <= num + m_colSpan; j++)
			{
				if (j < Row.Cells.Count)
				{
					Row.Cells[j].IsCellMergeContinue = false;
					Row.Cells[j].IsRowMergeContinue = false;
				}
			}
			m_colSpan = 1;
		}
		if (m_bIsCellMergeContinue || m_bIsRowMergeContinue)
		{
			if (!m_bIsCellMergeContinue || !Row.Grid.Style.AllowHorizontalOverflow)
			{
				return pdfStringLayoutResult;
			}
			if ((Row.RowOverflowIndex > 0 && Row.Cells.IndexOf(this) != Row.RowOverflowIndex + 1) || (Row.RowOverflowIndex == 0 && m_bIsCellMergeContinue))
			{
				return pdfStringLayoutResult;
			}
		}
		if (this.pdfGridLayouter != null && this.pdfGridLayouter.m_currentDrawingRowHeignt != bounds.Height && this.pdfGridLayouter.previousRowPendingRowSpan <= 0)
		{
			bounds = AdjustOuterLayoutArea(bounds, graphics);
		}
		DrawCellBackground(ref graphics, bounds);
		PdfPen textPen = GetTextPen();
		PdfBrush textBrush = GetTextBrush();
		PdfFont textFont = GetTextFont();
		PdfStringFormat format = ObtainStringFormat();
		graphics.Tag = PdfTag;
		RectangleF bounds2 = bounds;
		if (bounds2.Height >= graphics.ClientSize.Height)
		{
			if (Row.Grid.AllowRowBreakAcrossPages)
			{
				bounds2.Height -= bounds2.Y;
				bounds.Height -= bounds.Y;
				if (Row.Grid.IsChildGrid)
				{
					bounds2.Height -= Row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
				}
			}
			else
			{
				bounds2.Height = graphics.ClientSize.Height;
				bounds.Height = graphics.ClientSize.Height;
			}
		}
		if (Value is PdfGrid && cellBorderCuttOffX)
		{
			bounds2.Width -= Style.Borders.Left.Width / 2f;
			if (Row.Grid.ParentCell == null && Row.Grid.Columns.Count == 1 && Row.Grid.Columns[0].isCustomWidth)
			{
				bounds.Width -= Style.Borders.Left.Width / 2f;
			}
		}
		if (Value is PdfGrid && cellBorderCuttOffY)
		{
			bounds2.Height -= Style.Borders.Top.Width / 2f;
		}
		bounds2 = AdjustContentLayoutArea(bounds2);
		if (Value is PdfGrid)
		{
			graphics.Save();
			graphics.SetClip(new RectangleF(bounds2.X, bounds2.Y, bounds.Width, bounds.Height));
			_ = (Value as PdfGrid).Size.Width;
			_ = bounds2.Size.Width;
			if (PdfCatalog.StructTreeRoot != null)
			{
				PdfCatalog.StructTreeRoot.m_isChildGrid = true;
			}
			PdfGrid pdfGrid = Value as PdfGrid;
			pdfGrid.IsChildGrid = true;
			pdfGrid.ParentCell = this;
			pdfGrid.m_listOfNavigatePages = new List<int>();
			PdfGridLayouter pdfGridLayouter = new PdfGridLayouter(pdfGrid);
			pdfGridLayouter.cellInnerGrid = true;
			PdfLayoutFormat pdfLayoutFormat = new PdfGridLayoutFormat();
			if (Row.Grid.LayoutFormat != null)
			{
				pdfLayoutFormat = Row.Grid.LayoutFormat;
			}
			else
			{
				pdfLayoutFormat.Layout = PdfLayoutType.Paginate;
			}
			if (graphics.Layer != null)
			{
				PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
				pdfLayoutParams.Page = graphics.Page as PdfPage;
				pdfLayoutParams.m_graphics = graphics;
				pdfGrid.ParentCell.parentLayoutBounds = bounds;
				pdfLayoutParams.Bounds = bounds2;
				pdfLayoutParams.Format = pdfLayoutFormat;
				pdfGrid.SetSpan();
				PdfLayoutResult pdfLayoutResult = pdfGridLayouter.Layout(pdfLayoutParams);
				pdfGridLayouter.cellInnerGrid = false;
				Value = pdfGrid;
				if (pdfLayoutParams.Page != pdfLayoutResult.Page)
				{
					Row.NestedGridLayoutResult = pdfLayoutResult;
					bounds.Height = graphics.ClientSize.Height - bounds.Y;
				}
			}
			else
			{
				pdfGrid.SetSpan();
				pdfGridLayouter = new PdfGridLayouter(Value as PdfGrid);
				pdfGridLayouter.Layout(graphics, bounds2);
			}
			graphics.Restore();
		}
		else if (Value is PdfTextElement)
		{
			PdfTextElement pdfTextElement = Value as PdfTextElement;
			PdfPage page = graphics.Page as PdfPage;
			pdfTextElement.ispdfTextElement = true;
			string text = pdfTextElement.Text;
			PdfTextLayoutResult pdfTextLayoutResult;
			if (m_finsh)
			{
				pdfTextLayoutResult = pdfTextElement.Draw(page, bounds2) as PdfTextLayoutResult;
			}
			else
			{
				pdfTextElement.Text = RemainingString;
				pdfTextLayoutResult = pdfTextElement.Draw(page, bounds2) as PdfTextLayoutResult;
			}
			if (pdfTextLayoutResult.Remainder != null && pdfTextLayoutResult.Remainder != string.Empty)
			{
				RemainingString = pdfTextLayoutResult.Remainder;
				FinishedDrawingCell = false;
			}
			else if (!(Row.RowBreakHeight > 0f))
			{
				RemainingString = null;
				FinishedDrawingCell = true;
			}
			else
			{
				RemainingString = string.Empty;
			}
			pdfTextElement.Text = text;
		}
		else if (Value is PdfDocumentLinkAnnotation)
		{
			PdfPage obj = graphics.Page as PdfPage;
			PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation = Value as PdfDocumentLinkAnnotation;
			pdfDocumentLinkAnnotation.Bounds = bounds;
			obj.Annotations.Add(pdfDocumentLinkAnnotation);
			Value = pdfDocumentLinkAnnotation.Text;
			graphics.DrawString(Value.ToString(), textFont, textBrush, bounds2, format);
		}
		else if (Value is PdfUriAnnotation)
		{
			PdfPage obj2 = graphics.Page as PdfPage;
			PdfUriAnnotation pdfUriAnnotation = Value as PdfUriAnnotation;
			_ = Row.Grid.LastRow;
			RectangleF bounds3 = bounds;
			if (Row.Grid.LastRow.RowIndex == 0)
			{
				bounds3.Y += pdfUriAnnotation.Bounds.Y;
				pdfUriAnnotation.Bounds = bounds3;
			}
			else
			{
				pdfUriAnnotation.Bounds = bounds;
			}
			obj2.Annotations.Add(pdfUriAnnotation);
			Value = pdfUriAnnotation.Text;
			graphics.DrawString(Value.ToString(), textFont, textBrush, bounds2, format);
		}
		else if (Value is PdfFileLinkAnnotation)
		{
			PdfPage obj3 = graphics.Page as PdfPage;
			PdfFileLinkAnnotation pdfFileLinkAnnotation = Value as PdfFileLinkAnnotation;
			pdfFileLinkAnnotation.Bounds = bounds;
			obj3.Annotations.Add(pdfFileLinkAnnotation);
			Value = pdfFileLinkAnnotation.Text;
			graphics.DrawString(Value.ToString(), textFont, textBrush, bounds2, format);
		}
		else if (Value is PdfHTMLTextElement)
		{
			PdfPage page2 = graphics.Page as PdfPage;
			PdfLayoutFormat pdfLayoutFormat2 = new PdfLayoutFormat();
			pdfLayoutFormat2.Break = PdfLayoutBreakType.FitPage;
			pdfLayoutFormat2.PaginateBounds = this.pdfGridLayouter.pdfLayoutParams.Format.PaginateBounds;
			PdfTextLayoutResult pdfTextLayoutResult2 = null;
			PdfHTMLTextElement pdfHTMLTextElement = Value as PdfHTMLTextElement;
			pdfHTMLTextElement.m_bottomCellpadding = ((Style.CellPadding == null) ? m_row.Grid.Style.CellPadding.Bottom : Style.CellPadding.Bottom);
			if (layoutResult == null)
			{
				pdfHTMLTextElement.m_isPdfGrid = false;
				pdfHTMLTextElement.shapeBounds = RectangleF.Empty;
				pdfHTMLTextElement.m_isPdfGrid = true;
				layoutResult = pdfHTMLTextElement.Draw(page2, bounds2.Location, bounds2.Width, pdfLayoutFormat2);
				if (layoutResult != null)
				{
					tempHeight = layoutResult.Bounds.Height;
					pdfTextLayoutResult2 = layoutResult as PdfTextLayoutResult;
					if (pdfTextLayoutResult2 != null && pdfTextLayoutResult2.Remainder == null && tempHeight > 0f && Row != null && Row.RowBreakHeight > 0f)
					{
						Value = string.Empty;
						RemainingString = string.Empty;
						this.pdfGridLayouter.currentHtmlLayoutResult = pdfTextLayoutResult2;
					}
				}
			}
			else
			{
				if (!Row.IsHeaderRow || (Row.IsHeaderRow && pdfHTMLTextElement.Height > graphics.ClientSize.Height))
				{
					pdfHTMLTextElement.shapeBounds = new RectangleF(layoutResult.Bounds.X, tempHeight, 0f, 0f);
				}
				layoutResult = pdfHTMLTextElement.Draw(page2, bounds2.Location, bounds2.Width, pdfLayoutFormat2);
				tempHeight += layoutResult.Bounds.Height;
			}
			if (pdfHTMLTextElement.Height == tempHeight || (pdfTextLayoutResult2 != null && pdfTextLayoutResult2.Remainder == null && tempHeight > 0f))
			{
				FinishedDrawingCell = true;
			}
			else
			{
				FinishedDrawingCell = false;
			}
		}
		else if (Value is string || m_remainingString != null)
		{
			if ((decimal)bounds2.Height <= -1m)
			{
				bounds2.Y = graphics.ClientSize.Height - bounds.Height - bounds2.Height;
			}
			RectangleF layoutRectangle = ((!((decimal)bounds2.Height < (decimal)textFont.Height)) ? bounds2 : new RectangleF(bounds2.X, bounds2.Y, bounds2.Width, textFont.Height));
			if (bounds2.Height < textFont.Height && Row.Grid.IsChildGrid && Row.Grid.ParentCell != null)
			{
				float num2 = layoutRectangle.Height - Row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom - Row.Grid.Style.CellPadding.Bottom;
				if (num2 > 0f && num2 < textFont.Height)
				{
					layoutRectangle.Height = num2;
				}
				else if (num2 + Row.Grid.Style.CellPadding.Bottom > 0f && num2 + Row.Grid.Style.CellPadding.Bottom < textFont.Height)
				{
					layoutRectangle.Height = num2 + Row.Grid.Style.CellPadding.Bottom;
				}
				else if (bounds.Height < textFont.Height)
				{
					layoutRectangle.Height = bounds.Height;
				}
				else if (bounds.Height - Row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom < textFont.Height)
				{
					layoutRectangle.Height = bounds.Height - Row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
				}
			}
			if (Row.Grid.AllowRowBreakAcrossPages && Row.Grid.IsChildGrid && Row.RowBreakHeight > 0f)
			{
				layoutRectangle.Height -= Row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom / 2f;
			}
			if (Style.CellPadding != null && Style.CellPadding.Bottom == 0f && Style.CellPadding.Left == 0f && Style.CellPadding.Right == 0f && Style.CellPadding.Top == 0f)
			{
				layoutRectangle.Width -= Style.Borders.Left.Width + Style.Borders.Right.Width;
			}
			if (this.pdfGridLayouter != null && this.pdfGridLayouter.drawFinalRow && graphics.ClientSize.Height < layoutRectangle.Height)
			{
				layoutRectangle.Height = this.pdfGridLayouter.endPageHeight;
				bounds.Height = layoutRectangle.Height + 10f;
			}
			if (m_skipCellValue)
			{
				Value = string.Empty;
			}
			if (m_finsh)
			{
				string s = ((m_remainingString == string.Empty) ? m_remainingString : ((string)Value));
				graphics.DrawString(s, textFont, textPen, textBrush, layoutRectangle, format);
			}
			else
			{
				graphics.DrawString(m_remainingString, textFont, textPen, textBrush, layoutRectangle, format);
			}
			pdfStringLayoutResult = graphics.StringLayoutResult;
			if (this.pdfGridLayouter != null && Row.RowBreakHeight - graphics.ClientSize.Height < 0f && pdfStringLayoutResult != null && pdfStringLayoutResult.Remainder != null)
			{
				SizeF sizeF = textFont.MeasureString(pdfStringLayoutResult.Remainder, layoutRectangle.Width);
				if (sizeF.Height >= graphics.ClientSize.Height)
				{
					this.pdfGridLayouter.paginateWithRowBreak = true;
				}
				else if (sizeF.Height <= graphics.ClientSize.Height)
				{
					this.pdfGridLayouter.paginateWithoutRowBreak = true;
				}
				else if (this.pdfGridLayouter.currentRowHeight < 0f)
				{
					this.pdfGridLayouter.drawFinalRow = true;
					this.pdfGridLayouter.endPageHeight = sizeF.Height;
				}
			}
			if (pdfStringLayoutResult != null && Row.Grid.IsChildGrid && Row.RowBreakHeight > 0f)
			{
				bounds.Height -= Row.Grid.ParentCell.Row.Grid.Style.CellPadding.Bottom;
			}
		}
		else if (m_value is PdfImage && m_imagePosition == PdfGridImagePosition.Stretch)
		{
			PdfPaddings pdfPaddings = null;
			if (Style.CellPadding != null && Style.CellPadding != new PdfPaddings())
			{
				pdfPaddings = Style.CellPadding;
				bounds = new RectangleF(bounds.X + pdfPaddings.Left, bounds.Y + pdfPaddings.Top, bounds.Width - (pdfPaddings.Left + pdfPaddings.Right), bounds.Height - (pdfPaddings.Top + pdfPaddings.Bottom));
			}
			else if (Row.Grid.Style.CellPadding != null && Row.Grid.Style.CellPadding != new PdfPaddings())
			{
				pdfPaddings = Row.Grid.Style.CellPadding;
				bounds = new RectangleF(bounds.X + pdfPaddings.Left, bounds.Y + pdfPaddings.Top, bounds.Width - (pdfPaddings.Left + pdfPaddings.Right), bounds.Height - (pdfPaddings.Top + pdfPaddings.Bottom));
			}
			graphics.IsTemplateGraphics = false;
			PdfImage pdfImage = m_value as PdfImage;
			float num3 = pdfImage.Width;
			float num4 = pdfImage.Height;
			float num5 = 0f;
			if (m_pdfGridStretchOption == PdfGridStretchOption.Uniform || m_pdfGridStretchOption == PdfGridStretchOption.UniformToFill)
			{
				float num6 = 1f;
				if (num3 > bounds.Width)
				{
					num6 = num3 / bounds.Width;
					num3 = bounds.Width;
					num4 /= num6;
				}
				if (num4 > bounds.Height)
				{
					num6 = num4 / bounds.Height;
					num4 = bounds.Height;
					num3 /= num6;
				}
				if (num3 < bounds.Width && num4 < bounds.Height)
				{
					float num7 = bounds.Width - num3;
					num5 = bounds.Height - num4;
					if (num7 < num5)
					{
						num6 = num3 / bounds.Width;
						num3 = bounds.Width;
						num4 /= num6;
					}
					else
					{
						num6 = num4 / bounds.Height;
						num4 = bounds.Height;
						num3 /= num6;
					}
				}
			}
			if (m_pdfGridStretchOption == PdfGridStretchOption.Fill || m_pdfGridStretchOption == PdfGridStretchOption.None)
			{
				num3 = bounds.Width;
				num4 = bounds.Height;
			}
			if (m_pdfGridStretchOption == PdfGridStretchOption.UniformToFill)
			{
				float num8 = 1f;
				if (num3 == bounds.Width && num4 < bounds.Height)
				{
					num8 = num4 / bounds.Height;
					num4 = bounds.Height;
					num3 /= num8;
				}
				if (num4 == bounds.Height && num3 < bounds.Width)
				{
					num8 = num3 / bounds.Width;
					num3 = bounds.Width;
					num4 /= num8;
				}
				PdfImage image = pdfImage.Clone();
				PdfPage obj4 = graphics.Page as PdfPage;
				PdfGraphicsState state = obj4.Graphics.Save();
				obj4.Graphics.SetClip(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height));
				obj4.Graphics.DrawImage(image, bounds.X, bounds.Y, num3, num4);
				obj4.Graphics.Restore(state);
			}
			else
			{
				float x = bounds.X;
				float y = bounds.Y;
				graphics.DrawImage(pdfImage, x, y, num3, num4);
				bounds = new RectangleF(bounds.X - pdfPaddings.Left, bounds.Y - pdfPaddings.Top, bounds.Width + (pdfPaddings.Left + pdfPaddings.Right), bounds.Height + (pdfPaddings.Top + pdfPaddings.Bottom));
			}
			m_isImageDrawn = true;
		}
		if (Style.Borders != null && Style.Borders.Left != null)
		{
			if (graphics.Tag != null)
			{
				graphics.Tag = new PdfArtifact();
			}
			DrawCellBorders(ref graphics, bounds);
			layoutBounds = bounds;
		}
		graphics.Tag = null;
		return pdfStringLayoutResult;
	}

	private void DrawParentCells(PdfGraphics graphics, RectangleF bounds, bool b)
	{
		PointF location = default(PointF);
		if (bounds.Height < graphics.ClientSize.Height && b)
		{
			bounds.Height += bounds.Y - location.Y;
		}
		RectangleF bounds2 = new RectangleF(location, new SizeF(bounds.Width, bounds.Height));
		if (!b)
		{
			bounds2.Y = bounds.Y;
			bounds2.Height = bounds.Height;
		}
		PdfGridCellStyle style = Style.Clone() as PdfGridCellStyle;
		MemberwiseClone();
		PdfGridCell pdfGridCell = this;
		if (m_parent != null)
		{
			if (pdfGridCell.Row.Grid.Rows.Count == 1 && pdfGridCell.Row.Grid.Rows[0].Cells.Count == 1)
			{
				pdfGridCell.Row.Grid.Rows[0].Cells[0].present = true;
			}
			else
			{
				foreach (PdfGridRow row in pdfGridCell.Row.Grid.Rows)
				{
					if (row != pdfGridCell.Row)
					{
						continue;
					}
					foreach (PdfGridCell cell in Row.Cells)
					{
						if (cell == pdfGridCell)
						{
							cell.present = true;
							break;
						}
					}
				}
			}
			while (pdfGridCell.m_parent != null)
			{
				pdfGridCell = pdfGridCell.m_parent;
				pdfGridCell.present = true;
				bounds2.X += pdfGridCell.Row.Grid.Style.CellPadding.Left;
				if (pdfGridCell.Row.Cells.Count > 0)
				{
					bounds2.X += pdfGridCell.Row.Cells[0].Style.Borders.Left.Width;
				}
			}
		}
		if (bounds.X >= bounds2.X)
		{
			bounds2.X -= bounds.X;
			if (bounds2.X < 0f)
			{
				bounds2.X = bounds.X;
			}
		}
		PdfGrid pdfGrid = pdfGridCell.Row.Grid;
		for (int i = 0; i < pdfGrid.Rows.Count; i++)
		{
			for (int j = 0; j < pdfGrid.Rows[i].Cells.Count; j++)
			{
				if (!pdfGrid.Rows[i].Cells[j].present)
				{
					continue;
				}
				int num = 0;
				if (pdfGrid.Rows[i].Style.BackgroundBrush != null)
				{
					Style.BackgroundBrush = pdfGrid.Rows[i].Style.BackgroundBrush;
					float num2 = 0f;
					if (j > 0)
					{
						for (int k = 0; k < j; k++)
						{
							num2 += pdfGrid.Columns[k].Width;
						}
					}
					bounds2.Width = pdfGrid.Rows[i].Width - num2;
					if (pdfGrid.Rows[i].Cells[j].Value is PdfGrid pdfGrid2)
					{
						for (int l = 0; l < pdfGrid2.Rows.Count; l++)
						{
							for (int m = 0; m < pdfGrid2.Rows[l].Cells.Count; m++)
							{
								if (pdfGrid2.Rows[l].Cells[m].present && m > 0)
								{
									bounds2.Width = pdfGrid2.Rows[l].Cells[m].Width;
									num = m;
								}
							}
						}
					}
					DrawCellBackground(ref graphics, bounds2);
				}
				pdfGrid.Rows[i].Cells[j].present = false;
				if (pdfGrid.Rows[i].Cells[j].Style.BackgroundBrush != null)
				{
					Style.BackgroundBrush = pdfGrid.Rows[i].Cells[j].Style.BackgroundBrush;
					if (num == 0)
					{
						bounds2.Width = pdfGrid.Columns[j].Width;
					}
					if (bounds2.X == 0f && pdfGrid.Rows[i].Cells[j].Style != null && pdfGrid.Rows[i].Cells[j].Style.Borders != null && pdfGrid.Rows[i].Cells[j].Style.Borders.Left != null)
					{
						bounds2.X = pdfGrid.Rows[i].Cells[j].Style.Borders.Left.Width / 2f;
						bounds2.Width -= pdfGrid.Rows[i].Cells[j].Style.Borders.Left.Width / 2f;
					}
					if (bounds2.Y == 0f && pdfGrid.Rows[i].Cells[j].Style != null && pdfGrid.Rows[i].Cells[j].Style.Borders != null && pdfGrid.Rows[i].Cells[j].Style.Borders.Top != null)
					{
						bounds2.Y = pdfGrid.Rows[i].Cells[j].Style.Borders.Top.Width / 2f;
						if (pdfGrid.Rows[i].m_paginatedGridRow)
						{
							bounds2.Height += pdfGrid.Rows[i].Cells[j].Style.Borders.Top.Width / 2f;
						}
						else
						{
							bounds2.Height -= pdfGrid.Rows[i].Cells[j].Style.Borders.Top.Width / 2f;
						}
					}
					DrawCellBackground(ref graphics, bounds2);
				}
				if (!(pdfGrid.Rows[i].Cells[j].Value is PdfGrid))
				{
					continue;
				}
				if (pdfGrid.Style != null && !pdfGrid.Rows[i].isrowFinish && num == 0)
				{
					bounds2.X += pdfGrid.Style.CellPadding.Left;
				}
				pdfGrid = pdfGrid.Rows[i].Cells[j].Value as PdfGrid;
				if (pdfGrid.Style.BackgroundBrush != null)
				{
					Style.BackgroundBrush = pdfGrid.Style.BackgroundBrush;
					if (num == 0 && j < pdfGrid.Columns.Count)
					{
						bounds2.Width = pdfGrid.Columns[j].Width;
					}
					DrawCellBackground(ref graphics, bounds2);
				}
				i = -1;
				break;
			}
		}
		if (bounds.Height < graphics.ClientSize.Height)
		{
			bounds.Height -= bounds.Y - location.Y;
		}
		Style = style;
	}

	internal PdfGridCell Clone(PdfGridCell gridcell)
	{
		return (PdfGridCell)gridcell.MemberwiseClone();
	}

	private float MeasureWidth()
	{
		float num = 0f;
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		if (Value is string)
		{
			float num2 = float.MaxValue;
			if (m_parent != null)
			{
				num2 = GetColumnWidth();
				if (Row.Grid.LayoutFormat == null)
				{
					if (Style.CellPadding != null)
					{
						num2 -= Style.CellPadding.Left + Style.CellPadding.Right;
					}
					num2 -= Row.Grid.Style.CellSpacing;
				}
			}
			PdfStringLayoutResult pdfStringLayoutResult = pdfStringLayouter.Layout((string)Value, GetTextFont(), StringFormat, new SizeF(num2, float.MaxValue));
			num += pdfStringLayoutResult.ActualSize.Width;
			if (Style.Borders != null && Style.Borders.Left != null && Style.Borders.Right != null)
			{
				num += (Style.Borders.Left.Width + Style.Borders.Right.Width) * 2f;
			}
		}
		else if (Value is PdfGrid)
		{
			num = (Value as PdfGrid).Size.Width;
		}
		else if (Value is PdfTextElement)
		{
			float width = float.MaxValue;
			if (m_parent != null)
			{
				width = GetColumnWidth();
			}
			PdfTextElement pdfTextElement = Value as PdfTextElement;
			string text = pdfTextElement.Text;
			if (!m_finsh)
			{
				text = ((!string.IsNullOrEmpty(m_remainingString)) ? m_remainingString : ((string)Value));
			}
			PdfStringLayoutResult pdfStringLayoutResult2 = pdfStringLayouter.Layout(text, pdfTextElement.Font ?? GetTextFont(), pdfTextElement.StringFormat ?? StringFormat, new SizeF(width, float.MaxValue));
			num += pdfStringLayoutResult2.ActualSize.Width;
			num += (Style.Borders.Left.Width + Style.Borders.Right.Width) * 2f;
		}
		num = ((Style.CellPadding == null) ? (num + (Row.Grid.Style.CellPadding.Left + Row.Grid.Style.CellPadding.Right)) : (num + (Style.CellPadding.Left + Style.CellPadding.Right)));
		return num + Row.Grid.Style.CellSpacing;
	}

	internal float MeasureHeight()
	{
		float num = CalculateWidth();
		if (Style.CellPadding == null)
		{
			num -= m_row.Grid.Style.CellPadding.Right + m_row.Grid.Style.CellPadding.Left;
		}
		else
		{
			num -= Style.CellPadding.Right + Style.CellPadding.Left;
			num -= Style.Borders.Left.Width + Style.Borders.Right.Width;
		}
		m_outerCellWidth = num;
		float num2 = 0f;
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		if (Value is PdfHTMLTextElement)
		{
			PdfHTMLTextElement pdfHTMLTextElement = Value as PdfHTMLTextElement;
			pdfHTMLTextElement.m_htmllist.Clear();
			string text = (pdfHTMLTextElement.HTMLText = pdfHTMLTextElement.HTMLText);
			float width = num + (Style.Borders.Left.Width + Style.Borders.Right.Width);
			PdfStringFormat pdfStringFormat = new PdfStringFormat();
			pdfStringFormat.MeasureTrailingSpaces = true;
			PdfStringLayouter pdfStringLayouter2 = new PdfStringLayouter();
			pdfHTMLTextElement.ParseHtml(text);
			text = string.Empty;
			for (int i = 0; i < pdfHTMLTextElement.m_htmllist.Count; i++)
			{
				text += pdfHTMLTextElement.m_htmllist[i].minnerText;
			}
			pdfHTMLTextElement.m_htmllist.Clear();
			SizeF size = pdfHTMLTextElement.Font.MeasureString(text, width);
			num2 = pdfStringLayouter2.Layout(text, pdfHTMLTextElement.Font, pdfStringFormat, size).ActualSize.Height;
			num2 += (Style.Borders.Top.Width + Style.Borders.Bottom.Width) * 2f;
		}
		if (Value is PdfTextElement)
		{
			PdfTextElement pdfTextElement = Value as PdfTextElement;
			string text2 = pdfTextElement.Text;
			if (!m_finsh)
			{
				text2 = ((!string.IsNullOrEmpty(m_remainingString)) ? m_remainingString : ((string)Value));
			}
			PdfStringLayoutResult pdfStringLayoutResult = pdfStringLayouter.Layout(text2, pdfTextElement.Font ?? GetTextFont(), pdfTextElement.StringFormat ?? StringFormat, new SizeF(num, float.MaxValue));
			num2 += pdfStringLayoutResult.ActualSize.Height;
			num2 += (Style.Borders.Top.Width + Style.Borders.Bottom.Width) * 2f;
		}
		else if (Value is string || m_remainingString != null || Value is PdfDocumentLinkAnnotation)
		{
			string empty = string.Empty;
			empty = ((!(Value is PdfDocumentLinkAnnotation)) ? ((string)Value) : (Value as PdfDocumentLinkAnnotation).Text);
			if (!m_finsh)
			{
				empty = ((!string.IsNullOrEmpty(m_remainingString)) ? m_remainingString : ((string)Value));
			}
			PdfStringLayoutResult pdfStringLayoutResult2 = pdfStringLayouter.Layout(empty, GetTextFont(), StringFormat, new SizeF(num, float.MaxValue));
			num2 += pdfStringLayoutResult2.ActualSize.Height;
			num2 += (Style.Borders.Top.Width + Style.Borders.Bottom.Width) * 2f;
			if (pdfStringLayoutResult2.LineCount > 1 && Style.StringFormat != null && Style.StringFormat.LineSpacing != 0f)
			{
				num2 += (float)(pdfStringLayoutResult2.LineCount - 1) * Style.StringFormat.LineSpacing;
			}
		}
		else if (Value is PdfGrid)
		{
			(Value as PdfGrid).parentGridWidth = num;
			num2 = (Value as PdfGrid).Size.Height;
			num2 += Style.Borders.Top.Width / 2f + Style.Borders.Bottom.Width / 2f;
		}
		else if (m_value is PdfImage)
		{
			PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
			PdfImage pdfImage = m_value as PdfImage;
			num2 = pdfUnitConvertor.ConvertFromPixels(pdfImage.Height, PdfGraphicsUnit.Point);
		}
		num2 = ((Style.CellPadding != null) ? (num2 + (Style.CellPadding.Top + Style.CellPadding.Bottom)) : (num2 + (Row.Grid.Style.CellPadding.Top + Row.Grid.Style.CellPadding.Bottom)));
		return num2 + Row.Grid.Style.CellSpacing;
	}

	private RectangleF AdjustOuterLayoutArea(RectangleF bounds, PdfGraphics g)
	{
		bool flag = false;
		float cellSpacing = Row.Grid.Style.CellSpacing;
		if (cellSpacing > 0f)
		{
			bounds = new RectangleF(bounds.X + cellSpacing, bounds.Y + cellSpacing, bounds.Width - cellSpacing, bounds.Height - cellSpacing);
		}
		int num = Row.Cells.IndexOf(this);
		if (ColumnSpan > 1 || (Row.RowOverflowIndex > 0 && num == Row.RowOverflowIndex + 1 && m_bIsCellMergeContinue))
		{
			int num2 = ColumnSpan;
			if (num2 == 1 && m_bIsCellMergeContinue)
			{
				for (int i = num + 1; i < Row.Grid.Columns.Count && Row.Cells[i].m_bIsCellMergeContinue; i++)
				{
					num2++;
				}
			}
			float num3 = 0f;
			for (int j = num; j < num + num2; j++)
			{
				if (Row.Grid.Style.AllowHorizontalOverflow)
				{
					float num4 = ((Row.Grid.Size.Width < g.ClientSize.Width) ? Row.Grid.Size.Width : g.ClientSize.Width);
					float num5 = ((!(Row.Grid.Size.Width > g.ClientSize.Width)) ? (num3 + Row.Grid.Columns[j].Width) : (bounds.X + num3 + Row.Grid.Columns[j].Width));
					if (num5 > num4)
					{
						break;
					}
				}
				num3 += Row.Grid.Columns[j].Width;
				if (Row.Grid.IsChildGrid && !cellBorderCuttOffX)
				{
					if (j == 0)
					{
						num3 -= Style.Borders.Left.Width / 2f;
					}
					if ((j != 0 && j == Row.Cells.Count - 1) || (j == 0 && Row.Cells.Count - 1 == 0))
					{
						num3 -= Style.Borders.Right.Width / 2f;
					}
				}
			}
			num3 -= Row.Grid.Style.CellSpacing;
			bounds.Width = num3;
			if (bounds.Width > 0f && cellBorderCuttOffX)
			{
				bounds.Width -= Style.Borders.Left.Width / 2f;
			}
		}
		if (RowSpan > 1 || Row.RowSpanExists)
		{
			int num6 = RowSpan;
			int num7 = Row.Grid.Rows.IndexOf(Row);
			if (num7 == -1)
			{
				num7 = Row.Grid.Headers.IndexOf(Row);
				if (num7 != -1)
				{
					flag = true;
				}
			}
			if (num6 == 1 && m_bIsCellMergeContinue)
			{
				for (int k = num7 + 1; k < Row.Grid.Rows.Count && (flag ? Row.Grid.Headers[k].Cells[num].m_bIsCellMergeContinue : Row.Grid.Rows[k].Cells[num].m_bIsCellMergeContinue); k++)
				{
					num6++;
				}
			}
			float num8 = 0f;
			float num9 = 0f;
			if (flag)
			{
				for (int l = num7; l < num7 + num6; l++)
				{
					num8 += Row.Grid.Headers[l].Height;
				}
				num8 -= Row.Grid.Style.CellSpacing;
				bounds.Height = num8;
			}
			else
			{
				for (int m = num7; m < num7 + num6; m++)
				{
					if (!Row.Grid.Rows[m].m_isRowSpanRowHeightSet)
					{
						Row.Grid.Rows[m].m_isRowHeightSet = false;
					}
					num8 += (flag ? Row.Grid.Headers[m].Height : Row.Grid.Rows[m].Height);
					PdfGridRow pdfGridRow = Row.Grid.Rows[m];
					int num10 = Row.Grid.Rows.IndexOf(pdfGridRow);
					if (RowSpan > 1)
					{
						foreach (PdfGridCell cell in pdfGridRow.Cells)
						{
							if (cell.RowSpan <= 1)
							{
								continue;
							}
							float num11 = 0f;
							for (int n = m; n < m + cell.RowSpan; n++)
							{
								if (!Row.Grid.Rows[n].m_isRowSpanRowHeightSet)
								{
									Row.Grid.Rows[n].m_isRowHeightSet = false;
								}
								num11 += Row.Grid.Rows[n].Height;
								if (!Row.Grid.Rows[n].m_isRowSpanRowHeightSet)
								{
									Row.Grid.Rows[n].m_isRowHeightSet = true;
								}
							}
							if (cell.Height > num11 && !(cell.Value is PdfImage) && num9 < cell.Height - num11)
							{
								num9 = cell.Height - num11;
								if (rowSpanRemainingHeight != 0f && num9 > rowSpanRemainingHeight)
								{
									num9 += rowSpanRemainingHeight;
								}
								int index = pdfGridRow.Cells.IndexOf(cell);
								Row.Grid.Rows[num10 + cell.RowSpan - 1].Cells[index].m_rowSpanRemainingHeight = num9;
								rowSpanRemainingHeight = Row.Grid.Rows[num10 + cell.RowSpan - 1].Cells[index].m_rowSpanRemainingHeight;
							}
						}
					}
					if (!Row.Grid.Rows[m].m_isRowSpanRowHeightSet)
					{
						Row.Grid.Rows[m].m_isRowHeightSet = true;
					}
				}
				int index2 = Row.Cells.IndexOf(this);
				num8 -= Row.Grid.Style.CellSpacing;
				if (Row.Cells[index2].Height > num8 && !Row.Grid.Rows[num7 + num6 - 1].m_isRowHeightSet)
				{
					Row.Grid.Rows[num7 + num6 - 1].Cells[index2].m_rowSpanRemainingHeight = Row.Cells[index2].Height - num8;
					num8 = (bounds.Height = Row.Cells[index2].Height);
				}
				else
				{
					bounds.Height = num8;
				}
				if (!Row.RowMergeComplete)
				{
					bounds.Height = num8;
				}
			}
			if (bounds.Height > 0f && cellBorderCuttOffY)
			{
				bounds.Height -= Style.Borders.Top.Width / 2f;
			}
		}
		return bounds;
	}

	private RectangleF AdjustContentLayoutArea(RectangleF bounds)
	{
		if (Value is PdfGrid)
		{
			SizeF size = (Value as PdfGrid).Size;
			if (Style.CellPadding == null)
			{
				RectangleF rectangleF = bounds;
				bounds.Width -= m_row.Grid.Style.CellPadding.Right + m_row.Grid.Style.CellPadding.Left;
				bounds.Height -= m_row.Grid.Style.CellPadding.Bottom + m_row.Grid.Style.CellPadding.Top;
				if (StringFormat.Alignment == PdfTextAlignment.Center)
				{
					bounds.X += m_row.Grid.Style.CellPadding.Left + (bounds.Width - size.Width) / 2f;
					bounds.Y += m_row.Grid.Style.CellPadding.Top + (bounds.Height - size.Height) / 2f;
					if (bounds.Y < 0f)
					{
						bounds.Y = rectangleF.Y;
					}
				}
				else if (StringFormat.Alignment == PdfTextAlignment.Left)
				{
					bounds.X += m_row.Grid.Style.CellPadding.Left;
					bounds.Y += m_row.Grid.Style.CellPadding.Top;
				}
				else if (StringFormat.Alignment == PdfTextAlignment.Right)
				{
					bounds.X += m_row.Grid.Style.CellPadding.Left + (bounds.Width - size.Width);
					bounds.Y += m_row.Grid.Style.CellPadding.Top;
					bounds.Width = size.Width;
				}
			}
			else
			{
				bounds.Width -= Style.CellPadding.Right + Style.CellPadding.Left;
				bounds.Height -= Style.CellPadding.Bottom + Style.CellPadding.Top;
				if (StringFormat.Alignment == PdfTextAlignment.Center)
				{
					bounds.X += Style.CellPadding.Left + (bounds.Width - size.Width) / 2f;
					bounds.Y += Style.CellPadding.Top + (bounds.Height - size.Height) / 2f;
				}
				else if (StringFormat.Alignment == PdfTextAlignment.Left)
				{
					bounds.X += Style.CellPadding.Left;
					bounds.Y += Style.CellPadding.Top;
				}
				else if (StringFormat.Alignment == PdfTextAlignment.Right)
				{
					bounds.X += Style.CellPadding.Left + (bounds.Width - size.Width);
					bounds.Y += Style.CellPadding.Top;
					bounds.Width = size.Width;
				}
			}
		}
		else if (Style.CellPadding == null)
		{
			bounds.X += m_row.Grid.Style.CellPadding.Left;
			bounds.Y += m_row.Grid.Style.CellPadding.Top;
			bounds.Width -= m_row.Grid.Style.CellPadding.Right + m_row.Grid.Style.CellPadding.Left;
			bounds.Height -= m_row.Grid.Style.CellPadding.Bottom + m_row.Grid.Style.CellPadding.Top;
		}
		else
		{
			bounds.X += Style.CellPadding.Left;
			bounds.Y += Style.CellPadding.Top;
			bounds.Width -= Style.CellPadding.Right + Style.CellPadding.Left;
			bounds.Height -= Style.CellPadding.Bottom + Style.CellPadding.Top;
		}
		return bounds;
	}

	private bool CheckLastCell(int spanCount)
	{
		bool result = false;
		if (m_row.Cells.IndexOf(this) + spanCount == m_row.Cells.Count)
		{
			return true;
		}
		return result;
	}

	internal void DrawCellBorders(ref PdfGraphics graphics, RectangleF bounds)
	{
		bool flag = graphics.Tag != null;
		if (Row.RowSpanExists && Row.Grid.AllowRowBreakAcrossPages)
		{
			float num = 0f;
			bool flag2 = false;
			if (Row.Grid.m_listOfNavigatePages.Count > 0 && Row.Grid.LayoutFormat != null)
			{
				PdfLayoutFormat layoutFormat = Row.Grid.LayoutFormat;
				if (layoutFormat.UsePaginateBounds && layoutFormat.Break == PdfLayoutBreakType.FitPage && layoutFormat.Layout == PdfLayoutType.Paginate)
				{
					_ = layoutFormat.PaginateBounds;
					if (layoutFormat.PaginateBounds.Height > 0f)
					{
						num = layoutFormat.PaginateBounds.Bottom;
						flag2 = true;
					}
				}
			}
			RectangleF empty = RectangleF.Empty;
			if (!Row.Grid.IsChildGrid && Row.Grid.m_listOfNavigatePages.Count == 1)
			{
				empty = Row.Grid.m_gridLocation;
				num += empty.Y;
			}
			if (flag2 && bounds.Bottom > num && Row.RowIndex + 1 < Row.Grid.Rows.Count)
			{
				PdfGridRow pdfGridRow = Row.Grid.Rows[Row.RowIndex + 1];
				if (num - bounds.Y > bounds.Height)
				{
					pdfGridRow.m_borderReminingHeight = bounds.Height - Row.Height;
					bounds.Height = Row.Height;
				}
				else
				{
					pdfGridRow.m_borderReminingHeight = bounds.Height - (num - bounds.Y);
					bounds.Height = num - bounds.Y;
				}
				pdfGridRow.m_drawCellBroders = true;
			}
		}
		if (Row.Grid.Style.BorderOverlapStyle == PdfBorderOverlapStyle.Inside)
		{
			bounds.X += Style.Borders.Left.Width;
			bounds.Y += Style.Borders.Top.Width;
			bounds.Width -= Style.Borders.Right.Width;
			bounds.Height -= Style.Borders.Bottom.Width;
		}
		graphics.IsTemplateGraphics = true;
		if (Style.Borders.IsAll)
		{
			if (bounds.X + bounds.Width > graphics.ClientSize.Width - m_style.Borders.Left.Width / 2f)
			{
				bounds.Width -= m_style.Borders.Left.Width / 2f;
			}
			if (bounds.Y + bounds.Height > graphics.ClientSize.Height - m_style.Borders.Bottom.Width / 2f)
			{
				bounds.Height -= m_style.Borders.Bottom.Width / 2f;
			}
			SetTransparency(ref graphics, m_style.Borders.Left);
			if (m_style.Borders.Left.Width != 0f && m_style.Borders.Left.Color.A != 0)
			{
				graphics.DrawRectangle(m_style.Borders.Left, bounds);
			}
			graphics.Restore();
			goto IL_0913;
		}
		PointF point = new PointF(bounds.X, bounds.Y + bounds.Height);
		PointF location = bounds.Location;
		PdfPen pdfPen = m_style.Borders.Left;
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(m_style.Borders.Left.Color, m_style.Borders.Left.Width);
		}
		if (pdfPen.Width != 0f && pdfPen.Color.A != 0)
		{
			if (m_style.Borders.Left.DashStyle == PdfDashStyle.Solid)
			{
				pdfPen.LineCap = PdfLineCap.Square;
			}
			SetTransparency(ref graphics, pdfPen);
			if (graphics.Tag == null)
			{
				if (!flag)
				{
					_ = graphics.IsTaggedPdf;
					if (!graphics.IsTaggedPdf)
					{
						goto IL_04c4;
					}
				}
				graphics.Tag = new PdfArtifact();
			}
			goto IL_04c4;
		}
		goto IL_04d8;
		IL_04c4:
		graphics.DrawLine(pdfPen, point, location);
		graphics.Restore();
		goto IL_04d8;
		IL_06a3:
		point = bounds.Location;
		location = new PointF(bounds.X + bounds.Width, bounds.Y);
		pdfPen = m_style.Borders.Top;
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(m_style.Borders.Top.Color, m_style.Borders.Top.Width);
		}
		if (pdfPen.Width != 0f && pdfPen.Color.A != 0)
		{
			if (m_style.Borders.Top.DashStyle == PdfDashStyle.Solid)
			{
				pdfPen.LineCap = PdfLineCap.Square;
			}
			SetTransparency(ref graphics, pdfPen);
			graphics.DrawLine(pdfPen, point, location);
			graphics.Restore();
		}
		if (graphics.Tag == null)
		{
			if (!flag)
			{
				_ = graphics.IsTaggedPdf;
				if (!graphics.IsTaggedPdf)
				{
					goto IL_079a;
				}
			}
			graphics.Tag = new PdfArtifact();
		}
		goto IL_079a;
		IL_0913:
		graphics.IsTemplateGraphics = false;
		return;
		IL_0501:
		point = new PointF(bounds.X + bounds.Width, bounds.Y);
		location = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
		pdfPen = m_style.Borders.Right;
		if (bounds.X + bounds.Width > graphics.ClientSize.Width - pdfPen.Width / 2f)
		{
			point = new PointF(graphics.ClientSize.Width - pdfPen.Width / 2f, bounds.Y);
			location = new PointF(graphics.ClientSize.Width - pdfPen.Width / 2f, bounds.Y + bounds.Height);
		}
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(m_style.Borders.Right.Color, m_style.Borders.Right.Width);
		}
		if (pdfPen.Width != 0f && pdfPen.Color.A != 0)
		{
			if (m_style.Borders.Right.DashStyle == PdfDashStyle.Solid)
			{
				pdfPen.LineCap = PdfLineCap.Square;
			}
			SetTransparency(ref graphics, pdfPen);
			graphics.DrawLine(pdfPen, point, location);
			graphics.Restore();
		}
		if (graphics.Tag == null)
		{
			if (!flag)
			{
				_ = graphics.IsTaggedPdf;
				if (!graphics.IsTaggedPdf)
				{
					goto IL_06a3;
				}
			}
			graphics.Tag = new PdfArtifact();
		}
		goto IL_06a3;
		IL_04d8:
		if (graphics.Tag == null)
		{
			if (!flag)
			{
				_ = graphics.IsTaggedPdf;
				if (!graphics.IsTaggedPdf)
				{
					goto IL_0501;
				}
			}
			graphics.Tag = new PdfArtifact();
		}
		goto IL_0501;
		IL_079a:
		point = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
		location = new PointF(bounds.X, bounds.Y + bounds.Height);
		pdfPen = m_style.Borders.Bottom;
		if (bounds.Y + bounds.Height > graphics.ClientSize.Height - pdfPen.Width / 2f)
		{
			point = new PointF(bounds.X + bounds.Width, graphics.ClientSize.Height - pdfPen.Width / 2f);
			location = new PointF(bounds.X, graphics.ClientSize.Height - pdfPen.Width / 2f);
		}
		if (pdfPen.IsImmutable)
		{
			pdfPen = new PdfPen(m_style.Borders.Bottom.Color, m_style.Borders.Bottom.Width);
		}
		if (pdfPen.Width != 0f && pdfPen.Color.A != 0)
		{
			if (m_style.Borders.Bottom.DashStyle == PdfDashStyle.Solid)
			{
				pdfPen.LineCap = PdfLineCap.Square;
			}
			SetTransparency(ref graphics, pdfPen);
			graphics.DrawLine(pdfPen, point, location);
			graphics.Restore();
		}
		goto IL_0913;
	}

	private void SetTransparency(ref PdfGraphics graphics, PdfPen pen)
	{
		float transparency = (float)(int)pen.Color.A / 255f;
		graphics.Save();
		graphics.SetTransparency(transparency);
	}

	private PdfGridCell ObtainNextCell()
	{
		int num = m_row.Cells.IndexOf(this);
		if (num + 1 <= m_row.Cells.Count)
		{
			return m_row.Cells[num + 1];
		}
		return null;
	}

	internal void DrawCellBackground(ref PdfGraphics graphics, RectangleF bounds)
	{
		PdfBrush backgroundBrush = GetBackgroundBrush();
		graphics.IsTemplateGraphics = true;
		if (graphics.Tag == null)
		{
			_ = graphics.IsTaggedPdf;
			if (graphics.IsTaggedPdf)
			{
				graphics.Tag = new PdfArtifact();
			}
		}
		if (backgroundBrush != null)
		{
			graphics.Save();
			graphics.DrawRectangle(backgroundBrush, bounds);
			graphics.Restore();
		}
		if (Style.BackgroundImage != null)
		{
			if (Style.CellPadding != null && Style.CellPadding != new PdfPaddings())
			{
				PdfPaddings cellPadding = Style.CellPadding;
				bounds = new RectangleF(bounds.X + cellPadding.Left, bounds.Y + cellPadding.Top, bounds.Width - (cellPadding.Left + cellPadding.Right), bounds.Height - (cellPadding.Top + cellPadding.Bottom));
			}
			else if (Row.Grid.Style.CellPadding != null && Row.Grid.Style.CellPadding != new PdfPaddings())
			{
				PdfPaddings cellPadding2 = Row.Grid.Style.CellPadding;
				bounds = new RectangleF(bounds.X + cellPadding2.Left, bounds.Y + cellPadding2.Top, bounds.Width - (cellPadding2.Left + cellPadding2.Right), bounds.Height - (cellPadding2.Top + cellPadding2.Bottom));
			}
			graphics.IsTemplateGraphics = false;
			PdfImage backgroundImage = Style.BackgroundImage;
			if (m_imagePosition == PdfGridImagePosition.Stretch)
			{
				graphics.DrawImage(Style.BackgroundImage, bounds);
			}
			else if (m_imagePosition == PdfGridImagePosition.Center)
			{
				float width = backgroundImage.PhysicalDimension.Width;
				float height = backgroundImage.PhysicalDimension.Height;
				float x = ((!(width > bounds.Width)) ? (bounds.X + (bounds.Width - width) / 2f) : bounds.X);
				float y = ((!(height > bounds.Height)) ? (bounds.Y + (bounds.Height - height) / 2f) : bounds.Y);
				bool flag = false;
				if (width > bounds.Width || height > bounds.Height)
				{
					graphics.Save();
					graphics.SetClip(bounds);
					flag = true;
				}
				graphics.DrawImage(backgroundImage, x, y);
				if (flag)
				{
					graphics.Restore();
				}
			}
			else if (m_imagePosition == PdfGridImagePosition.Fit)
			{
				float width2 = backgroundImage.PhysicalDimension.Width;
				float height2 = backgroundImage.PhysicalDimension.Height;
				float val = bounds.Width / width2;
				float val2 = bounds.Height / height2;
				float num = Math.Min(val, val2);
				float num2 = width2 * num;
				float num3 = height2 * num;
				float x2 = bounds.X + (bounds.Width - num2) / 2f;
				float y2 = bounds.Y + (bounds.Height - num3) / 2f;
				graphics.DrawImage(backgroundImage, x2, y2, num2, num3);
			}
			else if (m_imagePosition == PdfGridImagePosition.Tile)
			{
				float x3 = bounds.X;
				float y3 = bounds.Y;
				float num4 = x3;
				float num5 = y3;
				_ = bounds.Width;
				new PdfUnitConvertor();
				for (; num5 < bounds.Bottom; num5 += backgroundImage.PhysicalDimension.Height)
				{
					for (num4 = x3; num4 < bounds.Right; num4 += backgroundImage.PhysicalDimension.Width)
					{
						if (num4 + backgroundImage.PhysicalDimension.Width < bounds.Right && num5 + backgroundImage.PhysicalDimension.Height < bounds.Bottom)
						{
							graphics.DrawImage(backgroundImage, new PointF(num4, num5));
						}
					}
				}
			}
		}
		graphics.IsTemplateGraphics = false;
	}

	private PdfStringFormat ObtainStringFormat()
	{
		return Style.StringFormat ?? StringFormat;
	}

	private PdfFont GetTextFont()
	{
		return Style.Font ?? Row.Style.Font ?? Row.Grid.Style.Font ?? PdfDocument.DefaultFont;
	}

	private PdfBrush GetTextBrush()
	{
		return Style.TextBrush ?? Row.Style.TextBrush ?? Row.Grid.Style.TextBrush ?? PdfBrushes.Black;
	}

	private PdfPen GetTextPen()
	{
		return Style.TextPen ?? Row.Style.TextPen ?? Row.Grid.Style.TextPen;
	}

	private PdfBrush GetBackgroundBrush()
	{
		return Style.BackgroundBrush ?? Row.Style.BackgroundBrush ?? Row.Grid.Style.BackgroundBrush;
	}

	private float CalculateWidth()
	{
		int num = Row.Cells.IndexOf(this);
		int columnSpan = ColumnSpan;
		float num2 = 0f;
		bool flag = false;
		float num3 = 0f;
		for (int i = 0; i < columnSpan; i++)
		{
			float width = Row.Grid.Columns[num + i].Width;
			if (num3 != float.MinValue || width != float.MinValue)
			{
				num3 = ((columnSpan <= 1 || Row.Grid.m_hasRowSpanSpan || width == float.MinValue) ? (num3 + width) : (((columnSpan <= num || columnSpan != Row.Cells.Count / 2 || columnSpan % 2 != 0) && m_parent == null) ? (num3 + width) : (num3 + (width + (float)columnSpan * (width / (float)Row.Cells.Count + (float)Row.Grid.Rows.Count)))));
			}
		}
		if (Row.Grid.parentGridWidth != 0f && num3 > Row.Grid.parentGridWidth)
		{
			num3 = Row.Grid.parentGridWidth / (float)Row.Grid.Columns.Count * (float)columnSpan;
		}
		if (Row.Grid.Columns.Count > 1 && Row.Grid.IsChildGrid)
		{
			for (int j = 0; j < Row.Grid.Columns.Count; j++)
			{
				num2 = Math.Abs(Row.Grid.Columns[0].Width - Row.Grid.Columns[1].Width);
				if (Row.Grid.Columns[j].Width % num2 == 0f)
				{
					flag = true;
				}
			}
		}
		if (m_parent != null && m_parent.Row.Width > 0f && Row.Grid.IsChildGrid && Row.Width > m_parent.Row.Width)
		{
			num3 = 0f;
			for (int k = 0; k < m_parent.ColumnSpan; k++)
			{
				num3 += m_parent.Row.Grid.Columns[k].Width;
			}
			num3 /= (float)Row.Cells.Count;
		}
		else if (m_parent != null && Row.Grid.IsChildGrid && (num3 == float.MinValue || (m_parent.Row.Width == float.MinValue && Row.Grid.Columns.Count > 1 && !flag && Row.Grid.IsChildGrid)))
		{
			num3 = FindGridColumnWidth(m_parent);
			num3 /= (float)Row.Cells.Count;
			if (columnSpan > 1)
			{
				num3 *= (float)columnSpan;
			}
		}
		return num3;
	}

	private float GetColumnWidth()
	{
		float num = float.MaxValue;
		num = m_parent.CalculateWidth();
		if (m_parent != null && Row.Grid != null && Row.Grid.IsChildGrid && m_parent.Row != null && m_parent.Row.Grid != null && m_parent.Row.Grid.LayoutFormat != null && m_parent.Row.Grid.LayoutFormat.Break == PdfLayoutBreakType.FitElement && m_parent.Row.Grid.Style.CellPadding != null)
		{
			num -= m_parent.Row.Grid.Style.CellPadding.Left + m_parent.Row.Grid.Style.CellPadding.Right;
			if (Row.Grid.Style.CellPadding != null)
			{
				num -= Row.Grid.Style.CellPadding.Left + Row.Grid.Style.CellPadding.Right;
			}
			if (Style.Borders != null && Style.Borders.Left != null && Style.Borders.Right != null)
			{
				num -= (Style.Borders.Left.Width + Style.Borders.Right.Width) * 2f;
			}
		}
		num /= (float)Row.Grid.Columns.Count;
		if (num <= 0f)
		{
			num = float.MaxValue;
		}
		return num;
	}

	private float FindGridColumnWidth(PdfGridCell pdfGridCell)
	{
		float result = float.MinValue;
		if (pdfGridCell.m_parent != null && pdfGridCell.m_outerCellWidth == float.MinValue)
		{
			result = FindGridColumnWidth(pdfGridCell.m_parent);
			result /= (float)pdfGridCell.Row.Cells.Count;
		}
		else if (pdfGridCell.m_parent == null && pdfGridCell.m_outerCellWidth > 0f)
		{
			result = pdfGridCell.m_outerCellWidth;
		}
		return result;
	}
}
