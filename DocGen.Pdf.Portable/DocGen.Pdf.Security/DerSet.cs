using System;
using System.Collections;
using System.IO;

namespace DocGen.Pdf.Security;

internal class DerSet : Asn1Set
{
	internal static readonly DerSet Empty = new DerSet();

	internal DerSet()
		: base(0)
	{
	}

	internal DerSet(params Asn1Encode[] collection)
		: base(collection.Length)
	{
		foreach (Asn1Encode obj in collection)
		{
			AddObject(obj);
		}
		SortObjects();
	}

	internal DerSet(Asn1EncodeCollection collection)
		: this(collection, isSort: true)
	{
	}

	internal DerSet(Asn1EncodeCollection collection, bool isSort)
		: base(collection.Count)
	{
		foreach (Asn1Encode item in collection)
		{
			AddObject(item);
		}
		if (isSort)
		{
			SortObjects();
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
		outputStream.WriteEncoded(49, bytes);
	}

	internal static DerSet FromCollection(Asn1EncodeCollection collection)
	{
		if (collection.Count >= 1)
		{
			return new DerSet(collection);
		}
		return Empty;
	}

	internal static DerSet FromCollection(Asn1EncodeCollection collection, bool isSort)
	{
		if (collection.Count >= 1)
		{
			return new DerSet(collection, isSort);
		}
		return Empty;
	}
}
