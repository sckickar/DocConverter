using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class X509RevocationResponse : X509ExtensionBase
{
	private OcspHelper m_helper;

	private ResponseInformation m_data;

	internal byte[] EncodedBytes => m_helper.GetEncoded();

	internal OneTimeResponse[] Responses
	{
		get
		{
			Asn1Sequence sequence = m_data.Sequence;
			OneTimeResponse[] array = new OneTimeResponse[sequence.Count];
			for (int i = 0; i != array.Length; i++)
			{
				OneTimeResponseHelper oneTimeResponseHelper = new OneTimeResponseHelper();
				array[i] = new OneTimeResponse(oneTimeResponseHelper.GetResponse(sequence[i]));
			}
			return array;
		}
	}

	internal X509Certificate[] Certificates
	{
		get
		{
			IList list = new List<X509Certificate>();
			Asn1Sequence sequence = m_helper.Sequence;
			if (sequence != null)
			{
				foreach (Asn1Encode item in sequence)
				{
					list.Add(new X509CertificateParser().ReadCertificate(item.GetEncoded()));
				}
			}
			X509Certificate[] array = new X509Certificate[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = (X509Certificate)list[i];
			}
			return array;
		}
	}

	internal X509RevocationResponse(OcspHelper helper)
	{
		m_helper = helper;
		m_data = helper.ResponseInformation;
	}

	protected override X509Extensions GetX509Extensions()
	{
		return null;
	}

	internal bool Verify(CipherParameter publicKey)
	{
		SignerUtilities signerUtilities = new SignerUtilities();
		string algorithmName = signerUtilities.GetAlgorithmName(m_helper.Algorithm.ObjectID);
		ISigner signer = signerUtilities.GetSigner(algorithmName);
		signer.Initialize(isSigning: false, publicKey);
		byte[] derEncoded = m_data.GetDerEncoded();
		signer.BlockUpdate(derEncoded, 0, derEncoded.Length);
		return signer.ValidateSignature(m_helper.Signature.GetBytes());
	}
}
