namespace DocGen.DocIO.DLS;

public interface IWTextRange : IParagraphItem, IEntity
{
	string Text { get; set; }

	WCharacterFormat CharacterFormat { get; }

	void ApplyCharacterFormat(WCharacterFormat charFormat);
}
