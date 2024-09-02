using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class RowFormat : FormatBase
{
	public class TablePositioning : FormatBase
	{
		internal const int HorizPosKey = 62;

		internal const int VertPosKey = 63;

		internal const int HorizRelKey = 64;

		internal const int VertRelKey = 65;

		internal const int DistanceFromTopKey = 66;

		internal const int DistanceFromBottomKey = 67;

		internal const int DistanceFromLeftKey = 68;

		internal const int DistanceFromRightKey = 69;

		internal const int AllowOverlapKey = 70;

		internal const float DEF_HORIZ_DISTANCE = 0f;

		internal RowFormat m_ownerRowFormat;

		internal bool AllowOverlap
		{
			get
			{
				return (bool)GetPropertyValue(70);
			}
			set
			{
				SetPropertyValue(70, value);
			}
		}

		public HorizontalPosition HorizPositionAbs
		{
			get
			{
				if (HorizPosition == -4f)
				{
					return HorizontalPosition.Center;
				}
				if (HorizPosition == -8f)
				{
					return HorizontalPosition.Right;
				}
				if (HorizPosition == -12f)
				{
					return HorizontalPosition.Inside;
				}
				if (HorizPosition == -16f)
				{
					return HorizontalPosition.Outside;
				}
				return HorizontalPosition.Left;
			}
			set
			{
				HorizPosition = (float)value;
			}
		}

		public VerticalPosition VertPositionAbs
		{
			get
			{
				if (VertPosition == -4f)
				{
					return VerticalPosition.Top;
				}
				if (VertPosition == -8f)
				{
					return VerticalPosition.Center;
				}
				if (VertPosition == -12f)
				{
					return VerticalPosition.Bottom;
				}
				if (VertPosition == -16f)
				{
					return VerticalPosition.Inside;
				}
				if (VertPosition == -20f)
				{
					return VerticalPosition.Outside;
				}
				return VerticalPosition.None;
			}
			set
			{
				VertPosition = (float)value;
			}
		}

		public float HorizPosition
		{
			get
			{
				return (float)GetPropertyValue(62);
			}
			set
			{
				SetPropertyValue(62, value);
			}
		}

		public float VertPosition
		{
			get
			{
				return (float)GetPropertyValue(63);
			}
			set
			{
				SetPropertyValue(63, value);
			}
		}

		public HorizontalRelation HorizRelationTo
		{
			get
			{
				return (HorizontalRelation)GetPropertyValue(64);
			}
			set
			{
				SetPropertyValue(64, value);
			}
		}

		public VerticalRelation VertRelationTo
		{
			get
			{
				return (VerticalRelation)GetPropertyValue(65);
			}
			set
			{
				SetPropertyValue(65, value);
			}
		}

		public float DistanceFromTop
		{
			get
			{
				return (float)GetPropertyValue(66);
			}
			set
			{
				SetPropertyValue(66, value);
			}
		}

		public float DistanceFromBottom
		{
			get
			{
				return (float)GetPropertyValue(67);
			}
			set
			{
				SetPropertyValue(67, value);
			}
		}

		public float DistanceFromLeft
		{
			get
			{
				return (float)GetPropertyValue(68);
			}
			set
			{
				SetPropertyValue(68, value);
			}
		}

		public float DistanceFromRight
		{
			get
			{
				return (float)GetPropertyValue(69);
			}
			set
			{
				SetPropertyValue(69, value);
			}
		}

		internal TablePositioning(RowFormat ownerRowFormat)
		{
			m_ownerRowFormat = ownerRowFormat;
		}

		internal TablePositioning(FormatBase parent, int baseKey)
			: base(parent, baseKey)
		{
			m_ownerRowFormat = (RowFormat)parent;
		}

		internal object GetPropertyValue(int propertyKey)
		{
			return m_ownerRowFormat.GetPropertyValue(propertyKey);
		}

		private void SetPropertyValue(int propertyKey, object value)
		{
			m_ownerRowFormat.SetPropertyValue(propertyKey, value);
		}

		protected override object GetDefValue(int key)
		{
			return m_ownerRowFormat.GetDefValue(key);
		}

		internal bool Compare(TablePositioning tablePositioning)
		{
			if (!Compare(62, tablePositioning))
			{
				return false;
			}
			if (!Compare(63, tablePositioning))
			{
				return false;
			}
			if (!Compare(64, tablePositioning))
			{
				return false;
			}
			if (!Compare(68, tablePositioning))
			{
				return false;
			}
			if (!Compare(67, tablePositioning))
			{
				return false;
			}
			if (!Compare(69, tablePositioning))
			{
				return false;
			}
			if (!Compare(69, tablePositioning))
			{
				return false;
			}
			if (!Compare(70, tablePositioning))
			{
				return false;
			}
			return true;
		}
	}

	internal const int BordersKey = 1;

	internal const int RowHeightKey = 2;

	internal const int PaddingsKey = 3;

	internal const int PreferredWidthTypeKey = 11;

	internal const int PreferredWidthKey = 12;

	internal const int GridBeforeWidthTypeKey = 13;

	internal const int GridBeforeWidthKey = 14;

	internal const int GridAfterWidthTypeKey = 15;

	internal const int GridAfterWidthKey = 16;

	internal const int CellSpacingKey = 52;

	internal const int LeftIndentKey = 53;

	internal const int SpacingBetweenCellsKey = 102;

	internal const int IsAutoResizedCellsKey = 103;

	internal const int IsBreakAcrossPagesKey = 106;

	internal const int IsHeaderRowKey = 107;

	internal const int BidiTableKey = 104;

	internal const int RowAlignmentKey = 105;

	internal const int ShadingColorKey = 108;

	internal const int ForeColorKey = 111;

	internal const int TextureStyleKey = 110;

	internal const int PositioningKey = 120;

	internal const int DEF_BORDER_COUNT = 6;

	internal const int HiddenKey = 121;

	internal const int ChangedFormatKey = 122;

	internal const int FormatChangeAuthorNameKey = 123;

	internal const int FormatChangeDateTimeKey = 124;

	internal const int TableStyleNameKey = 125;

	private byte m_bFlags;

	private PreferredWidthInfo m_gridBeforeWidth;

	private PreferredWidthInfo m_gridAfterWidth;

	private PreferredWidthInfo m_preferredWidth;

	private List<Stream> m_xmlProps;

	internal float AfterWidth;

	internal float BeforeWidth;

	internal PreferredWidthInfo PreferredWidth
	{
		get
		{
			if (m_preferredWidth == null)
			{
				m_preferredWidth = new PreferredWidthInfo(this, 11);
			}
			return m_preferredWidth;
		}
	}

	internal PreferredWidthInfo GridBeforeWidth
	{
		get
		{
			if (m_gridBeforeWidth == null)
			{
				m_gridBeforeWidth = new PreferredWidthInfo(this, 13);
			}
			return m_gridBeforeWidth;
		}
	}

	internal PreferredWidthInfo GridAfterWidth
	{
		get
		{
			if (m_gridAfterWidth == null)
			{
				m_gridAfterWidth = new PreferredWidthInfo(this, 15);
			}
			return m_gridAfterWidth;
		}
	}

	internal short GridBefore
	{
		get
		{
			if (BeforeWidth > 0f && OwnerRow != null)
			{
				return GetGridCount(-1);
			}
			return -1;
		}
	}

	internal short GridAfter
	{
		get
		{
			if (AfterWidth > 0f && OwnerRow != null)
			{
				return GetGridCount(OwnerRow.Cells.Count);
			}
			return -1;
		}
	}

	internal bool Hidden
	{
		get
		{
			return (bool)GetPropertyValue(121);
		}
		set
		{
			SetPropertyValue(121, value);
		}
	}

	public Color BackColor
	{
		get
		{
			return (Color)GetPropertyValue(108);
		}
		set
		{
			SetPropertyValue(108, value);
			if (base.Document == null || base.Document.IsOpening)
			{
				return;
			}
			if (base.OwnerBase is WTableRow)
			{
				foreach (WTableCell cell in (base.OwnerBase as WTableRow).Cells)
				{
					cell.CellFormat.BackColor = value;
				}
				return;
			}
			if (!(base.OwnerBase is WTable))
			{
				return;
			}
			foreach (WTableRow row in (base.OwnerBase as WTable).Rows)
			{
				row.RowFormat.BackColor = value;
			}
		}
	}

	internal Color ForeColor
	{
		get
		{
			return (Color)GetPropertyValue(111);
		}
		set
		{
			SetPropertyValue(111, value);
		}
	}

	internal TextureStyle TextureStyle
	{
		get
		{
			return (TextureStyle)GetPropertyValue(110);
		}
		set
		{
			SetPropertyValue(110, value);
			if (base.Document == null || base.Document.IsOpening)
			{
				return;
			}
			if (base.OwnerBase is WTableRow)
			{
				foreach (WTableCell cell in (base.OwnerBase as WTableRow).Cells)
				{
					cell.CellFormat.TextureStyle = value;
				}
				return;
			}
			if (!(base.OwnerBase is WTable))
			{
				return;
			}
			foreach (WTableRow row in (base.OwnerBase as WTable).Rows)
			{
				row.RowFormat.TextureStyle = value;
			}
		}
	}

	public Borders Borders => GetPropertyValue(1) as Borders;

	public Paddings Paddings => GetPropertyValue(3) as Paddings;

	public float CellSpacing
	{
		get
		{
			return (float)GetPropertyValue(52);
		}
		set
		{
			SetPropertyValue(52, value);
		}
	}

	public float LeftIndent
	{
		get
		{
			return (float)GetPropertyValue(53);
		}
		set
		{
			if ((value < -1080f || value > 1080f || float.IsNaN(value)) && base.Document != null && !base.Document.IsOpening)
			{
				throw new ArgumentOutOfRangeException("LeftIndent", "Table Left Indent must be between -1080 pt and 1080 pt");
			}
			SetPropertyValue(53, value);
		}
	}

	public bool IsAutoResized
	{
		get
		{
			return (bool)GetPropertyValue(103);
		}
		set
		{
			SetPropertyValue(103, value);
		}
	}

	public bool IsBreakAcrossPages
	{
		get
		{
			return (bool)GetPropertyValue(106);
		}
		set
		{
			SetPropertyValue(106, value);
		}
	}

	internal bool IsHeaderRow
	{
		get
		{
			return (bool)GetPropertyValue(107);
		}
		set
		{
			SetPropertyValue(107, value);
		}
	}

	public bool Bidi
	{
		get
		{
			return (bool)GetPropertyValue(104);
		}
		set
		{
			SetPropertyValue(104, value);
		}
	}

	public RowAlignment HorizontalAlignment
	{
		get
		{
			return (RowAlignment)GetPropertyValue(105);
		}
		set
		{
			SetPropertyValue(105, value);
		}
	}

	internal bool SkipDefaultPadding
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			return (bool)GetPropertyValue(122);
		}
		set
		{
			SetPropertyValue(122, value);
		}
	}

	internal WTableRow OwnerRow => base.OwnerBase as WTableRow;

	internal float Height
	{
		get
		{
			return (float)GetPropertyValue(2);
		}
		set
		{
			SetPropertyValue(2, value);
		}
	}

	internal bool CancelOnChange
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		private set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public bool WrapTextAround
	{
		get
		{
			return GetTextWrapAround();
		}
		set
		{
			SetTextWrapAround(value);
		}
	}

	public TablePositioning Positioning => GetPropertyValue(120) as TablePositioning;

	internal List<Stream> XmlProps
	{
		get
		{
			if (m_xmlProps == null)
			{
				m_xmlProps = new List<Stream>();
			}
			return m_xmlProps;
		}
	}

	internal string FormatChangeAuthorName
	{
		get
		{
			return (string)GetPropertyValue(123);
		}
		set
		{
			SetPropertyValue(123, value);
		}
	}

	internal DateTime FormatChangeDateTime
	{
		get
		{
			return (DateTime)GetPropertyValue(124);
		}
		set
		{
			SetPropertyValue(124, value);
		}
	}

	internal bool IsLeftIndentDefined
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal string TableStyleName
	{
		get
		{
			return (string)GetPropertyValue(125);
		}
		set
		{
			SetPropertyValue(125, value);
		}
	}

	public RowFormat()
	{
		Paddings.SetOwner(this);
		Positioning.SetOwner(this);
	}

	internal RowFormat(IWordDocument doc)
		: base(doc)
	{
		Paddings.SetOwner(this);
		Positioning.SetOwner(this);
	}

	internal float GetCellSpacing()
	{
		float num = CellSpacing;
		if (num == -1f)
		{
			num = 0f;
		}
		return num;
	}

	internal short GetGridCount(int index)
	{
		if (OwnerRow == null || OwnerRow.OwnerTable == null)
		{
			return -1;
		}
		float num = 0f;
		float num2 = 0f;
		WTable ownerTable = OwnerRow.OwnerTable;
		WTableColumnCollection tableGrid = ownerTable.TableGrid;
		if (index == -1)
		{
			num2 = BeforeWidth * 20f;
		}
		else
		{
			num += BeforeWidth * 20f;
			if (index > 0)
			{
				num += GetCellOffset(index);
			}
			num2 = ((index >= OwnerRow.Cells.Count) ? (AfterWidth * 20f) : (OwnerRow.Cells[index].Width * 20f));
		}
		int num3 = ((num == 0f) ? GetOffsetIndex(tableGrid, num) : (GetOffsetIndex(tableGrid, num) + 1));
		int num4 = GetOffsetIndex(tableGrid, num + num2) + 1;
		if (index >= 0 && index < OwnerRow.Cells.Count)
		{
			OwnerRow.Cells[index].GridColumnStartIndex = (short)num3;
			if (m_doc.IsDOCX() && !m_doc.IsOpening && !m_doc.IsCloning && ownerTable.TableFormat.IsAutoResized && num4 - num3 > 1 && num3 == 0 && !OwnerRow.Cells[index].CellFormat.PropertiesHash.ContainsKey(13))
			{
				int num5 = ownerTable.MaxCellCountWithoutSpannedCells(ownerTable);
				if (num5 != 0 && num4 - num3 > num5)
				{
					return (short)num5;
				}
			}
		}
		return (short)(num4 - num3);
	}

	private int GetOffsetIndex(WTableColumnCollection tableGrid, float offset)
	{
		offset = (float)Math.Round(offset);
		int num = 0;
		if (tableGrid.Contains(offset))
		{
			return tableGrid.IndexOf(offset);
		}
		for (int i = 0; i < tableGrid.Count; i++)
		{
			if (tableGrid[i].EndOffset > offset)
			{
				return i;
			}
		}
		return tableGrid.Count - 1;
	}

	private float GetCellOffset(int index)
	{
		float num = 0f;
		int i = 0;
		for (int count = OwnerRow.Cells.Count; i < count && i != index; i++)
		{
			num += OwnerRow.Cells[i].Width * 20f;
		}
		return num;
	}

	private bool GetTextWrapAround()
	{
		if (HasValue(64))
		{
			return true;
		}
		if (HasValue(62))
		{
			return Positioning.HorizPosition != 0f;
		}
		if (HasValue(65))
		{
			if (Positioning.VertRelationTo == VerticalRelation.Paragraph)
			{
				return true;
			}
			if (HasValue(63))
			{
				return Positioning.VertPosition != 0f;
			}
		}
		else if (HasValue(63))
		{
			return Positioning.VertPosition != 0f;
		}
		return false;
	}

	private void SetTextWrapAround(bool value)
	{
		if (value)
		{
			Positioning.DistanceFromLeft = 9f;
			Positioning.DistanceFromRight = 9f;
			Positioning.VertRelationTo = VerticalRelation.Paragraph;
			Positioning.VertPosition = 0f;
		}
		else
		{
			ClearAbsolutePosition();
		}
	}

	private void ClearAbsolutePosition()
	{
		if (base.PropertiesHash.ContainsKey(68))
		{
			base.PropertiesHash.Remove(68);
		}
		if (base.PropertiesHash.ContainsKey(69))
		{
			base.PropertiesHash.Remove(69);
		}
		if (base.PropertiesHash.ContainsKey(66))
		{
			base.PropertiesHash.Remove(66);
		}
		if (base.PropertiesHash.ContainsKey(67))
		{
			base.PropertiesHash.Remove(67);
		}
		if (base.PropertiesHash.ContainsKey(64))
		{
			base.PropertiesHash.Remove(64);
		}
		if (base.PropertiesHash.ContainsKey(62))
		{
			base.PropertiesHash.Remove(62);
		}
		if (base.PropertiesHash.ContainsKey(65))
		{
			base.PropertiesHash.Remove(65);
		}
		if (base.PropertiesHash.ContainsKey(63))
		{
			base.PropertiesHash.Remove(63);
		}
	}

	internal object GetPropertyValue(int propertyKey)
	{
		return base[propertyKey];
	}

	internal void SetPropertyValue(int propertyKey, object value)
	{
		base[propertyKey] = value;
	}

	internal bool HasSprms()
	{
		return m_sprms != null;
	}

	private short GetRowIndent()
	{
		if (m_sprms == null)
		{
			return short.MinValue;
		}
		SinglePropertyModifierRecord singlePropertyModifierRecord = m_sprms[63073];
		if (singlePropertyModifierRecord == null || singlePropertyModifierRecord.ByteArray == null)
		{
			return short.MinValue;
		}
		if (singlePropertyModifierRecord.ByteArray.Length == 3)
		{
			return BitConverter.ToInt16(singlePropertyModifierRecord.ByteArray, 1);
		}
		return short.MinValue;
	}

	internal override bool HasValue(int propertyKey)
	{
		if (propertyKey != 1 && propertyKey != 3 && HasKey(propertyKey))
		{
			return true;
		}
		if (m_sprms == null || m_sprms.Count == 0)
		{
			return false;
		}
		int sprmOption = GetSprmOption(propertyKey);
		if (sprmOption == int.MaxValue)
		{
			return false;
		}
		if (m_sprms[sprmOption] == null)
		{
			return false;
		}
		return true;
	}

	internal override int GetSprmOption(int propertyKey)
	{
		switch (propertyKey)
		{
		case 106:
			return 13315;
		case 107:
			return 13316;
		case 121:
			return 54850;
		case 53:
			return 63073;
		case 11:
		case 12:
			return 62996;
		case 13:
		case 14:
			return 62999;
		case 15:
		case 16:
			return 63000;
		case 3:
			return 54836;
		case 2:
			return 37895;
		case 68:
			return 37904;
		case 69:
			return 37918;
		case 66:
			return 37905;
		case 67:
			return 37919;
		case 62:
			return 37902;
		case 63:
			return 37903;
		case 64:
		case 65:
			return 13837;
		default:
			return int.MaxValue;
		}
	}

	internal override void AcceptChanges()
	{
		if (m_sprms != null && m_sprms.Length > 0)
		{
			m_sprms.RemoveValue(54887);
			base.AcceptChanges();
		}
	}

	internal float UpdateRowBeforeAfterWidth(short gridSpan, bool isAfterWidth)
	{
		if (OwnerRow == null || OwnerRow.OwnerTable == null || OwnerRow.OwnerTable.TableGrid == null || OwnerRow.OwnerTable.TableGrid.Count == 0)
		{
			return 0f;
		}
		WTableColumnCollection tableGrid = OwnerRow.OwnerTable.TableGrid;
		float num = 0f;
		int num2 = (isAfterWidth ? (tableGrid.Count - gridSpan) : 0);
		int num3 = num2 + gridSpan;
		bool num4;
		if (!(num3 >= 0 && num2 >= 0 && isAfterWidth))
		{
			if (num3 >= tableGrid.Count)
			{
				goto IL_00e9;
			}
			num4 = num2 - 1 < tableGrid.Count;
		}
		else
		{
			num4 = num3 <= tableGrid.Count;
		}
		if (num4)
		{
			num = ((num2 != 0) ? (tableGrid[num3 - 1].EndOffset - tableGrid[num2 - 1].EndOffset) : ((num3 == 0) ? tableGrid[num3].EndOffset : tableGrid[num3 - 1].EndOffset));
		}
		goto IL_00e9;
		IL_00e9:
		return num / 20f;
	}

	internal void ClearPreferredWidthPropertyValue(int key)
	{
		if (m_propertiesHash.ContainsKey(key))
		{
			PreferredWidth.Width = 0f;
			PreferredWidth.WidthType = FtsWidth.None;
			m_propertiesHash.Remove(key);
		}
	}

	internal bool Compare(RowFormat rowFormat)
	{
		if (rowFormat == null)
		{
			return false;
		}
		if (!Compare(2, rowFormat))
		{
			return false;
		}
		if (!Compare(11, rowFormat))
		{
			return false;
		}
		if (!Compare(12, rowFormat))
		{
			return false;
		}
		if (!Compare(14, rowFormat))
		{
			return false;
		}
		if (!Compare(13, rowFormat))
		{
			return false;
		}
		if (!Compare(15, rowFormat))
		{
			return false;
		}
		if (!Compare(16, rowFormat))
		{
			return false;
		}
		if (!Compare(52, rowFormat))
		{
			return false;
		}
		if (!Compare(53, rowFormat))
		{
			return false;
		}
		if (!Compare(102, rowFormat))
		{
			return false;
		}
		if (!Compare(103, rowFormat))
		{
			return false;
		}
		if (!Compare(106, rowFormat))
		{
			return false;
		}
		if (!Compare(107, rowFormat))
		{
			return false;
		}
		if (!Compare(104, rowFormat))
		{
			return false;
		}
		if (!Compare(105, rowFormat))
		{
			return false;
		}
		if (!Compare(108, rowFormat))
		{
			return false;
		}
		if (!Compare(111, rowFormat))
		{
			return false;
		}
		if (!Compare(110, rowFormat))
		{
			return false;
		}
		if (!Compare(121, rowFormat))
		{
			return false;
		}
		if (Positioning != null && rowFormat.Positioning != null && !Positioning.Compare(rowFormat.Positioning))
		{
			return false;
		}
		if (Borders != null && rowFormat.Borders != null && !Borders.Compare(rowFormat.Borders))
		{
			return false;
		}
		if (Paddings != null && rowFormat.Paddings != null && !Paddings.Compare(rowFormat.Paddings))
		{
			return false;
		}
		return true;
	}

	internal override void ApplyBase(FormatBase baseFormat)
	{
		base.ApplyBase(baseFormat);
		Borders.ApplyBase((baseFormat as RowFormat).Borders);
		Paddings.ApplyBase((baseFormat as RowFormat).Paddings);
		Positioning.ApplyBase((baseFormat as RowFormat).Positioning);
	}

	protected internal override void EnsureComposites()
	{
		EnsureComposites(1);
		EnsureComposites(3);
		EnsureComposites(120);
	}

	protected override FormatBase GetDefComposite(int key)
	{
		return key switch
		{
			1 => GetDefComposite(1, new Borders(this, 1)), 
			3 => GetDefComposite(3, new Paddings(this, 3)), 
			120 => GetDefComposite(120, new TablePositioning(this, 120)), 
			_ => null, 
		};
	}

	protected override object GetDefValue(int key)
	{
		switch (key)
		{
		case 52:
			return -1f;
		case 53:
			return 0f;
		case 102:
			return 0f;
		case 104:
		case 107:
		case 121:
			return false;
		case 103:
			return false;
		case 70:
		case 106:
			return true;
		case 105:
		{
			bool flag = base.PropertiesHash.ContainsKey(62);
			if (flag && Positioning.HorizPosition == -4f)
			{
				return RowAlignment.Center;
			}
			if (flag && Positioning.HorizPosition == -8f)
			{
				return RowAlignment.Right;
			}
			return RowAlignment.Left;
		}
		case 108:
		case 111:
			return Color.Empty;
		case 2:
		case 12:
		case 14:
		case 16:
			return 0f;
		case 11:
		case 13:
		case 15:
			return FtsWidth.None;
		case 62:
		case 63:
		case 66:
		case 67:
			return 0f;
		case 68:
		case 69:
			return 0f;
		case 64:
			return HorizontalRelation.Column;
		case 65:
			return VerticalRelation.Margin;
		case 110:
			return TextureStyle.TextureNone;
		case 123:
			return string.Empty;
		case 124:
			return DateTime.MinValue;
		case 122:
			return false;
		default:
			throw new NotImplementedException();
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("CellSpacing"))
		{
			CellSpacing = reader.ReadFloat("CellSpacing");
		}
		if (reader.HasAttribute("LeftOffset"))
		{
			LeftIndent = reader.ReadFloat("LeftOffset");
		}
		if (reader.HasAttribute("HAlignment"))
		{
			HorizontalAlignment = (RowAlignment)(object)reader.ReadEnum("HAlignment", typeof(RowAlignment));
		}
		if (reader.HasAttribute("IsAutoResized"))
		{
			IsAutoResized = reader.ReadBoolean("IsAutoResized");
		}
		if (reader.HasAttribute("IsBreakAcrossPages"))
		{
			IsBreakAcrossPages = reader.ReadBoolean("IsBreakAcrossPages");
		}
		if (reader.HasAttribute("BidiTable"))
		{
			Bidi = reader.ReadBoolean("BidiTable");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		if (m_sprms == null)
		{
			base.WriteXmlAttributes(writer);
			if (CellSpacing != -1f)
			{
				writer.WriteValue("CellSpacing", CellSpacing);
			}
			if (LeftIndent != 0f)
			{
				writer.WriteValue("LeftOffset", LeftIndent);
			}
			if (HorizontalAlignment != 0)
			{
				writer.WriteValue("HAlignment", HorizontalAlignment);
			}
			if (IsAutoResized)
			{
				writer.WriteValue("IsAutoResized", IsAutoResized);
			}
			if (IsBreakAcrossPages)
			{
				writer.WriteValue("IsBreakAcrossPages", IsBreakAcrossPages);
			}
			if (Bidi)
			{
				writer.WriteValue("BidiTable", Bidi);
			}
		}
	}

	protected override void InitXDLSHolder()
	{
		if (m_sprms == null)
		{
			base.XDLSHolder.AddElement("borders", Borders);
			base.XDLSHolder.AddElement("Paddings", Paddings);
		}
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		if (m_sprms != null)
		{
			byte[] array = new byte[m_sprms.Length];
			m_sprms.Save(array, 0);
			writer.WriteChildBinaryElement("internal-data", array);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		bool result = base.ReadXmlContent(reader);
		if (reader.TagName == "internal-data")
		{
			TablePropertiesConverter.SprmsToFormat(new SinglePropertyModifierArray(reader.ReadChildBinaryElement()), this, null, null, null, isNewPropertyHash: false);
			result = true;
		}
		return result;
	}

	protected override void OnChange(FormatBase format, int propKey)
	{
		if (!CancelOnChange && (base.OwnerBase == null || !base.OwnerBase.Document.IsOpening) && base.OwnerBase != null && base.OwnerBase is WTable)
		{
			(base.OwnerBase as WTable).UpdateFormat(format, propKey);
		}
	}

	internal void RemoveRowSprms()
	{
		if (m_sprms != null)
		{
			m_sprms.RemoveValue(54792);
			m_sprms.RemoveValue(54896);
			m_sprms.RemoveValue(54802);
			m_sprms.RemoveValue(54834);
			m_sprms.RemoveValue(54810);
			m_sprms.RemoveValue(54811);
			m_sprms.RemoveValue(54812);
			m_sprms.RemoveValue(54813);
		}
	}

	protected internal new void ImportContainer(FormatBase format)
	{
		base.ImportContainer(format);
	}

	private void ImportXmlProps(RowFormat format)
	{
		if (format.m_xmlProps == null || format.m_xmlProps.Count <= 0)
		{
			return;
		}
		foreach (Stream xmlProp in format.XmlProps)
		{
			XmlProps.Add(CloneStream(xmlProp));
		}
	}

	protected override void ImportMembers(FormatBase format)
	{
		base.ImportMembers(format);
		if (format is RowFormat rowFormat)
		{
			if (rowFormat.GridAfter != -1)
			{
				AfterWidth = rowFormat.UpdateRowBeforeAfterWidth(rowFormat.GridAfter, isAfterWidth: true);
			}
			if (rowFormat.GridBefore != -1)
			{
				BeforeWidth = rowFormat.UpdateRowBeforeAfterWidth(rowFormat.GridBefore, isAfterWidth: false);
			}
			SkipDefaultPadding = rowFormat.SkipDefaultPadding;
			CopyProperties(rowFormat);
			EnsureComposites();
			base.IsDefault = false;
		}
	}

	internal override void RemovePositioning()
	{
		if (m_sprms != null && m_sprms.Count > 0)
		{
			m_sprms.RemoveValue(13837);
			m_sprms.RemoveValue(37902);
			m_sprms.RemoveValue(37903);
			m_sprms.RemoveValue(37919);
			m_sprms.RemoveValue(37904);
			m_sprms.RemoveValue(37918);
			m_sprms.RemoveValue(37905);
		}
	}

	internal override void Close()
	{
		base.Close();
		if (Borders != null)
		{
			Borders.Close();
		}
		if (Paddings != null)
		{
			Paddings.Close();
		}
		if (m_gridBeforeWidth != null)
		{
			m_gridBeforeWidth.Close();
			m_gridBeforeWidth = null;
		}
		if (m_gridAfterWidth != null)
		{
			m_gridAfterWidth.Close();
			m_gridAfterWidth = null;
		}
		if (m_preferredWidth != null)
		{
			m_preferredWidth.Close();
			m_preferredWidth = null;
		}
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
			m_xmlProps = null;
		}
	}

	internal void CheckDefPadding()
	{
		if (!Paddings.HasKey(1))
		{
			Paddings.Left = 5.4f;
		}
		if (!Paddings.HasKey(4))
		{
			Paddings.Right = 5.4f;
		}
	}
}
