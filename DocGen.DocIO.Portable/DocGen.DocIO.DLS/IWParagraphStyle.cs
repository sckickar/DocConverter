namespace DocGen.DocIO.DLS;

public interface IWParagraphStyle : IStyle
{
	bool IsPrimaryStyle { get; set; }

	WParagraphFormat ParagraphFormat { get; }

	WCharacterFormat CharacterFormat { get; }

	new void Close();
}
