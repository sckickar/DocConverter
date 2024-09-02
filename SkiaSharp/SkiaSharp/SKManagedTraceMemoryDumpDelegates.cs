using System;

namespace SkiaSharp;

internal struct SKManagedTraceMemoryDumpDelegates : IEquatable<SKManagedTraceMemoryDumpDelegates>
{
	public SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate fDumpNumericValue;

	public SKManagedTraceMemoryDumpDumpStringValueProxyDelegate fDumpStringValue;

	public readonly bool Equals(SKManagedTraceMemoryDumpDelegates obj)
	{
		if (fDumpNumericValue == obj.fDumpNumericValue)
		{
			return fDumpStringValue == obj.fDumpStringValue;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKManagedTraceMemoryDumpDelegates obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKManagedTraceMemoryDumpDelegates left, SKManagedTraceMemoryDumpDelegates right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKManagedTraceMemoryDumpDelegates left, SKManagedTraceMemoryDumpDelegates right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fDumpNumericValue);
		hashCode.Add(fDumpStringValue);
		return hashCode.ToHashCode();
	}
}
