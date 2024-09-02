using System.Collections;

namespace DocGen.OfficeChart;

public interface IOfficeChartData
{
	IOfficeDataRange this[int firstRow, int firstCol, int lastRow, int lastCol] { get; }

	void SetValue(int rowIndex, int columnIndex, int value);

	void SetValue(int rowIndex, int columnIndex, double value);

	void SetValue(int rowIndex, int columnIndex, string value);

	void SetValue(int rowIndex, int columnIndex, object value);

	void SetChartData(object[][] data);

	void SetDataRange(object[][] data, int rowIndex, int columnIndex);

	void SetDataRange(IEnumerable enumerable, int rowIndex, int columnIndex);

	object GetValue(int rowIndex, int columnIndex);

	void Clear();
}
