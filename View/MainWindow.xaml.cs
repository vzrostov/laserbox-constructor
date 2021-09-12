using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LevelConstructor.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace LevelConstructor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IFieldInitializer
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public Cell[] InitGameField(int columns, int rows)
		{
			Canvas.Children.Clear();
			float step = Math.Min(60f/columns, 90f/rows);
			var cells = new List<Cell>();
			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < columns; col++)
				{
					var cellRectangle = new Rectangle
					{
						Height = step,
						Width = step,
					};
					var objectRectangle = new Rectangle
					{
						Height = step,
						Width = step
					};
					var shadowRectangle = new Rectangle()
					{
						Height = step * 2,
						Width = step * 2
					};
					Canvas.SetLeft(objectRectangle, col * step);
					Canvas.SetTop(objectRectangle, row * step);
					Canvas.Children.Add(objectRectangle);

					Canvas.SetLeft(cellRectangle, col * step);
					Canvas.SetTop(cellRectangle, row * step);
					Canvas.Children.Add(cellRectangle);

					Canvas.SetLeft(shadowRectangle, col * step);
					Canvas.SetTop(shadowRectangle, row * step);
					Canvas.Children.Add(shadowRectangle);

					cells.Add(new Cell(cellRectangle, objectRectangle, shadowRectangle, row, col, Resources["CellContextMenu"] as ContextMenu, Canvas));
				}
			}
			return cells.ToArray();
		}

		private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
		{
			var cells = InitGameField(6, 9);
			var viewModel = DataContext as MainViewModel;
			if (viewModel != null)
			{
				viewModel.FieldInitializer = this;
				viewModel.Cells = cells;
			}
		}

		private void OnPrintBtnClick(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			if(dialog.ShowDialog() != true)
				return;
			RenderTargetBitmap rtb = new RenderTargetBitmap(400, 400, 96, 96, PixelFormats.Pbgra32);

			VisualBrush sourceBrush = new VisualBrush(mainCanvas);
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen())
			{
				drawingContext.PushTransform(new ScaleTransform(400 / mainCanvas.ActualHeight, 400 / mainCanvas.ActualWidth));
				drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(mainCanvas.ActualWidth, mainCanvas.ActualHeight)));
			}
			rtb.Render(drawingVisual);

			//rtb.Render(mainCanvas);
			PngBitmapEncoder png = new PngBitmapEncoder();
			png.Frames.Add(BitmapFrame.Create(rtb));
			var path = dialog.FileName.EndsWith(".png") ? dialog.FileName : dialog.FileName + ".png";
			using (FileStream stream = File.Create(path))
			{
				png.Save(stream);
			}
		}
	}

	public interface IFieldInitializer
	{
		Cell[] InitGameField(int cols, int rows);
	}
}
