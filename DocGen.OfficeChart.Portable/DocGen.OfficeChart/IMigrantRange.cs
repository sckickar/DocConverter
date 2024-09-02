using System;
using System.Collections;

namespace DocGen.OfficeChart;

internal interface IMigrantRange : IRange, IParentApplication, IEnumerable
{
	void ResetRowColumn(int iRow, int iColumn);

	void SetValue(int value);

	void SetValue(double value);

	void SetValue(DateTime value);

	void SetValue(bool value);

	void SetValue(string value);
}
