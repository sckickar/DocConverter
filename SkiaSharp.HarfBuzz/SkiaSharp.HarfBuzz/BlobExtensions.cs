using System;
using System.Runtime.InteropServices;
using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz;

public static class BlobExtensions
{
	public static Blob ToHarfBuzzBlob(this SKStreamAsset asset)
	{
		if (asset == null)
		{
			throw new ArgumentNullException("asset");
		}
		int length = asset.Length;
		IntPtr memoryBase = asset.GetMemoryBase();
		Blob blob;
		if (memoryBase != IntPtr.Zero)
		{
			blob = new Blob(memoryBase, length, MemoryMode.ReadOnly, delegate
			{
				asset.Dispose();
			});
		}
		else
		{
			IntPtr ptr = Marshal.AllocCoTaskMem(length);
			asset.Read(ptr, length);
			blob = new Blob(ptr, length, MemoryMode.ReadOnly, delegate
			{
				Marshal.FreeCoTaskMem(ptr);
			});
		}
		blob.MakeImmutable();
		return blob;
	}
}
