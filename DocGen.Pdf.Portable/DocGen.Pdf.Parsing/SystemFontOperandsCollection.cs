using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontOperandsCollection
{
	private readonly LinkedList<object> store;

	public int Count => store.Count;

	public object First => SystemFontEnumerable.First(store);

	public object Last => SystemFontEnumerable.Last(store);

	public SystemFontOperandsCollection()
	{
		store = new LinkedList<object>();
	}

	public object GetElementAt(SystemFontOrigin origin, int index)
	{
		if (origin == SystemFontOrigin.Begin)
		{
			LinkedListNode<object> linkedListNode = store.First;
			for (int i = 0; i < index; i++)
			{
				linkedListNode = linkedListNode.Next;
			}
			return linkedListNode.Value;
		}
		LinkedListNode<object> linkedListNode2 = store.Last;
		for (int j = 0; j < index; j++)
		{
			linkedListNode2 = linkedListNode2.Previous;
		}
		return linkedListNode2.Value;
	}

	public void AddLast(object obj)
	{
		store.AddLast(obj);
	}

	public void AddFirst(object obj)
	{
		store.AddFirst(obj);
	}

	public object GetLast()
	{
		object value = store.Last.Value;
		store.RemoveLast();
		return value;
	}

	public T GetLastAs<T>()
	{
		return (T)GetLast();
	}

	public int GetLastAsInt()
	{
		SystemFontHelper.UnboxInteger(GetLast(), out var res);
		return res;
	}

	public double GetLastAsReal()
	{
		SystemFontHelper.UnboxReal(GetLast(), out var res);
		return res;
	}

	public object GetFirst()
	{
		object value = store.First.Value;
		store.RemoveFirst();
		return value;
	}

	public T GetFirstAs<T>()
	{
		return (T)GetFirst();
	}

	public int GetFirstAsInt()
	{
		SystemFontHelper.UnboxInteger(GetFirst(), out var res);
		return res;
	}

	public double GetFirstAsReal()
	{
		SystemFontHelper.UnboxReal(GetFirst(), out var res);
		return res;
	}

	public void Clear()
	{
		store.Clear();
	}
}
