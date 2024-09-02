using System;
using System.Collections.Generic;
using System.Globalization;

namespace DocGen.OfficeChart.Calculate;

internal class CalcQuickBase : ISheetData, ICalcData, IDisposable
{
	private int _calcQuickID;

	private Dictionary<object, object> _controlModifiedFlags;

	private FormulaInfoHashtable _dataStore;

	private CalcEngine _engine;

	private Dictionary<object, object> _keyToRowsMap;

	private Dictionary<object, object> _keyToVectors;

	private Dictionary<object, object> _nameToControlMap;

	private Dictionary<object, object> _rowsToKeyMap;

	private bool autoCalc;

	private string cellPrefix = "!0!A";

	private bool checkKeys = true;

	private bool disposeEngineResource = true;

	protected bool ignoreChanges;

	private const string LEFTBRACE = "{";

	private string TIC = "\"";

	private const char LEFTBRACKET = '[';

	private const char RIGHTBRACKET = ']';

	private string VALIDLEFTCHARS = "+-*/><=^(&" + CalcEngine.ParseArgumentSeparator;

	private string VALIDRIGHTCHARS = "+-*/><=^)&" + CalcEngine.ParseArgumentSeparator;

	public bool AutoCalc
	{
		get
		{
			return autoCalc;
		}
		set
		{
			autoCalc = value;
			Engine.CalculatingSuspended = !value;
			Engine.IgnoreValueChanged = !value;
			Engine.UseDependencies = value;
			if (value)
			{
				SetDirty();
			}
		}
	}

	private int calcQuickID
	{
		get
		{
			_calcQuickID++;
			if (_calcQuickID == int.MaxValue)
			{
				_calcQuickID = 1;
			}
			return _calcQuickID;
		}
	}

	public bool CheckKeys
	{
		get
		{
			return checkKeys;
		}
		set
		{
			checkKeys = value;
		}
	}

	protected Dictionary<object, object> ControlModifiedFlags => _controlModifiedFlags;

	protected FormulaInfoHashtable DataStore => _dataStore;

	public bool DisposeEngineResource
	{
		get
		{
			return disposeEngineResource;
		}
		set
		{
			disposeEngineResource = value;
		}
	}

	public CalcEngine Engine => _engine;

	public char FormulaCharacter
	{
		get
		{
			return CalcEngine.FormulaCharacter;
		}
		set
		{
			CalcEngine.FormulaCharacter = value;
		}
	}

	protected Dictionary<object, object> KeyToRowsMap => _keyToRowsMap;

	protected Dictionary<object, object> KeyToVectors => _keyToVectors;

	protected Dictionary<object, object> NameToControlMap => _nameToControlMap;

	protected Dictionary<object, object> RowsToKeyMap => _rowsToKeyMap;

