namespace DocGen.Office;

public interface IOfficeMathAccent : IOfficeMathFunctionBase, IOfficeMathEntity
{
	string AccentCharacter { get; set; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
