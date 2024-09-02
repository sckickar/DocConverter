using System;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class TableStyleTableProperties : FormatBase
{
	internal const int BordersKey = 1;

	internal const int PaddingsKey = 3;

	internal const int ColumnStripeKey = 4;

	internal const int RowStripeKey = 5;

	internal const int CellSpacingKey = 52;

	internal const int LeftIndentKey = 53;

	internal const int AllowPageBreaksKey = 8;

	internal const int RowAlignmentKey = 105;

	internal const int ShadingColorKey = 108;

	internal const int ForeColorKey = 111;

	internal const int TextureStyleKey = 110;

	public Color BackColor
	{
		get
		{
			return (Color)GetPropertyValue(108);
		}
		set
		{
			SetPropertyValue(108, value);
		}
	}

	public Color ForeColor
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

	public TextureStyle TextureStyle
	{
		get
		{
			return (TextureStyle)GetPropertyValue(110);
		}
		set
		{
			SetPropertyValue(110, value);
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
			SetPropertyValue(53, value);
		}
	}

	public bool AllowPageBreaks
	{
		get
		{
			return (bool)GetPropertyValue(8);
		}
		set
		{
			SetPropertyValue(8, value);
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

	public long ColumnStripe
	{
		get
		{
			return (long)GetPropertyValue(4);
		}
		set
		{
			SetPropertyValue(4, value);
		}
	}

	public long RowStripe
	{
		get
		{
			return (long)GetPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
		}
	}

	internal TableStyleTableProperties(IWordDocument doc)
		: base(doc)
	{
	}

	internal FormatBase GetAsTableFormat()
	{
		RowFormat rowFormat = new RowFormat();
		rowFormat.UpdateProperties(this);
		if (base.BaseFormat != null)
		{
			rowFormat.ApplyBase((base.BaseFormat as TableStyleTableProperties).GetAsTableFormat());
		}
		return rowFormat;
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
		Borders.ApplyBase((baseFormat as TableStyleTableProperties).Borders);
		Paddings.ApplyBase((baseFormat as TableStyleTableProperties).Paddings);
	}

	protected internal override void EnsureComposites()
	{
		EnsureComposites(1);
		EnsureComposites(3);
	}

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			4 => 0L, 
			5 => 0L, 
			52 => -1f, 
			53 => 0f, 
			8 => true, 
			105 => RowAlignment.Left, 
			108 => Color.Empty, 
			111 => Color.Empty, 
			110 => TextureStyle.TextureNone, 
			_ => throw new NotImplementedException(), 
		};
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

	internal bool Compare(TableStyleTableProperties tableStyleTableProperties)
	{
		if (!Compare(4, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(5, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(52, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(53, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(8, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(105, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(108, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(111, tableStyleTableProperties))
		{
			return false;
		}
		if (!Compare(110, tableStyleTableProperties))
		{
			return false;
		}
		return true;
	}
}
