using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Pdf;

internal class FontFile
{
	private const int c1 = 52845;

	private const int c2 = 22719;

	private int m_skipBytes = 4;

	private Dictionary<string, byte[]> m_glyphs = new Dictionary<string, byte[]>();

	internal Dictionary<int, string> m_differenceEncoding = new Dictionary<int, string>();

	internal double[] m_fontMatrix = new double[6];

	internal bool m_hasFontMatrix;

	internal CffGlyphs m_cffGlyphs = new CffGlyphs();

	public void Type1()
	{
	}

	private void ParseDifferenceEncoding(StreamReader br)
	{
		bool flag = true;
		string text;
		while ((text = br.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.StartsWith("readonly"))
			{
				break;
			}
			int num = 0;
			if (!text.StartsWith("dup") || !text.Contains("/"))
			{
				continue;
			}
			string[] array = text.Split(new string[2] { " ", "/" }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length < 3)
			{
				continue;
			}
			_ = array[num];
			num++;
			string text2 = array[num];
			int num2 = text2.IndexOf('#');
			int key;
			if (num2 == -1)
			{
				key = int.Parse(text2);
			}
			else
			{
				text2.Substring(0, num2);
				key = int.Parse(text2.Substring(num2 + 1, text2.Length));
			}
			num++;
			string text3 = array[num];
			m_differenceEncoding.Add(key, text3);
			char c = text3[0];
			if (c == 'B' || c == 'C' || c == 'c' || c == 'G')
			{
				int num3 = 1;
				int length = text3.Length;
				while (!flag && num3 < length)
				{
					flag = char.IsLetter(text3[num3++]);
				}
			}
		}
	}

