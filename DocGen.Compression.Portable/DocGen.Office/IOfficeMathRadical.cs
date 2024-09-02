namespace DocGen.Office;

public interface IOfficeMathRadical : IOfficeMathFunctionBase, IOfficeMathEntity
{
	IOfficeMath Degree { get; }

	IOfficeMath Equation { get; }

	bool HideDegree { get; set; }

	IOfficeRunFormat ControlProperties { get; set; }
}
