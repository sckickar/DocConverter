namespace DocGen.DocIO.DLS;

public interface IHtmlConverter
{
	void AppendToTextBody(ITextBody dlsTextBody, string html, int paragraphIndex, int paragraphItemIndex);

	void AppendToTextBody(ITextBody dlsTextBody, string html, int paragraphIndex, int paragraphItemIndex, IWParagraphStyle style, ListStyle listStyle);

	bool IsValid(string html, XHTMLValidationType type);

	bool IsValid(string html, XHTMLValidationType type, out string exceptionMessage);
}
