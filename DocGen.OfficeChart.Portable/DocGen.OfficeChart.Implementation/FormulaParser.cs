using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class FormulaParser
{
	private const string DEF_IF_FUNCTION = "IF";

	private const int DEF_SPACE_OPTIONS = 64;

	private const int DEF_SPACE_DATA = 256;

	private const int DDELinkNameOptions = 32738;

	private const char AbsoluteCellReference = '$';

	private FormulaTokenizer m_tokenizer;

	private List<Ptg> m_arrTokens = new List<Ptg>();

	private Stack<AttrPtg> m_tokenSpaces = new Stack<AttrPtg>();

	private WorkbookImpl m_book;

	public List<Ptg> Tokens => m_arrTokens;

	public NumberFormatInfo NumberFormat
	{
		get
		{
			return m_tokenizer.NumberFormat;
		}
		set
		{
			m_tokenizer.NumberFormat = value;
		}
	}

	public FormulaParser(WorkbookImpl book)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		m_book = book;
		m_tokenizer = new FormulaTokenizer(book);
	}

	public void SetSeparators(char operandsSeparator, char arrayRowsSeparator)
	{
		m_tokenizer.ArgumentSeparator = operandsSeparator;
	}

	public void Parse(string formula, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		formula = formula.TrimEnd(' ');
		if (formula.Length == 0)
		{
			throw new ArgumentException("formula - string cannot be empty.");
		}
		CultureInfo cultureInfo = m_book.AppImplementation.CheckAndApplySeperators();
		m_tokenizer.NumberFormat = cultureInfo.NumberFormat;
		m_arrTokens.Clear();
		m_tokenSpaces.Clear();
		m_tokenizer.Prepare(formula);
		m_tokenizer.NextToken();
		AttrPtg attrPtg = null;
		ParseExpression(Priority.None, indexes, i, options, arguments);
		if (m_tokenSpaces.Count > 0)
		{
			attrPtg = m_tokenSpaces.Pop();
			attrPtg.SpaceAfterToken = true;
			m_arrTokens.Add(attrPtg);
			attrPtg = null;
		}
	}

	private Ptg ParseExpression(Priority priority, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		Ptg ptg = null;
		ptg = ParseFirstOperand(priority, indexes, i, ref options, arguments);
		m_arrTokens.Add(ptg);
		AttrPtg tokenSpace = null;
		if (ptg == null)
		{
			m_tokenizer.RaiseUnexpectedToken("No Expression found");
		}
		while (true)
		{
			FormulaToken tokenType = m_tokenizer.TokenType;
			switch (tokenType)
			{
			case FormulaToken.EndOfFormula:
				return ptg;
			case FormulaToken.tAdd:
				if (priority >= Priority.PlusMinus)
				{
					if (tokenSpace != null)
					{
						m_tokenSpaces.Push(tokenSpace);
						tokenSpace = null;
					}
					return ptg;
				}
				m_tokenizer.NextToken();
				ParseExpression(Priority.PlusMinus, indexes, i, options, arguments);
				ptg = CreateBinaryOperation(tokenType, ref tokenSpace);
				break;
			case FormulaToken.tSub:
				if (priority >= Priority.PlusMinus)
				{
					if (tokenSpace != null)
					{
						m_tokenSpaces.Push(tokenSpace);
						tokenSpace = null;
					}
					return ptg;
				}
				m_tokenizer.NextToken();
				ParseExpression(Priority.PlusMinus, indexes, i, options, arguments);
				ptg = CreateBinaryOperation(tokenType, ref tokenSpace);
				break;
			case FormulaToken.tPercent:
				m_tokenizer.NextToken();
				ptg = new UnaryOperationPtg("%");
				m_arrTokens.Add(ptg);
				break;
			case FormulaToken.tPower:
				if (priority >= Priority.Power)
				{
					return ptg;
				}
				m_tokenizer.NextToken();
				ParseExpression(Priority.Power, indexes, i, options, arguments);
				ptg = CreateBinaryOperation(FormulaToken.tPower, ref tokenSpace);
				break;
			case FormulaToken.tConcat:
				if (priority >= Priority.Concat)
				{
					return ptg;
				}
				m_tokenizer.NextToken();
				ParseExpression(Priority.Concat, indexes, i, options, arguments);
				ptg = CreateBinaryOperation(FormulaToken.tConcat, ref tokenSpace);
				break;
			case FormulaToken.tCellRange:
				if (priority >= Priority.CellRange)
				{
					return ptg;
				}
				UpdateOptions(ref options);
				m_tokenizer.NextToken();
				ParseExpression(Priority.CellRange, indexes, i, options, arguments);
				ptg = CreateBinaryOperation(FormulaToken.tCellRange, ref tokenSpace);
				break;
			case FormulaToken.tMul:
			case FormulaToken.tDiv:
				if (priority >= Priority.MulDiv)
				{
					return ptg;
				}
				UpdateOptions(ref options);
				m_tokenizer.NextToken();
				ParseExpression(Priority.MulDiv, indexes, i, options, arguments);
				_ = 5;
				ptg = CreateBinaryOperation(tokenType, ref tokenSpace);
				break;
			case FormulaToken.CloseParenthesis:
				if (tokenSpace != null)
				{
					m_tokenSpaces.Push(tokenSpace);
					tokenSpace = null;
				}
				return ptg;
			case FormulaToken.tLessThan:
			case FormulaToken.tLessEqual:
			case FormulaToken.tEqual:
			case FormulaToken.tGreaterEqual:
			case FormulaToken.tGreater:
			case FormulaToken.tNotEqual:
				if (priority >= Priority.Equality)
				{
					return ptg;
				}
				tokenType = m_tokenizer.TokenType;
				m_tokenizer.NextToken();
				ParseExpression(Priority.Equality, indexes, i, options, arguments);
				ptg = CreateBinaryOperation(tokenType, ref tokenSpace);
				break;
			case FormulaToken.Space:
				tokenSpace = ParseSpaces(indexes, i, options, arguments);
				continue;
			case FormulaToken.Comma:
				if ((options & (OfficeParseFormulaOptions.ParseOperand | OfficeParseFormulaOptions.ParseComplexOperand)) == 0)
				{
					m_tokenizer.NextToken();
					ParseExpression(Priority.None, indexes, i, options, arguments);
					ptg = new CellRangeListPtg(m_tokenizer.ArgumentSeparator.ToString());
					m_arrTokens.Add(ptg);
					break;
				}
				return ptg;
			case FormulaToken.tParentheses:
				return ptg;
			case FormulaToken.tError:
				if (tokenSpace == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				ptg = ParseError(indexes, i, options, arguments.Worksheet);
				m_tokenizer.NextToken();
				break;
			default:
				m_tokenizer.RaiseUnexpectedToken("Unexpected token.");
				break;
			}
			tokenSpace = null;
		}
	}

	private Ptg CreateBinaryOperation(FormulaToken tokenType, ref AttrPtg tokenSpace)
	{
		if (tokenSpace != null)
		{
			m_arrTokens.Add(tokenSpace);
			tokenSpace = null;
		}
		else if (m_tokenSpaces.Count > 0)
		{
			m_arrTokens.Add(m_tokenSpaces.Pop());
		}
		Ptg ptg = FormulaUtil.CreatePtgByType(tokenType);
		m_arrTokens.Add(ptg);
		return ptg;
	}

	private AttrPtg ParseSpaces(Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		int length = m_tokenizer.TokenString.Length;
		int num = 0;
		Ptg ptg = null;
		bool flag = true;
		FormulaToken previousTokenType = m_tokenizer.PreviousTokenType;
		if (previousTokenType == FormulaToken.CloseParenthesis || previousTokenType == FormulaToken.Identifier || previousTokenType == FormulaToken.Identifier3D)
		{
			m_tokenizer.NextToken();
			flag = false;
			FormulaToken tokenType = m_tokenizer.TokenType;
			if (tokenType == FormulaToken.Identifier || tokenType == FormulaToken.tParentheses || tokenType == FormulaToken.Identifier3D)
			{
				num = length - 1;
				ParseExpression(Priority.None, indexes, i, options, arguments);
				ptg = FormulaUtil.CreatePtgByType(FormulaToken.tCellRangeIntersection);
				m_arrTokens.Add(ptg);
				goto IL_00a8;
			}
		}
		num = length;
		if (flag)
		{
			m_tokenizer.NextToken();
		}
		goto IL_00a8;
		IL_00a8:
		AttrPtg attrPtg = null;
		if (num > 0)
		{
			attrPtg = CreateSpaceToken(num);
			if (ptg != null)
			{
				m_arrTokens.Insert(m_arrTokens.Count - 1, attrPtg);
				attrPtg = null;
			}
		}
		return attrPtg;
	}

	private AttrPtg CreateSpaceToken(int spaceCount)
	{
		AttrPtg obj = (AttrPtg)FormulaUtil.CreatePtg(FormulaToken.tAttr, 64, 256);
		obj.SpaceCount = spaceCount;
		return obj;
	}

	private Ptg ParseFirstOperand(Priority priority, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, ref OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		Ptg result = null;
		AttrPtg attrPtg = null;
		bool flag;
		do
		{
			flag = false;
			switch (m_tokenizer.TokenType)
			{
			case FormulaToken.tAdd:
				m_tokenizer.NextToken();
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				if ((options & OfficeParseFormulaOptions.ParseOperand) != 0)
				{
					options -= 8;
					options |= OfficeParseFormulaOptions.ParseComplexOperand;
				}
				result = ParseExpression(Priority.UnaryMinus, indexes, i, options, arguments);
				result = new UnaryOperationPtg("+");
				break;
			case FormulaToken.tSub:
				m_tokenizer.NextToken();
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				if ((options & OfficeParseFormulaOptions.ParseOperand) != 0)
				{
					options -= 8;
					options |= OfficeParseFormulaOptions.ParseComplexOperand;
				}
				result = ParseExpression(Priority.UnaryMinus, indexes, i, options, arguments);
				result = new UnaryOperationPtg("-");
				break;
			case FormulaToken.tParentheses:
			{
				if (attrPtg != null)
				{
					m_arrTokens.RemoveAt(m_arrTokens.Count - 1);
				}
				m_tokenizer.NextToken();
				OfficeParseFormulaOptions options2 = options & ~OfficeParseFormulaOptions.ParseOperand;
				result = ParseExpression(Priority.None, indexes, i, options2, arguments);
				if (m_tokenizer.TokenType != FormulaToken.CloseParenthesis)
				{
					m_tokenizer.RaiseUnexpectedToken("End parenthesis not found");
				}
				if (attrPtg != null)
				{
					m_arrTokens.Add(attrPtg);
				}
				else if (m_tokenSpaces.Count > 0)
				{
					attrPtg = m_tokenSpaces.Pop();
					attrPtg.SpaceAfterToken = true;
					m_arrTokens.Add(attrPtg);
				}
				result = new ParenthesesPtg();
				m_tokenizer.NextToken();
				break;
			}
			case FormulaToken.tFunction1:
				result = ParseFunction(indexes, i, options, arguments);
				break;
			case FormulaToken.Identifier:
			case FormulaToken.Identifier3D:
			{
				string tokenString = m_tokenizer.TokenString;
				if (m_tokenizer.TokenString == "Overview!#REF!")
				{
					m_tokenizer.TokenType = FormulaToken.tError;
				}
				m_tokenizer.NextToken();
				UpdateOptions(ref options);
				result = ParseIdentifier(tokenString, indexes, i, options, arguments);
				break;
			}
			case FormulaToken.DDELink:
				result = ParseDDELink(indexes, i, options, arguments);
				break;
			case FormulaToken.ValueTrue:
				result = new BooleanPtg(value: true);
				m_tokenizer.NextToken();
				break;
			case FormulaToken.ValueFalse:
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				result = new BooleanPtg(value: false);
				m_tokenizer.NextToken();
				break;
			case FormulaToken.tNumber:
				try
				{
					result = new DoublePtg(double.Parse(m_tokenizer.TokenString, NumberStyles.Float, m_tokenizer.NumberFormat));
				}
				catch (Exception ex)
				{
					m_tokenizer.RaiseException($"Invalid number {m_tokenizer.TokenString}", ex);
				}
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				m_tokenizer.NextToken();
				UpdateOptions(ref options);
				break;
			case FormulaToken.tInteger:
			{
				if (ushort.TryParse(m_tokenizer.TokenString, NumberStyles.Float, m_tokenizer.NumberFormat, out var result2))
				{
					result = new IntegerPtg(result2);
					if (attrPtg == null && m_tokenSpaces.Count > 0)
					{
						m_arrTokens.Add(m_tokenSpaces.Pop());
					}
					m_tokenizer.NextToken();
					UpdateOptions(ref options);
					break;
				}
				goto case FormulaToken.tNumber;
			}
			case FormulaToken.tStringConstant:
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				result = new StringConstantPtg(m_tokenizer.TokenString);
				m_tokenizer.NextToken();
				break;
			case FormulaToken.tError:
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				result = ParseError(indexes, i, options, arguments.Worksheet);
				m_tokenizer.NextToken();
				break;
			case FormulaToken.tArray1:
			{
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				FormulaToken tokenId = ArrayPtg.IndexToCode(FormulaUtil.GetIndex(typeof(ArrayPtg), 1, indexes, i, options));
				result = ParseArray(tokenId, arguments);
				m_tokenizer.NextToken();
				break;
			}
			case FormulaToken.Space:
				attrPtg = (AttrPtg)FormulaUtil.CreatePtg(FormulaToken.tAttr, 64, 256);
				attrPtg.SpaceCount = m_tokenizer.TokenString.Length;
				m_arrTokens.Add(attrPtg);
				m_tokenizer.NextToken();
				flag = true;
				result = attrPtg;
				break;
			case FormulaToken.Comma:
				if (attrPtg == null && m_tokenSpaces.Count > 0)
				{
					m_arrTokens.Add(m_tokenSpaces.Pop());
				}
				result = FormulaUtil.CreatePtg(FormulaToken.tMissingArgument);
				break;
			}
		}
		while (flag);
		return result;
	}

	private void UpdateOptions(ref OfficeParseFormulaOptions options)
	{
		if ((options & OfficeParseFormulaOptions.ParseOperand) != 0)
		{
			FormulaToken tokenType = m_tokenizer.TokenType;
			if (tokenType != FormulaToken.Comma && tokenType != FormulaToken.EndOfFormula && tokenType != FormulaToken.CloseParenthesis && tokenType != FormulaToken.Space && tokenType != FormulaToken.tCellRange)
			{
				options -= 8;
				options |= OfficeParseFormulaOptions.ParseComplexOperand;
			}
		}
	}

	private Ptg ParseDDELink(Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		string tokenString = m_tokenizer.TokenString;
		m_tokenizer.NextToken();
		string tokenString2 = m_tokenizer.TokenString;
		m_tokenizer.NextToken();
		string tokenString3 = m_tokenizer.TokenString;
		m_tokenizer.TokenType = FormulaToken.None;
		m_tokenizer.NextToken();
		return CreateDDELink(tokenString, tokenString2, tokenString3, indexes, i, options, arguments);
	}

	private Ptg CreateDDELink(string strDDELink, string strParamName, string strName, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		string text = strDDELink + "|" + strParamName;
		ExternBookCollection externWorkbooks = ((WorkbookImpl)arguments.Workbook).ExternWorkbooks;
		ExternWorkbookImpl externWorkbookImpl = externWorkbooks[text];
		int num = -1;
		if (externWorkbookImpl == null)
		{
			num = externWorkbooks.AddDDEFile(text);
			externWorkbookImpl = externWorkbooks[num];
		}
		else
		{
			num = externWorkbookImpl.Index;
		}
		ExternNamesCollection externNames = externWorkbookImpl.ExternNames;
		int num2 = externNames.GetNameIndex(strName);
		if (num2 < 0)
		{
			num2 = externNames.Add(strName);
		}
		externNames[num2].Record.Options = 32738;
		NameXPtg obj = (NameXPtg)FormulaUtil.CreatePtg(NameXPtg.IndexToCode(FormulaUtil.GetIndex(typeof(ArrayPtg), 1, indexes, i, options)));
		obj.NameIndex = (ushort)(num2 + 1);
		obj.RefIndex = (ushort)num;
		return obj;
	}

	private Ptg ParseIdentifier(string identifier, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (identifier.Length == 0)
		{
			throw new ArgumentException("identifier - string cannot be empty.");
		}
		Ptg result = null;
		int iRefIndex = -1;
		if (m_tokenizer.PreviousTokenType == FormulaToken.Identifier3D)
		{
			int num = identifier.LastIndexOf('!');
			if (num <= 0)
			{
				if (arguments.CellRow != 0 && arguments.CellColumn != 0)
				{
					throw new ArgumentOutOfRangeException("identifier");
				}
			}
			else
			{
				string location = identifier.Substring(0, num);
				identifier = identifier.Substring(num + 1);
				iRefIndex = ConvertLocationIntoReference(location, arguments);
			}
		}
		if (!TryGetNamedRange(identifier, arguments, indexes, i, options, iRefIndex, out result) && !TryCreateRange(identifier, indexes, i, options, arguments, out result, iRefIndex))
		{
			result = CreateNamedRange(identifier, arguments, indexes, i, options, iRefIndex);
		}
		return result;
	}

	private bool TryGetNamedRange(string strToken, ParseParameters arguments, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, int iRefIndex, out Ptg result)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		if (strToken == null || strToken.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strToken");
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)arguments.Workbook;
		result = null;
		if (iRefIndex < 0 || workbookImpl.IsLocalReference(iRefIndex))
		{
			IWorksheet worksheet = arguments.Worksheet;
			IName name;
			if (iRefIndex == -1)
			{
				name = ((worksheet != null) ? worksheet.Names[strToken] : (name = workbookImpl.Names[strToken]));
			}
			else
			{
				IWorksheet sheetByReference = workbookImpl.GetSheetByReference(iRefIndex, bThrowExceptions: false);
				name = ((sheetByReference != null) ? sheetByReference.Names : workbookImpl.Names)[strToken];
			}
			if (name != null)
			{
				result = CreateNameToken(iRefIndex, (name as NameImpl).Index, arguments, indexes, i, options);
			}
		}
		else
		{
			int bookIndex = workbookImpl.GetBookIndex(iRefIndex);
			int nameIndex = workbookImpl.ExternWorkbooks[bookIndex].ExternNames.GetNameIndex(strToken);
			if (nameIndex >= 0)
			{
				result = CreateNameToken(iRefIndex, nameIndex, arguments, indexes, i, options);
			}
		}
		return result != null;
	}

	private int ConvertLocationIntoReference(string location, ParseParameters arguments)
	{
		int length = location.Length;
		if (location[0] == '\'' && location[length - 1] == '\'')
		{
			location = location.Substring(1, length - 2);
		}
		string text = null;
		string text2 = null;
		string text3 = null;
		int num = location.IndexOf('[');
		int num2 = location.IndexOf(']');
		int startIndex = ((num2 > 0) ? (num2 + 1) : 0);
		if (num > 0)
		{
			text3 = location.Substring(0, num);
		}
		if (num2 > 0)
		{
			text2 = location.Substring(num + 1, num2 - num - 1);
		}
		int result = -1;
		text = location.Substring(startIndex);
		WorkbookImpl workbookImpl = (WorkbookImpl)arguments.Workbook;
		int result2 = -1;
		if (text2 == null)
		{
			return workbookImpl.AddSheetReference(text);
		}
		ExternWorkbookImpl externWorkbookImpl = null;
		if (workbookImpl.IsWorkbookOpening && text3 == null && int.TryParse(text2, out result2))
		{
			if (result2 == 0 || workbookImpl.ExternWorkbooks.Count == 0)
			{
				result2 = workbookImpl.ExternWorkbooks.InsertSelfSupbook() + 1;
			}
			if (result2 > workbookImpl.ExternWorkbooks.Count)
			{
				return result;
			}
			externWorkbookImpl = workbookImpl.ExternWorkbooks[result2 - 1];
		}
		else
		{
			externWorkbookImpl = workbookImpl.ExternWorkbooks.FindOrAdd(text2, text3);
		}
		int num3 = ((text != null && text.Length > 0) ? externWorkbookImpl.FindOrAddSheet(text) : 65534);
		return m_book.AddSheetReference(externWorkbookImpl.Index, num3, num3);
	}

	private Ptg ParseFunction(Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		if ((options & OfficeParseFormulaOptions.ParseComplexOperand) != 0)
		{
			options -= 16;
			options |= OfficeParseFormulaOptions.ParseOperand;
		}
		string text = m_tokenizer.TokenString.ToUpper();
		if (text.StartsWith("_xlfn."))
		{
			text.Replace("_xlfn.", string.Empty);
		}
		Ptg result = null;
		AttrPtg attrPtg = null;
		if (m_tokenizer.PreviousTokenType == FormulaToken.Space)
		{
			int index = m_arrTokens.Count - 1;
			AttrPtg attrPtg2 = (AttrPtg)m_arrTokens[index];
			if (!attrPtg2.SpaceAfterToken)
			{
				attrPtg = attrPtg2;
				m_arrTokens.RemoveAt(index);
			}
		}
		ExcelFunction value;
		int iBookIndex;
		int iNameIndex;
		if (text == "IF")
		{
			result = CreateIFFunction(indexes, i, options, arguments, attrPtg);
			attrPtg = null;
		}
		else if (FormulaUtil.FunctionAliasToId.TryGetValue(text, out value))
		{
			result = ((!IsFunctionSupported(value, m_book.Version)) ? CreateCustomFunction(indexes, i, options, arguments, registerFunction: true) : CreateFunction(value, indexes, i, options, arguments));
		}
		else if (FormulaUtil.IsCustomFunction(m_tokenizer.TokenString, arguments.Workbook, out iBookIndex, out iNameIndex) || !TryCreateFunction2007(ref result, indexes, i, options, arguments))
		{
			result = CreateCustomFunction(indexes, i, options, arguments, registerFunction: false);
			attrPtg = null;
		}
		if (attrPtg != null)
		{
			m_arrTokens.Add(attrPtg);
		}
		else if (m_tokenSpaces.Count > 0)
		{
			attrPtg = m_tokenSpaces.Pop();
			attrPtg.AttrData1 = 4;
			m_arrTokens.Add(attrPtg);
		}
		attrPtg = null;
		return result;
	}

	private bool IsFunctionSupported(ExcelFunction functionId, OfficeVersion excelVersion)
	{
		bool result = true;
		if (excelVersion < OfficeVersion.Excel2013)
		{
			if (FormulaUtil.IsExcel2013Function(functionId))
			{
				result = false;
			}
			else if (excelVersion < OfficeVersion.Excel2010)
			{
				if (FormulaUtil.IsExcel2010Function(functionId))
				{
					result = false;
				}
				else if (excelVersion < OfficeVersion.Excel2007)
				{
					if (FormulaUtil.IsExcel2007Function(functionId))
					{
						result = false;
					}
					if (m_book.ExternWorkbooks.ContainsExternName(functionId.ToString()))
					{
						result = false;
					}
				}
			}
		}
		return result;
	}

	private bool TryCreateFunction2007(ref Ptg result, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		string tokenString = m_tokenizer.TokenString;
		tokenString = tokenString.ToUpper();
		if (arguments.Workbook.Version == OfficeVersion.Excel97to2003 || !Enum.IsDefined(typeof(Excel2007Function), tokenString))
		{
			return false;
		}
		Excel2007Function functionId = (Excel2007Function)Enum.Parse(typeof(Excel2007Function), tokenString, ignoreCase: true);
		result = CreateFunction((ExcelFunction)functionId, indexes, i, options, arguments);
		return true;
	}

	private bool TryCreateRange(string strFormula, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments, out Ptg resultToken, int iRefIndex)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		if (strFormula == null)
		{
			throw new ArgumentNullException("strFormula");
		}
		bool flag = true;
		resultToken = null;
		_ = arguments.Worksheet;
		WorkbookImpl book = (WorkbookImpl)arguments.Workbook;
		bool isR1C = arguments.IsR1C1;
		int cellRow = arguments.CellRow;
		int cellColumn = arguments.CellColumn;
		_ = arguments.WorksheetNames;
		if (m_book.FormulaUtil.IsCellRange(strFormula, isR1C, out var strRow, out var strColumn, out var strRow2, out var strColumn2))
		{
			FormulaToken token = AreaPtg.IndexToCode(FormulaUtil.GetIndex(typeof(AreaPtg), 0, indexes, i, options));
			resultToken = FormulaUtil.CreatePtg(token, cellRow, cellColumn, strRow, strColumn, strRow2, strColumn2, isR1C, book);
		}
		else if (FormulaUtil.IsCell(strFormula, isR1C, out strRow, out strColumn))
		{
			bool flag2 = false;
			if (strColumn != null)
			{
				flag2 = strColumn.Contains('$'.ToString());
			}
			FormulaToken token = ((flag2 || indexes == null || !indexes.ContainsKey(typeof(RefNPtg))) ? RefPtg.IndexToCode(FormulaUtil.GetIndex(typeof(RefPtg), 0, indexes, i, options)) : RefNPtg.IndexToCode(FormulaUtil.GetIndex(typeof(RefNPtg), 0, indexes, i, options) + 1));
			resultToken = FormulaUtil.CreatePtg(token, cellRow, cellColumn, strRow, strColumn, isR1C);
		}
		else
		{
			flag = false;
		}
		if (flag && iRefIndex != -1)
		{
			resultToken = (resultToken as IToken3D).Get3DToken(iRefIndex);
		}
		return flag;
	}

	private Ptg CreateNamedRange(string strToken, ParseParameters arguments, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, int iRefIndex)
	{
		WorkbookImpl workbookImpl = (WorkbookImpl)arguments.Workbook;
		if (iRefIndex >= 0 && !workbookImpl.IsLocalReference(iRefIndex))
		{
			return CreateExternalName(iRefIndex, strToken, arguments, indexes, i, options);
		}
		return CreateLocalName(iRefIndex, strToken, arguments, indexes, i, options);
	}

	private Ptg CreateExternalName(int iRefIndex, string strToken, ParseParameters arguments, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		if (strToken == null)
		{
			throw new ArgumentNullException("strToken");
		}
		if (iRefIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iRefIndex");
		}
		WorkbookImpl obj = (WorkbookImpl)arguments.Workbook;
		int bookIndex = obj.GetBookIndex(iRefIndex);
		ExternNamesCollection externNames = obj.ExternWorkbooks[bookIndex].ExternNames;
		int num = externNames.GetNameIndex(strToken);
		if (num < 0)
		{
			num = externNames.Add(strToken);
		}
		return CreateNameToken(iRefIndex, num, arguments, indexes, i, options);
	}

	private Ptg CreateLocalName(int iRefIndex, string strToken, ParseParameters arguments, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		if (strToken == null || strToken.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strToken");
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)arguments.Workbook;
		IWorksheet worksheet = arguments.Worksheet;
		INames names = null;
		IName name;
		if (iRefIndex == -1)
		{
			name = worksheet?.Names[strToken];
			if (name == null)
			{
				name = workbookImpl.Names[strToken];
			}
			names = workbookImpl.Names;
		}
		else
		{
			IWorksheet sheetByReference = workbookImpl.GetSheetByReference(iRefIndex, bThrowExceptions: false);
			names = ((sheetByReference != null) ? sheetByReference.Names : workbookImpl.Names);
			name = names[strToken];
		}
		if (name == null)
		{
			name = names.Add(strToken);
		}
		int index = (name as NameImpl).Index;
		return CreateNameToken(iRefIndex, index, arguments, indexes, i, options);
	}

	private Ptg CreateNameToken(int iRefIndex, int iNameIndex, ParseParameters arguments, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options)
	{
		if (iRefIndex >= 0)
		{
			return FormulaUtil.CreatePtg(NameXPtg.IndexToCode(FormulaUtil.GetIndex(typeof(NameXPtg), 0, indexes, i, options)), iRefIndex, iNameIndex);
		}
		return FormulaUtil.CreatePtg(NamePtg.IndexToCode(FormulaUtil.GetIndex(typeof(NamePtg), 0, indexes, i, options)), iNameIndex);
	}

	private Ptg CreateLocalName(string strNameLocation, string strToken, ParseParameters arguments, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options)
	{
		IWorkbook workbook = arguments.Workbook;
		IWorksheet worksheet = arguments.Worksheet;
		if (strNameLocation != null && strNameLocation.Length > 0)
		{
			worksheet = workbook.Worksheets[strNameLocation];
		}
		IName name = worksheet?.Names[strToken];
		if (name == null)
		{
			name = workbook.Names[strToken];
		}
		Ptg result = null;
		if (name != null)
		{
			int index = (name as NameImpl).Index;
			result = FormulaUtil.CreatePtg(NamePtg.IndexToCode(FormulaUtil.GetIndex(typeof(NamePtg), 0, indexes, i, options)), index);
		}
		return result;
	}

	private bool IsExternLocation(IWorkbook book, string strLocation)
	{
		if (strLocation == null || strLocation.Length == 0)
		{
			return false;
		}
		if (book.Worksheets[strLocation] != null)
		{
			return false;
		}
		return true;
	}

	private Ptg CreateFunction(ExcelFunction functionId, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments)
	{
		FunctionPtg functionPtg = null;
		List<int> list = ExtractOperands(options, arguments, functionId);
		if (FormulaUtil.FunctionIdToParamCount.TryGetValue(functionId, out var value))
		{
			if (functionId == ExcelFunction.IFERROR && list.Count <= 2)
			{
				m_arrTokens.Add(FormulaUtil.CreatePtg(FormulaToken.tMissingArgument));
				list.Add(m_arrTokens.Count);
			}
			if (functionId == ExcelFunction.RAND)
			{
				list.Clear();
				if (value != list.Count)
				{
					m_tokenizer.RaiseException("Wrong arguments number for function: " + functionId, null);
				}
			}
			else if (value != list.Count - 1)
			{
				m_tokenizer.RaiseException("Wrong arguments number for function: " + functionId, null);
			}
			functionPtg = (FunctionPtg)FormulaUtil.CreatePtg(FunctionPtg.IndexToCode(FormulaUtil.GetIndex(typeof(FunctionPtg), 0, indexes, i, options)), functionId);
		}
		else
		{
			if (functionId == ExcelFunction.GETPIVOTDATA && list.Count > 4 && list.Count % 2 == 0)
			{
				m_arrTokens.Add(FormulaUtil.CreatePtg(FormulaToken.tMissingArgument));
				list.Add(m_arrTokens.Count);
			}
			functionPtg = (FunctionPtg)FormulaUtil.CreatePtg(FunctionVarPtg.IndexToCode(FormulaUtil.GetIndex(typeof(FunctionVarPtg), 0, indexes, i, options)), functionId);
			functionPtg.NumberOfArguments = (byte)(list.Count - 1);
		}
		return functionPtg;
	}

	private Ptg CreateIFFunction(Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments, AttrPtg spaceBeforeIf)
	{
		List<int> list = ExtractOperands(options, arguments, ExcelFunction.IF);
		int num = list.Count - 1;
		if (num > 3 || num < 2)
		{
			m_tokenizer.RaiseException("Argument count for IF function must be 2 or 3.", null);
		}
		int num2 = list[1];
		int num3 = list[2];
		int num4 = GetTokensSize(num2, list[2], arguments) + 4;
		int num5 = ((num == 3) ? (GetTokensSize(num3, list[3], arguments) + 4) : 0);
		AttrPtg attrPtg = null;
		if (m_tokenSpaces.Count > 0)
		{
			attrPtg = m_tokenSpaces.Pop();
			attrPtg.AttrData1 = 4;
			num5 += attrPtg.GetSize(arguments.Version);
		}
		if (spaceBeforeIf != null)
		{
			num5 += spaceBeforeIf.GetSize(arguments.Version);
		}
		Ptg item = FormulaUtil.CreatePtg(FormulaToken.tAttr, 2, num4);
		m_arrTokens.Insert(num2, item);
		num3++;
		int num6 = (((options & OfficeParseFormulaOptions.InArray) == 0) ? 8 : 0);
		item = FormulaUtil.CreatePtg(FormulaToken.tAttr, num6, num5 + 3);
		m_arrTokens.Insert(num3, item);
		if (spaceBeforeIf != null)
		{
			m_arrTokens.Add(spaceBeforeIf);
		}
		if (attrPtg != null)
		{
			m_arrTokens.Add(attrPtg);
		}
		if (num == 3)
		{
			item = FormulaUtil.CreatePtg(FormulaToken.tAttr, num6, 3);
			m_arrTokens.Add(item);
		}
		FunctionPtg obj = (FunctionPtg)FormulaUtil.CreatePtg(FunctionVarPtg.IndexToCode(FormulaUtil.GetIndex(typeof(FunctionVarPtg), 1, indexes, i, options)), ExcelFunction.IF);
		obj.NumberOfArguments = (byte)(list.Count - 1);
		return obj;
	}

	private Ptg CreateCustomFunction(Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, ParseParameters arguments, bool registerFunction)
	{
		_ = arguments.Worksheet;
		if (!FormulaUtil.IsCustomFunction(m_tokenizer.TokenString, arguments.Workbook, out var iBookIndex, out var iNameIndex))
		{
			if (registerFunction)
			{
				IName name = m_book.Names.Add(m_tokenizer.TokenString);
				(name as NameImpl).IsFunction = true;
				iNameIndex = (name as NameImpl).Index;
				iBookIndex = -1;
			}
			else
			{
				m_tokenizer.RaiseException(m_tokenizer.TokenString + " isn't custom function.", null);
			}
		}
		Ptg item = ((iBookIndex != -1) ? FormulaUtil.CreatePtg(FormulaToken.tNameX1, iBookIndex, iNameIndex) : FormulaUtil.CreatePtg(FormulaToken.tName1, iNameIndex));
		m_arrTokens.Add(item);
		List<int> list = ExtractOperands(options, arguments, ExcelFunction.CustomFunction);
		FunctionVarPtg obj = (FunctionVarPtg)FormulaUtil.CreatePtg(FunctionVarPtg.IndexToCode(FormulaUtil.GetIndex(typeof(FunctionVarPtg), 1, indexes, i, options)), ExcelFunction.CustomFunction);
		obj.NumberOfArguments = (byte)list.Count;
		return obj;
	}

	private int GetTokensSize(int iStartToken, int iEndToken, ParseParameters arguments)
	{
		if (iStartToken < 0 || iStartToken >= iEndToken)
		{
			throw new ArgumentOutOfRangeException("iStartToken");
		}
		int num = 0;
		for (int i = iStartToken; i < iEndToken; i++)
		{
			Ptg ptg = m_arrTokens[i];
			num += ptg.GetSize(arguments.Version);
		}
		return num;
	}

	private List<int> ExtractOperands(OfficeParseFormulaOptions options, ParseParameters arguments, ExcelFunction functionId)
	{
		Dictionary<Type, ReferenceIndexAttribute> indexes = FormulaUtil.FunctionIdToIndex[functionId];
		OfficeParseFormulaOptions officeParseFormulaOptions = options;
		if ((options & OfficeParseFormulaOptions.RootLevel) != 0)
		{
			officeParseFormulaOptions--;
		}
		officeParseFormulaOptions |= OfficeParseFormulaOptions.ParseOperand;
		if (FormulaUtil.IndexOf(FormulaUtil.SemiVolatileFunctions, functionId) != -1)
		{
			m_arrTokens.Add(FormulaUtil.CreatePtg(FormulaToken.tAttr, 1, 0));
		}
		m_tokenizer.NextToken();
		if (m_tokenizer.TokenType != FormulaToken.tParentheses)
		{
			throw new ArgumentOutOfRangeException("Can't extract function arguments.");
		}
		m_tokenizer.NextToken();
		int num = 0;
		List<int> list = new List<int>();
		list.Add(m_arrTokens.Count);
		while (m_tokenizer.TokenType != FormulaToken.CloseParenthesis)
		{
			ParseExpression(Priority.None, indexes, num, officeParseFormulaOptions, arguments);
			if (m_tokenSpaces.Count > 0)
			{
				AttrPtg attrPtg = m_tokenSpaces.Pop();
				attrPtg.SpaceAfterToken = true;
				m_arrTokens.Add(attrPtg);
			}
			list.Add(m_arrTokens.Count);
			if (m_tokenizer.TokenType == FormulaToken.Comma)
			{
				m_tokenizer.NextToken();
			}
			num++;
		}
		m_tokenizer.NextToken();
		return list;
	}

	private Ptg ParseArray(FormulaToken tokenId, ParseParameters arguments)
	{
		string tokenString = m_tokenizer.TokenString;
		return FormulaUtil.CreatePtg(tokenId, tokenString, arguments.FormulaUtility);
	}

	private Ptg ParseError(Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options, IWorksheet sheet)
	{
		Ptg ptg = null;
		string tokenString = m_tokenizer.TokenString;
		int num = tokenString.LastIndexOf('!', tokenString.Length - 2);
		if (num != -1)
		{
			tokenString = tokenString.Substring(0, num);
			tokenString = tokenString.Trim('\'');
			int num2 = m_book.AddSheetReference(tokenString);
			int index = FormulaUtil.GetIndex(typeof(RefError3dPtg), 1, indexes, i, options);
			ptg = FormulaUtil.CreatePtg(RefError3dPtg.IndexToCode(index));
			((RefError3dPtg)ptg).RefIndex = (ushort)num2;
		}
		else
		{
			if (tokenString.EndsWith("#REF!"))
			{
				int index = FormulaUtil.GetIndex(typeof(RefErrorPtg), 1, indexes, i, options);
				ptg = FormulaUtil.CreatePtg((sheet != null) ? RefErrorPtg.IndexToCode(index) : RefError3dPtg.IndexToCode(index));
			}
			else
			{
				string tokenString2 = m_tokenizer.TokenString;
				ptg = (Ptg)FormulaUtil.ErrorNameToConstructor[tokenString2].Invoke(new object[1] { tokenString2 });
			}
			if (ptg is IReference reference)
			{
				reference.RefIndex = ushort.MaxValue;
			}
		}
		return ptg;
	}
}
