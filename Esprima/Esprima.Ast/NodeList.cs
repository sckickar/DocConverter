using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public readonly struct NodeList<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T> where T : class, INode
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private int _index;

		private T[] _items;

		private int _count;

		private bool IsDisposed => _count < 0;

		public T Current
		{
			get
			{
				ThrowIfDisposed();
				if (_index < 0)
				{
					return ExceptionHelper.ThrowInvalidOperationException<T>();
				}
				return _items[_index];
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(T[] items, int count)
		{
			this = default(Enumerator);
			_index = -1;
			_items = items;
			_count = count;
		}

		public void Dispose()
		{
			_items = null;
			_count = -1;
		}

		public bool MoveNext()
		{
			ThrowIfDisposed();
			if (_index + 1 == _count)
			{
				return false;
			}
			_index++;
			return true;
		}

		public void Reset()
		{
			ThrowIfDisposed();
			_index = -1;
		}

		private void ThrowIfDisposed()
		{
			if (IsDisposed)
			{
				ExceptionHelper.ThrowObjectDisposedException("Enumerator");
			}
		}
	}

	private readonly T[] _items;

	private readonly int _count;

	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _count;
		}
	}

	public T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if ((uint)index >= (uint)_count)
			{
				ExceptionHelper.ThrowIndexOutOfRangeException();
			}
			return _items[index];
		}
	}

	internal NodeList(ICollection<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		int num = (_count = collection.Count);
		if ((_items = ((num == 0) ? null : new T[num])) != null)
		{
			collection.CopyTo(_items, 0);
		}
	}

	internal NodeList(T[] items, int count)
	{
		_items = items;
		_count = count;
	}

	public NodeList<INode> AsNodes()
	{
		INode[] items = _items;
		return new NodeList<INode>(items, _count);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_items, Count);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
public static class NodeList
{
	internal static NodeList<T> From<T>(ref ArrayList<T> arrayList) where T : class, INode
	{
		arrayList.Yield(out var items, out var count);
		arrayList = default(ArrayList<T>);
		return new NodeList<T>(items, count);
	}

	public static NodeList<T> Create<T>(IEnumerable<T> source) where T : class, INode
	{
		if (source != null)
		{
			if (source is NodeList<T>)
			{
				return (NodeList<T>)(object)source;
			}
			if (!(source is ICollection<T> collection))
			{
				if (source is IReadOnlyList<T> readOnlyList)
				{
					if (readOnlyList.Count == 0)
					{
						return default(NodeList<T>);
					}
					ArrayList<T> arrayList = new ArrayList<T>(readOnlyList.Count);
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						arrayList.Add(readOnlyList[i]);
					}
					return From(ref arrayList);
				}
				int? num = ((source is IReadOnlyCollection<T> readOnlyCollection) ? new int?(readOnlyCollection.Count) : null);
				ArrayList<T> obj;
				if (num.HasValue)
				{
					int valueOrDefault = num.GetValueOrDefault();
					obj = new ArrayList<T>(valueOrDefault);
				}
				else
				{
					obj = default(ArrayList<T>);
				}
				ArrayList<T> arrayList2 = obj;
				if (!num.HasValue || num > 0)
				{
					foreach (T item in source)
					{
						arrayList2.Add(item);
					}
				}
				return From(ref arrayList2);
			}
			if (collection.Count <= 0)
			{
				return default(NodeList<T>);
			}
			return new NodeList<T>(collection);
		}
		throw new ArgumentNullException("source");
	}
}
