namespace DocGen.Office;

public interface IOfficeMathFormat
{
	bool HasAlignment { get; set; }

	IOfficeMathBreak Break { get; set; }

	bool HasLiteral { get; set; }

	bool HasNormalText { get; set; }

	MathFontType Font { get; set; }

	MathStyleType Style { get; set; }
}
