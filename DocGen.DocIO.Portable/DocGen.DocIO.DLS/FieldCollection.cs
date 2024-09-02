using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class FieldCollection : CollectionImpl
{
	internal List<string> m_sortedAutoNumFieldIndexes;

	internal List<WField> m_sortedAutoNumFields;

	private Dictionary<char, int> CharValues;

	internal WField this[string name] => FindByName(name);

	internal WField this[int index] => base.InnerList[index] as WField;

	internal List<WField> SortedAutoNumFields
	{
		get
		{
			if (m_sortedAutoNumFields == null)
			{
				m_sortedAutoNumFields = new List<WField>();
			}
			return m_sortedAutoNumFields;
		}
		set
		{
			m_sortedAutoNumFields = value;
		}
	}

	internal List<string> SortedAutoNumFieldIndexes
	{
		get
		{
			if (m_sortedAutoNumFieldIndexes == null)
			{
				m_sortedAutoNumFieldIndexes = new List<string>();
			}
			return m_sortedAutoNumFieldIndexes;
		}
		set
		{
			m_sortedAutoNumFieldIndexes = value;
		}
	}

	internal FieldCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	public WField FindByName(string name)
	{
		string text = name.Replace('-', '_');
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			WField wField = base.InnerList[i] as WField;
			if (wField.FieldValue.ToUpper() == text.ToUpper())
			{
				return wField;
			}
		}
		return null;
	}

	public void RemoveAt(int index)
	{
		WField wField = base.InnerList[index] as WField;
		if ((wField.FieldType == FieldType.FieldAutoNum || wField.FieldType == FieldType.FieldAutoNumLegal) && SortedAutoNumFields.Contains(wField))
		{
			int index2 = SortedAutoNumFields.IndexOf(wField);
			SortedAutoNumFields.Remove(wField);
			SortedAutoNumFieldIndexes.RemoveAt(index2);
		}
		Remove(wField);
	}

	public void Remove(WField field)
	{
		if ((field.FieldType == FieldType.FieldAutoNum || field.FieldType == FieldType.FieldAutoNumLegal) && SortedAutoNumFields.Contains(field))
		{
			int index = SortedAutoNumFields.IndexOf(field);
			SortedAutoNumFields.Remove(field);
			SortedAutoNumFieldIndexes.RemoveAt(index);
		}
		base.InnerList.Remove(field);
		field.IsAdded = false;
	}

	public void Clear()
	{
		while (base.InnerList.Count > 0)
		{
			int index = base.InnerList.Count - 1;
			RemoveAt(index);
		}
	}

	internal void Add(WField field)
	{
		if (!field.IsAdded)
		{
			if (field.FieldType == FieldType.FieldAutoNum || field.FieldType == FieldType.FieldAutoNumLegal)
			{
				InsertAutoNumFieldInAsc(field);
			}
			base.InnerList.Add(field);
			field.IsAdded = true;
		}
	}

	internal void InsertAutoNumFieldInAsc(WField field)
	{
		string hierarchicalIndex = field.GetHierarchicalIndex(string.Empty);
		if (SortedAutoNumFieldIndexes.Count == 0 && hierarchicalIndex != null && !hierarchicalIndex.Contains("-"))
		{
			SortedAutoNumFieldIndexes.Add(hierarchicalIndex);
			field.OriginalField = null;
			SortedAutoNumFields.Add(field);
		}
		else
		{
			if (hierarchicalIndex == null || hierarchicalIndex.Contains("-"))
			{
				return;
			}
			bool flag = false;
			int num = SortedAutoNumFieldIndexes.Count - 1;
			while (num > -1 && !flag)
			{
				string oldHierarchicalIndex = SortedAutoNumFieldIndexes[num];
				if (IsNewIndexhasLowHierarchy(oldHierarchicalIndex, hierarchicalIndex))
				{
					num--;
				}
				else
				{
					flag = true;
				}
			}
			SortedAutoNumFieldIndexes.Insert(num + 1, hierarchicalIndex);
			field.OriginalField = null;
			SortedAutoNumFields.Insert(num + 1, field);
		}
	}

	private bool IsNewIndexhasLowHierarchy(string oldHierarchicalIndex, string newHierarchicalIndex)
	{
		string[] array = oldHierarchicalIndex.Split(';');
		string[] array2 = newHierarchicalIndex.Split(';');
		for (int i = 0; i < array.Length || i < array2.Length; i++)
		{
			if (i < array.Length && i >= array2.Length)
			{
				return true;
			}
			if (i >= array.Length && i < array2.Length)
			{
				return false;
			}
			if (Convert.ToInt32(array2[i]) < Convert.ToInt32(array[i]))
			{
				return true;
			}
			if (Convert.ToInt32(array2[i]) > Convert.ToInt32(array[i]))
			{
				return false;
			}
		}
		return false;
	}

	internal string GetAutoNumFieldResult(WField field)
	{
		int num = -1;
		int num2 = 0;
		char c = ' ';
		string empty = string.Empty;
		string empty2 = string.Empty;
		num = ((field.OriginalField == null) ? SortedAutoNumFields.IndexOf(field) : SortedAutoNumFields.IndexOf(field.OriginalField));
		if (num == 0)
		{
			return (num + 1).ToString();
		}
		for (int num3 = num - 1; num3 >= 0; num3--)
		{
			if (SortedAutoNumFields[num3].OwnerParagraph.StyleName == SortedAutoNumFields[num].OwnerParagraph.StyleName && SortedAutoNumFields[num3].OwnerParagraph.ParagraphFormat.OutlineLevel == SortedAutoNumFields[num].OwnerParagraph.ParagraphFormat.OutlineLevel)
			{
				c = GetAutoNumSeparatorChar(num3);
				empty = GetNumberFormat(num3);
				empty2 = SortedAutoNumFields[num3].Text.TrimEnd(c);
				switch (empty)
				{
				case "ALPHABETIC":
				case "alphabetic":
					num2 = GetAsNumberFromLetter(empty2);
					break;
				case "ROMAN":
				case "roman":
					num2 = GetAsNumberFromRoman(empty2);
					break;
				default:
					num2 = Convert.ToInt32(empty2);
					break;
				}
				if (IsBothFieldsInSameParagarph(num3, num, field.OwnerParagraph != null && field.OwnerParagraph.Owner is WTableCell))
				{
					num2--;
				}
				break;
			}
			if (SortedAutoNumFields[num3].OwnerParagraph.StyleName.StartsWith("Heading") || SortedAutoNumFields[num3].OwnerParagraph.ParagraphFormat.OutlineLevel.ToString().StartsWith("Level"))
			{
				return (num2 + 1).ToString();
			}
		}
		return (num2 + 1).ToString();
	}

	private bool IsBothFieldsInSameParagarph(int previousIndex, int currentIndex, bool currentFieldIsInTable)
	{
		string text = SortedAutoNumFieldIndexes[previousIndex];
		string text2 = SortedAutoNumFieldIndexes[currentIndex];
		string[] array = text.Split(';');
		string[] array2 = text2.Split(';');
		if (array.Length == array2.Length && array[0] == array2[0])
		{
			if (!currentFieldIsInTable && array[^3] == array2[^3])
			{
				return true;
			}
			if (currentFieldIsInTable)
			{
				for (int num = array.Length - 5; num > 0; num--)
				{
					if (array[num] != array2[num])
					{
						return false;
					}
				}
				return true;
			}
		}
		return false;
	}

	private int GetAsNumberFromRoman(string roman)
	{
		if (CharValues == null)
		{
			CharValues = new Dictionary<char, int>();
			CharValues.Add('I', 1);
			CharValues.Add('V', 5);
			CharValues.Add('X', 10);
			CharValues.Add('L', 50);
			CharValues.Add('C', 100);
			CharValues.Add('D', 500);
			CharValues.Add('M', 1000);
		}
		if (roman.Length == 0)
		{
			return 0;
		}
		roman = roman.ToUpper();
		if (roman[0] == '(')
		{
			int num = roman.LastIndexOf(')');
			string roman2 = roman.Substring(1, num - 1);
			string roman3 = roman.Substring(num + 1);
			return 1000 * GetAsNumberFromRoman(roman2) + GetAsNumberFromRoman(roman3);
		}
		int num2 = 0;
		int num3 = 0;
		for (int num4 = roman.Length - 1; num4 >= 0; num4--)
		{
			int num5 = CharValues[roman[num4]];
			if (num5 < num3)
			{
				num2 -= num5;
			}
			else
			{
				num2 += num5;
				num3 = num5;
			}
		}
		return num2;
	}

	private char GetAutoNumSeparatorChar(int indexOfField)
	{
		char result = '.';
		_ = new char[0];
		_ = string.Empty;
		WField wField = SortedAutoNumFields[indexOfField];
		string[] array = wField.FieldCode.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == "\\s" && i + 1 < array.Length)
			{
				result = array[i + 1].ToCharArray()[0];
			}
			else if (array[i] == "\\e" && wField.FieldType == FieldType.FieldAutoNumLegal)
			{
				result = '\0';
				break;
			}
		}
		return result;
	}

	private string GetNumberFormat(int indexOfField)
	{
		string result = string.Empty;
		_ = string.Empty;
		string[] array = SortedAutoNumFields[indexOfField].FieldCode.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 2)
		{
			result = array[2];
		}
		return result;
	}

	private int GetAsNumberFromLetter(string s)
	{
		int num = 0;
		int num2 = 1;
		for (int i = 0; i < s.Length; i++)
		{
			int num3 = char.ToUpper(s[i]) - 64;
			if (num2 == 1)
			{
				num += num3;
			}
			if (num2 > 1)
			{
				num += 25 + num3;
			}
			num2++;
		}
		return num;
	}
}
