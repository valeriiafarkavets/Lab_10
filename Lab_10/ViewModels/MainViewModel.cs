using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using Lab_10.Commands;
using Lab_10.Models;
namespace Lab_10.ViewModels{
    public class MainViewModel : INotifyPropertyChanged{
        private SparseMatrix matrixA = new(4, 4);
        private SparseMatrix matrixB = new(4, 4);
        private SparseMatrix? result;
        private string status = "Створіть матриці та введіть елементи.";
        private string memoryInfo = "Порівняння ще не виконано.";
        public string RowsInput { get; set; } = "4";
        public string ColumnsInput { get; set; } = "4";
        public string RowInput { get; set; } = "0";
        public string ColumnInput { get; set; } = "0";
        public string ValueInput { get; set; } = "5";
        public string ScalarInput { get; set; } = "2";
        public string[] MatrixNames { get; } = { "A", "B" };
        public string SelectedMatrix { get; set; } = "A";
        private SparseMatrix ActiveMatrix =>
            SelectedMatrix == "A" ? matrixA : matrixB;
        public List<MatrixElement> ElementsA =>
            matrixA.GetNonZeroElements();
        public List<MatrixElement> ElementsB =>
            matrixB.GetNonZeroElements();
        public List<MatrixElement> ResultElements =>
            result?.GetNonZeroElements() ?? new List<MatrixElement>();
        public string InfoA => matrixA.ToString();
        public string InfoB => matrixB.ToString();
        public string ResultInfo =>
            result?.ToString() ?? "Результат відсутній.";
        public string Status{
            get => status;
            set {
                status = value;
                OnPropertyChanged();
            }
        }
        public string MemoryInfo{
            get => memoryInfo;
            set {
                memoryInfo = value;
                OnPropertyChanged();
            }
        }
        public ICommand CreateCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SumCommand { get; }
        public ICommand MultiplyCommand { get; }
        public ICommand ScalarCommand { get; }
        public ICommand EqualityCommand { get; }
        public ICommand TransposeCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand MemoryCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public MainViewModel() {
            CreateCommand = new RelayCommand(() => ExecuteSafe(CreateMatrix));
            AddCommand = new RelayCommand(() => ExecuteSafe(AddElement));
            RemoveCommand = new RelayCommand(() => ExecuteSafe(RemoveElement));
            SumCommand = new RelayCommand(() => ExecuteSafe(Sum));
            MultiplyCommand = new RelayCommand(() => ExecuteSafe(Multiply));
            ScalarCommand = new RelayCommand(() => ExecuteSafe(MultiplyScalar));
            EqualityCommand = new RelayCommand(() => ExecuteSafe(CompareEquality));
            TransposeCommand = new RelayCommand(() => ExecuteSafe(Transpose));
            ImportCommand = new RelayCommand(() => ExecuteSafe(Import));
            ExportCommand = new RelayCommand(() => ExecuteSafe(Export));
            MemoryCommand = new RelayCommand(() => ExecuteSafe(CompareMemory));
        }
        private void ExecuteSafe(Action action){
            try{
                action();
            }
            catch (Exception ex){
                Status = $"Помилка: {ex.Message}";
            }
        }
        private void CreateMatrix(){
            if (!int.TryParse(RowsInput, out int rows) ||
                !int.TryParse(ColumnsInput, out int columns) ||
                rows <= 0 || columns <= 0)
                throw new ArgumentException(
                    "Розміри матриці мають бути додатними цілими числами.");
            SparseMatrix matrix = new(rows, columns);
            if (SelectedMatrix == "A")
                matrixA = matrix;
            else
                matrixB = matrix;
            Status = $"Створено матрицю {SelectedMatrix}: {rows} × {columns}.";
            Refresh();
        }
        private (int Row, int Column) ReadCoordinates(){
            if (!int.TryParse(RowInput, out int row) ||
                !int.TryParse(ColumnInput, out int column))
                throw new ArgumentException(
                    "Введіть цілі індекси рядка та стовпця.");
            return (row, column);
        }
        private double ReadValue(string text){
            if (!double.TryParse(text, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out double value) &&
                !double.TryParse(text, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out value))
                throw new ArgumentException("Введіть коректне число.");
            if (!double.IsFinite(value))
                throw new ArgumentException("Число має бути скінченним.");
            return value;
        }
        private void AddElement(){
            var coordinates = ReadCoordinates();
            double value = ReadValue(ValueInput);
            ActiveMatrix[coordinates.Row, coordinates.Column] = value;
            Status = value == 0
                ? $"Нульове значення вилучено з матриці {SelectedMatrix}."
                : $"Елемент матриці {SelectedMatrix} " +
                  $"[{coordinates.Row}, {coordinates.Column}] = {value}.";
            Refresh();
        }
        private void RemoveElement(){
            var coordinates = ReadCoordinates();
            ActiveMatrix[coordinates.Row, coordinates.Column] = 0;
            Status = $"Елемент [{coordinates.Row}, {coordinates.Column}] " +
                     $"вилучено з матриці {SelectedMatrix}.";
            Refresh();
        }
        private void Sum(){
            result = matrixA + matrixB;
            Status = "Виконано додавання A + B.";
            Refresh();
        }
        private void Multiply() {
            result = matrixA * matrixB;
            Status = "Виконано множення A × B.";
            Refresh();
        }
        private void MultiplyScalar(){
            double scalar = ReadValue(ScalarInput);
            result = ActiveMatrix * scalar;
            Status = $"Матрицю {SelectedMatrix} помножено на {scalar}.";
            Refresh();
        }
        private void CompareEquality() {
            Status = matrixA == matrixB
                ? "Матриці A та B рівні за значеннями."
                : "Матриці A та B не рівні.";
        }
        private void Transpose(){
            result = ActiveMatrix.Transpose();
            Status = $"Виконано транспонування матриці {SelectedMatrix}.";
            Refresh();
        }
        private void Import() {
            OpenFileDialog dialog = new(){
                Filter = "COO files (*.coo)|*.coo|Text files (*.txt)|*.txt"
            };
            if (dialog.ShowDialog() != true)
                return;
            SparseMatrix imported =
                SparseMatrix.ImportCoo(File.ReadAllText(dialog.FileName));
            if (SelectedMatrix == "A")
                matrixA = imported;
            else
                matrixB = imported;
            Status = $"Матрицю {SelectedMatrix} імпортовано з файлу COO.";
            Refresh();
        }
        private void Export() {
            SaveFileDialog dialog = new(){
                Filter = "COO files (*.coo)|*.coo",
                FileName = $"Matrix_{SelectedMatrix}.coo",
                DefaultExt = ".coo"
            };
            if (dialog.ShowDialog() != true)
                return;
            File.WriteAllText(dialog.FileName, ActiveMatrix.ExportCoo());
            Status = $"Матрицю {SelectedMatrix} експортовано у форматі COO.";
        }
        private void CompareMemory() {
            const int rows = 1000;
            const int columns = 1000;
            const int count = 100;
            long beforeDense = GC.GetAllocatedBytesForCurrentThread();
            double[,] dense = new double[rows, columns];
            for (int i = 0; i < count; i++)
                dense[i, i] = i + 1;
            long denseBytes =
                GC.GetAllocatedBytesForCurrentThread() - beforeDense;
            long beforeSparse = GC.GetAllocatedBytesForCurrentThread();
            SparseMatrix sparse = new(rows, columns);
            for (int i = 0; i < count; i++)
                sparse[i, i] = i + 1;
            long sparseBytes =
                GC.GetAllocatedBytesForCurrentThread() - beforeSparse;
            GC.KeepAlive(dense);
            GC.KeepAlive(sparse);
            double saving =
                (1.0 - (double)sparseBytes / denseBytes) * 100;
            MemoryInfo =
                $"Розмір: {rows} × {columns}\n" +
                $"Ненульових елементів: {count}\n" +
                $"Щільна матриця: {denseBytes:N0} байт\n" +
                $"Розріджена матриця: {sparseBytes:N0} байт\n" +
                $"Зменшення обсягу виділення: {saving:F2}%";
            Status = "Порівняння виділеної пам'яті виконано.";
        }
        private void Refresh() {
            OnPropertyChanged(nameof(ElementsA));
            OnPropertyChanged(nameof(ElementsB));
            OnPropertyChanged(nameof(ResultElements));
            OnPropertyChanged(nameof(InfoA));
            OnPropertyChanged(nameof(InfoB));
            OnPropertyChanged(nameof(ResultInfo));
        }
        private void OnPropertyChanged(  [CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }
    }
}