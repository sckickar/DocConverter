using System.IO;

namespace DocGen.DocIO.DLS;

public interface IWParagraph : ITextBodyItem, IEntity, IStyleHolder, ICompositeEntity
{
	new string StyleName { get; }

	string Text { get; set; }

	bool EnableStyleSeparator { get; set; }

	ParagraphItem this[int index] { get; }

	ParagraphItemCollection Items { get; }

	WParagraphFormat ParagraphFormat { get; }

	WListFormat ListFormat { get; }

	WCharacterFormat BreakCharacterFormat { get; }

	bool IsInCell { get; }

	bool IsEndOfSection { get; }

	bool IsEndOfDocument { get; }

	string ListString { get; }

	new void ApplyStyle(string styleName);

	new void ApplyStyle(BuiltinStyle builtinStyle);

	IWTextRange AppendText(string text);

	IInlineContentControl AppendInlineContentControl(ContentControlType controlType);

	IWPicture AppendPicture(Stream imageStream);

	IWPicture AppendPicture(byte[] imageBytes);

	IWPicture AppendPicture(byte[] svgData, byte[] imageBytes);

	WChart AppendChart(object[][] data, float width, float height);

	WChart AppendChart(float width, float height);

	WChart AppendChart(Stream excelStream, int sheetNumber, string dataRange, float width, float height);

	IWField AppendField(string fieldName, FieldType fieldType);

	BookmarkStart AppendBookmarkStart(string name);

	BookmarkEnd AppendBookmarkEnd(string name);

	WComment AppendComment(string text);

	WFootnote AppendFootnote(FootnoteType type);

	IWTextBox AppendTextBox(float width, float height);

	WSymbol AppendSymbol(byte characterCode);

	Break AppendBreak(BreakType breakType);

	Shape AppendShape(AutoShapeType autoShapeType, float width, float height);

	void AppendHTML(string html);

	IWParagraphStyle GetStyle();

	TextSelection Find(string given, bool caseSensitive, bool wholeWord);

	int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord);

	int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord, bool saveFormatting);

	WCheckBox AppendCheckBox();

	WTextFormField AppendTextFormField(string defaultText);

	WDropDownFormField AppendDropDownFormField();

	WCheckBox AppendCheckBox(string checkBoxName, bool defaultCheckBoxValue);

	WTextFormField AppendTextFormField(string formFieldName, string defaultText);

	WDropDownFormField AppendDropDownFormField(string dropDropDownName);

	IWField AppendHyperlink(string link, string text, HyperlinkType type);

	IWField AppendHyperlink(string link, WPicture picture, HyperlinkType type);

	void RemoveAbsPosition();

	TableOfContent AppendTOC(int lowerHeadingLevel, int upperHeadingLevel);

	void AppendCrossReference(ReferenceType referenceType, ReferenceKind referenceKind, Entity referenceEntity, bool insertAsHyperlink, bool includePosition, bool separatorNumber, string separatorString);

	WOleObject AppendOleObject(Stream oleStream, WPicture olePicture, OleObjectType type);

	WOleObject AppendOleObject(byte[] oleBytes, WPicture olePicture, OleObjectType type);

	WOleObject AppendOleObject(Stream oleStream, WPicture olePicture, OleLinkType oleLinkType);

	WOleObject AppendOleObject(byte[] oleBytes, WPicture olePicture, OleLinkType oleLinkType);

	WOleObject AppendOleObject(byte[] oleBytes, WPicture olePicture, string fileExtension);

	WOleObject AppendOleObject(Stream oleStream, WPicture olePicture, string fileExtension);

	WMath AppendMath();

	WMath AppendMath(string laTeX);

	WSection InsertSectionBreak();

	WSection InsertSectionBreak(SectionBreakCode breakType);
}
