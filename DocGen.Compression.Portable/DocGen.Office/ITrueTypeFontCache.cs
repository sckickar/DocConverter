namespace DocGen.Office;

internal interface ITrueTypeFontCache
{
	bool EqualsTo(ITrueTypeFontCache obj);

	ITrueTypeFontPrimitive GetInternals();

	void SetInternals(ITrueTypeFontPrimitive internals);
}
