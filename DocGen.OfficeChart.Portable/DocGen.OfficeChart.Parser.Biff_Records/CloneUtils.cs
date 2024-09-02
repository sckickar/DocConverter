using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

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

	[CLSCompliant(false)]
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

	public static List<T> CloneCloneable<T>(List<T> toClone)
	{
		if (toClone == null)
		{
			return null;
		}
		int count = toClone.Count;
		List<T> list = new List<T>(count);
		for (int i = 0; i < count; i++)
		{
			T item = ((toClone[i] is ICloneable cloneable) ? ((T)cloneable.Clone()) : toClone[i]);
			list.Add(item);
		}
		return list;
	}

	public static object CloneCloneable(ICloneable toClone)
	{
		return toClone?.Clone();
	}

	public static List<BiffRecordRaw> CloneCloneable(List<BiffRecordRaw> toClone)
	{
		if (toClone == null)
		{
			return null;
		}
		int count = toClone.Count;
		List<BiffRecordRaw> list = new List<BiffRecordRaw>(count);
		for (int i = 0; i < count; i++)
		{
			ICloneable toClone2 = toClone[i];
			list.Add((BiffRecordRaw)CloneCloneable(toClone2));
		}
		return list;
	}

	public static List<TextWithFormat> CloneCloneable(List<TextWithFormat> toClone)
	{
		if (toClone == null)
		{
			return null;
		}
		int count = toClone.Count;
		List<TextWithFormat> list = new List<TextWithFormat>(count);
		for (int i = 0; i < count; i++)
		{
			TextWithFormat item = toClone[i].TypedClone();
			list.Add(item);
		}
		return list;
	}

	public static SortedList<int, int> CloneSortedList(SortedList<int, int> toClone)
	{
		if (toClone == null)
		{
			return null;
		}
		int count = toClone.Count;
		SortedList<int, int> sortedList = new SortedList<int, int>(count);
		IList<int> keys = toClone.Keys;
		IList<int> values = toClone.Values;
		for (int i = 0; i < count; i++)
		{
			sortedList.Add(keys[i], values[i]);
		}
		return sortedList;
	}

	public static SortedList<TKey, TValue> CloneCloneable<TKey, TValue>(SortedList<TKey, TValue> list) where TKey : IComparable
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		SortedList<TKey, TValue> sortedList = new SortedList<TKey, TValue>(list.Count);
		foreach (KeyValuePair<TKey, TValue> item in list)
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
			sortedList.Add(val2, val);
		}
		return sortedList;
	}

	public static List<T> CloneCloneable<T>(IList<T> toClone, object parent)
	{
		if (toClone == null)
		{
			return null;
		}
		int count = toClone.Count;
		List<T> list = new List<T>(count);
		for (int i = 0; i < count; i++)
		{
			ICloneParent toClone2 = (ICloneParent)(object)toClone[i];
			list.Add((T)CloneCloneable(toClone2, parent));
		}
		return list;
	}

	public static object CloneCloneable(ICloneParent toClone, object parent)
	{
		return toClone?.Clone(parent);
	}

	[CLSCompliant(false)]
	public static object CloneMsoBase(MsoBase toClone, MsoBase parent)
	{
		return toClone?.Clone(parent);
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

	public static Ptg[] ClonePtgArray(Ptg[] arrToClone)
	{
		if (arrToClone == null)
		{
			return null;
		}
		int num = arrToClone.Length;
		Ptg[] array = new Ptg[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (Ptg)((ICloneable)arrToClone[i]).Clone();
		}
		return array;
	}

	[CLSCompliant(false)]
	public static ColumnInfoRecord[] CloneArray(ColumnInfoRecord[] arrToClone)
	{
		if (arrToClone == null)
		{
			return null;
		}
		int num = arrToClone.Length;
		ColumnInfoRecord[] array = new ColumnInfoRecord[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (ColumnInfoRecord)CloneCloneable(arrToClone[i]);
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

	public static Dictionary<TextWithFormat, int> CloneHash(Dictionary<TextWithFormat, int> hash)
	{
		if (hash == null)
		{
			throw new ArgumentNullException("hash");
		}
		Dictionary<TextWithFormat, int> dictionary = new Dictionary<TextWithFormat, int>();
		foreach (KeyValuePair<TextWithFormat, int> item in hash)
		{
			TextWithFormat key = item.Key.TypedClone();
			dictionary.Add(key, item.Value);
		}
		return dictionary;
	}

	public static Dictionary<object, int> CloneHash(Dictionary<object, int> hash)
	{
		if (hash == null)
		{
			throw new ArgumentNullException("hash");
		}
		Dictionary<object, int> dictionary = new Dictionary<object, int>();
		foreach (KeyValuePair<object, int> item in hash)
		{
			object obj = item.Key;
			if (obj is TextWithFormat textWithFormat)
			{
				obj = textWithFormat.Clone();
			}
			dictionary.Add(obj, item.Value);
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

	public static Dictionary<TKey, TValue> CloneHash<TKey, TValue>(Dictionary<TKey, TValue> hash, object parent)
	{
		if (hash == null)
		{
			throw new ArgumentNullException("hash");
		}
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(hash.Count);
		foreach (KeyValuePair<TKey, TValue> item in hash)
		{
			TValue val = item.Value;
			if (val is ICloneParent cloneParent)
			{
				val = (TValue)cloneParent.Clone(parent);
			}
			TKey val2 = item.Key;
			if (val2 is ICloneParent cloneParent2)
			{
				val2 = (TKey)cloneParent2.Clone(parent);
			}
			dictionary.Add(val2, val);
		}
		return dictionary;
	}

	public static Stream CloneStream(Stream stream)
	{
		if (stream == null)
		{
			return null;
		}
		long position = stream.Position;
		MemoryStream memoryStream = new MemoryStream((int)stream.Length);
		stream.Position = 0L;
		byte[] buffer = new byte[32768];
		int count;
		while ((count = stream.Read(buffer, 0, 32768)) != 0)
		{
			memoryStream.Write(buffer, 0, count);
		}
		stream.Position = position;
		memoryStream.Position = position;
		return memoryStream;
	}

	public static bool[] CloneBoolArray(bool[] sourceArray)
	{
		bool[] array = null;
		if (sourceArray != null)
		{
			int num = sourceArray.Length;
			array = new bool[num];
			if (num > 0)
			{
				Buffer.BlockCopy(sourceArray, 0, array, 0, num);
			}
		}
		return array;
	}
}
