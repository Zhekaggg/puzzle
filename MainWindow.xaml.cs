using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PuzzleGame
{
    public partial class MainWindow : Window
    {
        private const int Rows = 3;
        private const int Columns = 3;
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
            }
        }

        private void PositionPuzzlePieces()
        {
            double pieceWidth = canvas.ActualWidth / Columns;
            double pieceHeight = canvas.ActualHeight / Rows;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    int index = i * Columns + j;
                    Image piece = puzzlePieces[index];
                    double left = j * pieceWidth + random.NextDouble() * (pieceWidth / 2);
                    double top = i * pieceHeight + random.NextDouble() * (pieceHeight / 2);
                    piece.Width = pieceWidth;
                    piece.Height = pieceHeight;
                    Canvas.SetLeft(piece, left);
                    Canvas.SetTop(piece, top);
                }
            }
        }

        private void PuzzlePiece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image puzzlePiece = sender as Image;
            if (puzzlePiece != null)
            {
                Canvas.SetZIndex(puzzlePiece, Canvas.GetZIndex(imagePlaceholder) + 1); // Bring puzzle piece to the top
                puzzlePiece.CaptureMouse();
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
    }
}
