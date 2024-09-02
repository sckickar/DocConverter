using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp;

internal struct HashCode
{
	private static readonly uint s_seed = GenerateGlobalSeed();

	private const uint Prime1 = 2654435761u;

	private const uint Prime2 = 2246822519u;

	private const uint Prime3 = 3266489917u;

	private const uint Prime4 = 668265263u;

	private const uint Prime5 = 374761393u;

	private uint _v1;

	private uint _v2;

	private uint _v3;

	private uint _v4;

	private uint _queue1;

	private uint _queue2;

	private uint _queue3;

	private uint _length;

	private static uint GenerateGlobalSeed()
	{
		Random random = new Random();
		return (uint)random.Next();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
	{
		v1 = (uint)((int)s_seed + -1640531535 + -2048144777);
		v2 = s_seed + 2246822519u;
		v3 = s_seed;
		v4 = s_seed - 2654435761u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Round(uint hash, uint input)
	{
		return RotateLeft(hash + (uint)((int)input * -2048144777), 13) * 2654435761u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint QueueRound(uint hash, uint queuedValue)
	{
		return RotateLeft(hash + (uint)((int)queuedValue * -1028477379), 17) * 668265263;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint MixState(uint v1, uint v2, uint v3, uint v4)
	{
		return RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint RotateLeft(uint value, int offset)
	{
		return (value << offset) | (value >> 32 - offset);
	}

	private static uint MixEmptyState()
	{
		return s_seed + 374761393;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint MixFinal(uint hash)
	{
		hash ^= hash >> 15;
		hash *= 2246822519u;
		hash ^= hash >> 13;
		hash *= 3266489917u;
		hash ^= hash >> 16;
		return hash;
	}

	public unsafe void Add(void* value)
	{
		Add((value != null) ? ((IntPtr)value).GetHashCode() : 0);
	}

	public void Add<T>(T value)
	{
		Add(value?.GetHashCode() ?? 0);
	}

	private void Add(int value)
	{
		uint num = _length++;
		switch (num % 4)
		{
		case 0u:
			_queue1 = (uint)value;
			return;
		case 1u:
			_queue2 = (uint)value;
			return;
		case 2u:
			_queue3 = (uint)value;
			return;
		}
		if (num == 3)
		{
			Initialize(out _v1, out _v2, out _v3, out _v4);
		}
		_v1 = Round(_v1, _queue1);
		_v2 = Round(_v2, _queue2);
		_v3 = Round(_v3, _queue3);
		_v4 = Round(_v4, (uint)value);
	}

	public int ToHashCode()
	{
		uint length = _length;
		uint num = length % 4;
		uint num2 = ((length < 4) ? MixEmptyState() : MixState(_v1, _v2, _v3, _v4));
		num2 += length * 4;
		if (num != 0)
		{
			num2 = QueueRound(num2, _queue1);
			if (num > 1)
			{
				num2 = QueueRound(num2, _queue2);
				if (num > 2)
				{
					num2 = QueueRound(num2, _queue3);
				}
			}
		}
		return (int)MixFinal(num2);
	}
}
