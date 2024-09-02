namespace DocGen.DocIO.DLS;

public interface IParagraphItem : IEntity
{
	WParagraph OwnerParagraph { get; }

	bool IsInsertRevision { get; }

	bool IsDeleteRevision { get; }
}
