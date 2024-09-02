using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Esprima.Utils;

internal sealed class JsonTextWriter : JsonWriter
{
	private enum StructureKind : byte
	{
		Array,
		Object
	}

	private enum TokenKind
	{
		Scalar,
		String,
		Structure
	}

	[DebuggerDisplay("Count = {Count}")]
	private struct Stack<T> : IEnumerable<T>, IEnumerable
	{
		private T[] _items;

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

		public int Count { get; private set; }

		public ref T Top
		{
			get
			{
				if (Count <= 0)
				{
					throw new InvalidOperationException();
				}
				return ref _items[Count - 1];
			}
		}

		public Stack(int capacity)
		{
			_items = new T[capacity];
			Count = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return _items[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Push(T item)
		{
			int capacity = Capacity;
			if (Count == capacity)
			{
				Array.Resize(ref _items, Math.Max(capacity * 2, 4));
			}
			_items[Count] = item;
			Count++;
		}

		public T Pop()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException();
			}
			T top = Top;
			Top = default(T);
			Count--;
			return top;
		}
	}

	private readonly TextWriter _writer;

	private readonly string _indent;

	private Stack<StructureKind> _structures;

	private Stack<int> _counters;

	private string _memberName;

	public int Depth => _structures.Count;

	private bool Pretty => !string.IsNullOrEmpty(_indent);

	public JsonTextWriter(TextWriter writer)
		: this(writer, null)
	{
	}

	public JsonTextWriter(TextWriter writer, string indent)
	{
		_writer = writer ?? throw new ArgumentNullException("writer");
		_indent = indent;
		_counters = new Stack<int>(8);
		_structures = new Stack<StructureKind>(8);
	}

	public override void StartObject()
	{
		StartStructured(StructureKind.Object);
	}

	public override void EndObject()
	{
		EndStructured();
	}

	public override void StartArray()
	{
		StartStructured(StructureKind.Array);
	}

	public override void EndArray()
	{
		EndStructured();
	}

	public override void Member(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (Depth == 0 || _structures.Top != StructureKind.Object)
		{
			throw new InvalidOperationException("Member must be within an object.");
		}
		if (_memberName != null)
		{
			throw new InvalidOperationException("Missing value for member: " + _memberName);
		}
		_memberName = name;
	}

	public override void String(string str)
	{
		if (str == null)
		{
			Null();
		}
		else
		{
			Write(str, TokenKind.String);
		}
	}

	public override void Null()
	{
		Write("null", TokenKind.Scalar);
	}

	public override void Boolean(bool flag)
	{
		Write(flag ? "true" : "false", TokenKind.Scalar);
	}

	public void Number(int n)
	{
		Write(n.ToString(CultureInfo.InvariantCulture), TokenKind.Scalar);
	}

	public override void Number(long n)
	{
		Write(n.ToString(CultureInfo.InvariantCulture), TokenKind.Scalar);
	}

	public override void Number(double n)
	{
		if (double.IsNaN(n) || double.IsInfinity(n))
		{
			throw new ArgumentOutOfRangeException("n", n, null);
		}
		Write(n.ToString(CultureInfo.InvariantCulture), TokenKind.Scalar);
	}

	private void Eol()
	{
		if (Pretty)
		{
			_writer.WriteLine();
		}
	}

	private void Indent(int? depth = null)
	{
		if (Pretty)
		{
			int num = depth ?? Depth;
			for (int i = 0; i < num; i++)
			{
				_writer.Write(_indent);
			}
		}
	}

	private void StartStructured(StructureKind kind)
	{
		Write((kind == StructureKind.Array) ? "[" : "{", TokenKind.Structure);
		_counters.Push(0);
		_structures.Push(kind);
	}

	private void EndStructured()
	{
		if (Depth == 0)
		{
			throw new InvalidOperationException("No JSON structure in effect.");
		}
		if (_memberName != null)
		{
			throw new InvalidOperationException("Missing value for member: " + _memberName);
		}
		if (_counters.Top > 0)
		{
			Eol();
			Indent(Depth - 1);
		}
		_writer.Write((_structures.Pop() == StructureKind.Array) ? "]" : "}");
		_counters.Pop();
	}

	private void Write(string token, TokenKind kind)
	{
		if (Depth == 0 && kind == TokenKind.Scalar)
		{
			throw new InvalidOperationException("JSON text must start with an object or an array.");
		}
		TextWriter writer = _writer;
		if (Depth > 0)
		{
			if (_structures.Top == StructureKind.Object && _memberName == null)
			{
				throw new InvalidOperationException("JSON object member name is undefined.");
			}
			if (_counters.Top > 0)
			{
				writer.Write(',');
			}
			Eol();
		}
		string memberName = _memberName;
		_memberName = null;
		if (memberName != null)
		{
			Indent();
			Enquote(memberName, writer);
			writer.Write(Pretty ? ": " : ":");
		}
		if (Depth > 0 && _structures.Top == StructureKind.Array)
		{
			Indent();
		}
		if (kind == TokenKind.String)
		{
			Enquote(token, writer);
		}
		else
		{
			writer.Write(token);
		}
		if (Depth > 0)
		{
			_counters.Top++;
		}
	}

	private static void Enquote(string s, TextWriter writer)
	{
		int length = (s ?? string.Empty).Length;
		writer.Write('"');
		for (int i = 0; i < length; i++)
		{
			char c = s[i];
			switch (c)
			{
			case '"':
			case '\\':
				writer.Write('\\');
				writer.Write(c);
				continue;
			case '\b':
				writer.Write("\\b");
				continue;
			case '\t':
				writer.Write("\\t");
				continue;
			case '\n':
				writer.Write("\\n");
				continue;
			case '\f':
				writer.Write("\\f");
				continue;
			case '\r':
				writer.Write("\\r");
				continue;
			}
			if (c < ' ')
			{
				writer.Write("\\u");
				int num = c;
				writer.Write(num.ToString("x4", CultureInfo.InvariantCulture));
			}
			else
			{
				writer.Write(c);
			}
		}
		writer.Write('"');
	}
}