	internal CffGlyphs ParseType1FontFile(byte[] content)
	{
		StreamReader streamReader = new StreamReader(new MemoryStream(content));
		while (true)
		{
			string text = streamReader.ReadLine();
			if (text == null)
			{
				break;
			}
			if (text.StartsWith("/Encoding 256 array"))
			{
				ParseDifferenceEncoding(streamReader);
			}
			else if (text.StartsWith("/lenIV"))
			{
				string[] array = text.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
				m_skipBytes = Convert.ToInt32(array[1]);
			}
			else
			{
				if (text.IndexOf("/FontMatrix") == -1)
				{
					continue;
				}
				string text2 = "";
				int num = text.IndexOf('[');
				if (num != -1)
				{
					int num2 = text.IndexOf(']');
					text2 = text.Substring(num + 1, num2 - (num + 1));
				}
				else
				{
					num = text.IndexOf('{');
					if (num != -1)
					{
						int num2 = text.IndexOf('}');
						text2 = text.Substring(num + 1, num2 - (num + 1));
					}
				}
				string[] array2 = text2.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length == 6)
				{
					m_hasFontMatrix = true;
				}
				for (int i = 0; i < 6; i++)
				{
					m_fontMatrix[i] = Convert.ToDouble(array2[i]);
				}
			}
		}
		if (streamReader != null)
		{
			try
			{
				streamReader.Dispose();
			}
			catch (Exception)
			{
			}
		}
		m_cffGlyphs.Glyphs = ParseEncodedContent(content);
		m_cffGlyphs.FontMatrix = m_fontMatrix;
		m_cffGlyphs.DifferenceEncoding = m_differenceEncoding;
		_ = m_glyphs.Count;
		return m_cffGlyphs;
	}

	public Dictionary<string, byte[]> ParseEncodedContent(byte[] cont)
	{
		string rd = "rd";
		string nd = "nd";
		int num = cont.Length;
		int i = -1;
		int num2 = -1;
		for (int j = 4; j < num; j++)
		{
			if (cont[j - 3] == 101 && cont[j - 2] == 120 && cont[j - 1] == 101 && cont[j] == 99)
			{
				for (i = j + 1; cont[i] == 10 || cont[i] == 13; i++)
				{
				}
				j = num;
			}
		}
		if (i != -1)
		{
			for (int j = i; j < num - 10; j++)
			{
				if (cont[j] == 99 && cont[j + 1] == 108 && cont[j + 2] == 101 && cont[j + 3] == 97 && cont[j + 4] == 114 && cont[j + 5] == 116 && cont[j + 6] == 111 && cont[j + 7] == 109 && cont[j + 8] == 97 && cont[j + 9] == 114 && cont[j + 10] == 107)
				{
					num2 = j - 1;
					while (cont[num2] == 10 || cont[num2] == 13)
					{
						num2--;
					}
					j = num;
				}
			}
		}
		if (num2 == -1)
		{
			num2 = num;
		}
		int num3 = 55665;
		int num4 = 4;
		for (int j = i; j < i + num4 * 2; j++)
		{
			char c = (char)cont[j];
			if ((c < '0' || c > '9') && (c < 'A' || c > 'F') && (c < 'a' || c > 'f'))
			{
				break;
			}
		}
		MemoryStream memoryStream = new MemoryStream(num2 - i);
		if (i != -1)
		{
			for (int j = i; j < num2; j++)
			{
				byte num5 = cont[j];
				int num6 = num5 ^ (num3 >> 8);
				num3 = (num5 + num3) * 52845 + 22719;
				if (j > i + num4)
				{
					memoryStream.WriteByte((byte)num6);
				}
			}
			cont = memoryStream.ToArray();
			memoryStream.Position = 0L;
		}
		StreamReader streamReader = new StreamReader(memoryStream);
		while (true)
		{
			string text = streamReader.ReadLine();
			if (text == null)
			{
				break;
			}
			if (text.StartsWith("/lenIV"))
			{
				string[] array = text.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
				m_skipBytes = Convert.ToInt32(array[1]);
			}
		}
		streamReader.Dispose();
		int num7 = cont.Length;
		int k = 0;
		int num8 = -1;
		i = -1;
		for (; k < num7 && k != num7; k++)
		{
			if (k + 11 < num7 && cont[k] == 47 && cont[k + 1] == 67 && cont[k + 2] == 104 && cont[k + 3] == 97 && cont[k + 4] == 114 && cont[k + 5] == 83 && cont[k + 6] == 116 && cont[k + 7] == 114 && cont[k + 8] == 105 && cont[k + 9] == 110 && cont[k + 10] == 103 && cont[k + 11] == 115)
			{
				i = k + 11;
			}
			else if (k + 5 < num7 && cont[k] == 47 && cont[k + 1] == 83 && cont[k + 2] == 117 && cont[k + 3] == 98 && cont[k + 4] == 114 && cont[k + 5] == 115)
			{
				num8 = k + 6;
			}
			if (num8 > -1 && i > -1)
			{
				break;
			}
		}
		Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
		if (i != -1)
		{
			dictionary = ExtractFontData(m_skipBytes, cont, i, rd, num7, nd);
			_ = dictionary.Count;
		}
		if (num8 > -1)
		{
			dictionary = ExtractSubroutineData(m_skipBytes, cont, num8, i, rd, num7, nd);
		}
		return dictionary;
	}

	private Dictionary<string, byte[]> ExtractSubroutineData(int skipBytes, byte[] cont, int start, int charStart, string rd, int l, string nd)
	{
		while (cont[start] == 32 || cont[start] == 10 || cont[start] == 13)
		{
			start++;
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			char c = (char)cont[start];
			if (c == ' ')
			{
				break;
			}
			stringBuilder.Append(c);
			start++;
		}
		int num = Convert.ToInt32(stringBuilder.ToString());
		for (int i = 0; i < num; i++)
		{
			while (start < l && !((cont[start - 2] == 100 && cont[start - 1] == 117 && cont[start] == 112) || start == charStart))
			{
				start++;
			}
			if (start == charStart)
			{
				i = num;
				continue;
			}
			while (cont[start + 1] == 32)
			{
				start++;
			}
			StringBuilder stringBuilder2 = new StringBuilder("subrs");
			while (true)
			{
				start++;
				char c2 = (char)cont[start];
				if (c2 == ' ')
				{
					break;
				}
				stringBuilder2.Append(c2);
			}
			stringBuilder = new StringBuilder();
			while (true)
			{
				start++;
				char c3 = (char)cont[start];
				if (c3 == ' ')
				{
					break;
				}
				stringBuilder.Append(c3);
			}
			int num2 = Convert.ToInt32(stringBuilder.ToString());
			while (cont[start] == 32)
			{
				start++;
			}
			start = start + rd.Length + 1;
			byte[] stream = GetStream(skipBytes, start, num2, cont);
			m_glyphs.Add(stringBuilder2.ToString(), stream);
			start = start + num2 + nd.Length;
		}
		return m_glyphs;
	}

	private Dictionary<string, byte[]> ExtractFontData(int skipBytes, byte[] cont, int start, string rd, int l, string nd)
	{
		int num = cont.Length;
		int num2 = 0;
		while (start < num && cont[start] != 47)
		{
			start++;
		}
		int i = start;
		while (start < l)
		{
			if (cont[i] == 47)
			{
				for (i += 2; i < num && (cont[i - 1] != 124 || (cont[i] != 45 && cont[i] != 48) || (cont[i + 1] != 10 && cont[i + 1] != 13)) && (cont[i - 1] != 78 || cont[i] != 68); i++)
				{
				}
			}
			if (num - i < 3 || (cont[i - 1] != 47 && cont[i] == 101 && cont[i + 1] == 110 && cont[i + 2] == 100))
			{
				break;
			}
			i++;
		}
		while (start <= i)
		{
			StringBuilder stringBuilder = new StringBuilder(20);
			while (true)
			{
				start++;
				char c = (char)cont[start];
				if (c == ' ')
				{
					break;
				}
				stringBuilder.Append(c);
			}
			start++;
			StringBuilder stringBuilder2 = new StringBuilder();
			while (true)
			{
				char c2 = (char)cont[start];
				if (c2 == ' ')
				{
					break;
				}
				stringBuilder2.Append(c2);
				start++;
			}
			int num3 = Convert.ToInt32(stringBuilder2.ToString());
			while (cont[start] == 32)
			{
				start++;
			}
			start = start + rd.Length + 1;
			byte[] stream = GetStream(skipBytes, start, num3, cont);
			m_glyphs.Add(stringBuilder.ToString(), stream);
			num2++;
			start = start + num3 + nd.Length;
			while (start <= i && cont[start] != 47)
			{
				start++;
			}
		}
		return m_glyphs;
	}

	private byte[] GetStream(int skipBytes, int start, int byteCount, byte[] cont)
	{
		MemoryStream memoryStream = new MemoryStream();
		int num = 4330;
		for (int i = 0; i < byteCount; i++)
		{
			byte num2 = cont[start + i];
			int num3 = num2 ^ (num >> 8);
			num = (num2 + num) * 52845 + 22719;
			if (i >= skipBytes)
			{
				memoryStream.WriteByte((byte)num3);
			}
		}
		return memoryStream.ToArray();
	}
}
