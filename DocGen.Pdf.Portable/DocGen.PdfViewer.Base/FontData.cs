namespace DocGen.PdfViewer.Base;

internal class FontData : PostScriptObj
{
	private readonly KeyProperty<string> familyName;

	private readonly KeyProperty<string> weight;

	private readonly KeyProperty<double> italicAngle;

	public string FamilyName => familyName.GetValue();

	public string Weight => weight.GetValue();

	public double ItalicAngle => italicAngle.GetValue();

	public FontData()
	{
		familyName = CreateProperty<string>(new KeyPropertyDescriptor
		{
			Name = "FamilyName"
		});
		weight = CreateProperty<string>(new KeyPropertyDescriptor
		{
			Name = "Weight"
		});
		italicAngle = CreateProperty(new KeyPropertyDescriptor
		{
			Name = "ItalicAngle"
		}, 0.0);
	}
}
