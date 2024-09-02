using System;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

internal class WAbsoluteTab : ParagraphItem, ILeafWidget, IWidget
{
	private AbsoluteTabAlignment m_alignment;

	private AbsoluteTabRelation m_relation;

	private TabLeader m_tabLeader;

	internal string Text
	{
		get
		{
			if (Alignment == AbsoluteTabAlignment.Left)
			{
				return ControlChar.LineBreak;
			}
			return ControlChar.Tab;
		}
	}

	internal float Position => GetTabPostion();

	public override EntityType EntityType => EntityType.AbsoluteTab;

	internal AbsoluteTabAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	internal AbsoluteTabRelation Relation
	{
		get
		{
			return m_relation;
		}
		set
		{
			m_relation = value;
		}
	}

	internal TabLeader TabLeader
	{
		get
		{
			return m_tabLeader;
		}
		set
		{
			m_tabLeader = value;
		}
	}

	internal WCharacterFormat CharacterFormat
	{
		get
		{
			return m_charFormat;
		}
		set
		{
			m_charFormat = value;
		}
	}

	internal WAbsoluteTab(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(base.Document);
		m_charFormat.SetOwner(this);
	}

	private float GetTabPostion()
	{
		if (m_relation == AbsoluteTabRelation.Margin)
		{
			return GetTabPostionRelativeToMargin();
		}
		return GetTabPostionRelativeToIndent();
	}

	private float GetTabPostionRelativeToMargin()
	{
		float result = 0f;
		WParagraph wParagraph = base.OwnerParagraph;
		if (wParagraph == null)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		Entity ownerEntity = wParagraph.GetOwnerEntity();
		switch (m_alignment)
		{
		case AbsoluteTabAlignment.Right:
			if (wParagraph.Owner.Owner is WSection)
			{
				result = (wParagraph.Owner.Owner as WSection).PageSetup.ClientWidth;
			}
			else if (wParagraph.IsInCell)
			{
				result = GetCellWidth(ownerEntity as WTableCell);
			}
			else if (ownerEntity is WTextBox || ownerEntity is Shape)
			{
				result = (GetAbsoluteTabBaseEntity(this) as WSection).PageSetup.ClientWidth;
			}
			break;
		case AbsoluteTabAlignment.Center:
			if (wParagraph.Owner.Owner is WSection)
			{
				result = (wParagraph.Owner.Owner as WSection).PageSetup.ClientWidth / 2f;
			}
			else if (wParagraph.IsInCell)
			{
				result = GetCellWidth(ownerEntity as WTableCell) / 2f;
			}
			else if (ownerEntity is WTextBox || ownerEntity is Shape)
			{
				result = (GetAbsoluteTabBaseEntity(this) as WSection).PageSetup.ClientWidth / 2f;
			}
			break;
		}
		return result;
	}

	private float GetTabPostionRelativeToIndent()
	{
		float result = 0f;
		WParagraph wParagraph = base.OwnerParagraph;
		if (wParagraph == null)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		Entity ownerEntity = wParagraph.GetOwnerEntity();
		switch (m_alignment)
		{
		case AbsoluteTabAlignment.Right:
			if (wParagraph.Owner.Owner is WSection)
			{
				result = (wParagraph.Owner.Owner as WSection).PageSetup.ClientWidth - wParagraph.ParagraphFormat.RightIndent;
			}
			else if (wParagraph.IsInCell)
			{
				result = GetCellWidth(ownerEntity as WTableCell) - wParagraph.ParagraphFormat.RightIndent;
			}
			else if (ownerEntity is WTextBox || ownerEntity is Shape)
			{
				result = (GetAbsoluteTabBaseEntity(this) as WSection).PageSetup.ClientWidth - wParagraph.ParagraphFormat.RightIndent;
			}
			break;
		case AbsoluteTabAlignment.Center:
			if (wParagraph.Owner.Owner is WSection)
			{
				result = ((wParagraph.Owner.Owner as WSection).PageSetup.ClientWidth + wParagraph.ParagraphFormat.LeftIndent) / 2f;
			}
			else if (wParagraph.IsInCell)
			{
				result = (GetCellWidth(ownerEntity as WTableCell) + wParagraph.ParagraphFormat.LeftIndent) / 2f;
			}
			else if (ownerEntity is WTextBox || ownerEntity is Shape)
			{
				result = ((GetAbsoluteTabBaseEntity(this) as WSection).PageSetup.ClientWidth + wParagraph.ParagraphFormat.LeftIndent) / 2f;
			}
			break;
		}
		return result;
	}

	private Entity GetAbsoluteTabBaseEntity(Entity ent)
	{
		while (!(ent is WSection) && ent.Owner != null)
		{
			ent = ent.Owner;
		}
		return ent;
	}

	private float GetCellWidth(WTableCell tableCell)
	{
		float num = 0f;
		if (tableCell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f)
		{
			num = (float)Math.Round(tableCell.OwnerRow.OwnerTable.TableFormat.CellSpacing, 2) * 2f;
		}
		return tableCell.Width - GetLeftPadding(tableCell) - GetRightPadding(tableCell) - num;
	}

