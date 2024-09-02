namespace DocGen.DocIO.DLS;

public interface IInlineContentControl : IParagraphItem, IEntity
{
	ContentControlProperties ContentControlProperties { get; }

	WCharacterFormat BreakCharacterFormat { get; }

	ParagraphItemCollection ParagraphItems { get; }
}
