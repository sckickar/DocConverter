namespace DocGen.Office;

public interface IOfficeMathBaseCollection : ICollectionBase, IOfficeMathEntity
{
	IOfficeMathFunctionBase this[int index] { get; }

	IOfficeMathFunctionBase Add(int index, MathFunctionType mathFunctionType);

	IOfficeMathFunctionBase Add(MathFunctionType mathFunctionType);
}
