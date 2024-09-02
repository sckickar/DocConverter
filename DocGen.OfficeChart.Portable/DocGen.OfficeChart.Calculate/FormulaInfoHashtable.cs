using System.Collections.Generic;

namespace DocGen.OfficeChart.Calculate;

internal class FormulaInfoHashtable : Dictionary<object, object>
{
	public new FormulaInfo this[object obj]
	{
		get
		{
			return base[obj] as FormulaInfo;
		}
		set
		{
			base[obj] = value;
		}
	}
}
