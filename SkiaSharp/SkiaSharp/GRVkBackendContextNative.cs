using System;

namespace SkiaSharp;

internal struct GRVkBackendContextNative : IEquatable<GRVkBackendContextNative>
{
	public IntPtr fInstance;

	public IntPtr fPhysicalDevice;

	public IntPtr fDevice;

	public IntPtr fQueue;

	public uint fGraphicsQueueIndex;

	public uint fMinAPIVersion;

	public uint fInstanceVersion;

	public uint fMaxAPIVersion;

	public uint fExtensions;

	public IntPtr fVkExtensions;

	public uint fFeatures;

	public IntPtr fDeviceFeatures;

	public IntPtr fDeviceFeatures2;

	public IntPtr fMemoryAllocator;

	public GRVkGetProcProxyDelegate fGetProc;

	public unsafe void* fGetProcUserData;

	public byte fOwnsInstanceAndDevice;

	public byte fProtectedContext;

	public unsafe readonly bool Equals(GRVkBackendContextNative obj)
	{
		if (fInstance == obj.fInstance && fPhysicalDevice == obj.fPhysicalDevice && fDevice == obj.fDevice && fQueue == obj.fQueue && fGraphicsQueueIndex == obj.fGraphicsQueueIndex && fMinAPIVersion == obj.fMinAPIVersion && fInstanceVersion == obj.fInstanceVersion && fMaxAPIVersion == obj.fMaxAPIVersion && fExtensions == obj.fExtensions && fVkExtensions == obj.fVkExtensions && fFeatures == obj.fFeatures && fDeviceFeatures == obj.fDeviceFeatures && fDeviceFeatures2 == obj.fDeviceFeatures2 && fMemoryAllocator == obj.fMemoryAllocator && fGetProc == obj.fGetProc && fGetProcUserData == obj.fGetProcUserData && fOwnsInstanceAndDevice == obj.fOwnsInstanceAndDevice)
		{
			return fProtectedContext == obj.fProtectedContext;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRVkBackendContextNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRVkBackendContextNative left, GRVkBackendContextNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRVkBackendContextNative left, GRVkBackendContextNative right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fInstance);
		hashCode.Add(fPhysicalDevice);
		hashCode.Add(fDevice);
		hashCode.Add(fQueue);
		hashCode.Add(fGraphicsQueueIndex);
		hashCode.Add(fMinAPIVersion);
		hashCode.Add(fInstanceVersion);
		hashCode.Add(fMaxAPIVersion);
		hashCode.Add(fExtensions);
		hashCode.Add(fVkExtensions);
		hashCode.Add(fFeatures);
		hashCode.Add(fDeviceFeatures);
		hashCode.Add(fDeviceFeatures2);
		hashCode.Add(fMemoryAllocator);
		hashCode.Add(fGetProc);
		hashCode.Add(fGetProcUserData);
		hashCode.Add(fOwnsInstanceAndDevice);
		hashCode.Add(fProtectedContext);
		return hashCode.ToHashCode();
	}
}
