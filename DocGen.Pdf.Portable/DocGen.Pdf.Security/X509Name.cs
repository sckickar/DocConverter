using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

internal class X509Name : Asn1Encode
{
	internal static readonly DerObjectID C;

	internal static readonly DerObjectID O;

	internal static readonly DerObjectID OU;

	internal static readonly DerObjectID T;

	internal static readonly DerObjectID CN;

	internal static readonly DerObjectID Street;

	internal static readonly DerObjectID SerialNumber;

	internal static readonly DerObjectID L;

	internal static readonly DerObjectID ST;

	internal static readonly DerObjectID Surname;

	internal static readonly DerObjectID GivenName;

	internal static readonly DerObjectID Initials;

	internal static readonly DerObjectID Generation;

	internal static readonly DerObjectID UniqueIdentifier;

	internal static readonly DerObjectID BusinessCategory;

	internal static readonly DerObjectID PostalCode;

	internal static readonly DerObjectID DnQualifier;

	internal static readonly DerObjectID Pseudonym;

	internal static readonly DerObjectID DateOfBirth;

	internal static readonly DerObjectID PlaceOfBirth;

	internal static readonly DerObjectID Gender;

	internal static readonly DerObjectID CountryOfCitizenship;

	internal static readonly DerObjectID CountryOfResidence;

	internal static readonly DerObjectID NameAtBirth;

	internal static readonly DerObjectID PostalAddress;

	internal static readonly DerObjectID DmdName;

	internal static readonly DerObjectID TelephoneNumber;

	internal static readonly DerObjectID Name;

	internal static readonly DerObjectID EmailAddress;

	internal static readonly DerObjectID UnstructuredName;

	internal static readonly DerObjectID UnstructuredAddress;

	internal static readonly DerObjectID E;

	internal static readonly DerObjectID DC;

	internal static readonly DerObjectID UID;

	private static readonly bool[] defaultReverse;

	internal static readonly Dictionary<DerObjectID, string> DefaultSymbols;

	internal static readonly Dictionary<DerObjectID, string> RFC2253Symbols;

	internal static readonly Dictionary<DerObjectID, string> RFC1779Symbols;

	internal static readonly Dictionary<string, DerObjectID> DefaultLookup;

	private readonly List<object> m_ordering = new List<object>();

	private List<object> m_values = new List<object>();

	private List<object> m_added = new List<object>();

	private Asn1Sequence m_sequence;

	internal static bool DefaultReverse
	{
		get
		{
			return defaultReverse[0];
		}
		set
		{
			defaultReverse[0] = value;
		}
	}

