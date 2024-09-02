using System;

namespace SkiaSharp;

public class SKColorSpaceIccProfile : SKObject
{
	public unsafe long Size
	{
		get
		{
			uint num = default(uint);
			SkiaApi.sk_colorspace_icc_profile_get_buffer(Handle, &num);
			return num;
		}
	}

	public unsafe IntPtr Buffer => (IntPtr)SkiaApi.sk_colorspace_icc_profile_get_buffer(Handle, null);

	internal SKColorSpaceIccProfile(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKColorSpaceIccProfile()
		: this(SkiaApi.sk_colorspace_icc_profile_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKColorSpaceIccProfile instance.");
		}
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_colorspace_icc_profile_delete(Handle);
	}

	public unsafe bool ToColorSpaceXyz(out SKColorSpaceXyz toXyzD50)
	{
		fixed (SKColorSpaceXyz* toXYZD = &toXyzD50)
		{
			return SkiaApi.sk_colorspace_icc_profile_get_to_xyzd50(Handle, toXYZD);
		}
	}

	public SKColorSpaceXyz ToColorSpaceXyz()
	{
		if (!ToColorSpaceXyz(out var toXyzD))
		{
			return SKColorSpaceXyz.Empty;
		}
		return toXyzD;
	}

	public static SKColorSpaceIccProfile Create(byte[] data)
	{
		return Create(data.AsSpan());
	}

	public static SKColorSpaceIccProfile Create(ReadOnlySpan<byte> data)
	{
		if (data.IsEmpty)
		{
			return null;
		}
		SKData sKData = SKData.CreateCopy(data);
		SKColorSpaceIccProfile sKColorSpaceIccProfile = Create(sKData);
		if (sKColorSpaceIccProfile == null)
		{
			sKData.Dispose();
		}
		return sKColorSpaceIccProfile;
	}

	public static SKColorSpaceIccProfile Create(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.IsEmpty)
		{
			return null;
		}
		return SKObject.Referenced(Create(data.Data, data.Size), data);
	}

	public unsafe static SKColorSpaceIccProfile Create(IntPtr data, long length)
	{
		if (data == IntPtr.Zero)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			return null;
		}
		SKColorSpaceIccProfile sKColorSpaceIccProfile = new SKColorSpaceIccProfile();
		if (!SkiaApi.sk_colorspace_icc_profile_parse((void*)data, (IntPtr)length, sKColorSpaceIccProfile.Handle))
		{
			sKColorSpaceIccProfile.Dispose();
			sKColorSpaceIccProfile = null;
		}
		return sKColorSpaceIccProfile;
	}
}
