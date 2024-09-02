using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

internal class PdfPKCSCertificate
{
	internal class CertificateIdentifier
	{
		private readonly byte[] m_id;

		internal byte[] ID => m_id;

		internal CertificateIdentifier(CipherParameter pubKey)
		{
			m_id = CreateSubjectKeyID(pubKey).GetKeyIdentifier();
		}

		internal CertificateIdentifier(byte[] id)
		{
			m_id = id;
		}

		public override int GetHashCode()
		{
			return Asn1Constants.GetHashCode(m_id);
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (!(obj is CertificateIdentifier certificateIdentifier))
			{
				return false;
			}
			return Asn1Constants.AreEqual(m_id, certificateIdentifier.m_id);
		}
	}

	private class CertificateTable : IEnumerable
	{
		private readonly Dictionary<string, object> m_orig = new Dictionary<string, object>();

		private readonly Dictionary<string, object> m_keys = new Dictionary<string, object>();

		internal ICollection Keys => m_orig.Keys;

		internal object this[string key]
		{
			get
			{
				string key2 = key.ToLowerInvariant();
				if (m_keys.ContainsKey(key2))
				{
					return m_orig[(string)m_keys[key2]];
				}
				return null;
			}
			set
			{
				string key2 = key.ToLowerInvariant();
				if (m_keys.ContainsKey(key2))
				{
					m_orig.Remove((string)m_keys[key2]);
				}
				m_keys.Add(key2, key);
				m_orig.Add(key, value);
			}
		}

		internal void Clear()
		{
			m_orig.Clear();
			m_keys.Clear();
		}

		public IEnumerator GetEnumerator()
		{
			return m_orig.GetEnumerator();
		}

		internal object Remove(string key)
		{
			string key2 = key.ToLowerInvariant();
			string text = (string)m_keys[key2];
			if (text == null)
			{
				return null;
			}
			m_keys.Remove(key2);
			object result = m_orig[text];
			m_orig.Remove(text);
			return result;
		}
	}

	private readonly CertificateTable m_keys = new CertificateTable();

	private readonly CertificateTable m_certificates = new CertificateTable();

	private readonly Dictionary<string, string> m_localIdentifiers = new Dictionary<string, string>();

	private readonly Dictionary<CertificateIdentifier, X509Certificates> m_chainCertificates = new Dictionary<CertificateIdentifier, X509Certificates>();

	private readonly Dictionary<string, X509Certificates> m_keyCertificates = new Dictionary<string, X509Certificates>();

	internal IEnumerable KeyEnumerable => new EnumerableProxy(GetContentTable().Keys);

	private static SubjectKeyID CreateSubjectKeyID(CipherParameter publicKey)
	{
		if (publicKey is RsaKeyParam)
		{
			RsaKeyParam rsaKeyParam = (RsaKeyParam)publicKey;
			return new SubjectKeyID(new PublicKeyInformation(new Algorithms(PKCSOIDs.RsaEncryption, DerNull.Value), new RSAPublicKey(rsaKeyParam.Modulus, rsaKeyParam.Exponent).GetAsn1()));
		}
		if (publicKey is ECPublicKeyParam)
		{
			ECPublicKeyParam eCPublicKeyParam = (ECPublicKeyParam)publicKey;
			if (eCPublicKeyParam.AlgorithmName == "ECGOST3410")
			{
				if (eCPublicKeyParam.PublicKeyParamSet == null)
				{
					throw new Exception("Not a CryptoPro parameter set");
				}
				EllipticPoint pointQ = eCPublicKeyParam.PointQ;
				Number byteBI = pointQ.PointX.ToIntValue();
				Number byteBI2 = pointQ.PointY.ToIntValue();
				byte[] array = new byte[64];
				DecompressBytes(array, 0, byteBI);
				DecompressBytes(array, 32, byteBI2);
				ECGostAlgorithm eCGostAlgorithm = new ECGostAlgorithm(eCPublicKeyParam.PublicKeyParamSet, CRYPTOIDs.IDR3411X94);
				return new SubjectKeyID(new PublicKeyInformation(new Algorithms(CRYPTOIDs.IDR3410, eCGostAlgorithm.GetAsn1()), new DerOctet(array)));
			}
			ECX962Params eCX962Params;
			if (eCPublicKeyParam.PublicKeyParamSet == null)
			{
				EllipticCurveParams parameters = eCPublicKeyParam.Parameters;
				eCX962Params = new ECX962Params(new ECX9Field(parameters.Curve, parameters.PointG, parameters.NumberX, parameters.NumberY, parameters.ECSeed()));
			}
			else
			{
				eCX962Params = new ECX962Params(eCPublicKeyParam.PublicKeyParamSet);
			}
			Asn1Octet asn1Octet = (Asn1Octet)new ECx9Point(eCPublicKeyParam.PointQ).GetAsn1();
			return new SubjectKeyID(new PublicKeyInformation(new Algorithms(ECDSAOIDs.IdECPublicKey, eCX962Params.GetAsn1()), asn1Octet.GetOctets()));
		}
		throw new Exception("Invalid Key");
	}

