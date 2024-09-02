using System;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartBaseStyleIdentity : StyleInfoIdentityBase
{
	private ChartBaseStylesMap styleInfoMap;

	public ChartBaseStylesMap BaseStylesMap => styleInfoMap;

	public override void Dispose()
	{
		styleInfoMap = null;
		GC.SuppressFinalize(this);
	}

	public ChartBaseStyleIdentity(ChartBaseStylesMap styleInfoMap)
	{
		this.styleInfoMap = styleInfoMap;
	}

	public override IStyleInfo[] GetBaseStyles(IStyleInfo thisStyleInfo)
	{
		if (styleInfoMap != null)
		{
			return styleInfoMap.GetBaseStyles(thisStyleInfo as ChartStyleInfo);
		}
		return null;
	}
}
