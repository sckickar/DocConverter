using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class OcspRequestCreator
{
	private class RequestCreatorHelper
	{
		private CertificateIdentity m_id;

		private X509Extensions m_extensions;

		internal RequestCreatorHelper(CertificateIdentity id, X509Extensions extensions)
		{
			m_id = id;
			m_extensions = extensions;
		}

		internal RevocationRequest ToRequest()
		{
			return new RevocationRequest(m_id.ID, m_extensions);
		}
	}

	private IList m_list = new List<object>();

	private OcspTag m_requestorName;

	private X509Extensions m_requestExtensions;

	internal void AddRequest(CertificateIdentity id)
	{
		m_list.Add(new RequestCreatorHelper(id, null));
	}

	internal void SetRequestExtensions(X509Extensions extensions)
	{
		m_requestExtensions = extensions;
	}

	private OcspRequestHelper CreateRequest(DerObjectID signingAlgorithm, CipherParameter privateKey, X509Certificate[] chain, int count)
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		foreach (RequestCreatorHelper item in m_list)
		{
			try
			{
				asn1EncodeCollection.Add(item.ToRequest());
			}
			catch (Exception)
			{
				throw new Exception("Invalid request creation");
			}
		}
		return new OcspRequestHelper(new RevocationListRequest(new OcspRequestCollection(m_requestorName, new DerSequence(asn1EncodeCollection), m_requestExtensions)));
	}

	internal OcspRequestHelper Generate()
	{
		return CreateRequest(null, null, null, 0);
	}
}
