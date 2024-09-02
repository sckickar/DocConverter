namespace DocGen.Office;

public interface IOfficeMathLeftScript : IOfficeMathFunctionBase, IOfficeMathEntity
{
	IOfficeMath Subscript { get; }

	IOfficeMath Superscript { get; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
