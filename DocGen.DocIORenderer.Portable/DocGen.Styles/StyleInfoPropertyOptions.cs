namespace DocGen.Styles;

internal enum StyleInfoPropertyOptions
{
	None = 0,
	Serializable = 1,
	Cloneable = 2,
	Disposable = 4,
	CloneableAndDisposable = 6,
	All = 7
}
