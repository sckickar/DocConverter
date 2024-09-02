using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal interface IChartFillObjectGetter
{
	ChartBorderImpl Border { get; }

	ChartInteriorImpl Interior { get; }

	IInternalFill Fill { get; }

	ShadowImpl Shadow { get; }

	ThreeDFormatImpl ThreeD { get; }
}
