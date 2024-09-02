using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1Set : Asn1, IEnumerable
{
	private class Asn1SetHelper : IAsn1SetHelper, IAsn1
	{
		private readonly Asn1Set m_set;

		private readonly int m_max;

		private int m_index;

		public Asn1SetHelper(Asn1Set outer)
		{
			m_set = outer;
			m_max = outer.Count;
		}

		public IAsn1 ReadObject()
		{
			if (m_index == m_max)
			{
				return null;
			}
			Asn1Encode asn1Encode = m_set[m_index++];
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

		public virtual Asn1 GetAsn1()
		{
			return m_set;
		}
	}

	private List<object> m_objects;

	internal object Objects => m_objects;

	internal Asn1Encode this[int index] => (Asn1Encode)m_objects[index];

	internal int Count => m_objects.Count;

	internal IAsn1SetHelper Parser => new Asn1SetHelper(this);

	protected internal Asn1Set(int capacity)
	{
		m_objects = new List<object>(capacity);
	}

	internal Asn1Encode[] ToArray()
	{
		Asn1Encode[] array = new Asn1Encode[Count];
		for (int i = 0; i < Count; i++)
		{
			array[i] = this[i];
		}
		return array;
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
		if (!(asn1Object is Asn1Set asn1Set))
		{
			return false;
		}
		if (Count != asn1Set.Count)
		{
			return false;
		}
		IEnumerator enumerator = GetEnumerator();
		IEnumerator enumerator2 = asn1Set.GetEnumerator();
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			Asn1 asn = GetCurrentSet(enumerator).GetAsn1();
			Asn1 asn2 = GetCurrentSet(enumerator2).GetAsn1();
			if (!asn.Equals(asn2))
			{
				return false;
			}
		}
		return true;
	}

	private Asn1Encode GetCurrentSet(IEnumerator e)
	{
		Asn1Encode asn1Encode = (Asn1Encode)e.Current;
		if (asn1Encode == null)
		{
			return DerNull.Value;
		}
		return asn1Encode;
	}

	private bool LessThanOrEqual(byte[] a, byte[] b)
	{
		int num = Math.Min(a.Length, b.Length);
		for (int i = 0; i != num; i++)
		{
			if (a[i] != b[i])
			{
				return a[i] < b[i];
			}
		}
		return num == a.Length;
	}

	protected internal void AddObject(Asn1Encode obj)
	{
		m_objects.Add(obj);
	}

	public override string ToString()
	{
		return m_objects.ToString();
	}

	internal override void Encode(DerStream derOut)
	{
		throw new NotImplementedException();
	}

	public static Asn1Set GetAsn1Set(object obj)
	{
		if (obj == null || obj is Asn1Set)
		{
			return (Asn1Set)obj;
		}
		if (obj is IAsn1SetHelper)
		{
			return GetAsn1Set(((IAsn1SetHelper)obj).GetAsn1());
		}
		if (obj is byte[])
		{
			try
			{
				return GetAsn1Set(Asn1.FromByteArray((byte[])obj));
			}
			catch (IOException ex)
			{
				throw new ArgumentException("Invalid byte array to create Asn1Set: " + ex.Message);
			}
		}
		if (obj is Asn1Encode)
		{
			Asn1 asn = ((Asn1Encode)obj).GetAsn1();
			if (asn is Asn1Set)
			{
				return (Asn1Set)asn;
			}
		}
		throw new ArgumentException("Invalid entry in sequence " + obj.GetType().FullName, "obj");
	}

	public static Asn1Set GetAsn1Set(Asn1Tag taggedObject, bool isExplicit)
	{
		Asn1 @object = taggedObject.GetObject();
		if (isExplicit)
		{
			if (!taggedObject.IsExplicit)
			{
				throw new ArgumentException("Tagged object is implicit.");
			}
			return (Asn1Set)@object;
		}
		if (taggedObject.IsExplicit)
		{
			return new DerSet(@object);
		}
		if (@object is Asn1Set)
		{
			return (Asn1Set)@object;
		}
		if (@object is Asn1Sequence)
		{
			Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
			foreach (Asn1Encode item in (Asn1Sequence)@object)
			{
				asn1EncodeCollection.Add(item);
			}
			return new DerSet(asn1EncodeCollection, isSort: false);
		}
		throw new ArgumentException("Invalid entry in sequence " + taggedObject.GetType().FullName, "obj");
	}

	public virtual IEnumerator GetEnumerator()
	{
		return m_objects.GetEnumerator();
	}

	protected internal void SortObjects()
	{
		if (m_objects.Count <= 1)
		{
			return;
		}
		bool flag = true;
		int num = m_objects.Count - 1;
		while (flag)
		{
			int i = 0;
			int num2 = 0;
			byte[] a = ((Asn1Encode)m_objects[0]).GetEncoded();
			flag = false;
			for (; i != num; i++)
			{
				byte[] encoded = ((Asn1Encode)m_objects[i + 1]).GetEncoded();
				if (LessThanOrEqual(a, encoded))
				{
					a = encoded;
					continue;
				}
				object value = m_objects[i];
				m_objects[i] = m_objects[i + 1];
				m_objects[i + 1] = value;
				flag = true;
				num2 = i;
			}
			num = num2;
		}
	}
}
