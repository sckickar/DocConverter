using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class CellFormat : FormatBase
{
	internal const int BordersKey = 1;

	internal const int VrAlignmentKey = 2;

	internal const int PaddingsKey = 3;

	internal const int ShadingColorKey = 4;

	internal const int ForeColorKey = 5;

	internal const int VerticalMergeKey = 6;

	internal const int TextureStyleKey = 7;

	internal const int HorizontalMergeKey = 8;

	internal const int TextWrapKey = 9;

	internal const int FitTextKey = 10;

	internal const int TextDirectionKey = 11;

	internal const int CellWidthKey = 12;

	internal const int PreferredWidthTypeKey = 13;

	internal const int PreferredWidthKey = 14;

	internal const int FormatChangeAuthorNameKey = 15;

	internal const int FormatChangeDateTimeKey = 16;

	internal const int CellGridSpanKey = 17;

	private RowFormat m_ownerRowFormat;

	private byte m_bFlags = 2;

	private byte m_bFlags1 = 2;

	private PreferredWidthInfo m_preferredWidth;

	private List<Stream> m_xmlProps;

	private bool CancelOnChange
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

	internal bool Hidden
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

	internal bool IsChangedFormat
	{
		get
		{
			return (m_bFlags1 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal PreferredWidthInfo PreferredWidth
	{
		get
		{
			if (m_preferredWidth == null)
			{
				m_preferredWidth = new PreferredWidthInfo(this, 13);
			}
			return m_preferredWidth;
		}
	}

	public Borders Borders
	{
		get
		{
			return GetPropertyValue(1) as Borders;
		}
		internal set
		{
			SetPropertyValue(1, value);
		}
	}

	public Paddings Paddings => GetPropertyValue(3) as Paddings;

	public VerticalAlignment VerticalAlignment
	{
		get
		{
			return (VerticalAlignment)GetPropertyValue(2);
		}
		set
		{
			SetPropertyValue(2, value);
		}
	}

	public Color BackColor
	{
		get
		{
			return (Color)GetPropertyValue(4);
		}
		set
		{
			SetPropertyValue(4, value);
		}
	}

	public CellMerge VerticalMerge
	{
		get
		{
			return (CellMerge)GetPropertyValue(6);
		}
		set
		{
			SetPropertyValue(6, value);
		}
	}

	public CellMerge HorizontalMerge
	{
		get
		{
			CellMerge horizMerge = (CellMerge)GetPropertyValue(8);
			if (horizMerge == CellMerge.Continue)
			{
				UpdateHorizontalMerge(ref horizMerge);
			}
			return horizMerge;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && value == CellMerge.Start)
			{
				PreferredWidth.WidthType = FtsWidth.None;
				PreferredWidth.Width = 0f;
			}
			SetPropertyValue(8, value);
		}
	}

	public bool TextWrap
	{
		get
		{
			return (bool)GetPropertyValue(9);
		}
		set
		{
			SetPropertyValue(9, value);
		}
	}

	public bool FitText
	{
		get
		{
			return (bool)GetPropertyValue(10);
		}
		set
		{
			SetPropertyValue(10, value);
		}
	}

	public TextDirection TextDirection
	{
		get
		{
			return (TextDirection)GetPropertyValue(11);
		}
		set
		{
			SetPropertyValue(11, value);
		}
	}

	public bool SamePaddingsAsTable
	{
		get
		{
			return HasSamePaddingsAsTable();
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
			if (!value && OwnerRowFormat != null)
			{
				Paddings.ImportContainer(OwnerRowFormat.Paddings);
			}
		}
	}

	internal RowFormat OwnerRowFormat => GetOwnerRowFormatValue();

	internal int CurCellIndex => GetOwnerCellIndex();

	internal float CellWidth
	{
		get
		{
			return (float)GetPropertyValue(12);
		}
		set
		{
			SetPropertyValue(12, value);
		}
	}

	internal short CellGridSpan
	{
		get
		{
			return (short)GetPropertyValue(17);
		}
		set
		{
			SetPropertyValue(17, value);
		}
	}

	internal Color ForeColor
	{
		get
		{
			return (Color)GetPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
		}
	}

	internal TextureStyle TextureStyle
	{
		get
		{
			return (TextureStyle)GetPropertyValue(7);
		}
		set
		{
			SetPropertyValue(7, value);
		}
	}

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

	internal bool HideMark
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal string FormatChangeAuthorName
	{
		get
		{
			return (string)GetPropertyValue(15);
		}
		set
		{
			SetPropertyValue(15, value);
		}
	}

	internal DateTime FormatChangeDateTime
	{
		get
		{
			return (DateTime)GetPropertyValue(16);
		}
		set
		{
			SetPropertyValue(16, value);
		}
	}

	public CellFormat()
	{
		Borders.SetOwner(this);
	}

	internal object GetPropertyValue(int propertyKey)
	{
		return base[propertyKey];
	}

	internal void SetPropertyValue(int propertyKey, object value)
	{
		base[propertyKey] = value;
	}

	private RowFormat GetOwnerRowFormatValue()
	{
		if (m_ownerRowFormat != null)
		{
			return m_ownerRowFormat;
		}
		if (!(base.OwnerBase is WTableCell wTableCell))
		{
			return null;
		}
		if (!(wTableCell.Owner is WTableRow wTableRow))
		{
			return null;
		}
		m_ownerRowFormat = wTableRow.RowFormat;
		return m_ownerRowFormat;
	}

	private int GetOwnerCellIndex()
	{
		if (base.OwnerBase is WTableCell wTableCell)
		{
			return wTableCell.GetCellIndex();
		}
		return -1;
	}

	private bool HasSamePaddingsAsTable()
	{
		_ = (m_bFlags & 2) >> 1;
		return (m_bFlags & 2) >> 1 != 0;
	}

	private void UpdateHorizontalMerge(ref CellMerge horizMerge)
	{
		if ((CurCellIndex > 0 && ((WTableCell)base.OwnerBase).OwnerRow.Cells[CurCellIndex - 1].CellFormat.HorizontalMerge == CellMerge.None) || CurCellIndex == 0)
		{
			HorizontalMerge = (horizMerge = CellMerge.None);
		}
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

	internal override void Close()
	{
		m_ownerRowFormat = null;
		if (m_preferredWidth != null)
		{
			m_preferredWidth.Close();
			m_preferredWidth = null;
		}
		if (Borders != null)
		{
			Borders.Close();
		}
		if (Paddings != null)
		{
			Paddings.Close();
		}
		base.Close();
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
			m_xmlProps = null;
		}
	}

	internal override bool HasValue(int propertyKey)
	{
		if (HasKey(propertyKey))
		{
			return true;
		}
		return false;
	}

	internal override void ApplyBase(FormatBase baseFormat)
	{
		base.ApplyBase(baseFormat);
		Borders.ApplyBase((baseFormat as CellFormat).Borders);
		Paddings.ApplyBase((baseFormat as CellFormat).Paddings);
	}

	protected internal override void EnsureComposites()
	{
		EnsureComposites(1);
		EnsureComposites(3);
	}

	protected override object GetDefValue(int key)
	{
		switch (key)
		{
		case 1:
		case 3:
			return GetDefComposite(key);
		case 12:
		case 14:
			return 0f;
		case 13:
			return FtsWidth.None;
		case 2:
			return VerticalAlignment.Top;
		case 6:
			return CellMerge.None;
		case 8:
			return CellMerge.None;
		case 4:
			return Color.Empty;
		case 9:
			return true;
		case 10:
			return false;
		case 5:
			return Color.Empty;
		case 7:
			return TextureStyle.TextureNone;
		case 11:
			return TextDirection.Horizontal;
		case 15:
			return string.Empty;
		case 16:
			return DateTime.MinValue;
		case 17:
			return (short)1;
		default:
			throw new NotImplementedException();
		}
	}

	protected override FormatBase GetDefComposite(int key)
	{
		return key switch
		{
			1 => GetDefComposite(1, new Borders(this, 1)), 
			3 => GetDefComposite(3, new Paddings(this, 3)), 
			_ => null, 
		};
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (HasKey(9))
		{
			writer.WriteValue("TextWrap", TextWrap);
		}
		writer.WriteValue("SamePaddingsAsTable", (m_bFlags & 2) >> 1 != 0);
		if (!OwnerRowFormat.HasSprms())
		{
			if (VerticalAlignment != 0)
			{
				writer.WriteValue("VAlignment", VerticalAlignment);
			}
			if (VerticalMerge != 0)
			{
				writer.WriteValue("VMerge", VerticalMerge);
			}
			if (HorizontalMerge != 0)
			{
				writer.WriteValue("HMerge", HorizontalMerge);
			}
			if (BackColor != Color.Empty)
			{
				writer.WriteValue("ShadingColor", BackColor);
			}
			if (FitText)
			{
				writer.WriteValue("FitText", FitText);
			}
			if (TextDirection != 0)
			{
				writer.WriteValue("TextDirection", TextDirection);
			}
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("VAlignment"))
		{
			VerticalAlignment = (VerticalAlignment)(object)reader.ReadEnum("VAlignment", typeof(VerticalAlignment));
		}
		if (reader.HasAttribute("VMerge"))
		{
			VerticalMerge = (CellMerge)(object)reader.ReadEnum("VMerge", typeof(CellMerge));
		}
		if (reader.HasAttribute("HMerge"))
		{
			HorizontalMerge = (CellMerge)(object)reader.ReadEnum("HMerge", typeof(CellMerge));
		}
		if (reader.HasAttribute("ShadingColor"))
		{
			BackColor = reader.ReadColor("ShadingColor");
		}
		if (reader.HasAttribute("TextWrap"))
		{
			TextWrap = reader.ReadBoolean("TextWrap");
		}
		if (reader.HasAttribute("SamePaddingsAsTable"))
		{
			SamePaddingsAsTable = reader.ReadBoolean("SamePaddingsAsTable");
		}
		if (reader.HasAttribute("FitText"))
		{
			FitText = reader.ReadBoolean("FitText");
		}
		if (reader.HasAttribute("TextDirection"))
		{
			TextDirection = (TextDirection)(object)reader.ReadEnum("TextDirection", typeof(TextDirection));
		}
	}

	protected override void InitXDLSHolder()
	{
		if (!OwnerRowFormat.HasSprms())
		{
			base.XDLSHolder.AddElement("borders", Borders);
			base.XDLSHolder.AddElement("Paddings", Paddings);
		}
	}

	protected internal new void ImportContainer(FormatBase format)
	{
		base.ImportContainer(format);
		if (format is CellFormat cellFormat)
		{
			HideMark = cellFormat.HideMark;
		}
	}

	private void ImportXmlProps(CellFormat format)
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
		if (format is CellFormat)
		{
			Borders.SetOwner(this);
			SamePaddingsAsTable = ((CellFormat)format).SamePaddingsAsTable;
			CellWidth = ((CellFormat)format).CellWidth;
		}
		else
		{
			ApplyParentRowFormat(format as RowFormat);
		}
	}

	protected override void OnChange(FormatBase format, int propKey)
	{
		if (!CancelOnChange && (base.OwnerBase == null || !base.OwnerBase.Document.IsOpening))
		{
			int num = int.MinValue;
			if (format is Borders || format is Border)
			{
				num = 1;
			}
			else if (format is Paddings)
			{
				num = 3;
			}
			_ = int.MinValue;
		}
	}

	internal void UpdateCellFormat(TableStyleCellProperties cellProperties)
	{
		if (cellProperties.HasValue(4))
		{
			base[4] = cellProperties.BackColor;
		}
		if (cellProperties.HasValue(5))
		{
			base[5] = cellProperties.ForeColor;
		}
		if (cellProperties.HasValue(7))
		{
			base[7] = cellProperties.TextureStyle;
		}
		if (cellProperties.HasValue(9))
		{
			base[9] = cellProperties.TextWrap;
		}
		if (cellProperties.HasValue(2))
		{
			base[2] = cellProperties.VerticalAlignment;
		}
		if (cellProperties.BaseFormat != null)
		{
			base.BaseFormat = cellProperties.BaseFormat;
		}
		Paddings.UpdatePaddings(cellProperties.Paddings);
	}

	private void ApplyParentRowFormat(RowFormat rowFormat)
	{
		BackColor = rowFormat.BackColor;
		ImportBorderSettings(rowFormat.Borders);
	}

	private void ImportBorderSettings(Borders borders)
	{
		Borders.Left.BorderType = borders.Left.BorderType;
		Borders.Left.Color = borders.Left.Color;
		Borders.Left.IsDefault = borders.Left.IsDefault;
		Borders.Left.LineWidth = borders.Left.LineWidth;
		Borders.Left.Shadow = borders.Left.Shadow;
		Borders.Left.Space = borders.Left.Space;
		Borders.Right.BorderType = borders.Right.BorderType;
		Borders.Right.Color = borders.Right.Color;
		Borders.Right.IsDefault = borders.Right.IsDefault;
		Borders.Right.LineWidth = borders.Right.LineWidth;
		Borders.Right.Shadow = borders.Right.Shadow;
		Borders.Right.Space = borders.Right.Space;
		Borders.Top.BorderType = borders.Top.BorderType;
		Borders.Top.Color = borders.Top.Color;
		Borders.Top.IsDefault = borders.Top.IsDefault;
		Borders.Top.LineWidth = borders.Top.LineWidth;
		Borders.Top.Shadow = borders.Top.Shadow;
		Borders.Top.Space = borders.Top.Space;
		Borders.Bottom.BorderType = borders.Bottom.BorderType;
		Borders.Bottom.Color = borders.Bottom.Color;
		Borders.Bottom.IsDefault = borders.Bottom.IsDefault;
		Borders.Bottom.LineWidth = borders.Bottom.LineWidth;
		Borders.Bottom.Shadow = borders.Bottom.Shadow;
		Borders.Bottom.Space = borders.Bottom.Space;
	}

	internal bool Compare(CellFormat cellFormat)
	{
		if (cellFormat == null)
		{
			return false;
		}
		if (!Compare(2, cellFormat))
		{
			return false;
		}
		if (!Compare(5, cellFormat))
		{
			return false;
		}
		if (!Compare(4, cellFormat))
		{
			return false;
		}
		if (!Compare(6, cellFormat))
		{
			return false;
		}
		if (!Compare(7, cellFormat))
		{
			return false;
		}
		if (!Compare(8, cellFormat))
		{
			return false;
		}
		if (!Compare(9, cellFormat))
		{
			return false;
		}
		if (!Compare(10, cellFormat))
		{
			return false;
		}
		if (!Compare(11, cellFormat))
		{
			return false;
		}
		if (!Compare(14, cellFormat))
		{
			return false;
		}
		if (!Compare(13, cellFormat))
		{
			return false;
		}
		if (!Compare(17, cellFormat))
		{
			return false;
		}
		if (Borders != null && cellFormat.Borders != null && !Borders.Compare(cellFormat.Borders))
		{
			return false;
		}
		if (Paddings != null && cellFormat.Paddings != null && !Paddings.Compare(cellFormat.Paddings))
		{
			return false;
		}
		return true;
	}
}
