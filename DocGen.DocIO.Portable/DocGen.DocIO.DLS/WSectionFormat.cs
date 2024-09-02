using System;

namespace DocGen.DocIO.DLS;

internal class WSectionFormat : FormatBase
{
	private const float DEF_DISTANCE_BETWEEN_COLUMNS = 36f;

	internal const int BreakcodeKey = 1;

	internal const int TextDirectionKey = 2;

	internal const int PageSetupKey = 3;

	internal const int ChangedFormatKey = 4;

	internal const int FormatChangeAuthorNameKey = 5;

	internal const int FormatChangeDateTimeKey = 6;

	internal ColumnCollection m_columns;

	internal ColumnCollection m_sectFormattingColumnCollection;

	internal ColumnCollection SectFormattingColumnCollection
	{
		get
		{
			return m_sectFormattingColumnCollection;
		}
		set
		{
			m_sectFormattingColumnCollection = value;
		}
	}

	internal WPageSetup PageSetup
	{
		get
		{
			return (WPageSetup)GetPropertyValue(3);
		}
		set
		{
			SetPropertyValue(3, value);
		}
	}

	internal SectionBreakCode BreakCode
	{
		get
		{
			return (SectionBreakCode)GetPropertyValue(1);
		}
		set
		{
			SetPropertyValue(1, value);
		}
	}

	internal ColumnCollection Columns => m_columns;

	internal DocTextDirection TextDirection
	{
		get
		{
			return (DocTextDirection)GetPropertyValue(2);
		}
		set
		{
			SetPropertyValue(2, value);
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			return (bool)GetPropertyValue(4);
		}
		set
		{
			if (value)
			{
				SetPropertyValue(4, true);
			}
		}
	}

	internal string FormatChangeAuthorName
	{
		get
		{
			return (string)GetPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
		}
	}

	internal DateTime FormatChangeDateTime
	{
		get
		{
			return (DateTime)GetPropertyValue(6);
		}
		set
		{
			SetPropertyValue(6, value);
		}
	}

	internal WSectionFormat(WSection section)
		: base(section.Document, section)
	{
	}

	internal WSectionFormat Clone()
	{
		WSectionFormat wSectionFormat = new WSectionFormat(base.OwnerBase as WSection);
		wSectionFormat.ImportContainer(this);
		wSectionFormat.CopyProperties(this);
		return wSectionFormat;
	}

	internal object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		base[propKey] = value;
	}

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			1 => SectionBreakCode.NewPage, 
			2 => DocTextDirection.LeftToRight, 
			3 => new WPageSetup(base.OwnerBase as WSection), 
			4 => false, 
			5 => string.Empty, 
			6 => DateTime.MinValue, 
			_ => throw new ArgumentException("key not found"), 
		};
	}

	protected override FormatBase GetDefComposite(int key)
	{
		if (key == 3)
		{
			return GetDefComposite(3, new WPageSetup(base.OwnerBase as WSection));
		}
		return null;
	}

	internal bool Compare(WSectionFormat sectionFormat)
	{
		if (!Compare(1, sectionFormat))
		{
			return false;
		}
		if (!Compare(2, sectionFormat))
		{
			return false;
		}
		if (PageSetup != null && sectionFormat.PageSetup != null && !PageSetup.Compare(sectionFormat.PageSetup))
		{
			return false;
		}
		if (Columns.Count != sectionFormat.Columns.Count)
		{
			return false;
		}
		if (Columns.Count > 0 && sectionFormat.Columns.Count > 0)
		{
			for (int i = 0; i < sectionFormat.Columns.Count; i++)
			{
				if (Columns[i].Compare(sectionFormat.Columns[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal override void Close()
	{
		if (PageSetup != null)
		{
			PageSetup.Close();
			PageSetup = null;
		}
		if (Columns != null)
		{
			Columns.Close();
			m_columns = null;
		}
		base.Close();
	}
}
