namespace Lab_10.Models{
    public class MatrixElement {
        public int Row { get; set; }
        public int Column { get; set; }
        public double Value { get; set; }
        public MatrixElement(int row, int column, double value){
            Row = row;
            Column = column;
            Value = value;
        }
    }
}