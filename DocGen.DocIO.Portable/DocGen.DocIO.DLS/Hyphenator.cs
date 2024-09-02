using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Convertors;

namespace DocGen.DocIO.DLS;

public class Hyphenator
{
	private Dictionary<string, List<int>> m_patterns;

	private static Dictionary<string, Stream> m_dictionaries;

	private static Dictionary<string, Hyphenator> m_loadedHyphenators;

	private const char marker = '.';

	private int leftMin = 2;

	private int rightMin = 3;

	internal Dictionary<string, List<int>> Patterns
	{
		get
		{
			if (m_patterns == null)
			{
				m_patterns = new Dictionary<string, List<int>>();
			}
			return m_patterns;
		}
		set
		{
			m_patterns = value;
		}
	}

	public static Dictionary<string, Stream> Dictionaries
	{
		get
		{
			if (m_dictionaries == null)
			{
				m_dictionaries = new Dictionary<string, Stream>();
			}
			return m_dictionaries;
		}
		set
		{
			m_dictionaries = value;
		}
	}

	internal static Dictionary<string, Hyphenator> LoadedHyphenators
	{
		get
		{
			if (m_loadedHyphenators == null)
			{
				m_loadedHyphenators = new Dictionary<string, Hyphenator>();
			}
			return m_loadedHyphenators;
		}
		set
		{
			m_loadedHyphenators = value;
		}
	}

	public event AddDictionaryEventHandler AddDictionary;

	internal Hyphenator()
	{
	}

	internal Hyphenator(Stream file)
	{
		LoadPattern(file);
	}

	public static void UnloadDictionaries()
	{
		LoadedHyphenators.Clear();
		Dictionaries.Clear();
	}

	private void LoadPattern(Stream file)
	{
		Encoding encoding = null;
		string text = "Windows-1252";
		try
		{
			encoding = Encoding.GetEncoding(text);
		}
		catch (Exception)
		{
			encoding = ((!(text == "Windows-1252")) ? ((Encoding)new DocGen.DocIO.DLS.Convertors.ASCIIEncoding()) : ((Encoding)new Windows1252Encoding()));
		}
		StreamReader streamReader = new StreamReader(file, encoding);
		streamReader.ReadLine();
		string text2;
		while ((text2 = streamReader.ReadLine()) != null)
		{
			if (text2 == "" || text2[0].Equals("%") || text2[0].Equals('#'))
			{
				continue;
			}
			if (text2.Contains("COMPOUNDLEFTHYPHENMIN"))
			{
				leftMin = int.Parse(text2[22].ToString(CultureInfo.InvariantCulture));
				continue;
			}
			if (text2.Contains("COMPOUNDRIGHTHYPHENMIN"))
			{
				rightMin = int.Parse(text2[23].ToString(CultureInfo.InvariantCulture));
				continue;
			}
			if (text2.Contains("LEFTHYPHENMIN"))
			{
				leftMin = int.Parse(text2[14].ToString(CultureInfo.InvariantCulture));
				continue;
			}
			if (text2.Contains("RIGHTHYPHENMIN"))
			{
				leftMin = int.Parse(text2[15].ToString(CultureInfo.InvariantCulture));
				continue;
			}
			List<int> list = new List<int>(text2.Length);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			string text3 = text2;
			for (int i = 0; i < text3.Length; i++)
			{
				char c = text3[i];
				if (char.IsDigit(c))
				{
					list.Add(int.Parse(c.ToString(CultureInfo.InvariantCulture)));
					flag = false;
					continue;
				}
				if (flag)
				{
					list.Add(0);
				}
				stringBuilder.Append(c);
				flag = true;
			}
			if (flag)
			{
				list.Add(0);
			}
			if (FindMaxValue(list) != 0 && !Patterns.ContainsKey(stringBuilder.ToString()))
			{
				Patterns.Add(stringBuilder.ToString(), list);
			}
		}
	}

	private int FindMaxValue(List<int> levels)
	{
		int num = levels[0];
		foreach (int level in levels)
		{
			if (level > num)
			{
				num = level;
			}
		}
		return num;
	}

	internal string HyphenateText(string text)
	{
		int[] hyphenationMask = CreateHyphenateMaskFromLevels(GetPositions(text));
		return HyphenateByMask(text, hyphenationMask);
	}

	private int[] GetPositions(string text)
	{
		string text2 = new StringBuilder().Append('.').Append(text).Append('.')
			.ToString();
		int[] array = new int[text2.Length + 1];
		for (int i = 0; i < text2.Length - 1; i++)
		{
			for (int j = i; j <= text2.Length; j++)
			{
				string text3 = "";
				for (int k = i; k < j; k++)
				{
					text3 += text2[k];
				}
				if (!Patterns.ContainsKey(text3) || text3.Equals("-") || text3.Equals(",") || text3.Equals("'"))
				{
					continue;
				}
				List<int> list = Patterns[text3];
				int l = 0;
				int num = list.Count;
				for (; list[l] == 0; l++)
				{
				}
				while (list[num - 1] == 0)
				{
					num--;
				}
				List<int> list2 = new List<int>();
				for (int m = l; m < num; m++)
				{
					list2.Add(list[m]);
				}
				int num2 = 0;
				for (int n = i + l; n < i + l + list2.Count; n++)
				{
					if (array[n] < list2[num2])
					{
						array[n] = list2[num2];
					}
					num2++;
				}
			}
		}
		return array;
	}

	private static int[] CreateHyphenateMaskFromLevels(int[] levels)
	{
		int num = levels.Length - 2;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			if (i != 0 && levels[i + 1] % 2 != 0)
			{
				array[i] = 1;
			}
			else
			{
				array[i] = 0;
			}
		}
		return array;
	}

	private string HyphenateByMask(string originalWord, int[] hyphenationMask)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = originalWord.Length - rightMin;
		for (int i = 0; i < originalWord.Length; i++)
		{
			if (i >= leftMin && i <= num && hyphenationMask[i] > 0)
			{
				stringBuilder.Append("=");
			}
			stringBuilder.Append(originalWord[i]);
		}
		return stringBuilder.ToString();
	}

	internal string GetAlternateForMissedLanguageCode(string languageCode)
	{
		if (this.AddDictionary != null)
		{
			AddDictionaryEventArgs addDictionaryEventArgs = new AddDictionaryEventArgs(languageCode, "en-US");
			this.AddDictionary(this, addDictionaryEventArgs);
			Stream dictionaryStream = addDictionaryEventArgs.DictionaryStream;
			if (addDictionaryEventArgs.DictionaryStream != null && !Dictionaries.ContainsKey(languageCode))
			{
				Dictionaries.Add(languageCode, dictionaryStream);
			}
		}
		return languageCode;
	}
}