	static X509Name()
	{
		C = new DerObjectID("2.5.4.6");
		O = new DerObjectID("2.5.4.10");
		OU = new DerObjectID("2.5.4.11");
		T = new DerObjectID("2.5.4.12");
		CN = new DerObjectID("2.5.4.3");
		Street = new DerObjectID("2.5.4.9");
		SerialNumber = new DerObjectID("2.5.4.5");
		L = new DerObjectID("2.5.4.7");
		ST = new DerObjectID("2.5.4.8");
		Surname = new DerObjectID("2.5.4.4");
		GivenName = new DerObjectID("2.5.4.42");
		Initials = new DerObjectID("2.5.4.43");
		Generation = new DerObjectID("2.5.4.44");
		UniqueIdentifier = new DerObjectID("2.5.4.45");
		BusinessCategory = new DerObjectID("2.5.4.15");
		PostalCode = new DerObjectID("2.5.4.17");
		DnQualifier = new DerObjectID("2.5.4.46");
		Pseudonym = new DerObjectID("2.5.4.65");
		DateOfBirth = new DerObjectID("1.3.6.1.5.5.7.9.1");
		PlaceOfBirth = new DerObjectID("1.3.6.1.5.5.7.9.2");
		Gender = new DerObjectID("1.3.6.1.5.5.7.9.3");
		CountryOfCitizenship = new DerObjectID("1.3.6.1.5.5.7.9.4");
		CountryOfResidence = new DerObjectID("1.3.6.1.5.5.7.9.5");
		NameAtBirth = new DerObjectID("1.3.36.8.3.14");
		PostalAddress = new DerObjectID("2.5.4.16");
		DmdName = new DerObjectID("2.5.4.54");
		TelephoneNumber = X509Objects.TelephoneNumberID;
		Name = X509Objects.NameID;
		EmailAddress = PKCSOIDs.Pkcs9AtEmailAddress;
		UnstructuredName = PKCSOIDs.Pkcs9AtUnstructuredName;
		UnstructuredAddress = PKCSOIDs.Pkcs9AtUnstructuredAddress;
		E = EmailAddress;
		DC = new DerObjectID("0.9.2342.19200300.100.1.25");
		UID = new DerObjectID("0.9.2342.19200300.100.1.1");
		defaultReverse = new bool[1];
		DefaultSymbols = new Dictionary<DerObjectID, string>();
		RFC2253Symbols = new Dictionary<DerObjectID, string>();
		RFC1779Symbols = new Dictionary<DerObjectID, string>();
		DefaultLookup = new Dictionary<string, DerObjectID>();
		DefaultSymbols.Add(C, "C");
		DefaultSymbols.Add(O, "O");
		DefaultSymbols.Add(T, "T");
		DefaultSymbols.Add(OU, "OU");
		DefaultSymbols.Add(CN, "CN");
		DefaultSymbols.Add(L, "L");
		DefaultSymbols.Add(ST, "ST");
		DefaultSymbols.Add(SerialNumber, "SERIALNUMBER");
		DefaultSymbols.Add(EmailAddress, "E");
		DefaultSymbols.Add(DC, "DC");
		DefaultSymbols.Add(UID, "UID");
		DefaultSymbols.Add(Street, "STREET");
		DefaultSymbols.Add(Surname, "SURNAME");
		DefaultSymbols.Add(GivenName, "GIVENNAME");
		DefaultSymbols.Add(Initials, "INITIALS");
		DefaultSymbols.Add(Generation, "GENERATION");
		DefaultSymbols.Add(UnstructuredAddress, "unstructuredAddress");
		DefaultSymbols.Add(UnstructuredName, "unstructuredName");
		DefaultSymbols.Add(UniqueIdentifier, "UniqueIdentifier");
		DefaultSymbols.Add(DnQualifier, "DN");
		DefaultSymbols.Add(Pseudonym, "Pseudonym");
		DefaultSymbols.Add(PostalAddress, "PostalAddress");
		DefaultSymbols.Add(NameAtBirth, "NameAtBirth");
		DefaultSymbols.Add(CountryOfCitizenship, "CountryOfCitizenship");
		DefaultSymbols.Add(CountryOfResidence, "CountryOfResidence");
		DefaultSymbols.Add(Gender, "Gender");
		DefaultSymbols.Add(PlaceOfBirth, "PlaceOfBirth");
		DefaultSymbols.Add(DateOfBirth, "DateOfBirth");
		DefaultSymbols.Add(PostalCode, "PostalCode");
		DefaultSymbols.Add(BusinessCategory, "BusinessCategory");
		DefaultSymbols.Add(TelephoneNumber, "TelephoneNumber");
		RFC2253Symbols.Add(C, "C");
		RFC2253Symbols.Add(O, "O");
		RFC2253Symbols.Add(OU, "OU");
		RFC2253Symbols.Add(CN, "CN");
		RFC2253Symbols.Add(L, "L");
		RFC2253Symbols.Add(ST, "ST");
		RFC2253Symbols.Add(Street, "STREET");
		RFC2253Symbols.Add(DC, "DC");
		RFC2253Symbols.Add(UID, "UID");
		RFC1779Symbols.Add(C, "C");
		RFC1779Symbols.Add(O, "O");
		RFC1779Symbols.Add(OU, "OU");
		RFC1779Symbols.Add(CN, "CN");
		RFC1779Symbols.Add(L, "L");
		RFC1779Symbols.Add(ST, "ST");
		RFC1779Symbols.Add(Street, "STREET");
		DefaultLookup.Add("c", C);
		DefaultLookup.Add("o", O);
		DefaultLookup.Add("t", T);
		DefaultLookup.Add("ou", OU);
		DefaultLookup.Add("cn", CN);
		DefaultLookup.Add("l", L);
		DefaultLookup.Add("st", ST);
		DefaultLookup.Add("serialnumber", SerialNumber);
		DefaultLookup.Add("street", Street);
		DefaultLookup.Add("emailaddress", E);
		DefaultLookup.Add("dc", DC);
		DefaultLookup.Add("e", E);
		DefaultLookup.Add("uid", UID);
		DefaultLookup.Add("surname", Surname);
		DefaultLookup.Add("givenname", GivenName);
		DefaultLookup.Add("initials", Initials);
		DefaultLookup.Add("generation", Generation);
		DefaultLookup.Add("unstructuredaddress", UnstructuredAddress);
		DefaultLookup.Add("unstructuredname", UnstructuredName);
		DefaultLookup.Add("uniqueidentifier", UniqueIdentifier);
		DefaultLookup.Add("dn", DnQualifier);
		DefaultLookup.Add("pseudonym", Pseudonym);
		DefaultLookup.Add("postaladdress", PostalAddress);
		DefaultLookup.Add("nameofbirth", NameAtBirth);
		DefaultLookup.Add("countryofcitizenship", CountryOfCitizenship);
		DefaultLookup.Add("countryofresidence", CountryOfResidence);
		DefaultLookup.Add("gender", Gender);
		DefaultLookup.Add("placeofbirth", PlaceOfBirth);
		DefaultLookup.Add("dateofbirth", DateOfBirth);
		DefaultLookup.Add("postalcode", PostalCode);
		DefaultLookup.Add("businesscategory", BusinessCategory);
		DefaultLookup.Add("telephonenumber", TelephoneNumber);
	}

