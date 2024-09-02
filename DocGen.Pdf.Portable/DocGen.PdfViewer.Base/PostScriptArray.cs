using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DocGen.PdfViewer.Base;

internal class PostScriptArray : PostScriptObj, IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable
{
	private readonly List<object> store;

	public static PostScriptArray MatrixIdentity => new PostScriptArray(new object[6] { 1, 0, 0, 1, 0, 0 });

	public object this[int index]
	{
		get
		{
			return store[index];
		}
		set
		{
			store[index] = value;
		}
	}

	public int Count => store.Count;

	public bool IsReadOnly => false;

	public PostScriptArray()
	{
		store = new List<object>();
	}

	public PostScriptArray(int capacity)
	{
		store = new List<object>(capacity);
		for (int i = 0; i < capacity; i++)
		{
			store.Add(null);
		}
	}

	public PostScriptArray(object[] initialValue)
	{
		store = new List<object>(initialValue);
	}

	public Matrix ToMatrix()
	{
		Helper.ParseReal(store[0], out var res);
		Helper.ParseReal(store[1], out var res2);
		Helper.ParseReal(store[2], out var res3);
		Helper.ParseReal(store[3], out var res4);
		Helper.ParseReal(store[4], out var res5);
		Helper.ParseReal(store[5], out var res6);
		return new Matrix(res, res2, res3, res4, res5, res6);
	}

	public int IndexOf(object item)
	{
		return store.IndexOf(item);
	}

	public void Insert(int index, object item)
	{
		store.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		store.RemoveAt(index);
	}

	public void Add(object item)
	{
		store.Add(item);
	}

	public void Clear()
	{
		store.Clear();
	}

	public bool Contains(object item)
	{
		return store.Contains(item);
	}

	public void CopyTo(object[] array, int arrayIndex)
	{
		store.CopyTo(array, arrayIndex);
	}

	public bool Remove(object item)
	{
		return store.Remove(item);
	}

	public IEnumerator<object> GetEnumerator()
	{
		return store.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return store.GetEnumerator();
	}

	public void Load(object[] content)
	{
		foreach (object item in content)
		{
			store.Add(item);
		}
	}

	public T GetElementAs<T>(int index)
	{
		return (T)store[index];
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object item in store)
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(item);
		}
		stringBuilder.Remove(0, 1);
		stringBuilder.Append("]");
		stringBuilder.Insert(0, "[");
		return stringBuilder.ToString();
	}
}
