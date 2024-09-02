using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Security;

internal class Asn1Sequence : Asn1, IEnumerable
{
	private class Asn1SequenceHelper : IAsn1Collection, IAsn1
	{
		private readonly Asn1Sequence m_sequence;

		private readonly int m_max;

		private int m_index;

		internal Asn1SequenceHelper(Asn1Sequence sequence)
		{
			m_sequence = sequence;
			m_max = sequence.Count;
		}

		public IAsn1 ReadObject()
		{
			if (m_index == m_max)
			{
				return null;
			}
			Asn1Encode asn1Encode = m_sequence[m_index++];
			if (asn1Encode is Asn1Sequence)
			{
				return ((Asn1Sequence)asn1Encode).Parser;
			}
			if (asn1Encode is Asn1Set)
			{
				return ((Asn1Set)asn1Encode).Parser;
			}
			return asn1Encode;
		}

		public Asn1 GetAsn1()
		{
			return m_sequence;
		}
	}

	private List<object> m_objects;

	internal IList Objects => m_objects;

	internal virtual Asn1Encode this[int index] => (Asn1Encode)m_objects[index];

	internal virtual IAsn1Collection Parser => new Asn1SequenceHelper(this);

	internal virtual int Count => m_objects.Count;

	public Asn1Sequence()
		: this(0)
	{
		m_objects = new List<object>();
	}

	public Asn1Sequence(params Asn1Encode[] asn1EncodableArray)
		: this(asn1EncodableArray.Length)
	{
		foreach (Asn1Encode obj in asn1EncodableArray)
		{
			AddObject(obj);
		}
	}

	public Asn1Sequence(List<Asn1> sequence)
		: base(Asn1UniversalTags.Sequence | Asn1UniversalTags.Constructed)
	{
		m_objects = new List<object>();
		foreach (Asn1 item in sequence)
		{
			m_objects.Add(item);
		}
	}

	internal Asn1Sequence(Asn1EncodeCollection collection)
		: this(collection.Count)
	{
		foreach (Asn1 item in collection)
		{
			AddObject(item);
		}
	}

	internal Asn1Sequence(int capacity)
		: base(Asn1UniversalTags.Sequence | Asn1UniversalTags.Constructed)
	{
		m_objects = new List<object>();
	}

	internal static Asn1Sequence GetSequence(object obj)
	{
		if (obj == null || obj is Asn1Sequence)
		{
			return (Asn1Sequence)obj;
		}
		if (obj is IAsn1Collection)
		{
			return GetSequence(((IAsn1Collection)obj).GetAsn1());
		}
		if (obj is byte[])
		{
			try
			{
				return GetSequence(Asn1.FromByteArray((byte[])obj));
			}
			catch (IOException ex)
			{
				throw new ArgumentException(ex.Message);
			}
		}
		if (obj is Asn1Encode)
		{
			Asn1 asn = ((Asn1Encode)obj).GetAsn1();
			if (asn is Asn1Sequence)
			{
				return (Asn1Sequence)asn;
			}
		}
		throw new ArgumentException("Invalid entry in sequence " + obj.GetType().FullName, "obj");
	}

	internal static Asn1Sequence GetSequence(Asn1Tag obj, bool explicitly)
	{
		Asn1 @object = obj.GetObject();
		if (explicitly)
		{
			if (!obj.IsExplicit)
			{
				throw new ArgumentException("Invalid entry in sequence");
			}
			return (Asn1Sequence)@object;
		}
		if (obj.IsExplicit)
		{
			if (obj is BerTag)
			{
				return new BerSequence(@object);
			}
			return new DerSequence(@object);
		}
		if (@object is Asn1Sequence)
		{
			return (Asn1Sequence)@object;
		}
		throw new ArgumentException("Invalid entry in sequence " + obj.GetType().FullName, "obj");
	}

	public virtual IEnumerator GetEnumerator()
	{
		return m_objects.GetEnumerator();
	}

	public override int GetHashCode()
	{
		int num = Count;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				num *= 17;
				num = ((current != null) ? (num ^ current.GetHashCode()) : (num ^ DerNull.Value.GetAsn1Hash()));
			}
			return num;
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

	protected override bool IsEquals(Asn1 asn1Object)
	{
		if (!(asn1Object is Asn1Sequence asn1Sequence))
		{
			return false;
		}
		if (Count != asn1Sequence.Count)
		{
			return false;
		}
		IEnumerator enumerator = GetEnumerator();
		IEnumerator enumerator2 = asn1Sequence.GetEnumerator();
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			Asn1 asn = GetCurrentObject(enumerator).GetAsn1();
			Asn1 asn2 = GetCurrentObject(enumerator2).GetAsn1();
			if (!asn.Equals(asn2))
			{
				return false;
			}
		}
		return true;
	}

	private Asn1Encode GetCurrentObject(IEnumerator e)
	{
		Asn1Encode asn1Encode = (Asn1Encode)e.Current;
		if (asn1Encode == null)
		{
			return DerNull.Value;
		}
		return asn1Encode;
	}

	protected internal void AddObject(Asn1Encode obj)
	{
		m_objects.Add(obj);
	}

	public override string ToString()
	{
		return ToString(m_objects);
	}

	internal string ToString(IEnumerable e)
	{
		StringBuilder stringBuilder = new StringBuilder("[");
		IEnumerator enumerator = e.GetEnumerator();
		if (enumerator.MoveNext())
		{
			stringBuilder.Append(enumerator.Current.ToString());
			while (enumerator.MoveNext())
			{
				stringBuilder.Append(", ");
				stringBuilder.Append(enumerator.Current.ToString());
			}
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	private byte[] ToArray()
	{
		MemoryStream memoryStream = new MemoryStream();
		foreach (Asn1Encode @object in m_objects)
		{
			byte[] array = null;
			if (@object is Asn1Integer)
			{
				array = (@object as Asn1Integer).AsnEncode();
			}
			else if (@object is Asn1Boolean)
			{
				array = (@object as Asn1Boolean).AsnEncode();
			}
			else if (@object is Asn1Null)
			{
				array = (@object as Asn1Null).AsnEncode();
			}
			else if (@object is Asn1Identifier)
			{
				array = (@object as Asn1Identifier).Asn1Encode();
			}
			else if (@object is Asn1Octet)
			{
				array = (@object as Asn1Octet).AsnEncode();
			}
			else if (@object is Asn1Sequence)
			{
				array = (@object as Asn1Sequence).AsnEncode();
			}
			else if (@object is Algorithms)
			{
				array = (@object as Algorithms).AsnEncode();
			}
			memoryStream.Write(array, 0, array.Length);
		}
		return memoryStream.ToArray();
	}

	internal override void Encode(DerStream derOut)
	{
		throw new NotImplementedException();
	}

	internal byte[] AsnEncode()
	{
		byte[] bytes = ToArray();
		return Asn1Encode(bytes);
	}
}
