using System;
using System.Collections;

namespace DocGen.Pdf.Security;

internal class BerSequence : DerSequence
{
	public new static readonly BerSequence Empty = new BerSequence();

	internal BerSequence()
	{
	}

	internal BerSequence(Asn1Encode asn1)
		: base(asn1)
	{
	}

	internal BerSequence(params Asn1Encode[] collection)
		: base(collection)
	{
	}

	internal BerSequence(Asn1EncodeCollection collection)
		: base(collection)
	{
	}

	public new static BerSequence FromCollection(Asn1EncodeCollection collection)
	{
		if (collection.Count >= 1)
		{
			return new BerSequence(collection);
		}
		return Empty;
	}

	internal override void Encode(DerStream stream)
	{
		if (stream is Asn1DerStream)
		{
			stream.m_stream.WriteByte(48);
			stream.m_stream.WriteByte(128);
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Asn1Encode obj = (Asn1Encode)enumerator.Current;
						stream.WriteObject(obj);
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
			}
			stream.m_stream.WriteByte(0);
			stream.m_stream.WriteByte(0);
		}
		else
		{
			base.Encode(stream);
		}
	}
}
