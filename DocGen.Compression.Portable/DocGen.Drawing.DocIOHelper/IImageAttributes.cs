namespace DocGen.Drawing.DocIOHelper;

internal interface IImageAttributes
{
	void SetColorMatrix(IColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type);

	void SetGamma(float gamma, ColorAdjustType type);
}
