namespace DocGen.Office;

public interface IOfficeMathMatrixRow : IOfficeMathEntity
{
	int RowIndex { get; }

	IOfficeMaths Arguments { get; }
}
