using System;
using System.ComponentModel;

namespace DocGen.Chart;

[TypeConverter(typeof(ChartMarginsConverter))]
internal class ChartMargins : ICloneable
{
	private int top;

	private int left;

	private int bottom;

	private int right;

	[DefaultValue(10)]
	[NotifyParentProperty(true)]
	[Description("Specifies the top value.")]
	public int Top
	{
		get
		{
			return top;
		}
		set
		{
			if (top != value)
			{
				top = value;
				OnChanged();
			}
		}
	}

	[DefaultValue(10)]
	[NotifyParentProperty(true)]
	[Description("Specifies the left value.")]
	public int Left
	{
		get
		{
			return left;
		}
		set
		{
			if (left != value)
			{
				left = value;
				OnChanged();
			}
		}
	}

	[DefaultValue(10)]
	[NotifyParentProperty(true)]
	[Description("Specifies the bottom value.")]
	public int Bottom
	{
		get
		{
			return bottom;
		}
		set
		{
			if (bottom != value)
			{
				bottom = value;
				OnChanged();
			}
		}
	}

	[DefaultValue(10)]
	[NotifyParentProperty(true)]
	[Description("Specifies the right value.")]
	public int Right
	{
		get
		{
			return right;
		}
		set
		{
			if (right != value)
			{
				right = value;
				OnChanged();
			}
		}
	}

	public event EventHandler Changed;

	public ChartMargins()
		: this(10, 10, 10, 10)
	{
	}

	public ChartMargins(int left, int top, int right, int bottom)
	{
		this.top = top;
		this.left = left;
		this.right = right;
		this.bottom = bottom;
	}

	public override bool Equals(object obj)
	{
		if (obj is ChartMargins chartMargins)
		{
			if (chartMargins.left == left && chartMargins.top == top && chartMargins.right == right)
			{
				return chartMargins.bottom == bottom;
			}
			return false;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return left ^ top ^ right ^ bottom;
	}

	public ChartMargins Clone()
	{
		return new ChartMargins(left, top, right, bottom);
	}

	object ICloneable.Clone()
	{
		return new ChartMargins(left, top, right, bottom);
	}

	protected virtual void OnChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}
}
