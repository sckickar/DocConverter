using System;

namespace DocGen.Pdf.Security;

internal class KeyGenParam
{
	private SecureRandomAlgorithm randomNumber;

	private int strength;

	internal SecureRandomAlgorithm Random => randomNumber;

	internal int Strength => strength;

	internal KeyGenParam(SecureRandomAlgorithm randomNumber, int strength)
	{
		if (randomNumber == null)
		{
			throw new ArgumentNullException("randomNumber");
		}
		if (strength < 1)
		{
			throw new ArgumentException("Must be a positive value");
		}
		this.randomNumber = randomNumber;
		this.strength = strength;
	}
}
