using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DocGen.Office.Markdown;

namespace DocGen.DocIO.DLS;

public interface IWordDocument : ICompositeEntity, IEntity, IDisposable
{
	FontSettings FontSettings { get; }

	Footnote Footnotes { get; set; }

	Endnote Endnotes { get; set; }

	float DefaultTabWidth { get; set; }

	BuiltinDocumentProperties BuiltinDocumentProperties { get; }

	Template AttachedTemplate { get; }

	bool UpdateStylesOnOpen { get; set; }

	CustomDocumentProperties CustomDocumentProperties { get; }

	WSectionCollection Sections { get; }

	IStyleCollection Styles { get; }

	ListStyleCollection ListStyles { get; }

	BookmarkCollection Bookmarks { get; }

	TextBoxCollection TextBoxes { get; }

	CommentsCollection Comments { get; }

	WSection LastSection { get; }

	WParagraph LastParagraph { get; }

	FootEndNoteNumberFormat EndnoteNumberFormat { get; set; }

	FootEndNoteNumberFormat FootnoteNumberFormat { get; set; }

	EndnoteRestartIndex RestartIndexForEndnote { get; set; }

	EndnotePosition EndnotePosition { get; set; }

	FootnoteRestartIndex RestartIndexForFootnotes { get; set; }

	FootnotePosition FootnotePosition { get; set; }

	Watermark Watermark { get; set; }

	Background Background { get; }

	MailMerge MailMerge { get; }

	ProtectionType ProtectionType { get; set; }

	ViewSetup ViewSetup { get; }

	bool ThrowExceptionsForUnsupportedElements { get; set; }

	int InitialFootnoteNumber { get; set; }

	int InitialEndnoteNumber { get; set; }

	new EntityCollection ChildEntities { get; }

	XHTMLValidationType XHTMLValidateOption { get; set; }

	[Obsolete("This property has been deprecated. Use the Picture property of Background class to set the background image of the document")]
	byte[] BackgroundImage { get; set; }

	DocVariables Variables { get; }

	DocProperties Properties { get; }

	bool HasChanges { get; }

	bool TrackChanges { get; set; }

	bool ReplaceFirst { get; set; }

	HTMLImportSettings HTMLImportSettings { get; set; }

	MdImportSettings MdImportSettings { get; }

	SaveOptions SaveOptions { get; }

	ImportOptions ImportOptions { get; set; }

	bool ImportStyles { get; set; }

	bool ImportStylesOnTypeMismatch { get; set; }

	[Obsolete("This property has been deprecated. Use the UpdateDocumentFields method of WordDocument class to update the fields in the document.")]
	bool UpdateFields { get; set; }

	FormatType ActualFormatType { get; }

	Dictionary<string, string> FontSubstitutionTable { get; set; }

	bool HasMacros { get; }

	Hyphenator Hyphenator { get; }

	RevisionOptions RevisionOptions { get; }

	IWParagraph CreateParagraph();

	void EnsureMinimal();

	IWSection AddSection();

	IWParagraphStyle AddParagraphStyle(string styleName);

	ListStyle AddListStyle(ListType listType, string styleName);

	string GetText();

	new WordDocument Clone();

	void ImportSection(IWSection section);

	void ImportContent(IWordDocument doc);

	void ImportContent(IWordDocument doc, ImportOptions importOptions);

	void ImportContent(IWordDocument doc, bool importStyles);

	IStyle AddStyle(BuiltinStyle builtinStyle);

	void AcceptChanges();

	void RejectChanges();

	void Protect(ProtectionType type);

	void Protect(ProtectionType type, string password);

	void EncryptDocument(string password);

	void RemoveEncryption();

	List<Entity> GetCrossReferenceItems(ReferenceType refernceType);

	void Open(Stream stream, FormatType formatType, XHTMLValidationType validationType, string baseUrl);

	void Open(Stream stream, FormatType formatType, XHTMLValidationType validationType);

	void Open(Stream stream, FormatType formatType);

	void Open(Stream stream, FormatType formatType, string password);

	void Save(Stream stream, FormatType formatType);

	void Close();

	TextSelection Find(Regex pattern);

	TextSelection[] FindSingleLine(Regex pattern);

	TextSelection Find(string given, bool caseSensitive, bool wholeWord);

	TextSelection[] FindSingleLine(string given, bool caseSensitive, bool wholeWord);

	TextSelection[] FindAll(Regex pattern);

	TextSelection[] FindAll(string given, bool caseSensitive, bool wholeWord);

	int Replace(Regex pattern, string replace);

	int Replace(string given, string replace, bool caseSensitive, bool wholeWord);

	int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord);

	int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord, bool saveFormatting);

	int Replace(Regex pattern, TextSelection textSelection);

	int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting);

	int Replace(string given, TextBodyPart bodyPart, bool caseSensitive, bool wholeWord);

	int Replace(string given, TextBodyPart bodyPart, bool caseSensitive, bool wholeWord, bool saveFormatting);

	int Replace(Regex pattern, TextBodyPart bodyPart);

	int Replace(Regex pattern, TextBodyPart bodyPart, bool saveFormatting);

	int Replace(string given, IWordDocument replaceDoc, bool caseSensitive, bool wholeWord);

	int Replace(string given, IWordDocument replaceDoc, bool caseSensitive, bool wholeWord, bool saveFormatting);

	int Replace(Regex pattern, IWordDocument replaceDoc, bool saveFormatting);

	void UpdateWordCount();

	void UpdateDocumentFields();

	void UpdateAlternateChunks();

	int ReplaceSingleLine(string given, string replace, bool caseSensitive, bool wholeWord);

	int ReplaceSingleLine(Regex pattern, string replace);

	int ReplaceSingleLine(string given, TextSelection replacement, bool caseSensitive, bool wholeWord);

	int ReplaceSingleLine(Regex pattern, TextSelection replacement);

	int ReplaceSingleLine(string given, TextBodyPart replacement, bool caseSensitive, bool wholeWord);

	int ReplaceSingleLine(Regex pattern, TextBodyPart replacement);

	TextSelection FindNext(TextBodyItem startTextBodyItem, string given, bool caseSensitive, bool wholeWord);

	TextSelection FindNext(TextBodyItem startBodyItem, Regex pattern);

	TextSelection[] FindNextSingleLine(TextBodyItem startTextBodyItem, string given, bool caseSensitive, bool wholeWord);

	TextSelection[] FindNextSingleLine(TextBodyItem startBodyItem, Regex pattern);

	void ResetFindNext();

	ParagraphItem CreateParagraphItem(ParagraphItemType itemType);

	void RemoveMacros();
}
