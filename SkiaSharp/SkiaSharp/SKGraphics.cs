using System;

namespace SkiaSharp;

public static class SKGraphics
{
	public static void Init()
	{
		SkiaApi.sk_graphics_init();
	}

	public static void PurgeFontCache()
	{
		SkiaApi.sk_graphics_purge_font_cache();
	}

	public static void PurgeResourceCache()
	{
		SkiaApi.sk_graphics_purge_resource_cache();
	}

	public static void PurgeAllCaches()
	{
		SkiaApi.sk_graphics_purge_all_caches();
	}

	public static long GetFontCacheUsed()
	{
		return (long)SkiaApi.sk_graphics_get_font_cache_used();
	}

	public static long GetFontCacheLimit()
	{
		return (long)SkiaApi.sk_graphics_get_font_cache_limit();
	}

	public static long SetFontCacheLimit(long bytes)
	{
		return (long)SkiaApi.sk_graphics_set_font_cache_limit((IntPtr)bytes);
	}

	public static int GetFontCacheCountUsed()
	{
		return SkiaApi.sk_graphics_get_font_cache_count_used();
	}

	public static int GetFontCacheCountLimit()
	{
		return SkiaApi.sk_graphics_get_font_cache_count_limit();
	}

	public static int SetFontCacheCountLimit(int count)
	{
		return SkiaApi.sk_graphics_set_font_cache_count_limit(count);
	}

	public static int GetFontCachePointSizeLimit()
	{
		return SkiaApi.sk_graphics_get_font_cache_point_size_limit();
	}

	public static int SetFontCachePointSizeLimit(int count)
	{
		return SkiaApi.sk_graphics_set_font_cache_point_size_limit(count);
	}

	public static long GetResourceCacheTotalBytesUsed()
	{
		return (long)SkiaApi.sk_graphics_get_resource_cache_total_bytes_used();
	}

	public static long GetResourceCacheTotalByteLimit()
	{
		return (long)SkiaApi.sk_graphics_get_resource_cache_total_byte_limit();
	}

	public static long SetResourceCacheTotalByteLimit(long bytes)
	{
		return (long)SkiaApi.sk_graphics_set_resource_cache_total_byte_limit((IntPtr)bytes);
	}

	public static long GetResourceCacheSingleAllocationByteLimit()
	{
		return (long)SkiaApi.sk_graphics_get_resource_cache_single_allocation_byte_limit();
	}

	public static long SetResourceCacheSingleAllocationByteLimit(long bytes)
	{
		return (long)SkiaApi.sk_graphics_set_resource_cache_single_allocation_byte_limit((IntPtr)bytes);
	}

	public static void DumpMemoryStatistics(SKTraceMemoryDump dump)
	{
		if (dump == null)
		{
			throw new ArgumentNullException("dump");
		}
		SkiaApi.sk_graphics_dump_memory_statistics(dump.Handle);
	}
}
