using System;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Parsing;

public class PdfExportAnnotationCollection : PdfCollection
{
	public PdfLoadedAnnotation this[int index]
	{
		get
		{
			int count = base.List.Count;
			if (count < 0 || index >= count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedAnnotation;
		}
	}

	public void Add(PdfLoadedAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		base.List.Add(annotation);
	}
}