	internal static X509Name GetName(Asn1Tag tag, bool isExplicit)
	{
		return GetName(Asn1Sequence.GetSequence(tag, isExplicit));
	}

	internal static X509Name GetName(object obj)
	{
		if (obj == null || obj is X509Name)
		{
			return (X509Name)obj;
		}
		if (obj != null)
		{
			return new X509Name(Asn1Sequence.GetSequence(obj));
		}
		throw new ArgumentException("Invalid entry");
	}

	protected X509Name(Asn1Sequence sequence)
	{
		m_sequence = sequence;
		foreach (Asn1Encode item in sequence)
		{
			Asn1Set asn1Set = Asn1Set.GetAsn1Set(item.GetAsn1());
			for (int i = 0; i < asn1Set.Count; i++)
			{
				Asn1Sequence sequence2 = Asn1Sequence.GetSequence(asn1Set[i].GetAsn1());
				if (sequence2.Count != 2)
				{
					throw new ArgumentException("Invalid length in sequence");
				}
				m_ordering.Add(DerObjectID.GetID(sequence2[0].GetAsn1()));
				Asn1 asn = sequence2[1].GetAsn1();
				if (asn is IAsn1String)
				{
					string text = ((IAsn1String)asn).GetString();
					if (text.StartsWith("#"))
					{
						text = "\\" + text;
					}
					m_values.Add(text);
				}
				m_added.Add(i != 0);
			}
		}
	}

	public override Asn1 GetAsn1()
	{
		if (m_sequence == null)
		{
			Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
			Asn1EncodeCollection collection = new Asn1EncodeCollection();
			DerObjectID derObjectID = null;
			for (int i = 0; i != m_ordering.Count; i++)
			{
				DerObjectID derObjectID2 = (DerObjectID)m_ordering[i];
				_ = (string)m_values[i];
				if (derObjectID != null && !(bool)m_added[i])
				{
					asn1EncodeCollection.Add(new DerSet(collection));
					collection = new Asn1EncodeCollection();
				}
				derObjectID = derObjectID2;
			}
			asn1EncodeCollection.Add(new DerSet(collection));
			m_sequence = new DerSequence(asn1EncodeCollection);
		}
		return m_sequence;
	}

