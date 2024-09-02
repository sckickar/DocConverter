using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.Drawing;

namespace DocGen.Pdf.IO;

internal class PdfMatrix
{
	private PdfReader m_contentStream;

	private string m_key;

	private float m_width;

	private float m_height;

	private float m_x;

	private float m_y;

	private float[] m_translationMatrix;

	private float[] m_scaleMatrix;

	private SizeF m_pageSize;

	internal List<string> m_token = new List<string>();

	private string m_marginToken = string.Empty;

	private string m_rectToken = string.Empty;

	private string m_prevRectToken = string.Empty;

	private float m_leftMargin;

	private float m_topMargin;

	internal RectangleF m_scaledBounds;

	public float GetScaleX => m_x;

	public float GetScaleY => m_y;

	public float GetHeight => m_height;

	public float GetWidth => m_width;

	public float LeftMargin => m_leftMargin;

	public float TopMargin => m_topMargin;

	public PdfMatrix()
	{
	}

	public PdfMatrix(PdfReader ContentStream, string key, SizeF pageSize)
	{
		m_contentStream = ContentStream;
		m_key = key;
		m_pageSize = pageSize;
		List<string> matrixString = MatrixCalculation();
		ConvertToArray(matrixString);
		if (m_token.Count == 2)
		{
			if (m_translationMatrix != null)
			{
				SetTranslationMatrix();
			}
			else if (m_x == 0f && m_y == 0f && m_width == 0f && m_height == 0f)
			{
				SetScaleMatrix(pageSize);
			}
		}
	}

