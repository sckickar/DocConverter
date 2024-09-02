namespace DocGen.Office;

public interface IOfficeMathBorderBox : IOfficeMathFunctionBase, IOfficeMathEntity
{
	bool HideTop { get; set; }

	bool HideBottom { get; set; }

	bool HideRight { get; set; }

	bool HideLeft { get; set; }

	bool StrikeDiagonalUp { get; set; }

	bool StrikeDiagonalDown { get; set; }

	bool StrikeVertical { get; set; }

	bool StrikeHorizontal { get; set; }

	IOfficeMath Equation { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
