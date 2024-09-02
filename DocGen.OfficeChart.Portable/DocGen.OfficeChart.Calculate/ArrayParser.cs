using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.Calculate;

internal class ArrayParser : IDisposable
{
	internal delegate int ArrayDelegate(int row, int col, ref int height, ref int width, ICalcData calcData);

	private string validFunctionNameChars = "_";

	private char[] markers = new char[5] { '+', '-', '*', '/', '&' };

	private int length;

	private const char BMARKER = '\u0092';

	internal ArrayDelegate GetArrayRecordPosition;

	internal CalcEngine Engine;

	internal ArrayParser(CalcEngine engine)
	{
		Engine = engine;
	}

	internal string[] SplitString(string formula)
	{
		string pattern = "([-+*/&])";
		formula = formula.Replace(" ", "").ToUpper();
		Engine.MarkNamedRanges(ref formula);
		return Regex.Split(formula, pattern);
	}

	internal string CalculateArraySize(string substring, ref int height, ref int width, ref int minHeight, ref int minWidth)
	{
		int num = substring.IndexOf(":");
		_ = string.Empty;
		int num2 = 0;
		int num3 = 0;
		minWidth = 0;
		minHeight = 0;
		if (Engine.IsCellReference(substring))
		{
			if (num > -1)
			{
				int num4 = Engine.RowIndex(substring.Substring(0, num));
				int num5 = Engine.RowIndex(substring.Substring(num + 1));
				int num6 = Engine.ColIndex(substring.Substring(0, num));
				int num7 = Engine.ColIndex(substring.Substring(num + 1));
				num2 = num5 - num4 + 1;
				num3 = num7 - num6 + 1;
				if (num2 > height)
				{
					num5 -= num2 - height;
				}
				if (num3 > width)
				{
					num7 -= num3 - width;
				}
				if (num2 < height)
				{
					minHeight = num2;
				}
				if (num3 < width)
				{
					minWidth = num3;
				}
				substring = RangeInfo.GetAlphaLabel(num6) + num4 + ":" + RangeInfo.GetAlphaLabel(num7) + num5;
			}
			else
			{
				minHeight = 1;
				minWidth = 1;
			}
		}
		else
		{
			num2 = 1;
			num3 = 1;
			if (substring.Contains(",") && !substring.Contains(";"))
			{
				num3 = Engine.SplitArgsPreservingQuotedCommas(substring).Length;
			}
			else if (substring.Contains(";") && !substring.Contains(","))
			{
				num2 = Engine.SplitArguments(substring, ';').Length;
			}
			if (num2 < height)
			{
				minHeight = num2;
			}
			if (num3 < width)
			{
				minWidth = num3;
			}
		}
		return substring;
	}

