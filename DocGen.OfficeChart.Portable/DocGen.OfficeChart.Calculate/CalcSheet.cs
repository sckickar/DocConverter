using System;
using System.IO;

namespace DocGen.OfficeChart.Calculate;

internal class CalcSheet : ISheetData, ICalcData
{
	private bool calcSuspended = true;

	internal object[,] data;

	[ThreadStatic]
	private static char delimiter = '\t';

	internal CalcEngine engine;

	private bool inSerialization;

	private bool lockSheetChanges;

	private string name = "";

	public bool CalculationsSuspended
	{
		get
		{
			return calcSuspended;
		}
		set
		{
			calcSuspended = value;
		}
	}

	public int ColCount => data.GetLength(1);

	public static char Delimter
	{
		get
		{
			return delimiter;
		}
		set
		{
			delimiter = value;
		}
	}

	public CalcEngine Engine
	{
		get
		{
			return engine;
		}
		set
		{
			Engine = value;
		}
	}

	public bool LockSheetChanges
	{
		get
		{
			return lockSheetChanges;
		}
		set
		{
			lockSheetChanges = value;
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public int RowCount => data.GetLength(0);

	public object this[int row, int col]
	{
		get
		{
			return ((ICalcData)this).GetValueRowCol(row, col);
		}
		set
		{
			SetValue(row, col, value.ToString());
		}
	}

	public event ValueChangedEventHandler CalculatedValueChanged;

	public event ValueChangedEventHandler ValueChanged;

	public CalcSheet()
	{
		data = null;
	}

	public CalcSheet(int rows, int cols)
	{
		data = new object[rows, cols];
	}

	public virtual object GetValueRowCol(int row, int col)
	{
		if (row <= data.GetLength(0) && col <= data.GetLength(1))
		{
			if (inSerialization && engine != null)
			{
				string formulaRowCol = engine.GetFormulaRowCol(this, row, col);
				if (formulaRowCol.Length > 0)
				{
					return formulaRowCol;
				}
			}
			return data[row - 1, col - 1];
		}
		return "0";
	}

	protected virtual void OnCalculatedValueChanged(ValueChangedEventArgs e)
	{
		if (this.CalculatedValueChanged != null)
		{
			this.CalculatedValueChanged(this, e);
		}
	}

	protected virtual void OnValueChanged(ValueChangedEventArgs e)
	{
		if (this.ValueChanged != null)
		{
			this.ValueChanged(this, e);
		}
	}

	public static CalcSheet ReadSSS(StreamReader sr)
	{
		string s = sr.ReadLine();
		int num = int.Parse(s);
		s = sr.ReadLine();
		int cols = int.Parse(s);
		CalcSheet calcSheet = new CalcSheet(num, cols);
		s = sr.ReadLine();
		calcSheet.name = s;
		for (int i = 0; i < num; i++)
		{
			s = sr.ReadLine();
			int num2 = 0;
			string[] array = s.Split(delimiter);
			foreach (string text in array)
			{
				calcSheet.data[i, num2] = text;
				num2++;
			}
		}
		return calcSheet;
	}

	public virtual void SetValue(int row, int col, string val)
	{
		if (!LockSheetChanges)
		{
			SetValueRowCol(val, row, col);
			if (!CalculationsSuspended)
			{
				Engine.GetFormulaText(ref val);
				ValueChangedEventArgs e = new ValueChangedEventArgs(row, col, val);
				OnValueChanged(e);
			}
		}
	}

	public virtual void SetValueRowCol(object value, int row, int col)
	{
		if (!LockSheetChanges)
		{
			data[row - 1, col - 1] = value;
			if (this.CalculatedValueChanged != null)
			{
				ValueChangedEventArgs e = new ValueChangedEventArgs(row, col, value.ToString());
				OnCalculatedValueChanged(e);
			}
		}
	}

	public virtual void WireParentObject()
	{
	}

	public int GetFirstRow()
	{
		return 1;
	}

	public int GetLastRow()
	{
		return RowCount;
	}

	public int GetRowCount()
	{
		return RowCount;
	}

	public int GetFirstColumn()
	{
		return 1;
	}

	public int GetLastColumn()
	{
		return ColCount;
	}

	public int GetColumnCount()
	{
		return ColCount;
	}

	public void WriteSSS(StreamWriter sw, bool valuesOnly)
	{
		int length = data.GetLength(0);
		int length2 = data.GetLength(1);
		sw.WriteLine(length);
		sw.WriteLine(length2);
		sw.WriteLine(name);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				if (j > 0)
				{
					sw.Write(delimiter);
				}
				if (!valuesOnly)
				{
					string formulaRowCol = engine.GetFormulaRowCol(this, i + 1, j + 1);
					if (formulaRowCol.Length > 0)
					{
						sw.Write(formulaRowCol);
						continue;
					}
				}
				sw.Write(data[i, j]);
			}
			sw.WriteLine("");
		}
	}

	public void WriteSSS(StreamWriter sw)
	{
		WriteSSS(sw, valuesOnly: false);
	}
}
