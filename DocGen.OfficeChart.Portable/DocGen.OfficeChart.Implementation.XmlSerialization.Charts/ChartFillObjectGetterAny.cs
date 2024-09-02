using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartFillObjectGetterAny : IChartFillObjectGetter
{
	private ChartBorderImpl m_border;

	private ChartInteriorImpl m_interior;

	private IInternalFill m_fill;

	private ShadowImpl m_shadow;

	private ThreeDFormatImpl m_threeD;

	public ChartBorderImpl Border => m_border;

	public ChartInteriorImpl Interior => m_interior;

	public IInternalFill Fill => m_fill;

	public ShadowImpl Shadow => m_shadow;

	public ThreeDFormatImpl ThreeD => m_threeD;

	public ChartFillObjectGetterAny(ChartBorderImpl border, ChartInteriorImpl interior, IInternalFill fill, ShadowImpl shadow, ThreeDFormatImpl three_d)
	{
		m_border = border;
		m_interior = interior;
		m_fill = fill;
		m_shadow = shadow;
		m_threeD = three_d;
	}
}
