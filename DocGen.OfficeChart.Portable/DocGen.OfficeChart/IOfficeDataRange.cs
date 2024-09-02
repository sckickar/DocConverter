namespace DocGen.OfficeChart;

public interface IOfficeDataRange
{
	int FirstRow { get; }

	int LastRow { get; }

	int FirstColumn { get; }

	int LastColumn { get; }

	void SetValue(int rowIndex, int columnIndex, int value);

	void SetValue(int rowIndex, int columnIndex, double value);

	void SetValue(int rowIndex, int columnIndex, string value);

	void SetValue(int rowIndex, int columnIndex, object value);

	object GetValue(int rowIndex, int columnIndex);

	object GetValue(int rowIndex, int columnIndex, bool useFormulaValue);
}
