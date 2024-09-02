namespace DocGen.DocIO.DLS;

internal class PreferredWidthInfo
{
	private int m_widthTypeKey;

	private FormatBase m_ownerFormat;

	internal float Width
	{
		get
		{
			if (m_ownerFormat is RowFormat)
			{
				return (float)(m_ownerFormat as RowFormat).GetPropertyValue(m_widthTypeKey + 1);
			}
			return (float)(m_ownerFormat as CellFormat).GetPropertyValue(m_widthTypeKey + 1);
		}
		set
		{
			if (m_ownerFormat is RowFormat)
			{
				(m_ownerFormat as RowFormat).SetPropertyValue(m_widthTypeKey + 1, value);
			}
			else
			{
				(m_ownerFormat as CellFormat).SetPropertyValue(m_widthTypeKey + 1, value);
			}
		}
	}

	internal FtsWidth WidthType
	{
		get
		{
			if (m_ownerFormat is RowFormat)
			{
				return (FtsWidth)(m_ownerFormat as RowFormat).GetPropertyValue(m_widthTypeKey);
			}
			return (FtsWidth)(m_ownerFormat as CellFormat).GetPropertyValue(m_widthTypeKey);
		}
		set
		{
			if (m_ownerFormat is RowFormat)
			{
				(m_ownerFormat as RowFormat).SetPropertyValue(m_widthTypeKey, value);
				return;
			}
			(m_ownerFormat as CellFormat).SetPropertyValue(m_widthTypeKey, value);
			if (value == FtsWidth.Percentage && (m_ownerFormat as CellFormat).OwnerBase is WTableCell && ((m_ownerFormat as CellFormat).OwnerBase as WTableCell).Document.IsOpening && ((m_ownerFormat as CellFormat).OwnerBase as WTableCell).OwnerRow != null && ((m_ownerFormat as CellFormat).OwnerBase as WTableCell).OwnerRow.OwnerTable != null && ((m_ownerFormat as CellFormat).OwnerBase as WTableCell).OwnerRow.OwnerTable.PreferredTableWidth.WidthType == FtsWidth.None)
			{
				((m_ownerFormat as CellFormat).OwnerBase as WTableCell).OwnerRow.OwnerTable.IsTableCellWidthDefined = true;
			}
		}
	}

	internal void Close()
	{
		if (m_ownerFormat != null)
		{
			m_ownerFormat = null;
		}
	}

	internal PreferredWidthInfo(FormatBase ownerFormat, int key)
	{
		m_ownerFormat = ownerFormat;
		m_widthTypeKey = key;
	}

	internal bool Compare(PreferredWidthInfo preferredWidth)
	{
		if (preferredWidth.Width != Width)
		{
			return false;
		}
		if (WidthType != preferredWidth.WidthType)
		{
			return false;
		}
		return true;
	}
}
