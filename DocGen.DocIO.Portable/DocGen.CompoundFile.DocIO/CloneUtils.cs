using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.CompoundFile.DocIO;

internal sealed class CloneUtils
{
	public static int[] CloneIntArray(int[] array)
	{
		if (array == null)
		{
			return null;
		}
		int num = array.Length;
		int[] array2 = new int[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static ushort[] CloneUshortArray(ushort[] array)
	{
		if (array == null)
		{
			return null;
		}
		int num = array.Length;
		ushort[] array2 = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static string[] CloneStringArray(string[] array)
	{
		if (array == null)
		{
			return null;
		}
		int num = array.Length;
		string[] array2 = new string[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static object[] CloneArray(object[] array)
	{
		if (array == null)
		{
			return null;
		}
		int num = array.Length;
		object[] array2 = new object[num];
		for (int i = 0; i < num; i++)
		{
			object obj = array[i];
			if (obj is ICloneable cloneable)
			{
				obj = cloneable.Clone();
			}
			array2[i] = obj;
		}
		return array2;
	}

	public static object CloneCloneable(ICloneable toClone)
	{
		return toClone?.Clone();
	}

	public static byte[] CloneByteArray(byte[] arr)
	{
		if (arr == null)
		{
			return null;
		}
		int num = arr.Length;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = arr[i];
		}
		return array;
	}

	public static Dictionary<TKey, TValue> CloneHash<TKey, TValue>(Dictionary<TKey, TValue> hash)
	{
		if (hash == null)
		{
			throw new ArgumentNullException("hash");
		}
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(hash.Count);
		foreach (KeyValuePair<TKey, TValue> item in hash)
		{
			TValue val = item.Value;
			if (val is ICloneable cloneable)
			{
				val = (TValue)cloneable.Clone();
			}
			TKey val2 = item.Key;
			if (val2 is ICloneable cloneable2)
			{
				val2 = (TKey)cloneable2.Clone();
			}
			dictionary.Add(val2, val);
		}
		return dictionary;
	}

	public static Dictionary<int, int> CloneHash(Dictionary<int, int> hash)
	{
		if (hash == null)
		{
			throw new ArgumentNullException("hash");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, int> item in hash)
		{
			dictionary.Add(item.Key, item.Value);
		}
		return dictionary;
	}

	public static Stream CloneStream(Stream stream)
	{
		if (stream == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream;
	}
}
