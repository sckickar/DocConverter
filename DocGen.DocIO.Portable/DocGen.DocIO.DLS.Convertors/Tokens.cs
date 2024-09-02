namespace DocGen.DocIO.DLS.Convertors;

internal class Tokens : Groups
{
	private string tokenName;

	private string tokenValue;

	internal string TokenName
	{
		get
		{
			return tokenName;
		}
		set
		{
			tokenName = value;
		}
	}

	internal string TokenValue
	{
		get
		{
			return tokenValue;
		}
		set
		{
			tokenValue = value;
		}
	}
}
