using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public class SKRuntimeEffectUniforms : IEnumerable<string>, IEnumerable
{
	internal struct Variable
	{
		public int Index { get; set; }

		public string Name { get; set; }

		public int Offset { get; set; }

		public int Size { get; set; }
	}

	private readonly string[] names;

	private readonly Dictionary<string, Variable> uniforms;

	private SKData data;

	public IReadOnlyList<string> Names => names;

	internal IReadOnlyList<Variable> Variables => uniforms.Values.OrderBy((Variable v) => v.Index).ToArray();

	public int Count => names.Length;

	public SKRuntimeEffectUniform this[string name]
	{
		set
		{
			Add(name, value);
		}
	}

	public SKRuntimeEffectUniforms(SKRuntimeEffect effect)
	{
		if (effect == null)
		{
			throw new ArgumentNullException("effect");
		}
		names = effect.Uniforms.ToArray();
		uniforms = new Dictionary<string, Variable>();
		int uniformSize = effect.UniformSize;
		data = ((uniformSize > 0) ? SKData.Create(effect.UniformSize) : SKData.Empty);
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			IntPtr variable = SkiaApi.sk_runtimeeffect_get_uniform_from_index(effect.Handle, i);
			uniforms[text] = new Variable
			{
				Index = i,
				Name = text,
				Offset = (int)SkiaApi.sk_runtimeeffect_uniform_get_offset(variable),
				Size = (int)SkiaApi.sk_runtimeeffect_uniform_get_size_in_bytes(variable)
			};
		}
	}

	public void Reset()
	{
		if (data.Size != 0L)
		{
			data = SKData.Create(data.Size);
		}
	}

	public bool Contains(string name)
	{
		return Array.IndexOf(names, name) != -1;
	}

	public void Add(string name, SKRuntimeEffectUniform value)
	{
		int num = Array.IndexOf(names, name);
		if (num == -1)
		{
			throw new ArgumentOutOfRangeException(name, "Variable was not found for name: '" + name + "'.");
		}
		Variable variable = uniforms[name];
		Span<byte> span = data.Span.Slice(variable.Offset, variable.Size);
		if (value.IsEmpty)
		{
			span.Fill(0);
			return;
		}
		if (value.Size != variable.Size)
		{
			throw new ArgumentException($"Value size of {value.Size} does not match uniform size of {variable.Size}.", "value");
		}
		value.WriteTo(span);
	}

	public SKData ToData()
	{
		if (data.Size == 0L)
		{
			return SKData.Empty;
		}
		return SKData.CreateCopy(data.Data, data.Size);
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
