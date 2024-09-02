using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Security;

internal class BerOctet : DerOctet, IEnumerable
{
	private const int m_max = 1000;

	private IEnumerable m_octets;

	public BerOctet(byte[] bytes)
		: base(bytes)
	{
	}

	public BerOctet(IEnumerable octets)
		: base(GetBytes(octets))
	{
		m_octets = octets;
	}

	public BerOctet(Asn1 asn1)
		: base(asn1)
	{
	}

	public BerOctet(Asn1Encode asn1)
		: base(asn1.GetAsn1())
	{
	}

	internal override byte[] GetOctets()
	{
		return m_value;
	}

	private IList GenerateOctets()
	{
		List<DerOctet> list = new List<DerOctet>();
		for (int i = 0; i < m_value.Length; i += 1000)
		{
			byte[] array = new byte[Math.Min(m_value.Length, i + 1000) - i];
			Array.Copy(m_value, i, array, 0, array.Length);
			list.Add(new DerOctet(array));
		}
		return list;
	}

	internal override void Encode(DerStream stream)
	{
		if (stream is Asn1DerStream)
		{
			stream.m_stream.WriteByte(36);
			stream.m_stream.WriteByte(128);
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						DerOctet obj = (DerOctet)enumerator.Current;
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

	internal static BerOctet GetBerOctet(Asn1Sequence sequence)
	{
		List<Asn1Encode> list = new List<Asn1Encode>();
		foreach (Asn1Encode item in sequence)
		{
			list.Add(item);
		}
		return new BerOctet(list);
	}

	private static byte[] GetBytes(IEnumerable octets)
	{
		MemoryStream memoryStream = new MemoryStream();
		foreach (DerOctet octet in octets)
		{
			byte[] octets2 = octet.GetOctets();
			memoryStream.Write(octets2, 0, octets2.Length);
		}
		return memoryStream.ToArray();
	}

	public IEnumerator GetEnumerator()
	{
		if (m_octets == null)
		{
			return GenerateOctets().GetEnumerator();
		}
		return m_octets.GetEnumerator();
	}
}
