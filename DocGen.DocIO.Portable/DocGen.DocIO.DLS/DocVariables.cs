using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.DocIO.DLS;

public class DocVariables
{
	private Dictionary<string, string> m_variables;

	public string this[string name]
	{
		get
		{
			if (m_variables.ContainsKey(name))
			{
				return m_variables[name];
			}
			return null;
		}
		set
		{
			m_variables[name] = value;
		}
	}

	public int Count => m_variables.Count;

	internal Dictionary<string, string> Items => m_variables;

	public DocVariables()
	{
		m_variables = new Dictionary<string, string>();
	}

	public void Add(string name, string value)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		m_variables.Add(name, value);
	}

	public string GetNameByIndex(int index)
	{
		CheckIndex(index);
		return FindItem(index, returnName: true);
	}

	public string GetValueByIndex(int index)
	{
		CheckIndex(index);
		return FindItem(index, returnName: false);
	}

	public void Remove(string name)
	{
		m_variables.Remove(name);
	}

	internal void Close()
	{
		if (m_variables != null)
		{
			m_variables.Clear();
			m_variables = null;
		}
	}

	internal void UpdateVariables(byte[] variables)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(variables), Encoding.Unicode);
		binaryReader.ReadInt16();
		int num = binaryReader.ReadInt16();
		binaryReader.ReadInt16();
		int num2 = 0;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			num2 = binaryReader.ReadInt16();
			char[] value = binaryReader.ReadChars(num2);
			array[i] = new string(value);
			binaryReader.ReadInt32();
		}
		for (int j = 0; j < num; j++)
		{
			num2 = binaryReader.ReadUInt16();
			char[] value2 = binaryReader.ReadChars(num2);
			m_variables.Add(array[j], new string(value2));
		}
	}

	internal byte[] ToByteArray()
	{
		if (m_variables.Count == 0)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.Unicode);
		string[] array = new string[m_variables.Count];
		string[] array2 = new string[m_variables.Count];
		int num = 0;
		SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
		foreach (string key in m_variables.Keys)
		{
			sortedDictionary.Add(key, m_variables[key]);
		}
		foreach (string key2 in sortedDictionary.Keys)
		{
			array2[num] = sortedDictionary[array[num] = key2];
			num++;
		}
		binaryWriter.Write(byte.MaxValue);
		binaryWriter.Write(byte.MaxValue);
		short num2 = (short)m_variables.Count;
		binaryWriter.Write(num2);
		binaryWriter.Write((short)4);
		for (int i = 0; i < num2; i++)
		{
			binaryWriter.Write((short)array[i].Length);
			binaryWriter.Write(array[i].ToCharArray());
			binaryWriter.Write(int.MaxValue);
		}
		for (int j = 0; j < num2; j++)
		{
			binaryWriter.Write((short)array2[j].Length);
			binaryWriter.Write(array2[j].ToCharArray());
		}
		return memoryStream.ToArray();
	}

	private string FindItem(int index, bool returnName)
	{
		IDictionaryEnumerator dictionaryEnumerator = m_variables.GetEnumerator();
		for (int i = 0; i <= index; i++)
		{
			dictionaryEnumerator.MoveNext();
		}
		return (string)(returnName ? dictionaryEnumerator.Entry.Key : dictionaryEnumerator.Entry.Value);
	}

	private void CheckIndex(int index)
	{
		if (index < 0 || index >= m_variables.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index must be larger than 0 and lower than number of variables in the document");
		}
	}
}
