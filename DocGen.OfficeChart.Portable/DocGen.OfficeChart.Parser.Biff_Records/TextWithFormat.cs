using System;
using System.Collections.Generic;
using System.Text;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class TextWithFormat : IComparable, ICloneable
{
	[Flags]
	internal enum StringType : byte
	{
		NonUnicode = 0,
		Unicode = 1,
		FarEast = 4,
		RichText = 8
	}

	private const byte DEF_COMPRESSED_MASK = 1;

	private const byte DEF_RICHTEXT_MASK = 8;

	internal const int DEF_FR_SIZE = 4;

	private const byte DEF_PLAIN_OPTIONS = 1;

	private const byte DEF_RTF_OPTIONS = 9;

	private SortedList<int, int> m_arrFormattingRuns;

	private string m_strValue = string.Empty;

	private int m_iDefaultIndex;

	private StringType m_options = StringType.Unicode;

	private bool m_bNeedDefragment = true;

	public int RefCount;

	private string m_rtfText;

	private bool m_isPreserved;

	internal string RtfText
	{
		get
		{
			return m_rtfText;
		}
		set
		{
			m_rtfText = value;
		}
	}

	public string Text
	{
		get
		{
			return m_strValue;
		}
		set
		{
			m_strValue = value;
		}
	}

	public SortedList<int, int> FormattingRuns
	{
		get
		{
			if (m_arrFormattingRuns == null)
			{
				m_arrFormattingRuns = new SortedList<int, int>();
			}
			return m_arrFormattingRuns;
		}
	}

	internal SortedList<int, int> InnerFormattingRuns => m_arrFormattingRuns;

	public int DefaultFontIndex
	{
		get
		{
			return m_iDefaultIndex;
		}
		set
		{
			m_iDefaultIndex = value;
		}
	}

	public int FormattingRunsCount
	{
		get
		{
			if (m_arrFormattingRuns == null)
			{
				return 0;
			}
			return m_arrFormattingRuns.Count;
		}
	}

	public bool IsPreserved
	{
		get
		{
			return m_isPreserved;
		}
		set
		{
			m_isPreserved = value;
		}
	}

	public TextWithFormat()
	{
	}

	public TextWithFormat(int fontIndex)
		: this()
	{
		m_iDefaultIndex = fontIndex;
	}

	public static implicit operator string(TextWithFormat format)
	{
		return format.Text;
	}

	public static explicit operator TextWithFormat(string value)
	{
		return new TextWithFormat
		{
			Text = value
		};
	}

	public void SetTextFontIndex(int iStartPos, int iEndPos, int iFontIndex)
	{
		m_bNeedDefragment = true;
		CreateFormattingRuns();
		if (iStartPos < 0 || iStartPos > m_strValue.Length)
		{
			throw new ArgumentOutOfRangeException("iStartPos");
		}
		if (iEndPos < 0 || iEndPos > m_strValue.Length)
		{
			throw new ArgumentOutOfRangeException("iEndPos");
		}
		if (iStartPos > iEndPos)
		{
			throw new ArgumentException("iStartPos cannot be larger than iEndPos.");
		}
		int previousPosition = GetPreviousPosition(iStartPos);
		int previousPosition2 = GetPreviousPosition(iEndPos);
		if (previousPosition < 0)
		{
			_ = m_iDefaultIndex;
		}
		else
		{
			_ = m_arrFormattingRuns[previousPosition];
		}
		int value = ((previousPosition2 >= 0) ? m_arrFormattingRuns[previousPosition2] : m_iDefaultIndex);
		RemoveAllInsideRange(previousPosition, previousPosition2);
		m_arrFormattingRuns[iStartPos] = iFontIndex;
		if (iEndPos < m_strValue.Length - 1 && !m_arrFormattingRuns.ContainsKey(iEndPos + 1))
		{
			m_arrFormattingRuns[iEndPos + 1] = value;
		}
	}

	public int GetTextFontIndex(int iPos)
	{
		if (m_arrFormattingRuns == null)
		{
			return m_iDefaultIndex;
		}
		int result = m_iDefaultIndex;
		int previousPosition = GetPreviousPosition(iPos);
		if (previousPosition >= 0)
		{
			result = m_arrFormattingRuns[previousPosition];
		}
		return result;
	}

	public int GetTextFontIndex(int iPos, bool iscopy)
	{
		if (m_arrFormattingRuns == null)
		{
			return m_iDefaultIndex;
		}
		int result = m_iDefaultIndex;
		int positionByIndex = GetPositionByIndex(iPos);
		if (positionByIndex >= 0)
		{
			result = m_arrFormattingRuns[positionByIndex];
		}
		return result;
	}

	public int GetFontByIndex(int iIndex)
	{
		if (m_arrFormattingRuns == null)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		CheckOffset(m_arrFormattingRuns.Count, iIndex);
		return m_arrFormattingRuns.Values[iIndex];
	}

	public int GetPositionByIndex(int iIndex)
	{
		if (m_arrFormattingRuns.Count <= iIndex || iIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		return m_arrFormattingRuns.Keys[iIndex];
	}

	public void SetFontByIndex(int index, int iFontIndex)
	{
		CreateFormattingRuns();
		int key = m_arrFormattingRuns.Keys[index];
		m_arrFormattingRuns[key] = iFontIndex;
	}

	public void ClearFormatting()
	{
		if (m_arrFormattingRuns != null)
		{
			m_arrFormattingRuns.Clear();
		}
	}

	public int CompareTo(object obj)
	{
		int num = 0;
		if (obj is TextWithFormat textWithFormat)
		{
			num = string.CompareOrdinal(textWithFormat.m_strValue, m_strValue);
			if (num == 0)
			{
				if (FormattingRunsCount == 0 && textWithFormat.FormattingRunsCount == 0)
				{
					return 0;
				}
				Defragment();
				textWithFormat.Defragment();
				return CompareFormattingRuns(m_arrFormattingRuns, textWithFormat.m_arrFormattingRuns);
			}
		}
		return num;
	}

	public static int CompareFormattingRuns(SortedList<int, int> fRuns1, SortedList<int, int> fRuns2)
	{
		if (fRuns1 == null && fRuns2 == null)
		{
			return 0;
		}
		if (fRuns1 == null)
		{
			return -1;
		}
		if (fRuns2 == null)
		{
			return 1;
		}
		int num = Math.Min(fRuns1.Count, fRuns2.Count);
		IList<int> keys = fRuns1.Keys;
		IList<int> keys2 = fRuns2.Keys;
		IList<int> values = fRuns1.Values;
		IList<int> values2 = fRuns2.Values;
		for (int i = 0; i < num; i++)
		{
			int num2 = keys[i] - keys2[i];
			if (num2 != 0)
			{
				return num2;
			}
			num2 = values[i] - values2[i];
			if (num2 != 0)
			{
				return num2;
			}
		}
		return fRuns1.Count - fRuns2.Count;
	}

	private void CreateFormattingRuns()
	{
		if (m_arrFormattingRuns == null)
		{
			m_arrFormattingRuns = new SortedList<int, int>();
		}
	}

	private int GetPreviousPosition(int iPos)
	{
		int num = 0;
		int num2 = FormattingRunsCount - 1;
		if (num2 < 0)
		{
			return -1;
		}
		IList<int> keys = m_arrFormattingRuns.Keys;
		int num4;
		while (true)
		{
			int num3 = (num + num2) / 2;
			num4 = keys[num3];
			if (num >= num2 - 1)
			{
				int num5 = keys[num2];
				if (num5 <= iPos)
				{
					return num5;
				}
				if (num4 <= iPos)
				{
					return num4;
				}
				return -1;
			}
			if (num4 == iPos)
			{
				break;
			}
			if (num4 < iPos)
			{
				num = Math.Min(num2, num3);
			}
			else
			{
				num2 = Math.Max(num, num3);
			}
		}
		return num4;
	}

	internal void RemoveAllInsideRange(int iStartPos, int iEndPos)
	{
		int count = m_arrFormattingRuns.Count;
		if (count != 0)
		{
			IList<int> keys = m_arrFormattingRuns.Keys;
			int[] array = new int[count];
			keys.CopyTo(array, 0);
			int num = ((iStartPos != -1) ? Array.BinarySearch(array, iStartPos) : 0);
			int num2 = ((iEndPos == -1) ? (array.Length - 1) : Array.BinarySearch(array, iEndPos));
			if (keys[num] == iStartPos)
			{
				num++;
			}
			for (int i = num; i <= num2; i++)
			{
				m_arrFormattingRuns.RemoveAt(num);
			}
		}
	}

	public void Defragment()
	{
		if (m_arrFormattingRuns == null)
		{
			m_bNeedDefragment = false;
		}
		if (!m_bNeedDefragment)
		{
			return;
		}
		int num = m_arrFormattingRuns.Count;
		IList<int> values = m_arrFormattingRuns.Values;
		int num2 = 0;
		while (num2 < num - 1)
		{
			if (values[num2] == values[num2 + 1])
			{
				m_arrFormattingRuns.RemoveAt(num2 + 1);
				num--;
			}
			else
			{
				num2++;
			}
		}
		m_bNeedDefragment = false;
	}

	public static void CheckOffset(int len, int iOffset)
	{
		if (iOffset < 0 || iOffset > len)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
	}

	public void CopyFormattingTo(TextWithFormat twin)
	{
		if (twin == null)
		{
			throw new ArgumentNullException("twin");
		}
		if (m_arrFormattingRuns == null || m_arrFormattingRuns.Count == 0)
		{
			twin.m_arrFormattingRuns = null;
			return;
		}
		twin.m_arrFormattingRuns = new SortedList<int, int>();
		IList<int> keys = m_arrFormattingRuns.Keys;
		IList<int> values = m_arrFormattingRuns.Values;
		int i = 0;
		for (int count = m_arrFormattingRuns.Count; i < count; i++)
		{
			twin.m_arrFormattingRuns.Add(keys[i], values[i]);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(m_strValue + Environment.NewLine);
		stringBuilder.Append("str[ 0 ] ... - " + m_iDefaultIndex);
		if (m_arrFormattingRuns != null)
		{
			IList<int> keys = m_arrFormattingRuns.Keys;
			int i = 0;
			for (int count = m_arrFormattingRuns.Count; i < count; i++)
			{
				int num = keys[i];
				stringBuilder.AppendFormat("\nstr[ {0} ] ... - {1}", num, m_arrFormattingRuns[num]);
			}
		}
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TextWithFormat))
		{
			return false;
		}
		return CompareTo(obj) == 0;
	}

	public override int GetHashCode()
	{
		return m_strValue.GetHashCode();
	}

	public virtual int Parse(byte[] data, int iOffset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int len = data.Length;
		CheckOffset(len, iOffset);
		if (iOffset < 0 || iOffset > data.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int num = iOffset;
		CheckOffset(len, num + 2);
		ushort usChartCount = BitConverter.ToUInt16(data, num);
		num += 2;
		CheckOffset(len, num + 1);
		m_options = (StringType)data[num];
		num++;
		bool bIsUnicode = (m_options & StringType.Unicode) != 0;
		bool num2 = (m_options & StringType.FarEast) != 0;
		bool num3 = (m_options & StringType.RichText) != 0;
		int num4 = 0;
		int num5 = 0;
		if (num3)
		{
			CheckOffset(len, num + 2);
			num4 = BitConverter.ToUInt16(data, num);
			num += 2;
		}
		if (num2)
		{
			CheckOffset(len, num + 4);
			num5 = BitConverter.ToInt32(data, num);
			num += 4;
		}
		Text = GetText(data, usChartCount, bIsUnicode, ref num);
		if (num4 > 0)
		{
			ParseFormattingRuns(data, num, num4);
		}
		if (num5 > 0)
		{
			ParseFarEastData(data, num, num5);
		}
		return num - iOffset;
	}

	public int GetTextSize()
	{
		return Encoding.Unicode.GetByteCount(m_strValue) + 3;
	}

	public int GetFormattingSize()
	{
		Defragment();
		int formattingRunsCount = FormattingRunsCount;
		if (formattingRunsCount > 0)
		{
			m_options |= StringType.RichText;
		}
		return formattingRunsCount * 4;
	}

	public byte[] SerializeFormatting()
	{
		byte[] array = SerializeFormattingRuns();
		if (array != null && array.Length != 0)
		{
			m_options |= StringType.RichText;
		}
		return array;
	}

	public int SerializeFormatting(byte[] arrBuffer, int iOffset, bool bDefragment)
	{
		int num = SerializeFormattingRuns(arrBuffer, iOffset, bDefragment);
		if (num > 0)
		{
			m_options |= StringType.RichText;
		}
		return num;
	}

	public StringType GetOptions()
	{
		return m_options;
	}

	private string GetText(byte[] data, ushort usChartCount, bool bIsUnicode, ref int iOffset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int num;
		string @string;
		if (bIsUnicode)
		{
			num = usChartCount * 2;
			CheckOffset(data.Length, iOffset + num);
			@string = Encoding.Unicode.GetString(data, iOffset, num);
		}
		else
		{
			num = usChartCount;
			CheckOffset(data.Length, iOffset + num);
			@string = BiffRecordRaw.LatinEncoding.GetString(data, iOffset, num);
		}
		iOffset += num;
		return @string;
	}

	private void ParseFormattingRuns(byte[] data, int iOffset, int iFRCount)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (iOffset >= data.Length && iFRCount > 0)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		CreateFormattingRuns();
		int len = data.Length;
		int num = 0;
		while (num < iFRCount)
		{
			CheckOffset(len, iOffset + 4);
			int key = BitConverter.ToUInt16(data, iOffset);
			int value = BitConverter.ToUInt16(data, iOffset + 2);
			m_arrFormattingRuns[key] = value;
			num++;
			iOffset += 4;
		}
	}

	internal void ParseFormattingRuns(byte[] data)
	{
		if (data != null && data.Length != 0)
		{
			ParseFormattingRuns(data, 0, data.Length / 4);
		}
	}

	private void ParseFarEastData(byte[] data, int iOffset, int iFarEastDataLen)
	{
	}

	private byte[] SerializeFormattingRuns()
	{
		Defragment();
		int formattingRunsCount = FormattingRunsCount;
		if (formattingRunsCount == 0)
		{
			return null;
		}
		byte[] array = new byte[formattingRunsCount * 4];
		SerializeFormattingRuns(array, 0, bDefragment: true);
		return array;
	}

	private int SerializeFormattingRuns(byte[] arrDestination, int iOffset, bool bDefragment)
	{
		if (bDefragment)
		{
			Defragment();
		}
		int formattingRunsCount = FormattingRunsCount;
		if (formattingRunsCount == 0)
		{
			return 0;
		}
		if (arrDestination == null)
		{
			throw new ArgumentNullException("arrDestination");
		}
		int num = formattingRunsCount * 4;
		if (iOffset < 0 || iOffset + num > arrDestination.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		IList<int> keys = m_arrFormattingRuns.Keys;
		IList<int> values = m_arrFormattingRuns.Values;
		int num2 = 0;
		while (num2 < formattingRunsCount)
		{
			byte[] bytes = BitConverter.GetBytes((ushort)keys[num2]);
			arrDestination[iOffset] = bytes[0];
			arrDestination[iOffset + 1] = bytes[1];
			bytes = BitConverter.GetBytes((ushort)values[num2]);
			arrDestination[iOffset + 2] = bytes[0];
			arrDestination[iOffset + 3] = bytes[1];
			num2++;
			iOffset += 4;
		}
		return num;
	}

	public object Clone()
	{
		return TypedClone();
	}

	public TextWithFormat TypedClone()
	{
		TextWithFormat textWithFormat = MemberwiseClone() as TextWithFormat;
		if (m_arrFormattingRuns != null)
		{
			CopyFormattingTo(textWithFormat);
		}
		return textWithFormat;
	}

	public TextWithFormat Clone(Dictionary<int, int> dicFontIndexes)
	{
		TextWithFormat textWithFormat = TypedClone();
		if (dicFontIndexes != null)
		{
			textWithFormat.UpdateFontIndexes(dicFontIndexes);
		}
		return textWithFormat;
	}

	private void UpdateFontIndexes(Dictionary<int, int> dicFontIndexes)
	{
		if (dicFontIndexes == null)
		{
			throw new ArgumentNullException("arrFontIndexes");
		}
		if (FormattingRunsCount > 0)
		{
			IList<int> values = m_arrFormattingRuns.Values;
			IList<int> keys = m_arrFormattingRuns.Keys;
			int i = 0;
			for (int formattingRunsCount = FormattingRunsCount; i < formattingRunsCount; i++)
			{
				int key = keys[i];
				int iOldIndex = values[i];
				iOldIndex = FontImpl.UpdateFontIndexes(iOldIndex, dicFontIndexes, OfficeParseOptions.Default);
				m_arrFormattingRuns[key] = iOldIndex;
			}
		}
	}

	internal void ReplaceFont(int oldFontIndex, int newFontIndex)
	{
		SortedList<int, int> sortedList = new SortedList<int, int>();
		foreach (KeyValuePair<int, int> arrFormattingRun in m_arrFormattingRuns)
		{
			int num = arrFormattingRun.Value;
			if (num == oldFontIndex)
			{
				num = newFontIndex;
			}
			sortedList.Add(arrFormattingRun.Key, num);
		}
		m_arrFormattingRuns = sortedList;
	}

	internal void RemoveAtStart(int length)
	{
		int previousPosition = GetPreviousPosition(length);
		if (previousPosition >= 0)
		{
			int value = m_arrFormattingRuns[previousPosition];
			for (int num = m_arrFormattingRuns.IndexOfKey(previousPosition); num >= 0; num--)
			{
				m_arrFormattingRuns.RemoveAt(num);
			}
			m_arrFormattingRuns[length] = value;
			SortedList<int, int> sortedList = new SortedList<int, int>();
			foreach (KeyValuePair<int, int> arrFormattingRun in m_arrFormattingRuns)
			{
				sortedList[arrFormattingRun.Key - length] = arrFormattingRun.Value;
			}
			m_arrFormattingRuns = sortedList;
		}
		Text = Text.Substring(length);
	}

	internal void RemoveAtEnd(int length)
	{
		int num = Text.Length - length;
		int iPos = num - 1;
		int previousPosition = GetPreviousPosition(iPos);
		if (previousPosition >= 0)
		{
			_ = m_arrFormattingRuns[previousPosition];
			int num2 = m_arrFormattingRuns.IndexOfKey(previousPosition) + 1;
			for (int num3 = m_arrFormattingRuns.Count - 1; num3 >= num2; num3--)
			{
				m_arrFormattingRuns.RemoveAt(num3);
			}
		}
		else
		{
			ClearFormatting();
		}
		Text = Text.Substring(0, num);
	}
}
