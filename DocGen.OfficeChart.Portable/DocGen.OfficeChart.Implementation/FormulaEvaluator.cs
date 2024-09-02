using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class FormulaEvaluator
{
	public object TryGetValue(Ptg[] formula, IWorksheet sheet)
	{
		if (formula == null || formula.Length == 0)
		{
			return null;
		}
		if (formula.Length == 1)
		{
			Ptg token = formula[0];
			return GetSingleTokenResult(token, sheet);
		}
		return null;
	}

	private object GetSingleTokenResult(Ptg token, IWorksheet sheet)
	{
		object result = null;
		switch (token.TokenCode)
		{
		case FormulaToken.tBoolean:
			result = (token as BooleanPtg).Value;
			break;
		case FormulaToken.tNumber:
			result = (token as DoublePtg).Value;
			break;
		case FormulaToken.tInteger:
			result = (double)(int)(token as IntegerPtg).Value;
			break;
		case FormulaToken.tStringConstant:
			result = (token as StringConstantPtg).Value;
			break;
		case FormulaToken.tRef1:
		case FormulaToken.tRef2:
		case FormulaToken.tRef3:
		{
			IRange range = (token as RefPtg).GetRange(sheet.Workbook, sheet);
			if ((range as RangeImpl).IsSingleCell || range.HasFormula)
			{
				result = range.Value2;
			}
			break;
		}
		}
		return result;
	}
}
