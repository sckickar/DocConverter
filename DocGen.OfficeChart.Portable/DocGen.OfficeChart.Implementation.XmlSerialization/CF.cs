namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal sealed class CF
{
	public const string ConditionalFormattingsTagName = "conditionalFormattings";

	public const string ConditionalFormattingTagName = "conditionalFormatting";

	public const string RuleTagName = "cfRule";

	public const string EndsWith = "endsWith";

	public const string BeginsWith = "beginsWith";

	public const string ContainsText = "containsText";

	public const string NotContainsText = "notContainsText";

	public const string TypeContainsError = "containsErrors";

	public const string TypeNotContainsError = "notContainsErrors";

	public const string TextAttributeName = "text";

	public const string TypeAttributeName = "type";

	public const string TimePeriodTypeName = "timePeriod";

	public const string TimePeriodAttributeName = "timePeriod";

	public const string DifferentialFormattingIdAttributeName = "dxfId";

	public const string OperatorAttributeName = "operator";

	public const string BorderColorTagName = "borderColor";

	public const string NegativeFillColorTagName = "negativeFillColor";

	public const string NegativeBorderColorTagName = "negativeBorderColor";

	public const string AxisColorTagName = "axisColor";

	public const string BorderAttributeName = "border";

	public const string GradientAttributeName = "gradient";

	public const string DirectionAttributeName = "direction";

	public const string NegativeBarColorSameAsPositiveAttributeName = "negativeBarColorSameAsPositive";

	public const string NegativeBarBorderColorSameAsPositiveAttributeName = "negativeBarBorderColorSameAsPositive";

	public const string AxisPositionAttributeName = "axisPosition";

	public const string TimePeriodToday = "today";

	public const string TimePeriodYesterday = "yesterday";

	public const string TimePeriodTomorrow = "tomorrow";

	public const string TimePeriodLastsevenDays = "last7Days";

	public const string TimePeriodLastWeek = "lastWeek";

	public const string TimePeriodThisWeek = "thisWeek";

	public const string TimePeriodNextWeek = "nextWeek";

	public const string TimePeriodLastMonth = "lastMonth";

	public const string TimePeriodThisMonth = "thisMonth";

	public const string TimePeriodNextMonth = "nextMonth";

	public const string OperatorBeginsWith = "beginsWith";

	public const string OperatorBetween = "between";

	public const string OperatorContains = "containsText";

	public const string OperatorEndsWith = "endsWith";

	public const string OperatorEqual = "equal";

	public const string OperatorGreaterThan = "greaterThan";

	public const string OperatorGreaterThanOrEqual = "greaterThanOrEqual";

	public const string OperatorLessThan = "lessThan";

	public const string OperatorLessThanOrEqual = "lessThanOrEqual";

	public const string OperatorNotBetween = "notBetween";

	public const string OperatorDoesNotContain = "notContains";

	public const string OperatorNotEqual = "notEqual";

	public const string StopIfTrueAttributeName = "stopIfTrue";

	public const string PriorityAttributeName = "priority";

	public const string FormulaTagName = "formula";

	public const string TypeCellIs = "cellIs";

	public const string TypeExpression = "expression";

	public const string TypeDataBar = "dataBar";

	public const string Pivot = "pivot";

	public const string TypeIconSet = "iconSet";

	public const string TypeColorScale = "colorScale";

	public const string TypeContainsBlank = "containsBlanks";

	public const string TypeNotContainsBlank = "notContainsBlanks";

	public const string DataBarTag = "dataBar";

	public const string ValueObjectTag = "cfvo";

	public const int DefaultDataBarMinLength = 10;

	public const int DefaultDataBarMaxLength = 90;

	public const string MaxLengthTag = "maxLength";

	public const string MinLengthTag = "minLength";

	public const string ShowValueAttribute = "showValue";

	public const string IconSetTag = "iconSet";

	public const string IconSetAttribute = "iconSet";

	public const string ColorScaleTag = "colorScale";

	public const string PercentAttribute = "percent";

	public const string ReverseAttribute = "reverse";

	public const string GreaterAttribute = "gte";

	public static readonly string[] ValueTypes = new string[7] { "none", "num", "min", "max", "percent", "percentile", "formula" };

	public static string[] IconSetTypeNames = new string[17]
	{
		"3Arrows", "3ArrowsGray", "3Flags", "3TrafficLights1", "3TrafficLights2", "3Signs", "3Symbols", "3Symbols2", "4Arrows", "4ArrowsGray",
		"4RedToBlack", "4Rating", "4TrafficLights", "5Arrows", "5ArrowsGray", "5Rating", "5Quarters"
	};

	private CF()
	{
	}
}
