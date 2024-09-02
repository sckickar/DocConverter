using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart;

internal interface INameIndexChangedEventProvider
{
	event NameImpl.NameIndexChangedEventHandler NameIndexChanged;
}
