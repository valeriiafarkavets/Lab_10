using System;
using Lab_10.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Lab_10.Tests{
    [TestClass]
    public class SparseMatrixTests {
        [TestMethod]
        public void InvalidDimensionsTest(){
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SparseMatrix(0, 3));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SparseMatrix(3, -1));
        }
        [TestMethod]
        public void IndexerTest(){
            SparseMatrix matrix = new(3, 3);
            Assert.AreEqual(0.0, matrix[1, 2]);
            matrix[1, 2] = 7.5;
            Assert.AreEqual(7.5, matrix[1, 2]);
            Assert.AreEqual(1, matrix.NonZeroCount);
            matrix[1, 2] = 10;
            Assert.AreEqual(10.0, matrix[1, 2]);
            Assert.AreEqual(1, matrix.NonZeroCount);
        }
        [TestMethod]
        public void IndexOutOfRangeTest(){
            SparseMatrix matrix = new(3, 3);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => {double value = matrix[-1, 0];});
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => {matrix[0, 3] = 5;});
        }
        [TestMethod]
        public void ZeroRemovalTest(){
            SparseMatrix matrix = new(3, 3);
            matrix[0, 1] = 8;
            matrix[2, 2] = 4;
            Assert.AreEqual(2, matrix.NonZeroCount);
            matrix[0, 1] = 0;
            Assert.AreEqual(1, matrix.NonZeroCount);
            Assert.AreEqual(0.0, matrix[0, 1]);
        }
        [TestMethod]
        public void AdditionTest(){
            SparseMatrix a = new(2, 2);
            SparseMatrix b = new(2, 2);
            a[0, 0] = 5;
            a[1, 1] = 4;

            b[0, 0] = 3;
            b[1, 1] = -4;
            SparseMatrix result = a + b;
            Assert.AreEqual(8.0, result[0, 0]);
            Assert.AreEqual(0.0, result[1, 1]);
            Assert.AreEqual(1, result.NonZeroCount);
            Assert.AreEqual(5.0, a[0, 0]);
            Assert.AreEqual(4.0, a[1, 1]);
            Assert.AreEqual(3.0, b[0, 0]);
            Assert.AreEqual(-4.0, b[1, 1]);
        }
        [TestMethod]
        public void ScalarMultiplicationTest(){
            SparseMatrix matrix = new(2, 2);
            matrix[0, 1] = 6;
            SparseMatrix result = matrix * 3;
            SparseMatrix reverse = 3 * matrix;
            SparseMatrix zero = matrix * 0;
            Assert.AreEqual(18.0, result[0, 1]);
            Assert.AreEqual(result, reverse);
            Assert.AreEqual(0, zero.NonZeroCount);
            Assert.AreEqual(6.0, matrix[0, 1]);
        }
        [TestMethod]
        public void MatrixMultiplicationTest() {
            double[,] denseA = {
                { 2, 0, 3 },
                { 0, 4, 0 }
            };
            double[,] denseB = {
                { 1, 0 },
                { 0, 5 },
                { 2, 3 }
            };
            SparseMatrix a = new(2, 3);
            SparseMatrix b = new(3, 2);
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 3; j++)
                    a[i, j] = denseA[i, j];
            }
            for (int i = 0; i < 3; i++){
                for (int j = 0; j < 2; j++)
                    b[i, j] = denseB[i, j];
            }
            SparseMatrix result = a * b;
            Assert.AreEqual(2, result.Rows);
            Assert.AreEqual(2, result.Columns);
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 2; j++) {
                    double expected = 0;
                    for (int k = 0; k < 3; k++)
                        expected += denseA[i, k] * denseB[k, j];
                    Assert.AreEqual(expected, result[i, j]);
                }
            }
        }
        [TestMethod]
        public void TransposeTest(){
            SparseMatrix matrix = new(2, 3);
            matrix[0, 2] = 9;
            matrix[1, 0] = 4;
            SparseMatrix result = matrix.Transpose();
            Assert.AreEqual(3, result.Rows);
            Assert.AreEqual(2, result.Columns);
            Assert.AreEqual(9.0, result[2, 0]);
            Assert.AreEqual(4.0, result[0, 1]);
            Assert.AreEqual(matrix, result.Transpose());
        }
        [TestMethod]
        public void EqualityTest(){
            SparseMatrix a = new(3, 3);
            SparseMatrix b = new(3, 3);
            a[0, 1] = 7;
            a[2, 0] = 5;

            b[2, 0] = 5;
            b[0, 1] = 7;
            Assert.IsTrue(a == b);
            Assert.IsTrue(a.Equals(b));
            Assert.IsFalse(a != b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            b[0, 1] = 10;
            Assert.IsTrue(a != b);
            Assert.IsFalse(a == b);
        }
        [TestMethod]
        public void IncompatibleDimensionsTest(){
            SparseMatrix a = new(2, 3);
            SparseMatrix b = new(4, 2);
            Assert.ThrowsException<ArgumentException>(() => {SparseMatrix result = a + b;});
            Assert.ThrowsException<ArgumentException>(() => { SparseMatrix result = a * b;});
        }
        [TestMethod]
        public void CooImportExportTest(){
            SparseMatrix matrix = new(4, 5);
            matrix[0, 2] = 7.5;
            matrix[3, 4] = -2;
            string text = matrix.ExportCoo();
            SparseMatrix imported = SparseMatrix.ImportCoo(text);
            Assert.AreEqual(matrix, imported);
            Assert.AreEqual(2, imported.NonZeroCount);
        }
    }
}