	internal List<string[]> ResizeCellRange(string formula, string originalFormula)
	{
		List<string[]> list = new List<string[]>();
		string[] array = SplitString(formula);
		_ = string.Empty;
		int height = GetHeight(array);
		int width = GetWidth(array);
		int minHeight = 0;
		int minWidth = 0;
		length = height * width;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IndexOfAny(markers) > -1 || !Engine.IsCellReference(array[i]))
			{
				if (array[i].StartsWith("[") && array[i].EndsWith("]"))
				{
					array[i] = array[i].Substring(1, array[i].Length - 2);
					if (array[i].Contains(","))
					{
						array[i] = CalculateArraySize(array[i], ref height, ref width, ref minHeight, ref minWidth);
						list.Add(Engine.SplitArgsPreservingQuotedCommas(array[i]));
					}
					else if (array[i].Contains(";"))
					{
						array[i] = CalculateArraySize(array[i], ref height, ref width, ref minHeight, ref minWidth);
						list.Add(Engine.SplitArguments(array[i], ';'));
					}
				}
				else
				{
					list.Add(new string[1] { array[i] });
					if (array[i].IndexOfAny(markers) < 0)
					{
						minHeight = 1;
					}
				}
			}
			else
			{
				string obj = array[i];
				int num = 0;
				if (!char.IsLetterOrDigit(obj[num + 1]) && Engine.IsCellReference(array[i]))
				{
					array[i] = Engine.GetCellsFromArgs(array[i], findCellsFromRange: false)[0];
				}
				array[i] = CalculateArraySize(array[i], ref height, ref width, ref minHeight, ref minWidth);
				list.Add(Engine.GetCellsFromArgs(array[i]));
			}
			if (list[i].Length >= length || array[i].IndexOfAny(markers) >= 0)
			{
				continue;
			}
			string[] array2 = list[i];
			int num2 = list[i].Length;
			int num3 = 0;
			Array.Resize(ref array2, length);
			if (minWidth != 0 && minWidth < width)
			{
				num3 = 0;
				int num4 = 0;
				int num5 = 0;
				while (num5 < length)
				{
					while (num4 < width && num5 < length)
					{
						if (num3 >= num2)
						{
							num3 = 0;
						}
						array2[num5] = list[i][num3];
						num4++;
						num5++;
					}
					num3++;
					num4 = 0;
				}
			}
			if (minHeight != 0 && minHeight < height)
			{
				num3 = 0;
				for (int j = num2; j < length; j++)
				{
					if (num3 >= num2)
					{
						num3 = 0;
					}
					array2[j] = list[i][num3];
					num3++;
				}
			}
			list[i] = array2;
		}
		return list;
	}

	internal string Parse(string formula, string originalFormula)
	{
		char c = '\u007f';
		string text = formula;
		string text2 = string.Empty;
		_ = string.Empty;
		string[] array = null;
		if (formula.IndexOfAny(markers) < 0)
		{
			return formula;
		}
		if (text.Contains(CalcEngine.ParseArgumentSeparator.ToString()))
		{
			text = text.Replace("{", "\"{").Replace("}", "}\"");
			array = Engine.SplitArgsPreservingQuotedCommas(text);
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				if (text3.IndexOfAny(markers) > -1 && !text3.Contains("\""))
				{
					text = text3;
					text2 = text;
				}
			}
			if (text2 == string.Empty && ((array.Length != 1 && array[0].IndexOfAny(markers) < 0) || (array[0].Contains("\"{") && array[0].Contains("}\"")) || text.Contains(c.ToString())))
			{
				return formula;
			}
		}
		text = text.Replace("(", "").Replace(")", "");
		text = text.Replace("\"", "");
		List<string[]> list = ResizeCellRange(text, originalFormula);
		List<string> list2 = new List<string>();
		string text4 = string.Empty;
		string text5 = string.Empty;
		if (length == 0)
		{
			length = list[0].Length;
		}
		int count = list.Count;
		int num = 0;
		for (int j = 0; j < length; j++)
		{
			num = j;
			string text6 = "";
			for (int k = 0; k < count; k++)
			{
				if (list[k].Length == 1)
				{
					j = 0;
				}
				text6 += list[k][j];
				j = num;
			}
			list2.Add(text6);
		}
		foreach (string item in list2)
		{
			text5 = text5 + item + CalcEngine.ParseArgumentSeparator;
		}
		if (array != null && array.Length > 1)
		{
			for (int l = 0; l < array.Length; l++)
			{
				text4 = ((!array[l].Equals(text2)) ? ((l != 0) ? (text4 + CalcEngine.ParseArgumentSeparator + array[l]) : (text4 + array[l])) : ((l != 0) ? ((l == array.Length - 1) ? (text4 + CalcEngine.ParseArgumentSeparator + text5.Substring(0, text5.Length - 1) + ")") : (text4 + CalcEngine.ParseArgumentSeparator + text5.Substring(0, text5.Length - 1))) : (text4 + "(" + text5.Substring(0, text5.Length - 1))));
			}
		}
		else
		{
			text4 = text4 + "(" + text5.Substring(0, text5.Length - 1) + ")";
		}
		return text4.Replace("\"", "");
	}

	internal string ParseLibraryFormula(string formula)
	{
		if (formula.Length > 0)
		{
			formula = formula.Substring(2, formula.Length - 3);
		}
		string text = formula;
		formula = Engine.CheckForNamedRange(formula);
		if ((formula.StartsWith("{") && formula.EndsWith("}")) || (Engine.findNamedRange && Engine.IsRange(formula)))
		{
			return ParseDimensionalArray(formula);
		}
		Dictionary<object, object> dictionary = Engine.SaveStrings(ref formula);
		formula = formula.Replace(" ", "").Replace("{", "\"[").Replace("}", "]\"");
		int num = formula.IndexOf(')');
		if (num == -1)
		{
			formula = Parse(formula, text);
		}
		else
		{
			while (num > -1)
			{
				int num2 = 0;
				int num3 = num - 1;
				while (num3 > -1 && (formula[num3] != '(' || num2 != 0))
				{
					if (formula[num3] == ')')
					{
						num2++;
					}
					num3--;
				}
				if (num3 == -1)
				{
					return Engine.ErrorStrings[0].ToString();
				}
				int num4 = num3 - 1;
				while (num4 > -1 && (char.IsLetterOrDigit(formula[num4]) || validFunctionNameChars.IndexOf(formula[num4]) > -1 || formula[num4].Equals(CalcEngine.ParseDecimalSeparator)))
				{
					num4--;
				}
				int num5 = num3 - num4 - 1;
				if (num5 > 0 && Engine.LibraryFunctions.ContainsKey(formula.Substring(num4 + 1, num5)))
				{
					string formula2 = formula.Substring(num3, num - num3 + 1);
					formula2 = Parse(formula2, text);
					formula = formula.Substring(0, num4 + 1) + formula.Substring(num4 + 1, num5) + formula2.Replace('(', '{').Replace(')', '}') + formula.Substring(num + 1);
				}
				else if (num5 == 0)
				{
					string text2 = string.Empty;
					if (num3 > 0)
					{
						text2 = formula.Substring(0, num3);
					}
					text2 = text2 + "{" + formula.Substring(num3 + 1, num - num3 - 1) + "}";
					if (num < formula.Length)
					{
						text2 += formula.Substring(num + 1);
					}
					formula = text2;
					if (!text2.Contains("(") && !text2.Contains(")"))
					{
						formula = Parse(formula, text).Replace('(', '{').Replace(')', '}');
					}
				}
				else
				{
					formula = formula.Replace('(', '{').Replace(')', '}');
				}
				num = formula.IndexOf(')');
			}
		}
		if (dictionary != null && dictionary.Count > 0)
		{
			Engine.SetStrings(ref formula, dictionary);
		}
		formula = formula.Replace('{', '(').Replace('}', ')').Replace("\"[", "{")
			.Replace("]\"", "}");
		if (IsMultiCellArray(text))
		{
			formula = ParseMultiCellArray(formula, text);
		}
		return formula;
	}

	internal string ComputeInteriorFunction(string arg, string label, int computedLevel)
	{
		string text = string.Empty;
		switch (label)
		{
		case "LEN":
			text = ComputeLen(arg, computedLevel);
			break;
		case "ROW":
			text = ComputeRow(arg, computedLevel);
			break;
		case "COLUMN":
			text = ComputeColumn(arg, computedLevel);
			break;
		case "IF":
			text = ComputeIF(arg, computedLevel);
			break;
		}
		if (computedLevel > 0 && label != "IF")
		{
			return "{" + text + "}";
		}
		return text;
	}

	internal bool IsMultiCellArray(string formula)
	{
		if (formula.StartsWith("(") && formula.EndsWith(")"))
		{
			formula = formula.Substring(1, formula.Length - 2);
		}
		bool result = false;
		string[] array = SplitString(formula);
		foreach (string text in array)
		{
			if ((text.Contains("(") && !text.Contains(")")) || (!text.Contains("(") && text.Contains(")")))
			{
				result = false;
				break;
			}
			if (Engine.IsRange(text.Replace("$", "")) || (text.StartsWith("{") && text.EndsWith("}")))
			{
				result = true;
			}
		}
		return result;
	}

	internal string ParseMultiCellArray(string formula, string originalFormula)
	{
		string text = originalFormula;
		string[] substrings = SplitString(originalFormula);
		Engine.MarkNamedRanges(ref originalFormula);
		if (text != originalFormula)
		{
			return ParseDimensionalArray(formula);
		}
		if (Engine.cell != string.Empty)
		{
			int height = GetHeight(substrings);
			int width = GetWidth(substrings);
			int num = GetPosition(ref height, ref width);
			if (GetWidth(substrings) > width && num != -1 && num >= width)
			{
				num += num / width * (GetWidth(substrings) - width);
			}
			if (num >= 0)
			{
				if (formula.StartsWith("(") && formula.EndsWith(")"))
				{
					formula = formula.Substring(1, formula.Length - 2);
				}
				string[] array = Engine.SplitArgsPreservingQuotedCommas(formula);
				if (array.Length > num)
				{
					formula = array[num];
				}
			}
			else
			{
				formula = Engine.ErrorStrings[0].ToString();
			}
		}
		return formula;
	}

	internal string ParseDimensionalArray(string formula)
	{
		formula = formula.Replace(" ", "");
		string text = formula;
		if (formula.IndexOfAny(markers) > -1)
		{
			formula = formula.Replace("{", "\"[").Replace("}", "]\"");
			formula = Parse(formula, text).Replace("(", "{").Replace(")", "}");
		}
		if (formula.StartsWith("{") && formula.EndsWith("}"))
		{
			formula = formula.Substring(1, formula.Length - 2);
		}
		if (formula.Length == 1)
		{
			return formula;
		}
		int num = formula.IndexOf(":");
		string[] array;
		if (Engine.IsCellReference(formula) && num > -1)
		{
			string[] cellsFromArgs = Engine.GetCellsFromArgs(formula);
			string text2 = string.Empty;
			string text3 = ",";
			int num2 = Engine.RowIndex(formula.Substring(0, num));
			int num3 = Engine.RowIndex(formula.Substring(num + 1));
			int num4 = Engine.ColIndex(formula.Substring(0, num));
			int num5 = Engine.ColIndex(formula.Substring(num + 1));
			int num6 = num3 - num2 + 1;
			int num7 = num5 - num4 + 1;
			if (num7 == 1)
			{
				text3 = ";";
			}
			else if (num6 == 1)
			{
				text3 = ",";
			}
			array = cellsFromArgs;
			foreach (string text4 in array)
			{
				text2 = text2 + text4 + text3;
			}
			formula = text2.Substring(0, text2.Length - 1).Replace(" ", "");
			text = formula;
			if (num6 > 1 && num7 > 1)
			{
				return ParseRangeArray(formula, num6, num7);
			}
		}
		if (text.Contains(",") && !text.Contains(";"))
		{
			return ParseHorizontalArray(formula);
		}
		if (text.Contains(";") && !text.Contains(","))
		{
			return ParseVerticalArray(formula);
		}
		if (text.IndexOfAny(markers) > -1)
		{
			string[] substrings = SplitString(text);
			return ParseRangeArray(formula, GetHeight(substrings), GetWidth(substrings));
		}
		string[] array2 = Engine.SplitArguments(formula, ';');
		List<string[]> list = new List<string[]>();
		List<string> list2 = new List<string>();
		int height = array2.Length;
		int width = 0;
		array = array2;
		foreach (string args in array)
		{
			string[] array3 = Engine.SplitArgsPreservingQuotedCommas(args);
			list.Add(array3);
			width = array3.Length;
		}
		for (int j = 0; j < height; j++)
		{
			for (int k = 0; k < width; k++)
			{
				list2.Add(list[j][k]);
			}
		}
		if (Engine.cell == string.Empty)
		{
			return list2[0];
		}
		int num8 = width;
		int num9 = GetPosition(ref height, ref width);
		if (num8 > width && num9 != -1 && num9 >= width)
		{
			num9 += num9 / width * (num8 - width);
		}
		formula = ((num9 <= -1) ? Engine.ErrorStrings[0].ToString() : list2[num9]);
		return formula;
	}

	internal string ParseRangeArray(string formula, int height, int width)
	{
		string[] array = Engine.SplitArgsPreservingQuotedCommas(formula);
		if (Engine.cell == string.Empty)
		{
			return array[0];
		}
		int num = width;
		int num2 = GetPosition(ref height, ref width);
		if (num > width && num2 != -1 && num2 >= width)
		{
			num2 += num2 / width * (num - width);
		}
		formula = ((num2 <= -1) ? Engine.ErrorStrings[0].ToString() : array[num2]);
		return formula;
	}

	internal string ParseHorizontalArray(string formula)
	{
		int height = 1048576;
		string[] array = Engine.SplitArgsPreservingQuotedCommas(formula);
		int width = array.Length;
		if (Engine.cell == string.Empty)
		{
			return array[0];
		}
		int position = GetPosition(ref height, ref width);
		position = ((position > -1) ? (position % width) : position);
		formula = ((position <= -1) ? Engine.ErrorStrings[0].ToString() : array[position]);
		return formula;
	}

	internal string ParseVerticalArray(string formula)
	{
		int width = 16384;
		string[] array = Engine.SplitArguments(formula, ';');
		int height = array.Length;
		if (Engine.cell == string.Empty)
		{
			return array[0];
		}
		int position = GetPosition(ref height, ref width);
		position = ((position > -1) ? (position / width) : position);
		formula = ((position <= -1) ? Engine.ErrorStrings[0].ToString() : array[position]);
		return formula;
	}

	internal int GetHeight(string[] substrings)
	{
		int result = 0;
		_ = string.Empty;
		int result2 = 0;
		substrings[0] = substrings[0].Replace("$", "");
		if (Engine.IsCellReference(substrings[0]))
		{
			result = (int.TryParse(Engine.ComputeRows(substrings[0]), out result) ? result : 0);
		}
		else if (substrings[0].Contains(",") && !substrings[0].Contains(";"))
		{
			result = 1;
		}
		else if (substrings[0].Contains(";") && !substrings[0].Contains(","))
		{
			result = Engine.SplitArguments(substrings[0], ';').Length;
		}
		for (int i = 0; i < substrings.Length; i++)
		{
			if (result == 0)
			{
				result = 1;
			}
			substrings[i] = substrings[i].Replace("$", "");
			if (Engine.IsCellReference(substrings[i]))
			{
				result2 = (int.TryParse(Engine.ComputeRows(substrings[i]), out result2) ? result2 : 0);
			}
			else if (substrings[i].Contains(",") && !substrings[i].Contains(";"))
			{
				result2 = 1;
			}
			else if (substrings[i].Contains(";") && !substrings[i].Contains(","))
			{
				result2 = Engine.SplitArguments(substrings[i], ';').Length;
			}
			if (result2 != 0 && (result == 1 || (result2 < result && result2 != 1)))
			{
				result = result2;
			}
		}
		return result;
	}

	internal int GetWidth(string[] substrings)
	{
		int result = 0;
		_ = string.Empty;
		int result2 = 0;
		substrings[0] = substrings[0].Replace("$", "");
		if (Engine.IsCellReference(substrings[0]))
		{
			result = (int.TryParse(Engine.ComputeColumns(substrings[0]), out result) ? result : 0);
		}
		else if (substrings[0].Contains(",") && !substrings[0].Contains(";"))
		{
			result = Engine.SplitArgsPreservingQuotedCommas(substrings[0]).Length;
		}
		else if (substrings[0].Contains(";") && !substrings[0].Contains(","))
		{
			result = 1;
		}
		for (int i = 0; i < substrings.Length; i++)
		{
			substrings[i] = substrings[i].Replace("$", "");
			if (result == 0)
			{
				result = 1;
			}
			if (Engine.IsCellReference(substrings[i]))
			{
				result2 = (int.TryParse(Engine.ComputeColumns(substrings[i]), out result2) ? result2 : 0);
			}
			else if (substrings[i].Contains(",") && !substrings[0].Contains(";"))
			{
				result2 = Engine.SplitArgsPreservingQuotedCommas(substrings[i]).Length;
			}
			else if (substrings[i].Contains(";") && !substrings[0].Contains(","))
			{
				result2 = 1;
			}
			if (result2 != 0 && (result == 1 || (result2 < result && result2 != 1)))
			{
				result = result2;
			}
		}
		return result;
	}

	internal int GetPosition(ref int height, ref int width)
	{
		int num = Engine.RowIndex(Engine.cell);
		if (num == -1 && Engine.grid is ISheetData)
		{
			num = ((ISheetData)Engine.grid).GetFirstRow();
		}
		int num2 = Engine.ColIndex(Engine.cell);
		if (num2 == -1 && Engine.grid is ISheetData)
		{
			num2 = ((ISheetData)Engine.grid).GetFirstColumn();
		}
		return GetArrayRecordPosition(num, num2, ref height, ref width, Engine.grid);
	}

	internal string ComputeLen(string arg, int computedLevel)
	{
		string empty = string.Empty;
		string text = string.Empty;
		string empty2 = string.Empty;
		if (arg.IndexOf(':') > -1 && computedLevel > 0)
		{
			string[] cellsFromArgs = Engine.GetCellsFromArgs(arg);
			foreach (string arg2 in cellsFromArgs)
			{
				empty = Engine.GetValueFromArg(arg2).Replace("\"", string.Empty);
				if (empty != string.Empty)
				{
					empty2 = empty.Length.ToString();
					text = text + empty2 + CalcEngine.ParseArgumentSeparator;
				}
			}
			text = text.Remove(text.Length - 1);
		}
		else if (computedLevel > 0)
		{
			string[] cellsFromArgs = Engine.SplitArgsPreservingQuotedCommas(arg);
			foreach (string arg3 in cellsFromArgs)
			{
				empty = Engine.GetValueFromArg(arg3).Replace("\"", string.Empty);
				empty2 = empty.Length.ToString();
				text = text + empty2 + CalcEngine.ParseArgumentSeparator;
			}
			text = text.Remove(text.Length - 1);
		}
		else if (arg.IndexOf(':') > -1)
		{
			if (Engine.cell != string.Empty)
			{
				string[] cellsFromArgs2 = Engine.GetCellsFromArgs(arg);
				int result = (int.TryParse(Engine.ComputeRows(arg), out result) ? result : 0);
				int result2 = (int.TryParse(Engine.ComputeColumns(arg), out result2) ? result2 : 0);
				int position = GetPosition(ref result, ref result2);
				if (position >= 0 && cellsFromArgs2.Length > position)
				{
					empty = Engine.GetValueFromArg(cellsFromArgs2[position]).Replace("\"", string.Empty);
					if (empty != string.Empty)
					{
						text = empty.Length.ToString();
					}
				}
				else
				{
					text = Engine.ErrorStrings[0].ToString();
				}
			}
			else
			{
				text = string.Empty;
			}
		}
		else
		{
			string[] array = Engine.SplitArgsPreservingQuotedCommas(arg);
			empty = Engine.GetValueFromArg(array[0]).Replace("\"", string.Empty);
			if (empty != string.Empty)
			{
				text = empty.Length.ToString();
			}
		}
		return text;
	}

	internal string ComputeRow(string arg, int computedLevel)
	{
		string text = string.Empty;
		arg = arg.Replace("\"", "");
		string[] cellsFromArgs = Engine.GetCellsFromArgs(arg);
		int num = Engine.RowIndex(cellsFromArgs[0].ToString());
		int num2 = Engine.RowIndex(cellsFromArgs[^1].ToString());
		if (computedLevel > 0)
		{
			for (int i = num; i <= num2; i++)
			{
				text = text + i + CalcEngine.ParseArgumentSeparator;
			}
			return text.Remove(text.Length - 1);
		}
		if (Engine.cell != string.Empty)
		{
			string[] array = new string[num2 - num + 1];
			int num3 = 0;
			for (int j = num; j <= num2; j++)
			{
				array[num3] = j.ToString();
				num3++;
			}
			int result = (int.TryParse(Engine.ComputeRows(arg), out result) ? result : 0);
			int result2 = (int.TryParse(Engine.ComputeColumns(arg), out result2) ? result2 : 0);
			if (result2 == 1)
			{
				result2 = 16384;
			}
			int position = GetPosition(ref result, ref result2);
			position = ((position > -1) ? (position / result2) : position);
			if (position > -1)
			{
				return array[position].ToString();
			}
			return Engine.ErrorStrings[0].ToString();
		}
		return string.Empty;
	}

	internal string ComputeColumn(string arg, int computedLevel)
	{
		string text = string.Empty;
		arg = arg.Replace("\"", "");
		string[] cellsFromArgs = Engine.GetCellsFromArgs(arg);
		int num = Engine.ColIndex(cellsFromArgs[0].ToString());
		int num2 = Engine.ColIndex(cellsFromArgs[^1].ToString());
		if (computedLevel > 0)
		{
			for (int i = num; i <= num2; i++)
			{
				text = text + i + CalcEngine.ParseArgumentSeparator;
			}
			return text.Remove(text.Length - 1);
		}
		if (Engine.cell != string.Empty)
		{
			string[] array = new string[num2 - num + 1];
			int num3 = 0;
			for (int j = num; j <= num2; j++)
			{
				array[num3] = j.ToString();
				num3++;
			}
			int result = (int.TryParse(Engine.ComputeRows(arg), out result) ? result : 0);
			int result2 = (int.TryParse(Engine.ComputeColumns(arg), out result2) ? result2 : 0);
			if (result == 1)
			{
				result = 1048576;
			}
			int position = GetPosition(ref result, ref result2);
			position = ((position > -1) ? (position % result2) : position);
			if (position > -1)
			{
				return array[position].ToString();
			}
			return Engine.ErrorStrings[0].ToString();
		}
		return string.Empty;
	}

	internal string ComputeIF(string arg, int computedLevel)
	{
		string text = string.Empty;
		arg = arg.Replace('\u0092'.ToString(), string.Empty);
		string[] array = Engine.SplitArgsPreservingQuotedCommas(arg);
		string[] array2 = null;
		if (array.GetLength(0) <= 3)
		{
			for (int i = 0; i < array[0].Length; i++)
			{
				string text2 = string.Empty;
				string text3 = string.Empty;
				string empty = string.Empty;
				while (i != array[0].Length && (char.IsDigit(array[0][i]) | (array[0][i] == ':') | (array[0][i] == '!') | (i != 0 && Engine.IsUpper(array[0][i]) && !char.IsDigit(array[0][i - 1])) | (i == 0 && Engine.IsUpper(array[0][i]))))
				{
					text2 += array[0][i++];
				}
				while (i != array[0].Length && ((array[0][i] == '"') | char.IsLetter(array[0][i]) | char.IsDigit(array[0][i])))
				{
					text3 += array[0][i++];
				}
				array2 = Engine.GetCellsFromArgs(text2);
				if (computedLevel > 0)
				{
					string[] array3 = null;
					string[] array4 = null;
					if (array[1] != null)
					{
						array3 = Engine.GetCellsFromArgs(array[1]);
					}
					if (array.Length > 2 && array[2] != null)
					{
						array4 = Engine.GetCellsFromArgs(array[2]);
					}
					for (int j = 0; j <= array2.Length - 1; j++)
					{
						empty = Engine.GetValueFromArg('\u0092' + array2[j] + text3 + '\u0092');
						text = ((empty.ToUpper().Replace("\"", "").ToUpper()
							.Equals("TRUE") && Engine.IsRange(array[1])) ? PerformLogicalTestForRange(empty, text, computedLevel, array3[j]) : ((array.Length < 3 && empty.ToUpper().Replace("\"", "").ToUpper()
							.Equals("FALSE")) ? PerformLogicalTestForRange(empty, text, computedLevel, "FALSE") : ((!empty.ToUpper().Replace("\"", "").ToUpper()
							.Equals("FALSE") || !Engine.IsRange(array[2])) ? PerformLogicalTest(empty, text, computedLevel, array) : PerformLogicalTestForRange(empty, text, computedLevel, array4[j]))));
					}
				}
				else if (Engine.cell != string.Empty)
				{
					empty = Engine.GetValueFromArg('\u0092' + text2 + text3 + '\u0092');
					text = PerformLogicalTest(empty, text, computedLevel, array);
				}
				text = text.Remove(text.Length - 1);
			}
		}
		else
		{
			text = string.Empty;
		}
		if (text != string.Empty)
		{
			text = "\"" + text + "\"";
		}
		return text;
	}

	internal string PerformLogicalTestForRange(string logicTest, string strArray, int computedLevel, string logicalRange)
	{
		double result = 0.0;
		if (logicTest.ToUpper().Replace("\"", "").ToUpper()
			.Equals("TRUE") || (double.TryParse(logicTest, NumberStyles.Any, null, out result) && result != 0.0))
		{
			logicTest = Engine.GetValueFromArg(logicalRange);
			if (string.IsNullOrEmpty(logicTest) && Engine.TreatStringsAsZero && computedLevel > 0)
			{
				logicTest = "0";
			}
			strArray = strArray + logicTest + ";";
		}
		else if (logicTest.ToUpper().Replace("\"", "").ToUpper()
			.Equals("FALSE") || (double.TryParse(logicTest, NumberStyles.Any, null, out result) && result != 0.0))
		{
			logicTest = Engine.GetValueFromArg(logicalRange);
			if (string.IsNullOrEmpty(logicTest) && Engine.TreatStringsAsZero && computedLevel > 0)
			{
				logicTest = "0";
			}
			strArray = strArray + logicTest + ";";
		}
		return strArray;
	}

	internal string PerformLogicalTest(string logicTest, string strArray, int computedLevel, string[] ss)
	{
		double result = 0.0;
		if (logicTest.ToUpper().Replace("\"", "").ToUpper()
			.Equals("TRUE") || (double.TryParse(logicTest, NumberStyles.Any, null, out result) && result != 0.0))
		{
			logicTest = Engine.GetValueFromArg(ss[1]);
			if (string.IsNullOrEmpty(logicTest) && Engine.TreatStringsAsZero && computedLevel > 0)
			{
				logicTest = "0";
			}
			strArray = strArray + logicTest + ";";
		}
		else if (logicTest.ToUpper().Replace("\"", "").ToUpper()
			.Equals("FALSE") || (double.TryParse(logicTest, NumberStyles.Any, null, out result) && result != 0.0))
		{
			logicTest = Engine.GetValueFromArg(ss[2]);
			if (string.IsNullOrEmpty(logicTest) && Engine.TreatStringsAsZero && computedLevel > 0)
			{
				logicTest = "0";
			}
			strArray = strArray + logicTest + ";";
		}
		return strArray;
	}

	internal string ComputeCountIF(string[] s1, char op, string criteria, bool isNumber, double compare, int computedLevel, int count)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string text = string.Empty;
		string[] array = null;
		for (int i = 0; i < criteria.Length; i++)
		{
			string text2 = string.Empty;
			string text3 = string.Empty;
			string empty = string.Empty;
			string text4 = string.Empty;
			criteria = criteria.Replace('\u0092'.ToString(), string.Empty);
			while (i != criteria.Length && criteria[i] == '*')
			{
				text4 += criteria[i++];
			}
			while (i != criteria.Length && (char.IsDigit(criteria[i]) | (criteria[i] == ':') | (criteria[i] == '!') | Engine.IsUpper(criteria[i])))
			{
				text2 += criteria[i++];
			}
			while (i != criteria.Length && ((criteria[i] == '"') | (criteria[i] == '*') | char.IsLetter(criteria[i]) | char.IsDigit(criteria[i])))
			{
				text3 += criteria[i++];
			}
			array = Engine.GetCellsFromArgs(text2);
			for (int j = 0; j <= array.Length - 1; j++)
			{
				int num = 0;
				empty = Engine.GetValueFromArg('\u0092' + text4 + array[j] + text3 + '\u0092');
				empty = empty.Replace("\"", string.Empty);
				if (!dictionary.ContainsKey(empty))
				{
					for (int k = 0; k < count; k++)
					{
						string valueFromArg = Engine.GetValueFromArg(s1[k]);
						if (Engine.CheckForCriteriaMatch(valueFromArg.ToUpper(), op, empty.ToUpper(), isNumber, compare))
						{
							num++;
						}
					}
					dictionary.Add(empty, num);
				}
				text = text + (dictionary.ContainsKey(empty) ? dictionary[empty].ToString() : num.ToString()) + ";";
			}
		}
		return text.Remove(text.Length - 1);
	}

	public void Dispose()
	{
		Engine = null;
	}
}
