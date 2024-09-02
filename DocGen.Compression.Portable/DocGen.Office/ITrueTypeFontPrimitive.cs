namespace DocGen.Office;

internal interface ITrueTypeFontPrimitive
{
	ObjectStatus Status { get; set; }

	bool IsSaving { get; set; }

	int ObjectCollectionIndex { get; set; }

	ITrueTypeFontPrimitive ClonedObject { get; }

	int Position { get; set; }
}
