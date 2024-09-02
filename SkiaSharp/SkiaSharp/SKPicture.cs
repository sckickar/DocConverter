using System;
using System.IO;

namespace SkiaSharp;

public class SKPicture : SKObject, ISKReferenceCounted
{
	public uint UniqueId => SkiaApi.sk_picture_get_unique_id(Handle);

	public unsafe SKRect CullRect
	{
		get
		{
			SKRect result = default(SKRect);
			SkiaApi.sk_picture_get_cull_rect(Handle, &result);
			return result;
		}
	}

	internal SKPicture(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public SKData Serialize()
	{
		return SKData.GetObject(SkiaApi.sk_picture_serialize_to_data(Handle));
	}

	public void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKManagedWStream stream2 = new SKManagedWStream(stream);
		Serialize(stream2);
	}

	public void Serialize(SKWStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SkiaApi.sk_picture_serialize_to_stream(Handle, stream.Handle);
	}

	public SKShader ToShader()
	{
		return ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy)
	{
		return SKShader.GetObject(SkiaApi.sk_picture_make_shader(Handle, tmx, tmy, null, null));
	}

	public unsafe SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile)
	{
		return SKShader.GetObject(SkiaApi.sk_picture_make_shader(Handle, tmx, tmy, null, &tile));
	}

	public unsafe SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile)
	{
		return SKShader.GetObject(SkiaApi.sk_picture_make_shader(Handle, tmx, tmy, &localMatrix, &tile));
	}

	public unsafe static SKPicture Deserialize(IntPtr data, int length)
	{
		if (data == IntPtr.Zero)
		{
			throw new ArgumentNullException("data");
		}
		if (length == 0)
		{
			return null;
		}
		return GetObject(SkiaApi.sk_picture_deserialize_from_memory((void*)data, (IntPtr)length));
	}

	public unsafe static SKPicture Deserialize(ReadOnlySpan<byte> data)
	{
		if (data.Length == 0)
		{
			return null;
		}
		fixed (byte* ptr = data)
		{
			void* buffer = ptr;
			return GetObject(SkiaApi.sk_picture_deserialize_from_memory(buffer, (IntPtr)data.Length));
		}
	}

	public static SKPicture Deserialize(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return GetObject(SkiaApi.sk_picture_deserialize_from_data(data.Handle));
	}

	public static SKPicture Deserialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKManagedStream stream2 = new SKManagedStream(stream);
		return Deserialize(stream2);
	}

	public static SKPicture Deserialize(SKStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return GetObject(SkiaApi.sk_picture_deserialize_from_stream(stream.Handle));
	}

	internal static SKPicture GetObject(IntPtr handle, bool owns = true, bool unrefExisting = true)
	{
		return SKObject.GetOrAddObject(handle, owns, unrefExisting, (IntPtr h, bool o) => new SKPicture(h, o));
	}
}
