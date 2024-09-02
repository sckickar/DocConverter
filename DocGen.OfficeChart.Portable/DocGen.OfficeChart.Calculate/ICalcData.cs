namespace DocGen.OfficeChart.Calculate;

internal interface ICalcData
{
	event ValueChangedEventHandler ValueChanged;

	object GetValueRowCol(int row, int col);

	void SetValueRowCol(object value, int row, int col);

	void WireParentObject();
}
