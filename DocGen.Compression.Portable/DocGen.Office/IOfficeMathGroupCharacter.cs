namespace DocGen.Office;

public interface IOfficeMathGroupCharacter : IOfficeMathFunctionBase, IOfficeMathEntity
{
	bool HasAlignTop { get; set; }

	string GroupCharacter { get; set; }

	bool HasCharacterTop { get; set; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
