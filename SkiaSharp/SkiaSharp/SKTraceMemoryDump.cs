using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKTraceMemoryDump : SKObject, ISKSkipObjectRegistration
{
	private static readonly SKManagedTraceMemoryDumpDelegates delegates;

	private readonly IntPtr userData;

	unsafe static SKTraceMemoryDump()
	{
		delegates = new SKManagedTraceMemoryDumpDelegates
		{
			fDumpNumericValue = DumpNumericValueInternal,
			fDumpStringValue = DumpStringValueInternal
		};
		SkiaApi.sk_managedtracememorydump_set_procs(delegates);
	}

	protected unsafe SKTraceMemoryDump(bool detailedDump, bool dumpWrappedObjects)
		: base(IntPtr.Zero, owns: true)
	{
		userData = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_managedtracememorydump_new(detailedDump, dumpWrappedObjects, (void*)userData);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKTraceMemoryDump instance.");
		}
	}

	protected override void DisposeNative()
	{
		DelegateProxies.GetUserData<SKTraceMemoryDump>(userData, out var gch);
		SkiaApi.sk_managedtracememorydump_delete(Handle);
		gch.Free();
	}

	protected virtual void OnDumpNumericValue(string dumpName, string valueName, string units, ulong value)
	{
	}

	protected virtual void OnDumpStringValue(string dumpName, string valueName, string value)
	{
	}

	[MonoPInvokeCallback(typeof(SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate))]
	private unsafe static void DumpNumericValueInternal(IntPtr d, void* context, void* dumpName, void* valueName, void* units, ulong value)
	{
		GCHandle gch;
		SKTraceMemoryDump sKTraceMemoryDump = DelegateProxies.GetUserData<SKTraceMemoryDump>((IntPtr)context, out gch);
		sKTraceMemoryDump.OnDumpNumericValue(Marshal.PtrToStringAnsi((IntPtr)dumpName), Marshal.PtrToStringAnsi((IntPtr)valueName), Marshal.PtrToStringAnsi((IntPtr)units), value);
	}

	[MonoPInvokeCallback(typeof(SKManagedTraceMemoryDumpDumpStringValueProxyDelegate))]
	private unsafe static void DumpStringValueInternal(IntPtr d, void* context, void* dumpName, void* valueName, void* value)
	{
		GCHandle gch;
		SKTraceMemoryDump sKTraceMemoryDump = DelegateProxies.GetUserData<SKTraceMemoryDump>((IntPtr)context, out gch);
		sKTraceMemoryDump.OnDumpStringValue(Marshal.PtrToStringAnsi((IntPtr)dumpName), Marshal.PtrToStringAnsi((IntPtr)valueName), Marshal.PtrToStringAnsi((IntPtr)value));
	}
}
