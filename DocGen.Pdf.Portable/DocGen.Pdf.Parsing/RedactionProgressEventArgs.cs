namespace DocGen.Pdf.Parsing;

public class RedactionProgressEventArgs
{
	internal float m_progress;

	public float Progress => m_progress;

	internal RedactionProgressEventArgs()
	{
		m_progress = 0f;
	}
}
