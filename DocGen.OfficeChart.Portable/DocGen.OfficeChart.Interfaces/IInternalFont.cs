using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Interfaces;

internal interface IInternalFont : IOfficeFont, IParentApplication, IOptimizedUpdate
{
	int Index { get; }

	FontImpl Font { get; }
}
