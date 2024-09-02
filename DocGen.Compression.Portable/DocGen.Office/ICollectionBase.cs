namespace DocGen.Office;

public interface ICollectionBase : IOfficeMathEntity
{
	int Count { get; }

	void Remove(IOfficeMathEntity item);

	void Clear();
}
