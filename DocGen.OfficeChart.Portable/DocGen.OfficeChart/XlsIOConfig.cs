namespace DocGen.OfficeChart;

internal class XlsIOConfig
{
	public string Copyright => "DocGen, Inc. 2001 - 2004";

	public XlsIOConfig()
	{
		if (ExcelEngine.IsSecurityGranted)
		{
			ExcelEngine.ValidateLicense();
		}
	}
}
