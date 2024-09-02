namespace DocGen.DocIO.DLS;

public interface IWSection : ICompositeEntity, IEntity
{
	IWParagraphCollection Paragraphs { get; }

	IWTableCollection Tables { get; }

	WTextBody Body { get; }

	WPageSetup PageSetup { get; }

	ColumnCollection Columns { get; }

	SectionBreakCode BreakCode { get; set; }

	bool ProtectForm { get; set; }

	WHeadersFooters HeadersFooters { get; }

	Column AddColumn(float width, float spacing);

	IWParagraph AddParagraph();

	IWTable AddTable();

	new WSection Clone();

	void MakeColumnsEqual();
}
