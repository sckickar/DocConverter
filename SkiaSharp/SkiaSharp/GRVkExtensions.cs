using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class GRVkExtensions : SKObject, ISKSkipObjectRegistration
{
	internal GRVkExtensions(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	private GRVkExtensions()
		: this(SkiaApi.gr_vk_extensions_new(), owns: true)
	{
	}

	protected override void DisposeNative()
	{
		SkiaApi.gr_vk_extensions_delete(Handle);
	}

	public void HasExtension(string extension, int minVersion)
	{
		SkiaApi.gr_vk_extensions_has_extension(Handle, extension, (uint)minVersion);
	}

	public void Initialize(GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice)
	{
		Initialize(getProc, vkInstance, vkPhysicalDevice, null, null);
	}

	public unsafe void Initialize(GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions)
	{
		GCHandle gch;
		IntPtr contextPtr;
		GRVkGetProcProxyDelegate getProc2 = DelegateProxies.Create(getProc, DelegateProxies.GRVkGetProcDelegateProxy, out gch, out contextPtr);
		try
		{
			SkiaApi.gr_vk_extensions_init(Handle, getProc2, (void*)contextPtr, vkInstance, vkPhysicalDevice, (instanceExtensions != null) ? ((uint)instanceExtensions.Length) : 0u, instanceExtensions, (deviceExtensions != null) ? ((uint)deviceExtensions.Length) : 0u, deviceExtensions);
		}
		finally
		{
			gch.Free();
		}
	}

	public static GRVkExtensions Create(GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions)
	{
		GRVkExtensions gRVkExtensions = new GRVkExtensions();
		gRVkExtensions.Initialize(getProc, vkInstance, vkPhysicalDevice, instanceExtensions, deviceExtensions);
		return gRVkExtensions;
	}

	internal static GRVkExtensions GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new GRVkExtensions(handle, owns: true);
		}
		return null;
	}
}
