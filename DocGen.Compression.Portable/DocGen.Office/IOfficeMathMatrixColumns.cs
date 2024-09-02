namespace DocGen.Office;

public interface IOfficeMathMatrixColumns : ICollectionBase, IOfficeMathEntity
{
	IOfficeMathMatrixColumn this[int index] { get; }

	IOfficeMathMatrixColumn Add(int index);

	IOfficeMathMatrixColumn Add();
}
