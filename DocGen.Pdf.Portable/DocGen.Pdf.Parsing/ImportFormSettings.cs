namespace DocGen.Pdf.Parsing;

public class ImportFormSettings
{
	private DataFormat dataFormat;

	internal bool AsPerSpecification = true;

	private string formName = string.Empty;

	private bool ignoreErrors;

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

	public bool IgnoreErrors
	{
		get
		{
			return ignoreErrors;
		}
		set
		{
			ignoreErrors = value;
		}
	}
}
