namespace DocGen.Pdf.Parsing;

public class ExportFormSettings
{
	private DataFormat dataFormat;

	internal bool AsPerSpecification;

	private string formName = string.Empty;

	public DataFormat DataFormat
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

	public string FormName
	{
		get
		{
			return formName;
		}
		set
		{
			formName = value;
		}
	}
}