	private float GetLeftPadding(WTableCell tableCell)
	{
		float num = tableCell.CellFormat.Paddings.Left;
		if (!tableCell.CellFormat.Paddings.HasKey(1) || num == -0.05f)
		{
			num = (tableCell.OwnerRow.RowFormat.Paddings.HasKey(1) ? tableCell.OwnerRow.RowFormat.Paddings.Left : (tableCell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(1) ? tableCell.OwnerRow.OwnerTable.TableFormat.Paddings.Left : ((tableCell.Document.ActualFormatType == FormatType.Doc) ? 0f : ((!(tableCell.OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle) || !wTableStyle.TableProperties.Paddings.HasKey(1)) ? 5.4f : wTableStyle.TableProperties.Paddings.Left))));
		}
		return num;
	}

	private float GetRightPadding(WTableCell tableCell)
	{
		float num = tableCell.CellFormat.Paddings.Right;
		if (!tableCell.CellFormat.Paddings.HasKey(4) || num == -0.05f)
		{
			num = (tableCell.OwnerRow.RowFormat.Paddings.HasKey(4) ? tableCell.OwnerRow.RowFormat.Paddings.Right : (tableCell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(4) ? tableCell.OwnerRow.OwnerTable.TableFormat.Paddings.Right : ((tableCell.Document.ActualFormatType == FormatType.Doc) ? 0f : ((!(tableCell.OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle) || !wTableStyle.TableProperties.Paddings.HasKey(4)) ? 5.4f : wTableStyle.TableProperties.Paddings.Right))));
		}
		return num;
	}

	internal override void AttachToParagraph(WParagraph paragraph, int itemPos)
	{
		base.AttachToParagraph(paragraph, itemPos);
		if (paragraph.ParagraphFormat.AbsoluteTab == null)
		{
			paragraph.ParagraphFormat.AbsoluteTab = this;
		}
	}

	internal override void Detach()
	{
		WParagraph wParagraph = base.OwnerParagraph;
		if (wParagraph == null)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		wParagraph.ParagraphFormat.AbsoluteTab = null;
		foreach (Entity childEntity in wParagraph.ChildEntities)
		{
			if (childEntity is WAbsoluteTab)
			{
				wParagraph.ParagraphFormat.AbsoluteTab = childEntity as WAbsoluteTab;
				break;
			}
		}
		base.Detach();
	}

	internal override void Close()
	{
		base.Close();
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new TabsLayoutInfo(ChildrenLayoutDirection.Horizontal);
		(m_layoutInfo as TabsLayoutInfo).AddTab(GetLayoutTabPostion(), (DocGen.Layouting.TabJustification)Alignment, (DocGen.Layouting.TabLeader)TabLeader);
		m_layoutInfo.Font = new SyncFont(CharacterFormat.GetFontToRender(FontScriptType.English));
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		SizeF result = dc.MeasureString(" ", CharacterFormat.GetFontToRender(FontScriptType.English), null, FontScriptType.English);
		result.Width = 0f;
		return result;
	}

	private float GetLayoutTabPostion()
	{
		Entity absoluteTabBaseEntity = GetAbsoluteTabBaseEntity(GetOwnerParagraphValue());
		return GetAbsolutePosition(absoluteTabBaseEntity, 0f);
	}

	internal float GetAbsolutePosition(IEntity ent, float position)
	{
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		WTableCell wTableCell = ownerParagraphValue.GetOwnerEntity() as WTableCell;
		switch (m_alignment)
		{
		case AbsoluteTabAlignment.Left:
		case AbsoluteTabAlignment.Right:
			switch (Relation)
			{
			case AbsoluteTabRelation.Margin:
				if (ownerParagraphValue.IsInCell)
				{
					position = (((IWidget)wTableCell).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Width;
				}
				else if (ent is WSection)
				{
					position = (ent as WSection).PageSetup.ClientWidth;
				}
				break;
			case AbsoluteTabRelation.Indent:
				if (ownerParagraphValue.IsInCell)
				{
					position = (((IWidget)wTableCell).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Width - ownerParagraphValue.ParagraphFormat.RightIndent;
				}
				else if (ent is WSection)
				{
					position = (ent as WSection).PageSetup.ClientWidth - ownerParagraphValue.ParagraphFormat.RightIndent;
				}
				break;
			}
			return position;
		case AbsoluteTabAlignment.Center:
			switch (Relation)
			{
			case AbsoluteTabRelation.Margin:
				if (ownerParagraphValue.IsInCell)
				{
					position = (((IWidget)wTableCell).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Width / 2f;
				}
				else if (ent is WSection)
				{
					position = (ent as WSection).PageSetup.ClientWidth / 2f;
				}
				break;
			case AbsoluteTabRelation.Indent:
				if (ownerParagraphValue.IsInCell)
				{
					position = ((((IWidget)wTableCell).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Width + ownerParagraphValue.ParagraphFormat.LeftIndent) / 2f;
				}
				else if (ent is WSection)
				{
					position = ((ent as WSection).PageSetup.ClientWidth + ownerParagraphValue.ParagraphFormat.LeftIndent) / 2f;
				}
				break;
			}
			return position;
		default:
			return position;
		}
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}
}
