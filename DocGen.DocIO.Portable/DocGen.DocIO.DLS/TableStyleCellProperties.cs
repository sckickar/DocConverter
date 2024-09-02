using System;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class TableStyleCellProperties : FormatBase
{
	internal const int BordersKey = 1;

	internal const int PaddingsKey = 3;

	internal const int TextWrapKey = 9;

	internal const int VerticalAlignmentKey = 2;

	internal const int ShadingColorKey = 4;

	internal const int ForeColorKey = 5;

	internal const int TextureStyleKey = 7;

	internal const int TextDirectionKey = 11;

	internal const int HorizontalMergeKey = 8;

	internal const int VerticalMergeKey = 6;

	internal const int PreferredWidthTypeKey = 13;

	internal const int PreferredWidthKey = 14;

	internal const int CellWidthKey = 12;

	internal const int FitTextKey = 10;

	internal const int FormatChangeAuthorNameKey = 15;

	internal const int FormatChangeDateTimeKey = 16;

	internal const int CellGridSpanKey = 17;

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

	public Color ForeColor
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

	public TextureStyle TextureStyle
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

	public Borders Borders => GetPropertyValue(1) as Borders;

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

	public bool TextWrap
	{
		get
		{
			return (bool)base[9];
		}
		set
		{
			base[9] = value;
		}
	}

	internal TableStyleCellProperties(IWordDocument doc)
		: base(doc)
	{
	}

	internal object GetPropertyValue(int propertyKey)
	{
		return base[propertyKey];
	}

	internal void SetPropertyValue(int propertyKey, object value)
	{
		base[propertyKey] = value;
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
		Borders.ApplyBase((baseFormat as TableStyleCellProperties).Borders);
		Paddings.ApplyBase((baseFormat as TableStyleCellProperties).Paddings);
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
		case 9:
			return true;
		case 2:
			return VerticalAlignment.Top;
		case 4:
			return Color.Empty;
		case 5:
			return Color.Empty;
		case 7:
			return TextureStyle.TextureNone;
		case 1:
		case 3:
			return GetDefComposite(key);
		case 11:
			return TextDirection.Horizontal;
		case 8:
			return CellMerge.None;
		case 6:
			return CellMerge.None;
		case 13:
			return FtsWidth.None;
		case 12:
		case 14:
			return 0f;
		case 10:
			return false;
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

	internal bool Compare(TableStyleCellProperties tableStyleproeprties)
	{
		if (!Compare(4, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(7, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(11, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(8, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(6, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(14, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(13, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(12, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(10, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(17, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(ForeColor, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(ForeColor, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(BackColor, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(2, tableStyleproeprties))
		{
			return false;
		}
		if (!Compare(9, tableStyleproeprties))
		{
			return false;
		}
		if (Borders != null && tableStyleproeprties.Borders != null && !Borders.Compare(tableStyleproeprties.Borders))
		{
			return false;
		}
		if (Paddings != null && tableStyleproeprties.Paddings != null && !Paddings.Compare(tableStyleproeprties.Paddings))
		{
			return false;
		}
		return true;
	}
}
