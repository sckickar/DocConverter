namespace DocGen.Office;

public interface IOfficeMathParagraph : IOfficeMathEntity
{
	MathJustification Justification { get; set; }

	IOfficeMaths Maths { get; }

	object Owner { get; }

	string LaTeX { get; set; }
}
