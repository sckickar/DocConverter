namespace DocGen.OfficeChart;

internal interface IStyle : IExtendedFormat, IParentApplication, IOptimizedUpdate
{
	bool BuiltIn { get; }

	string Name { get; }

	bool IsInitialized { get; }

	IInterior Interior { get; }
}
