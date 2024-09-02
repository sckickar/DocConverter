namespace DocGen.Drawing.SkiaSharpHelper;

internal class GraphicsState
{
	internal int m_nativeState;

	internal GraphicsState(int state)
	{
		m_nativeState = state;
	}
}