	public string this[string key]
	{
		get
		{
			key = key.ToUpper();
			if (DataStore.ContainsKey(key))
			{
				FormulaInfo formulaInfo = DataStore[key];
				string formulaText = formulaInfo.FormulaText;
				string text = string.Empty;
				if (formulaText.Length > 0 && formulaText[0] == CalcEngine.FormulaCharacter && formulaInfo.calcID != Engine.GetCalcID())
				{
					Engine.cell = cellPrefix + KeyToRowsMap[key].ToString();
					text = Engine.cell;
					formulaText = formulaText.Substring(1);
					try
					{
						formulaInfo.ParsedFormula = Engine.ParseFormula(MarkKeys(formulaText));
					}
					catch (Exception ex)
					{
						if (CheckKeys)
						{
							formulaInfo.FormulaValue = ex.Message;
							formulaInfo.calcID = Engine.GetCalcID();
							if (this.ValueSet != null)
							{
								this.ValueSet(this, new QuickValueSetEventArgs(key, formulaInfo.FormulaValue, FormulaInfoSetAction.CalculatedValueSet));
							}
							return DataStore[key].FormulaValue;
						}
					}
					try
					{
						formulaInfo.FormulaValue = Engine.ComputeFormula(formulaInfo.ParsedFormula);
					}
					catch (Exception ex2)
					{
						if (ThrowCircularException && ex2.Message.StartsWith(Engine.FormulaErrorStrings[Engine.circular_reference_]))
						{
							throw ex2;
						}
					}
					formulaInfo.calcID = Engine.GetCalcID();
					if (this.ValueSet != null)
					{
						this.ValueSet(this, new QuickValueSetEventArgs(key, formulaInfo.FormulaValue, FormulaInfoSetAction.CalculatedValueSet));
					}
				}
				if (Engine.ThrowCircularException && Engine.IterationMaxCount > 0)
				{
					if (text != Engine.cell && text != string.Empty)
					{
						formulaInfo.FormulaValue = Engine.HandleIteration(text, formulaInfo);
					}
					else
					{
						formulaInfo.FormulaValue = Engine.HandleIteration(Engine.cell, formulaInfo);
					}
				}
				return DataStore[key].FormulaValue;
			}
			if (KeyToVectors.ContainsKey(key))
			{
				return KeyToVectors[key].ToString();
			}
			return string.Empty;
		}
		set
		{
			key = key.ToUpper();
			string text = value.ToString().Trim();
			if (!DataStore.ContainsKey(key) || text.StartsWith("{"))
			{
				if (text.StartsWith("{"))
				{
					if (!KeyToVectors.ContainsKey(key))
					{
						KeyToVectors.Add(key, string.Empty);
					}
					text = text.Substring(1, text.Length - 2);
					int num = KeyToRowsMap.Count + 1;
					string[] array = text.Split(new char[1] { CalcEngine.ParseArgumentSeparator });
					string value2 = $"A{num}:A{num + array.GetLength(0) - 1}";
					KeyToVectors[key] = value2;
					string[] array2 = array;
					foreach (string formulaText in array2)
					{
						string text2 = $"Q_{KeyToRowsMap.Count + 1}";
						DataStore.Add(text2, new FormulaInfo());
						KeyToRowsMap.Add(text2, KeyToRowsMap.Count + 1);
						RowsToKeyMap.Add(RowsToKeyMap.Count + 1, text2);
						FormulaInfo formulaInfo = DataStore[text2];
						formulaInfo.FormulaText = string.Empty;
						formulaInfo.ParsedFormula = string.Empty;
						formulaInfo.FormulaValue = ParseAndCompute(formulaText);
					}
					return;
				}
				DataStore.Add(key, new FormulaInfo());
				KeyToRowsMap.Add(key, KeyToRowsMap.Count + 1);
				RowsToKeyMap.Add(RowsToKeyMap.Count + 1, key);
			}
			if (KeyToVectors.ContainsKey(key))
			{
				KeyToVectors.Remove(key);
			}
			FormulaInfo formulaInfo2 = DataStore[key];
			if (!ignoreChanges && formulaInfo2.FormulaText != null && formulaInfo2.FormulaText.Length > 0 && formulaInfo2.FormulaText != text)
			{
				string text3 = cellPrefix + KeyToRowsMap[key].ToString();
				if (Engine.DependentFormulaCells.ContainsKey(text3) && Engine.DependentFormulaCells[text3] != null)
				{
					Engine.ClearFormulaDependentCells(text3);
				}
			}
			if (text.Length > 0 && text[0] == CalcEngine.FormulaCharacter)
			{
				formulaInfo2.FormulaText = text;
				if (this.ValueSet != null)
				{
					this.ValueSet(this, new QuickValueSetEventArgs(key, text, FormulaInfoSetAction.FormulaSet));
				}
			}
			else if (formulaInfo2.FormulaValue != text)
			{
				formulaInfo2.FormulaText = string.Empty;
				formulaInfo2.ParsedFormula = string.Empty;
				formulaInfo2.FormulaValue = text;
				if (this.ValueSet != null)
				{
					this.ValueSet(this, new QuickValueSetEventArgs(key, text, FormulaInfoSetAction.NonFormulaSet));
				}
			}
			if (AutoCalc)
			{
				UpdateDependencies(key);
			}
		}
	}

