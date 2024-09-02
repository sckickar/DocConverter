namespace DocGen.Pdf.Parsing;

internal interface ISystemFontProperty
{
	SystemFontPropertyDescriptor Descriptor { get; }

	bool SetValue(object value);
}
