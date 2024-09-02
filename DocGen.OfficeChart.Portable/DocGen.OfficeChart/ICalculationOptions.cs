namespace DocGen.OfficeChart;

internal interface ICalculationOptions : IParentApplication
{
	int MaximumIteration { get; set; }

	bool RecalcOnSave { get; set; }

	double MaximumChange { get; set; }

	bool IsIterationEnabled { get; set; }

	bool R1C1ReferenceMode { get; set; }

	OfficeCalculationMode CalculationMode { get; set; }
}
