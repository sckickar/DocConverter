using System;

namespace DocGen.Pdf.Security;

internal class ECCipherKeyParam
{
	private readonly CipherParameter m_publicKeyParameter;

	private readonly CipherParameter m_privateKeyParameter;

	internal ECCipherKeyParam(CipherParameter publicKeyParameter, CipherParameter privateKeyParameter)
	{
		if (publicKeyParameter.IsPrivate)
		{
			throw new ArgumentException("Expected a public key", "publicParameter");
		}
		if (!privateKeyParameter.IsPrivate)
		{
			throw new ArgumentException("Expected a private key", "privateParameter");
		}
		m_publicKeyParameter = publicKeyParameter;
		m_privateKeyParameter = privateKeyParameter;
	}
}
