using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WTextBox : ParagraphItem, IWTextBox, IParagraphItem, IEntity, ICompositeEntity, ILeafWidget, IWidget
{
	internal WTextBody m_textBody;

	protected WTextBoxFormat m_txbxFormat;

	private int m_txbxSpid;

	internal Dictionary<string, Stream> m_docxProps;

	private Shape m_shape;

	private RectangleF m_textLayoutingBounds;

	private byte m_bFlags = 1;

	public string Name
	{
		get
		{
			if (m_txbxFormat == null)
			{
				return null;
			}
			if (m_txbxFormat != null && string.IsNullOrEmpty(m_txbxFormat.Name))
			{
				int num = base.Document.TextBoxes.InnerList.IndexOf(this);
				WTextBoxFormat txbxFormat = m_txbxFormat;
				int num2 = ++num;
				txbxFormat.Name = "Text Box " + num2;
			}
			return m_txbxFormat.Name;
		}
		set
		{
			if (m_txbxFormat != null)
			{
				m_txbxFormat.Name = value;
			}
		}
	}

	public bool Visible
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && IsShape)
			{
				Shape.Visible = value;
			}
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public EntityCollection ChildEntities => m_textBody.ChildEntities;

	public override EntityType EntityType => EntityType.TextBox;

	internal RectangleF TextLayoutingBounds
	{
		get
		{
			return m_textLayoutingBounds;
		}
		set
		{
			m_textLayoutingBounds = value;
		}
	}

	internal bool IsShape
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal Dictionary<string, Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new Dictionary<string, Stream>();
			}
			return m_docxProps;
		}
	}

	public WTextBoxFormat TextBoxFormat
	{
		get
		{
			return m_txbxFormat;
		}
		set
		{
			m_txbxFormat = value;
		}
	}

	public WTextBody TextBoxBody => m_textBody;

	internal int TextBoxSpid
	{
		get
		{
			return m_txbxSpid;
		}
		set
		{
			m_txbxSpid = value;
		}
	}

	internal WCharacterFormat CharacterFormat => m_charFormat;

	internal Shape Shape
	{
		get
		{
			return m_shape;
		}
		set
		{
			m_shape = value;
		}
	}

	ILayoutInfo IWidget.LayoutInfo
	{
		get
		{
			if (m_layoutInfo == null)
			{
				CreateLayoutInfo();
			}
			return m_layoutInfo;
		}
	}

	public WTextBox(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(doc, this);
		m_txbxFormat = new WTextBoxFormat(base.Document);
		m_txbxFormat.SetOwner(this);
		m_textBody = new WTextBody(base.Document, this);
		if (base.Document != null && !base.Document.IsOpening)
		{
			IsShape = true;
			m_shape = new Shape(base.Document, AutoShapeType.Rectangle);
			m_shape.WrapFormat.AllowOverlap = m_txbxFormat.AllowOverlap;
		}
		if (base.Document != null && !base.Document.IsOpening && (!base.Document.IsCloning & !base.Document.IsMailMerge))
		{
			m_txbxFormat.InternalMargin.SetDefaultMargins();
		}
	}

	internal override void AddSelf()
	{
		if (m_textBody != null)
		{
			m_textBody.AddSelf();
		}
	}

	internal override void AttachToDocument()
	{
		base.Document.TextBoxes.Add(this);
		if (TextBoxFormat.TextWrappingStyle != 0)
		{
			base.Document.FloatingItems.Add(this);
		}
		if (m_textBody != null)
		{
			m_textBody.AttachToDocument();
		}
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		TextBoxBody.CloneRelationsTo(doc, nextOwner);
		if (nextOwner.OwnerBase is HeaderFooter || nextOwner is HeaderFooter)
		{
			TextBoxFormat.IsHeaderTextBox = true;
		}
		TextBoxFormat.CloneRelationsTo(doc, nextOwner);
		base.Document.CloneShapeEscher(doc, this);
		base.IsCloned = false;
	}

	protected override object CloneImpl()
	{
		WTextBox wTextBox = (WTextBox)base.CloneImpl();
		wTextBox.m_textBody = (WTextBody)TextBoxBody.Clone();
		if (Shape != null)
		{
			wTextBox.Shape = (Shape)Shape.Clone();
		}
		int i = 0;
		for (int count = wTextBox.m_textBody.Items.Count; i < count; i++)
		{
			wTextBox.m_textBody.Items[i].SetOwner(wTextBox.m_textBody);
		}
		wTextBox.m_txbxFormat = TextBoxFormat.Clone();
		wTextBox.m_textBody.SetOwner(wTextBox);
		wTextBox.m_txbxFormat.SetOwner(wTextBox);
		wTextBox.IsCloned = true;
		return wTextBox;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		if (TextBoxFormat.TextWrappingStyle != 0)
		{
			m_layoutInfo.IsSkipBottomAlign = true;
		}
		if (Entity.IsVerticalTextDirection(TextBoxFormat.TextDirection))
		{
			m_layoutInfo.IsVerticalText = true;
		}
		if (!Visible && GetTextWrappingStyle() != 0)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (base.IsDeleteRevision && !base.Document.RevisionOptions.ShowDeletedText)
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_textBody != null)
		{
			m_textBody.InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				return;
			}
		}
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal bool IsNoNeedToConsiderLineWidth()
	{
		if (!TextBoxFormat.NoLine || IsShape)
		{
			if (IsShape)
			{
				return !TextBoxFormat.HasKey(11);
			}
			return false;
		}
		return true;
	}

	internal void CalculateBoundsBasedOnLineWidth(ref RectangleF bounds, WTextBoxFormat textBoxFormat)
	{
		bounds.X += textBoxFormat.InternalMargin.Left + textBoxFormat.LineWidth / 2f;
		bounds.Y += textBoxFormat.InternalMargin.Top + textBoxFormat.LineWidth / 2f;
		bounds.Width -= textBoxFormat.LineWidth + textBoxFormat.InternalMargin.Left + textBoxFormat.InternalMargin.Right;
		bounds.Height -= textBoxFormat.InternalMargin.Top + textBoxFormat.InternalMargin.Bottom + textBoxFormat.LineWidth;
	}

	internal override void Detach()
	{
		base.Document.TextBoxes.Remove(this);
		base.Document.FloatingItems.Remove(this);
	}

	internal TextBodyItem GetNextTextBodyItem()
	{
		if (base.Owner is WParagraph)
		{
			return base.OwnerParagraph.GetNextTextBodyItemValue();
		}
		return null;
	}

	internal override void Close()
	{
		if (m_textBody != null)
		{
			m_textBody.Close();
			m_textBody = null;
		}
		if (m_shape != null)
		{
			m_shape.Close();
			m_shape = null;
		}
		if (m_txbxFormat != null)
		{
			m_txbxFormat.Close();
			m_txbxFormat = null;
		}
		if (m_docxProps != null)
		{
			foreach (Stream value in m_docxProps.Values)
			{
				value.Close();
			}
			m_docxProps.Clear();
			m_docxProps = null;
		}
		base.Close();
	}

	internal void SetTextBody(WTextBody textBody)
	{
		m_textBody = textBody;
	}

	internal WTable GetAsTable(int currPageIndex)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float rightMargin = 0f;
		Color color = default(Color);
		WTable wTable = new WTable(base.Document, showBorder: false);
		wTable.ResetCells(1, 1);
		wTable.TableFormat.HorizontalAlignment = GetHorAlign(TextBoxFormat.HorizontalAlignment);
		wTable.Rows[0].Cells[0].CellFormat.TextDirection = TextBoxFormat.TextDirection;
		color = TextBoxFormat.FillColor;
		float num6 = TextBoxFormat.Width;
		float num7 = TextBoxFormat.Height;
		if (TextBoxFormat.WidthRelativePercent != 0f)
		{
			num6 = GetWidthRelativeToPercent(isDocToPdf: false);
		}
		if (TextBoxFormat.HeightRelativePercent != 0f)
		{
			num7 = GetHeightRelativeToPercent(isDocToPdf: false);
		}
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = 0f;
		WSection wSection = new WSection(base.Document);
		if (base.Owner != null)
		{
			Entity owner = base.Owner;
			while (!(owner is WSection) && owner.Owner != null)
			{
				owner = owner.Owner;
			}
			if (owner is WSection)
			{
				wSection = owner as WSection;
				num = wSection.PageSetup.Margins.Left + (wSection.Document.DOP.GutterAtTop ? 0f : wSection.PageSetup.Margins.Gutter);
				rightMargin = wSection.PageSetup.Margins.Right;
				num2 = ((wSection.PageSetup.Margins.Top > 0f) ? wSection.PageSetup.Margins.Top : 36f) + (wSection.Document.DOP.GutterAtTop ? wSection.PageSetup.Margins.Gutter : 0f);
				num3 = ((wSection.PageSetup.Margins.Bottom > 0f) ? wSection.PageSetup.Margins.Bottom : 36f);
				num9 = wSection.PageSetup.PageSize.Height;
				num8 = wSection.PageSetup.PageSize.Width;
				num10 = wSection.PageSetup.ClientWidth;
				num11 = wSection.PageSetup.PageSize.Height - (num4 + num5);
				num5 = wSection.PageSetup.FooterDistance;
				num4 = wSection.PageSetup.HeaderDistance;
			}
		}
		WTableRow wTableRow = wTable.Rows[0];
		WTableCell wTableCell = wTableRow.Cells[0];
		wTableRow.Height = num7;
		if (!TextBoxFormat.NoLine)
		{
			float num12 = TextBoxFormat.LineWidth;
			if (TextBoxFormat.LineStyle == TextBoxLineStyle.Double)
			{
				num12 /= 3f;
			}
			else if (TextBoxFormat.LineStyle == TextBoxLineStyle.Triple)
			{
				num12 /= 5f;
			}
			wTableRow.RowFormat.Borders.LineWidth = num12;
			wTableCell.CellFormat.Borders.LineWidth = num12;
			wTableRow.RowFormat.Borders.Color = TextBoxFormat.LineColor;
			wTableCell.CellFormat.Borders.Color = TextBoxFormat.LineColor;
			wTableRow.RowFormat.Borders.BorderType = GetBordersStyle(TextBoxFormat.LineStyle);
			wTableCell.CellFormat.Borders.BorderType = GetBordersStyle(TextBoxFormat.LineStyle);
		}
		else
		{
			wTableCell.CellFormat.Borders.BorderType = BorderStyle.None;
			wTableRow.RowFormat.Borders.BorderType = BorderStyle.None;
		}
		if (TextBoxFormat.TextWrappingStyle != 0)
		{
			switch (TextBoxFormat.VerticalOrigin)
			{
			case VerticalOrigin.Page:
			case VerticalOrigin.TopMargin:
				wTable.TableFormat.Positioning.VertRelationTo = VerticalRelation.Page;
				switch (TextBoxFormat.VerticalAlignment)
				{
				case ShapeVerticalAlignment.Top:
					wTable.TableFormat.Positioning.VertPosition -= TextBoxFormat.InternalMargin.Top;
					break;
				case ShapeVerticalAlignment.Center:
					wTable.TableFormat.Positioning.VertPosition = (num9 - num7) / 2f;
					break;
				case ShapeVerticalAlignment.Bottom:
					wTable.TableFormat.Positioning.VertPosition = num9 - num7 - TextBoxFormat.InternalMargin.Bottom;
					break;
				case ShapeVerticalAlignment.None:
					if (Math.Abs(TextBoxFormat.VerticalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.VertPosition = num9 * (TextBoxFormat.VerticalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.VertPosition = TextBoxFormat.VerticalPosition;
					}
					break;
				}
				break;
			case VerticalOrigin.Paragraph:
			case VerticalOrigin.Line:
				wTable.TableFormat.Positioning.VertRelationTo = VerticalRelation.Paragraph;
				wTable.TableFormat.Positioning.VertPosition = TextBoxFormat.VerticalPosition;
				break;
			case VerticalOrigin.Margin:
				wTable.TableFormat.Positioning.VertRelationTo = VerticalRelation.Margin;
				switch (TextBoxFormat.VerticalAlignment)
				{
				case ShapeVerticalAlignment.Top:
					if (Math.Abs(TextBoxFormat.VerticalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.VertPosition = (num2 - TextBoxFormat.InternalMargin.Top) * (TextBoxFormat.VerticalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.VertPosition = TextBoxFormat.VerticalPosition - TextBoxFormat.InternalMargin.Top + num2;
					}
					break;
				case ShapeVerticalAlignment.Center:
					if (Math.Abs(TextBoxFormat.VerticalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.VertPosition = num11 / 2f * (TextBoxFormat.VerticalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.VertPosition = (num11 - num7) / 2f;
					}
					break;
				case ShapeVerticalAlignment.Bottom:
					if (Math.Abs(TextBoxFormat.VerticalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.VertPosition = (num11 - TextBoxFormat.InternalMargin.Bottom - num3) * (TextBoxFormat.VerticalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.VertPosition = num11 - num7 - TextBoxFormat.InternalMargin.Bottom - num3;
					}
					break;
				case ShapeVerticalAlignment.None:
					if (Math.Abs(TextBoxFormat.VerticalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.VertPosition = num11 * (TextBoxFormat.VerticalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.VertPosition = TextBoxFormat.VerticalPosition;
					}
					break;
				}
				break;
			default:
				if (wTable.TableFormat.Positioning.VertPosition == 0f)
				{
					wTable.TableFormat.Positioning.VertPosition = TextBoxFormat.VerticalPosition;
				}
				break;
			}
			switch (TextBoxFormat.HorizontalOrigin)
			{
			case HorizontalOrigin.Page:
				wTable.TableFormat.Positioning.HorizRelationTo = HorizontalRelation.Page;
				switch (TextBoxFormat.HorizontalAlignment)
				{
				case ShapeHorizontalAlignment.Center:
					wTable.TableFormat.Positioning.HorizPosition = (num8 - num6) / 2f;
					break;
				case ShapeHorizontalAlignment.Left:
					wTable.TableFormat.Positioning.HorizPosition -= TextBoxFormat.InternalMargin.Left;
					break;
				case ShapeHorizontalAlignment.Right:
					wTable.TableFormat.Positioning.HorizPosition = num8 - num6 - TextBoxFormat.InternalMargin.Right;
					break;
				case ShapeHorizontalAlignment.None:
					if (Math.Abs(TextBoxFormat.HorizontalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.HorizPosition = num8 * (TextBoxFormat.HorizontalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.HorizPosition = TextBoxFormat.HorizontalPosition;
					}
					break;
				}
				break;
			case HorizontalOrigin.Column:
				wTable.TableFormat.Positioning.HorizRelationTo = HorizontalRelation.Column;
				switch (TextBoxFormat.HorizontalAlignment)
				{
				case ShapeHorizontalAlignment.Center:
					wTable.TableFormat.Positioning.HorizPosition = (num10 - num6) / 2f;
					break;
				case ShapeHorizontalAlignment.Left:
					wTable.TableFormat.Positioning.HorizPosition = wTable.TableFormat.LeftIndent - TextBoxFormat.InternalMargin.Left;
					break;
				case ShapeHorizontalAlignment.Right:
					wTable.TableFormat.Positioning.HorizPosition = num10 - num6 - TextBoxFormat.InternalMargin.Right;
					break;
				case ShapeHorizontalAlignment.None:
					wTable.TableFormat.Positioning.HorizPosition = TextBoxFormat.HorizontalPosition;
					break;
				}
				break;
			case HorizontalOrigin.Margin:
				wTable.TableFormat.Positioning.HorizRelationTo = HorizontalRelation.Margin;
				switch (TextBoxFormat.HorizontalAlignment)
				{
				case ShapeHorizontalAlignment.Center:
					if (Math.Abs(TextBoxFormat.HorizontalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.HorizPosition = num10 / 2f * (TextBoxFormat.HorizontalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.HorizPosition = (num10 - num6) / 2f;
					}
					break;
				case ShapeHorizontalAlignment.Left:
					if (Math.Abs(TextBoxFormat.HorizontalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.HorizPosition = (num - TextBoxFormat.InternalMargin.Left) * (TextBoxFormat.HorizontalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.HorizPosition = wTable.TableFormat.LeftIndent - TextBoxFormat.InternalMargin.Left;
					}
					break;
				case ShapeHorizontalAlignment.Right:
					if (Math.Abs(TextBoxFormat.HorizontalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.HorizPosition = (num10 - TextBoxFormat.InternalMargin.Right) * (TextBoxFormat.HorizontalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.HorizPosition = num10 - num6 - TextBoxFormat.InternalMargin.Right;
					}
					break;
				case ShapeHorizontalAlignment.None:
					if (Math.Abs(TextBoxFormat.HorizontalRelativePercent) <= 1000f)
					{
						wTable.TableFormat.Positioning.HorizPosition = num10 * (TextBoxFormat.HorizontalRelativePercent / 100f);
					}
					else
					{
						wTable.TableFormat.Positioning.HorizPosition = TextBoxFormat.HorizontalPosition;
					}
					break;
				}
				break;
			case HorizontalOrigin.LeftMargin:
				wTable.TableFormat.Positioning.HorizPosition = GetLeftMarginHorizPosition(num, TextBoxFormat.HorizontalAlignment, TextBoxFormat.HorizontalPosition, num6, TextBoxFormat.TextWrappingStyle);
				break;
			case HorizontalOrigin.RightMargin:
				wTable.TableFormat.Positioning.HorizPosition = GetRightMarginHorizPosition(num8, rightMargin, TextBoxFormat.HorizontalAlignment, TextBoxFormat.HorizontalPosition, num6, TextBoxFormat.TextWrappingStyle);
				break;
			case HorizontalOrigin.InsideMargin:
				if (currPageIndex % 2 == 0)
				{
					wTable.TableFormat.Positioning.HorizPosition = GetRightMarginHorizPosition(num8, rightMargin, TextBoxFormat.HorizontalAlignment, TextBoxFormat.HorizontalPosition, num6, TextBoxFormat.TextWrappingStyle);
				}
				else
				{
					wTable.TableFormat.Positioning.HorizPosition = GetLeftMarginHorizPosition(num, TextBoxFormat.HorizontalAlignment, TextBoxFormat.HorizontalPosition, num6, TextBoxFormat.TextWrappingStyle);
				}
				break;
			case HorizontalOrigin.OutsideMargin:
				if (currPageIndex % 2 == 0)
				{
					wTable.TableFormat.Positioning.HorizPosition = GetLeftMarginHorizPosition(num, TextBoxFormat.HorizontalAlignment, TextBoxFormat.HorizontalPosition, num6, TextBoxFormat.TextWrappingStyle);
				}
				else
				{
					wTable.TableFormat.Positioning.HorizPosition = GetRightMarginHorizPosition(num8, rightMargin, TextBoxFormat.HorizontalAlignment, TextBoxFormat.HorizontalPosition, num6, TextBoxFormat.TextWrappingStyle);
				}
				break;
			default:
				if (wTable.TableFormat.Positioning.VertPosition == 0f)
				{
					wTable.TableFormat.Positioning.VertPosition = TextBoxFormat.VerticalPosition;
				}
				break;
			}
			if (TextBoxFormat.HorizontalOrigin != HorizontalOrigin.Page && TextBoxFormat.HorizontalOrigin != HorizontalOrigin.Column)
			{
				wTable.TableFormat.Positioning.HorizPosition += num;
			}
		}
		if (TextBoxFormat.FillEfects.Type == BackgroundType.NoBackground)
		{
			color = ((TextBoxFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText) ? Color.Transparent : TextBoxFormat.FillColor);
		}
		else if (TextBoxFormat.FillEfects.Type == BackgroundType.Gradient)
		{
			color = TextBoxFormat.FillEfects.Gradient.Color2;
			wTableCell.CellFormat.TextureStyle = TextureStyle.Texture30Percent;
		}
		wTable.TableFormat.BackColor = color;
		wTable.TableFormat.ForeColor = TextBoxFormat.TextThemeColor;
		wTable.TableFormat.Paddings.Left = TextBoxFormat.InternalMargin.Left;
		wTable.TableFormat.Paddings.Right = TextBoxFormat.InternalMargin.Right;
		wTable.TableFormat.Paddings.Top = TextBoxFormat.InternalMargin.Top;
		wTable.TableFormat.Paddings.Bottom = TextBoxFormat.InternalMargin.Bottom;
		wTableCell.Width = num6;
		wTableCell.CellFormat.BackColor = color;
		wTableCell.CellFormat.VerticalAlignment = TextBoxFormat.TextVerticalAlignment;
		wTable.Rows[0].HeightType = TableRowHeightType.Exactly;
		if (TextBoxFormat.LineWidth < 1f)
		{
			wTableRow.RowFormat.Borders.BorderType = BorderStyle.None;
		}
		int i = 0;
		for (int count = TextBoxBody.Items.Count; i < count; i++)
		{
			TextBodyItem textBodyItem = TextBoxBody.Items[i];
			wTableCell.Items.Add(textBodyItem.Clone());
		}
		return wTable;
	}

	private float GetRightMarginHorizPosition(float pageWidth, float rightMargin, ShapeHorizontalAlignment horzAlignment, float horzPosition, float shapeWidth, TextWrappingStyle textWrapStyle)
	{
		float num = pageWidth - rightMargin;
		float num2 = num + horzPosition;
		switch (horzAlignment)
		{
		case ShapeHorizontalAlignment.Center:
			num2 = num + (rightMargin - shapeWidth) / 2f;
			break;
		case ShapeHorizontalAlignment.Left:
			num2 = num;
			break;
		case ShapeHorizontalAlignment.Right:
			num2 = pageWidth - shapeWidth;
			break;
		}
		if ((num2 < 0f || num2 + shapeWidth > pageWidth) && textWrapStyle != TextWrappingStyle.InFrontOfText && textWrapStyle != TextWrappingStyle.Behind)
		{
			num2 = pageWidth - shapeWidth;
		}
		return num2;
	}

	private float GetLeftMarginHorizPosition(float leftMargin, ShapeHorizontalAlignment horzAlignment, float horzPosition, float shapeWidth, TextWrappingStyle textWrapStyle)
	{
		float num = horzPosition;
		switch (horzAlignment)
		{
		case ShapeHorizontalAlignment.Center:
			num = (leftMargin - shapeWidth) / 2f;
			break;
		case ShapeHorizontalAlignment.Left:
			num = 0f;
			break;
		case ShapeHorizontalAlignment.Right:
			num = leftMargin - shapeWidth;
			break;
		}
		if (num < 0f && textWrapStyle != TextWrappingStyle.InFrontOfText && textWrapStyle != TextWrappingStyle.Behind)
		{
			num = 0f;
		}
		return num;
	}

	private RowAlignment GetHorAlign(ShapeHorizontalAlignment shapeAlign)
	{
		return shapeAlign switch
		{
			ShapeHorizontalAlignment.Center => RowAlignment.Center, 
			ShapeHorizontalAlignment.Right => RowAlignment.Right, 
			_ => RowAlignment.Left, 
		};
	}

	internal BorderStyle GetBordersStyle(TextBoxLineStyle lineStyle)
	{
		return lineStyle switch
		{
			TextBoxLineStyle.Simple => BorderStyle.Single, 
			TextBoxLineStyle.Double => BorderStyle.Double, 
			TextBoxLineStyle.ThickThin => BorderStyle.ThickThinMediumGap, 
			TextBoxLineStyle.ThinThick => BorderStyle.ThinThickMediumGap, 
			TextBoxLineStyle.Triple => BorderStyle.Triple, 
			_ => BorderStyle.None, 
		};
	}

	internal void InitializeVMLDefaultValues()
	{
		TextBoxFormat.TextWrappingStyle = TextWrappingStyle.Inline;
		TextBoxFormat.FillColor = Color.White;
	}

	internal void ApplyCharacterFormat(WCharacterFormat charFormat)
	{
		if (charFormat != null)
		{
			SetParagraphItemCharacterFormat(charFormat);
		}
	}

	internal bool Compare(WTextBox textBox)
	{
		if ((CharacterFormat == null && textBox.CharacterFormat != null) || (CharacterFormat != null && textBox.CharacterFormat == null) || (TextBoxBody == null && textBox.TextBoxBody != null) || (TextBoxBody != null && textBox.TextBoxBody == null) || (TextBoxFormat == null && textBox.TextBoxFormat != null) || (TextBoxFormat != null && textBox.TextBoxFormat == null))
		{
			return false;
		}
		if (CharacterFormat != null && !CharacterFormat.Compare(textBox.CharacterFormat))
		{
			return false;
		}
		if (IsShape && textBox.IsShape)
		{
			if (!Shape.Compare(textBox.Shape))
			{
				return false;
			}
		}
		else if (TextBoxFormat != null && !IsShape && !textBox.IsShape && !TextBoxFormat.Compare(textBox.TextBoxFormat))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0014');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0014');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (CharacterFormat != null)
		{
			stringBuilder.Append(CharacterFormat.GetAsString());
		}
		if (TextBoxFormat != null)
		{
			stringBuilder.Append(TextBoxFormat.GetAsString());
		}
		if (IsShape && Shape != null)
		{
			stringBuilder.Append(Shape.GetAsString());
		}
		else if (TextBoxBody != null)
		{
			stringBuilder.Append(TextBoxBody.GetAsString());
		}
		return stringBuilder;
	}

	internal List<string> GetIgnorableProperties()
	{
		return new List<string> { "ChildEntities", "DocxProps", "DocGen.Layouting.IWidget.LayoutInfo", "TextLayoutingBounds", "Visible", "IsShape", "TextBoxSpid", "EntityType" };
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("body", TextBoxBody);
		base.XDLSHolder.AddElement("textbox-format", TextBoxFormat);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.TextBox);
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		float width = TextBoxFormat.Width;
		float height = TextBoxFormat.Height;
		if (TextBoxFormat.WidthRelativePercent != 0f)
		{
			width = GetWidthRelativeToPercent(isDocToPdf: true);
		}
		if (TextBoxFormat.HeightRelativePercent != 0f)
		{
			height = GetHeightRelativeToPercent(isDocToPdf: true);
		}
		return new SizeF(width, height);
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
		TextBoxFormat.IsWrappingBoundsAdded = false;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	internal byte[] GetAsImage()
	{
		try
		{
			DocumentLayouter documentLayouter = new DocumentLayouter();
			byte[] result = documentLayouter.ConvertAsImage(this);
			documentLayouter.Close();
			return result;
		}
		catch
		{
			return null;
		}
	}
}
