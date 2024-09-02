using System;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WTableCell : WTextBody, ICompositeEntity, IEntity, IWidget
{
	private CellFormat m_cellFormat;

	private WCharacterFormat m_charFormat;

	internal TextureStyle m_textureStyle;

	internal Color m_foreColor = Color.Empty;

	internal CellFormat m_trackCellFormat;

	private short m_gridStartIndex;

	private CellContentControl m_CellContentControl;

	private float m_cellStartPosition = float.MinValue;

	private float m_cellEndPosition = float.MinValue;

	private ColumnSizeInfo m_sizeInfo;

	private string m_ComparisonText;

	public short GridSpan
	{
		get
		{
			return CellFormat.CellGridSpan;
		}
		internal set
		{
			CellFormat.CellGridSpan = value;
		}
	}

	internal short GridColumnStartIndex
	{
		get
		{
			return m_gridStartIndex;
		}
		set
		{
			m_gridStartIndex = value;
		}
	}

	internal CellContentControl ContentControl
	{
		get
		{
			return m_CellContentControl;
		}
		set
		{
			m_CellContentControl = value;
			m_CellContentControl.OwnerCell = this;
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			if (OwnerRow != null)
			{
				return OwnerRow.RowFormat.IsChangedFormat;
			}
			return false;
		}
	}

	public override EntityType EntityType => EntityType.TableCell;

	public WTableRow OwnerRow => base.Owner as WTableRow;

	public CellFormat CellFormat => m_cellFormat;

	public float Width
	{
		get
		{
			return CellFormat.CellWidth;
		}
		set
		{
			float cellWidth = CellFormat.CellWidth;
			CellFormat.CellWidth = value;
			if (!base.Document.IsOpening && OwnerRow != null && OwnerRow.OwnerTable != null)
			{
				OwnerRow.OwnerTable.m_tableWidth = float.MinValue;
				OwnerRow.OwnerTable.IsTableGridUpdated = false;
				OwnerRow.OwnerTable.IsTableGridVerified = false;
				UpdateTablePreferredWidth(cellWidth, value);
			}
		}
	}

	internal Color ForeColor
	{
		get
		{
			return CellFormat.ForeColor;
		}
		set
		{
			CellFormat.ForeColor = value;
		}
	}

	internal TextureStyle TextureStyle
	{
		get
		{
			return CellFormat.TextureStyle;
		}
		set
		{
			CellFormat.TextureStyle = value;
		}
	}

	internal WCharacterFormat CharacterFormat => m_charFormat;

	internal bool IsFixedWidth => Width > -1f;

	internal CellFormat TrackCellFormat
	{
		get
		{
			if (m_trackCellFormat == null)
			{
				m_trackCellFormat = new CellFormat();
				m_trackCellFormat.SetOwner(this);
			}
			return m_trackCellFormat;
		}
	}

	internal PreferredWidthInfo PreferredWidth => CellFormat.PreferredWidth;

	internal float CellStartPosition
	{
		get
		{
			if (m_cellStartPosition == float.MinValue)
			{
				m_cellStartPosition = GetCellStartPositionValue();
			}
			return m_cellStartPosition;
		}
	}

	internal float CellEndPosition
	{
		get
		{
			if (m_cellEndPosition == float.MinValue)
			{
				m_cellEndPosition = GetCellEndPositionValue();
			}
			return m_cellEndPosition;
		}
	}

	internal ColumnSizeInfo SizeInfo
	{
		get
		{
			if (m_sizeInfo == null)
			{
				m_sizeInfo = new ColumnSizeInfo();
			}
			return m_sizeInfo;
		}
	}

	internal string ComparisonText
	{
		get
		{
			if (m_ComparisonText == null)
			{
				m_ComparisonText = GetWordComparisonText();
			}
			return m_ComparisonText;
		}
	}

	protected override IEntityCollectionBase WidgetCollection
	{
		get
		{
			if (m_CellContentControl != null && m_CellContentControl.MappedCell != null && m_CellContentControl.MappedCell.ChildEntities.Count > 0)
			{
				m_CellContentControl.MappedCell.SetOwner(OwnerRow);
				m_CellContentControl.MappedCell.OwnerRow.SetOwner(OwnerRow.OwnerTable);
				m_CellContentControl.MappedCell.CellFormat.SetOwner(this);
				m_CellContentControl.MappedCell.CharacterFormat.SetOwner(this);
				if (m_CellContentControl.Paragraphs.Count > 0 && base.Paragraphs[0].ChildEntities[0] is ParagraphItem)
				{
					(m_CellContentControl.Paragraphs[0].Items[0] as WTextRange).ApplyCharacterFormat((base.Paragraphs[0].ChildEntities[0] as ParagraphItem).ParaItemCharFormat);
				}
				return m_CellContentControl.MappedCell.m_bodyItems;
			}
			return m_bodyItems;
		}
	}

	public WTableCell(IWordDocument document)
		: base((WordDocument)document, null)
	{
		m_cellFormat = new CellFormat();
		m_cellFormat.SetOwner(this);
		m_charFormat = new WCharacterFormat(base.Document);
		m_charFormat.SetOwner(this);
	}

	public new Entity Clone()
	{
		return (Entity)CloneImpl();
	}

	public int GetCellIndex()
	{
		if (ContentControl != null && ContentControl.MappedCell != null)
		{
			int indexInOwnerCollection = GetIndexInOwnerCollection();
			if (indexInOwnerCollection <= 0)
			{
				return 0;
			}
			return indexInOwnerCollection;
		}
		return GetIndexInOwnerCollection();
	}

	protected override object CloneImpl()
	{
		WTableCell wTableCell = (WTableCell)base.CloneImpl();
		wTableCell.m_cellFormat = new CellFormat();
		wTableCell.m_cellFormat.SetOwner(wTableCell);
		wTableCell.m_cellFormat.ImportContainer(m_cellFormat);
		wTableCell.m_cellFormat.Paddings.ImportPaddings(m_cellFormat.Paddings);
		wTableCell.m_charFormat = new WCharacterFormat(base.Document);
		wTableCell.m_charFormat.ImportContainer(CharacterFormat);
		wTableCell.m_charFormat.SetOwner(wTableCell);
		return wTableCell;
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("cell-format", CellFormat);
		base.XDLSHolder.AddElement("character-format", CharacterFormat);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (!m_cellFormat.OwnerRowFormat.HasSprms())
		{
			if (IsFixedWidth)
			{
				writer.WriteValue("Width", Width);
			}
			if (ForeColor != Color.Empty)
			{
				writer.WriteValue("ForeColor", ForeColor);
			}
			writer.WriteValue("Texture", TextureStyle);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Width"))
		{
			Width = reader.ReadFloat("Width");
		}
		if (reader.HasAttribute("ForeColor"))
		{
			ForeColor = reader.ReadColor("ForeColor");
		}
		if (reader.HasAttribute("Texture"))
		{
			TextureStyle = (TextureStyle)(object)reader.ReadEnum("Texture", typeof(TextureStyle));
		}
	}

	internal bool IsCellWidthZero()
	{
		if (PreferredWidth.WidthType < FtsWidth.Percentage || PreferredWidth.Width == 0f)
		{
			return Width == 0f;
		}
		return false;
	}

	private void UpdateTablePreferredWidth(float prevWidth, float newValue)
	{
		if ((byte)OwnerRow.OwnerTable.PreferredTableWidth.WidthType > 1)
		{
			float num = (float)Math.Round(OwnerRow.GetRowWidth(), 2);
			if (OwnerRow.OwnerTable.PreferredTableWidth.WidthType == FtsWidth.Point && OwnerRow.OwnerTable.PreferredTableWidth.Width < num)
			{
				OwnerRow.OwnerTable.PreferredTableWidth.Width += newValue - prevWidth;
			}
			else if (OwnerRow.OwnerTable.PreferredTableWidth.WidthType == FtsWidth.Percentage)
			{
				float ownerWidth = OwnerRow.OwnerTable.GetOwnerWidth();
				float num2 = ownerWidth * OwnerRow.OwnerTable.PreferredTableWidth.Width / 100f;
				if (num2 < num)
				{
					OwnerRow.OwnerTable.PreferredTableWidth.Width = (num2 + newValue - prevWidth) / ownerWidth * 100f;
				}
			}
		}
		if (CellFormat.HorizontalMerge == CellMerge.Start)
		{
			PreferredWidth.WidthType = FtsWidth.None;
			PreferredWidth.Width = 0f;
		}
		else if (PreferredWidth.WidthType == FtsWidth.Percentage)
		{
			float ownerWidth2 = OwnerRow.OwnerTable.GetOwnerWidth();
			float tableClientWidth = OwnerRow.OwnerTable.GetTableClientWidth(ownerWidth2);
			PreferredWidth.Width = newValue / tableClientWidth * 100f;
		}
		else
		{
			PreferredWidth.WidthType = FtsWidth.Point;
			PreferredWidth.Width = newValue;
		}
	}

	internal void ApplyTableStyleBaseFormats(CellFormat cellFormat, WParagraphFormat paraFormat, WCharacterFormat charFormat, BodyItemCollection items)
	{
		CellFormat.ApplyBase(cellFormat);
		bool isOverrideTableStyleFontSize = base.Document.Settings.CompatibilityOptions[CompatibilityOption.overrideTableStyleFontSizeAndJustification];
		bool isOverrideFontSize = false;
		bool isOverrideFontSizeBidi = false;
		IsOverrideStyleHierarchy(ref isOverrideFontSize, ref isOverrideFontSizeBidi);
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] is WParagraph)
			{
				WParagraph wParagraph = items[i] as WParagraph;
				wParagraph.ParagraphFormat.TableStyleParagraphFormat = paraFormat;
				wParagraph.BreakCharacterFormat.TableStyleCharacterFormat = charFormat;
				if (isOverrideFontSize || isOverrideFontSizeBidi)
				{
					IsFontSizeDefinedInStyle(wParagraph.ParaStyle, ref isOverrideFontSize, ref isOverrideFontSizeBidi);
				}
				SetDocDefaultFontSize(wParagraph.BreakCharacterFormat, isOverrideFontSizeBidi, isOverrideFontSize);
				SetDocDefaultFontSize(wParagraph.BreakCharacterFormat.TableStyleCharacterFormat, isOverrideFontSizeBidi, isOverrideFontSize);
				for (int j = 0; j < wParagraph.Items.Count; j++)
				{
					if (wParagraph.Items[j] != null)
					{
						WCharacterFormat paraItemCharFormat = wParagraph.Items[j].ParaItemCharFormat;
						paraItemCharFormat.TableStyleCharacterFormat = charFormat;
						bool hasFontSize = paraItemCharFormat.HasKey(3);
						bool hasFontSizeBidi = paraItemCharFormat.HasKey(62);
						SetDocDefaultFontSize(paraItemCharFormat, isOverrideFontSizeBidi, isOverrideFontSize);
						SetDocDefaultFontSize(paraItemCharFormat.TableStyleCharacterFormat, isOverrideFontSizeBidi, isOverrideFontSize);
						SetCharFontSize(wParagraph, paraItemCharFormat, charFormat, isOverrideTableStyleFontSize, hasFontSize, hasFontSizeBidi);
						if (wParagraph.Items[j] is InlineContentControl)
						{
							InlineContentControl inlineControl = wParagraph.Items[j] as InlineContentControl;
							ApplyTableStyleBaseFormatsInlineCC(inlineControl, charFormat, isOverrideFontSizeBidi, isOverrideFontSize, wParagraph, isOverrideTableStyleFontSize);
						}
					}
				}
			}
			else if (items[i] is BlockContentControl)
			{
				BodyItemCollection items2 = (items[i] as BlockContentControl).TextBody.Items;
				ApplyTableStyleBaseFormats(cellFormat, paraFormat, charFormat, items2);
			}
		}
	}

	private void ApplyTableStyleBaseFormatsInlineCC(InlineContentControl inlineControl, WCharacterFormat charFormat, bool isOverrideFontSizeBidi, bool isOverrideFontSize, WParagraph para, bool isOverrideTableStyleFontSize)
	{
		foreach (ParagraphItem paragraphItem in inlineControl.ParagraphItems)
		{
			WCharacterFormat paraItemCharFormat = paragraphItem.ParaItemCharFormat;
			bool hasFontSize = paraItemCharFormat.HasKey(3);
			bool hasFontSizeBidi = paraItemCharFormat.HasKey(62);
			paraItemCharFormat.TableStyleCharacterFormat = charFormat;
			SetDocDefaultFontSize(paragraphItem.ParaItemCharFormat, isOverrideFontSizeBidi, isOverrideFontSize);
			SetCharFontSize(para, paraItemCharFormat, charFormat, isOverrideTableStyleFontSize, hasFontSize, hasFontSizeBidi);
			if (paragraphItem is InlineContentControl)
			{
				inlineControl = paragraphItem as InlineContentControl;
				ApplyTableStyleBaseFormatsInlineCC(inlineControl, charFormat, isOverrideFontSizeBidi, isOverrideFontSize, para, isOverrideTableStyleFontSize);
			}
		}
	}

	private bool IsTableStyleHasFormatting()
	{
		if ((base.Owner.Owner as WTable).GetStyle() is WTableStyle { ParagraphFormat: not null } wTableStyle && IsNotEmptyParaFormat(wTableStyle.ParagraphFormat) && !wTableStyle.CharacterFormat.HasKey(3) && !wTableStyle.CharacterFormat.HasKey(62))
		{
			return false;
		}
		return true;
	}

	private bool IsNotEmptyParaFormat(WParagraphFormat paraFormat)
	{
		if (paraFormat.PropertiesHash.Count == 0)
		{
			return false;
		}
		if (new WParagraphFormat(base.Document).Compare(paraFormat))
		{
			return false;
		}
		return true;
	}

	private void IsOverrideStyleHierarchy(ref bool isOverrideFontSize, ref bool isOverrideFontSizeBidi)
	{
		if (base.Document.DefCharFormat == null || IsTableStyleHasFormatting() || base.Document.Settings.CompatibilityOptions[CompatibilityOption.overrideTableStyleFontSizeAndJustification] || IsNormalStyleFontSizeNotInLimit(ref isOverrideFontSize, ref isOverrideFontSizeBidi))
		{
			isOverrideFontSize = false;
			isOverrideFontSizeBidi = false;
		}
	}

	private void IsFontSizeDefinedInStyle(IWParagraphStyle paraStyle, ref bool isOverrideFontSize, ref bool isOverrideFontSizeBidi)
	{
		while (paraStyle is WParagraphStyle && !(paraStyle.Name == "Normal"))
		{
			if (paraStyle.CharacterFormat.HasKey(3))
			{
				isOverrideFontSize = false;
			}
			if (paraStyle.CharacterFormat.HasKey(62))
			{
				isOverrideFontSizeBidi = false;
			}
			if (!isOverrideFontSize || !isOverrideFontSizeBidi)
			{
				break;
			}
			paraStyle = (paraStyle as WParagraphStyle).BaseStyle;
		}
	}

	private bool IsNormalStyleFontSizeNotInLimit(ref bool isOverrideFontSize, ref bool isOverrideFontSizeBidi)
	{
		WCharacterFormat wCharacterFormat = ((base.Document.Styles.FindByName("Normal") is WParagraphStyle wParagraphStyle) ? wParagraphStyle.CharacterFormat : null);
		if (wCharacterFormat == null)
		{
			return true;
		}
		if (wCharacterFormat.HasKey(3) && (float)wCharacterFormat.PropertiesHash[3] == 12f)
		{
			isOverrideFontSize = true;
		}
		if (wCharacterFormat.HasKey(62) && (float)wCharacterFormat.PropertiesHash[62] == 12f)
		{
			isOverrideFontSizeBidi = true;
		}
		if (isOverrideFontSize | isOverrideFontSizeBidi)
		{
			return false;
		}
		return true;
	}

	private void SetDocDefaultFontSize(WCharacterFormat charFormat, bool isOverrideFontSizeBidi, bool isOverrideFontSize)
	{
		if (isOverrideFontSize && UseDocDefaultFontSize(charFormat, 3))
		{
			charFormat.FontSize = charFormat.Document.DefCharFormat.FontSize;
		}
		if (isOverrideFontSizeBidi && UseDocDefaultFontSize(charFormat, 62))
		{
			charFormat.FontSizeBidi = charFormat.Document.DefCharFormat.FontSizeBidi;
		}
	}

	private void SetCharFontSize(WParagraph ownerPara, WCharacterFormat paraItemCharFormat, WCharacterFormat tableStyleCharFormat, bool isOverrideTableStyleFontSize, bool hasFontSize, bool hasFontSizeBidi)
	{
		SetFontSizeForItem(ownerPara, paraItemCharFormat, tableStyleCharFormat, isOverrideTableStyleFontSize, 3, hasFontSize);
		SetFontSizeForItem(ownerPara, paraItemCharFormat, tableStyleCharFormat, isOverrideTableStyleFontSize, 62, hasFontSizeBidi);
		SetFontSizeForItem(ownerPara, paraItemCharFormat.TableStyleCharacterFormat, tableStyleCharFormat, isOverrideTableStyleFontSize, 3, hasFontSize);
		SetFontSizeForItem(ownerPara, paraItemCharFormat.TableStyleCharacterFormat, tableStyleCharFormat, isOverrideTableStyleFontSize, 62, hasFontSizeBidi);
	}

	private void SetFontSizeForItem(WParagraph ownerPara, WCharacterFormat paraItemCharFormat, WCharacterFormat tableStyleCharFormat, bool isOverrideTableStyleFontSize, int key, bool haskey)
	{
		WCharacterFormat wCharacterFormat = ((base.Document.Styles.FindByName("Normal") is WParagraphStyle wParagraphStyle) ? wParagraphStyle.CharacterFormat : null);
		float num = ((wCharacterFormat == null) ? 0f : ((!wCharacterFormat.HasKey(key)) ? 0f : (key switch
		{
			62 => wCharacterFormat.FontSizeBidi, 
			3 => wCharacterFormat.FontSize, 
			_ => 0f, 
		})));
		if (!tableStyleCharFormat.HasKey(key) || num != 12f || !IsGetFontSizeFromTableStyle(paraItemCharFormat, ownerPara, key, haskey))
		{
			return;
		}
		if ((wCharacterFormat.FontSize == 12f && !isOverrideTableStyleFontSize) || !wCharacterFormat.HasKey(key))
		{
			switch (key)
			{
			case 3:
				paraItemCharFormat.FontSize = tableStyleCharFormat.FontSize;
				break;
			case 62:
				paraItemCharFormat.FontSizeBidi = tableStyleCharFormat.FontSizeBidi;
				break;
			}
		}
		else if (wCharacterFormat.HasKey(key) && ((wCharacterFormat.FontSize == 12f && isOverrideTableStyleFontSize) || wCharacterFormat.FontSize != 12f))
		{
			switch (key)
			{
			case 3:
				paraItemCharFormat.FontSize = wCharacterFormat.FontSize;
				break;
			case 62:
				paraItemCharFormat.FontSizeBidi = wCharacterFormat.FontSizeBidi;
				break;
			}
		}
	}

	private bool IsGetFontSizeFromTableStyle(WCharacterFormat paraItemCharFormat, WParagraph ownerPara, int key, bool hasFontSizeKey)
	{
		WCharacterStyle wCharacterStyle = ((!string.IsNullOrEmpty(paraItemCharFormat.CharStyleName)) ? (base.Document.Styles.FindByName(paraItemCharFormat.CharStyleName) as WCharacterStyle) : null);
		WCharacterStyle wCharacterStyle2 = ((wCharacterStyle != null && wCharacterStyle.BaseStyle != null && !string.IsNullOrEmpty(wCharacterStyle.BaseStyle.Name)) ? (base.Document.Styles.FindByName(wCharacterStyle.BaseStyle.Name) as WCharacterStyle) : null);
		WParagraphStyle wParagraphStyle = ((wCharacterStyle != null && !string.IsNullOrEmpty(wCharacterStyle.LinkedStyleName)) ? (base.Document.Styles.FindByName(wCharacterStyle.LinkedStyleName) as WParagraphStyle) : null);
		WParagraphStyle wParagraphStyle2 = null;
		WCharacterStyle wCharacterStyle3 = null;
		WParagraphStyle wParagraphStyle3 = null;
		if (ownerPara != null && ownerPara.ParaStyle != null && ownerPara.ParaStyle.Name != "Normal")
		{
			wParagraphStyle2 = base.Document.Styles.FindByName(ownerPara.ParaStyle.Name) as WParagraphStyle;
			wCharacterStyle3 = ((wParagraphStyle2 != null && !string.IsNullOrEmpty(wParagraphStyle2.LinkedStyleName)) ? (base.Document.Styles.FindByName(wParagraphStyle2.LinkedStyleName) as WCharacterStyle) : null);
			wParagraphStyle3 = ((wParagraphStyle2 != null && wParagraphStyle2.BaseStyle != null && !string.IsNullOrEmpty(wParagraphStyle2.BaseStyle.Name)) ? (base.Document.Styles.FindByName(wParagraphStyle2.BaseStyle.Name) as WParagraphStyle) : null);
		}
		if (!hasFontSizeKey && (wCharacterStyle == null || !wCharacterStyle.CharacterFormat.HasKey(key)) && (wParagraphStyle2 == null || !wParagraphStyle2.CharacterFormat.HasKey(key)) && (wCharacterStyle3 == null || !wCharacterStyle3.CharacterFormat.HasKey(key)) && (wParagraphStyle == null || !wParagraphStyle.CharacterFormat.HasKey(key)) && (wParagraphStyle3 == null || !wParagraphStyle3.CharacterFormat.HasKey(key)))
		{
			if (wCharacterStyle2 != null)
			{
				return !wCharacterStyle2.CharacterFormat.HasKey(key);
			}
			return true;
		}
		return false;
	}

	private bool UseDocDefaultFontSize(WCharacterFormat charFormat, short key)
	{
		if (!charFormat.HasKey(key))
		{
			return charFormat.Document.DefCharFormat.HasKey(key);
		}
		return false;
	}

	internal Entity CloneCell()
	{
		WTableCell wTableCell = (WTableCell)base.CloneImpl();
		wTableCell.m_cellFormat = new CellFormat();
		wTableCell.m_cellFormat.SetOwner(wTableCell);
		wTableCell.m_cellFormat.ImportContainer(m_cellFormat);
		wTableCell.m_cellFormat.Paddings.ImportPaddings(m_cellFormat.Paddings);
		wTableCell.m_charFormat = new WCharacterFormat(base.Document);
		wTableCell.m_charFormat.ImportContainer(CharacterFormat);
		if (!m_doc.IsCloning && wTableCell.m_charFormat.PropertiesHash.Count > 0)
		{
			foreach (int revisionKey in m_charFormat.RevisionKeys)
			{
				wTableCell.m_charFormat.PropertiesHash.Remove(revisionKey);
			}
		}
		wTableCell.m_charFormat.SetOwner(wTableCell);
		return wTableCell;
	}

	internal TextBodyItem GetNextTextBodyItem()
	{
		if (base.NextSibling == null)
		{
			if (OwnerRow == null)
			{
				return null;
			}
			if (OwnerRow.NextSibling == null)
			{
				if (OwnerRow.OwnerTable != null)
				{
					return OwnerRow.OwnerTable.GetNextTextBodyItemValue();
				}
				return null;
			}
			WTableRow wTableRow = OwnerRow;
			while (wTableRow.NextSibling != null)
			{
				wTableRow = wTableRow.NextSibling as WTableRow;
				foreach (WTableCell cell in wTableRow.Cells)
				{
					if (cell.Items.Count > 0)
					{
						return cell.Items[0];
					}
				}
			}
			return OwnerRow.OwnerTable.GetNextTextBodyItemValue();
		}
		WTableCell wTableCell2 = base.NextSibling as WTableCell;
		if (wTableCell2.Items.Count > 0)
		{
			return wTableCell2.Items[0];
		}
		return wTableCell2.GetNextTextBodyItem();
	}

	internal WTableCell GetVerticalMergeStartCell()
	{
		WTableCell wTableCell = null;
		int rowIndex = OwnerRow.GetRowIndex();
		int cellIndex = GetCellIndex();
		if (rowIndex > 0)
		{
			WTableRow wTableRow = OwnerRow.OwnerTable.Rows[rowIndex - 1];
			if (cellIndex == 0)
			{
				wTableCell = wTableRow.Cells[0];
			}
			else
			{
				float num = 0f;
				float num2 = 0f;
				int num3 = 0;
				while (cellIndex > 0 && num3 <= cellIndex - 1)
				{
					num += OwnerRow.Cells[num3].Width;
					num3++;
				}
				for (int i = 0; i < wTableRow.Cells.Count; i++)
				{
					num2 += wTableRow.Cells[i].Width;
					if (num == num2 && i + 1 < wTableRow.Cells.Count)
					{
						wTableCell = wTableRow.Cells[i + 1];
						break;
					}
				}
			}
			if (wTableCell != null && wTableCell.CellFormat.VerticalMerge == CellMerge.Continue)
			{
				wTableCell = wTableCell.GetVerticalMergeStartCell();
			}
			if (wTableCell == null)
			{
				wTableCell = this;
			}
		}
		return wTableCell;
	}

	internal override void Close()
	{
		if (m_charFormat != null)
		{
			m_charFormat.Close();
			m_charFormat = null;
		}
		if (m_cellFormat != null)
		{
			m_cellFormat.Close();
			m_cellFormat = null;
		}
		if (m_trackCellFormat != null)
		{
			m_trackCellFormat.Close();
			m_trackCellFormat = null;
		}
		if (m_CellContentControl != null)
		{
			m_CellContentControl.Close();
			m_CellContentControl = null;
		}
		base.Close();
	}

	internal float GetCellWidth()
	{
		float num = Width;
		if (CellFormat.HorizontalMerge == CellMerge.Start)
		{
			int cellIndex = GetCellIndex();
			int horizontalMergeEndCellIndex = GetHorizontalMergeEndCellIndex();
			for (int i = cellIndex + 1; i <= horizontalMergeEndCellIndex; i++)
			{
				num += OwnerRow.Cells[i].Width;
			}
		}
		return num;
	}

	internal float GetCellLayoutingWidth()
	{
		float num = Width;
		if (num == 0f)
		{
			float num2 = GetLeftPadding();
			float num3 = GetRightPadding();
			int indexInOwnerCollection = GetIndexInOwnerCollection();
			Border border = CellFormat.Borders.Left;
			if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
			{
				border = ((GetIndexInOwnerCollection() != 0) ? OwnerRow.RowFormat.Borders.Vertical : OwnerRow.RowFormat.Borders.Left);
			}
			if (!border.IsBorderDefined)
			{
				border = ((GetIndexInOwnerCollection() != 0) ? OwnerRow.OwnerTable.TableFormat.Borders.Vertical : OwnerRow.OwnerTable.TableFormat.Borders.Left);
			}
			Border border2 = CellFormat.Borders.Right;
			int num4 = OwnerRow.Cells.Count - 1;
			if (!border2.IsBorderDefined || (border2.IsBorderDefined && border2.BorderType == BorderStyle.None && border2.LineWidth == 0f && border2.Color.IsEmpty))
			{
				border2 = ((indexInOwnerCollection != num4) ? OwnerRow.RowFormat.Borders.Vertical : OwnerRow.RowFormat.Borders.Right);
			}
			if (!border2.IsBorderDefined)
			{
				border2 = ((indexInOwnerCollection != num4) ? OwnerRow.OwnerTable.TableFormat.Borders.Vertical : OwnerRow.OwnerTable.TableFormat.Borders.Right);
			}
			if (border.IsBorderDefined && border.BorderType != 0 && border.BorderType != BorderStyle.Cleared)
			{
				num2 += border.GetLineWidthValue() / 2f;
			}
			if (border2.IsBorderDefined && border2.BorderType != BorderStyle.Cleared && border2.BorderType != 0)
			{
				num3 += border2.GetLineWidthValue() / 2f;
			}
			num = num2 + num3;
		}
		return num;
	}

	private float GetCellStartPositionValue()
	{
		if (GetCellIndex() == 0)
		{
			return 0f;
		}
		if (base.PreviousSibling is WTableCell)
		{
			return (base.PreviousSibling as WTableCell).CellEndPosition;
		}
		return 0f;
	}

	private float GetCellEndPositionValue()
	{
		int cellIndex = GetCellIndex();
		if (CellFormat.HorizontalMerge == CellMerge.Continue)
		{
			for (int num = cellIndex - 1; num >= 0; num--)
			{
				if (OwnerRow.Cells[num].CellFormat.HorizontalMerge == CellMerge.Start)
				{
					return OwnerRow.Cells[num].CellEndPosition;
				}
			}
		}
		float num2 = CellStartPosition + GetCellLayoutingWidth();
		if (CellFormat.HorizontalMerge == CellMerge.Start)
		{
			int horizontalMergeEndCellIndex = GetHorizontalMergeEndCellIndex();
			for (int i = cellIndex + 1; i <= horizontalMergeEndCellIndex; i++)
			{
				num2 += OwnerRow.Cells[i].GetCellLayoutingWidth();
			}
		}
		return num2;
	}

	internal int GetHorizontalMergeEndCellIndex()
	{
		int num;
		for (int i = (num = GetCellIndex()) + 1; i < OwnerRow.Cells.Count && OwnerRow.Cells[i].CellFormat.HorizontalMerge == CellMerge.Continue; i++)
		{
			num++;
		}
		return num;
	}

	internal WTableCell GetPreviousCell()
	{
		int num = Index - 1;
		WTableCell wTableCell = OwnerRow.Cells[num];
		while (wTableCell.CellFormat.HorizontalMerge == CellMerge.Continue && num != 0)
		{
			num--;
			wTableCell = OwnerRow.Cells[num];
		}
		return wTableCell;
	}

	internal float GetLeftPadding()
	{
		float num = CellFormat.Paddings.Left;
		if (!CellFormat.Paddings.HasKey(1) || num == -0.05f)
		{
			num = (OwnerRow.RowFormat.Paddings.HasKey(1) ? OwnerRow.RowFormat.Paddings.Left : (OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(1) ? OwnerRow.OwnerTable.TableFormat.Paddings.Left : ((base.Document.ActualFormatType == FormatType.Doc) ? 0f : ((!(OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle) || !wTableStyle.TableProperties.Paddings.HasKey(1)) ? 5.4f : wTableStyle.TableProperties.Paddings.Left))));
		}
		return num;
	}

	internal float GetRightPadding()
	{
		float num = CellFormat.Paddings.Right;
		if (!CellFormat.Paddings.HasKey(4) || num == -0.05f)
		{
			num = (OwnerRow.RowFormat.Paddings.HasKey(4) ? OwnerRow.RowFormat.Paddings.Right : (OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(4) ? OwnerRow.OwnerTable.TableFormat.Paddings.Right : ((base.Document.ActualFormatType == FormatType.Doc) ? 0f : ((!(OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle) || !wTableStyle.TableProperties.Paddings.HasKey(4)) ? 5.4f : wTableStyle.TableProperties.Paddings.Right))));
		}
		return num;
	}

	internal float GetTopPadding()
	{
		float result = CellFormat.Paddings.Top;
		if (CellFormat.Paddings.Top == -0.05f || (CellFormat.Paddings.Top == 0f && !CellFormat.Paddings.HasKey(2)))
		{
			result = (OwnerRow.RowFormat.Paddings.HasKey(2) ? OwnerRow.RowFormat.Paddings.Top : (OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(2) ? OwnerRow.OwnerTable.TableFormat.Paddings.Top : ((base.Document.ActualFormatType == FormatType.Doc || !(OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle) || !wTableStyle.TableProperties.Paddings.HasKey(2)) ? 0f : wTableStyle.TableProperties.Paddings.Top)));
		}
		return result;
	}

	internal float GetBottomPadding()
	{
		float result = CellFormat.Paddings.Bottom;
		if (CellFormat.Paddings.Bottom == -0.05f || (CellFormat.Paddings.Bottom == 0f && !CellFormat.Paddings.HasKey(3)))
		{
			result = (OwnerRow.RowFormat.Paddings.HasKey(3) ? OwnerRow.RowFormat.Paddings.Bottom : (OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(3) ? OwnerRow.OwnerTable.TableFormat.Paddings.Bottom : ((base.Document.ActualFormatType == FormatType.Doc || !(OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle) || !wTableStyle.TableProperties.Paddings.HasKey(3)) ? 0f : wTableStyle.TableProperties.Paddings.Bottom)));
		}
		return result;
	}

	internal ColumnSizeInfo GetSizeInfo(bool isAutoFit, bool isAutoWidth, bool needtoCalculateParaWidth)
	{
		float minimumWordWidth = 0f;
		float maximumWordWidth = 0f;
		float paragraphWidth = 0f;
		SizeInfo.MinimumWidth = GetMinimumPreferredWidth();
		if (isAutoFit || isAutoWidth)
		{
			GetMinimumAndMaximumWordWidth(base.Items, ref minimumWordWidth, ref maximumWordWidth, ref paragraphWidth, needtoCalculateParaWidth);
			float cellSpacing = OwnerRow.OwnerTable.TableFormat.GetCellSpacing();
			SizeInfo.MinimumWordWidth = minimumWordWidth + SizeInfo.MinimumWidth + cellSpacing * 3f;
			SizeInfo.MaximumWordWidth = maximumWordWidth + SizeInfo.MinimumWidth + cellSpacing * 3f;
			SizeInfo.MaxParaWidth = paragraphWidth + SizeInfo.MinimumWidth + cellSpacing * 3f;
		}
		ColumnSizeInfo columnSizeInfo = new ColumnSizeInfo();
		columnSizeInfo.MinimumWidth = SizeInfo.MinimumWidth;
		columnSizeInfo.MinimumWordWidth = SizeInfo.MinimumWordWidth;
		if ((CellFormat.TextDirection == TextDirection.HorizontalFarEast || CellFormat.TextDirection == TextDirection.Horizontal) && CellFormat.HorizontalMerge == CellMerge.None)
		{
			columnSizeInfo.MaximumWordWidth = SizeInfo.MaximumWordWidth;
			columnSizeInfo.HasMaximumWordWidth = maximumWordWidth > 0f;
			SizeInfo.HasMaximumWordWidth = maximumWordWidth > 0f;
		}
		columnSizeInfo.MaxParaWidth = SizeInfo.MaxParaWidth;
		return columnSizeInfo;
	}

	private void GetMinimumAndMaximumWordWidth(BodyItemCollection bodyItemCollection, ref float minimumWordWidth, ref float maximumWordWidth, ref float paragraphWidth, bool needtoCalculateParaWidth)
	{
		for (int i = 0; i < bodyItemCollection.Count; i++)
		{
			GetMinimumAndMaximumWordWidth(bodyItemCollection[i], ref minimumWordWidth, ref maximumWordWidth, ref paragraphWidth, needtoCalculateParaWidth);
		}
	}

	private void GetMinimumAndMaximumWordWidth(TextBodyItem textBodyItem, ref float minimumWordWidth, ref float maximumWordWidth, ref float paragraphWidth, bool needtoCalculateParaWidth)
	{
		switch (textBodyItem.EntityType)
		{
		case EntityType.Paragraph:
			(textBodyItem as WParagraph).GetMinimumAndMaximumWordWidth(ref minimumWordWidth, ref maximumWordWidth, ref paragraphWidth, needtoCalculateParaWidth);
			break;
		case EntityType.Table:
			(textBodyItem as WTable).GetMinimumAndMaximumWordWidth(ref minimumWordWidth, ref maximumWordWidth);
			break;
		case EntityType.BlockContentControl:
			GetMinimumAndMaximumWordWidth((textBodyItem as BlockContentControl).TextBody.Items, ref minimumWordWidth, ref maximumWordWidth, ref paragraphWidth, needtoCalculateParaWidth);
			break;
		}
	}

	internal float GetMinimumPreferredWidth()
	{
		float num = GetLeftPadding() + GetRightPadding() + OwnerRow.OwnerTable.TableFormat.GetCellSpacing();
		float lineWidthValue = CellFormat.Borders.Left.GetLineWidthValue();
		if (lineWidthValue < OwnerRow.OwnerTable.TableFormat.Borders.Left.LineWidth && Index == 0)
		{
			lineWidthValue = OwnerRow.OwnerTable.TableFormat.Borders.Left.GetLineWidthValue();
		}
		else if (lineWidthValue < OwnerRow.OwnerTable.TableFormat.Borders.Vertical.LineWidth)
		{
			lineWidthValue = OwnerRow.OwnerTable.TableFormat.Borders.Vertical.GetLineWidthValue();
		}
		else if (lineWidthValue < OwnerRow.RowFormat.Borders.Left.LineWidth)
		{
			lineWidthValue = OwnerRow.RowFormat.Borders.Left.GetLineWidthValue();
		}
		else if (lineWidthValue < OwnerRow.RowFormat.Borders.Vertical.LineWidth)
		{
			lineWidthValue = OwnerRow.RowFormat.Borders.Vertical.GetLineWidthValue();
		}
		else if (OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle && Index == 0 && wTableStyle.ConditionalFormattingStyles.Count == 0 && !OwnerRow.OwnerTable.TableFormat.Borders.Left.HasKey(3) && wTableStyle.TableProperties.Borders.Left.HasKey(3) && lineWidthValue < wTableStyle.TableProperties.Borders.Left.GetLineWidthValue())
		{
			lineWidthValue = wTableStyle.TableProperties.Borders.Left.GetLineWidthValue();
		}
		float lineWidthValue2 = CellFormat.Borders.Right.GetLineWidthValue();
		if (lineWidthValue2 < OwnerRow.OwnerTable.TableFormat.Borders.Right.LineWidth && Index == OwnerRow.Cells.Count - 1)
		{
			lineWidthValue2 = OwnerRow.OwnerTable.TableFormat.Borders.Right.GetLineWidthValue();
		}
		else if (lineWidthValue2 < OwnerRow.OwnerTable.TableFormat.Borders.Vertical.LineWidth)
		{
			lineWidthValue2 = OwnerRow.OwnerTable.TableFormat.Borders.Vertical.GetLineWidthValue();
		}
		else if (lineWidthValue2 < OwnerRow.RowFormat.Borders.Right.LineWidth)
		{
			lineWidthValue2 = OwnerRow.RowFormat.Borders.Right.GetLineWidthValue();
		}
		else if (lineWidthValue2 < OwnerRow.RowFormat.Borders.Vertical.LineWidth)
		{
			lineWidthValue2 = OwnerRow.RowFormat.Borders.Vertical.GetLineWidthValue();
		}
		else if (OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle2 && Index == OwnerRow.Cells.Count - 1 && wTableStyle2.ConditionalFormattingStyles.Count == 0 && !OwnerRow.OwnerTable.TableFormat.Borders.Right.HasKey(3) && wTableStyle2.TableProperties.Borders.Right.HasKey(3) && lineWidthValue2 < wTableStyle2.TableProperties.Borders.Right.GetLineWidthValue())
		{
			lineWidthValue2 = wTableStyle2.TableProperties.Borders.Right.GetLineWidthValue();
		}
		return num + (lineWidthValue / 2f + lineWidthValue2 / 2f);
	}

	internal bool IsFitAsPerMaximumWordWidth(float width, float maxWordWidth)
	{
		WTable ownerTable = OwnerRow.OwnerTable;
		WSection wSection = GetOwnerSection(this) as WSection;
		if ((m_doc.ActualFormatType == FormatType.Docx || m_doc.ActualFormatType == FormatType.Word2013) && ownerTable != null && !ownerTable.IsInCell && !ownerTable.TableFormat.WrapTextAround && ownerTable.TableFormat.IsAutoResized && ownerTable.PreferredTableWidth.WidthType == FtsWidth.Point && ownerTable.PreferredTableWidth.Width > wSection.PageSetup.ClientWidth && GridSpan == 1 && SizeInfo.HasMaximumWordWidth && maxWordWidth > width && CellFormat.VerticalMerge == CellMerge.None)
		{
			return true;
		}
		if ((m_doc.ActualFormatType == FormatType.Docx || m_doc.ActualFormatType == FormatType.Word2013) && wSection != null && maxWordWidth < wSection.PageSetup.ClientWidth && ownerTable.Rows.Count == 1 && ownerTable.Rows[0].Cells.Count == 1 && ownerTable.PreferredTableWidth.WidthType == FtsWidth.Auto && PreferredWidth.WidthType == FtsWidth.Auto && ownerTable.TableFormat.IsAutoResized && !ownerTable.IsInCell && !ownerTable.TableFormat.WrapTextAround)
		{
			return true;
		}
		return false;
	}

	internal void SetHasPreferredWidth()
	{
		WTable ownerTable = OwnerRow.OwnerTable;
		if (ownerTable != null)
		{
			ownerTable.HasAutoPreferredCellWidth = (ownerTable.HasAutoPreferredCellWidth ? ownerTable.HasAutoPreferredCellWidth : (PreferredWidth.WidthType == FtsWidth.Auto));
			ownerTable.HasPointPreferredCellWidth = (ownerTable.HasPointPreferredCellWidth ? ownerTable.HasPointPreferredCellWidth : (PreferredWidth.WidthType == FtsWidth.Point));
			ownerTable.HasPercentPreferredCellWidth = (ownerTable.HasPercentPreferredCellWidth ? ownerTable.HasPercentPreferredCellWidth : (PreferredWidth.WidthType == FtsWidth.Percentage));
			ownerTable.HasNonePreferredCellWidth = (ownerTable.HasNonePreferredCellWidth ? ownerTable.HasNonePreferredCellWidth : (PreferredWidth.WidthType == FtsWidth.None));
		}
	}

	internal StringBuilder GetCellAsString()
	{
		char value = '\u0006';
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(value);
		stringBuilder.Append(GetAsString());
		stringBuilder.Append(value);
		return stringBuilder;
	}

	internal string GetWordComparisonText()
	{
		byte[] bytes = Encoding.UTF8.GetBytes(GetCellAsString().ToString());
		return base.Document.Comparison.ConvertBytesAsHash(bytes);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new CellLayoutInfo(this);
		CheckFootNoteInTextBody(this);
		if ((m_layoutInfo as CellLayoutInfo).IsColumnMergeContinue)
		{
			m_layoutInfo.IsSkip = true;
		}
		if ((m_layoutInfo as CellLayoutInfo).IsVerticalText)
		{
			m_layoutInfo.IsClipped = true;
		}
	}

	internal CellLayoutInfo CreateCellLayoutInfo()
	{
		CreateLayoutInfo();
		return m_layoutInfo as CellLayoutInfo;
	}

	internal void InitCellLayoutInfo()
	{
		if (m_layoutInfo is CellLayoutInfo)
		{
			(m_layoutInfo as CellLayoutInfo).InitLayoutInfo();
		}
		m_layoutInfo = null;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		if (m_layoutInfo is CellLayoutInfo)
		{
			(m_layoutInfo as CellLayoutInfo).InitLayoutInfo();
		}
		m_layoutInfo = null;
		base.InitLayoutInfo(entity, ref isLastTOCEntry);
		_ = isLastTOCEntry;
	}

	internal WTableRow GetOwnerRow(WTableCell ownerTableCell)
	{
		if ((((IWidget)ownerTableCell).LayoutInfo as CellLayoutInfo).IsRowMergeStart)
		{
			WTable ownerTable = ownerTableCell.OwnerRow.OwnerTable;
			float cellStartPosition = ownerTableCell.CellStartPosition;
			for (int i = ownerTableCell.OwnerRow.Index; i < ownerTable.Rows.Count; i++)
			{
				WTableRow wTableRow = ownerTable.Rows[i];
				float num = 0f;
				if (!(((IWidget)wTableRow).LayoutInfo as RowLayoutInfo).IsRowHasVerticalMergeEndCell)
				{
					continue;
				}
				for (int j = 0; j < wTableRow.Cells.Count; j++)
				{
					if ((((IWidget)wTableRow.Cells[j]).LayoutInfo as CellLayoutInfo).IsRowMergeEnd && Math.Round(num, 2) == Math.Round(cellStartPosition, 2))
					{
						return wTableRow.Cells[j].OwnerRow;
					}
					num += ownerTable.Rows[i].Cells[j].Width;
				}
			}
			return null;
		}
		return ownerTableCell.OwnerRow;
	}

	private void CheckFootNoteInTextBody(WTextBody textBody)
	{
		for (int i = 0; i < textBody.Items.Count; i++)
		{
			if (textBody.Items[i] is WParagraph)
			{
				CheckFootNoteInParagraph(textBody.Items[i] as WParagraph);
			}
			else
			{
				if (!(textBody.Items[i] is WTable))
				{
					continue;
				}
				WTable wTable = textBody.Items[i] as WTable;
				for (int j = 0; j < wTable.Rows.Count; j++)
				{
					WTableRow wTableRow = wTable.Rows[j];
					for (int k = 0; k < wTableRow.Cells.Count; k++)
					{
						CheckFootNoteInTextBody(wTableRow.Cells[k]);
					}
				}
			}
		}
	}

	private void CheckFootNoteInParagraph(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (!(paragraph.Items[i] is WFootnote))
			{
				continue;
			}
			_ = ((IWidget)paragraph.Items[i]).LayoutInfo;
			if (paragraph.Items[i] is WFootnote)
			{
				if ((paragraph.Items[i] as WFootnote).FootnoteType == FootnoteType.Endnote)
				{
					(m_layoutInfo as CellLayoutInfo).IsCellHasEndNote = true;
				}
				if ((paragraph.Items[i] as WFootnote).FootnoteType == FootnoteType.Footnote)
				{
					(m_layoutInfo as CellLayoutInfo).IsCellHasFootNote = true;
				}
			}
		}
	}

	void IWidget.InitLayoutInfo()
	{
		if (m_layoutInfo is CellLayoutInfo)
		{
			(m_layoutInfo as CellLayoutInfo).InitLayoutInfo();
		}
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	internal IWParagraph AddPrevParagraph()
	{
		WParagraph lastParagraph = m_doc.LastParagraph;
		int index = m_bodyItems.Add(lastParagraph);
		return m_bodyItems[index] as IWParagraph;
	}
}