	public bool ThrowCircularException
	{
		get
		{
			return Engine.ThrowCircularException;
		}
		set
		{
			Engine.ThrowCircularException = value;
		}
	}

	public event ValueChangedEventHandler ValueChanged;

	public event QuickValueSetEventHandler ValueSet;

	public CalcQuickBase()
	{
		InitCalcQuick(resetStaticMembers: false);
	}

	public CalcQuickBase(bool resetStaticMembers)
	{
		InitCalcQuick(resetStaticMembers);
	}

	public void ResetKeys()
	{
		DataStore.Clear();
		KeyToRowsMap.Clear();
		RowsToKeyMap.Clear();
		KeyToVectors.Clear();
		NameToControlMap.Clear();
	}

	private bool CheckAdjacentPiece(string s, string validChars, bool first)
	{
		bool result = true;
		s = s.Trim();
		if (s.Length > 0)
		{
			result = validChars.IndexOf(s[(!first) ? (s.Length - 1) : 0]) > -1;
		}
		return result;
	}

	public virtual CalcEngine CreateEngine()
	{
		return new CalcEngine(this);
	}

	public void Dispose()
	{
		_dataStore = null;
		_rowsToKeyMap = null;
		_keyToRowsMap = null;
		_keyToVectors = null;
		_controlModifiedFlags = null;
		_nameToControlMap = null;
		ValueChanged -= _engine.grid_ValueChanged;
		if (DisposeEngineResource)
		{
			_engine.DependentFormulaCells.Clear();
			_engine.DependentCells.Clear();
			if (_engine != null)
			{
				_engine.Dispose();
			}
			_engine = null;
		}
	}

