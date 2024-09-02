using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

[Preserve(AllMembers = true)]
internal class FormulaUtil : CommonObject
{
	internal enum ConstructorId
	{
		Default,
		String,
		ByteArrayOffset,
		StringParent,
		TwoUShorts,
		FunctionIndex,
		TwoStrings,
		FourStrings,
		Int3String4Bool,
		TokenType
	}

	[Preserve(AllMembers = true)]
	internal class TokenConstructor
	{
		private Dictionary<int, ConstructorInfo> m_hashConstructorToId = new Dictionary<int, ConstructorInfo>();

		private Type m_type;

		private ConstructorInfo DefaultConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.Default);
			}
			set
			{
				SetConstructor(ConstructorId.Default, value);
			}
		}

		private ConstructorInfo StringConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.String);
			}
			set
			{
				SetConstructor(ConstructorId.String, value);
			}
		}

		private ConstructorInfo ArrayConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.ByteArrayOffset);
			}
			set
			{
				SetConstructor(ConstructorId.ByteArrayOffset, value);
			}
		}

		private ConstructorInfo StringParentConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.StringParent);
			}
			set
			{
				SetConstructor(ConstructorId.StringParent, value);
			}
		}

		private ConstructorInfo TwoUShortsConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.TwoUShorts);
			}
			set
			{
				SetConstructor(ConstructorId.TwoUShorts, value);
			}
		}

		private ConstructorInfo FunctionIndexConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.FunctionIndex);
			}
			set
			{
				SetConstructor(ConstructorId.FunctionIndex, value);
			}
		}

		private ConstructorInfo TwoStringsConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.TwoStrings);
			}
			set
			{
				SetConstructor(ConstructorId.TwoStrings, value);
			}
		}

		private ConstructorInfo FourStringsConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.FourStrings);
			}
			set
			{
				SetConstructor(ConstructorId.FourStrings, value);
			}
		}

		private ConstructorInfo Int3String4BoolConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.Int3String4Bool);
			}
			set
			{
				SetConstructor(ConstructorId.Int3String4Bool, value);
			}
		}

		private ConstructorInfo TokenTypeConstructor
		{
			get
			{
				return GetConstructor(ConstructorId.TokenType);
			}
			set
			{
				SetConstructor(ConstructorId.TokenType, value);
			}
		}

		private TokenConstructor()
		{
		}

		public TokenConstructor(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type", "Token type can't be null");
			}
			if (!type.GetTypeInfo().IsSubclassOf(typeof(Ptg)))
			{
				throw new ArgumentException("class should be descendant of Ptg", "type");
			}
			m_type = type;
			DefaultConstructor = type.GetConstructor(new Type[0]);
			StringConstructor = type.GetConstructor(new Type[1] { typeof(string) });
			ArrayConstructor = type.GetConstructor(new Type[2]
			{
				typeof(DataProvider),
				typeof(int)
			});
			StringParentConstructor = type.GetConstructor(new Type[2]
			{
				typeof(string),
				typeof(IWorkbook)
			});
			TwoUShortsConstructor = type.GetConstructor(new Type[2]
			{
				typeof(ushort),
				typeof(ushort)
			});
			FunctionIndexConstructor = type.GetConstructor(new Type[1] { typeof(ExcelFunction) });
			Type[] types = new Type[5]
			{
				typeof(int),
				typeof(int),
				typeof(string),
				typeof(string),
				typeof(bool)
			};
			TwoStringsConstructor = type.GetConstructor(types);
			types = new Type[8]
			{
				typeof(int),
				typeof(int),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(bool),
				typeof(IWorkbook)
			};
			FourStringsConstructor = type.GetConstructor(types);
			types = new Type[9]
			{
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(bool),
				typeof(IWorkbook)
			};
			Int3String4BoolConstructor = type.GetConstructor(types);
			types = new Type[1] { typeof(FormulaToken) };
			TokenTypeConstructor = type.GetConstructor(types);
		}

		public Ptg CreatePtg()
		{
			try
			{
				return (Ptg)DefaultConstructor.Invoke(null);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(FormulaToken tokenType)
		{
			try
			{
				return (Ptg)TokenTypeConstructor.Invoke(new object[1] { tokenType });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(string strParam)
		{
			try
			{
				return (Ptg)StringConstructor.Invoke(new object[1] { strParam });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(DataProvider provider, ref int offset, ParseParameters arguments)
		{
			try
			{
				Ptg ptg = (Ptg)ArrayConstructor.Invoke(new object[2] { provider, offset });
				offset += ptg.GetSize(arguments.Version);
				return ptg;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(params object[] arrParams)
		{
			Type[] array = new Type[arrParams.Length];
			for (int i = 0; i < arrParams.Length; i++)
			{
				array[i] = arrParams[i].GetType();
			}
			ConstructorInfo constructor = m_type.GetConstructor(array);
			try
			{
				return (Ptg)constructor.Invoke(arrParams);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(string strParam, IWorkbook parent)
		{
			try
			{
				return (Ptg)StringParentConstructor.Invoke(new object[2] { strParam, parent });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(ushort iParam1, ushort iParam2)
		{
			try
			{
				return (Ptg)TwoUShortsConstructor.Invoke(new object[2] { iParam1, iParam2 });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(ExcelFunction functionIndex)
		{
			try
			{
				return (Ptg)FunctionIndexConstructor.Invoke(new object[1] { functionIndex });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(int iCellRow, int iCellColumn, string strParam1, string strParam2, bool bR1C1)
		{
			try
			{
				return (Ptg)TwoStringsConstructor.Invoke(new object[5] { iCellRow, iCellColumn, strParam1, strParam2, bR1C1 });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(int iCellRow, int iCellColumn, string strParam1, string strParam2, string strParam3, string strParam4, bool bR1C1, IWorkbook book)
		{
			try
			{
				object[] parameters = new object[8] { iCellRow, iCellColumn, strParam1, strParam2, strParam3, strParam4, bR1C1, book };
				return (Ptg)FourStringsConstructor.Invoke(parameters);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public Ptg CreatePtg(int iCellRow, int iCellColumn, int iRefIndex, string strParam1, string strParam2, string strParam3, string strParam4, bool bR1C1, IWorkbook book)
		{
			try
			{
				object[] parameters = new object[9] { iCellRow, iCellColumn, iRefIndex, strParam1, strParam2, strParam3, strParam4, bR1C1, book };
				return (Ptg)Int3String4BoolConstructor.Invoke(parameters);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		private ConstructorInfo GetConstructor(ConstructorId id)
		{
			m_hashConstructorToId.TryGetValue((int)id, out var value);
			return value;
		}

		private void SetConstructor(ConstructorId id, ConstructorInfo value)
		{
			if (value != null)
			{
				m_hashConstructorToId[(int)id] = value;
			}
			else
			{
				m_hashConstructorToId.Remove((int)id);
			}
		}
	}

	internal const int DEF_TYPE_REF = 0;

	internal const int DEF_TYPE_VALUE = 1;

	private const int DEF_TYPE_ARRAY = 2;

	private const int DEF_INDEX_DEFAULT = 0;

	private const int DEF_INDEX_ARRAY = 1;

	private const int DEF_INDEX_NAME = 2;

	private const int DEF_INDEX_ROOT_LEVEL = 3;

	private const RegexOptions DEF_REGEX = RegexOptions.None;

	public const int DEF_NAME_INDEX = 1;

	public const int DEF_REFERENCE_INDEX = 2;

	public const int DEF_ARRAY_INDEX = 2;

	internal const int DEF_OPTIONS_OPT_GOTO = 8;

	internal const int DEF_OPTIONS_NOT_OPT_GOTO = 0;

	private const char DEF_BOOKNAME_OPENBRACKET = '[';

	private const char DEF_BOOKNAME_CLOSEBRACKET = ']';

	public const string DEF_GROUP_COLUMN1 = "Column1";

	public const string DEF_GROUP_COLUMN2 = "Column2";

	public const string DEF_GROUP_ROW1 = "Row1";

	public const string DEF_GROUP_ROW2 = "Row2";

	private const char DEF_SHEET_NAME_DELIM = '\'';

	private static readonly int[][][] DEF_INDEXES_CONVERTION;

	public const string Excel2010FunctionPrefix = "_xlfn.";

	public static readonly char[] OpenBrackets;

	public static readonly char[] CloseBrackets;

	public static readonly char[] StringBrackets;

	public static readonly string[] UnaryOperations;

	public static readonly string[] PlusMinusArray;

	private static readonly TypedSortedListEx<string, object> m_listPlusMinus;

	public static readonly Dictionary<ExcelFunction, string> FunctionIdToAlias;

	public static readonly Dictionary<ExcelFunction, int> FunctionIdToParamCount;

	public static readonly Dictionary<string, ExcelFunction> FunctionAliasToId;

	public static readonly Dictionary<ExcelFunction, Dictionary<Type, ReferenceIndexAttribute>> FunctionIdToIndex;

	public static readonly Dictionary<string, ConstructorInfo> ErrorNameToConstructor;

	private static readonly Dictionary<int, string> s_hashErrorCodeToName;

	private static readonly Dictionary<string, int> s_hashNameToErrorCode;

	private static readonly Dictionary<FormulaToken, TokenConstructor> TokenCodeToConstructor;

	private static readonly Dictionary<FormulaToken, Ptg> s_hashTokenCodeToPtg;

	public static readonly Regex CellRegex;

	public static readonly Regex CellR1C1Regex;

	public static readonly Regex CellRangeRegex;

	public static readonly Regex FullRowRangeRegex;

	public static readonly Regex FullColumnRangeRegex;

	public static readonly Regex FullRowRangeR1C1Regex;

	public static readonly Regex FullColumnRangeR1C1Regex;

	public static readonly Regex Full3DRowRangeRegex;

	public static readonly Regex Full3DColumnRangeRegex;

	public static readonly Regex CellRangeR1C1Regex;

	public static readonly Regex CellRangeR1C1ShortRegex;

	public static readonly Regex CellRangeR1C13DShortRegex;

	public const string DEF_SHEETNAME_GROUP = "SheetName";

	public const string DEF_BOOKNAME_GROUP = "BookName";

	public const string DEF_RANGENAME_GROUP = "RangeName";

	public const string DEF_ROW_GROUP = "Row1";

	public const string DEF_COLUMN_GROUP = "Column1";

	public const string DEF_PATH_GROUP = "Path";

	private const string DEF_SHEET_NAME = "SheetName";

	private const string DEF_SHEET_NAME_REG_EXPR = "[^][:\\/?]*";

	public static readonly Regex Cell3DRegex;

	public static readonly Regex CellR1C13DRegex;

	public static readonly Regex CellRange3DRegex;

	public static readonly Regex CellRange3DRegex2;

	public static readonly Regex CellRangeR1C13DRegex;

	public static readonly Regex CellRangeR1C13DRegex2;

	private static readonly Regex AddInFunctionRegEx;

	internal static readonly ExcelFunction[] SemiVolatileFunctions;

	public static readonly FormulaToken[] NameXCodes;

	public static readonly FormulaToken[] NameCodes;

	private static readonly ExcelFunction[] m_excel2007Supported;

	private static readonly ExcelFunction[] m_excel2010Supported;

	private static readonly ExcelFunction[] m_excel2013Supported;

	private NumberFormatInfo m_numberFormat;

	private WorkbookImpl m_book;

	private static readonly string[] m_arrAllOperationsDefault;

	private string[][] m_arrOperationGroups = new string[6][]
	{
		new string[2] { " ", "," },
		new string[1] { "^" },
		new string[2] { "*", "/" },
		new string[2] { "+", "-" },
		new string[1] { "&" },
		new string[6] { "<", "<=", "<>", "=", ">", ">=" }
	};

	private TypedSortedListEx<string, object> m_arrAllOperations = new TypedSortedListEx<string, object>(StringComparer.CurrentCulture);

	private TypedSortedListEx<string, object>[] m_arrOperationsWithPriority;

	private string m_strArrayRowSeparator = ";";

	private string m_strOperandsSeparator = ",";

	private FormulaParser m_parser;

	public static Dictionary<int, string> ErrorCodeToName => s_hashErrorCodeToName;

	public static Dictionary<string, int> ErrorNameToCode => s_hashNameToErrorCode;

	public string ArrayRowSeparator => m_strArrayRowSeparator;

	public string OperandsSeparator => m_strOperandsSeparator;

	public NumberFormatInfo NumberFormat
	{
		get
		{
			return m_numberFormat;
		}
		set
		{
			m_numberFormat = value;
		}
	}

	public IWorkbook ParentWorkbook => m_book;

	public static event EvaluateEventHandler FormulaEvaluator;

	static FormulaUtil()
	{
		DEF_INDEXES_CONVERTION = new int[4][][]
		{
			new int[3][]
			{
				new int[3] { 1, 1, 1 },
				new int[3] { 2, 3, 3 },
				new int[3] { 3, 3, 3 }
			},
			new int[3][]
			{
				new int[3] { 2, 2, 3 },
				new int[3] { 2, 2, 3 },
				new int[3] { 2, 2, 3 }
			},
			new int[3][]
			{
				new int[3] { 3, 3, 3 },
				new int[3] { 3, 3, 3 },
				new int[3] { 3, 3, 3 }
			},
			new int[3][]
			{
				new int[3] { 2, 2, 1 },
				new int[3] { 2, 2, 3 },
				new int[3] { 2, 2, 3 }
			}
		};
		OpenBrackets = new char[5] { '{', '(', '"', '\'', '[' };
		CloseBrackets = new char[5] { '}', ')', '"', '\'', ']' };
		StringBrackets = new char[1] { '"' };
		UnaryOperations = new string[4] { "%", "(", "+", "-" };
		PlusMinusArray = new string[2] { "+", "-" };
		m_listPlusMinus = GetSortedList(PlusMinusArray);
		FunctionIdToAlias = new Dictionary<ExcelFunction, string>(407);
		FunctionIdToParamCount = new Dictionary<ExcelFunction, int>(407);
		FunctionAliasToId = new Dictionary<string, ExcelFunction>();
		FunctionIdToIndex = new Dictionary<ExcelFunction, Dictionary<Type, ReferenceIndexAttribute>>(407);
		ErrorNameToConstructor = new Dictionary<string, ConstructorInfo>(7);
		s_hashErrorCodeToName = new Dictionary<int, string>(7);
		s_hashNameToErrorCode = new Dictionary<string, int>(7);
		TokenCodeToConstructor = new Dictionary<FormulaToken, TokenConstructor>(25);
		s_hashTokenCodeToPtg = new Dictionary<FormulaToken, Ptg>();
		CellRegex = new Regex("(?<Column1>[\\$]?[A-Za-z]{1,3})(?<Row1>[\\$]?\\d+)", RegexOptions.None);
		CellR1C1Regex = new Regex("(?<Row1>R[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column1>C[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		CellRangeRegex = new Regex("(?<Column1>[\\$]?[A-Za-z]{1,3})(?<Row1>[\\$]?\\d+):(?<Column2>[\\$]?[A-Za-z]{1,3})(?<Row2>[\\$]?\\d+)", RegexOptions.None);
		FullRowRangeRegex = new Regex("(?<Row1>[\\$]?\\d+):(?<Row2>[\\$]?\\d+)", RegexOptions.None);
		FullColumnRangeRegex = new Regex("(?<Column1>[\\$]?[A-Za-z]{1,3}):(?<Column2>[\\$]?[A-Za-z]{1,3})", RegexOptions.None);
		FullRowRangeR1C1Regex = new Regex("(?<Row1>R[\\[]?[\\-]?[0-9]*[\\]]?):(?<Row2>R[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		FullColumnRangeR1C1Regex = new Regex("(?<Column1>C[\\[]?[\\-]?[0-9]*[\\]]?):(?<Column2>C[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		Full3DRowRangeRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Row1>[\\$]\\d+):(?<Row2>[\\$]\\d+)", RegexOptions.None);
		Full3DColumnRangeRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Column1>[\\$][A-Za-z]{1,3}):(?<Column2>[\\$][A-Za-z]{1,3})", RegexOptions.None);
		CellRangeR1C1Regex = new Regex("(?<Row1>R[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column1>C[\\[]?[\\-]?[0-9]*[\\]]?):(?<Row2>R[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column2>C[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		CellRangeR1C1ShortRegex = new Regex("[R|C][\\[]?[\\-]?[\\-0-9]*[\\]]?", RegexOptions.None);
		CellRangeR1C13DShortRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!][R|C][\\[]?[\\-]?[0-9]*[\\]]?", RegexOptions.None);
		Cell3DRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Column1>[\\$]?[A-Za-z]{1,3})(?<Row1>[\\$]?\\d+)", RegexOptions.None);
		CellR1C13DRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Row1>R[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column1>C[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		CellRange3DRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Column1>[\\$]?[A-Za-z]{1,3})(?<Row1>[\\$]?\\d+):(?<Column2>[\\$]?[A-Za-z]{1,3})(?<Row2>[\\$]?\\d+)", RegexOptions.None);
		CellRange3DRegex2 = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Column1>[\\$]?[A-Za-z]{1,3})(?<Row1>[\\$]?\\d+):(?<SheetName2>[^][:\\/?]*)[\\!](?<Column2>[\\$]?[A-Za-z]{1,3})(?<Row2>[\\$]?\\d+)", RegexOptions.None);
		CellRangeR1C13DRegex = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Row1>[R]?[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column1>[C]?[\\[]?[\\-]?[0-9]*[\\]]?):(?<Row2>[R]?[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column2>[C]?[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		CellRangeR1C13DRegex2 = new Regex("(?<SheetName>[^][:\\/?]*)[\\!](?<Row1>[R]?[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column1>[C]?[\\[]?[\\-]?[0-9]*[\\]]?):(?<SheetName2>[^][:\\/?]*)[\\!](?<Row2>[R]?[\\[]?[\\-]?[0-9]*[\\]]?)(?<Column2>[C]?[\\[]?[\\-]?[0-9]*[\\]]?)", RegexOptions.None);
		AddInFunctionRegEx = new Regex("('?)(?<Path>[^'][^\\[]+\\\\)?(?<BookName>[^\\]]+\\])?(?<SheetName>[^][:\\/?]*)\\1!(?<RangeName>[^][:\\/?]*)", RegexOptions.None);
		SemiVolatileFunctions = new ExcelFunction[4]
		{
			ExcelFunction.CELL,
			ExcelFunction.INFO,
			ExcelFunction.NOW,
			ExcelFunction.TODAY
		};
		NameXCodes = new FormulaToken[3]
		{
			FormulaToken.tNameX1,
			FormulaToken.tNameX2,
			FormulaToken.tNameX3
		};
		NameCodes = new FormulaToken[3]
		{
			FormulaToken.tName1,
			FormulaToken.tName2,
			FormulaToken.tName3
		};
		m_excel2007Supported = new ExcelFunction[64]
		{
			ExcelFunction.HEX2BIN,
			ExcelFunction.HEX2DEC,
			ExcelFunction.HEX2OCT,
			ExcelFunction.COUNTIFS,
			ExcelFunction.BIN2DEC,
			ExcelFunction.BIN2HEX,
			ExcelFunction.BIN2OCT,
			ExcelFunction.DEC2BIN,
			ExcelFunction.DEC2HEX,
			ExcelFunction.DEC2OCT,
			ExcelFunction.OCT2BIN,
			ExcelFunction.OCT2DEC,
			ExcelFunction.OCT2HEX,
			ExcelFunction.ODDFPRICE,
			ExcelFunction.ODDFYIELD,
			ExcelFunction.ODDLPRICE,
			ExcelFunction.ODDLYIELD,
			ExcelFunction.ISODD,
			ExcelFunction.ISEVEN,
			ExcelFunction.LCM,
			ExcelFunction.GCD,
			ExcelFunction.SUMIFS,
			ExcelFunction.AVERAGEIF,
			ExcelFunction.AVERAGEIFS,
			ExcelFunction.CONVERT,
			ExcelFunction.COMPLEX,
			ExcelFunction.COUPDAYBS,
			ExcelFunction.COUPDAYS,
			ExcelFunction.COUPDAYSNC,
			ExcelFunction.COUPNCD,
			ExcelFunction.COUPNUM,
			ExcelFunction.COUPPCD,
			ExcelFunction.DELTA,
			ExcelFunction.DISC,
			ExcelFunction.DOLLARDE,
			ExcelFunction.DOLLARFR,
			ExcelFunction.DURATION,
			ExcelFunction.EDATE,
			ExcelFunction.EFFECT,
			ExcelFunction.EOMONTH,
			ExcelFunction.ERF,
			ExcelFunction.ERFC,
			ExcelFunction.FACTDOUBLE,
			ExcelFunction.GESTEP,
			ExcelFunction.IFERROR,
			ExcelFunction.IMABS,
			ExcelFunction.IMAGINARY,
			ExcelFunction.IMARGUMENT,
			ExcelFunction.IMCONJUGATE,
			ExcelFunction.IMCOS,
			ExcelFunction.IMEXP,
			ExcelFunction.IMLN,
			ExcelFunction.IMLOG10,
			ExcelFunction.IMLOG2,
			ExcelFunction.IMREAL,
			ExcelFunction.IMSIN,
			ExcelFunction.IMSQRT,
			ExcelFunction.IMSUB,
			ExcelFunction.IMSUM,
			ExcelFunction.IMDIV,
			ExcelFunction.IMPOWER,
			ExcelFunction.IMPRODUCT,
			ExcelFunction.ACCRINT,
			ExcelFunction.ACCRINTM
		};
		m_excel2010Supported = new ExcelFunction[59]
		{
			ExcelFunction.AGGREGATE,
			ExcelFunction.CHISQ_DIST,
			ExcelFunction.CHISQ_DIST,
			ExcelFunction.BETA_INV,
			ExcelFunction.BETA_DIST,
			ExcelFunction.BINOM_DIST,
			ExcelFunction.BINOM_INV,
			ExcelFunction.CEILING_PRECISE,
			ExcelFunction.CHISQ_DIST_RT,
			ExcelFunction.CHISQ_INV_RT,
			ExcelFunction.CHISQ_TEST,
			ExcelFunction.CONFIDENCE_NORM,
			ExcelFunction.CONFIDENCE_T,
			ExcelFunction.COVARIANCE_P,
			ExcelFunction.COVARIANCE_S,
			ExcelFunction.ERF_PRECISE,
			ExcelFunction.ERFC_PRECISE,
			ExcelFunction.EXPON_DIST,
			ExcelFunction.F_DIST,
			ExcelFunction.F_DIST_RT,
			ExcelFunction.F_INV,
			ExcelFunction.F_INV_RT,
			ExcelFunction.F_TEST,
			ExcelFunction.FLOOR_PRECISE,
			ExcelFunction.GAMMA_DIST,
			ExcelFunction.GAMMA_INV,
			ExcelFunction.GAMMALN_PRECISE,
			ExcelFunction.HYPGEOM_DIST,
			ExcelFunction.LOGNORM_DIST,
			ExcelFunction.LOGNORM_INV,
			ExcelFunction.MODE_MULT,
			ExcelFunction.MODE_SNGL,
			ExcelFunction.NEGBINOM_DIST,
			ExcelFunction.NETWORKDAYS_INTL,
			ExcelFunction.NORM_DIST,
			ExcelFunction.NORM_INV,
			ExcelFunction.NORM_S_DIST,
			ExcelFunction.PERCENTILE_EXC,
			ExcelFunction.PERCENTILE_INC,
			ExcelFunction.PERCENTRANK_EXC,
			ExcelFunction.PERCENTRANK_INC,
			ExcelFunction.POISSON_DIST,
			ExcelFunction.QUARTILE_EXC,
			ExcelFunction.QUARTILE_INC,
			ExcelFunction.RANK_AVG,
			ExcelFunction.RANK_EQ,
			ExcelFunction.STDEV_P,
			ExcelFunction.STDEV_S,
			ExcelFunction.T_DIST,
			ExcelFunction.T_DIST_2T,
			ExcelFunction.T_DIST_RT,
			ExcelFunction.T_INV,
			ExcelFunction.T_INV_2T,
			ExcelFunction.T_TEST,
			ExcelFunction.VAR_P,
			ExcelFunction.VAR_S,
			ExcelFunction.WEIBULL_DIST,
			ExcelFunction.WORKDAY_INTL,
			ExcelFunction.Z_TEST
		};
		m_excel2013Supported = new ExcelFunction[51]
		{
			ExcelFunction.DAYS,
			ExcelFunction.ISOWEEKNUM,
			ExcelFunction.BITAND,
			ExcelFunction.BITLSHIFT,
			ExcelFunction.BITOR,
			ExcelFunction.BITRSHIFT,
			ExcelFunction.BITXOR,
			ExcelFunction.IMCOSH,
			ExcelFunction.IMCOT,
			ExcelFunction.IMCSC,
			ExcelFunction.IMCSCH,
			ExcelFunction.IMSEC,
			ExcelFunction.IMSECH,
			ExcelFunction.IMSINH,
			ExcelFunction.IMTAN,
			ExcelFunction.PDURATION,
			ExcelFunction.RRI,
			ExcelFunction.ISFORMULA,
			ExcelFunction.SHEET,
			ExcelFunction.SHEETS,
			ExcelFunction.IFNA,
			ExcelFunction.XOR,
			ExcelFunction.FORMULATEXT,
			ExcelFunction.ACOT,
			ExcelFunction.ACOTH,
			ExcelFunction.ARABIC,
			ExcelFunction.BASE,
			ExcelFunction.CEILING_MATH,
			ExcelFunction.COMBINA,
			ExcelFunction.COT,
			ExcelFunction.COTH,
			ExcelFunction.CSC,
			ExcelFunction.CSCH,
			ExcelFunction.DECIMAL,
			ExcelFunction.FLOOR_MATH,
			ExcelFunction.ISO_CEILING,
			ExcelFunction.MUNIT,
			ExcelFunction.SEC,
			ExcelFunction.SECH,
			ExcelFunction.BINOM_DIST_RANGE,
			ExcelFunction.GAMMA,
			ExcelFunction.GAUSS,
			ExcelFunction.PERMUTATIONA,
			ExcelFunction.PHI,
			ExcelFunction.SKEW_P,
			ExcelFunction.NUMBERVALUE,
			ExcelFunction.UNICHAR,
			ExcelFunction.UNICODE,
			ExcelFunction.ENCODEURL,
			ExcelFunction.FILTERXML,
			ExcelFunction.WEBSERVICE
		};
		m_arrAllOperationsDefault = new string[14]
		{
			" ", "&", "*", "+", ",", "-", "/", "<>", "<=", "<",
			"=", ">=", ">", "^"
		};
		FillTokenConstructors();
		FillExcelFunctions();
		FillErrorNames();
	}

	public FormulaUtil(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		FillDefaultOperations();
		m_arrOperationsWithPriority = new TypedSortedListEx<string, object>[m_arrOperationGroups.Length];
		FillPriorities();
		IApplication application2 = base.Application;
		m_parser = new FormulaParser(m_book);
		SetSeparators(application2.ArgumentsSeparator, application2.RowSeparator);
	}

	public FormulaUtil(IApplication application, object parent, NumberFormatInfo numberFormat, char chArgumentsSeparator, char chRowSeparator)
		: base(application, parent)
	{
		FindParents();
		FillDefaultOperations();
		m_arrOperationsWithPriority = new TypedSortedListEx<string, object>[m_arrOperationGroups.Length];
		FillPriorities();
		m_parser = new FormulaParser(m_book);
		m_parser.NumberFormat = numberFormat;
		m_numberFormat = numberFormat;
		SetSeparators(chArgumentsSeparator, chRowSeparator);
	}

	private void FindParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Workbook", "Can't find parent workbook");
		}
	}

	private void FillDefaultOperations()
	{
		int i = 0;
		for (int num = m_arrAllOperationsDefault.Length; i < num; i++)
		{
			m_arrAllOperations.Add(m_arrAllOperationsDefault[i], null);
		}
	}

	private static void FillTokenConstructors()
	{
		if (ApplicationImpl.AssemblyTypes == null)
		{
			ApplicationImpl.InitAssemblyTypes();
		}
		Type[] assemblyTypes = ApplicationImpl.AssemblyTypes;
		Type typeFromHandle = typeof(Ptg);
		for (int i = 0; i < assemblyTypes.Length; i++)
		{
			if (assemblyTypes[i].IsSubclassOf(typeFromHandle))
			{
				RegisterTokenClass(assemblyTypes[i]);
			}
		}
	}

	private static void FillExcelFunctions()
	{
		ReferenceIndexAttribute[] paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("COUNT", ExcelFunction.COUNT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IF", ExcelFunction.IF, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISNA", ExcelFunction.ISNA, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISERROR", ExcelFunction.ISERROR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("SUM", ExcelFunction.SUM, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("AVERAGE", ExcelFunction.AVERAGE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3),
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("MIN", ExcelFunction.MIN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3),
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("MAX", ExcelFunction.MAX, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("ROW", ExcelFunction.ROW, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("COLUMN", ExcelFunction.COLUMN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("NA", ExcelFunction.NA, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2),
			new ReferenceIndexAttribute(typeof(AreaPtg), 2, 1)
		};
		RegisterFunction("NPV", ExcelFunction.NPV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("STDEV", ExcelFunction.STDEV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DOLLAR", ExcelFunction.DOLLAR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FIXED", ExcelFunction.FIXED, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SIN", ExcelFunction.SIN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COS", ExcelFunction.COS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TAN", ExcelFunction.TAN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ATAN", ExcelFunction.ATAN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("PI", ExcelFunction.PI, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SQRT", ExcelFunction.SQRT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EXP", ExcelFunction.EXP, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LN", ExcelFunction.LN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOG10", ExcelFunction.LOG10, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ABS", ExcelFunction.ABS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("INT", ExcelFunction.INT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SIGN", ExcelFunction.SIGN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ROUND", ExcelFunction.ROUND, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("HEX2BIN", ExcelFunction.HEX2BIN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("HEX2DEC", ExcelFunction.HEX2DEC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("HEX2OCT", ExcelFunction.HEX2OCT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BIN2DEC", ExcelFunction.BIN2DEC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BIN2HEX", ExcelFunction.BIN2HEX, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BIN2OCT", ExcelFunction.BIN2OCT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("DEC2BIN", ExcelFunction.DEC2BIN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("DEC2HEX", ExcelFunction.DEC2HEX, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("DEC2OCT", ExcelFunction.DEC2OCT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("OCT2BIN", ExcelFunction.OCT2BIN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("OCT2DEC", ExcelFunction.OCT2DEC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("OCT2HEX", ExcelFunction.OCT2HEX, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ODDFPRICE", ExcelFunction.ODDFPRICE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ODDFYIELD", ExcelFunction.ODDFYIELD, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ODDLPRICE", ExcelFunction.ODDLPRICE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ODDLYIELD", ExcelFunction.ODDLYIELD, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISEVEN", ExcelFunction.ISEVEN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISODD", ExcelFunction.ISODD, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("AVERAGEIFS", ExcelFunction.AVERAGEIFS, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("AVERAGEIF", ExcelFunction.AVERAGEIF, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CONVERT", ExcelFunction.CONVERT, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COMPLEX", ExcelFunction.COMPLEX, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUPDAYBS", ExcelFunction.COUPDAYBS, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUPDAYS", ExcelFunction.COUPDAYS, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUPDAYSNC", ExcelFunction.COUPDAYSNC, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUPNCD", ExcelFunction.COUPNCD, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUPNUM", ExcelFunction.COUPNUM, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUPPCD", ExcelFunction.COUPPCD, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("DELTA", ExcelFunction.DELTA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DISC", ExcelFunction.DISC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DOLLARDE", ExcelFunction.DOLLARDE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DOLLARFR", ExcelFunction.DOLLARFR, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DURATION", ExcelFunction.DURATION, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EDATE", ExcelFunction.EDATE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EFFECT", ExcelFunction.EFFECT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EOMONTH", ExcelFunction.EOMONTH, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("ERF", ExcelFunction.ERF, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ERFC", ExcelFunction.ERFC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FACTDOUBLE", ExcelFunction.FACTDOUBLE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("GESTEP", ExcelFunction.GESTEP, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IFERROR", ExcelFunction.IFERROR, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMABS", ExcelFunction.IMABS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMAGINARY", ExcelFunction.IMAGINARY, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMARGUMENT", ExcelFunction.IMARGUMENT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMCONJUGATE", ExcelFunction.IMCONJUGATE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMCOS", ExcelFunction.IMCOS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMEXP", ExcelFunction.IMEXP, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMLN", ExcelFunction.IMLN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMLOG10", ExcelFunction.IMLOG10, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMLOG2", ExcelFunction.IMLOG2, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMPOWER", ExcelFunction.IMPOWER, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMPRODUCT", ExcelFunction.IMPRODUCT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMREAL", ExcelFunction.IMREAL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSIN", ExcelFunction.IMSIN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSQRT", ExcelFunction.IMSQRT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSUB", ExcelFunction.IMSUB, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSUM", ExcelFunction.IMSUM, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMDIV", ExcelFunction.IMDIV, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LCM", ExcelFunction.LCM, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SUMIFS", ExcelFunction.SUMIFS, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GCD", ExcelFunction.GCD, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COUNTIFS", ExcelFunction.COUNTIFS, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ACCRINT", ExcelFunction.ACCRINT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ACCRINTM", ExcelFunction.ACCRINTM, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("AGGREGATE", ExcelFunction.AGGREGATE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("AMORDEGRC", ExcelFunction.AMORDEGRC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("AMORLINC", ExcelFunction.AMORLINC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BAHTTEXT", ExcelFunction.BAHTTEXT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BESSELI", ExcelFunction.BESSELI, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BESSELJ", ExcelFunction.BESSELJ, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BESSELK", ExcelFunction.BESSELK, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BESSELY", ExcelFunction.BESSELY, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBEKPIMEMBER", ExcelFunction.CUBEKPIMEMBER, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBEMEMBER", ExcelFunction.CUBEMEMBER, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBERANKEDMEMBER", ExcelFunction.CUBERANKEDMEMBER, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBESET", ExcelFunction.CUBESET, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBESETCOUNT", ExcelFunction.CUBESETCOUNT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBEMEMBERPROPERTY", ExcelFunction.CUBEMEMBERPROPERTY, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUMIPMT", ExcelFunction.CUMIPMT, paramIndexes, 6);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUMPRINC", ExcelFunction.CUMPRINC, paramIndexes, 6);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FVSCHEDULE", ExcelFunction.FVSCHEDULE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("INTRATE", ExcelFunction.INTRATE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CUBEVALUE", ExcelFunction.CUBEVALUE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MDURATION", ExcelFunction.MDURATION, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MROUND", ExcelFunction.MROUND, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MULTINOMIAL", ExcelFunction.MULTINOMIAL, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NETWORKDAYS", ExcelFunction.NETWORKDAYS, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NOMINAL", ExcelFunction.NOMINAL, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PRICE", ExcelFunction.PRICE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PRICEDISC", ExcelFunction.PRICEDISC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PRICEMAT", ExcelFunction.PRICEMAT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("QUOTIENT", ExcelFunction.QUOTIENT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RANDBETWEEN", ExcelFunction.RANDBETWEEN, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RECEIVED", ExcelFunction.RECEIVED, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SERIESSUM", ExcelFunction.SERIESSUM, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SQRTPI", ExcelFunction.SQRTPI, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TBILLEQ", ExcelFunction.TBILLEQ, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TBILLPRICE", ExcelFunction.TBILLPRICE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TBILLYIELD", ExcelFunction.TBILLYIELD, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WEEKNUM", ExcelFunction.WEEKNUM, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WORKDAY", ExcelFunction.WORKDAY, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("XIRR", ExcelFunction.XIRR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("XNPV", ExcelFunction.XNPV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("YEARFRAC", ExcelFunction.YEARFRAC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("YIELD", ExcelFunction.YIELD, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("YIELDDISC", ExcelFunction.YIELDDISC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("YIELDMAT", ExcelFunction.YIELDMAT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WORKDAY.INTL", ExcelFunction.WORKDAYINTL, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BETA.INV", ExcelFunction.BETA_INV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BINOM.DIST", ExcelFunction.BINOM_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BINOM.INV", ExcelFunction.BINOM_INV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CEILING.PRECISE", ExcelFunction.CEILING_PRECISE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHISQ.DIST", ExcelFunction.CHISQ_DIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHISQ.DIST.RT", ExcelFunction.CHISQ_DIST_RT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHISQ.INV", ExcelFunction.CHISQ_INV, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHISQ.INV.RT", ExcelFunction.CHISQ_INV_RT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHISQ.TEST", ExcelFunction.CHISQ_TEST, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CONFIDENCE.NORM", ExcelFunction.CONFIDENCE_NORM, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CONFIDENCE.T", ExcelFunction.CONFIDENCE_T, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COVARIANCE.P", ExcelFunction.COVARIANCE_P, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COVARIANCE.S", ExcelFunction.COVARIANCE_S, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ERF.PRECISE", ExcelFunction.ERF_PRECISE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ERFC.PRECISE", ExcelFunction.ERFC_PRECISE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("F.DIST", ExcelFunction.F_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("F.DIST.RT", ExcelFunction.F_DIST_RT, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("F.INV", ExcelFunction.F_INV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("F.TEST", ExcelFunction.F_TEST, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FLOOR.PRECISE", ExcelFunction.FLOOR_PRECISE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("F.INV.RT", ExcelFunction.F_INV_RT, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMA.DIST", ExcelFunction.GAMMA_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMA.INV", ExcelFunction.GAMMA_INV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMALN.PRECISE", ExcelFunction.GAMMALN_PRECISE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("HYPGEOM.DIST", ExcelFunction.HYPGEOM_DIST, paramIndexes, 5);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOGNORM.DIST", ExcelFunction.LOGNORM_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOGNORM.INV", ExcelFunction.LOGNORM_INV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MODE.MULT", ExcelFunction.MODE_MULT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MODE.SNGL", ExcelFunction.MODE_SNGL, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NEGBINOM.DIST", ExcelFunction.NEGBINOM_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NETWORKDAYS.INTL", ExcelFunction.NETWORKDAYS_INTL, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORM.DIST", ExcelFunction.NORM_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORM.INV", ExcelFunction.NORM_INV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORM.S.DIST", ExcelFunction.NORM_S_DIST, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PERCENTILE.EXC", ExcelFunction.PERCENTILE_EXC, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PERCENTILE.INC", ExcelFunction.PERCENTILE_INC, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PERCENTRANK.EXC", ExcelFunction.PERCENTRANK_EXC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PERCENTRANK.INC", ExcelFunction.PERCENTRANK_INC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("POISSON.DIST", ExcelFunction.POISSON_DIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("QUARTILE.EXC", ExcelFunction.QUARTILE_EXC, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("QUARTILE.INC", ExcelFunction.QUARTILE_INC, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RANK.AVG", ExcelFunction.RANK_AVG, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RANK.EQ", ExcelFunction.RANK_EQ, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("STDEV.P", ExcelFunction.STDEV_P, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("STDEV.S", ExcelFunction.STDEV_S, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("T.DIST", ExcelFunction.T_DIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("T.DIST.2T", ExcelFunction.T_DIST_2T, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("T.DIST.RT", ExcelFunction.T_DIST_RT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("T.INV", ExcelFunction.T_INV, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("VAR.P", ExcelFunction.VAR_P, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("VAR.S", ExcelFunction.VAR_S, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WEIBULL.DIST", ExcelFunction.WEIBULL_DIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("Z.TEST", ExcelFunction.Z_TEST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1, 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("LOOKUP", ExcelFunction.LOOKUP, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("INDEX", ExcelFunction.INDEX, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("REPT", ExcelFunction.REPT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MID", ExcelFunction.MID, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LEN", ExcelFunction.LEN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("VALUE", ExcelFunction.VALUE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("TRUE", ExcelFunction.TRUE, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("FALSE", ExcelFunction.FALSE, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("AND", ExcelFunction.AND, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("OR", ExcelFunction.OR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NOT", ExcelFunction.NOT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MOD", ExcelFunction.MOD, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DCOUNT", ExcelFunction.DCOUNT, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DSUM", ExcelFunction.DSUM, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DAVERAGE", ExcelFunction.DAVERAGE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DMIN", ExcelFunction.DMIN, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DMAX", ExcelFunction.DMAX, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DSTDEV", ExcelFunction.DSTDEV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("VAR", ExcelFunction.VAR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DVAR", ExcelFunction.DVAR, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TEXT", ExcelFunction.TEXT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("LINEST", ExcelFunction.LINEST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("TREND", ExcelFunction.TREND, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("LOGEST", ExcelFunction.LOGEST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("GROWTH", ExcelFunction.GROWTH, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GOTO", ExcelFunction.GOTO, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("HALT", ExcelFunction.HALT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PV", ExcelFunction.PV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FV", ExcelFunction.FV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NPER", ExcelFunction.NPER, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PMT", ExcelFunction.PMT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RATE", ExcelFunction.RATE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3),
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2, 2)
		};
		RegisterFunction("MIRR", ExcelFunction.MIRR, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3),
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("IRR", ExcelFunction.IRR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("RAND", ExcelFunction.RAND, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("MATCH", ExcelFunction.MATCH, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DATE", ExcelFunction.DATE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TIME", ExcelFunction.TIME, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DAY", ExcelFunction.DAY, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MONTH", ExcelFunction.MONTH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("YEAR", ExcelFunction.YEAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WEEKDAY", ExcelFunction.WEEKDAY, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("HOUR", ExcelFunction.HOUR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MINUTE", ExcelFunction.MINUTE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SECOND", ExcelFunction.SECOND, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("NOW", ExcelFunction.NOW, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("AREAS", ExcelFunction.AREAS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("ROWS", ExcelFunction.ROWS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("COLUMNS", ExcelFunction.COLUMNS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("OFFSET", ExcelFunction.OFFSET, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ABSREF", ExcelFunction.ABSREF, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("RELREF", ExcelFunction.RELREF, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ARGUMENT", ExcelFunction.ARGUMENT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SEARCH", ExcelFunction.SEARCH, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3),
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TRANSPOSE", ExcelFunction.TRANSPOSE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("ERROR", ExcelFunction.ERROR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("STEP", ExcelFunction.STEP, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("TYPE", ExcelFunction.TYPE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ECHO", ExcelFunction.ECHO, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SETNAME", ExcelFunction.SETNAME, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CALLER", ExcelFunction.CALLER, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DEREF", ExcelFunction.DEREF, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("WINDOWS", ExcelFunction.WINDOWS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SERIES", ExcelFunction.SERIES, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DOCUMENTS", ExcelFunction.DOCUMENTS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ACTIVECELL", ExcelFunction.ACTIVECELL, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SELECTION", ExcelFunction.SELECTION, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("RESULT", ExcelFunction.RESULT, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ATAN2", ExcelFunction.ATAN2, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ASIN", ExcelFunction.ASIN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ACOS", ExcelFunction.ACOS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CHOOSE", ExcelFunction.CHOOSE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1)
		};
		RegisterFunction("HLOOKUP", ExcelFunction.HLOOKUP, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1)
		};
		RegisterFunction("VLOOKUP", ExcelFunction.VLOOKUP, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("LINKS", ExcelFunction.LINKS, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("INPUT", ExcelFunction.INPUT, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("ISREF", ExcelFunction.ISREF, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETFORMULA", ExcelFunction.GETFORMULA, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETNAME", ExcelFunction.GETNAME, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SETVALUE", ExcelFunction.SETVALUE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOG", ExcelFunction.LOG, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("EXEC", ExcelFunction.EXEC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHAR", ExcelFunction.CHAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOWER", ExcelFunction.LOWER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("UPPER", ExcelFunction.UPPER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PROPER", ExcelFunction.PROPER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LEFT", ExcelFunction.LEFT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RIGHT", ExcelFunction.RIGHT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EXACT", ExcelFunction.EXACT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TRIM", ExcelFunction.TRIM, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("REPLACE", ExcelFunction.REPLACE, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SUBSTITUTE", ExcelFunction.SUBSTITUTE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CODE", ExcelFunction.CODE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("NAMES", ExcelFunction.NAMES, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DIRECTORY", ExcelFunction.DIRECTORY, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FIND", ExcelFunction.FIND, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1)
		};
		RegisterFunction("CELL", ExcelFunction.CELL, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISERR", ExcelFunction.ISERR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISTEXT", ExcelFunction.ISTEXT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISNUMBER", ExcelFunction.ISNUMBER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISBLANK", ExcelFunction.ISBLANK, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("T", ExcelFunction.T, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("N", ExcelFunction.N, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FOPEN", ExcelFunction.FOPEN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FCLOSE", ExcelFunction.FCLOSE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FSIZE", ExcelFunction.FSIZE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FREADLN", ExcelFunction.FREADLN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FREAD", ExcelFunction.FREAD, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FWRITELN", ExcelFunction.FWRITELN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FWRITE", ExcelFunction.FWRITE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FPOS", ExcelFunction.FPOS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DATEVALUE", ExcelFunction.DATEVALUE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TIMEVALUE", ExcelFunction.TIMEVALUE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SLN", ExcelFunction.SLN, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SYD", ExcelFunction.SYD, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DDB", ExcelFunction.DDB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETDEF", ExcelFunction.GETDEF, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("REFTEXT", ExcelFunction.REFTEXT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("TEXTREF", ExcelFunction.TEXTREF, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("INDIRECT", ExcelFunction.INDIRECT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("REGISTER", ExcelFunction.REGISTER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CALL", ExcelFunction.CALL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ADDBAR", ExcelFunction.ADDBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ADDMENU", ExcelFunction.ADDMENU, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ADDCOMMAND", ExcelFunction.ADDCOMMAND, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ENABLECOMMAND", ExcelFunction.ENABLECOMMAND, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CHECKCOMMAND", ExcelFunction.CHECKCOMMAND, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("RENAMECOMMAND", ExcelFunction.RENAMECOMMAND, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SHOWBAR", ExcelFunction.SHOWBAR, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DELETEMENU", ExcelFunction.DELETEMENU, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DELETECOMMAND", ExcelFunction.DELETECOMMAND, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETCHARTITEM", ExcelFunction.GETCHARTITEM, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DIALOGBOX", ExcelFunction.DIALOGBOX, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CLEAN", ExcelFunction.CLEAN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("MDETERM", ExcelFunction.MDETERM, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("MINVERSE", ExcelFunction.MINVERSE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("MMULT", ExcelFunction.MMULT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FILES", ExcelFunction.FILES, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IPMT", ExcelFunction.IPMT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PPMT", ExcelFunction.PPMT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("COUNTA", ExcelFunction.COUNTA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CANCELKEY", ExcelFunction.CANCELKEY, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("INITIATE", ExcelFunction.INITIATE, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("REQUEST", ExcelFunction.REQUEST, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("POKE", ExcelFunction.POKE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("EXECUTE", ExcelFunction.EXECUTE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("TERMINATE", ExcelFunction.TERMINATE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("RESTART", ExcelFunction.RESTART, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("HELP", ExcelFunction.HELP, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETBAR", ExcelFunction.GETBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("PRODUCT", ExcelFunction.PRODUCT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FACT", ExcelFunction.FACT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETCELL", ExcelFunction.GETCELL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETWORKSPACE", ExcelFunction.GETWORKSPACE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETWINDOW", ExcelFunction.GETWINDOW, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETDOCUMENT", ExcelFunction.GETDOCUMENT, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DPRODUCT", ExcelFunction.DPRODUCT, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISNONTEXT", ExcelFunction.ISNONTEXT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETNOTE", ExcelFunction.GETNOTE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("NOTE", ExcelFunction.NOTE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("STDEVP", ExcelFunction.STDEVP, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("VARP", ExcelFunction.VARP, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DSTDEVP", ExcelFunction.DSTDEVP, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DVARP", ExcelFunction.DVARP, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TRUNC", ExcelFunction.TRUNC, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISLOGICAL", ExcelFunction.ISLOGICAL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DCOUNTA", ExcelFunction.DCOUNTA, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DELETEBAR", ExcelFunction.DELETEBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("UNREGISTER", ExcelFunction.UNREGISTER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("USDOLLAR", ExcelFunction.USDOLLAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FINDB", ExcelFunction.FINDB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SEARCHB", ExcelFunction.SEARCHB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("REPLACEB", ExcelFunction.REPLACEB, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LEFTB", ExcelFunction.LEFTB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RIGHTB", ExcelFunction.RIGHTB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MIDB", ExcelFunction.MIDB, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LENB", ExcelFunction.LENB, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ROUNDUP", ExcelFunction.ROUNDUP, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ROUNDDOWN", ExcelFunction.ROUNDDOWN, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ASC", ExcelFunction.ASC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DBCS", ExcelFunction.DBCS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1)
		};
		RegisterFunction("RANK", ExcelFunction.RANK, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("ADDRESS", ExcelFunction.ADDRESS, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DAYS360", ExcelFunction.DAYS360, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("TODAY", ExcelFunction.TODAY, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("VDB", ExcelFunction.VDB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("MEDIAN", ExcelFunction.MEDIAN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("SUMPRODUCT", ExcelFunction.SUMPRODUCT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SINH", ExcelFunction.SINH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COSH", ExcelFunction.COSH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TANH", ExcelFunction.TANH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ASINH", ExcelFunction.ASINH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ACOSH", ExcelFunction.ACOSH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ATANH", ExcelFunction.ATANH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DGET", ExcelFunction.DGET, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CREATEOBJECT", ExcelFunction.CREATEOBJECT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("VOLATILE", ExcelFunction.VOLATILE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("LASTERROR", ExcelFunction.LASTERROR, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CUSTOMUNDO", ExcelFunction.CUSTOMUNDO, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CUSTOMREPEAT", ExcelFunction.CUSTOMREPEAT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("FORMULACONVERT", ExcelFunction.FORMULACONVERT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETLINKINFO", ExcelFunction.GETLINKINFO, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("TEXTBOX", ExcelFunction.TEXTBOX, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("INFO", ExcelFunction.INFO, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GROUP", ExcelFunction.GROUP, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETOBJECT", ExcelFunction.GETOBJECT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DB", ExcelFunction.DB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("PAUSE", ExcelFunction.PAUSE, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("RESUME", ExcelFunction.RESUME, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("FREQUENCY", ExcelFunction.FREQUENCY, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ADDTOOLBAR", ExcelFunction.ADDTOOLBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DELETETOOLBAR", ExcelFunction.DELETETOOLBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("CustomFunction", ExcelFunction.CustomFunction, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("RESETTOOLBAR", ExcelFunction.RESETTOOLBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("EVALUATE", ExcelFunction.EVALUATE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETTOOLBAR", ExcelFunction.GETTOOLBAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETTOOL", ExcelFunction.GETTOOL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SPELLINGCHECK", ExcelFunction.SPELLINGCHECK, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ERROR.TYPE", ExcelFunction.ERRORTYPE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("APPTITLE", ExcelFunction.APPTITLE, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("WINDOWTITLE", ExcelFunction.WINDOWTITLE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SAVETOOLBAR", ExcelFunction.SAVETOOLBAR, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("ENABLETOOL", ExcelFunction.ENABLETOOL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("PRESSTOOL", ExcelFunction.PRESSTOOL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("REGISTERID", ExcelFunction.REGISTERID, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETWORKBOOK", ExcelFunction.GETWORKBOOK, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("AVEDEV", ExcelFunction.AVEDEV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BETA.DIST", ExcelFunction.BETA_DIST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("T.TEST", ExcelFunction.T_TEST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("T.INV.2T", ExcelFunction.T_INV_2T, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EXPON.DIST", ExcelFunction.EXPON_DIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BETADIST", ExcelFunction.BETADIST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EUROCONVERT", ExcelFunction.EUROCONVERT, paramIndexes, 5);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("REGISTER.ID", ExcelFunction.REGISTER_ID, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PHONETIC", ExcelFunction.PHONETIC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SQL.REQUEST", ExcelFunction.SQL_REQUEST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("JIS", ExcelFunction.JIS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMALN", ExcelFunction.GAMMALN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BETAINV", ExcelFunction.BETAINV, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BINOMDIST", ExcelFunction.BINOMDIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHIDIST", ExcelFunction.CHIDIST, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CHIINV", ExcelFunction.CHIINV, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COMBIN", ExcelFunction.COMBIN, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CONFIDENCE", ExcelFunction.CONFIDENCE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CRITBINOM", ExcelFunction.CRITBINOM, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EVEN", ExcelFunction.EVEN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("EXPONDIST", ExcelFunction.EXPONDIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FDIST", ExcelFunction.FDIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FINV", ExcelFunction.FINV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FISHER", ExcelFunction.FISHER, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FISHERINV", ExcelFunction.FISHERINV, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FLOOR", ExcelFunction.FLOOR, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMADIST", ExcelFunction.GAMMADIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMAINV", ExcelFunction.GAMMAINV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CEILING", ExcelFunction.CEILING, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("HYPGEOMDIST", ExcelFunction.HYPGEOMDIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOGNORMDIST", ExcelFunction.LOGNORMDIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("LOGINV", ExcelFunction.LOGINV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NEGBINOMDIST", ExcelFunction.NEGBINOMDIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORMDIST", ExcelFunction.NORMDIST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORMSDIST", ExcelFunction.NORMSDIST, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORMINV", ExcelFunction.NORMINV, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NORMSINV", ExcelFunction.NORMSINV, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("STANDARDIZE", ExcelFunction.STANDARDIZE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ODD", ExcelFunction.ODD, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PERMUT", ExcelFunction.PERMUT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("POISSON", ExcelFunction.POISSON, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TDIST", ExcelFunction.TDIST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WEIBULL", ExcelFunction.WEIBULL, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("SUMXMY2", ExcelFunction.SUMXMY2, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("SUMX2MY2", ExcelFunction.SUMX2MY2, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("SUMX2PY2", ExcelFunction.SUMX2PY2, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("CHITEST", ExcelFunction.CHITEST, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("CORREL", ExcelFunction.CORREL, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("COVAR", ExcelFunction.COVAR, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 3, 3)
		};
		RegisterFunction("FORECAST", ExcelFunction.FORECAST, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("FTEST", ExcelFunction.FTEST, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("INTERCEPT", ExcelFunction.INTERCEPT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("PEARSON", ExcelFunction.PEARSON, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("RSQ", ExcelFunction.RSQ, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("STEYX", ExcelFunction.STEYX, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("SLOPE", ExcelFunction.SLOPE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("TTEST", ExcelFunction.TTEST, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3, 3, 2)
		};
		RegisterFunction("PROB", ExcelFunction.PROB, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("DEVSQ", ExcelFunction.DEVSQ, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("GEOMEAN", ExcelFunction.GEOMEAN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("HARMEAN", ExcelFunction.HARMEAN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("SUMSQ", ExcelFunction.SUMSQ, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3),
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("KURT", ExcelFunction.KURT, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("SKEW", ExcelFunction.SKEW, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("ZTEST", ExcelFunction.ZTEST, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("LARGE", ExcelFunction.LARGE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("SMALL", ExcelFunction.SMALL, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("QUARTILE", ExcelFunction.QUARTILE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("PERCENTILE", ExcelFunction.PERCENTILE, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("PERCENTRANK", ExcelFunction.PERCENTRANK, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("MODE", ExcelFunction.MODE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 3)
		};
		RegisterFunction("TRIMMEAN", ExcelFunction.TRIMMEAN, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("TINV", ExcelFunction.TINV, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("MOVIECOMMAND", ExcelFunction.MOVIECOMMAND, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETMOVIE", ExcelFunction.GETMOVIE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CONCATENATE", ExcelFunction.CONCATENATE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("POWER", ExcelFunction.POWER, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("PIVOTADDDATA", ExcelFunction.PIVOTADDDATA, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETPIVOTTABLE", ExcelFunction.GETPIVOTTABLE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETPIVOTFIELD", ExcelFunction.GETPIVOTFIELD, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("GETPIVOTITEM", ExcelFunction.GETPIVOTITEM, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RADIANS", ExcelFunction.RADIANS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DEGREES", ExcelFunction.DEGREES, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2, 1)
		};
		RegisterFunction("SUBTOTAL", ExcelFunction.SUBTOTAL, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2, 1)
		};
		RegisterFunction("SUMIF", ExcelFunction.SUMIF, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1, 2)
		};
		RegisterFunction("COUNTIF", ExcelFunction.COUNTIF, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("COUNTBLANK", ExcelFunction.COUNTBLANK, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SCENARIOGET", ExcelFunction.SCENARIOGET, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("OPTIONSLISTSGET", ExcelFunction.OPTIONSLISTSGET, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISPMT", ExcelFunction.ISPMT, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DATEDIF", ExcelFunction.DATEDIF, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("DATESTRING", ExcelFunction.DATESTRING, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("NUMBERSTRING", ExcelFunction.NUMBERSTRING, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ROMAN", ExcelFunction.ROMAN, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("OPENDIALOG", ExcelFunction.OPENDIALOG, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("SAVEDIALOG", ExcelFunction.SAVEDIALOG, paramIndexes, 0);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("GETPIVOTDATA", ExcelFunction.GETPIVOTDATA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("HYPERLINK", ExcelFunction.HYPERLINK, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("AVERAGEA", ExcelFunction.AVERAGEA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("MAXA", ExcelFunction.MAXA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[2]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1),
			new ReferenceIndexAttribute(typeof(ArrayPtg), 3)
		};
		RegisterFunction("MINA", ExcelFunction.MINA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("STDEVPA", ExcelFunction.STDEVPA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("VARPA", ExcelFunction.VARPA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("STDEVA", ExcelFunction.STDEVA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 1)
		};
		RegisterFunction("VARA", ExcelFunction.VARA, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[0];
		RegisterFunction("NONE", ExcelFunction.NONE, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DAYS", ExcelFunction.DAYS, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISOWEEKNUM", ExcelFunction.ISOWEEKNUM, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BITAND", ExcelFunction.BITAND, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BITLSHIFT", ExcelFunction.BITLSHIFT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BITOR", ExcelFunction.BITOR, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BITRSHIFT", ExcelFunction.BITRSHIFT, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BITXOR", ExcelFunction.BITXOR, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMCOSH", ExcelFunction.IMCOSH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMCOT", ExcelFunction.IMCOT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMCSC", ExcelFunction.IMCSC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMCSCH", ExcelFunction.IMCSCH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSEC", ExcelFunction.IMSEC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSECH", ExcelFunction.IMSECH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMSINH", ExcelFunction.IMSINH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IMTAN", ExcelFunction.IMTAN, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PDURATION", ExcelFunction.PDURATION, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("RRI", ExcelFunction.RRI, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISFORMULA", ExcelFunction.ISFORMULA, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SHEET", ExcelFunction.SHEET, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SHEETS", ExcelFunction.SHEETS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("IFNA", ExcelFunction.IFNA, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("XOR", ExcelFunction.XOR, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FORMULATEXT", ExcelFunction.FORMULATEXT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ACOT", ExcelFunction.ACOT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ACOTH", ExcelFunction.ACOTH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ARABIC", ExcelFunction.ARABIC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BASE", ExcelFunction.BASE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CEILING.MATH", ExcelFunction.CEILING_MATH, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COMBINA", ExcelFunction.COMBINA, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COT", ExcelFunction.COT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("COTH", ExcelFunction.COTH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CSC", ExcelFunction.CSC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("CSCH", ExcelFunction.CSCH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("DECIMAL", ExcelFunction.DECIMAL, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FLOOR.MATH", ExcelFunction.FLOOR_MATH, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ISO.CEILING", ExcelFunction.ISO_CEILING, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("MUNIT", ExcelFunction.MUNIT, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SEC", ExcelFunction.SEC, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SECH", ExcelFunction.SECH, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("BINOM.DIST.RANGE", ExcelFunction.BINOM_DIST_RANGE, paramIndexes, 4);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAMMA", ExcelFunction.GAMMA, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("GAUSS", ExcelFunction.GAUSS, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PERMUTATIONA", ExcelFunction.PERMUTATIONA, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("PHI", ExcelFunction.PHI, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("SKEW.P", ExcelFunction.SKEW_P, paramIndexes, -1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("NUMBERVALUE", ExcelFunction.NUMBERVALUE, paramIndexes, 3);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("UNICHAR", ExcelFunction.UNICHAR, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("UNICODE", ExcelFunction.UNICODE, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("ENCODEURL", ExcelFunction.ENCODEURL, paramIndexes, 1);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("FILTERXML", ExcelFunction.FILTERXML, paramIndexes, 2);
		paramIndexes = new ReferenceIndexAttribute[1]
		{
			new ReferenceIndexAttribute(typeof(RefPtg), 2)
		};
		RegisterFunction("WEBSERVICE", ExcelFunction.WEBSERVICE, paramIndexes, 1);
	}

	private static void FillErrorNames()
	{
		Type[] assemblyTypes = ApplicationImpl.AssemblyTypes;
		int i = 0;
		for (int num = assemblyTypes.Length; i < num; i++)
		{
			AddErrorNames(assemblyTypes[i]);
		}
	}

	private static void AddErrorNames(Type type)
	{
		object[] customAttributes = type.GetTypeInfo().GetCustomAttributes(typeof(ErrorCodeAttribute), inherit: false);
		int i = 0;
		for (int num = customAttributes.Length; i < num; i++)
		{
			ErrorCodeAttribute errorCodeAttribute = customAttributes[i] as ErrorCodeAttribute;
			ConstructorInfo constructor = type.GetConstructor(new Type[1] { typeof(string) });
			ErrorNameToConstructor.Add(errorCodeAttribute.StringValue, constructor);
			s_hashErrorCodeToName.Add(errorCodeAttribute.ErrorCode, errorCodeAttribute.StringValue);
			s_hashNameToErrorCode.Add(errorCodeAttribute.StringValue, errorCodeAttribute.ErrorCode);
		}
	}

	private void FillPriorities()
	{
		int num = m_arrOperationGroups.Length;
		for (int i = 0; i < num; i++)
		{
			int j = 0;
			for (int num2 = m_arrOperationGroups[i].Length; j < num2; j++)
			{
				m_arrAllOperations[m_arrOperationGroups[i][j]] = i;
			}
		}
		int num3 = 0;
		int num4 = 0;
		string[] array = null;
		for (int num5 = num - 1; num5 >= 0; num5--)
		{
			num4 = m_arrOperationGroups[num5].Length;
			num3 += num4;
			string[] array2 = new string[num3];
			if (num5 < num - 1)
			{
				array.CopyTo(array2, num4);
			}
			m_arrOperationGroups[num5].CopyTo(array2, 0);
			m_arrOperationsWithPriority[num5] = GetSortedList(array2);
			array = array2;
		}
	}

	private static TypedSortedListEx<string, object> GetSortedList(string[] arrStrings)
	{
		if (arrStrings == null)
		{
			throw new ArgumentNullException("arrStrings");
		}
		int num = arrStrings.Length;
		TypedSortedListEx<string, object> typedSortedListEx = new TypedSortedListEx<string, object>(StringComparer.CurrentCulture, num);
		for (int i = 0; i < num; i++)
		{
			typedSortedListEx.Add(arrStrings[i], null);
		}
		return typedSortedListEx;
	}

	public Ptg[] ParseSharedString(string strFormula, int iFirstRow, int iFirstColumn, IWorksheet sheet)
	{
		Ptg[] tokens = ParseString(strFormula, sheet, null);
		IWorkbook workbook = sheet.Workbook;
		return ConvertTokensToShared(tokens, iFirstRow, iFirstColumn, workbook);
	}

	public Ptg[] ConvertTokensToShared(Ptg[] tokens, int row, int column, IWorkbook book)
	{
		if (tokens != null)
		{
			int num = tokens.Length;
			for (int i = 0; i < num; i++)
			{
				if (!(tokens[i] is IReference))
				{
					tokens[i] = tokens[i].ConvertPtgToNPtg(book, row - 1, column - 1);
				}
			}
		}
		return tokens;
	}

	public Ptg[] ParseString(string strFormula)
	{
		return ParseString(strFormula, null, null);
	}

	public Ptg[] ParseString(string strFormula, IWorksheet sheet, Dictionary<string, string> hashWorksheetNames)
	{
		return ParseString(strFormula, sheet, null, 0, hashWorksheetNames, OfficeParseFormulaOptions.RootLevel, 0, 0);
	}

	public Ptg[] ParseString(string strFormula, IWorksheet sheet, Dictionary<string, string> hashWorksheetNames, int iCellRow, int iCellColumn, bool bR1C1)
	{
		OfficeParseFormulaOptions options = ((!bR1C1) ? OfficeParseFormulaOptions.RootLevel : (OfficeParseFormulaOptions.RootLevel | OfficeParseFormulaOptions.UseR1C1));
		ParseParameters arguments = new ParseParameters(sheet, hashWorksheetNames, bR1C1, iCellRow, iCellColumn, this, m_book);
		m_parser.Parse(strFormula, null, 0, options, arguments);
		return m_parser.Tokens.ToArray();
	}

	public Ptg[] ParseString(string strFormula, IWorksheet sheet, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		bool r1C = (options & OfficeParseFormulaOptions.UseR1C1) != 0;
		ParseParameters arguments = new ParseParameters(sheet, hashWorksheetNames, r1C, iCellRow, iCellColumn, this, m_book);
		m_parser.Parse(strFormula, indexes, i, options, arguments);
		return m_parser.Tokens.ToArray();
	}

	public string GetLeftUnaryOperand(string strFormula, int OpIndex)
	{
		return GetOperand(strFormula, OpIndex, m_arrAllOperations, IsLeft: true);
	}

	public string GetRightUnaryOperand(string strFormula, int OpIndex)
	{
		string text = GetOperand(strFormula, OpIndex, m_arrAllOperations, IsLeft: false);
		int length = text.Length;
		if (length > 0 && text[length - 1] == '%')
		{
			text = "%" + text.Substring(0, length - 1);
		}
		return text;
	}

	public string GetRightBinaryOperand(string strFormula, int iFirstChar, string operation)
	{
		int num = (int)m_arrAllOperations[operation];
		return GetOperand(strFormula, iFirstChar - 1, m_arrOperationsWithPriority[num], IsLeft: false);
	}

	public string GetFunctionOperand(string strFormula, int iFirstChar)
	{
		TypedSortedListEx<string, object> sortedList = GetSortedList(new string[1] { OperandsSeparator });
		return GetOperand(strFormula, iFirstChar, sortedList, IsLeft: false);
	}

	[CLSCompliant(false)]
	public string ParseFormulaRecord(FormulaRecord formula)
	{
		return ParseFormulaRecord(formula, bR1C1: false);
	}

	[CLSCompliant(false)]
	public string ParseFormulaRecord(FormulaRecord formula, bool bR1C1)
	{
		Ptg[] parsedExpression = formula.ParsedExpression;
		return ParsePtgArray(parsedExpression, formula.Row, formula.Column, bR1C1, isForSerialization: false);
	}

	[CLSCompliant(false)]
	public string ParseSharedFormula(ISharedFormula sharedFormula)
	{
		Ptg[] formula = sharedFormula.Formula;
		return ParsePtgArray(formula, 0, 0, bR1C1: false, isForSerialization: false);
	}

	[CLSCompliant(false)]
	public string ParseSharedFormula(ISharedFormula sharedFormula, int row, int col)
	{
		return ParseSharedFormula(sharedFormula, row, col, bR1C1: false, isForSerialization: false);
	}

	[CLSCompliant(false)]
	public string ParseSharedFormula(ISharedFormula sharedFormula, int row, int col, bool bR1C1, bool isForSerialization)
	{
		Ptg[] formula = sharedFormula.Formula;
		return ParsePtgArray(formula, row, col, bR1C1, isForSerialization);
	}

	public string ParsePtgArray(Ptg[] ptgs)
	{
		if (ptgs == null)
		{
			return null;
		}
		return ParsePtgArray(ptgs, 0, 0, bR1C1: false, isForSerialization: false);
	}

	public string ParsePtgArray(Ptg[] ptgs, int row, int col, bool bR1C1, bool isForSerialization)
	{
		return ParsePtgArray(ptgs, row, col, bR1C1, null, isForSerialization);
	}

	public string ParsePtgArray(Ptg[] ptgs, int row, int col, bool bR1C1, NumberFormatInfo numberInfo, bool isForSerialization)
	{
		return ParsePtgArray(ptgs, row, col, bR1C1, numberInfo, bRemoveSheetNames: false, isForSerialization, null);
	}

	public string ParsePtgArray(Ptg[] ptgs, int row, int col, bool bR1C1, NumberFormatInfo numberInfo, bool bRemoveSheetNames, bool isForSerialization, IWorksheet sheet)
	{
		if (ptgs == null)
		{
			return null;
		}
		if (numberInfo == null)
		{
			numberInfo = m_numberFormat;
		}
		ptgs = SkipUnnecessaryTokens(ptgs);
		string text = string.Empty;
		_ = ptgs.Length;
		Stack<object> stack = new Stack<object>();
		int i = 0;
		for (int num = ptgs.Length; i < num; i++)
		{
			Ptg ptg = ptgs[i];
			if (!ptg.IsOperation)
			{
				ISheetReference sheetReference = ptg as ISheetReference;
				string operand = ((!bRemoveSheetNames || sheetReference == null) ? ptg.ToString(this, row, col, bR1C1, numberInfo, isForSerialization, sheet) : sheetReference.BaseToString(this, row, col, bR1C1));
				PushOperandToStack(stack, operand);
			}
			else
			{
				((OperationPtg)ptg).PushResultToStack(this, stack, isForSerialization);
			}
		}
		if (stack.Count != 0)
		{
			text += stack.Pop().ToString();
		}
		return text;
	}

	public void CheckFormulaVersion(Ptg[] ptgs)
	{
		if (ptgs == null)
		{
			return;
		}
		int i = 0;
		for (int num = ptgs.Length; i < num; i++)
		{
			if (ptgs[i] is FunctionPtg functionPtg && IsExcel2010Function(functionPtg.FunctionIndex) && m_book.Version != OfficeVersion.Excel2010 && m_book.Version != OfficeVersion.Excel2013)
			{
				throw new NotSupportedException("The formula is not supported in this Version");
			}
		}
	}

	public List<string> SplitArray(string strFormula, string strSeparator)
	{
		if (strFormula == null)
		{
			throw new ArgumentNullException("strFormula");
		}
		if (strFormula.Length == 0)
		{
			throw new ArgumentException("strFormula - string cannot be empty");
		}
		List<string> list = new List<string>();
		TypedSortedListEx<string, object> typedSortedListEx = new TypedSortedListEx<string, object>(1);
		typedSortedListEx.Add(strSeparator, null);
		int num = -1;
		int length = strFormula.Length;
		while (num < length)
		{
			string operand = GetOperand(strFormula, num, typedSortedListEx, IsLeft: false);
			num += operand.Length + 1;
			list.Add(operand);
		}
		return list;
	}

	public bool UpdateNameIndex(Ptg ptg, int[] arrNewIndex)
	{
		if (ptg == null)
		{
			return false;
		}
		bool result = false;
		if (IndexOf(NameCodes, ptg.TokenCode) != -1)
		{
			NamePtg namePtg = (NamePtg)ptg;
			int num = namePtg.ExternNameIndex - 1;
			int num2 = arrNewIndex[num];
			if (num != num2)
			{
				namePtg.ExternNameIndex = (ushort)(num2 + 1);
				result = true;
			}
		}
		else if (IndexOf(NameXCodes, ptg.TokenCode) != -1)
		{
			NameXPtg nameXPtg = (NameXPtg)ptg;
			if (m_book.IsLocalReference(nameXPtg.RefIndex))
			{
				int num3 = nameXPtg.NameIndex - 1;
				int num4 = arrNewIndex[num3];
				if (num4 != num3)
				{
					nameXPtg.NameIndex = (ushort)(num4 + 1);
					result = true;
				}
			}
		}
		return result;
	}

	public bool UpdateNameIndex(Ptg ptg, IDictionary<int, int> dicNewIndex)
	{
		if (ptg == null)
		{
			throw new ArgumentNullException("ptg");
		}
		bool result = false;
		if (IndexOf(NameCodes, ptg.TokenCode) != -1)
		{
			NamePtg namePtg = (NamePtg)ptg;
			int num = namePtg.ExternNameIndex - 1;
			int num2 = (dicNewIndex.ContainsKey(num) ? dicNewIndex[num] : num);
			if (num != num2)
			{
				namePtg.ExternNameIndex = (ushort)(num2 + 1);
				result = true;
			}
		}
		else if (IndexOf(NameXCodes, ptg.TokenCode) != -1)
		{
			NameXPtg nameXPtg = (NameXPtg)ptg;
			if (m_book.IsLocalReference(nameXPtg.RefIndex))
			{
				int num3 = nameXPtg.NameIndex - 1;
				int num4 = (dicNewIndex.ContainsKey(num3) ? dicNewIndex[num3] : num3);
				if (num4 != num3)
				{
					nameXPtg.NameIndex = (ushort)(num4 + 1);
					result = true;
				}
			}
		}
		return result;
	}

	public bool UpdateNameIndex(Ptg[] arrExpression, IDictionary<int, int> dicNewIndex)
	{
		if (arrExpression == null)
		{
			return false;
		}
		if (dicNewIndex == null)
		{
			throw new ArgumentNullException("dicNewIndex");
		}
		bool flag = false;
		int i = 0;
		for (int num = arrExpression.Length; i < num; i++)
		{
			flag |= UpdateNameIndex(arrExpression[i], dicNewIndex);
		}
		return flag;
	}

	public bool UpdateNameIndex(Ptg[] arrExpression, int[] arrNewIndex)
	{
		if (arrExpression == null)
		{
			return false;
		}
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		bool flag = false;
		int i = 0;
		for (int num = arrExpression.Length; i < num; i++)
		{
			flag |= UpdateNameIndex(arrExpression[i], arrNewIndex);
		}
		return flag;
	}

	public void SetSeparators(char operandsSeparator, char arrayRowsSeparator)
	{
		string text = operandsSeparator.ToString();
		string text2 = arrayRowsSeparator.ToString();
		if (!(text == m_strOperandsSeparator) || !(text2 == m_strArrayRowSeparator))
		{
			string[] arrOldKey = new string[2] { m_strOperandsSeparator, m_strArrayRowSeparator };
			string[] arrNewKey = new string[2] { text, text2 };
			ReplaceInDictionary(m_arrAllOperations, arrOldKey, arrNewKey);
			int i = 0;
			for (int num = m_arrOperationsWithPriority.Length; i < num; i++)
			{
				TypedSortedListEx<string, object> list = m_arrOperationsWithPriority[i];
				ReplaceInDictionary(list, arrOldKey, arrNewKey);
			}
			m_strOperandsSeparator = text;
			m_strArrayRowSeparator = text2;
			m_parser.SetSeparators(operandsSeparator, arrayRowsSeparator);
		}
	}

	private void ReplaceInDictionary(IDictionary list, string[] arrOldKey, string[] arrNewKey)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (arrOldKey == null)
		{
			throw new ArgumentNullException("arrOldKey");
		}
		if (arrNewKey == null)
		{
			throw new ArgumentNullException("arrNewKey");
		}
		int num = arrOldKey.Length;
		if (num != arrNewKey.Length)
		{
			throw new ArgumentException("arrOldKey and arrNewKey do not correspond each other");
		}
		object[] array = new object[num];
		bool[] array2 = new bool[num];
		for (int i = 0; i < num; i++)
		{
			string key = arrOldKey[i];
			bool flag;
			array2[i] = (flag = list.Contains(key));
			if (flag)
			{
				array[i] = list[key];
				list.Remove(key);
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (array2[j])
			{
				list.Add(arrNewKey[j], array[j]);
			}
		}
	}

	public static void MarkUsedReferences(Ptg[] tokens, bool[] usedItems)
	{
		if (tokens == null)
		{
			return;
		}
		int i = 0;
		for (int num = tokens.Length; i < num; i++)
		{
			if (tokens[i] is IReference reference)
			{
				usedItems[reference.RefIndex] = true;
			}
		}
	}

	public static bool UpdateReferenceIndexes(Ptg[] tokens, int[] arrUpdatedIndexes)
	{
		bool result = false;
		if (tokens != null)
		{
			int i = 0;
			for (int num = tokens.Length; i < num; i++)
			{
				if (tokens[i] is IReference reference)
				{
					int refIndex = reference.RefIndex;
					int num2 = arrUpdatedIndexes[refIndex];
					reference.RefIndex = (ushort)num2;
					result = true;
				}
			}
		}
		return result;
	}

	private Ptg CreateConstantPtg(string strFormula, IWorksheet sheet, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options)
	{
		return CreateConstantPtg(strFormula, sheet, null, 0, hashWorksheetNames, options, 0, 0);
	}

	private Ptg CreateConstantPtg(string strFormula, IWorksheet sheet, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		bool flag = (options & OfficeParseFormulaOptions.UseR1C1) != 0;
		if (strFormula.Length == 0)
		{
			return CreatePtg(FormulaToken.tMissingArgument);
		}
		if (strFormula[0] == '"')
		{
			return CreatePtg(FormulaToken.tStringConstant, strFormula.Substring(1, strFormula.Length - 2));
		}
		if (strFormula[0] == '{')
		{
			FormulaToken token = ArrayPtg.IndexToCode(GetIndex(typeof(ArrayPtg), 2, indexes, i, options));
			return CreatePtg(token, strFormula, this);
		}
		if (IsCellRange(strFormula, flag, out var strRow, out var strColumn, out var strRow2, out var strColumn2))
		{
			FormulaToken token = AreaPtg.IndexToCode(GetIndex(typeof(AreaPtg), 0, indexes, i, options));
			return CreatePtg(token, iCellRow, iCellColumn, strRow, strColumn, strRow2, strColumn2, flag, m_book);
		}
		if (IsCell(strFormula, flag, out strRow, out strColumn))
		{
			FormulaToken token = RefPtg.IndexToCode(GetIndex(typeof(RefPtg), 0, indexes, i, options));
			return CreatePtg(token, iCellRow, iCellColumn, strRow, strColumn, flag);
		}
		if (IsCellRange3D(strFormula, flag, out var strSheetName, out strRow, out strColumn, out strRow2, out strColumn2))
		{
			FormulaToken token = Area3DPtg.IndexToCode(GetIndex(typeof(Area3DPtg), 0, indexes, i, options));
			if (hashWorksheetNames != null && hashWorksheetNames.ContainsKey(strSheetName))
			{
				strFormula = strFormula.Replace(strSheetName, hashWorksheetNames[strSheetName]);
			}
			int num = m_book.AddSheetReference(strSheetName);
			return CreatePtg(token, iCellRow, iCellColumn, num, strRow, strColumn, strRow2, strColumn2, flag);
		}
		if (IsCell3D(strFormula, flag, out strSheetName, out strRow, out strColumn))
		{
			FormulaToken token = Ref3DPtg.IndexToCode(GetIndex(typeof(Ref3DPtg), 0, indexes, i, options));
			if (hashWorksheetNames != null && hashWorksheetNames.ContainsKey(strSheetName))
			{
				strSheetName = hashWorksheetNames[strSheetName];
			}
			int num2 = m_book.AddSheetReference(strSheetName);
			return CreatePtg(token, iCellRow, iCellColumn, num2, strRow, strColumn, flag);
		}
		if (IsNamedRange(strFormula, m_book, sheet))
		{
			FormulaToken token = NamePtg.IndexToCode(GetIndex(typeof(NamePtg), 0, indexes, i, options));
			if (sheet != null)
			{
				return CreatePtg(token, strFormula, m_book, sheet);
			}
			return CreatePtg(token, strFormula, m_book);
		}
		double result;
		bool flag2 = double.TryParse(strFormula, NumberStyles.Integer, null, out result);
		if (flag2 && result <= 65535.0 && result >= 0.0)
		{
			return CreatePtg(FormulaToken.tInteger, strFormula);
		}
		if (!flag2)
		{
			flag2 = double.TryParse(strFormula, NumberStyles.Any, m_numberFormat, out result);
		}
		if (flag2)
		{
			return CreatePtg(FormulaToken.tNumber, result);
		}
		try
		{
			bool.Parse(strFormula);
			return CreatePtg(FormulaToken.tBoolean, strFormula);
		}
		catch (FormatException)
		{
		}
		if (!m_book.ThrowOnUnknownNames)
		{
			m_book.Names.Add(strFormula);
			FormulaToken token = NamePtg.IndexToCode(GetIndex(typeof(NamePtg), 0, indexes, i, options));
			return CreatePtg(token, strFormula, m_book);
		}
		throw new ArgumentException("Can't parse formula: " + strFormula);
	}

	private static string NormalizeSheetName(string strSheetName)
	{
		if (strSheetName == null)
		{
			return null;
		}
		int length = strSheetName.Length;
		if (length >= 2 && strSheetName[0] == '\'' && '\'' == strSheetName[length - 1])
		{
			strSheetName = strSheetName.Substring(1, length - 2);
		}
		return strSheetName;
	}

	public static Ptg[] ParseExpression(DataProvider provider, int iLength, OfficeVersion version)
	{
		List<Ptg> list = new List<Ptg>();
		int offset = 0;
		while (offset < iLength)
		{
			list.Add(CreatePtg(provider, ref offset, version));
		}
		return list.ToArray();
	}

	public static Ptg[] ParseExpression(DataProvider provider, int offset, int iExpressionLength, out int finalOffset, OfficeVersion version)
	{
		List<Ptg> list = new List<Ptg>();
		int num = offset + iExpressionLength;
		while (offset < num)
		{
			list.Add(CreatePtg(provider, ref offset, version));
		}
		finalOffset = offset;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is IAdditionalData)
			{
				finalOffset = (list[i] as IAdditionalData).ReadArray(provider, finalOffset);
			}
		}
		return list.ToArray();
	}

	public static byte[] PtgArrayToByteArray(Ptg[] tokens, OfficeVersion version)
	{
		if (tokens == null)
		{
			return null;
		}
		int formulaLen;
		return PtgArrayToByteArray(tokens, out formulaLen, version);
	}

	public static byte[] PtgArrayToByteArray(Ptg[] arrTokens, out int formulaLen, OfficeVersion version)
	{
		if (arrTokens == null)
		{
			throw new ArgumentNullException("arrTokens");
		}
		BytesList bytesList = new BytesList(bExactSize: true);
		int num = arrTokens.Length;
		int i = 0;
		for (int num2 = num; i < num2; i++)
		{
			bytesList.AddRange(arrTokens[i].ToByteArray(version));
		}
		formulaLen = bytesList.Count;
		for (int j = 0; j < num; j++)
		{
			if (arrTokens[j] is ArrayPtg)
			{
				BytesList listBytes = (arrTokens[j] as ArrayPtg).GetListBytes();
				bytesList.AddRange(listBytes);
			}
		}
		return bytesList.InnerBuffer;
	}

	public static string GetLeftBinaryOperand(string strFormula, int OpIndex)
	{
		return GetOperand(strFormula, OpIndex, m_listPlusMinus, IsLeft: true);
	}

	public static int FindCorrespondingBracket(string strFormula, int BracketPos)
	{
		int delta;
		char[] startBrackets;
		if (IndexOf(OpenBrackets, strFormula[BracketPos]) != -1)
		{
			delta = 1;
			startBrackets = OpenBrackets;
		}
		else
		{
			if (IndexOf(CloseBrackets, strFormula[BracketPos]) == -1)
			{
				throw new ArgumentOutOfRangeException("Specified position is not a position of bracket");
			}
			delta = -1;
			startBrackets = CloseBrackets;
		}
		return FindCorrespondingBracket(strFormula, BracketPos, startBrackets, delta);
	}

	public static string GetOperand(string strFormula, int OpIndex, TypedSortedListEx<string, object> arrBreakStrings, bool IsLeft)
	{
		if (strFormula == null)
		{
			throw new ArgumentNullException("strFormula");
		}
		if (arrBreakStrings == null)
		{
			throw new ArgumentNullException("arrBreakStrings");
		}
		char[] array = (IsLeft ? CloseBrackets : OpenBrackets);
		char[] array2 = (IsLeft ? OpenBrackets : CloseBrackets);
		int num = ((!IsLeft) ? 1 : (-1));
		int i = OpIndex + num;
		if (!IsLeft && strFormula[i] == '#')
		{
			string errorOperand = GetErrorOperand(strFormula, i);
			i += errorOperand.Length;
			if (i >= strFormula.Length || IndexOf(strFormula, i, arrBreakStrings) != -1 || IndexOf(array2, strFormula[i]) != -1)
			{
				return errorOperand;
			}
		}
		int length;
		for (length = strFormula.Length; i >= 0 && i < length && IndexOf(PlusMinusArray, strFormula[i].ToString()) != -1; i += num)
		{
		}
		for (; i >= 0 && i < length; i += num)
		{
			if (IndexOf(array, strFormula[i]) != -1)
			{
				i = FindCorrespondingBracket(strFormula, i);
			}
			else if ((IndexOf(array2, strFormula[i]) != -1 && FindCorrespondingBracket(strFormula, i) != -1) || IndexOf(strFormula, i, arrBreakStrings) != -1)
			{
				return IsLeft ? strFormula.Substring(i + 1, OpIndex - i - 1) : strFormula.Substring(OpIndex + 1, i - OpIndex - 1);
			}
		}
		if (!IsLeft)
		{
			return strFormula.Substring(OpIndex + 1);
		}
		return strFormula.Substring(0, OpIndex);
	}

	[CLSCompliant(false)]
	public static void RegisterFunction(string functionName, ExcelFunction index, ReferenceIndexAttribute[] paramIndexes)
	{
		RegisterFunction(functionName, index, paramIndexes, -1);
	}

	[CLSCompliant(false)]
	public static void RegisterFunction(string functionName, ExcelFunction index, ReferenceIndexAttribute[] paramIndexes, int paramCount)
	{
		Dictionary<Type, ReferenceIndexAttribute> dictionary = null;
		if (paramIndexes != null && paramIndexes.Length != 0)
		{
			dictionary = new Dictionary<Type, ReferenceIndexAttribute>();
		}
		for (int i = 0; i < paramIndexes.Length; i++)
		{
			dictionary.Add(paramIndexes[i].TargetType, paramIndexes[i]);
		}
		if (dictionary == null)
		{
			dictionary = new Dictionary<Type, ReferenceIndexAttribute>(1);
			dictionary.Add(typeof(RefPtg), new ReferenceIndexAttribute(2));
		}
		FunctionIdToIndex.Add(index, dictionary);
		FunctionIdToAlias.Add(index, functionName);
		FunctionAliasToId.Add(functionName, index);
		if (paramCount != -1)
		{
			FunctionIdToParamCount.Add(index, paramCount);
		}
	}

	[CLSCompliant(false)]
	public static void RegisterFunction(string functionName, ExcelFunction index, int paramCount)
	{
		RegisterFunction(functionName, index, null, paramCount);
	}

	[CLSCompliant(false)]
	public static void RegisterFunction(string functionName, ExcelFunction index)
	{
		RegisterFunction(functionName, index, -1);
	}

	public static void RaiseFormulaEvaluation(object sender, EvaluateEventArgs e)
	{
		if (FormulaUtil.FormulaEvaluator != null)
		{
			FormulaUtil.FormulaEvaluator(sender, e);
		}
	}

	public static void RegisterTokenClass(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (!type.GetTypeInfo().IsSubclassOf(typeof(Ptg)))
		{
			throw new ArgumentException("class must be derived from Ptg class", "type");
		}
		TokenAttribute[] array = type.GetTypeInfo().GetCustomAttributes<TokenAttribute>(inherit: false).ToArray();
		if (array.Length != 0)
		{
			TokenConstructor tokenConstructor = new TokenConstructor(type);
			Ptg value = tokenConstructor.CreatePtg();
			for (int i = 0; i < array.Length; i++)
			{
				FormulaToken formulaType = array[i].FormulaType;
				TokenCodeToConstructor.Add(array[i].FormulaType, tokenConstructor);
				s_hashTokenCodeToPtg.Add(formulaType, value);
			}
		}
	}

	[CLSCompliant(false)]
	public static void RegisterAdditionalAlias(string aliasName, ExcelFunction functionIndex)
	{
		if (aliasName == null)
		{
			throw new ArgumentNullException("aliasName");
		}
		if (aliasName.Length == 0)
		{
			throw new ArgumentException("aliasName - string cannot be empty");
		}
		if (FunctionAliasToId.ContainsKey(aliasName))
		{
			throw new ArgumentOutOfRangeException("aliasName", "Alias name already exists.");
		}
		FunctionAliasToId.Add(aliasName, functionIndex);
	}

	public static void UpdateNameIndex(Ptg ptg, int iOldIndex, int iNewIndex)
	{
		if (ptg == null)
		{
			throw new ArgumentNullException("ptg");
		}
		if (IndexOf(NameCodes, ptg.TokenCode) != -1)
		{
			NamePtg namePtg = (NamePtg)ptg;
			if (namePtg.ExternNameIndex - 1 == iOldIndex)
			{
				namePtg.ExternNameIndex = (ushort)(iNewIndex + 1);
			}
		}
		else if (IndexOf(NameXCodes, ptg.TokenCode) != -1)
		{
			NameXPtg nameXPtg = (NameXPtg)ptg;
			if (nameXPtg.NameIndex - 1 == iOldIndex)
			{
				nameXPtg.NameIndex = (ushort)(iNewIndex + 1);
			}
		}
	}

	public static int IndexOf(FormulaToken[] array, FormulaToken value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf(string[] array, string value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	[CLSCompliant(false)]
	public static int IndexOf(ExcelFunction[] array, ExcelFunction value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	private static int IndexOf(char[] array, char value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			char c = array[i];
			if (value == c)
			{
				return i;
			}
		}
		return -1;
	}

	private static int IndexOf(string strFormula, int index, string[] arrBreakStrings)
	{
		if (strFormula == null)
		{
			throw new ArgumentNullException("strFormula");
		}
		if (arrBreakStrings == null)
		{
			throw new ArgumentNullException("arrBreakStrings");
		}
		int num = arrBreakStrings.Length;
		if (num == 0)
		{
			return -1;
		}
		char c = strFormula[index];
		int lowerBound = GetLowerBound(arrBreakStrings, c);
		if (lowerBound < 0)
		{
			return -1;
		}
		for (int i = lowerBound; i < num; i++)
		{
			string text = arrBreakStrings[i];
			int length = text.Length;
			if (text == null)
			{
				throw new ArgumentNullException();
			}
			if (length == 0)
			{
				throw new ArgumentException("String can't be empty");
			}
			if (text[0] != c)
			{
				return -1;
			}
			if (length == 1 || string.Compare(strFormula, index + 1, text, 1, length - 1) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	private static int IndexOf(string strFormula, int index, TypedSortedListEx<string, object> arrBreakStrings)
	{
		if (strFormula == null)
		{
			throw new ArgumentNullException("strFormula");
		}
		if (arrBreakStrings == null)
		{
			throw new ArgumentNullException("arrBreakStrings");
		}
		if (arrBreakStrings.Count == 0)
		{
			return -1;
		}
		char c = strFormula[index];
		int upperBound = GetUpperBound(arrBreakStrings, c);
		if (upperBound < 0)
		{
			return -1;
		}
		for (int num = upperBound; num >= 0; num--)
		{
			string key = arrBreakStrings.GetKey(num);
			int length = key.Length;
			if (key == null)
			{
				throw new ArgumentNullException();
			}
			if (length == 0)
			{
				throw new ArgumentException("String can't be empty");
			}
			if (key[0] != c)
			{
				return -1;
			}
			if (length == 1 || string.Compare(strFormula, index + 1, key, 1, length - 1) == 0)
			{
				return num;
			}
		}
		return -1;
	}

	private static int GetLowerBound(string[] arrStringValues, char chFirst)
	{
		if (arrStringValues == null)
		{
			throw new ArgumentNullException("arrStringValues");
		}
		int num = 0;
		int num2 = arrStringValues.Length - 1;
		while (num2 != num)
		{
			int num3 = (num2 + num) / 2;
			char c = (arrStringValues[num3] ?? throw new ArgumentNullException("String in the array can't be null."))[0];
			if (c >= chFirst)
			{
				if (num2 == num3)
				{
					break;
				}
				num2 = num3;
			}
			else if (c < chFirst)
			{
				if (num == num3)
				{
					break;
				}
				num = num3;
			}
		}
		if ((arrStringValues[num] ?? throw new ArgumentNullException())[0] == chFirst)
		{
			return num;
		}
		if ((arrStringValues[num2] ?? throw new ArgumentNullException())[0] == chFirst)
		{
			return num2;
		}
		return -1;
	}

	private static int GetLowerBound(TypedSortedListEx<string, object> arrStringValues, char chFirst)
	{
		if (arrStringValues == null)
		{
			throw new ArgumentNullException("arrStringValues");
		}
		int num = 0;
		int num2 = arrStringValues.Count - 1;
		while (num2 != num)
		{
			int num3 = (num2 + num) / 2;
			char c = (arrStringValues.GetKey(num3) ?? throw new ArgumentNullException("String in the array can't be null."))[0];
			if (c >= chFirst)
			{
				if (num2 == num3)
				{
					break;
				}
				num2 = num3;
			}
			else if (c < chFirst)
			{
				if (num == num3)
				{
					break;
				}
				num = num3;
			}
		}
		if ((arrStringValues.GetKey(num) ?? throw new ArgumentNullException())[0] == chFirst)
		{
			return num;
		}
		if ((arrStringValues.GetKey(num2) ?? throw new ArgumentNullException())[0] == chFirst)
		{
			return num2;
		}
		return -1;
	}

	private static int GetUpperBound(TypedSortedListEx<string, object> arrStringValues, char chFirst)
	{
		if (arrStringValues == null)
		{
			throw new ArgumentNullException("arrStringValues");
		}
		int num = 0;
		int num2 = arrStringValues.Count - 1;
		while (num2 != num)
		{
			int num3 = (num2 + num) / 2;
			char c = (arrStringValues.GetKey(num3) ?? throw new ArgumentNullException("String in the array can't be null."))[0];
			if (c > chFirst)
			{
				if (num2 == num3)
				{
					break;
				}
				num2 = num3;
			}
			else if (c <= chFirst)
			{
				if (num == num3)
				{
					break;
				}
				num = num3;
			}
		}
		if ((arrStringValues.GetKey(num) ?? throw new ArgumentNullException())[0] == chFirst)
		{
			return num;
		}
		if ((arrStringValues.GetKey(num2) ?? throw new ArgumentNullException())[0] == chFirst)
		{
			return num2;
		}
		return -1;
	}

	[CLSCompliant(false)]
	public static Ptg[] ConvertSharedFormulaTokens(SharedFormulaRecord shared, IWorkbook book, int iRow, int iColumn)
	{
		if (shared == null)
		{
			throw new ArgumentNullException("shared");
		}
		Ptg[] formula = shared.Formula;
		int num = formula.Length;
		Ptg[] array = new Ptg[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = formula[i].ConvertSharedToken(book, iRow, iColumn);
		}
		return array;
	}

	public Ptg[] UpdateFormula(Ptg[] arrPtgs, int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect, int iRow, int iColumn)
	{
		int i = 0;
		for (int num = arrPtgs.Length; i < num; i++)
		{
			Ptg ptg = arrPtgs[i];
			arrPtgs[i] = ptg.Offset(iCurIndex, iRow - 1, iColumn - 1, iSourceIndex, sourceRect, iDestIndex, destRect, out var _, m_book);
		}
		return arrPtgs;
	}

	public Ptg[] UpdateFormula(Ptg[] arrPtgs, int iRowDelta, int iColumnDelta)
	{
		int i = 0;
		for (int num = arrPtgs.Length; i < num; i++)
		{
			Ptg ptg = arrPtgs[i];
			arrPtgs[i] = ptg.Offset(iRowDelta, iColumnDelta, m_book);
		}
		return arrPtgs;
	}

	public static void PushOperandToStack(Stack<object> operands, string operand)
	{
		if (operands == null)
		{
			throw new ArgumentNullException("operands");
		}
		if (operand == null)
		{
			throw new ArgumentNullException("operand");
		}
		string item = operand;
		if (operands.Count > 0 && operands.Peek() is AttrPtg attrPtg)
		{
			operands.Pop();
			item = attrPtg.ToString() + operand;
		}
		operands.Push(item);
	}

	private Ptg[] CreateFunction(string strFormula, int bracketIndex, IWorkbook parent, IWorksheet sheet, Dictionary<Type, ReferenceIndexAttribute> indexes, int index, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		List<Ptg> list = new List<Ptg>();
		string text = strFormula.Substring(0, bracketIndex);
		string text2 = text.ToUpper();
		int index2 = GetIndex(typeof(FunctionVarPtg), 1, indexes, index, options);
		if (text2 == "IF")
		{
			return CreateIfFunction(index2, strFormula, bracketIndex, parent, sheet, hashWorksheetNames, options, iCellRow, iCellColumn);
		}
		if (FunctionAliasToId.ContainsKey(text2))
		{
			ExcelFunction excelFunction = FunctionAliasToId[text2];
			excelFunction = FunctionAliasToId[text2];
			OperationPtg operationPtg = null;
			operationPtg = ((!FunctionIdToParamCount.ContainsKey(excelFunction)) ? ((OperationPtg)CreatePtg(FunctionVarPtg.IndexToCode(GetIndex(typeof(FunctionVarPtg), 1, indexes, index, options)), excelFunction)) : ((OperationPtg)CreatePtg(FunctionPtg.IndexToCode(GetIndex(typeof(FunctionPtg), 1, indexes, index, options)), excelFunction)));
			if (IndexOf(SemiVolatileFunctions, excelFunction) != -1)
			{
				list.Add(CreatePtg(FormulaToken.tAttr, 1, 0));
			}
			string[] operands = operationPtg.GetOperands(strFormula, ref bracketIndex, this);
			int num = 0;
			Dictionary<Type, ReferenceIndexAttribute> dictionary = FunctionIdToIndex[excelFunction];
			OfficeParseFormulaOptions officeParseFormulaOptions = options;
			if ((options & OfficeParseFormulaOptions.RootLevel) != 0)
			{
				officeParseFormulaOptions--;
			}
			officeParseFormulaOptions |= OfficeParseFormulaOptions.ParseOperand;
			int i = 0;
			for (int num2 = operands.Length; i < num2; i++)
			{
				string operand = operands[i];
				if (dictionary != null)
				{
					list.AddRange(ParseOperandString(operand, parent, sheet, dictionary, num, hashWorksheetNames, officeParseFormulaOptions, iCellRow, iCellColumn));
				}
				else
				{
					list.AddRange(ParseOperandString(operand, parent, sheet, null, num, hashWorksheetNames, officeParseFormulaOptions, iCellRow, iCellColumn));
				}
				num++;
			}
			list.Add(operationPtg);
			return list.ToArray();
		}
		if (IsCustomFunction(text, parent, out var iBookIndex, out var iNameIndex))
		{
			if (iNameIndex != -1)
			{
				return CreateCustomFunction(index2, strFormula, bracketIndex, iBookIndex, iNameIndex, parent, sheet, hashWorksheetNames, options, iCellRow, iCellColumn);
			}
			return CreateCustomFunction(index2, strFormula, bracketIndex, parent, sheet, hashWorksheetNames, options, iCellRow, iCellColumn);
		}
		throw new ArgumentException("Unknown function name: '" + text2 + "' formula: " + strFormula);
	}

	internal static bool IsCustomFunction(string strFunctionName, IWorkbook book, out int iBookIndex, out int iNameIndex)
	{
		if (strFunctionName == null)
		{
			throw new ArgumentNullException("strFunctionName");
		}
		if (strFunctionName.Length == 0)
		{
			throw new ArgumentException("strFunctionName - string cannot be empty");
		}
		iBookIndex = -1;
		iNameIndex = -1;
		if (book == null)
		{
			return false;
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)book;
		Match match = AddInFunctionRegEx.Match(strFunctionName);
		bool flag = false;
		flag = ((match.Success && match.Value == strFunctionName) ? IsCustomFunction(workbookImpl, match, ref iBookIndex, ref iNameIndex) : (IsLocalCustomFunction(workbookImpl, strFunctionName, ref iNameIndex) || workbookImpl.ExternWorkbooks.ContainsExternName(strFunctionName, ref iBookIndex, ref iNameIndex)));
		if (!flag && !workbookImpl.ThrowOnUnknownNames)
		{
			IName name = workbookImpl.Names[strFunctionName];
			if (name == null)
			{
				name = workbookImpl.Names.Add(strFunctionName);
			}
			iNameIndex = (name as NameImpl).Index;
			flag = true;
		}
		return flag;
	}

	private static bool IsLocalCustomFunction(WorkbookImpl book, string strFunctionName, ref int iNameIndex)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (strFunctionName == null)
		{
			throw new ArgumentNullException("strFunctionName");
		}
		if (strFunctionName.Length == 0)
		{
			throw new ArgumentException("strFunctionName - string cannot be empty");
		}
		if (!(book.InnerNamesColection[strFunctionName] is NameImpl nameImpl))
		{
			return false;
		}
		iNameIndex = nameImpl.Index;
		return nameImpl.IsFunction;
	}

	private static bool IsCustomFunction(WorkbookImpl workbook, Match m, ref int iBookIndex, ref int iNameIndex)
	{
		if (m == null)
		{
			throw new ArgumentNullException("m");
		}
		string value = m.Groups["Path"].Value;
		string text = m.Groups["BookName"].Value;
		string value2 = m.Groups["SheetName"].Value;
		string value3 = m.Groups["RangeName"].Value;
		if (text == null || text.Length == 0)
		{
			text = value2;
			value2 = null;
		}
		int length = text.Length;
		if (text[0] == '[' && text[length - 1] == ']')
		{
			text = text.Substring(1, length - 2);
		}
		ExternWorkbookImpl externWorkbookImpl = null;
		if (value.Length > 0)
		{
			externWorkbookImpl = workbook.ExternWorkbooks[value + text];
			if (externWorkbookImpl == null)
			{
				return false;
			}
		}
		else
		{
			externWorkbookImpl = workbook.ExternWorkbooks.GetBookByShortName(text);
			if (externWorkbookImpl == null)
			{
				return false;
			}
		}
		iBookIndex = externWorkbookImpl.Index;
		iNameIndex = externWorkbookImpl.ExternNames.GetNameIndex(value3);
		return iNameIndex >= 0;
	}

	private Ptg[] CreateIfFunction(int iRefIndex, string strFormula, int bracketIndex, IWorkbook parent, IWorksheet sheet, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		List<Ptg> list = new List<Ptg>();
		ExcelFunction excelFunction = ExcelFunction.IF;
		OperationPtg operationPtg = null;
		operationPtg = (OperationPtg)CreatePtg(FunctionVarPtg.IndexToCode(iRefIndex), excelFunction);
		if (IndexOf(SemiVolatileFunctions, excelFunction) != -1)
		{
			list.Add(CreatePtg(FormulaToken.tAttr, 1, 0));
		}
		string[] operands = operationPtg.GetOperands(strFormula, ref bracketIndex, this);
		if (operands.Length > 3 || operands.Length < 2)
		{
			throw new ArgumentOutOfRangeException("Argument count must be 2 or 3", strFormula);
		}
		int num = 0;
		Dictionary<Type, ReferenceIndexAttribute> dictionary = FunctionIdToIndex[excelFunction];
		List<Ptg[]> list2 = new List<Ptg[]>(4);
		OfficeParseFormulaOptions officeParseFormulaOptions = options;
		if ((options & OfficeParseFormulaOptions.RootLevel) != 0)
		{
			officeParseFormulaOptions--;
		}
		int i = 0;
		for (int num2 = operands.Length; i < num2; i++)
		{
			string operand = operands[i];
			if (dictionary != null)
			{
				list2.Add(ParseOperandString(operand, parent, sheet, dictionary, num, hashWorksheetNames, officeParseFormulaOptions, iCellRow, iCellColumn));
			}
			else
			{
				list2.Add(ParseOperandString(operand, parent, sheet, null, num, hashWorksheetNames, officeParseFormulaOptions, iCellRow, iCellColumn));
			}
			num++;
		}
		list.AddRange(list2[0]);
		int num3 = 0;
		int num4 = 0;
		Ptg[] array = list2[1];
		Ptg[] array2 = null;
		OfficeVersion version = ((WorkbookImpl)parent).Version;
		int j = 0;
		for (int num5 = array.Length; j < num5; j++)
		{
			num3 += array[j].GetSize(version);
		}
		if (operands.Length == 3)
		{
			array2 = list2[2];
			int k = 0;
			for (int num6 = array2.Length; k < num6; k++)
			{
				num4 += array2[k].GetSize(version);
			}
			num4 += 4;
		}
		list.Add(CreatePtg(FormulaToken.tAttr, 2, num3 + 4));
		list.AddRange(array);
		int num7 = (((options & OfficeParseFormulaOptions.InArray) == 0) ? 8 : 0);
		list.Add(CreatePtg(FormulaToken.tAttr, num7, num4 + 3));
		if (array2 != null)
		{
			list.AddRange(array2);
			list.Add(CreatePtg(FormulaToken.tAttr, num7, 3));
		}
		list.Add(operationPtg);
		return list.ToArray();
	}

	private Ptg[] CreateCustomFunction(int iRefIndex, string strFormula, int bracketIndex, IWorkbook parent, IWorksheet sheet, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		string leftUnaryOperand = GetLeftUnaryOperand(strFormula, bracketIndex);
		int iRefIndex2;
		int nameIndexes = ((WorkbookImpl)parent).ExternWorkbooks.GetNameIndexes(leftUnaryOperand, out iRefIndex2);
		return CreateCustomFunction(iRefIndex, strFormula, bracketIndex, nameIndexes, iRefIndex2, parent, sheet, hashWorksheetNames, options, iCellRow, iCellColumn);
	}

	private Ptg[] CreateCustomFunction(int iRefIndex, string strFormula, int bracketIndex, int iBookIndex, int iNameIndex, IWorkbook parent, IWorksheet sheet, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		List<Ptg> list = new List<Ptg>();
		ExcelFunction excelFunction = ExcelFunction.CustomFunction;
		OperationPtg operationPtg = null;
		operationPtg = (OperationPtg)CreatePtg(FunctionVarPtg.IndexToCode(iRefIndex), excelFunction);
		if (IndexOf(SemiVolatileFunctions, excelFunction) != -1)
		{
			list.Add(CreatePtg(FormulaToken.tAttr, 1, 0));
		}
		_ = (WorkbookImpl)parent;
		string[] operands = operationPtg.GetOperands(strFormula, ref bracketIndex, this);
		int num = 0;
		Dictionary<Type, ReferenceIndexAttribute> dictionary = FunctionIdToIndex[excelFunction];
		OfficeParseFormulaOptions officeParseFormulaOptions = options;
		if ((options & OfficeParseFormulaOptions.RootLevel) != 0)
		{
			officeParseFormulaOptions--;
		}
		Ptg ptg = ((iBookIndex != -1) ? CreatePtg(FormulaToken.tNameX1, iBookIndex, iNameIndex) : (ptg = CreatePtg(FormulaToken.tName1, iNameIndex)));
		list.Add(ptg);
		int i = 0;
		for (int num2 = operands.Length; i < num2; i++)
		{
			string operand = operands[i];
			if (dictionary != null)
			{
				list.AddRange(ParseOperandString(operand, parent, sheet, dictionary, num, hashWorksheetNames, officeParseFormulaOptions, iCellRow, iCellColumn));
			}
			else
			{
				list.AddRange(ParseOperandString(operand, parent, sheet, null, num, hashWorksheetNames, officeParseFormulaOptions, iCellRow, iCellColumn));
			}
			num++;
		}
		list.Add(operationPtg);
		return list.ToArray();
	}

	public static Ptg CreateError(string strFormula, int errorIndex)
	{
		string errorOperand = GetErrorOperand(strFormula, errorIndex);
		return (Ptg)ErrorNameToConstructor[errorOperand].Invoke(new object[1] { strFormula });
	}

	private Ptg[] ParseOperandString(string operand, IWorkbook parent, IWorksheet sheet, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, Dictionary<string, string> hashWorksheetNames, OfficeParseFormulaOptions options, int iCellRow, int iCellColumn)
	{
		if (operand.Length == 0)
		{
			return new Ptg[1] { CreatePtg(FormulaToken.tMissingArgument, operand) };
		}
		try
		{
			options |= OfficeParseFormulaOptions.ParseOperand;
			return ParseString(operand, sheet, indexes, i, hashWorksheetNames, options, iCellRow, iCellColumn);
		}
		catch (Exception)
		{
			throw;
		}
	}

	private static OperationPtg CreateUnaryOperation(char OperationSymbol)
	{
		string text = OperationSymbol.ToString();
		if (OperationSymbol == '(')
		{
			return (OperationPtg)CreatePtg(FormulaToken.tParentheses, text);
		}
		return (OperationPtg)CreatePtg(UnaryOperationPtg.GetTokenId(text), text);
	}

	private OperationPtg CreateOperation(string strFormula)
	{
		return (OperationPtg)CreatePtg((strFormula == m_strOperandsSeparator) ? FormulaToken.tCellRangeList : BinaryOperationPtg.GetTokenId(strFormula), strFormula);
	}

	public static bool IsCell(string strFormula, bool bR1C1, out string strRow, out string strColumn)
	{
		Match match = (bR1C1 ? CellR1C1Regex : CellRegex).Match(strFormula);
		int num;
		if (match.Success)
		{
			num = ((match.Value == strFormula) ? 1 : 0);
			if (num != 0)
			{
				strRow = match.Groups["Row1"].Value;
				strColumn = match.Groups["Column1"].Value;
				return (byte)num != 0;
			}
		}
		else
		{
			num = 0;
		}
		strRow = null;
		strColumn = null;
		return (byte)num != 0;
	}

	internal static bool IsR1C1(string strFormula)
	{
		Match match = CellR1C1Regex.Match(strFormula);
		if (match.Success)
		{
			return match.Value == strFormula;
		}
		return false;
	}

	public bool IsCellRange(string strFormula, bool bR1C1, out string strRow1, out string strColumn1, out string strRow2, out string strColumn2)
	{
		strColumn1 = null;
		strColumn2 = null;
		strRow1 = null;
		strRow2 = null;
		Match match = (bR1C1 ? CellRangeR1C1Regex : CellRangeRegex).Match(strFormula);
		bool flag = IsSuccess(match, strFormula);
		if (flag)
		{
			strColumn1 = match.Groups["Column1"].Value;
			strColumn2 = match.Groups["Column2"].Value;
			strRow1 = match.Groups["Row1"].Value;
			strRow2 = match.Groups["Row2"].Value;
		}
		else if (bR1C1)
		{
			match = CellRangeR1C1ShortRegex.Match(strFormula);
			flag = IsSuccess(match, strFormula);
			if (flag)
			{
				if (strFormula[0].ToString() == "R")
				{
					strRow2 = (strRow1 = strFormula);
				}
				else
				{
					strColumn1 = (strColumn2 = strFormula);
				}
			}
			else
			{
				match = FullRowRangeR1C1Regex.Match(strFormula);
				flag = IsSuccess(match, strFormula);
				if (flag)
				{
					strColumn1 = "C1";
					strColumn2 = "C" + m_book.MaxColumnCount;
					strRow1 = match.Groups["Row1"].Value;
					strRow2 = match.Groups["Row2"].Value;
				}
				else
				{
					match = FullColumnRangeR1C1Regex.Match(strFormula);
					flag = IsSuccess(match, strFormula);
					if (flag)
					{
						strColumn1 = match.Groups["Column1"].Value;
						strColumn2 = match.Groups["Column2"].Value;
						strRow1 = "R1";
						strRow2 = "R" + m_book.MaxRowCount;
					}
				}
			}
		}
		else
		{
			match = FullRowRangeRegex.Match(strFormula);
			flag = IsSuccess(match, strFormula);
			if (flag)
			{
				strColumn1 = "$A";
				strColumn2 = "$" + RangeImpl.GetColumnName(m_book.MaxColumnCount);
				strRow1 = match.Groups["Row1"].Value;
				strRow2 = match.Groups["Row2"].Value;
			}
			else
			{
				match = FullColumnRangeRegex.Match(strFormula);
				flag = IsSuccess(match, strFormula);
				if (flag)
				{
					strColumn1 = match.Groups["Column1"].Value;
					strColumn2 = match.Groups["Column2"].Value;
					strRow1 = "$1";
					strRow2 = "$" + m_book.MaxRowCount;
				}
			}
		}
		return flag;
	}

	private static bool IsSuccess(Match m, string strFormula)
	{
		if (m.Success && m.Index == 0)
		{
			return m.Length == strFormula.Length;
		}
		return false;
	}

	public static bool IsCell3D(string strFormula, bool bR1C1, out string strSheetName, out string strRow, out string strColumn)
	{
		Match match = (bR1C1 ? CellR1C13DRegex : Cell3DRegex).Match(strFormula);
		if (match.Success && match.Index == 0 && match.Length == strFormula.Length)
		{
			strSheetName = match.Groups["SheetName"].Value;
			strSheetName = NormalizeSheetName(strSheetName);
			strRow = match.Groups["Row1"].Value;
			strColumn = match.Groups["Column1"].Value;
			return true;
		}
		strSheetName = null;
		strRow = null;
		strColumn = null;
		return false;
	}

	public bool IsCellRange3D(string strFormula, bool bR1C1, out string strSheetName, out string strRow1, out string strColumn1, out string strRow2, out string strColumn2)
	{
		strSheetName = null;
		strRow1 = null;
		strColumn1 = null;
		strRow2 = null;
		strColumn2 = null;
		Match match = (bR1C1 ? CellRangeR1C13DRegex : CellRange3DRegex).Match(strFormula);
		bool flag = match.Success && match.Index == 0 && match.Length == strFormula.Length;
		if (!flag)
		{
			match = (bR1C1 ? CellRangeR1C13DRegex2 : CellRange3DRegex2).Match(strFormula);
			flag = match.Success && match.Index == 0 && match.Length == strFormula.Length;
		}
		if (!flag)
		{
			match = Full3DRowRangeRegex.Match(strFormula);
			flag = IsSuccess(match, strFormula);
			if (flag)
			{
				strSheetName = match.Groups["SheetName"].Value;
				strColumn1 = "$A";
				strColumn2 = "$" + RangeImpl.GetColumnName(m_book.MaxColumnCount);
				strRow1 = match.Groups["Row1"].Value;
				strRow2 = match.Groups["Row2"].Value;
				strSheetName = NormalizeSheetName(strSheetName);
				return flag;
			}
			match = Full3DColumnRangeRegex.Match(strFormula);
			flag = IsSuccess(match, strFormula);
			if (flag)
			{
				strSheetName = match.Groups["SheetName"].Value;
				strColumn1 = match.Groups["Column1"].Value;
				strColumn2 = match.Groups["Column2"].Value;
				strRow1 = "$1";
				strRow2 = "$" + m_book.MaxRowCount;
				strSheetName = NormalizeSheetName(strSheetName);
				return flag;
			}
		}
		if (!flag && bR1C1)
		{
			match = CellRangeR1C13DShortRegex.Match(strFormula);
			flag = IsSuccess(match, strFormula);
			if (flag)
			{
				strSheetName = match.Groups["SheetName"].Value;
				if (strFormula[strSheetName.Length + 1] == 'R')
				{
					strRow2 = (strRow1 = strFormula.Substring(strSheetName.Length + 1));
				}
				else
				{
					strColumn1 = (strColumn2 = strFormula.Substring(strSheetName.Length + 1));
				}
			}
		}
		else if (flag)
		{
			strSheetName = match.Groups["SheetName"].Value;
			strRow1 = match.Groups["Row1"].Value;
			strColumn1 = match.Groups["Column1"].Value;
			strRow2 = match.Groups["Row2"].Value;
			strColumn2 = match.Groups["Column2"].Value;
		}
		strSheetName = NormalizeSheetName(strSheetName);
		return flag;
	}

	private static bool IsErrorString(string strFormula, int errorIndex)
	{
		foreach (string key in ErrorNameToConstructor.Keys)
		{
			if (string.Compare(strFormula, errorIndex, key, 0, key.Length) == 0)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsNamedRange(string strFormula, IWorkbook parent, IWorksheet sheet)
	{
		bool flag = false;
		if (sheet != null)
		{
			flag = sheet.Names.Contains(strFormula);
		}
		if (!flag && parent != null)
		{
			flag = parent.Names.Contains(strFormula);
		}
		return flag;
	}

	private static int FindCorrespondingBracket(string strFormula, int BracketPos, char[] StartBrackets, int delta)
	{
		bool flag = false;
		int num = IndexOf(OpenBrackets, strFormula[BracketPos]);
		char c;
		if (num != -1)
		{
			c = CloseBrackets[num];
		}
		else
		{
			num = IndexOf(CloseBrackets, strFormula[BracketPos]);
			if (num == -1)
			{
				throw new ArgumentOutOfRangeException("Specified position is not a position of bracket");
			}
			c = OpenBrackets[num];
		}
		if (IndexOf(StringBrackets, c) != -1)
		{
			flag = true;
		}
		int i = BracketPos + delta;
		for (int length = strFormula.Length; i < length && i >= 0; i += delta)
		{
			if (strFormula[i] == c)
			{
				return i;
			}
			if (!flag && IndexOf(StartBrackets, strFormula[i]) != -1)
			{
				i = FindCorrespondingBracket(strFormula, i, StartBrackets, delta);
			}
		}
		throw new ArgumentException("Expression is invalid. Can't find corresponding bracket");
	}

	private static bool IsUnaryOperation(string strFormula, int OpIndex)
	{
		return IndexOf(strFormula, OpIndex, UnaryOperations) != -1;
	}

	private bool IsOperation(string strFormula, int index, out int iOperationIndex)
	{
		iOperationIndex = IndexOf(strFormula, index, m_arrAllOperations);
		return iOperationIndex != -1;
	}

	private static bool IsFunction(string strOperand, out int iBracketPos)
	{
		iBracketPos = -1;
		if (IndexOf(StringBrackets, strOperand[0]) != -1)
		{
			return false;
		}
		iBracketPos = strOperand.IndexOf('(');
		if (iBracketPos != -1)
		{
			return FindCorrespondingBracket(strOperand, iBracketPos) == strOperand.Length - 1;
		}
		return false;
	}

	private static string GetErrorOperand(string strFormula, int errorIndex)
	{
		if (strFormula[errorIndex] != '#')
		{
			throw new ArgumentException("Not error string");
		}
		foreach (string key in ErrorNameToConstructor.Keys)
		{
			if (string.Compare(strFormula, errorIndex, key, 0, key.Length) == 0)
			{
				return key;
			}
		}
		throw new ArgumentException("Error name was not found");
	}

	private static int GetExpectedIndex(Type targetType, Dictionary<Type, ReferenceIndexAttribute> indexes, int i)
	{
		if (indexes == null)
		{
			return 2;
		}
		if (indexes.TryGetValue(targetType, out var value))
		{
			return value[i];
		}
		if (value == null && targetType != typeof(RefPtg))
		{
			return GetExpectedIndex(typeof(RefPtg), indexes, i);
		}
		return 2;
	}

	public static int GetIndex(Type targetType, int valueType, Dictionary<Type, ReferenceIndexAttribute> indexes, int i, OfficeParseFormulaOptions options)
	{
		bool flag = (options & OfficeParseFormulaOptions.InName) != 0;
		if (flag)
		{
			return 1;
		}
		if (indexes == null)
		{
			return 2;
		}
		int num = GetExpectedIndex(targetType, indexes, i) - 1;
		bool flag2 = (options & OfficeParseFormulaOptions.RootLevel) != 0;
		bool flag3 = (options & OfficeParseFormulaOptions.InArray) != 0;
		bool flag4 = (options & OfficeParseFormulaOptions.ParseComplexOperand) != 0;
		int num2 = (flag2 ? 3 : num);
		int num3 = (flag ? 2 : (flag3 ? 2 : 0));
		int num4 = DEF_INDEXES_CONVERTION[num2][valueType][num3];
		if (num4 == 1 && flag4 && !flag2)
		{
			num4 = 2;
		}
		return num4;
	}

	public static Ptg CreatePtg(DataProvider provider, ref int offset, OfficeVersion version)
	{
		FormulaToken formulaToken = (FormulaToken)provider.ReadByte(offset);
		if (!s_hashTokenCodeToPtg.TryGetValue(formulaToken, out var value))
		{
			throw new ArgumentException("Cannot find Formula token with code: " + formulaToken);
		}
		value = (Ptg)value.Clone();
		value.TokenCode = formulaToken;
		offset++;
		value.InfillPTG(provider, ref offset, version);
		return value;
	}

	public static Ptg CreatePtg(FormulaToken token)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg();
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtgByType(FormulaToken token)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(token);
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtg(FormulaToken token, string tokenString)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(tokenString);
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtg(FormulaToken token, string tokenString, IWorkbook parent)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(tokenString, parent);
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtg(FormulaToken token, params object[] arrParams)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(arrParams);
		ptg.TokenCode = token;
		return ptg;
	}

	[CLSCompliant(false)]
	public static Ptg CreatePtg(FormulaToken token, ushort iParam1, ushort iParam2)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(iParam1, iParam2);
		ptg.TokenCode = token;
		return ptg;
	}

	[CLSCompliant(false)]
	public static Ptg CreatePtg(FormulaToken token, ExcelFunction functionIndex)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(functionIndex);
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtg(FormulaToken token, int iCellRow, int iCellColumn, string strParam1, string strParam2, bool bR1C1)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(iCellRow, iCellColumn, strParam1, strParam2, bR1C1);
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtg(FormulaToken token, int iCellRow, int iCellColumn, string strParam1, string strParam2, string strParam3, string strParam4, bool bR1C1, IWorkbook book)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(iCellRow, iCellColumn, strParam1, strParam2, strParam3, strParam4, bR1C1, book);
		ptg.TokenCode = token;
		return ptg;
	}

	public static Ptg CreatePtg(FormulaToken token, int iCellRow, int iCellColumn, int iRefIndex, string strParam1, string strParam2, string strParam3, string strParam4, bool bR1C1, IWorkbook book)
	{
		Ptg ptg = TokenCodeToConstructor[token].CreatePtg(iCellRow, iCellColumn, iRefIndex, strParam1, strParam2, strParam3, strParam4, bR1C1, book);
		ptg.TokenCode = token;
		return ptg;
	}

	private static Ptg[] SkipUnnecessaryTokens(Ptg[] ptgs)
	{
		List<Ptg> list = new List<Ptg>();
		foreach (Ptg ptg in ptgs)
		{
			if (ptg is AttrPtg)
			{
				AttrPtg attrPtg = ptg as AttrPtg;
				if (attrPtg.HasOptGoto || attrPtg.HasOptimizedIf || attrPtg.HasSemiVolatile || attrPtg.HasOptimizedChoose)
				{
					continue;
				}
			}
			if (!(ptg is MemFuncPtg))
			{
				if (ptg is MemAreaPtg)
				{
					MemAreaPtg memAreaPtg = ptg as MemAreaPtg;
					list.AddRange(memAreaPtg.Subexpression);
				}
				list.Add(ptg);
			}
		}
		return list.ToArray();
	}

	private string PutUnaryOperationsAhead(string strFormula)
	{
		if (strFormula == null)
		{
			throw new ArgumentNullException("strFormula");
		}
		int num = strFormula.Length;
		while (strFormula[strFormula.Length - 1] == '%')
		{
			string leftUnaryOperand = GetLeftUnaryOperand(strFormula, strFormula.Length - 1);
			int startIndex = strFormula.Length - leftUnaryOperand.Length - 1;
			strFormula = strFormula.Insert(startIndex, "%");
			strFormula = strFormula.Substring(0, strFormula.Length - 1);
			num--;
			if (num == 0)
			{
				throw new ArgumentException("strFormula");
			}
		}
		return strFormula;
	}

	[CLSCompliant(false)]
	public static void EditRegisteredFunction(string functionName, ExcelFunction index, ReferenceIndexAttribute[] paramIndexes, int paramCount)
	{
		FunctionAliasToId.Remove(functionName);
		FunctionIdToIndex.Remove(index);
		FunctionIdToAlias.Remove(index);
		FunctionIdToParamCount.Remove(index);
		RegisterFunction(functionName, index, paramIndexes, paramCount);
	}

	public static bool IsExcel2013Function(ExcelFunction functionIndex)
	{
		return Array.IndexOf(m_excel2013Supported, functionIndex) >= 0;
	}

	public static bool IsExcel2010Function(ExcelFunction functionIndex)
	{
		return Array.IndexOf(m_excel2010Supported, functionIndex) >= 0;
	}

	public static bool IsExcel2007Function(ExcelFunction functionIndex)
	{
		return Array.IndexOf(m_excel2007Supported, functionIndex) >= 0;
	}

	internal bool HasExternalReference(Ptg[] ptg)
	{
		if (ptg == null)
		{
			return false;
		}
		bool result = false;
		int i = 0;
		for (int num = ptg.Length; i < num; i++)
		{
			if (ptg[i] is IReference reference && m_book.IsExternalReference(reference.RefIndex))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
