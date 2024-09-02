namespace DocGen.PdfViewer.Base;

internal class GraphicsObject
{
	private Matrix m_transformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

	private GraphicsState m_graphicState;

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

	internal GraphicsState Save()
	{
		m_graphicState = new GraphicsState();
		m_graphicState.TransformMatrix = m_transformMatrix;
		return m_graphicState;
	}

	internal void Restore(GraphicsState graphicState)
	{
		m_transformMatrix = graphicState.TransformMatrix;
	}

	internal void MultiplyTransform(Matrix matrix)
	{
		m_transformMatrix *= matrix;
	}

	internal void ScaleTransform(double scaleX, double scaleY)
	{
		m_transformMatrix = m_transformMatrix.Scale(scaleX, scaleY, 0.0, 0.0);
	}

	internal void TranslateTransform(double offsetX, double offsetY)
	{
		m_transformMatrix = m_transformMatrix.Translate(offsetX, offsetY);
	}

	internal void RotateTransform(double angle)
	{
		m_transformMatrix = m_transformMatrix.Rotate(angle, 0.0, 0.0);
	}
}
