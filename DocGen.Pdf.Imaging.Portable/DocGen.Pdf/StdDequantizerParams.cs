namespace DocGen.Pdf;

internal class StdDequantizerParams : DequantizerParams
{
	public int[][] exp;

	public float[][] nStep;

	public override int DequantizerType => QuantizationType_Fields.Q_TYPE_SCALAR_DZ;
}
