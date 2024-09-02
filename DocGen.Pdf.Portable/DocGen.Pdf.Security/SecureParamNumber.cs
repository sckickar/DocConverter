using System;

namespace DocGen.Pdf.Security;

internal class SecureParamNumber : ICipherParam
{
	private readonly ICipherParam m_parameters;

	private readonly SecureRandomAlgorithm m_random;

	public SecureRandomAlgorithm Random => m_random;

	public ICipherParam Parameters => m_parameters;

	public SecureParamNumber(ICipherParam parameters, SecureRandomAlgorithm random)
	{
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		if (random == null)
		{
			throw new ArgumentNullException("random");
		}
		m_parameters = parameters;
		m_random = random;
	}

	public SecureParamNumber(ICipherParam parameters)
		: this(parameters, new SecureRandomAlgorithm())
	{
	}
}
