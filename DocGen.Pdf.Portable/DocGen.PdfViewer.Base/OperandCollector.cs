using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class OperandCollector
{
	private readonly LinkedList<object> store;

	public int Count => store.Count;

	public object First => Countable.First(store);

	public object Last => Countable.Last(store);

	public OperandCollector()
	{
		store = new LinkedList<object>();
	}

	public object GetElementAt(Origin origin, int index)
	{
		if (origin == Origin.Begin)
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
		Helper.ParseInteger(GetLast(), out var res);
		return res;
	}

	public double GetLastAsReal()
	{
		Helper.ParseReal(GetLast(), out var res);
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
		Helper.ParseInteger(GetFirst(), out var res);
		return res;
	}

	public double GetFirstAsReal()
	{
		Helper.ParseReal(GetFirst(), out var res);
		return res;
	}

	public void Clear()
	{
		store.Clear();
	}
}
