using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PVHuy
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = Encoding.UTF8;
            // Input Number
            int N = Program.InputNumber();
            // Input Matrix
            int[,] Matrix = Program.InputMatrix(N);

            // Find a submatrix with two equal diagonals
            int CheckContinue = 0;
            for(int a=0;a<N-1;a++)
            {
                for (int m = 0; m < a + 1; m++)
                {
                    for (int n = 0; n < a + 1; n++)
                    {

                        int[,] Submatrix = new int[N - a, N - a];
                        for (int i = 0; i < N - a; i++)
                        {
                            for (int j = 0; j < N - a; j++)
                            {

                                Submatrix[i, j] = Matrix[i + m, j + n];
                            }
                        }
                        if(CheckTotalDiagonal(Submatrix))
                        {
                            CheckContinue = CheckContinue + 1;
                            Console.WriteLine("Kết quả là: ");
                            Program.PrintMatrix(Submatrix);
                        }
                        if (CheckContinue == 1)
                        {
                            break;
                        }    
                    }
                    if (CheckContinue == 1)
                    {
                        break;
                    }
                }
                if (CheckContinue == 1)
                {
                    break;
                }
            }
            if(CheckContinue == 0)
            {
                Console.WriteLine("Không có ma trận con nào thỏa mãn bài toán");
            }
        }


        static int InputNumber()
        {
            int N;
            InputN:
            Console.WriteLine("Nhập vào số tự nhiên n: ");
            string firstInput = Console.ReadLine();
            try
            {
                N = Int32.Parse(firstInput);
                if (N < 2 || N > 20)
                {
                    Console.WriteLine("n trong khoảng từ 2 tới 20");
                    goto InputN;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Đầu vào không phù hợp. Vui lòng nhập lại !");
                goto InputN;
            }
            return N;
        }

        static int[,] InputMatrix(int N)
        {
            InputMatrix:
            int[,] Matrix = new int[N, N];
            Console.WriteLine("Nhập vào ma trận: " + N.ToString() + "*" + N.ToString() + " :");
            for (int i = 0; i < N; i++)
            {
                string input = Console.ReadLine();
                string[] inputStringArray = input.Split(" ",StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < N; j++)
                {
                    try
                    {
                       if( inputStringArray.Length == N)
                       {
                            Matrix[i, j] = Int32.Parse(inputStringArray[j]);
                       }
                       else
                       {
                            Console.WriteLine("Đầu vào không phù hợp. Vui lòng nhập lại !");
                            goto InputMatrix;
                        }
                }
                    catch (Exception)
                    {
                        Console.WriteLine("Đầu vào không phù hợp. Vui lòng nhập lại !");
                        goto InputMatrix;
                    }
                }
            }

            return Matrix;
        }

        static void PrintMatrix(int[,] Matrix)
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    Console.Write(Matrix[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        static bool CheckTotalDiagonal( int[,] Matrix)
        {
            int Td1= 0;
            int Td2 = 0;

            // Total Diagonal 1
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                Td1= Td1+ Matrix[i, i];
            }

            // Total Diagonal 2
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                Td2 = Td2 + Matrix[i, Matrix.GetLength(1)-i-1];
            }

            if (Td1== Td2)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
