using System;
using System.Collections.Generic;

namespace SkiaSharp;

public readonly ref struct SKRuntimeEffectUniform
{
	private enum DataType
	{
		Empty,
		Float,
		FloatArray
	}

	private readonly DataType type;

	private readonly int size;

	private readonly float floatValue;

	private readonly ReadOnlySpan<float> floatArray;

	public static SKRuntimeEffectUniform Empty => default(SKRuntimeEffectUniform);

	public bool IsEmpty => type == DataType.Empty;

	public int Size => size;

	private SKRuntimeEffectUniform(DataType type, int size, float floatValue = 0f, ReadOnlySpan<float> floatArray = default(ReadOnlySpan<float>))
	{
		this.type = type;
		this.size = size;
		this.floatValue = floatValue;
		this.floatArray = floatArray;
	}

	public static implicit operator SKRuntimeEffectUniform(float value)
	{
		return new SKRuntimeEffectUniform(DataType.Float, 4, value);
	}

	public static implicit operator SKRuntimeEffectUniform(float[] value)
	{
		return (ReadOnlySpan<float>)value;
	}

	public static implicit operator SKRuntimeEffectUniform(Span<float> value)
	{
		return (ReadOnlySpan<float>)value;
	}

	public static implicit operator SKRuntimeEffectUniform(ReadOnlySpan<float> value)
	{
		return new SKRuntimeEffectUniform(DataType.FloatArray, 4 * value.Length, 0f, value);
	}

	public static implicit operator SKRuntimeEffectUniform(float[][] value)
	{
		List<float> list = new List<float>();
		foreach (float[] collection in value)
		{
			list.AddRange(collection);
		}
		return list.ToArray();
	}

	public static implicit operator SKRuntimeEffectUniform(SKMatrix value)
	{
		return value.Values;
	}

	public unsafe void WriteTo(Span<byte> data)
	{
		switch (type)
		{
		case DataType.Float:
			fixed (float* ptr2 = &floatValue)
			{
				void* pointer2 = ptr2;
				new ReadOnlySpan<byte>(pointer2, size).CopyTo(data);
			}
			break;
		case DataType.FloatArray:
			fixed (float* ptr = floatArray)
			{
				void* pointer = ptr;
				new ReadOnlySpan<byte>(pointer, size).CopyTo(data);
			}
			break;
		default:
			data.Fill(0);
			break;
		}
	}
}
