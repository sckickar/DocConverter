using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Parsing;

public class PdfAnnotationExportSettings
{
	private AnnotationDataFormat dataFormat;

	private bool exportAppearance;

	private PdfLoadedAnnotationType[] annotationType = new PdfLoadedAnnotationType[0];

	public AnnotationDataFormat DataFormat
	{
		get
		{
			return dataFormat;
		}
		set
		{
			dataFormat = value;
		}
	}

	public bool ExportAppearance
	{
		get
		{
			return exportAppearance;
		}
		set
		{
			exportAppearance = value;
		}
	}

	public PdfLoadedAnnotationType[] AnnotationTypes
	{
		internal get
		{
			return annotationType;
		}
		set
		{
			annotationType = value;
		}
	}
}