	public string TryParseAndCompute(string formulaText)
	{
		string text = "";
		try
		{
			return ParseAndCompute(formulaText);
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public string GetFormula(string key)
	{
		key = key.ToUpper();
		if (DataStore.ContainsKey(key))
		{
			return DataStore[key].FormulaText;
		}
		return string.Empty;
	}

	public object GetValueRowCol(int row, int col)
	{
		string key = RowsToKeyMap[row].ToString();
		string text = this[key].ToString();
		if (text != null && text.EndsWith("%") && text.Length > 1 && double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Any, null, out var result))
		{
			text = (result / 100.0).ToString();
		}
		return text;
	}

	protected void InitCalcQuick(bool resetStaticMembers)
	{
		_dataStore = new FormulaInfoHashtable();
		_rowsToKeyMap = new Dictionary<object, object>();
		_keyToRowsMap = new Dictionary<object, object>();
		_keyToVectors = new Dictionary<object, object>();
		_controlModifiedFlags = new Dictionary<object, object>();
		_nameToControlMap = new Dictionary<object, object>();
		_engine = CreateEngine();
		if (resetStaticMembers)
		{
			CalcEngine.ResetSheetFamilyID();
			_engine.DependentFormulaCells.Clear();
			_engine.DependentCells.Clear();
		}
		int num = CalcEngine.CreateSheetFamilyID();
		cellPrefix = $"!{num}!A";
		_engine.RegisterGridAsSheet(RangeInfo.GetAlphaLabel(calcQuickID), this, num);
		_engine.CalculatingSuspended = true;
		_engine.IgnoreValueChanged = true;
	}

	private string MarkKeys(string formula)
	{
		int num = formula.IndexOf('[');
		while (num > -1)
		{
			int num2 = formula.Substring(num).IndexOf(']') - 1;
			string empty = string.Empty;
			if (num2 > 0)
			{
				empty = formula.Substring(num + 1, num2).ToUpper();
				if (KeyToVectors.ContainsKey(empty))
				{
					string text = ((num + num2 + 2 < formula.Length) ? formula.Substring(num + num2 + 2) : string.Empty);
					if (CheckKeys && !CheckAdjacentPiece(text, VALIDRIGHTCHARS, first: true))
					{
						throw new ArgumentException($"[{empty}] not followed properly");
					}
					string text2 = ((num > 0) ? formula.Substring(0, num) : string.Empty);
					if (CheckKeys && !CheckAdjacentPiece(text2, VALIDLEFTCHARS, first: false))
					{
						throw new ArgumentException($"[{empty}] not preceded properly");
					}
					formula = text2 + KeyToVectors[empty].ToString() + text;
					num = formula.IndexOf('[');
					continue;
				}
				if (!KeyToRowsMap.ContainsKey(empty))
				{
					if (formula.ToUpper().IndexOf(TIC + "[" + empty + "]" + TIC) > 0)
					{
						break;
					}
					throw new ArgumentException("Unknown key: " + empty);
				}
				string text3 = ((num + num2 + 2 < formula.Length) ? formula.Substring(num + num2 + 2) : string.Empty);
				if (CheckKeys && !CheckAdjacentPiece(text3, VALIDRIGHTCHARS, first: true))
				{
					throw new ArgumentException($"[{empty}] not followed properly");
				}
				string text4 = ((num > 0) ? formula.Substring(0, num) : string.Empty);
				if (CheckKeys && !CheckAdjacentPiece(text4, VALIDLEFTCHARS, first: false))
				{
					throw new ArgumentException($"[{empty}] not preceded properly");
				}
				formula = text4 + "A" + KeyToRowsMap[empty].ToString() + text3;
				num = formula.IndexOf('[');
			}
			else
			{
				num = -1;
			}
		}
		return formula;
	}

	public string ParseAndCompute(string formulaText)
	{
		if (formulaText.Length > 0 && formulaText[0] == CalcEngine.FormulaCharacter)
		{
			formulaText = formulaText.Substring(1);
		}
		return Engine.ParseAndComputeFormula(MarkKeys(formulaText));
	}

	public void RefreshAllCalculations()
	{
		if (!AutoCalc)
		{
			return;
		}
		SetDirty();
		ignoreChanges = true;
		foreach (string key in DataStore.Keys)
		{
			FormulaInfo formulaInfo = DataStore[key];
			string formulaText = formulaInfo.FormulaText;
			if (formulaText.Length > 0 && formulaText[0] == CalcEngine.FormulaCharacter && formulaInfo.calcID != Engine.GetCalcID())
			{
				formulaText = formulaText.Substring(1);
				Engine.cell = cellPrefix + KeyToRowsMap[key].ToString();
				formulaInfo.ParsedFormula = Engine.ParseFormula(MarkKeys(formulaText));
				formulaInfo.FormulaValue = Engine.ComputeFormula(formulaInfo.ParsedFormula);
				formulaInfo.calcID = Engine.GetCalcID();
				if (this.ValueChanged != null)
				{
					this.ValueChanged(this, new ValueChangedEventArgs((int)KeyToRowsMap[key], 1, formulaInfo.FormulaValue));
				}
			}
			if (this.ValueSet != null)
			{
				this.ValueSet(this, new QuickValueSetEventArgs(key, formulaInfo.FormulaValue, FormulaInfoSetAction.CalculatedValueSet));
			}
		}
		ignoreChanges = false;
	}

	public void SetDirty()
	{
		Engine.UpdateCalcID();
	}

	public void SetValueRowCol(object value, int row, int col)
	{
	}

	public void UpdateDependencies(string key)
	{
		if (!AutoCalc)
		{
			return;
		}
		string key2 = cellPrefix + KeyToRowsMap[key].ToString();
		List<object> list = Engine.DependentCells[key2] as List<object>;
		SetDirty();
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			int num = item.IndexOf('A');
			if (num > -1)
			{
				num = int.Parse(item.Substring(num + 1));
				key = RowsToKeyMap[num].ToString();
				ignoreChanges = true;
				this[key] = this[key];
				ignoreChanges = false;
			}
		}
	}

	public void WireParentObject()
	{
	}

	public int GetFirstRow()
	{
		return 1;
	}

	public int GetLastRow()
	{
		return RowsToKeyMap.Count;
	}

	public int GetRowCount()
	{
		return RowsToKeyMap.Count;
	}

	public int GetFirstColumn()
	{
		return 1;
	}

	public int GetLastColumn()
	{
		return 25;
	}

	public int GetColumnCount()
	{
		return 25;
	}
}
