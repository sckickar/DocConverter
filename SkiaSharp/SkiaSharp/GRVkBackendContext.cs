using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class GRVkBackendContext : IDisposable
{
	private GRVkGetProcedureAddressDelegate getProc;

	private GRVkGetProcProxyDelegate getProcProxy;

	private GCHandle getProcHandle;

	private unsafe void* getProcContext;

	public IntPtr VkInstance { get; set; }

	public IntPtr VkPhysicalDevice { get; set; }

	public IntPtr VkDevice { get; set; }

	public IntPtr VkQueue { get; set; }

	public uint GraphicsQueueIndex { get; set; }

	public uint MaxAPIVersion { get; set; }

	public GRVkExtensions Extensions { get; set; }

	public IntPtr VkPhysicalDeviceFeatures { get; set; }

	public IntPtr VkPhysicalDeviceFeatures2 { get; set; }

	public unsafe GRVkGetProcedureAddressDelegate GetProcedureAddress
	{
		get
		{
			return getProc;
		}
		set
		{
			getProc = value;
			if (getProcHandle.IsAllocated)
			{
				getProcHandle.Free();
			}
			getProcProxy = null;
			getProcHandle = default(GCHandle);
			getProcContext = null;
			if (value != null)
			{
				getProcProxy = DelegateProxies.Create(value, DelegateProxies.GRVkGetProcDelegateProxy, out var gch, out var contextPtr);
				getProcHandle = gch;
				getProcContext = (void*)contextPtr;
			}
		}
	}

	public bool ProtectedContext { get; set; }

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && getProcHandle.IsAllocated)
		{
			getProcHandle.Free();
			getProcHandle = default(GCHandle);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal unsafe GRVkBackendContextNative ToNative()
	{
		GRVkBackendContextNative result = default(GRVkBackendContextNative);
		result.fInstance = VkInstance;
		result.fDevice = VkDevice;
		result.fPhysicalDevice = VkPhysicalDevice;
		result.fQueue = VkQueue;
		result.fGraphicsQueueIndex = GraphicsQueueIndex;
		result.fMaxAPIVersion = MaxAPIVersion;
		result.fVkExtensions = Extensions?.Handle ?? IntPtr.Zero;
		result.fDeviceFeatures = VkPhysicalDeviceFeatures;
		result.fDeviceFeatures2 = VkPhysicalDeviceFeatures2;
		result.fGetProcUserData = getProcContext;
		result.fGetProc = getProcProxy;
		result.fProtectedContext = (ProtectedContext ? ((byte)1) : ((byte)0));
		return result;
	}
}
