namespace DocGen.DocIO.DLS;

public interface IWCharacterStyle : IStyle
{
	bool IsPrimaryStyle { get; set; }

	WCharacterFormat CharacterFormat { get; }
}
