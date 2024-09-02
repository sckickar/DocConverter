namespace DocGen.Chart;

internal interface IEditableChartSeriesModel : IChartSeriesModel
{
	void Add(double x, double[] y);

	void Add(double x, double[] y, string category);

	void Add(double x, double[] y, bool isEmpty);

	void Add(double x, double[] y, bool isEmpty, string category);

	void Insert(int xIndex, double x, double[] yValues);

	void Insert(int xIndex, double x, double[] yValues, string category);

	void SetX(int xIndex, double value);

	void SetY(int xIndex, double[] yValues);

	void SetEmpty(int xIndex, bool isEmpty);

	void Remove(int xIndex);

	void Clear();
}
