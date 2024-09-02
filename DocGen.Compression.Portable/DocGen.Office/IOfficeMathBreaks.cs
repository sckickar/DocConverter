namespace DocGen.Office;

public interface IOfficeMathBreaks : ICollectionBase, IOfficeMathEntity
{
	IOfficeMathBreak this[int index] { get; }

	IOfficeMathBreak Add(int index);

	IOfficeMathBreak Add();
}
