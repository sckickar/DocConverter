namespace DocGen.Office;

public interface IOfficeMathMatrixRows : ICollectionBase, IOfficeMathEntity
{
	IOfficeMathMatrixRow this[int index] { get; }

	IOfficeMathMatrixRow Add(int index);

	IOfficeMathMatrixRow Add();
}
