using System;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

internal class PdfTransformationMatrix : ICloneable
{
	private const double DegRadFactor = Math.PI / 180.0;

	private const double RadDegFactor = 180.0 / Math.PI;

	private Matrix m_matrix;

	public float OffsetX => m_matrix.OffsetX;

	public float OffsetY => m_matrix.OffsetY;

	protected internal Matrix Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			if (m_matrix != value)
			{
				m_matrix = value;
			}
		}
	}

	public PdfTransformationMatrix()
	{
		m_matrix = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
	}

	internal PdfTransformationMatrix(bool value)
	{
		m_matrix = new Matrix(1f, 0f, 0f, -1f, 0f, 0f);
	}

	public void Translate(SizeF offsets)
	{
		Translate(offsets.Width, offsets.Height);
	}

	public void Translate(float offsetX, float offsetY)
	{
		m_matrix.Translate(offsetX, offsetY);
	}

	public void Scale(SizeF scales)
	{
		Scale(scales.Width, scales.Height);
	}

	public void Scale(float scaleX, float scaleY)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		pdfTransformationMatrix.m_matrix = new Matrix(scaleX, 0f, 0f, scaleY, 0f, 0f);
		Multiply(pdfTransformationMatrix);
	}

	public void Rotate(float angle)
	{
		angle = Convert.ToSingle((double)angle * Math.PI / 180.0);
		m_matrix.Elements[0] = Convert.ToSingle(Math.Cos(angle));
		m_matrix.Elements[1] = Convert.ToSingle(Math.Sin(angle));
		m_matrix.Elements[2] = Convert.ToSingle(0.0 - Math.Sin(angle));
		m_matrix.Elements[3] = Convert.ToSingle(Math.Cos(angle));
	}

	public void Skew(SizeF angles)
	{
		throw new NotImplementedException();
	}

	public void Skew(float angleX, float angleY)
	{
		float m = (float)Math.Tan(DegressToRadians(angleX));
		float m2 = (float)Math.Tan(DegressToRadians(angleY));
		Matrix matrix = new Matrix(1f, m, m2, 1f, 0f, 0f);
		m_matrix.Multiply(matrix);
	}

	public void Shear(float shearX, float shearY)
	{
		throw new NotImplementedException();
	}

	public void RotateAt(float angle, PointF point)
	{
		throw new NotImplementedException();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		char value = ' ';
		int i = 0;
		for (int num = m_matrix.Elements.Length; i < num; i++)
		{
			stringBuilder.Append(PdfNumber.FloatToString(m_matrix.Elements[i]));
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	internal Matrix Multiply(float[] Elements, Matrix matrix)
	{
		float[] array = new float[matrix.Elements.Length];
		array[0] = Elements[0] * matrix.Elements[0] + Elements[2] * matrix.Elements[1];
		array[1] = Elements[1] * matrix.Elements[0] + Elements[3] * matrix.Elements[1];
		array[2] = Elements[0] * matrix.Elements[2] + Elements[2] * matrix.Elements[3];
		array[3] = Elements[1] * matrix.Elements[2] + Elements[3] * matrix.Elements[3];
		array[4] = Elements[0] * matrix.Elements[4] + Elements[2] * matrix.Elements[5] + Elements[4];
		array[5] = Elements[1] * matrix.Elements[4] + Elements[3] * matrix.Elements[5] + Elements[5];
		return new Matrix(array[0], array[1], array[2], array[3], m_matrix.OffsetX, m_matrix.OffsetY);
	}

	protected internal void Multiply(PdfTransformationMatrix matrix)
	{
		m_matrix.Multiply(matrix.Matrix);
	}

	public static double DegressToRadians(float degreesX)
	{
		return Math.PI / 180.0 * (double)degreesX;
	}

	public static double RadiansToDegress(float radians)
	{
		return 180.0 / Math.PI * (double)radians;
	}

	public PdfTransformationMatrix Clone()
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		Matrix matrix = new Matrix(m_matrix.Elements[0], m_matrix.Elements[1], m_matrix.Elements[2], m_matrix.Elements[3], m_matrix.Elements[4], m_matrix.Elements[5]);
		pdfTransformationMatrix.m_matrix = matrix;
		return pdfTransformationMatrix;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}
}
