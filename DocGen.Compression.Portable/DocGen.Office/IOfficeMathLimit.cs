namespace DocGen.Office;

public interface IOfficeMathLimit : IOfficeMathFunctionBase, IOfficeMathEntity
{
	MathLimitType LimitType { get; set; }

	IOfficeMath Equation { get; }

	IOfficeMath Limit { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
