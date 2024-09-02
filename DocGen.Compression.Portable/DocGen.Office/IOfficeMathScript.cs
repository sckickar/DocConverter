namespace DocGen.Office;

public interface IOfficeMathScript : IOfficeMathFunctionBase, IOfficeMathEntity
{
	MathScriptType ScriptType { get; set; }

	IOfficeMath Equation { get; }

	IOfficeMath Script { get; }

	IOfficeRunFormat ControlProperties { get; set; }
}
