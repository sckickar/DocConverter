using System;

namespace DocGen.DocIO.DLS;

public class TableStyleRowProperties : FormatBase
{
	internal const int CellSpacingKey = 52;

	internal const int IsHiddenKey = 4;

	internal const int IsHeaderKey = 5;

	internal const int IsBreakAcrossPagesKey = 106;

	internal const int RowAlignmentKey = 105;

	public bool IsHidden
	{
		get
		{
			return (bool)GetPropertyValue(4);
		}
		set
		{
			SetPropertyValue(4, value);
		}
	}

	public bool IsHeader
	{
		get
		{
			return (bool)GetPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
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

	internal TableStyleRowProperties(IWordDocument doc)
		: base(doc)
	{
	}

	internal FormatBase GetAsRowFormat()
	{
		RowFormat rowFormat = new RowFormat();
		rowFormat.UpdateProperties(this);
		if (base.BaseFormat != null)
		{
			rowFormat.ApplyBase((base.BaseFormat as TableStyleRowProperties).GetAsRowFormat());
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

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			4 => false, 
			5 => false, 
			106 => true, 
			52 => -1f, 
			105 => RowAlignment.Left, 
			_ => throw new NotImplementedException(), 
		};
	}

	internal bool Compare(TableStyleRowProperties tableStyleRowProperties)
	{
		if (!Compare(52, tableStyleRowProperties))
		{
			return false;
		}
		if (!Compare(4, tableStyleRowProperties))
		{
			return false;
		}
		if (!Compare(5, tableStyleRowProperties))
		{
			return false;
		}
		if (!Compare(106, tableStyleRowProperties))
		{
			return false;
		}
		Compare(105, tableStyleRowProperties);
		return false;
	}
}
