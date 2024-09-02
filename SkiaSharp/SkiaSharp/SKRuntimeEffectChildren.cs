using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public class SKRuntimeEffectChildren : IEnumerable<string>, IEnumerable
{
	private readonly string[] names;

	private readonly SKShader[] children;

	public IReadOnlyList<string> Names => names;

	public int Count => names.Length;

	public SKShader this[string name]
	{
		set
		{
			Add(name, value);
		}
	}

	public SKRuntimeEffectChildren(SKRuntimeEffect effect)
	{
		if (effect == null)
		{
			throw new ArgumentNullException("effect");
		}
		names = effect.Children.ToArray();
		children = new SKShader[names.Length];
	}

	public void Reset()
	{
		Array.Clear(children, 0, children.Length);
	}

	public bool Contains(string name)
	{
		return Array.IndexOf(names, name) != -1;
	}

	public void Add(string name, SKShader value)
	{
		int num = Array.IndexOf(names, name);
		if (num == -1)
		{
			throw new ArgumentOutOfRangeException(name, "Variable was not found for name: '" + name + "'.");
		}
		children[num] = value;
	}

	public SKShader[] ToArray()
	{
		return children.ToArray();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<string> GetEnumerator()
	{
		return ((IEnumerable<string>)names).GetEnumerator();
	}
}
