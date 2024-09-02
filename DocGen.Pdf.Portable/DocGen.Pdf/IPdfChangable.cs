namespace DocGen.Pdf;

internal interface IPdfChangable
{
	bool Changed { get; }

	void FreezeChanges(object freezer);
}
