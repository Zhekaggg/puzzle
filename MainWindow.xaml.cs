using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PuzzleGame
{
    public partial class MainWindow : Window
    {
        private int Rows = 5;
        private int Columns = 5;
        private readonly List<Image> puzzlePieces = new List<Image>();
        private readonly Random random = new Random();
        private BitmapImage selectedImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializePuzzle()
        {
            Canvas.SetZIndex(imagePlaceholder, 0); // Set placeholder image behind puzzle pieces
            ClearPuzzle();

            // Create puzzle pieces
            for (int i = 0; i < Rows * Columns; i++)
            {
                Image puzzlePiece = new Image();
                puzzlePiece.Stretch = Stretch.Fill;
                puzzlePiece.MouseDown += PuzzlePiece_MouseDown;
                puzzlePieces.Add(puzzlePiece);
                canvas.Children.Add(puzzlePiece);
            }
        }

        private void ClearPuzzle()
        {
            foreach (Image piece in puzzlePieces)
            {
                canvas.Children.Remove(piece);
            }
            puzzlePieces.Clear();
        }

        private void ShufflePuzzle()
        {
            int[] indices = new int[Rows * Columns];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            for (int i = 0; i < indices.Length; i++)
            {
                int randomIndex = random.Next(i, indices.Length);
                int temp = indices[randomIndex];
                indices[randomIndex] = indices[i];
                indices[i] = temp;
            }

            for (int i = 0; i < puzzlePieces.Count; i++)
            {
                Image piece = puzzlePieces[i];
                int index = indices[i];
                canvas.Children.Remove(piece);
                canvas.Children.Insert(index, piece);

                // Устанавливаем Z-index в зависимости от правильности вставки пазла
                Canvas.SetZIndex(piece, index); // Правильные пазлы будут ниже
            }
        }
        private void PositionPuzzlePieces()
        {
            double pieceWidth = canvas.ActualWidth / Columns;
            double pieceHeight = canvas.ActualHeight / Rows;

            List<Image> shuffledPuzzlePieces = puzzlePieces.OrderBy(x => random.Next()).ToList();

            int row = 0;
            int col = 0;

            foreach (Image piece in shuffledPuzzlePieces)
            {
                // Вычисляем ближайшую ячейку канвы и выравниваем пазл по этой ячейке
                double left = col * pieceWidth;
                double top = row * pieceHeight;

                piece.Width = pieceWidth;
                piece.Height = pieceHeight;
                Canvas.SetLeft(piece, left);
                Canvas.SetTop(piece, top);

                col++;
                if (col >= Columns)
                {
                    col = 0;
                    row++;
                    if (row >= Rows)
                        row = 0;
                }
            }
        }



        private void PuzzlePiece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image puzzlePiece = sender as Image;
            if (puzzlePiece != null)
            {
                // Перемещаем пазл сверху всех остальных
                BringToFront(puzzlePiece);

                // Захватываем пазл мышью
                puzzlePiece.CaptureMouse();
            }
        }

        private void BringToFront(Image puzzlePiece)
        {
            // Получаем текущий индекс Z пазла
            int currentIndex = Canvas.GetZIndex(puzzlePiece);

            // Перемещаем пазл в самый верх по Z-Index
            int maxIndex = canvas.Children.Cast<UIElement>().Max(x => Canvas.GetZIndex(x));
            Canvas.SetZIndex(puzzlePiece, maxIndex + 1);

            // Перемещаем неправильно вставленные пазлы ниже по Z-Index
            foreach (Image piece in puzzlePieces)
            {
                if (piece != puzzlePiece && Canvas.GetZIndex(piece) > currentIndex)
                {
                    Canvas.SetZIndex(piece, Canvas.GetZIndex(piece) - 1);
                }
            }
        }


        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Image puzzlePiece = e.Source as Image;
                if (puzzlePiece != null && puzzlePiece.IsMouseCaptured)
                {
                    Point position = e.GetPosition(canvas);
                    double newX = position.X - puzzlePiece.ActualWidth / 2;
                    double newY = position.Y - puzzlePiece.ActualHeight / 2;
                    Canvas.SetLeft(puzzlePiece, newX);
                    Canvas.SetTop(puzzlePiece, newY);
                }
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image puzzlePiece = e.Source as Image;
            if (puzzlePiece != null)
            {
                puzzlePiece.ReleaseMouseCapture();

                // Находим координаты верхнего левого угла ячейки, на которую пазл должен быть выровнен
                double cellWidth = canvas.ActualWidth / Columns;
                double cellHeight = canvas.ActualHeight / Rows;
                int row = (int)Math.Round(Canvas.GetTop(puzzlePiece) / cellHeight);
                int col = (int)Math.Round(Canvas.GetLeft(puzzlePiece) / cellWidth);

                // Вычисляем ожидаемые координаты пазла
                double expectedLeft = col * cellWidth;
                double expectedTop = row * cellHeight;

                // Проверяем, находится ли пазл достаточно близко к своему месту
                if (Math.Abs(Canvas.GetLeft(puzzlePiece) - expectedLeft) <= cellWidth / 2 &&
                    Math.Abs(Canvas.GetTop(puzzlePiece) - expectedTop) <= cellHeight / 2)
                {
                    // Перемещаем пазл на свое место
                    Canvas.SetLeft(puzzlePiece, expectedLeft);
                    Canvas.SetTop(puzzlePiece, expectedTop);
                }
            }
        }

        private void ChoosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImage = new BitmapImage(new Uri(openFileDialog.FileName));
                SliceImage();
            }
        }

        private void SliceImage()
        {
            ClearPuzzle();

            double pieceWidth = selectedImage.PixelWidth / Columns;
            double pieceHeight = selectedImage.PixelHeight / Rows;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    CroppedBitmap croppedBitmap = new CroppedBitmap(selectedImage,
                        new Int32Rect(j * (int)pieceWidth, i * (int)pieceHeight, (int)pieceWidth, (int)pieceHeight));

                    Image piece = new Image();
                    piece.Source = croppedBitmap;
                    piece.Stretch = Stretch.Fill;
                    piece.MouseDown += PuzzlePiece_MouseDown;
                    puzzlePieces.Add(piece);
                    canvas.Children.Add(piece);
                }
            }

            ShufflePuzzle();
            PositionPuzzlePieces();
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            bool isCorrect = true;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    int index = i * Columns + j;
                    Image piece = puzzlePieces[index];
                    double expectedLeft = j * canvas.ActualWidth / Columns;
                    double expectedTop = i * canvas.ActualHeight / Rows;

                    if (Math.Abs(Canvas.GetLeft(piece) - expectedLeft) > 5 || Math.Abs(Canvas.GetTop(piece) - expectedTop) > 5)
                    {
                        isCorrect = false;
                        break;
                    }
                }

            }

            if (isCorrect)
            {
                MessageBox.Show("бля молодец");
            }
            else
            {
                MessageBox.Show("ээ не правельно");
            }
        }
        private void NumberOfPiecesButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новое окно выбора количества пазлов
            PuzzlePiecesDialog dialog = new PuzzlePiecesDialog();

            // Подписываемся на событие изменения количества пазлов
            dialog.PuzzlePiecesChanged += (horizontal, vertical) =>
            {
                Columns = horizontal; // Устанавливаем количество колонок
                Rows = vertical; // Устанавливаем количество строк
                InitializePuzzle(); // Пересоздаем пазл с новыми параметрами
            };

            // Показываем диалоговое окно
            dialog.ShowDialog();
        }
    }
}