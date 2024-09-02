using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public class SKRuntimeEffect : SKObject, ISKReferenceCounted
{
	private string[] children;

	private string[] uniforms;

	public int UniformSize => (int)SkiaApi.sk_runtimeeffect_get_uniform_size(Handle);

	public IReadOnlyList<string> Children => children ?? (children = GetChildrenNames().ToArray());

	public IReadOnlyList<string> Uniforms => uniforms ?? (uniforms = GetUniformNames().ToArray());

	internal SKRuntimeEffect(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public static SKRuntimeEffect Create(string sksl, out string errors)
	{
		using SKString sKString = new SKString(sksl);
		using SKString sKString2 = new SKString();
		SKRuntimeEffect @object = GetObject(SkiaApi.sk_runtimeeffect_make(sKString.Handle, sKString2.Handle));
		errors = sKString2?.ToString();
		string obj = errors;
		if (obj != null && obj.Length == 0)
		{
			errors = null;
		}
		return @object;
	}

	private IEnumerable<string> GetChildrenNames()
	{
		int count = (int)SkiaApi.sk_runtimeeffect_get_children_count(Handle);
		using SKString str = new SKString();
		for (int i = 0; i < count; i++)
		{
			SkiaApi.sk_runtimeeffect_get_child_name(Handle, i, str.Handle);
			yield return str.ToString();
		}
	}

	private IEnumerable<string> GetUniformNames()
	{
		int count = (int)SkiaApi.sk_runtimeeffect_get_uniforms_count(Handle);
		using SKString str = new SKString();
		for (int i = 0; i < count; i++)
		{
			SkiaApi.sk_runtimeeffect_get_uniform_name(Handle, i, str.Handle);
			yield return str.ToString();
		}
	}

	public unsafe SKShader ToShader(bool isOpaque)
	{
		return ToShader(isOpaque, null, null, null);
	}

	public unsafe SKShader ToShader(bool isOpaque, SKRuntimeEffectUniforms uniforms)
	{
		return ToShader(isOpaque, uniforms.ToData(), null, null);
	}

	public unsafe SKShader ToShader(bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children)
	{
		return ToShader(isOpaque, uniforms.ToData(), children.ToArray(), null);
	}

	public unsafe SKShader ToShader(bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix)
	{
		return ToShader(isOpaque, uniforms.ToData(), children.ToArray(), &localMatrix);
	}

	private unsafe SKShader ToShader(bool isOpaque, SKData uniforms, SKShader[] children, SKMatrix* localMatrix)
	{
		IntPtr intPtr = uniforms?.Handle ?? IntPtr.Zero;
		Utils.RentedArray<IntPtr> rentedArray = Utils.RentHandlesArray(children, nullIfEmpty: true);
		try
		{
			fixed (IntPtr* ptr = rentedArray)
			{
				return SKShader.GetObject(SkiaApi.sk_runtimeeffect_make_shader(Handle, intPtr, ptr, (IntPtr)rentedArray.Length, localMatrix, isOpaque));
			}
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	public SKColorFilter ToColorFilter()
	{
		return ToColorFilter((SKData)null, (SKShader[])null);
	}

	public SKColorFilter ToColorFilter(SKRuntimeEffectUniforms uniforms)
	{
		return ToColorFilter(uniforms.ToData(), null);
	}

	private SKColorFilter ToColorFilter(SKData uniforms)
	{
		return ToColorFilter(uniforms, null);
	}

	public SKColorFilter ToColorFilter(SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children)
	{
		return ToColorFilter(uniforms.ToData(), children.ToArray());
	}

	private unsafe SKColorFilter ToColorFilter(SKData uniforms, SKShader[] children)
	{
		IntPtr intPtr = uniforms?.Handle ?? IntPtr.Zero;
		Utils.RentedArray<IntPtr> rentedArray = Utils.RentHandlesArray(children, nullIfEmpty: true);
		try
		{
			fixed (IntPtr* ptr = rentedArray)
			{
				return SKColorFilter.GetObject(SkiaApi.sk_runtimeeffect_make_color_filter(Handle, intPtr, ptr, (IntPtr)rentedArray.Length));
			}
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal static SKRuntimeEffect GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKRuntimeEffect(h, o));
	}
}
