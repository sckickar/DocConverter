namespace DocGen.Office;

internal abstract class WidthTable : ICloneable
{
	public abstract int this[int index] { get; }

	public abstract WidthTable Clone();

	object ICloneable.Clone()
	{
		return Clone();
	}
}
