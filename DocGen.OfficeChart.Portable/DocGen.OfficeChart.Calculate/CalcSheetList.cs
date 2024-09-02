using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Calculate;

internal class CalcSheetList : List<object>
{
	private static int nextCalcSheetNumber;

	private CalcWorkbook workBook;

	public new CalcSheet this[int i]
	{
		get
		{
			return (CalcSheet)base[i];
		}
		set
		{
			base[i] = value;
		}
	}

	public CalcSheet this[string sheetName]
	{
		get
		{
			int num = NameToIndex(sheetName);
			if (num == -1)
			{
				throw new ArgumentOutOfRangeException($"{sheetName} not found.");
			}
			return (CalcSheet)base[num];
		}
		set
		{
			int num = NameToIndex(sheetName);
			if (num == -1)
			{
				throw new ArgumentOutOfRangeException($"{sheetName} not found.");
			}
			base[num] = value;
		}
	}

	public CalcSheetList()
	{
	}

	public CalcSheetList(CalcSheet[] list, CalcWorkbook parentWorkBook)
	{
		if (list != null)
		{
			foreach (CalcSheet item in list)
			{
				base.Add((object)item);
				nextCalcSheetNumber++;
			}
		}
		workBook = parentWorkBook;
	}

	public new void Add(object o)
	{
		if (!(o is CalcSheet calcSheet))
		{
			throw new ArgumentException("Must add a CalcSheet object");
		}
		CalcEngine calcEngine = null;
		if (base.Count == 0)
		{
			CalcEngine.ResetSheetFamilyID();
			calcEngine = new CalcEngine(calcSheet);
			calcEngine.UseDependencies = true;
			if (workBook.sheetfamilyID == -1)
			{
				workBook.sheetfamilyID = CalcEngine.CreateSheetFamilyID();
			}
		}
		else
		{
			calcEngine = this[0].engine;
		}
		calcSheet.engine = calcEngine;
		string name = calcSheet.Name;
		calcEngine.RegisterGridAsSheet(name, calcSheet, workBook.sheetfamilyID);
		workBook.sheetNames.Add(name);
		int num = nextCalcSheetNumber;
		nextCalcSheetNumber++;
		workBook.idLookUp.Add(name.ToLower(), num);
		base.Add((object)calcSheet);
	}

	public new void Insert(int index, object o)
	{
		throw new NotImplementedException("Insert");
	}

	public void InsertRange(int index, ICollection c)
	{
		throw new NotImplementedException("InsertRange");
	}

	public int NameToIndex(string sheetName)
	{
		int result = -1;
		string text = sheetName.ToLower();
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i].Name.ToLower() == text)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public new void Remove(object o)
	{
		if (!(o is CalcSheet calcSheet))
		{
			throw new ArgumentException("Must add a CalcSheet object");
		}
		workBook.sheetNames.Remove(calcSheet.Name);
		workBook.idLookUp.Remove(calcSheet.Name.ToLower());
		GridSheetFamilyItem sheetFamilyItem = CalcEngine.GetSheetFamilyItem(calcSheet);
		if (sheetFamilyItem.SheetNameToToken.ContainsKey(calcSheet.Name.ToUpper()))
		{
			sheetFamilyItem.SheetNameToToken.Remove(calcSheet.Name.ToUpper());
		}
		base.Remove(o);
	}

	public new void RemoveAt(int index)
	{
		CalcSheet o = this[index];
		Remove(o);
	}

	public new CalcSheet[] ToArray()
	{
		return (CalcSheet[])base.ToArray();
	}
}
