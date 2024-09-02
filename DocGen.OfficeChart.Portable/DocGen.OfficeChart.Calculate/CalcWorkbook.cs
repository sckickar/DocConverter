using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Calculate;

internal class CalcWorkbook
{
	private CalcSheetList calcSheetList;

	private CalcEngine engine;

	internal Dictionary<object, object> idLookUp;

	internal int sheetfamilyID = -1;

	[Obsolete("This field will be removed in a future version. Please use the CalcSheetList property instead.", false)]
	public List<object> sheetNames;

	internal CalcSheetList CalcSheetList
	{
		get
		{
			return calcSheetList;
		}
		set
		{
			calcSheetList = value;
		}
	}

	internal CalcSheet[] CalcSheets => calcSheetList.ToArray();

	public CalcEngine Engine
	{
		get
		{
			return engine;
		}
		set
		{
			if (engine == null)
			{
				engine = value;
			}
		}
	}

	public int SheetCount => calcSheetList.Count;

	internal CalcSheet this[string sheetName]
	{
		get
		{
			return calcSheetList[GetSheetID(sheetName)];
		}
		set
		{
			calcSheetList[GetSheetID(sheetName)].data = value.data;
		}
	}

	internal CalcSheet this[int sheetIndex]
	{
		get
		{
			return calcSheetList[sheetIndex];
		}
		set
		{
			calcSheetList[sheetIndex].data = value.data;
		}
	}

	internal CalcWorkbook(CalcSheet[] calcSheets, Dictionary<object, object> namedRanges)
	{
		calcSheetList = new CalcSheetList(calcSheets, this);
		int count = calcSheetList.Count;
		sheetNames = new List<object>(count);
		idLookUp = new Dictionary<object, object>();
		InitCalcWorkbook(count);
		if (count <= 0)
		{
			return;
		}
		Dictionary<object, object> dictionary = new Dictionary<object, object>();
		if (namedRanges != null)
		{
			foreach (string key in namedRanges.Keys)
			{
				dictionary.Add(key.ToUpperInvariant(), namedRanges[key]);
			}
		}
		engine.NamedRanges = dictionary;
	}

	public virtual void CalculateAll()
	{
		foreach (CalcSheet calcSheet2 in calcSheetList)
		{
			calcSheet2.CalculationsSuspended = false;
		}
		foreach (CalcSheet calcSheet3 in calcSheetList)
		{
			calcSheet3.Engine.UpdateCalcID();
			for (int i = 1; i <= calcSheet3.RowCount; i++)
			{
				for (int j = 1; j <= calcSheet3.ColCount; j++)
				{
					object obj = calcSheet3[i, j];
					if (obj != null)
					{
						string text = obj.ToString();
						if (text.Length > 0 && text[0] == CalcEngine.FormulaCharacter)
						{
							calcSheet3[i, j] = text;
						}
					}
				}
			}
		}
	}

	internal void ClearFormulas(CalcSheet sheet)
	{
		_ = DateTime.Now;
		string value = $"!{GetSheetID(sheet.Name)}!";
		List<object> list = new List<object>();
		foreach (string key4 in Engine.FormulaInfoTable.Keys)
		{
			if (key4.StartsWith(value))
			{
				list.Add(key4);
			}
		}
		foreach (string item in list)
		{
			Engine.FormulaInfoTable.Remove(item);
			List<object> list2 = new List<object>();
			foreach (string key5 in Engine.DependentCells.Keys)
			{
				if (key5.StartsWith(value))
				{
					list2.Add(key5);
				}
			}
			foreach (string item2 in list2)
			{
				Engine.DependentCells.Remove(item2);
			}
			list2.Clear();
			foreach (string key6 in Engine.DependentFormulaCells.Keys)
			{
				if (key6.StartsWith(value))
				{
					list2.Add(key6);
				}
			}
			foreach (string item3 in list2)
			{
				Engine.DependentFormulaCells.Remove(item3);
			}
		}
	}

	public int GetSheetID(string sheetName)
	{
		if (!idLookUp.ContainsKey(sheetName.ToLower()))
		{
			return -1;
		}
		return (int)idLookUp[sheetName.ToLower()];
	}

	private void GetSheetRowCol(string key, out int sheet, out int row, out int col)
	{
		key = key.Substring(1);
		int num = key.IndexOf('!');
		sheet = int.Parse(key.Substring(0, num));
		key = key.Substring(num + 1);
		row = Engine.RowIndex(key);
		col = Engine.ColIndex(key);
	}

	private void InitCalcWorkbook(int sheetCount)
	{
		CalcEngine.ResetSheetFamilyID();
		if (sheetCount > 0)
		{
			engine = new CalcEngine(calcSheetList[0]);
			engine.UseDependencies = true;
			sheetfamilyID = CalcEngine.CreateSheetFamilyID();
			for (int i = 0; i < sheetCount; i++)
			{
				string name = calcSheetList[i].Name;
				engine.RegisterGridAsSheet(name, calcSheetList[i], sheetfamilyID);
				sheetNames.Add(name);
				idLookUp.Add(name.ToLower(), i);
				calcSheetList[i].engine = engine;
			}
		}
	}
}
