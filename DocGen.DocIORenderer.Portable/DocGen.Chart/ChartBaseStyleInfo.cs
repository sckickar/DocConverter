namespace DocGen.Chart;

internal class ChartBaseStyleInfo : ChartStyleInfo
{
	public bool System
	{
		get
		{
			return base._System;
		}
		set
		{
			base._System = value;
		}
	}

	public bool HasSystem => base._HasSystem;

	public string Name
	{
		get
		{
			return base._Name;
		}
		set
		{
			base._Name = value;
		}
	}

	public bool HasName => base._HasName;

	public ChartBaseStylesMap BaseStylesMap
	{
		get
		{
			if (base.Identity is ChartBaseStyleIdentity chartBaseStyleIdentity)
			{
				return chartBaseStyleIdentity.BaseStylesMap;
			}
			return null;
		}
	}

	public ChartBaseStyleInfo(string name)
	{
		Name = name;
	}
}
