namespace SkiaSharp.Internals;

public interface IPlatformLock
{
	void EnterReadLock();

	void ExitReadLock();

	void EnterWriteLock();

	void ExitWriteLock();

	void EnterUpgradeableReadLock();

	void ExitUpgradeableReadLock();
}
