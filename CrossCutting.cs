using System;
using System.IO;

namespace ParetoFrontier_MDVRPTW
{
    public static class CrossCutting
    {
        public static void GenerateSymmetricMatrixNxN(int size, double[,] matrix, double min,
            double max)
        {
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                        matrix[i, j] = 0;
                    else if (i < j)
                        matrix[i, j] = random.NextDouble() * (max - min) + min;
                    else
                        matrix[i, j] = matrix[j, i];
                }
            }
        }

        public static void GenerateSymmetricMatrixNxN(int size, int[,] matrix, int min,
            int max)
        {
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                        matrix[i, j] = 0;
                    else if (i < j)
                        matrix[i, j] = random.Next(min, max);
                    else
                        matrix[i, j] = matrix[j, i];
                }
            }
        }

        public static double[][] GenerateMatrixRowsByColumns(double min,
            double max, int rows, int columns, int? controle = null)
        {
            Random random = new Random();
            double[][] matrix = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new double[columns];
                for (int j = 0; j < columns; j++)
                {
                    if (controle.HasValue && i < controle)
                    {
                        matrix[i][j] = 0;
                    }
                    else
                    {
                        matrix[i][j] = random.NextDouble() * (max - min) + min;
                    }
                }
            }
            return matrix;
        }

        public static void GenerateMatrixNxNxN(double[][][] matrix, double min,
            double max, int dim1, int dim2, int dim3, int? controle = null)
        {
            Random random = new Random();
            matrix = new double[dim1][][];
            for (int i = 0; i < dim1; i++)
            {
                matrix[i] = new double[dim2][];
                for (int j = 0; j < dim2; j++)
                {
                    matrix[i][j] = new double[dim3];
                    for (int k = 0; k < dim3; k++)
                    {
                        if (controle.HasValue && i < controle)
                        {
                            matrix[i][j][k] = 0;
                        }
                        else
                        {
                            matrix[i][j][k] = random.NextDouble() * (max - min) + min;
                        }
                    }
                }
            }
        }

        public static void GenerateSymmetricMatrixNxNxN(double[,,] matrix, double min,
            double max, int dim1, int dim2, int dim3)
        {
            Random random = new Random();
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    for (int k = 0; k < dim3; k++)
                    {
                        if (j == k)
                        {
                            matrix[i, j, k] = 0;
                        }
                        else if(j < k)
                        {
                            matrix[i, j, k] = random.NextDouble() * (max - min) + min;
                        }
                        else
                        {
                            matrix[i, j, k] = matrix[i, k, j];
                        }
                    }
                }
            }
        }

        public static void GenerateMatrixRowsByColumns(int[,] matrix, int min,
            int max, int rows, int columns, int? controle = null)
        {
            Random random = new Random();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (controle.HasValue && i < controle)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        matrix[i, j] = random.Next(min, max);
                    }
                }
            }
        }

        public static double[] GenerateRandomDoubleArray(double min, double max,
            int size, int? controle = null)
        {
            Random random = new Random();
            double[] matrix = new double[size];
            for (int i = 0; i < size; i++)
            {
                if (controle.HasValue && i < controle)
                {
                    matrix[i] = 0;
                }
                else
                {
                    matrix[i] = random.NextDouble() * (max - min) + min;
                }
            }
            return matrix;
        }

        public static int[] GenerateRandomIntArray(int min, int max,
            int size, int? controle = null)
        {
            Random random = new Random();
            int[] matrix = new int[size];
            for (int i = 0; i < size; i++)
            {
                if (controle.HasValue && i < controle)
                {
                    matrix[i] = 0;
                }
                else
                {
                    matrix[i] = random.Next(min, max);
                }
            }
            return matrix;
        }

        public static double[][] Generate01MatrixRowsByColumnsByIntervals(int rows, int columns)
        {
            int step = columns / rows;
            int intervalBegin = 0, intervalEnd = step;
            int controlEnd = 0;
            double[][] matrix = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new double[columns];
                for (int j = 0; j < columns; j++) 
                {
                    if((intervalBegin <= j && j < intervalEnd) || 
                        (controlEnd == (rows - 1)) && j >= intervalBegin)
                    {
                        matrix[i][j] = 1;
                    }
                    else
                    {
                        matrix[i][j] = 0;
                    }
                }
                controlEnd++;
                intervalBegin = intervalEnd;
                intervalEnd += step;
            }
            return matrix;
        }

        public static void WriteMatrixNxNxNToFile(StreamWriter file,
            double[][][] matrix, int dim1, int dim2, int dim3, string arrayName = null)
        {
            string name = string.IsNullOrEmpty(arrayName) ? "" : $"{arrayName} = ";
            file.Write($"{name}[{Environment.NewLine}");
            for (int i = 0; i < dim1; i++)
            {
                file.Write($"[");
                for (int j = 0; j < dim2; j++)
                {
                    file.Write($"[");
                    for (int k = 0; k < dim3; k++)
                    {
                        if (k == 0)
                        {
                            file.Write($"{(float)Math.Round(matrix[i][j][k], 1)}");
                        }
                        else
                        {
                            file.Write($", {(float)Math.Round(matrix[i][j][k], 1)}");
                        }
                    }
                    file.Write($"]");
                }
                if (i == (dim1 - 1))
                {
                    file.Write($"]{Environment.NewLine}");
                }
                else
                {
                    file.Write($"],{Environment.NewLine}");
                }
            }
            string semicolon = string.IsNullOrEmpty(arrayName) ? "" : ";";
            file.Write($"]{semicolon}{Environment.NewLine}");
        }

        public static void WriteMatrixNxNToFile(StreamWriter file,
            double[][] matrix, int rows, int columns, string arrayName = null)
        {
            string name = string.IsNullOrEmpty(arrayName) ? "" : $"{arrayName} = ";
            file.Write($"{name}[{Environment.NewLine}");
            for (int i = 0; i < rows; i++)
            {
                file.Write($"[");
                for (int j = 0; j < columns; j++)
                {
                    if (j == 0)
                    {
                        file.Write($"{(float)Math.Round(matrix[i][j], 1)}");
                    }
                    else
                    {
                        file.Write($", {(float)Math.Round(matrix[i][j], 1)}");
                    }
                }
                if (i == (rows - 1))
                {
                    file.Write($"]{Environment.NewLine}");
                }
                else
                {
                    file.Write($"],{Environment.NewLine}");
                }
            }
            string semicolon = string.IsNullOrEmpty(arrayName) ? "" : ";";
            file.Write($"]{semicolon}{Environment.NewLine}");
        }

        public static void WriteMatrixNxNToFile(StreamWriter file,
            int[][] matrix, int rows, int columns, string arrayName = null)
        {
            string name = string.IsNullOrEmpty(arrayName) ? "" : $"{arrayName} = ";
            file.Write($"{name}[{Environment.NewLine}");
            for (int i = 0; i < rows; i++)
            {
                file.Write($"[");
                for (int j = 0; j < columns; j++)
                {
                    if (j == 0)
                    {
                        file.Write($"{matrix[i][j]}");
                    }
                    else
                    {
                        file.Write($", {matrix[i][j]}");
                    }
                }
                if (i == (rows - 1))
                {
                    file.Write($"]{Environment.NewLine}");
                }
                else
                {
                    file.Write($"],{Environment.NewLine}");
                }
            }
            string semicolon = string.IsNullOrEmpty(arrayName) ? "" : ";";
            file.Write($"]{semicolon}{Environment.NewLine}");
        }

        public static void WriteArrayToFile(StreamWriter file,
            double[] array, int size, string arrayName = null)
        {
            string name = string.IsNullOrEmpty(arrayName) ? "" : $"{arrayName} = ";
            file.Write($"{name}[{Environment.NewLine}");
            for (int i = 0; i < size; i++)
            {
                if (i == 0)
                {
                    file.Write($"{(float)Math.Round(array[i], 1)}");;
                }
                else
                {
                    file.Write($", {(float)Math.Round(array[i], 1)}");
                }
            }
            string semicolon = string.IsNullOrEmpty(arrayName) ? "" : ";";
            file.Write($"]{semicolon}{Environment.NewLine}");
        }

        public static void WriteArrayToFile(StreamWriter file,
            int[] array, int size, string arrayName = null)
        {
            string name = string.IsNullOrEmpty(arrayName) ? "" : $"{arrayName} = ";
            file.Write($"{name}[{Environment.NewLine}");
            for (int i = 0; i < size; i++)
            {
                if (i == 0)
                {
                    file.Write($"{array[i]}");
                }
                else
                {
                    file.Write($", {array[i]}");
                }
            }
            string semicolon = string.IsNullOrEmpty(arrayName) ? "" : ";";
            file.Write($"]{semicolon}{Environment.NewLine}");
        }

        public static void WriteToDotnetDatFile(string text, bool? writeToDotnetDatFile = false, 
            StreamWriter dotnetDatFile = null)
        {
            if(writeToDotnetDatFile.HasValue && writeToDotnetDatFile.Value
                && dotnetDatFile != null)
            {
                dotnetDatFile.WriteLine(text);
            }
        }

        public static void WriteToOptimizationStudioDataFile(string text, 
            bool? writeToOptimizationStudioDataFile = false,
            StreamWriter optimizationStudioDataFile = null)
        {
            if (writeToOptimizationStudioDataFile.HasValue && writeToOptimizationStudioDataFile.Value
                && optimizationStudioDataFile != null)
            {
                optimizationStudioDataFile.WriteLine(text);
            }
        }

        public static double[] GetDoubleArray(int size)
        {
            double[] matrix = new double[size];
            return matrix;
        }

        public static int[] GetIntArrayArray(int size)
        {
            int[] matrix = new int[size];
            return matrix;
        }

        public static int[][] GetIntArrayArray(int rows, int columns)
        {
            int[][] matrix = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new int[columns];
            }
            return matrix;
        }

        public static double[][] GetDoubleArrayArray(int rows, int columns)
        {
            double[][] matrix = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new double[columns];
            }
            return matrix;
        }

        public static double[][][] GenerateArrayArrayArray(double[][][] matrix, 
            int dim1, int dim2, int dim3)
        {
            matrix = new double[dim1][][];
            for (int i = 0; i < dim1; i++)
            {
                matrix[i] = new double[dim2][];
                for (int j = 0; j < dim2; j++)
                {
                    matrix[i][j] = new double[dim3];
                }
            }
            return matrix;
        }
    }
}