	private List<string> MatrixCalculation()
	{
		m_contentStream.Position = 0L;
		string text = m_contentStream.ReadContent();
		string text2 = string.Empty;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = true;
		while (text != string.Empty || m_contentStream.Stream.Position < m_contentStream.Stream.Length)
		{
			if (text.Contains(" re"))
			{
				if (m_rectToken != string.Empty)
				{
					m_prevRectToken = m_rectToken;
				}
				m_rectToken = text;
			}
			if (text.Contains("q") && !flag3)
			{
				flag2 = true;
			}
			if (text.Contains("Q"))
			{
				m_token.Clear();
			}
			if (flag2 && text.Contains(" cm"))
			{
				if (m_token.Count == 0)
				{
					string[] array = text.Split(new string[2] { " ", "q" }, StringSplitOptions.RemoveEmptyEntries);
					if (array.Length == 7)
					{
						float.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
						float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
						float.TryParse(array[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
						float.TryParse(array[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var result4);
						float.TryParse(array[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var result5);
						float.TryParse(array[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var result6);
						if ((result != 1f || result4 != 1f) && text2 != null)
						{
							string[] array2 = text2.Split(new string[2] { " ", "q" }, StringSplitOptions.RemoveEmptyEntries);
							if (array2.Length >= 6)
							{
								float.TryParse(array2[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result7);
								float.TryParse(array2[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
								float.TryParse(array2[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
								float.TryParse(array2[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var result10);
								float.TryParse(array2[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
								float.TryParse(array2[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
								if (result7 != 1f || result10 != 1f)
								{
									float width = result7 * result;
									float height = result10 * result4;
									m_scaledBounds = new RectangleF(result5, result6, width, height);
								}
							}
						}
						m_token.Add(text);
					}
				}
				else
				{
					m_token.Add(text);
				}
			}
			flag3 = false;
			if (text.Contains(" cm"))
			{
				string[] array3 = text.Split(new string[2] { " ", "q" }, StringSplitOptions.RemoveEmptyEntries);
				if (array3.Length == 7)
				{
					float.TryParse(array3[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result13);
					if ((double)result13 != 1.0)
					{
						if (!flag2)
						{
							m_token.Add(text);
						}
					}
					else if (m_token.Count == 0)
					{
						text2 = text;
					}
				}
			}
			if (!flag)
			{
				m_marginToken = text;
			}
			flag = !text.Contains("Translate co-ordinate system");
			text = m_contentStream.ReadLine();
			if (!text.Contains("q"))
			{
				if (text == string.Empty && m_token.Count == 0)
				{
					text = m_contentStream.ReadLine();
				}
				if (text.Contains(m_key) || text.Equals(""))
				{
					break;
				}
			}
		}
		if (m_token.Count == 0 && text2.Length > 0)
		{
			m_token.Add(text2);
		}
		return m_token;
	}

	private void SetScaleMatrix(SizeF pageSize)
	{
		float num = m_scaleMatrix[0];
		m_x = m_scaleMatrix[4];
		if (pageSize.Width > pageSize.Height)
		{
			m_y = float.Parse(m_scaleMatrix[5].ToString());
		}
		else
		{
			m_y = ((num + float.Parse(m_scaleMatrix[5].ToString()) > pageSize.Height) ? (num + float.Parse(m_scaleMatrix[5].ToString()) - pageSize.Height) : float.Parse(m_scaleMatrix[5].ToString()));
		}
		m_width = m_scaleMatrix[0];
		if (m_scaleMatrix[1] != 0f || m_scaleMatrix[3] != 0f)
		{
			m_width = (float)Math.Sqrt(Math.Pow(m_scaleMatrix[0], 2.0) + Math.Pow(m_scaleMatrix[1], 2.0));
		}
		m_height = m_scaleMatrix[4];
		if (m_scaleMatrix[1] != 0f || m_scaleMatrix[3] != 0f)
		{
			m_height = (float)Math.Sqrt(Math.Pow(m_scaleMatrix[3], 2.0) + Math.Pow(m_scaleMatrix[4], 2.0));
		}
		m_x += m_leftMargin;
		m_y += m_topMargin;
	}

	private void SetTranslationMatrix()
	{
		m_x = float.Parse(m_translationMatrix[4].ToString());
		m_y = float.Parse(m_translationMatrix[5].ToString());
		if (m_scaleMatrix != null)
		{
			m_width = m_scaleMatrix[0];
			if (m_scaleMatrix[1] != 0f || m_scaleMatrix[3] != 0f)
			{
				m_width = (float)Math.Sqrt(Math.Pow(float.Parse(m_scaleMatrix[0].ToString()), 2.0) + Math.Pow(float.Parse(m_scaleMatrix[1].ToString()), 2.0));
			}
			m_height = m_scaleMatrix[4];
			if (m_scaleMatrix[1] != 0f || m_scaleMatrix[3] != 0f)
			{
				m_height = (float)Math.Sqrt(Math.Pow(m_scaleMatrix[3], 2.0) + Math.Pow(m_scaleMatrix[4], 2.0));
			}
		}
		else
		{
			m_width = -1f;
			m_height = -1f;
		}
		m_x += m_leftMargin;
		m_y += m_topMargin;
		if (m_y >= m_height)
		{
			m_y -= m_height;
		}
	}

	private void ConvertToArray(List<string> matrixString)
	{
		if (m_token.Count <= 0)
		{
			return;
		}
		string text = m_token[0].ToString();
		if (m_marginToken.Length > 0)
		{
			string[] array = m_marginToken.Split(new string[1] { "cm" }, StringSplitOptions.None)[0].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			m_leftMargin = float.Parse(array[4].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
			m_topMargin = float.Parse(array[5].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
			m_leftMargin = Math.Abs(m_leftMargin);
			m_topMargin = Math.Abs(m_topMargin);
		}
		if (m_token.Count == 2)
		{
			foreach (string item in matrixString)
			{
				string[] array2 = item.Split(new string[1] { "cm" }, StringSplitOptions.None)[0].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2[0] == "q")
				{
					for (int i = 0; i < array2.Length - 1; i++)
					{
						array2[i] = array2[i + 1];
					}
				}
				string text2 = array2[0];
				if (!(text2 == "1.00"))
				{
					if (text2 == "1")
					{
						m_translationMatrix = new float[array2.Length];
						for (int j = 0; j < array2.Length; j++)
						{
							m_translationMatrix[j] = float.Parse(array2[j], NumberStyles.Float, CultureInfo.InvariantCulture);
						}
					}
					else if (m_scaleMatrix == null)
					{
						m_scaleMatrix = new float[array2.Length];
						for (int k = 0; k < array2.Length; k++)
						{
							m_scaleMatrix[k] = float.Parse(array2[k], NumberStyles.Float, CultureInfo.InvariantCulture);
						}
					}
					else if (m_scaleMatrix[0] != 1f)
					{
						float.TryParse(array2[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
						float.TryParse(array2[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2);
						float.TryParse(array2[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var result3);
						float.TryParse(array2[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var result4);
						float.TryParse(array2[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var result5);
						float.TryParse(array2[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var result6);
						if (result == 0f && result4 == 0f)
						{
							if (m_scaleMatrix[0] > 0f && m_scaleMatrix[3] > 0f)
							{
								m_x = (result3 + result5) * m_scaleMatrix[0];
								m_y = (result6 + result4) * m_scaleMatrix[0];
								m_height = (0f - result3) * m_scaleMatrix[0];
								m_width = result2 * m_scaleMatrix[0];
							}
						}
						else
						{
							for (int l = 0; l < array2.Length; l++)
							{
								m_scaleMatrix[l] *= float.Parse(array2[l], NumberStyles.Float, CultureInfo.InvariantCulture);
							}
						}
					}
					else
					{
						for (int m = 0; m < array2.Length; m++)
						{
							m_scaleMatrix[m] = float.Parse(array2[m], NumberStyles.Float, CultureInfo.InvariantCulture);
						}
					}
				}
				else
				{
					m_translationMatrix = new float[array2.Length];
					for (int n = 0; n < array2.Length; n++)
					{
						m_translationMatrix[n] = float.Parse(array2[n], NumberStyles.Float, CultureInfo.InvariantCulture);
					}
				}
			}
			return;
		}
		if (m_token.Count != 1)
		{
			return;
		}
		string[] array3 = text.Split(new string[1] { "cm" }, StringSplitOptions.None)[0].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array3.Length == 0)
		{
			return;
		}
		float result7;
		float result8;
		float result9;
		float result10;
		if (array3[0] == "q")
		{
			for (int num = 0; num < array3.Length - 1; num++)
			{
				array3[num] = array3[num + 1];
			}
			float.TryParse(array3[4].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result7);
			float.TryParse(array3[5].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result8);
			float.TryParse(array3[0].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result9);
			float.TryParse(array3[3].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result10);
		}
		else
		{
			int num2 = 0;
			for (int num3 = 1; num3 <= array3.Length - 1; num3++)
			{
				if (array3[num3] == "q")
				{
					num2 = num3 + 1;
					break;
				}
			}
			float.TryParse(array3[num2 + 4].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result7);
			float.TryParse(array3[num2 + 5].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result8);
			float.TryParse(array3[num2].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result9);
			float.TryParse(array3[num2 + 3].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result10);
		}
		if (result8 > 0f)
		{
			result8 = m_pageSize.Height - result8;
		}
		RectangleF rectangleF = new RectangleF(new PointF(Math.Abs(result7), Math.Abs(result8)), new SizeF(Math.Abs(result9), Math.Abs(result10)));
		if (rectangleF.Y >= rectangleF.Height)
		{
			rectangleF.Y -= rectangleF.Height;
		}
		rectangleF.X += m_leftMargin;
		rectangleF.Y += m_topMargin;
		if (m_rectToken.Length > 0)
		{
			m_rectToken.Split(new string[1] { "cm" }, StringSplitOptions.None)[0].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}
		m_x = rectangleF.X;
		m_y = rectangleF.Y;
		m_height = rectangleF.Height;
		m_width = rectangleF.Width;
	}
}