	private void AppendValue(StringBuilder builder, IDictionary symbols, DerObjectID id, string value)
	{
		string text = (string)symbols[id];
		if (text != null)
		{
			builder.Append(text);
		}
		else
		{
			builder.Append(id.ID);
		}
		builder.Append('=');
		int i = builder.Length;
		builder.Append(value);
		int num = builder.Length;
		if (value.StartsWith("\\#"))
		{
			i += 2;
		}
		for (; i != num; i++)
		{
			if (builder[i] == ',' || builder[i] == '"' || builder[i] == '\\' || builder[i] == '+' || builder[i] == '=' || builder[i] == '<' || builder[i] == '>' || builder[i] == ';')
			{
				builder.Insert(i++, "\\");
				num++;
			}
		}
	}

	internal string ToString(bool isReverse, IDictionary symbols)
	{
		List<object> list = new List<object>();
		StringBuilder stringBuilder = null;
		for (int i = 0; i < m_ordering.Count; i++)
		{
			if (m_ordering.Count <= m_values.Count)
			{
				if ((bool)m_added[i])
				{
					stringBuilder.Append('+');
					AppendValue(stringBuilder, symbols, (DerObjectID)m_ordering[i], (string)m_values[i]);
				}
				else
				{
					stringBuilder = new StringBuilder();
					AppendValue(stringBuilder, symbols, (DerObjectID)m_ordering[i], (string)m_values[i]);
					list.Add(stringBuilder);
				}
			}
		}
		if (isReverse)
		{
			list.Reverse();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		if (list.Count > 0)
		{
			stringBuilder2.Append(list[0].ToString());
			for (int j = 1; j < list.Count; j++)
			{
				stringBuilder2.Append(',');
				stringBuilder2.Append(list[j].ToString());
			}
		}
		return stringBuilder2.ToString();
	}

	public override string ToString()
	{
		return ToString(DefaultReverse, DefaultSymbols);
	}

	internal bool Equivalent(X509Name other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		int count = m_ordering.Count;
		if (count != other.m_ordering.Count)
		{
			return false;
		}
		bool[] array = new bool[count];
		int num;
		int num2;
		int num3;
		if (m_ordering[0].Equals(other.m_ordering[0]))
		{
			num = 0;
			num2 = count;
			num3 = 1;
		}
		else
		{
			num = count - 1;
			num2 = -1;
			num3 = -1;
		}
		for (int i = num; i != num2; i += num3)
		{
			bool flag = false;
			DerObjectID derObjectID = (DerObjectID)m_ordering[i];
			string text = (string)m_values[i];
			for (int j = 0; j < count; j++)
			{
				if (array[j])
				{
					continue;
				}
				DerObjectID obj = (DerObjectID)other.m_ordering[j];
				if (derObjectID.Equals(obj))
				{
					string other2 = (string)other.m_values[j];
					if (CheckStringEquivalent(text, other2))
					{
						array[j] = true;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckStringEquivalent(string text, string other)
	{
		string text2 = RecognizeText(text);
		string text3 = RecognizeText(other);
		if (!text2.Equals(text3))
		{
			text2 = SkipSequence(text2);
			text3 = SkipSequence(text3);
			if (!text2.Equals(text3))
			{
				return false;
			}
		}
		return true;
	}

	private string RecognizeText(string text)
	{
		string text2 = text.ToLowerInvariant().Trim();
		if (text2.StartsWith("#"))
		{
			Asn1 asn = Asn1.FromByteArray(new PdfString().HexToBytes(text2.Substring(1)));
			if (asn is IAsn1String)
			{
				text2 = ((IAsn1String)asn).GetString().ToLowerInvariant().Trim();
			}
		}
		return text2;
	}

	private string SkipSequence(string sequence)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (sequence.Length != 0)
		{
			char c = sequence[0];
			stringBuilder.Append(c);
			for (int i = 1; i < sequence.Length; i++)
			{
				char c2 = sequence[i];
				if (c != ' ' || c2 != ' ')
				{
					stringBuilder.Append(c2);
				}
				c = c2;
			}
		}
		return stringBuilder.ToString();
	}
}
