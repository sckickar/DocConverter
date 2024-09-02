namespace DocGen.Office;

public interface ICrop
{
	float OffsetX { get; set; }

	float OffsetY { get; set; }

	float Width { get; set; }

	float Height { get; set; }

	float ContainerLeft { get; set; }

	float ContainerTop { get; set; }

	float ContainerWidth { get; set; }

	float ContainerHeight { get; set; }
}
