namespace DocGen.OfficeChart.Calculate;

internal class FormulaInfo
{
	private string _formulaText;

	private string _formulaValue;

	private string _parsedFormula;

	internal int calcID = -2147483647;

	public string FormulaText
	{
		get
		{
			return _formulaText;
		}
		set
		{
			_formulaText = value;
		}
	}

	public string FormulaValue
	{
		get
		{
			return _formulaValue;
		}
		set
		{
			_formulaValue = value;
		}
	}

	public string ParsedFormula
	{
		get
		{
			return _parsedFormula;
		}
		set
		{
			_parsedFormula = value;
		}
	}
}
