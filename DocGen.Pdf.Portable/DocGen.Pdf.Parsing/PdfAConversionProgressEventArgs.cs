namespace DocGen.Pdf.Parsing;

public class PdfAConversionProgressEventArgs
{
	internal float m_progressValue;

	public float ProgressValue => m_progressValue;

	internal PdfAConversionProgressEventArgs()
	{
		m_progressValue = 0f;
	}
}
