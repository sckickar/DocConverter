namespace DocGen.Office;

public interface IOfficeMathFraction : IOfficeMathFunctionBase, IOfficeMathEntity
{
	MathFractionType FractionType { get; set; }

	IOfficeMath Denominator { get; }

	IOfficeMath Numerator { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
