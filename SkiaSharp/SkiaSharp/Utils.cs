using System;
using System.Buffers;
using System.ComponentModel;
using System.Text;

namespace SkiaSharp;

internal static class Utils
{
	internal readonly ref struct RentedArray<T>
	{
		public readonly T[] Array;

		public readonly Span<T> Span;

		public int Length => Span.Length;

		public T this[int index]
		{
			get
			{
				return Span[index];
			}
			set
			{
				Span[index] = value;
			}
		}

		internal RentedArray(int length)
		{
			Array = ArrayPool<T>.Shared.Rent(length);
			Span = new Span<T>(Array, 0, length);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ref T GetPinnableReference()
		{
			return ref Span.GetPinnableReference();
		}

		public void Dispose()
		{
			if (Array != null)
			{
				ArrayPool<T>.Shared.Return(Array);
			}
		}

		public static explicit operator T[](RentedArray<T> scope)
		{
			return scope.Array;
		}

		public static implicit operator Span<T>(RentedArray<T> scope)
		{
			return scope.Span;
		}

		public static implicit operator ReadOnlySpan<T>(RentedArray<T> scope)
		{
			return scope.Span;
		}
	}

	internal const float NearlyZero = 0.00024414062f;

	internal static int GetPreambleSize(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ReadOnlySpan<byte> readOnlySpan = data.AsSpan();
		int length = readOnlySpan.Length;
		if (length >= 2 && readOnlySpan[0] == 254 && readOnlySpan[1] == byte.MaxValue)
		{
			return 2;
		}
		if (length >= 3 && readOnlySpan[0] == 239 && readOnlySpan[1] == 187 && readOnlySpan[2] == 191)
		{
			return 3;
		}
		if (length >= 3 && readOnlySpan[0] == 43 && readOnlySpan[1] == 47 && readOnlySpan[2] == 118)
		{
			return 3;
		}
		if (length >= 4 && readOnlySpan[0] == 0 && readOnlySpan[1] == 0 && readOnlySpan[2] == 254 && readOnlySpan[3] == byte.MaxValue)
		{
			return 4;
		}
		return 0;
	}

	internal unsafe static Span<byte> AsSpan(this IntPtr ptr, int size)
	{
		return new Span<byte>((void*)ptr, size);
	}

	internal unsafe static ReadOnlySpan<byte> AsReadOnlySpan(this IntPtr ptr, int size)
	{
		return new ReadOnlySpan<byte>((void*)ptr, size);
	}

	internal static bool NearlyEqual(float a, float b, float tolerance)
	{
		return Math.Abs(a - b) <= tolerance;
	}

	internal unsafe static byte[] GetBytes(this Encoding encoding, ReadOnlySpan<char> text)
	{
		if (text.Length == 0)
		{
			return new byte[0];
		}
		fixed (char* chars = text)
		{
			int byteCount = encoding.GetByteCount(chars, text.Length);
			if (byteCount == 0)
			{
				return new byte[0];
			}
			byte[] array = new byte[byteCount];
			fixed (byte* bytes = array)
			{
				encoding.GetBytes(chars, text.Length, bytes, byteCount);
			}
			return array;
		}
	}

	public static RentedArray<T> RentArray<T>(int length, bool nullIfEmpty = false)
	{
		if (!nullIfEmpty || length > 0)
		{
			return new RentedArray<T>(length);
		}
		return default(RentedArray<T>);
	}

	public static RentedArray<IntPtr> RentHandlesArray(SKObject[] objects, bool nullIfEmpty = false)
	{
		RentedArray<IntPtr> result = RentArray<IntPtr>((objects != null) ? objects.Length : 0, nullIfEmpty);
		for (int i = 0; i < result.Length; i++)
		{
			result[i] = objects[i]?.Handle ?? IntPtr.Zero;
		}
		return result;
	}
}
