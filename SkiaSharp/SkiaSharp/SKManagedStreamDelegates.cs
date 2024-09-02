using System;

namespace SkiaSharp;

internal struct SKManagedStreamDelegates : IEquatable<SKManagedStreamDelegates>
{
	public SKManagedStreamReadProxyDelegate fRead;

	public SKManagedStreamPeekProxyDelegate fPeek;

	public SKManagedStreamIsAtEndProxyDelegate fIsAtEnd;

	public SKManagedStreamHasPositionProxyDelegate fHasPosition;

	public SKManagedStreamHasLengthProxyDelegate fHasLength;

	public SKManagedStreamRewindProxyDelegate fRewind;

	public SKManagedStreamGetPositionProxyDelegate fGetPosition;

	public SKManagedStreamSeekProxyDelegate fSeek;

	public SKManagedStreamMoveProxyDelegate fMove;

	public SKManagedStreamGetLengthProxyDelegate fGetLength;

	public SKManagedStreamDuplicateProxyDelegate fDuplicate;

	public SKManagedStreamForkProxyDelegate fFork;

	public SKManagedStreamDestroyProxyDelegate fDestroy;

	public readonly bool Equals(SKManagedStreamDelegates obj)
	{
		if (fRead == obj.fRead && fPeek == obj.fPeek && fIsAtEnd == obj.fIsAtEnd && fHasPosition == obj.fHasPosition && fHasLength == obj.fHasLength && fRewind == obj.fRewind && fGetPosition == obj.fGetPosition && fSeek == obj.fSeek && fMove == obj.fMove && fGetLength == obj.fGetLength && fDuplicate == obj.fDuplicate && fFork == obj.fFork)
		{
			return fDestroy == obj.fDestroy;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKManagedStreamDelegates obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKManagedStreamDelegates left, SKManagedStreamDelegates right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKManagedStreamDelegates left, SKManagedStreamDelegates right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fRead);
		hashCode.Add(fPeek);
		hashCode.Add(fIsAtEnd);
		hashCode.Add(fHasPosition);
		hashCode.Add(fHasLength);
		hashCode.Add(fRewind);
		hashCode.Add(fGetPosition);
		hashCode.Add(fSeek);
		hashCode.Add(fMove);
		hashCode.Add(fGetLength);
		hashCode.Add(fDuplicate);
		hashCode.Add(fFork);
		hashCode.Add(fDestroy);
		return hashCode.ToHashCode();
	}
}
