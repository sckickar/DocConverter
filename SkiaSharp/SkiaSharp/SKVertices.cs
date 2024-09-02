using System;

namespace SkiaSharp;

public class SKVertices : SKObject, ISKNonVirtualReferenceCounted, ISKReferenceCounted, ISKSkipObjectRegistration
{
	internal SKVertices(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	void ISKNonVirtualReferenceCounted.ReferenceNative()
	{
		SkiaApi.sk_vertices_ref(Handle);
	}

	void ISKNonVirtualReferenceCounted.UnreferenceNative()
	{
		SkiaApi.sk_vertices_unref(Handle);
	}

	public static SKVertices CreateCopy(SKVertexMode vmode, SKPoint[] positions, SKColor[] colors)
	{
		return CreateCopy(vmode, positions, null, colors, null);
	}

	public static SKVertices CreateCopy(SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors)
	{
		return CreateCopy(vmode, positions, texs, colors, null);
	}

	public unsafe static SKVertices CreateCopy(SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, ushort[] indices)
	{
		if (positions == null)
		{
			throw new ArgumentNullException("positions");
		}
		if (texs != null && positions.Length != texs.Length)
		{
			throw new ArgumentException("The number of texture coordinates must match the number of vertices.", "texs");
		}
		if (colors != null && positions.Length != colors.Length)
		{
			throw new ArgumentException("The number of colors must match the number of vertices.", "colors");
		}
		int vertexCount = positions.Length;
		int indexCount = ((indices != null) ? indices.Length : 0);
		fixed (SKPoint* positions2 = positions)
		{
			fixed (SKPoint* texs2 = texs)
			{
				fixed (SKColor* colors2 = colors)
				{
					fixed (ushort* indices2 = indices)
					{
						return GetObject(SkiaApi.sk_vertices_make_copy(vmode, vertexCount, positions2, texs2, (uint*)colors2, indexCount, indices2));
					}
				}
			}
		}
	}

	internal static SKVertices GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKVertices(handle, owns: true);
		}
		return null;
	}
}
