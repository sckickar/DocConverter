namespace DocGen.Office;

public interface IOfficeMathBox : IOfficeMathFunctionBase, IOfficeMathEntity
{
	bool Alignment { get; set; }

	bool EnableDifferential { get; set; }

	bool NoBreak { get; set; }

	bool OperatorEmulator { get; set; }

	IOfficeMathBreak Break { get; set; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
