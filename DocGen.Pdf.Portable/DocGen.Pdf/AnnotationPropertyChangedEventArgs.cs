using System;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf;

internal class AnnotationPropertyChangedEventArgs : EventArgs
{
	internal PdfAnnotation Annotation;

	internal string PropertyName;

	internal AnnotationPropertyChangedEventArgs(PdfAnnotation annotation, string propertyName)
	{
		Annotation = annotation;
		PropertyName = propertyName;
	}
}
