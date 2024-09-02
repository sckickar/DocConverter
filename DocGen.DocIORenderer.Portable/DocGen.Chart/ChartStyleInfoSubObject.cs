using System.Diagnostics;
using DocGen.Styles;

namespace DocGen.Chart;

internal abstract class ChartStyleInfoSubObject : ChartSubStyleInfoBase
{
	[DebuggerStepThrough]
	public ChartStyleInfoSubObject(StyleInfoSubObjectIdentity identity, StyleInfoStore store)
		: base(identity, store)
	{
	}

	[DebuggerStepThrough]
	public ChartStyleInfoSubObject(StyleInfoStore store)
		: base(store)
	{
	}

	public ChartStyleInfo GetChartStyleInfo()
	{
		if (!(base.Identity is StyleInfoSubObjectIdentity styleInfoSubObjectIdentity))
		{
			return null;
		}
		return styleInfoSubObjectIdentity.Owner as ChartStyleInfo;
	}
}
