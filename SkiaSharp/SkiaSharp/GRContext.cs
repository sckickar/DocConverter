using System;
using System.ComponentModel;

namespace SkiaSharp;

public class GRContext : GRRecordingContext
{
	public new GRBackend Backend => base.Backend;

	public bool IsAbandoned => SkiaApi.gr_direct_context_is_abandoned(Handle);

	internal GRContext(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		AbandonContext();
		base.DisposeNative();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateGl() instead.")]
	public static GRContext Create(GRBackend backend)
	{
		return backend switch
		{
			GRBackend.Metal => throw new NotSupportedException(), 
			GRBackend.OpenGL => CreateGl(), 
			GRBackend.Vulkan => throw new NotSupportedException(), 
			GRBackend.Dawn => throw new NotSupportedException(), 
			_ => throw new ArgumentOutOfRangeException("backend"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateGl(GRGlInterface) instead.")]
	public static GRContext Create(GRBackend backend, GRGlInterface backendContext)
	{
		return backend switch
		{
			GRBackend.Metal => throw new NotSupportedException(), 
			GRBackend.OpenGL => CreateGl(backendContext), 
			GRBackend.Vulkan => throw new NotSupportedException(), 
			GRBackend.Dawn => throw new NotSupportedException(), 
			_ => throw new ArgumentOutOfRangeException("backend"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateGl(GRGlInterface) instead.")]
	public static GRContext Create(GRBackend backend, IntPtr backendContext)
	{
		return backend switch
		{
			GRBackend.Metal => throw new NotSupportedException(), 
			GRBackend.OpenGL => GetObject(SkiaApi.gr_direct_context_make_gl(backendContext)), 
			GRBackend.Vulkan => throw new NotSupportedException(), 
			GRBackend.Dawn => throw new NotSupportedException(), 
			_ => throw new ArgumentOutOfRangeException("backend"), 
		};
	}

	public static GRContext CreateGl()
	{
		return CreateGl(null, null);
	}

	public static GRContext CreateGl(GRGlInterface backendContext)
	{
		return CreateGl(backendContext, null);
	}

	public static GRContext CreateGl(GRContextOptions options)
	{
		return CreateGl(null, options);
	}

	public unsafe static GRContext CreateGl(GRGlInterface backendContext, GRContextOptions options)
	{
		IntPtr glInterface = backendContext?.Handle ?? IntPtr.Zero;
		if (options == null)
		{
			return GetObject(SkiaApi.gr_direct_context_make_gl(glInterface));
		}
		GRContextOptionsNative gRContextOptionsNative = options.ToNative();
		return GetObject(SkiaApi.gr_direct_context_make_gl_with_options(glInterface, &gRContextOptionsNative));
	}

	public static GRContext CreateVulkan(GRVkBackendContext backendContext)
	{
		return CreateVulkan(backendContext, null);
	}

	public unsafe static GRContext CreateVulkan(GRVkBackendContext backendContext, GRContextOptions options)
	{
		if (backendContext == null)
		{
			throw new ArgumentNullException("backendContext");
		}
		if (options == null)
		{
			return GetObject(SkiaApi.gr_direct_context_make_vulkan(backendContext.ToNative()));
		}
		GRContextOptionsNative gRContextOptionsNative = options.ToNative();
		return GetObject(SkiaApi.gr_direct_context_make_vulkan_with_options(backendContext.ToNative(), &gRContextOptionsNative));
	}

	public void AbandonContext(bool releaseResources = false)
	{
		if (releaseResources)
		{
			SkiaApi.gr_direct_context_release_resources_and_abandon_context(Handle);
		}
		else
		{
			SkiaApi.gr_direct_context_abandon_context(Handle);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetResourceCacheLimit() instead.")]
	public void GetResourceCacheLimits(out int maxResources, out long maxResourceBytes)
	{
		maxResources = -1;
		maxResourceBytes = GetResourceCacheLimit();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SetResourceCacheLimit(long) instead.")]
	public void SetResourceCacheLimits(int maxResources, long maxResourceBytes)
	{
		SetResourceCacheLimit(maxResourceBytes);
	}

	public long GetResourceCacheLimit()
	{
		return (long)SkiaApi.gr_direct_context_get_resource_cache_limit(Handle);
	}

	public void SetResourceCacheLimit(long maxResourceBytes)
	{
		SkiaApi.gr_direct_context_set_resource_cache_limit(Handle, (IntPtr)maxResourceBytes);
	}

	public unsafe void GetResourceCacheUsage(out int maxResources, out long maxResourceBytes)
	{
		IntPtr intPtr = default(IntPtr);
		fixed (int* maxResources2 = &maxResources)
		{
			SkiaApi.gr_direct_context_get_resource_cache_usage(Handle, maxResources2, &intPtr);
		}
		maxResourceBytes = (long)intPtr;
	}

	public void ResetContext(GRGlBackendState state)
	{
		ResetContext((uint)state);
	}

	public void ResetContext(GRBackendState state = GRBackendState.All)
	{
		ResetContext((uint)state);
	}

	public void ResetContext(uint state)
	{
		SkiaApi.gr_direct_context_reset_context(Handle, state);
	}

	public void Flush()
	{
		Flush(submit: true);
	}

	public void Flush(bool submit, bool synchronous = false)
	{
		if (submit)
		{
			SkiaApi.gr_direct_context_flush_and_submit(Handle, synchronous);
		}
		else
		{
			SkiaApi.gr_direct_context_flush(Handle);
		}
	}

	public void Submit(bool synchronous = false)
	{
		SkiaApi.gr_direct_context_submit(Handle, synchronous);
	}

	public new int GetMaxSurfaceSampleCount(SKColorType colorType)
	{
		return base.GetMaxSurfaceSampleCount(colorType);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public int GetRecommendedSampleCount(GRPixelConfig config, float dpi)
	{
		return 0;
	}

	public void DumpMemoryStatistics(SKTraceMemoryDump dump)
	{
		IntPtr handle = Handle;
		if (dump == null)
		{
			throw new ArgumentNullException("dump");
		}
		SkiaApi.gr_direct_context_dump_memory_statistics(handle, dump.Handle);
	}

	public void PurgeResources()
	{
		SkiaApi.gr_direct_context_free_gpu_resources(Handle);
	}

	public void PurgeUnusedResources(long milliseconds)
	{
		SkiaApi.gr_direct_context_perform_deferred_cleanup(Handle, milliseconds);
	}

	public void PurgeUnlockedResources(bool scratchResourcesOnly)
	{
		SkiaApi.gr_direct_context_purge_unlocked_resources(Handle, scratchResourcesOnly);
	}

	public void PurgeUnlockedResources(long bytesToPurge, bool preferScratchResources)
	{
		SkiaApi.gr_direct_context_purge_unlocked_resources_bytes(Handle, (IntPtr)bytesToPurge, preferScratchResources);
	}

	internal static GRContext GetObject(IntPtr handle, bool owns = true)
	{
		return SKObject.GetOrAddObject(handle, owns, (IntPtr h, bool o) => new GRContext(h, o));
	}
}
