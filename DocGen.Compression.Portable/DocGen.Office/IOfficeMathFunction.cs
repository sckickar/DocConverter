namespace DocGen.Office;

public interface IOfficeMathFunction : IOfficeMathFunctionBase, IOfficeMathEntity
{
	IOfficeMath Equation { get; }

	IOfficeMath FunctionName { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
