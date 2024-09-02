namespace DocGen.Office;

public interface IOfficeMathRightScript : IOfficeMathFunctionBase, IOfficeMathEntity
{
	bool IsSkipAlign { get; set; }

	IOfficeMath Subscript { get; }

	IOfficeMath Superscript { get; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
