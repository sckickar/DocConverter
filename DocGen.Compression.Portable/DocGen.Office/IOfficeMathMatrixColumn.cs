namespace DocGen.Office;

public interface IOfficeMathMatrixColumn : IOfficeMathEntity
{
	int ColumnIndex { get; }

	IOfficeMaths Arguments { get; }

	MathHorizontalAlignment HorizontalAlignment { get; set; }
}
