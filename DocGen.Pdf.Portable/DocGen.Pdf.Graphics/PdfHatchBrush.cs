using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class PdfHatchBrush : PdfTilingBrush
{
	private PdfHatchStyle m_hatchStyle;

	private PdfColor m_foreColor = PdfColor.Empty;

	private PdfColor m_backColor = PdfColor.Empty;

	internal PdfColor BackColor => m_backColor;

	public PdfHatchBrush(PdfHatchStyle hatchstyle, PdfColor foreColor)
		: base(new SizeF(8f, 8f))
	{
		m_hatchStyle = hatchstyle;
		m_foreColor = foreColor;
		CreateHatchBrush();
	}

	public PdfHatchBrush(PdfHatchStyle hatchstyle, PdfColor foreColor, PdfColor backColor)
		: base(new SizeF(8f, 8f))
	{
		m_hatchStyle = hatchstyle;
		m_foreColor = foreColor;
		m_backColor = backColor;
		CreateHatchBrush();
	}

	private void CreateHatchBrush()
	{
		PdfGraphics graphics = base.Graphics;
		PdfPen pdfPen = new PdfPen(m_foreColor, 1f);
		SizeF sizeF = new SizeF(8f, 8f);
		switch (m_hatchStyle)
		{
		case PdfHatchStyle.BackwardDiagonal:
			DrawBackwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.Cross:
			DrawCross(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DarkDownwardDiagonal:
			pdfPen.Width = 2f;
			DrawDownwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DarkHorizontal:
			pdfPen.Width = 2f;
			DrawHorizontal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DarkUpwardDiagonal:
			pdfPen.Width = 2f;
			DrawUpwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DarkVertical:
			pdfPen.Width = 2f;
			DrawVertical(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.ForwardDiagonal:
			DrawForwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DashedDownwardDiagonal:
			pdfPen.DashStyle = PdfDashStyle.Dash;
			DrawDownwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DashedHorizontal:
			pdfPen.DashStyle = PdfDashStyle.Dash;
			DrawHorizontal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DashedUpwardDiagonal:
			pdfPen.DashStyle = PdfDashStyle.Dash;
			DrawUpwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DashedVertical:
			pdfPen.DashStyle = PdfDashStyle.Dash;
			DrawVertical(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DiagonalBrick:
			DrawForwardDiagonal(graphics, pdfPen, sizeF);
			DrawBrickTails(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DiagonalCross:
			DrawForwardDiagonal(graphics, pdfPen, sizeF);
			DrawBackwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DottedDiamond:
			pdfPen.DashStyle = PdfDashStyle.Dot;
			DrawForwardDiagonal(graphics, pdfPen, sizeF);
			DrawBackwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.DottedGrid:
			pdfPen.DashStyle = PdfDashStyle.Dot;
			DrawCross(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.Horizontal:
			DrawHorizontal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.HorizontalBrick:
			DrawHorizontalBrick(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.LargeCheckerBoard:
			DrawCheckerBoard(graphics, pdfPen, sizeF, 4);
			break;
		case PdfHatchStyle.LightDownwardDiagonal:
			DrawDownwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.LightHorizontal:
			DrawHorizontal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.LightUpwardDiagonal:
			DrawUpwardDiagonal(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.LightVertical:
			DrawVertical(graphics, pdfPen, sizeF);
			break;
		case PdfHatchStyle.Weave:
			DrawWeave(graphics, pdfPen, sizeF);
			break;
		default:
		{
			PdfGraphicsState state = graphics.Save();
			graphics.SetTransparency(0.5f);
			graphics.DrawRectangle(new PdfSolidBrush(m_foreColor), new RectangleF(PointF.Empty, sizeF));
			graphics.Restore(state);
			break;
		}
		case PdfHatchStyle.LargeConfetti:
		case PdfHatchStyle.Divot:
			break;
		}
	}

	private static void DrawCross(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float num = brushSize.Width / 2f;
		float num2 = brushSize.Height / 2f;
		graphics.DrawLine(pen, num, 0f, num, brushSize.Height);
		graphics.DrawLine(pen, 0f, num2, brushSize.Width, num2);
	}

	private static void DrawBackwardDiagonal(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		graphics.DrawLine(pen, brushSize.Width, 0f, 0f, brushSize.Height);
		graphics.DrawLine(pen, -1f, 1f, 1f, -1f);
		graphics.DrawLine(pen, brushSize.Width - 1f, brushSize.Height + 1f, brushSize.Width + 1f, brushSize.Height - 1f);
	}

	private static void DrawForwardDiagonal(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		graphics.DrawLine(pen, 0f, 0f, brushSize.Width, brushSize.Height);
		graphics.DrawLine(pen, -1f, -1f, 1f, 1f);
		graphics.DrawLine(pen, brushSize.Width - 1f, brushSize.Height - 1f, brushSize.Width + 1f, brushSize.Height + 1f);
	}

	private static void DrawHorizontal(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float num = 0f;
		float num2 = brushSize.Height / 2f;
		float height = brushSize.Height;
		graphics.DrawLine(pen, 0f, num, brushSize.Width, num);
		graphics.DrawLine(pen, 0f, num2, brushSize.Width, num2);
		graphics.DrawLine(pen, 0f, height, brushSize.Width, height);
	}

	private static void DrawVertical(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float num = 0f;
		float num2 = brushSize.Height / 2f;
		float height = brushSize.Height;
		graphics.DrawLine(pen, num, 0f, num, brushSize.Height);
		graphics.DrawLine(pen, num2, 0f, num2, brushSize.Height);
		graphics.DrawLine(pen, height, 0f, height, brushSize.Height);
	}

	private static void DrawDownwardDiagonal(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float num = brushSize.Height / 2f;
		float num2 = brushSize.Width / 2f;
		graphics.DrawLine(pen, 0f, 0f, brushSize.Width, brushSize.Height);
		graphics.DrawLine(pen, 0f, num, num2, brushSize.Height);
		graphics.DrawLine(pen, num2, 0f, brushSize.Width, num);
		graphics.DrawLine(pen, -1f, -1f, 1f, 1f);
		graphics.DrawLine(pen, brushSize.Width - 1f, brushSize.Height - 1f, brushSize.Width + 1f, brushSize.Height + 1f);
	}

	private void DrawWeave(PdfGraphics g, PdfPen pen, SizeF brushSize)
	{
		g.TranslateTransform(-0.5f, -0.5f);
		g.DrawLine(pen, new PointF(0f, 0f), new PointF(0.5f, 0.5f));
		g.DrawLine(pen, new PointF(0f, 1f), new PointF(1f, 0f));
		g.DrawLine(pen, new PointF(0f, 5f), new PointF(5f, 0f));
		g.DrawLine(pen, new PointF(0f, 4f), new PointF(5f, 9f));
		g.DrawLine(pen, new PointF(2.5f, 2.5f), new PointF(9f, 9f));
		g.DrawLine(pen, new PointF(4f, 0f), new PointF(6.5f, 2.5f));
		g.DrawLine(pen, new PointF((float)(6.5 - Math.Sqrt(0.125)), (float)(2.5 + Math.Sqrt(0.125))), new PointF(9f, 0f));
		g.DrawLine(pen, new PointF(6.5f, 6.5f), new PointF(9f, 4f));
		g.DrawLine(pen, new PointF(2.5f, 6.5f), new PointF(0.5f, 8.5f));
	}

	private static void DrawUpwardDiagonal(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float num = brushSize.Height / 2f;
		float num2 = brushSize.Width / 2f;
		graphics.DrawLine(pen, brushSize.Width, 0f, 0f, brushSize.Height);
		graphics.DrawLine(pen, 0f, num, num2, 0f);
		graphics.DrawLine(pen, num2, brushSize.Height, brushSize.Width, num);
		graphics.DrawLine(pen, -1f, 1f, 1f, -1f);
		graphics.DrawLine(pen, brushSize.Width - 1f, brushSize.Height + 1f, brushSize.Width + 1f, brushSize.Height - 1f);
	}

	private static void DrawBrickTails(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float x = brushSize.Width / 2f;
		float y = brushSize.Height / 2f;
		graphics.DrawLine(pen, x, y, brushSize.Width, brushSize.Height);
	}

	private static void DrawHorizontalBrick(PdfGraphics graphics, PdfPen pen, SizeF brushSize)
	{
		float num = brushSize.Width / 2f;
		float num2 = brushSize.Height / 2f;
		graphics.DrawLine(pen, 0f, 0f, brushSize.Width, 0f);
		graphics.DrawLine(pen, 0f, brushSize.Height, brushSize.Width, brushSize.Height);
		graphics.DrawLine(pen, 0f, num2, brushSize.Width, num2);
		graphics.DrawLine(pen, num, 0f, num, num2);
		graphics.DrawLine(pen, 0f, num2, 0f, brushSize.Height);
		graphics.DrawLine(pen, brushSize.Width, num2, brushSize.Width, brushSize.Height);
	}

	private static void DrawCheckerBoard(PdfGraphics graphics, PdfPen pen, SizeF brushSize, int cellSize)
	{
		int num = (int)(brushSize.Width / (float)cellSize);
		int num2 = (int)(brushSize.Height / (float)cellSize);
		PdfSolidBrush brush = new PdfSolidBrush(pen.Color);
		for (int i = 0; i < num2; i++)
		{
			float y = i * cellSize;
			for (int j = 0; j < num; j++)
			{
				float x = j * cellSize;
				graphics.DrawRectangle(brush, x, y, cellSize, cellSize);
			}
		}
	}
}