	private static void DecompressBytes(byte[] encKey, int offset, Number byteBI)
	{
		byte[] array = byteBI.ToByteArray();
		int num = (byteBI.BitLength + 7) / 8;
		for (int i = 0; i < num; i++)
		{
			encKey[offset + i] = array[array.Length - 1 - i];
		}
	}

	internal PdfPKCSCertificate()
	{
	}

	public PdfPKCSCertificate(Stream input, char[] password)
		: this()
	{
		LoadCertificate(input, password);
	}

	internal void LoadCertificate(Stream input, char[] password)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		ContentInformation contentInformation = new PfxData((Asn1Sequence)Asn1.FromStream(input)).ContentInformation;
		bool flag = false;
		bool flag2 = password.Length == 0;
		m_keys.Clear();
		m_localIdentifiers.Clear();
		List<Asn1SequenceCollection> list = new List<Asn1SequenceCollection>();
		if (contentInformation.ContentType.ID.Equals(PKCSOIDs.Data.ID))
		{
			Asn1Sequence asn1Sequence = (Asn1Sequence)Asn1.FromByteArray(((Asn1Octet)contentInformation.Content).GetOctets());
			ContentInformation[] array = new ContentInformation[asn1Sequence.Count];
			for (int i = 0; i != array.Length; i++)
			{
				array[i] = ContentInformation.GetInformation(asn1Sequence[i]);
			}
			ContentInformation[] array2 = (ContentInformation[])array.Clone();
			foreach (ContentInformation contentInformation2 in array2)
			{
				DerObjectID contentType = contentInformation2.ContentType;
				if (contentType.ID.Equals(PKCSOIDs.Data.ID))
				{
					foreach (Asn1Sequence item in (Asn1Sequence)Asn1.FromByteArray(((Asn1Octet)contentInformation2.Content).GetOctets()))
					{
						Asn1SequenceCollection asn1SequenceCollection = new Asn1SequenceCollection(item);
						if (asn1SequenceCollection.ID.ID.Equals(PKCSOIDs.Pkcs8ShroudedKeyBag.ID))
						{
							EncryptedPrivateKey encryptedPrivateKeyInformation = EncryptedPrivateKey.GetEncryptedPrivateKeyInformation(asn1SequenceCollection.Value);
							KeyInformation keyInformation = KeyInformationCollection.CreatePrivateKeyInfo(password, flag2, encryptedPrivateKeyInformation);
							RsaPrivateKeyParam rsaPrivateKeyParam = null;
							ECPrivateKey eCPrivateKey = null;
							if (keyInformation.AlgorithmID.ObjectID.ID.Equals(PKCSOIDs.RsaEncryption.ID) || keyInformation.AlgorithmID.ObjectID.ID.Equals(X509Objects.IdEARsa.ID))
							{
								RSAKey rSAKey = new RSAKey(Asn1Sequence.GetSequence(keyInformation.PrivateKey));
								rsaPrivateKeyParam = new RsaPrivateKeyParam(rSAKey.Modulus, rSAKey.PublicExponent, rSAKey.PrivateExponent, rSAKey.Prime1, rSAKey.Prime2, rSAKey.Exponent1, rSAKey.Exponent2, rSAKey.Coefficient);
							}
							else if (keyInformation.AlgorithmID.ObjectID.ID.Equals(ECDSAOIDs.IdECPublicKey.ID))
							{
								ECX962Params eCX962Params = new ECX962Params(keyInformation.AlgorithmID.Parameters.GetAsn1());
								ECX9Field eCX9Field = ((!eCX962Params.IsNamedCurve) ? new ECX9Field((Asn1Sequence)eCX962Params.Parameters) : EllipicCryptoKeyGen.GetECCurveByObjectID((DerObjectID)eCX962Params.Parameters));
								Number key = new ECPrivateKeyParam(Asn1Sequence.GetSequence(keyInformation.PrivateKey)).GetKey();
								if (eCX962Params.IsNamedCurve)
								{
									eCPrivateKey = new ECPrivateKey("EC", key, (DerObjectID)eCX962Params.Parameters);
								}
								EllipticCurveParams parameters = new EllipticCurveParams(eCX9Field.Curve, eCX9Field.PointG, eCX9Field.NumberX, eCX9Field.NumberY, eCX9Field.Seed());
								eCPrivateKey = new ECPrivateKey(key, parameters);
							}
							CipherParameter cipherParameter = rsaPrivateKeyParam;
							if (cipherParameter == null && eCPrivateKey != null)
							{
								cipherParameter = eCPrivateKey;
							}
							Dictionary<string, object> dictionary = new Dictionary<string, object>();
							KeyEntry value = new KeyEntry(cipherParameter, dictionary);
							string text = null;
							Asn1Octet asn1Octet = null;
							if (asn1SequenceCollection.Attributes != null)
							{
								foreach (Asn1Sequence attribute in asn1SequenceCollection.Attributes)
								{
									DerObjectID iD = DerObjectID.GetID(attribute[0]);
									Asn1Set asn1Set = (Asn1Set)attribute[1];
									Asn1Encode asn1Encode = null;
									if (asn1Set.Count <= 0)
									{
										continue;
									}
									asn1Encode = asn1Set[0];
									if (dictionary.ContainsKey(iD.ID))
									{
										if (!dictionary[iD.ID].Equals(asn1Encode))
										{
											throw new IOException("attempt to add existing attribute with different value");
										}
									}
									else
									{
										dictionary.Add(iD.ID, asn1Encode);
									}
									if (iD.Equals(PKCSOIDs.Pkcs9AtFriendlyName))
									{
										text = ((DerBmpString)asn1Encode).GetString();
										m_keys[text] = value;
									}
									else if (iD.Equals(PKCSOIDs.Pkcs9AtLocalKeyID))
									{
										asn1Octet = (Asn1Octet)asn1Encode;
									}
								}
							}
							if (asn1Octet != null)
							{
								string text2 = PdfString.BytesToHex(asn1Octet.GetOctets());
								if (text == null)
								{
									m_keys[text2] = value;
								}
								else
								{
									m_localIdentifiers[text] = text2;
								}
							}
							else
							{
								flag = true;
								m_keys["unmarked"] = value;
							}
						}
						else if (asn1SequenceCollection.ID.Equals(PKCSOIDs.CertBag))
						{
							list.Add(asn1SequenceCollection);
						}
					}
				}
				else
				{
					if (!contentType.ID.Equals(PKCSOIDs.EncryptedData.ID))
					{
						continue;
					}
					Asn1Sequence obj2 = contentInformation2.Content as Asn1Sequence;
					if (obj2.Count != 2)
					{
						throw new ArgumentException("Invalid length of the sequence");
					}
					if (((DerInteger)obj2[0]).Value.IntValue != 0)
					{
						throw new ArgumentException("Invalid sequence version");
					}
					Asn1Sequence asn1Sequence2 = (Asn1Sequence)obj2[1];
					Asn1Octet asn1Octet2 = null;
					if (asn1Sequence2.Count == 3)
					{
						asn1Octet2 = Asn1Octet.GetOctetString((DerTag)asn1Sequence2[2], isExplicit: false);
					}
					foreach (Asn1Sequence item2 in (Asn1Sequence)Asn1.FromByteArray(GetCryptographicData(forEncryption: false, Algorithms.GetAlgorithms(asn1Sequence2[1]), password, flag2, asn1Octet2.GetOctets())))
					{
						Asn1SequenceCollection asn1SequenceCollection2 = new Asn1SequenceCollection(item2);
						if (asn1SequenceCollection2.ID.ID.Equals(PKCSOIDs.CertBag.ID))
						{
							list.Add(asn1SequenceCollection2);
						}
						else if (asn1SequenceCollection2.ID.ID.Equals(PKCSOIDs.Pkcs8ShroudedKeyBag.ID))
						{
							EncryptedPrivateKey encryptedPrivateKeyInformation2 = EncryptedPrivateKey.GetEncryptedPrivateKeyInformation(asn1SequenceCollection2.Value);
							KeyInformation keyInformation2 = KeyInformationCollection.CreatePrivateKeyInfo(password, flag2, encryptedPrivateKeyInformation2);
							RsaPrivateKeyParam rsaPrivateKeyParam2 = null;
							if (keyInformation2.AlgorithmID.ObjectID.ID.Equals(PKCSOIDs.RsaEncryption.ID) || keyInformation2.AlgorithmID.ObjectID.ID.Equals(X509Objects.IdEARsa.ID))
							{
								RSAKey rSAKey2 = new RSAKey(Asn1Sequence.GetSequence(keyInformation2.PrivateKey));
								rsaPrivateKeyParam2 = new RsaPrivateKeyParam(rSAKey2.Modulus, rSAKey2.PublicExponent, rSAKey2.PrivateExponent, rSAKey2.Prime1, rSAKey2.Prime2, rSAKey2.Exponent1, rSAKey2.Exponent2, rSAKey2.Coefficient);
							}
							RsaPrivateKeyParam key2 = rsaPrivateKeyParam2;
							Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
							KeyEntry value2 = new KeyEntry(key2, dictionary2);
							string text3 = null;
							Asn1Octet asn1Octet3 = null;
							foreach (Asn1Sequence attribute2 in asn1SequenceCollection2.Attributes)
							{
								DerObjectID derObjectID = (DerObjectID)attribute2[0];
								Asn1Set asn1Set2 = (Asn1Set)attribute2[1];
								Asn1Encode asn1Encode2 = null;
								if (asn1Set2.Count <= 0)
								{
									continue;
								}
								asn1Encode2 = asn1Set2[0];
								if (dictionary2.ContainsKey(derObjectID.ID))
								{
									if (!dictionary2[derObjectID.ID].Equals(asn1Encode2))
									{
										throw new IOException("attempt to add existing attribute with different value");
									}
								}
								else
								{
									dictionary2.Add(derObjectID.ID, asn1Encode2);
								}
								if (derObjectID.Equals(PKCSOIDs.Pkcs9AtFriendlyName))
								{
									text3 = ((DerBmpString)asn1Encode2).GetString();
									m_keys[text3] = value2;
								}
								else if (derObjectID.Equals(PKCSOIDs.Pkcs9AtLocalKeyID))
								{
									asn1Octet3 = (Asn1Octet)asn1Encode2;
								}
							}
							string text4 = PdfString.BytesToHex(asn1Octet3.GetOctets());
							if (text3 == null)
							{
								m_keys[text4] = value2;
							}
							else
							{
								m_localIdentifiers[text3] = text4;
							}
						}
						else
						{
							if (!asn1SequenceCollection2.ID.Equals(PKCSOIDs.KeyBag))
							{
								continue;
							}
							KeyInformation information = KeyInformation.GetInformation(asn1SequenceCollection2.Value);
							RsaPrivateKeyParam rsaPrivateKeyParam3 = null;
							if (information.AlgorithmID.ObjectID.ID.Equals(PKCSOIDs.RsaEncryption.ID) || information.AlgorithmID.ObjectID.ID.Equals(X509Objects.IdEARsa.ID))
							{
								RSAKey rSAKey3 = new RSAKey(Asn1Sequence.GetSequence(information.PrivateKey));
								rsaPrivateKeyParam3 = new RsaPrivateKeyParam(rSAKey3.Modulus, rSAKey3.PublicExponent, rSAKey3.PrivateExponent, rSAKey3.Prime1, rSAKey3.Prime2, rSAKey3.Exponent1, rSAKey3.Exponent2, rSAKey3.Coefficient);
							}
							RsaPrivateKeyParam key3 = rsaPrivateKeyParam3;
							string text5 = null;
							Asn1Octet asn1Octet4 = null;
							Dictionary<object, object> dictionary3 = new Dictionary<object, object>();
							KeyEntry value3 = new KeyEntry(key3, dictionary3);
							foreach (Asn1Sequence attribute3 in asn1SequenceCollection2.Attributes)
							{
								DerObjectID derObjectID2 = (DerObjectID)attribute3[0];
								Asn1Set asn1Set3 = (Asn1Set)attribute3[1];
								Asn1Encode asn1Encode3 = null;
								if (asn1Set3.Count <= 0)
								{
									continue;
								}
								asn1Encode3 = asn1Set3[0];
								if (dictionary3.ContainsKey(derObjectID2.ID))
								{
									if (!dictionary3[derObjectID2.ID].Equals(asn1Encode3))
									{
										throw new IOException("attempt to add existing attribute with different value");
									}
								}
								else
								{
									dictionary3.Add(derObjectID2.ID, asn1Encode3);
								}
								if (derObjectID2.Equals(PKCSOIDs.Pkcs9AtFriendlyName))
								{
									text5 = ((DerBmpString)asn1Encode3).GetString();
									m_keys[text5] = value3;
								}
								else if (derObjectID2.Equals(PKCSOIDs.Pkcs9AtLocalKeyID))
								{
									asn1Octet4 = (Asn1Octet)asn1Encode3;
								}
							}
							string text6 = PdfString.BytesToHex(asn1Octet4.GetOctets());
							if (text5 == null)
							{
								m_keys[text6] = value3;
							}
							else
							{
								m_localIdentifiers[text5] = text6;
							}
						}
					}
				}
			}
		}
		m_certificates.Clear();
		m_chainCertificates.Clear();
		m_keyCertificates.Clear();
		foreach (Asn1SequenceCollection item3 in list)
		{
			byte[] octets = ((Asn1Octet)Asn1Tag.GetTag(((Asn1Sequence)item3.Value)[1]).GetObject()).GetOctets();
			X509Certificate x509Certificate = new X509CertificateParser().ReadCertificate(new MemoryStream(octets, writable: false));
			Dictionary<object, object> dictionary4 = new Dictionary<object, object>();
			Asn1Octet asn1Octet5 = null;
			string text7 = null;
			if (item3.Attributes != null)
			{
				foreach (Asn1Sequence attribute4 in item3.Attributes)
				{
					DerObjectID iD2 = DerObjectID.GetID(attribute4[0]);
					Asn1Set asn1Set4 = (Asn1Set)attribute4[1];
					if (asn1Set4.Count <= 0)
					{
						continue;
					}
					Asn1Encode asn1Encode4 = asn1Set4[0];
					if (dictionary4.ContainsKey(iD2.ID))
					{
						if (!dictionary4[iD2.ID].Equals(asn1Encode4))
						{
							throw new IOException("attempt to add existing attribute with different value");
						}
					}
					else
					{
						dictionary4.Add(iD2.ID, asn1Encode4);
					}
					if (iD2.Equals(PKCSOIDs.Pkcs9AtFriendlyName))
					{
						text7 = ((DerBmpString)asn1Encode4).GetString();
					}
					else if (iD2.Equals(PKCSOIDs.Pkcs9AtLocalKeyID))
					{
						asn1Octet5 = (Asn1Octet)asn1Encode4;
					}
				}
			}
			CertificateIdentifier certificateIdentifier = new CertificateIdentifier(x509Certificate.GetPublicKey());
			X509Certificates value4 = new X509Certificates(x509Certificate);
			m_chainCertificates[certificateIdentifier] = value4;
			if (flag)
			{
				if (m_keyCertificates.Count == 0)
				{
					string key4 = PdfString.BytesToHex(certificateIdentifier.ID);
					m_keyCertificates[key4] = value4;
					object value5 = m_keys["unmarked"];
					m_keys.Remove("unmarked");
					m_keys[key4] = value5;
				}
			}
			else
			{
				if (asn1Octet5 != null)
				{
					string key5 = PdfString.BytesToHex(asn1Octet5.GetOctets());
					m_keyCertificates[key5] = value4;
				}
				if (text7 != null)
				{
					m_certificates[text7] = value4;
				}
			}
		}
	}

	internal KeyEntry GetKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return (KeyEntry)m_keys[key];
	}

	internal bool IsCertificate(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (m_certificates[key] != null)
		{
			return m_keys[key] == null;
		}
		return false;
	}

	internal bool IsKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return m_keys[key] != null;
	}

	private IDictionary GetContentTable()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key3 in m_certificates.Keys)
		{
			dictionary[key3] = "cert";
		}
		foreach (string key4 in m_keys.Keys)
		{
			if (!dictionary.ContainsKey(key4))
			{
				dictionary.Add(key4, "Key");
			}
		}
		return dictionary;
	}

	internal X509Certificates GetCertificate(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		X509Certificates x509Certificates = (X509Certificates)m_certificates[key];
		if (x509Certificates == null)
		{
			string text = null;
			if (m_localIdentifiers.ContainsKey(key))
			{
				text = m_localIdentifiers[key];
			}
			if (text != null)
			{
				if (m_keyCertificates.ContainsKey(text))
				{
					x509Certificates = m_keyCertificates[text];
				}
			}
			else if (m_keyCertificates.ContainsKey(key))
			{
				x509Certificates = m_keyCertificates[key];
			}
		}
		return x509Certificates;
	}

	internal X509Certificates[] GetCertificateChain(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (!IsKey(key))
		{
			return null;
		}
		X509Certificates x509Certificates = GetCertificate(key);
		if (x509Certificates != null)
		{
			List<X509Certificates> list = new List<X509Certificates>();
			while (x509Certificates != null)
			{
				X509Certificate certificate = x509Certificates.Certificate;
				X509Certificates x509Certificates2 = null;
				Asn1Octet extension = certificate.GetExtension(X509Extensions.AuthorityKeyIdentifier);
				if (extension != null)
				{
					KeyIdentifier keyIdentifier = KeyIdentifier.GetKeyIdentifier(Asn1.FromByteArray(extension.GetOctets()));
					if (keyIdentifier.KeyID != null && m_chainCertificates.ContainsKey(new CertificateIdentifier(keyIdentifier.KeyID)))
					{
						x509Certificates2 = m_chainCertificates[new CertificateIdentifier(keyIdentifier.KeyID)];
					}
				}
				if (x509Certificates2 == null)
				{
					X509Name issuerDN = certificate.IssuerDN;
					X509Name subjectDN = certificate.SubjectDN;
					if (!object.Equals(issuerDN, subjectDN))
					{
						foreach (CertificateIdentifier key2 in m_chainCertificates.Keys)
						{
							X509Certificates x509Certificates3 = null;
							if (m_chainCertificates.ContainsKey(key2))
							{
								x509Certificates3 = m_chainCertificates[key2];
							}
							X509Certificate certificate2 = x509Certificates3.Certificate;
							if (object.Equals(certificate2.SubjectDN, issuerDN))
							{
								try
								{
									certificate.Verify(certificate2.GetPublicKey());
									x509Certificates2 = x509Certificates3;
								}
								catch (Exception)
								{
									continue;
								}
								break;
							}
						}
					}
				}
				list.Add(x509Certificates);
				x509Certificates = ((x509Certificates2 == x509Certificates) ? null : x509Certificates2);
			}
			X509Certificates[] array = new X509Certificates[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = list[i];
			}
			return array;
		}
		return null;
	}

	private static byte[] GetCryptographicData(bool forEncryption, Algorithms id, char[] password, bool isZero, byte[] data)
	{
		PasswordUtility passwordUtility = new PasswordUtility();
		if (!(passwordUtility.CreateEncoder(id.ObjectID) is IBufferedCipher bufferedCipher))
		{
			throw new Exception("Invalid encryption algorithm : " + id.ObjectID);
		}
		ICipherParam parameters = passwordUtility.GenerateCipherParameters(parameters: PKCS12PasswordParameter.GetPBEParameter(id.Parameters), algorithmOid: id.ObjectID, password: password, isWrong: isZero);
		bufferedCipher.Initialize(forEncryption, parameters);
		return bufferedCipher.DoFinal(data);
	}
}
