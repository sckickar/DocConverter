namespace DocGen.Office;

public interface IOfficeMathBar : IOfficeMathFunctionBase, IOfficeMathEntity
{
	bool BarTop { get; set; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
