using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.FormatParser;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class FormatsCollection : CommonObject
{
	public const string DecimalSeparator = ".";

	public const string ThousandSeparator = ",";

	public const string Percentage = "%";

	public const string Fraction = "/";

	public const string Date = "date";

	public const string Time = ":";

	public const string Exponent = "E";

	public const string Minus = "-";

	public const string Currency = "$";

	public const string DEFAULT_EXPONENTAIL = "E+";

	private string[] AdditionalDateFormats = new string[1] { "[$-409]m/d/yy h:mm AM/PM;@" };

	internal const int DEF_FIRST_CUSTOM_INDEX = 163;

	internal string[] DEF_FORMAT_STRING = new string[36]
	{
		"General", "0", "0.00", "#,##0", "#,##0.00", "\"$\"#,##0_);\\( \"$\"#,##0\\ )", "\"$\"#,##0_);[Red]\\( \"$\"#,##0\\ )", "\"$\"#,##0.00_);\\( \"$\"#,##0.00\\ )", "\"$\"#,##0.00_);[Red]\\( \"$\"#,##0.00\\ )", "0%",
		"0.00%", "0.00E+00", "# ?/?", "# ??/??", "m/d/yyyy", "d\\-mmm\\-yy", "d\\-mmm", "mmm\\-yy", "h:mm\\ AM/PM", "h:mm:ss\\ AM/PM",
		"h:mm", "h:mm:ss", "m/d/yy\\ h:mm", "_( #,##0_);\\( #,##0\\ )", "_( #,##0_);[Red]\\( #,##0\\ )", "_( #,##0.00_);\\( #,##0.00\\ )", "_( #,##0.00_);[Red]\\( #,##0.00\\ )", "_(* #,##0_);_(* \\( #,##0\\ );_(* \"-\"_);_( @_ )", "_(\"$\"* #,##0_);_(\"$\"* \\( #,##0\\ );_(\"$\"* \"-\"_);_( @_ )", "_(* #,##0.00_);_(* \\(#,##0.00\\);_(* \"-\"??_);_(@_)",
		"_(\"$\"* #,##0.00_);_(\"$\"* \\( #,##0.00\\ );_(\"$\"* \"-\"??_);_( @_ )", "mm:ss", "[h]:mm:ss", "mm:ss.0", "##0.0E+0", "@"
	};

	private string[] DEF_CURRENCY_FORMAT_STRING = new string[10] { "\"$\"#,##0.00", "#,##0.00\\ [$֏-42B]", "[$₸-43F]#,##0.00", "[$£-809]#,##0.00", "[$¥-411]#,##0.00", "#,##0.00\\ [$₽-419]", "#,##0.00\\ [$₭-454]", "[$₦-466]\\ #,##0.00", "#,##0.00\\ [$€-484]", "[$₹-4009]\\ #,##0.00" };

	private string[] DEF_CURRENCY_SYMBOL = new string[10] { "$", "֏", "₸", "£", "¥", "₱", "₭", "₦", "€", "₹" };

	private const int CountryJapan = 81;

	private TypedSortedListEx<int, FormatImpl> m_rawFormats = new TypedSortedListEx<int, FormatImpl>();

	private Dictionary<string, FormatImpl> m_hashFormatStrings = new Dictionary<string, FormatImpl>();

	private FormatParserImpl m_parser;

	private Dictionary<string, int[]> m_formatIndexes = new Dictionary<string, int[]>();

	private bool m_hasNumFormats;

	private Dictionary<string, string> m_currencyFormatStrings;

	[CLSCompliant(false)]
	public FormatImpl this[int iIndex] => m_rawFormats[iIndex];

	internal bool HasNumberFormats
	{
		get
		{
			return m_hasNumFormats;
		}
		set
		{
			m_hasNumFormats = value;
		}
	}

	[CLSCompliant(false)]
	public FormatImpl this[string strFormat] => m_hashFormatStrings[strFormat];

	public FormatParserImpl Parser
	{
		get
		{
			if (m_parser == null)
			{
				m_parser = new FormatParserImpl(base.Application, this);
			}
			return m_parser;
		}
	}

	internal Dictionary<string, string> CurrencyFormatStrings
	{
		get
		{
			if (m_currencyFormatStrings == null)
			{
				m_currencyFormatStrings = new Dictionary<string, string>();
				int i = 0;
				for (int num = DEF_CURRENCY_SYMBOL.Length; i < num; i++)
				{
					m_currencyFormatStrings[DEF_CURRENCY_SYMBOL[i]] = DEF_CURRENCY_FORMAT_STRING[i];
				}
			}
			return m_currencyFormatStrings;
		}
	}

	public bool IsReadOnly => false;

	public ICollection Values
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public ICollection Keys
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsFixedSize => m_rawFormats.IsFixedSize;

	public bool IsSynchronized => m_rawFormats.IsSynchronized;

	public int Count => m_rawFormats.Count;

	public object SyncRoot
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public FormatsCollection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	public int Parse(IList data, int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int count = data.Count;
		if (iPos < 0 || iPos >= count)
		{
			throw new ArgumentOutOfRangeException("iPos");
		}
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.AddRange(GetUsedFormats(OfficeVersion.Excel97to2003));
	}

	[CLSCompliant(false)]
	public void Add(FormatRecord format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		FormatImpl format2 = new FormatImpl(base.Application, this, format);
		_ = format.Index;
		Register(format2);
	}

	internal void Add(int formatId, string formatString)
	{
		if (formatString == null)
		{
			throw new ArgumentOutOfRangeException("formatString");
		}
		if (formatId < 0)
		{
			throw new ArgumentOutOfRangeException("formatId");
		}
		if (formatString.Length != 0)
		{
			FormatImpl format = new FormatImpl(base.Application, this, formatId, formatString);
			Register(format);
		}
	}

	private void Register(FormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		m_rawFormats[format.Index] = format;
		m_hashFormatStrings[format.FormatString] = format;
	}

	public FormatsCollection Clone(object parent)
	{
		FormatsCollection formatsCollection = (FormatsCollection)MemberwiseClone();
		formatsCollection.SetParent(parent);
		formatsCollection.m_parser = null;
		formatsCollection.m_rawFormats = new TypedSortedListEx<int, FormatImpl>();
		formatsCollection.m_hashFormatStrings = new Dictionary<string, FormatImpl>();
		foreach (KeyValuePair<int, FormatImpl> rawFormat in m_rawFormats)
		{
			FormatImpl value = rawFormat.Value;
			value = (FormatImpl)value.Clone(formatsCollection);
			formatsCollection.m_rawFormats.Add(rawFormat.Key, value);
			formatsCollection.m_hashFormatStrings.Add(value.FormatString, value);
		}
		return formatsCollection;
	}

	public int CreateFormat(string formatString)
	{
		if (formatString == null)
		{
			throw new ArgumentNullException("formatString");
		}
		if (formatString.Length == 0)
		{
			throw new ArgumentException("formatString - string cannot be empty");
		}
		if (formatString.Contains("E+".ToLower()))
		{
			formatString = formatString.Replace("E+".ToLower(), "E+");
		}
		if (formatString.Contains("E+".ToLower()))
		{
			formatString = formatString.Replace("E+".ToLower(), "E+");
		}
		formatString = GetCustomizedString(formatString);
		if (ContainsFormat(formatString))
		{
			return m_hashFormatStrings[formatString].Index;
		}
		int count = m_rawFormats.Count;
		int num = m_rawFormats.GetKey(count - 1);
		if (num < 163)
		{
			num = 163;
		}
		num++;
		FormatRecord formatRecord = (FormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Format);
		formatRecord.FormatString = formatString;
		formatRecord.Index = num;
		Add(formatRecord);
		return num;
	}

	private string GetCustomizedString(string formatString)
	{
		string text = formatString.ToLower();
		if ((text.Contains("d") && text.Contains("m") && text.Contains("y")) || text.Contains("h"))
		{
			text = text.Replace("am", "AM");
			text = text.Replace("pm", "PM");
			if (!text.Contains("\\"))
			{
				string[] array = new string[4] { ",", " ", ".", "-" };
				if (text.Contains("h") || text.Contains("s"))
				{
					return formatString;
				}
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (text.Contains(text2))
					{
						string newValue = text2.PadLeft(2, '\\');
						text = text.Replace(text2, newValue);
					}
				}
			}
			return text;
		}
		return formatString;
	}

	public bool ContainsFormat(string formatString)
	{
		if (formatString == null || formatString.Length == 0)
		{
			return false;
		}
		return m_hashFormatStrings.ContainsKey(formatString);
	}

	public int FindOrCreateFormat(string formatString)
	{
		if (CultureInfo.CurrentCulture.Name != "en-US" && m_hashFormatStrings.ContainsKey(formatString))
		{
			return CreateFormat(formatString);
		}
		if (!m_hashFormatStrings.TryGetValue(formatString, out var value))
		{
			return CreateFormat(formatString);
		}
		return value.Index;
	}

	public void InsertDefaultFormats()
	{
		FormatRecord formatRecord = (FormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Format);
		int num = 0;
		int i = 0;
		for (int num2 = DEF_FORMAT_STRING.Length; i < num2; i++)
		{
			formatRecord.Index = num;
			formatRecord.FormatString = DEF_FORMAT_STRING[i];
			if (!m_rawFormats.Contains(formatRecord.Index))
			{
				Add((FormatRecord)formatRecord.Clone());
			}
			if (num == 22)
			{
				num = 36;
			}
			num++;
		}
	}

	public List<FormatRecord> GetUsedFormats(OfficeVersion version)
	{
		List<FormatRecord> list = new List<FormatRecord>();
		if (version == OfficeVersion.Excel97to2003)
		{
			list.Add(this[5].Record);
			list.Add(this[6].Record);
			list.Add(this[7].Record);
			list.Add(this[8].Record);
			list.Add(this[42].Record);
			list.Add(this[41].Record);
			list.Add(this[44].Record);
			list.Add(this[43].Record);
		}
		else
		{
			if (this[5].FormatString != DEF_FORMAT_STRING[5])
			{
				list.Add(this[5].Record);
			}
			if (this[6].FormatString != DEF_FORMAT_STRING[6])
			{
				list.Add(this[6].Record);
			}
			if (this[7].FormatString != DEF_FORMAT_STRING[7])
			{
				list.Add(this[7].Record);
			}
			if (this[8].FormatString != DEF_FORMAT_STRING[8])
			{
				list.Add(this[8].Record);
			}
			if (this[42].FormatString != DEF_FORMAT_STRING[28])
			{
				list.Add(this[42].Record);
			}
			if (this[41].FormatString != DEF_FORMAT_STRING[27])
			{
				list.Add(this[41].Record);
			}
			if (this[44].FormatString != DEF_FORMAT_STRING[30])
			{
				list.Add(this[44].Record);
			}
			if (this[43].FormatString != DEF_FORMAT_STRING[29])
			{
				list.Add(this[43].Record);
			}
		}
		int num = m_rawFormats.IndexOfKey(49);
		int count = m_rawFormats.Count;
		if (num >= 0 && num < count - 1)
		{
			for (int i = num + 1; i < count; i++)
			{
				FormatImpl byIndex = m_rawFormats.GetByIndex(i);
				if (byIndex.Index >= 163 || HasNumberFormats)
				{
					list.Add(byIndex.Record);
				}
			}
		}
		return list;
	}

	public Dictionary<int, int> Merge(FormatsCollection source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		TypedSortedListEx<int, FormatImpl> rawFormats = source.m_rawFormats;
		int i = 0;
		for (int count = source.Count; i < count; i++)
		{
			FormatImpl byIndex = rawFormats.GetByIndex(i);
			int index = byIndex.Index;
			int value = AddCopy(byIndex);
			dictionary.Add(index, value);
		}
		return dictionary;
	}

	public Dictionary<int, int> AddRange(IDictionary dicIndexes, FormatsCollection source)
	{
		if (dicIndexes == null)
		{
			throw new ArgumentNullException("dicIndexes");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int key in dicIndexes.Keys)
		{
			FormatImpl format = source[key];
			int value = AddCopy(format);
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	private int AddCopy(FormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		return AddCopy(format.Record);
	}

	private int AddCopy(FormatRecord format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		string formatString = format.FormatString;
		return CreateFormat(formatString);
	}

	private void AddJapaneseFormats()
	{
		Add(27, "[$-411]ge.m.d");
		Add(28, "[$-411]ggge\\\"年\\\"m\\\"月\\\"d\\\"日\\\"");
		Add(29, "[$-411]ggge\\\"年\\\"m\\\"月\\\"d\\\"日\\\"");
		Add(30, "m/d/yy");
		Add(31, "yyyy\\\"年\\\"m\\\"月\\\"d\\\"日\\\"");
		Add(32, "h\\\"時\\\"mm\\\"分\\\"");
		Add(33, "h\\\"時\\\"mm\\\"分\\\"ss\\\"秒\\\"");
		Add(34, "yyyy\\\"年\\\"m\\\"月\\\"");
		Add(35, "m\\\"月\\\"d\\\"日\\\"");
		Add(36, "[$-411]ge.m.d");
		Add(50, "[$-411]ge.m.d");
		Add(51, "[$-411]ggge\\\"年\\\"m\\\"月\\\"d\\\"日\\\"");
		Add(52, "yyyy\\\"年\\\"m\\\"月\\\"");
		Add(53, "m\\\"月\\\"d\\\"日\\\"");
		Add(54, "[$-411]ggge\\\"年\\\"m\\\"月\\\"d\\\"日\\\"");
		Add(55, "yyyy\\\"年\\\"m\\\"月\\\"");
		Add(56, "m\" 月\"d\" 日\"");
		Add(57, "[$-411]ge.m.d yyyy\\\"年\\\"");
		Add(58, "[$-411]ggge\\\"年\\\"m\\\"月\\\"d\\\"日\\\"");
	}

	internal void AddDefaultFormats(int country)
	{
		if (country == 81)
		{
			AddJapaneseFormats();
		}
	}

	internal void FillFormatIndexes()
	{
		if (m_formatIndexes.Count <= 0)
		{
			List<int> list = new List<int>();
			list.AddRange(new int[5] { 14, 15, 16, 17, 22 });
			string[] additionalDateFormats = AdditionalDateFormats;
			foreach (string formatString in additionalDateFormats)
			{
				list.Add(CreateFormat(formatString));
			}
			m_formatIndexes.Add("%", new int[2] { 9, 10 });
			m_formatIndexes.Add(",", new int[6] { 3, 4, 5, 6, 7, 8 });
			m_formatIndexes.Add(".", new int[2] { 2, 7 });
			m_formatIndexes.Add("date", list.ToArray());
			m_formatIndexes.Add(":", new int[4] { 18, 19, 20, 21 });
			m_formatIndexes.Add("/", new int[2] { 12, 13 });
			m_formatIndexes.Add("E", new int[1] { 11 });
		}
	}

	internal string GetDateFormat(string strValue)
	{
		string[] array = new string[4] { "-", "/", ",", " " };
		string text = null;
		bool flag = false;
		int num = strValue.IndexOf(":"[0]);
		if (num != -1)
		{
			flag = true;
			if (num <= 2)
			{
				return GetTimeFormat(strValue);
			}
		}
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (strValue.IndexOf(text2) != -1)
			{
				text = text2;
				break;
			}
		}
		int[] array3 = m_formatIndexes["date"];
		string text3 = strValue;
		if (flag)
		{
			int num2 = strValue.IndexOf("AM");
			if (num2 != -1)
			{
				text3 = strValue.Remove(num2, 2);
				text3 = text3.Remove(text3.Length - 1);
			}
			num2 = strValue.IndexOf("PM");
			if (num2 != -1)
			{
				text3 = strValue.Remove(num2, 2);
				text3 = text3.Remove(text3.Length - 1);
			}
		}
		string text4 = null;
		string[] array4 = text3.Split(text[0]);
		switch (array4.Length)
		{
		case 3:
			if (flag)
			{
				if (IsStandardTimeFormat(strValue))
				{
					return m_rawFormats[array3[5]].FormatString;
				}
				return m_rawFormats[array3[4]].FormatString;
			}
			if (array4[1].Length > 1 && char.IsLetter(array4[1], 1))
			{
				return m_rawFormats[array3[1]].FormatString;
			}
			return m_rawFormats[array3[0]].FormatString;
		case 2:
			if (char.IsLetter(array4[1], 1))
			{
				return m_rawFormats[array3[2]].FormatString;
			}
			return m_rawFormats[array3[3]].FormatString;
		default:
			return GetTimeFormat(strValue);
		}
	}

	private string GetTimeFormat(string strValue)
	{
		bool flag = false;
		bool flag2 = false;
		flag = IsStandardTimeFormat(strValue);
		flag2 = HasSecond(strValue);
		int[] array = m_formatIndexes[":"];
		string result = m_rawFormats[array[0]].FormatString;
		int[] array2 = array;
		foreach (int key in array2)
		{
			string formatString = m_rawFormats[key].FormatString;
			if (IsStandardTimeFormat(formatString) == flag && HasSecond(formatString) == flag2)
			{
				result = formatString;
			}
		}
		return result;
	}

	private bool HasSecond(string strValue)
	{
		return strValue.Split(":"[0]).Length > 2;
	}

	private bool IsStandardTimeFormat(string strValue)
	{
		if (strValue.IndexOf("AM") == -1)
		{
			return strValue.IndexOf("PM") != -1;
		}
		return true;
	}

	internal string GetNumberFormat(string strValue)
	{
		string text = ParseNumberFormat(strValue);
		if (text == null)
		{
			return "0.00";
		}
		return text;
	}

	private string ParseNumberFormat(string strValue)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		flag5 = strValue.IndexOf("$") != -1;
		flag = strValue.IndexOf("."[0]) != -1;
		flag2 = strValue.IndexOf("%"[0]) != -1;
		bool num = strValue.IndexOf(","[0]) != -1;
		flag3 = strValue.IndexOf("E"[0]) != -1;
		flag4 = strValue.IndexOf("/"[0]) != -1;
		int[] array = null;
		string result = null;
		FormatImpl formatImpl = null;
		if (num)
		{
			array = m_formatIndexes[","];
			int[] array2 = array;
			foreach (int key in array2)
			{
				formatImpl = m_rawFormats[key];
				if (formatImpl.DecimalPlaces > 1 && flag && (!flag5 || formatImpl.FormatString.IndexOf("$") != -1))
				{
					result = formatImpl.FormatString;
					break;
				}
			}
		}
		else if (flag4)
		{
			array = m_formatIndexes["/"];
			formatImpl = ((strValue.Split("/"[0])[0].Length <= 1) ? m_rawFormats[array[0]] : m_rawFormats[array[1]]);
			result = formatImpl.FormatString;
		}
		else if (flag2)
		{
			array = m_formatIndexes["%"];
			result = ((!flag) ? m_rawFormats[array[0]].FormatString : m_rawFormats[array[1]].FormatString);
		}
		else if (flag)
		{
			array = m_formatIndexes["."];
			if (flag5)
			{
				array = m_formatIndexes[","];
				result = "$" + m_rawFormats[array[1]].FormatString;
			}
			else
			{
				result = m_rawFormats[array[0]].FormatString;
			}
		}
		else if (flag3)
		{
			array = m_formatIndexes["E"];
			formatImpl = m_rawFormats[array[0]];
			result = formatImpl.FormatString;
		}
		else if (flag5)
		{
			array = m_formatIndexes[","];
			result = "$" + m_rawFormats[array[1]].FormatString;
		}
		else
		{
			result = m_rawFormats[1].FormatString;
		}
		return result;
	}

	public IEnumerator<KeyValuePair<int, FormatImpl>> GetEnumerator()
	{
		return m_rawFormats.GetEnumerator();
	}

	public void Remove(int key)
	{
		FormatImpl formatImpl = m_rawFormats[key];
		if (formatImpl != null)
		{
			m_rawFormats.Remove(key);
			m_hashFormatStrings.Remove(formatImpl.FormatString);
		}
	}

	public bool Contains(int key)
	{
		return m_rawFormats.Contains(key);
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, FormatImpl> hashFormatString in m_hashFormatStrings)
		{
			hashFormatString.Value.Clear();
		}
		m_rawFormats.Clear();
		m_hashFormatStrings.Clear();
		m_rawFormats = null;
		m_hashFormatStrings = null;
		if (m_parser != null)
		{
			m_parser.Clear();
			m_parser.Dispose();
		}
		Dispose();
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}
}
