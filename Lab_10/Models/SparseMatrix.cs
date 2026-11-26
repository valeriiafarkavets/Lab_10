using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
namespace Lab_10.Models{
    public class SparseMatrix : IEquatable<SparseMatrix>{
        private readonly Dictionary<(int Row, int Column), double> elements = new();
        public int Rows { get; }
        public int Columns { get; }
        public int NonZeroCount => elements.Count;
        public SparseMatrix(int rows, int columns) {
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows));
            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns));
            Rows = rows;
            Columns = columns;
        }
        public double this[int row, int column] {
            get{
                CheckBounds(row, column);

                return elements.TryGetValue((row, column), out double value)
                    ? value : 0;
            }
            set {
                CheckBounds(row, column);
                if (value == 0)
                    elements.Remove((row, column));
                else
                    elements[(row, column)] = value;
            }
        }
        private void CheckBounds(int row, int column){
            if (row < 0 || row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column >= Columns)
                throw new ArgumentOutOfRangeException(nameof(column));
        }
        public List<MatrixElement> GetNonZeroElements() {
            return elements
                .OrderBy(item => item.Key.Row)
                .ThenBy(item => item.Key.Column)
                .Select(item => new MatrixElement( item.Key.Row, item.Key.Column, item.Value))
                .ToList();
        }
        public static SparseMatrix operator +(SparseMatrix a, SparseMatrix b){
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new ArgumentException("Розміри матриць не збігаються.");
            SparseMatrix result = new(a.Rows, a.Columns);
            foreach (var item in a.elements)
                result[item.Key.Row, item.Key.Column] = item.Value;
            foreach (var item in b.elements)
                result[item.Key.Row, item.Key.Column] += item.Value;
            return result;
        }
        public static SparseMatrix operator *(SparseMatrix matrix, double number)  {
            SparseMatrix result = new(matrix.Rows, matrix.Columns);
            foreach (var item in matrix.elements)
                result[item.Key.Row, item.Key.Column] = item.Value * number;
            return result;
        }
        public static SparseMatrix operator *(double number, SparseMatrix matrix){
            return matrix * number;
        }
        public static SparseMatrix operator *(SparseMatrix a, SparseMatrix b){
            if (a.Columns != b.Rows)
                throw new ArgumentException(
                    "Несумісні розміри матриць для множення.");
            SparseMatrix result = new(a.Rows, b.Columns);
            var groupedRows = b.elements.ToLookup(item => item.Key.Row);
            foreach (var first in a.elements){
                foreach (var second in groupedRows[first.Key.Column]){
                    result[first.Key.Row, second.Key.Column] +=
                        first.Value * second.Value;
                }
            }
            return result;
        }
        public SparseMatrix Transpose() {
            SparseMatrix result = new(Columns, Rows);
            foreach (var item in elements)
                result[item.Key.Column, item.Key.Row] = item.Value;
            return result;
        }
        public bool Equals(SparseMatrix? other){
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            if (Rows != other.Rows || Columns != other.Columns || NonZeroCount != other.NonZeroCount)
                return false;
            foreach (var item in elements) {
                if (!other.elements.TryGetValue(item.Key, out double value) || !item.Value.Equals(value))
                    return false;
            }
            return true;
        }
        public override bool Equals(object? obj){
            return obj is SparseMatrix matrix && Equals(matrix);
        }
        public override int GetHashCode(){
            HashCode hash = new();
            hash.Add(Rows);
            hash.Add(Columns);
            foreach (var item in elements
                .OrderBy(item => item.Key.Row)
                .ThenBy(item => item.Key.Column)) {
                hash.Add(item.Key.Row);
                hash.Add(item.Key.Column);
                hash.Add(item.Value);
            }
            return hash.ToHashCode();
        }
        public static bool operator ==(SparseMatrix? a, SparseMatrix? b){
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }
        public static bool operator !=(SparseMatrix? a, SparseMatrix? b){
            return !(a == b);
        }
        public override string ToString(){
            return $"Матриця {Rows} × {Columns}, " +
                   $"ненульових елементів: {NonZeroCount}";
        }
        public string ExportCoo(){
            StringBuilder text = new();
            text.AppendLine($"{Rows} {Columns} {NonZeroCount}");
            foreach (MatrixElement item in GetNonZeroElements()) {
                text.AppendLine($"{item.Row} {item.Column} " + item.Value.ToString("R", CultureInfo.InvariantCulture));
            }
            return text.ToString();
        }
        public static SparseMatrix ImportCoo(string text) {
            string[] lines = text.Split(new[] { '\r', '\n' },StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length == 0)
                throw new FormatException("Файл COO порожній.");
            string[] header = lines[0].Split( (char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (header.Length != 3 ||
                !int.TryParse(header[0], out int rows) ||
                !int.TryParse(header[1], out int columns) ||
                !int.TryParse(header[2], out int count) ||
                count < 0 || count != lines.Length - 1)
                throw new FormatException("Некоректний заголовок COO.");
            SparseMatrix result = new(rows, columns);
            for (int i = 1; i < lines.Length; i++){
                string[] parts = lines[i].Split( (char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3 ||
                    !int.TryParse(parts[0], out int row) ||
                    !int.TryParse(parts[1], out int column) ||
                    !double.TryParse(parts[2], NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double value))
                    throw new FormatException($"Помилка в рядку {i + 1}.");
                if (value == 0)
                    throw new FormatException("COO має містити ненульові значення.");
                if (result[row, column] != 0)
                    throw new FormatException("Повторні координати елемента.");
                result[row, column] = value;
            }
            return result;
        }
        public long DenseMemoryBytes => (long)Rows * Columns * sizeof(double);
        public long SparsePayloadBytes =>
            (long)NonZeroCount * (2 * sizeof(int) + sizeof(double));
    }
}