namespace DocGen.DocIO.DLS;

public interface ITextBody : ICompositeEntity, IEntity
{
	IWTableCollection Tables { get; }

	IWParagraphCollection Paragraphs { get; }

	FormFieldCollection FormFields { get; }

	IWParagraph LastParagraph { get; }

	new EntityCollection ChildEntities { get; }

	IWParagraph AddParagraph();

	IWTable AddTable();

	IBlockContentControl AddBlockContentControl(ContentControlType controlType);

	void InsertXHTML(string html);

	void InsertXHTML(string html, int paragraphIndex);

	void InsertXHTML(string html, int paragraphIndex, int paragraphItemIndex);

	bool IsValidXHTML(string html, XHTMLValidationType type);

	bool IsValidXHTML(string html, XHTMLValidationType type, out string exceptionMessage);

	void EnsureMinimum();
}
