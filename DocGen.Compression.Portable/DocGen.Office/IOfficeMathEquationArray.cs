namespace DocGen.Office;

public interface IOfficeMathEquationArray : IOfficeMathFunctionBase, IOfficeMathEntity
{
	MathVerticalAlignment VerticalAlignment { get; set; }

	bool ExpandEquationContainer { get; set; }

	bool ExpandEquationContent { get; set; }

	float RowSpacing { get; set; }

	SpacingRule RowSpacingRule { get; set; }

	IOfficeMaths Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
