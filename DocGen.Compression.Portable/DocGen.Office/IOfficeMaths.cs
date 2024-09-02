namespace DocGen.Office;

public interface IOfficeMaths : ICollectionBase, IOfficeMathEntity
{
	IOfficeMath this[int index] { get; }

	IOfficeMath Add(int index);

	IOfficeMath Add();
}
