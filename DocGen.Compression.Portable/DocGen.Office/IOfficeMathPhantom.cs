namespace DocGen.Office;

public interface IOfficeMathPhantom : IOfficeMathFunctionBase, IOfficeMathEntity
{
	bool Show { get; set; }

	bool Transparent { get; set; }

	bool ZeroAscent { get; set; }

	bool ZeroDescent { get; set; }

	bool ZeroWidth { get; set; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
