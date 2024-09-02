namespace DocGen.OfficeChart;

internal interface IAutoFilterCondition
{
	OfficeFilterDataType DataType { get; set; }

	OfficeFilterCondition ConditionOperator { get; set; }

	string String { get; set; }

	bool Boolean { get; }

	byte ErrorCode { get; }

	double Double { get; set; }
}
