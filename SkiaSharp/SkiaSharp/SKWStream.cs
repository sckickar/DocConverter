using System;

namespace SkiaSharp;

public abstract class SKWStream : SKObject
{
	public virtual int BytesWritten => (int)SkiaApi.sk_wstream_bytes_written(Handle);

	internal SKWStream(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public unsafe virtual bool Write(byte[] buffer, int size)
	{
		fixed (byte* buffer2 = buffer)
		{
			return SkiaApi.sk_wstream_write(Handle, buffer2, (IntPtr)size);
		}
	}

	public bool NewLine()
	{
		return SkiaApi.sk_wstream_newline(Handle);
	}

	public virtual void Flush()
	{
		SkiaApi.sk_wstream_flush(Handle);
	}

	public bool Write8(byte value)
	{
		return SkiaApi.sk_wstream_write_8(Handle, value);
	}

	public bool Write16(ushort value)
	{
		return SkiaApi.sk_wstream_write_16(Handle, value);
	}

	public bool Write32(uint value)
	{
		return SkiaApi.sk_wstream_write_32(Handle, value);
	}

	public bool WriteText(string value)
	{
		return SkiaApi.sk_wstream_write_text(Handle, value);
	}

	public bool WriteDecimalAsTest(int value)
	{
		return SkiaApi.sk_wstream_write_dec_as_text(Handle, value);
	}

	public bool WriteBigDecimalAsText(long value, int digits)
	{
		return SkiaApi.sk_wstream_write_bigdec_as_text(Handle, value, digits);
	}

	public bool WriteHexAsText(uint value, int digits)
	{
		return SkiaApi.sk_wstream_write_hex_as_text(Handle, value, digits);
	}

	public bool WriteScalarAsText(float value)
	{
		return SkiaApi.sk_wstream_write_scalar_as_text(Handle, value);
	}

	public bool WriteBool(bool value)
	{
		return SkiaApi.sk_wstream_write_bool(Handle, value);
	}

	public bool WriteScalar(float value)
	{
		return SkiaApi.sk_wstream_write_scalar(Handle, value);
	}

	public bool WritePackedUInt32(uint value)
	{
		return SkiaApi.sk_wstream_write_packed_uint(Handle, (IntPtr)value);
	}

	public bool WriteStream(SKStream input, int length)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		return SkiaApi.sk_wstream_write_stream(Handle, input.Handle, (IntPtr)length);
	}

	public static int GetSizeOfPackedUInt32(uint value)
	{
		return SkiaApi.sk_wstream_get_size_of_packed_uint((IntPtr)value);
	}
}
