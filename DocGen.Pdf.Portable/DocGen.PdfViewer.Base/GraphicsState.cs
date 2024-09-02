namespace DocGen.PdfViewer.Base;

internal class GraphicsState
{
	private Matrix m_transformMatrix;

	internal Matrix TransformMatrix
	{
		get
		{
			return m_transformMatrix;
		}
		set
		{
			m_transformMatrix = value;
		}
	}

	internal void Save(GraphicsState graphicState)
	{
		m_transformMatrix = graphicState.TransformMatrix;
	}

	internal void Restore(GraphicsState graphicState)
	{
		m_transformMatrix = graphicState.TransformMatrix;
	}
}
