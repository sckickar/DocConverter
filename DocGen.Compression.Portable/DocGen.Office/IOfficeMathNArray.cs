namespace DocGen.Office;

public interface IOfficeMathNArray : IOfficeMathFunctionBase, IOfficeMathEntity
{
	string NArrayCharacter { get; set; }

	bool HasGrow { get; set; }

	bool HideLowerLimit { get; set; }

	bool HideUpperLimit { get; set; }

	bool SubSuperscriptLimit { get; set; }

	IOfficeMath Equation { get; }

	IOfficeMath Subscript { get; }

	IOfficeMath Superscript { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
