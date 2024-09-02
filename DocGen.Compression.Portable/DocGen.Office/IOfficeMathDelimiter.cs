namespace DocGen.Office;

public interface IOfficeMathDelimiter : IOfficeMathFunctionBase, IOfficeMathEntity
{
	string BeginCharacter { get; set; }

	string EndCharacter { get; set; }

	bool IsGrow { get; set; }

	string Seperator { get; set; }

	MathDelimiterShapeType DelimiterShape { get; set; }

	IOfficeMaths Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
