using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp.Internals;

public static class PlatformLock
{
	private class ReadWriteLock : IPlatformLock
	{
		private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		public void EnterReadLock()
		{
			_lock.EnterReadLock();
		}

		public void ExitReadLock()
		{
			_lock.ExitReadLock();
		}

		public void EnterWriteLock()
		{
			_lock.EnterWriteLock();
		}

		public void ExitWriteLock()
		{
			_lock.ExitWriteLock();
		}

		public void EnterUpgradeableReadLock()
		{
			_lock.EnterUpgradeableReadLock();
		}

		public void ExitUpgradeableReadLock()
		{
			_lock.ExitUpgradeableReadLock();
		}
	}

	private class NonAlertableWin32Lock : IPlatformLock
	{
		public struct CRITICAL_SECTION
		{
			public IntPtr DebugInfo;

			public int LockCount;

			public int RecursionCount;

			public IntPtr OwningThread;

			public IntPtr LockSemaphore;

			public UIntPtr SpinCount;
		}

		private IntPtr _cs;

		public NonAlertableWin32Lock()
		{
			_cs = Marshal.AllocHGlobal(Marshal.SizeOf<CRITICAL_SECTION>());
			if (_cs == IntPtr.Zero)
			{
				throw new OutOfMemoryException("Failed to allocate memory for critical section");
			}
			InitializeCriticalSectionEx(_cs, 4000u, 0u);
		}

		~NonAlertableWin32Lock()
		{
			if (_cs != IntPtr.Zero)
			{
				DeleteCriticalSection(_cs);
				Marshal.FreeHGlobal(_cs);
				_cs = IntPtr.Zero;
			}
		}

		private void Enter()
		{
			if (_cs != IntPtr.Zero)
			{
				EnterCriticalSection(_cs);
			}
		}

		private void Leave()
		{
			if (_cs != IntPtr.Zero)
			{
				LeaveCriticalSection(_cs);
			}
		}

		public void EnterReadLock()
		{
			Enter();
		}

		public void ExitReadLock()
		{
			Leave();
		}

		public void EnterWriteLock()
		{
			Enter();
		}

		public void ExitWriteLock()
		{
			Leave();
		}

		public void EnterUpgradeableReadLock()
		{
			Enter();
		}

		public void ExitUpgradeableReadLock()
		{
			Leave();
		}

		[DllImport("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool InitializeCriticalSectionEx(IntPtr lpCriticalSection, uint dwSpinCount, uint Flags);

		[DllImport("Kernel32.dll")]
		private static extern void DeleteCriticalSection(IntPtr lpCriticalSection);

		[DllImport("Kernel32.dll")]
		private static extern void EnterCriticalSection(IntPtr lpCriticalSection);

		[DllImport("Kernel32.dll")]
		private static extern void LeaveCriticalSection(IntPtr lpCriticalSection);
	}

	public static Func<IPlatformLock> Factory { get; set; } = DefaultFactory;


	public static IPlatformLock Create()
	{
		return Factory();
	}

	public static IPlatformLock DefaultFactory()
	{
		if (PlatformConfiguration.IsWindows)
		{
			return new NonAlertableWin32Lock();
		}
		return new ReadWriteLock();
	}
}
