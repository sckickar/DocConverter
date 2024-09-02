using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Esprima;

[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}, Version = {_localVersion}")]
internal struct ArrayList<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
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

	private T[] _items;

	private int _count;

	private int Capacity
	{
		get
		{
			T[] items = _items;
			if (items == null)
			{
				return 0;
			}
			return items.Length;
		}
	}

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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (index < 0 || index >= _count)
			{
				ExceptionHelper.ThrowIndexOutOfRangeException();
			}
			_items[index] = value;
		}
	}

	public ArrayList(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity");
		}
		_items = ((initialCapacity > 0) ? new T[initialCapacity] : null);
		_count = 0;
	}

	internal void AddRange<TSource>(ArrayList<TSource> list) where TSource : T
	{
		int count = list.Count;
		if (count != 0)
		{
			int count2 = _count;
			int num = count2 + count;
			if (Capacity < num)
			{
				Resize(num);
			}
			Array.Copy(list._items, 0, _items, count2, count);
			_count = num;
		}
	}

	internal void Add(T item)
	{
		int capacity = Capacity;
		if (_count == capacity)
		{
			Resize(Math.Max(capacity * 2, 4));
		}
		_items[_count] = item;
		_count++;
	}

	internal void Resize(int size)
	{
		Array.Resize(ref _items, size);
	}

	[Conditional("DEBUG")]
	private void AssertUnchanged()
	{
	}

	[Conditional("DEBUG")]
	private void OnChanged()
	{
	}

	internal void RemoveAt(int index)
	{
		if (index < 0 || index >= _count)
		{
			throw new ArgumentOutOfRangeException("index", index, null);
		}
		_items[index] = default(T);
		_count--;
		if (index < _count - 1)
		{
			Array.Copy(_items, index + 1, _items, index, Count - index);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Push(T item)
	{
		Add(item);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal T Pop()
	{
		int index = _count - 1;
		T result = this[index];
		RemoveAt(index);
		return result;
	}

	public void Yield(out T[] items, out int count)
	{
		items = _items;
		count = _count;
		this = default(ArrayList<T>);
	}

	internal ArrayList<TResult> Select<TResult>(Func<T, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		ArrayList<TResult> arrayList = default(ArrayList<TResult>);
		arrayList._count = Count;
		arrayList._items = new TResult[Count];
		ArrayList<TResult> result = arrayList;
		for (int i = 0; i < Count; i++)
		{
			result._items[i] = selector(_items[i]);
		}
		return result;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_items, _count);
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
