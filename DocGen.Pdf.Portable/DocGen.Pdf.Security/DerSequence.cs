using System;
using System.Collections;
using System.IO;

namespace DocGen.Pdf.Security;

internal class DerSequence : Asn1Sequence
{
	internal static readonly DerSequence Empty = new DerSequence();

	internal static DerSequence FromCollection(Asn1EncodeCollection collection)
	{
		if (collection.Count >= 1)
		{
			return new DerSequence(collection);
		}
		return Empty;
	}

	internal DerSequence()
		: base(0)
	{
	}

	internal DerSequence(Asn1Encode asn1)
		: base(1)
	{
		AddObject(asn1);
	}

	internal DerSequence(params Asn1Encode[] collection)
		: base(collection.Length)
	{
		foreach (Asn1Encode obj in collection)
		{
			AddObject(obj);
		}
	}

	internal DerSequence(Asn1EncodeCollection collection)
		: base(collection.Count)
	{
		foreach (Asn1Encode item in collection)
		{
			AddObject(item);
		}
	}

	internal override void Encode(DerStream outputStream)
	{
		MemoryStream memoryStream = new MemoryStream();
		DerStream derStream = new DerStream(memoryStream);
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Asn1Encode obj = (Asn1Encode)enumerator.Current;
				derStream.WriteObject(obj);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		derStream.m_stream.Dispose();
		byte[] bytes = memoryStream.ToArray();
		outputStream.WriteEncoded(48, bytes);
	}
}
