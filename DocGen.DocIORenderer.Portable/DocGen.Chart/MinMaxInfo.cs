using System;
using System.ComponentModel;

namespace DocGen.Chart;

[TypeConverter(typeof(MinMaxInfoConverter))]
internal class MinMaxInfo
{
	internal double min;

	internal double max;

	internal double interval;

	[Browsable(false)]
	[Description("Returns the difference between the upper and lower boundary of this range.")]
	public double Delta => max - min;

	public double Min
	{
		get
		{
			return min;
		}
		set
		{
			if (min != value)
			{
				min = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[Description("Gets or sets the upper boundary of this range.")]
	public double Max
	{
		get
		{
			return max;
		}
		set
		{
			if (max != value)
			{
				max = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[Description("Gets or sets the value of the interval associated with this range.")]
	public double Interval
	{
		get
		{
			return interval;
		}
		set
		{
			if (interval != value)
			{
				if (value > 0.0)
				{
					interval = value;
				}
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[Browsable(false)]
	public int NumberOfIntervals
	{
		get
		{
			int num = (int)Math.Round((max - min) / interval);
			if (num != 0)
			{
				return num;
			}
			return 1;
		}
	}

	public event EventHandler SettingsChanged;

	public MinMaxInfo Clone()
	{
		return new MinMaxInfo(min, max, interval);
	}

	public bool Equals(MinMaxInfo minMaxInfo)
	{
		if (min == minMaxInfo.Min && max == minMaxInfo.Max)
		{
			return Interval == minMaxInfo.Interval;
		}
		return false;
	}

	public bool Contains(double d)
	{
		if (min <= d)
		{
			return max >= d;
		}
		return false;
	}

	public bool Intersects(MinMaxInfo r)
	{
		if (!Contains(r.Min) && !Contains(r.Max))
		{
			return r.Contains(Min);
		}
		return true;
	}

	public MinMaxInfo(double min, double max, double interval)
	{
		if (double.IsNaN(min) || double.IsNaN(max))
		{
			this.min = 0.0;
			this.max = 5.0;
			this.interval = 1.0;
		}
		else
		{
			this.min = min;
			this.max = max;
			Interval = interval;
		}
	}

	protected virtual void OnSettingsChanged(EventArgs e)
	{
		if (this.SettingsChanged != null)
		{
			this.SettingsChanged(this, e);
		}
	}

	public override string ToString()
	{
		return new MinMaxInfoConverter().ConvertToString(this);
	}
}
