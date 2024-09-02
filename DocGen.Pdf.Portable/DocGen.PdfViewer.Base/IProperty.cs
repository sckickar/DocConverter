namespace DocGen.PdfViewer.Base;

internal interface IProperty
{
	KeyPropertyDescriptor Descriptor { get; }

	bool SetValue(object value);
}
