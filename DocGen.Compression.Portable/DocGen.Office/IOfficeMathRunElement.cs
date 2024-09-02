namespace DocGen.Office;

public interface IOfficeMathRunElement : IOfficeMathFunctionBase, IOfficeMathEntity
{
	IOfficeRun Item { get; set; }

	IOfficeMathFormat MathFormat { get; }
}
