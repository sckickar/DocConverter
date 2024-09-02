using System;

namespace SkiaSharp;

public abstract class SKStream : SKObject
{
	public bool IsAtEnd => SkiaApi.sk_stream_is_at_end(Handle);

	public bool HasPosition => SkiaApi.sk_stream_has_position(Handle);

	public int Position
	{
		get
		{
			return (int)SkiaApi.sk_stream_get_position(Handle);
		}
		set
		{
			Seek(value);
		}
	}

	public bool HasLength => SkiaApi.sk_stream_has_length(Handle);

	public int Length => (int)SkiaApi.sk_stream_get_length(Handle);

	internal SKStream(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public sbyte ReadSByte()
	{
		if (ReadSByte(out var buffer))
		{
			return buffer;
		}
		return 0;
	}

	public short ReadInt16()
	{
		if (ReadInt16(out var buffer))
		{
			return buffer;
		}
		return 0;
	}

	public int ReadInt32()
	{
		if (ReadInt32(out var buffer))
		{
			return buffer;
		}
		return 0;
	}

	public byte ReadByte()
	{
		if (ReadByte(out var buffer))
		{
			return buffer;
		}
		return 0;
	}

	public ushort ReadUInt16()
	{
		if (ReadUInt16(out var buffer))
		{
			return buffer;
		}
		return 0;
	}

	public uint ReadUInt32()
	{
		if (ReadUInt32(out var buffer))
		{
			return buffer;
		}
		return 0u;
	}

	public bool ReadBool()
	{
		if (ReadBool(out var buffer))
		{
			return buffer;
		}
		return false;
	}

	public unsafe bool ReadSByte(out sbyte buffer)
	{
		fixed (sbyte* buffer2 = &buffer)
		{
			return SkiaApi.sk_stream_read_s8(Handle, buffer2);
		}
	}

	public unsafe bool ReadInt16(out short buffer)
	{
		fixed (short* buffer2 = &buffer)
		{
			return SkiaApi.sk_stream_read_s16(Handle, buffer2);
		}
	}

	public unsafe bool ReadInt32(out int buffer)
	{
		fixed (int* buffer2 = &buffer)
		{
			return SkiaApi.sk_stream_read_s32(Handle, buffer2);
		}
	}

	public unsafe bool ReadByte(out byte buffer)
	{
		fixed (byte* buffer2 = &buffer)
		{
			return SkiaApi.sk_stream_read_u8(Handle, buffer2);
		}
	}

	public unsafe bool ReadUInt16(out ushort buffer)
	{
		fixed (ushort* buffer2 = &buffer)
		{
			return SkiaApi.sk_stream_read_u16(Handle, buffer2);
		}
	}

	public unsafe bool ReadUInt32(out uint buffer)
	{
		fixed (uint* buffer2 = &buffer)
		{
			return SkiaApi.sk_stream_read_u32(Handle, buffer2);
		}
	}

	public unsafe bool ReadBool(out bool buffer)
	{
		byte b = default(byte);
		bool result = SkiaApi.sk_stream_read_bool(Handle, &b);
		buffer = b > 0;
		return result;
	}

	public unsafe int Read(byte[] buffer, int size)
	{
		fixed (byte* ptr = buffer)
		{
			return Read((IntPtr)ptr, size);
		}
	}

	public unsafe int Read(IntPtr buffer, int size)
	{
		return (int)SkiaApi.sk_stream_read(Handle, (void*)buffer, (IntPtr)size);
	}

	public unsafe int Peek(IntPtr buffer, int size)
	{
		return (int)SkiaApi.sk_stream_peek(Handle, (void*)buffer, (IntPtr)size);
	}

	public int Skip(int size)
	{
		return (int)SkiaApi.sk_stream_skip(Handle, (IntPtr)size);
	}

	public bool Rewind()
	{
		return SkiaApi.sk_stream_rewind(Handle);
	}

	public bool Seek(int position)
	{
		return SkiaApi.sk_stream_seek(Handle, (IntPtr)position);
	}

	public bool Move(long offset)
	{
		return Move((int)offset);
	}

	public bool Move(int offset)
	{
		return SkiaApi.sk_stream_move(Handle, offset);
	}

	public unsafe IntPtr GetMemoryBase()
	{
		return (IntPtr)SkiaApi.sk_stream_get_memory_base(Handle);
	}

	internal SKStream Fork()
	{
		return GetObject(SkiaApi.sk_stream_fork(Handle));
	}

	internal SKStream Duplicate()
	{
		return GetObject(SkiaApi.sk_stream_duplicate(Handle));
	}

	internal static SKStream GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (Func<IntPtr, bool, SKStream>)((IntPtr h, bool o) => new SKStreamImplementation(h, o)));
	}
}
