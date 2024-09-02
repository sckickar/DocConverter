namespace DocGen.Styles;

internal interface IStyleCloneable
{
	object Clone();

	void Dispose();

	bool ShouldClone();

	bool ShouldDispose();
}
