using System.IO;

namespace DocGen.DocIO.DLS;

public interface IWPicture : IParagraphItem, IEntity
{
	float Height { get; set; }

	float Width { get; set; }

	float HeightScale { get; set; }

	float WidthScale { get; set; }

	float Rotation { get; set; }

	bool FlipHorizontal { get; set; }

	bool FlipVertical { get; set; }

	byte[] SvgData { get; }

	byte[] ImageBytes { get; }

	HorizontalOrigin HorizontalOrigin { get; set; }

	VerticalOrigin VerticalOrigin { get; set; }

	float HorizontalPosition { get; set; }

	float VerticalPosition { get; set; }

	TextWrappingStyle TextWrappingStyle { get; set; }

	TextWrappingType TextWrappingType { get; set; }

	bool IsBelowText { get; set; }

	ShapeHorizontalAlignment HorizontalAlignment { get; set; }

	ShapeVerticalAlignment VerticalAlignment { get; set; }

	string AlternativeText { get; set; }

	string Name { get; set; }

	string Title { get; set; }

	bool Visible { get; set; }

	WCharacterFormat CharacterFormat { get; }

	void LoadImage(Stream imageStream);

	void LoadImage(byte[] imageBytes);

	void LoadImage(byte[] svgData, byte[] imageBytes);

	IWParagraph AddCaption(string name, CaptionNumberingFormat format, CaptionPosition captionPosition);
}
