namespace DocGen.Office;

public interface IOfficeMath : IOfficeMathEntity
{
	int ArgumentSize { get; set; }

	IOfficeMathBaseCollection Functions { get; }

	IOfficeMathMatrixColumn OwnerColumn { get; }

	IOfficeMath OwnerMath { get; }

	IOfficeMathMatrixRow OwnerRow { get; }

	IOfficeMathBreaks Breaks { get; }
}
