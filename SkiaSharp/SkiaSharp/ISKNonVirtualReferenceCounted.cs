namespace SkiaSharp;

internal interface ISKNonVirtualReferenceCounted : ISKReferenceCounted
{
	void ReferenceNative();

	void UnreferenceNative();
}
