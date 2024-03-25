using System;
using System.Windows;

namespace PuzzleGame
{
    public partial class PuzzlePiecesDialog : Window
    {
        public event Action<int, int> PuzzlePiecesChanged;

        public PuzzlePiecesDialog()
        {
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем количество пазлов по горизонтали
            int horizontalPieces;
            if (!int.TryParse(HorizontalTextBox.Text, out horizontalPieces))
            {
                MessageBox.Show("Некорректное значение для количества пазлов по горизонтали.");
                return;
            }

            // Получаем количество пазлов по вертикали
            int verticalPieces;
            if (!int.TryParse(VerticalTextBox.Text, out verticalPieces))
            {
                MessageBox.Show("Некорректное значение для количества пазлов по вертикали.");
                return;
            }

            // Проверяем, что количество пазлов в допустимых пределах (максимум 10)
            if (horizontalPieces <= 2 || horizontalPieces > 10 || verticalPieces <= 2 || verticalPieces > 10)
            {
                MessageBox.Show("Количество пазлов должно быть от 3 до 10.");
                return;
            }

            // Вызываем событие с новыми значениями
            PuzzlePiecesChanged?.Invoke(horizontalPieces, verticalPieces);

            // Закрываем окно
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно без изменений
            Close();
        }
    }
}
