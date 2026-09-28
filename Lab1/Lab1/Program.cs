using System.Globalization;

namespace Lab1
{
    internal class Program
    {
        const double EPS = 1e-5;
        const double ZERO = 1e-10;
        const int MAX_ITER = 1000;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("===== Лабораторная работа №1. Решение СЛАУ =====");
                Console.WriteLine("1 - метод Гаусса с выбором главного элемента");
                Console.WriteLine("2 - метод простой итерации");
                Console.WriteLine("3 - метод Зейделя");
                Console.WriteLine("0 - выход");
                Console.Write("Выберите пункт: ");
                string choice = Console.ReadLine();

                if (choice == "0")
                    break;

                if (choice == "1")
                {
                    int n;
                    double[,] a = ReadSystem("input1.txt", out n);
                    if (a != null)
                        Gauss(a, n);
                }
                else if (choice == "2")
                {
                    int n;
                    double[,] a = ReadSystem("input2.txt", out n);
                    if (a != null)
                        SimpleIteration(a, n);
                }
                else if (choice == "3")
                {
                    int n;
                    double[,] a = ReadSystem("input2.txt", out n);
                    if (a != null)
                        Seidel(a, n);
                }
                else
                {
                    Console.WriteLine("Нет такого пункта");
                }
            }
        }

        static double[,] ReadSystem(string defaultFile, out int n)
        {
            n = 0;
            Console.Write("Ввод: 1 - из файла, 2 - с клавиатуры: ");
            string mode = Console.ReadLine();
            try
            {
                if (mode == "2")
                {
                    Console.Write("Порядок системы n = ");
                    n = int.Parse(Console.ReadLine());
                    double[,] a = new double[n, n + 1];
                    Console.WriteLine("Введите по строкам коэффициенты и свободный член через пробел:");
                    for (int i = 0; i < n; i++)
                    {
                        Console.Write("уравнение {0}: ", i + 1);
                        string[] parts = Console.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != n + 1)
                        {
                            Console.WriteLine("Нужно ввести {0} чисел, повторите", n + 1);
                            i--;
                            continue;
                        }
                        for (int j = 0; j <= n; j++)
                            a[i, j] = ToDouble(parts[j]);
                    }
                    return a;
                }
                else
                {
                    Console.Write("Имя файла (Enter - {0}): ", defaultFile);
                    string name = Console.ReadLine();
                    if (name == "")
                        name = defaultFile;
                    string[] nums = File.ReadAllText(name).Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    n = int.Parse(nums[0]);
                    double[,] a = new double[n, n + 1];
                    int k = 1;
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j <= n; j++)
                        {
                            a[i, j] = ToDouble(nums[k]);
                            k++;
                        }
                    return a;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при вводе данных: " + ex.Message);
                return null;
            }
        }

        static double ToDouble(string s)
        {
            return double.Parse(s.Replace(',', '.'));
        }

        static void PrintMatrix(double[,] a, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    Console.Write("{0,10:F4}", a[i, j]);
                Console.WriteLine("  |{0,10:F4}", a[i, n]);
            }
        }

        static void Gauss(double[,] a, int n)
        {
            double[,] a0 = (double[,])a.Clone();

            int[] mainRow = new int[n];
            int[] mainCol = new int[n];
            bool[] usedRow = new bool[n];
            bool[] usedCol = new bool[n];

            Console.WriteLine();
            Console.WriteLine("Исходная расширенная матрица:");
            PrintMatrix(a, n);

            for (int k = 0; k < n; k++)
            {
                int p = -1, q = -1;
                double max = 0;
                for (int i = 0; i < n; i++)
                {
                    if (usedRow[i]) continue;
                    for (int j = 0; j < n; j++)
                    {
                        if (usedCol[j]) continue;
                        if (Math.Abs(a[i, j]) > max)
                        {
                            max = Math.Abs(a[i, j]);
                            p = i;
                            q = j;
                        }
                    }
                }

                if (max < ZERO)
                {
                    Console.WriteLine();
                    Console.WriteLine("Шаг {0}: все оставшиеся коэффициенты равны 0, главный элемент выбрать нельзя.", k + 1);
                    Console.WriteLine("Определитель системы равен 0, ранг матрицы системы = {0}.", k);

                    bool sovmestna = true;
                    for (int i = 0; i < n; i++)
                        if (!usedRow[i] && Math.Abs(a[i, n]) > ZERO)
                            sovmestna = false;

                    if (!sovmestna)
                    {
                        Console.WriteLine("Есть уравнение вида 0 = b, где b не равно 0.");
                        Console.WriteLine("Система несовместна - решений нет.");
                        return;
                    }

                    Console.WriteLine("Оставшиеся уравнения имеют вид 0 = 0.");
                    Console.WriteLine("Система имеет бесконечно много решений.");
                    Console.WriteLine("Одно из них (свободные неизвестные приравняем нулю):");
                    double[] xs = BackSubstitution(a, n, k, mainRow, mainCol);
                    for (int i = 0; i < n; i++)
                        Console.WriteLine("x{0} = {1,12:F6}", i + 1, xs[i]);
                    Check(a0, xs, n);
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Шаг {0}. Главный элемент a[{1},{2}] = {3:F4}", k + 1, p + 1, q + 1, a[p, q]);

                for (int i = 0; i < n; i++)
                {
                    if (i == p || usedRow[i]) continue;
                    double m = -a[i, q] / a[p, q];
                    Console.WriteLine("  m{0} = -a[{0},{1}] / a[{2},{1}] = {3:F6}", i + 1, q + 1, p + 1, m);
                    for (int j = 0; j <= n; j++)
                        a[i, j] += m * a[p, j];
                    a[i, q] = 0;
                }

                usedRow[p] = true;
                usedCol[q] = true;
                mainRow[k] = p;
                mainCol[k] = q;

                Console.WriteLine("Матрица после шага {0}:", k + 1);
                PrintMatrix(a, n);
            }

            Console.WriteLine();
            Console.WriteLine("Обратный ход (по главным строкам, начиная с последней):");
            double[] x = BackSubstitution(a, n, n, mainRow, mainCol);

            Console.WriteLine();
            Console.WriteLine("Решение:");
            for (int i = 0; i < n; i++)
                Console.WriteLine("x{0} = {1,12:F6}", i + 1, x[i]);

            Check(a0, x, n);
        }

        static double[] BackSubstitution(double[,] a, int n, int steps, int[] mainRow, int[] mainCol)
        {
            double[] x = new double[n];
            for (int k = steps - 1; k >= 0; k--)
            {
                int p = mainRow[k];
                int q = mainCol[k];
                double s = a[p, n];
                for (int j = 0; j < n; j++)
                    if (j != q)
                        s -= a[p, j] * x[j];
                x[q] = s / a[p, q];
                Console.WriteLine("  из строки {0}: x{1} = {2:F6}", p + 1, q + 1, x[q]);
            }
            return x;
        }

        static void Check(double[,] a, double[] x, int n)
        {
            Console.WriteLine();
            Console.WriteLine("Проверка подстановкой (невязки r = b - Ax):");
            for (int i = 0; i < n; i++)
            {
                double s = 0;
                for (int j = 0; j < n; j++)
                    s += a[i, j] * x[j];
                Console.WriteLine("r{0} = {1:E2}", i + 1, a[i, n] - s);
            }
        }

        static bool PrintCD(double[,] a, int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(a[i, i]) < ZERO)
                {
                    Console.WriteLine("a[{0},{0}] = 0, выразить x{0} из уравнения {0} нельзя.", i + 1);
                    Console.WriteLine("Нужно переставить или скомбинировать уравнения.");
                    return false;
                }
            }

            Console.WriteLine("Матрица C и вектор d (x = Cx + d):");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == i)
                        Console.Write("{0,12:F6}", 0.0);
                    else
                        Console.Write("{0,12:F6}", -a[i, j] / a[i, i]);
                }
                Console.WriteLine("  |{0,12:F6}", a[i, n] / a[i, i]);
            }
            return true;
        }

        static bool CheckConvergence(double[,] a, int n)
        {
            double norm1 = 0;
            Console.Write("Суммы модулей по столбцам:");
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int i = 0; i < n; i++)
                    if (i != j)
                        sum += Math.Abs(a[i, j] / a[i, i]);
                Console.Write("{0,10:F6}", sum);
                if (sum > norm1)
                    norm1 = sum;
            }
            Console.WriteLine();

            double normInf = 0;
            Console.Write("Суммы модулей по строкам: ");
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                    if (j != i)
                        sum += Math.Abs(a[i, j] / a[i, i]);
                Console.Write("{0,10:F6}", sum);
                if (sum > normInf)
                    normInf = sum;
            }
            Console.WriteLine();

            Console.WriteLine("||C||1   = {0:F6}", norm1);
            Console.WriteLine("||C||inf = {0:F6}", normInf);
            if (norm1 < 1 || normInf < 1)
            {
                Console.WriteLine("Условие сходимости ||C|| < 1 выполнено, метод сходится.");
                return true;
            }
            Console.WriteLine("Обе нормы >= 1, достаточное условие сходимости НЕ выполнено, метод может расходиться!");
            return false;
        }

        static bool AskContinue()
        {
            Console.Write("Всё равно выполнять итерации? (y/n): ");
            string ans = Console.ReadLine();
            return ans == "y" || ans == "Y" || ans == "д" || ans == "Д";
        }

        static void PrintHead(int n)
        {
            Console.WriteLine();
            Console.Write("{0,4}", "k");
            for (int i = 0; i < n; i++)
                Console.Write("{0,12}", "x" + (i + 1));
            Console.WriteLine("{0,14}", "max|dx|");
        }

        static void SimpleIteration(double[,] a, int n)
        {
            Console.WriteLine();
            Console.WriteLine("Метод простой итерации. Система:");
            PrintMatrix(a, n);
            if (!PrintCD(a, n))
                return;
            if (!CheckConvergence(a, n) && !AskContinue())
                return;

            double[] xOld = new double[n];
            double[] x = new double[n];

            for (int i = 0; i < n; i++)
                xOld[i] = a[i, n] / a[i, i];

            PrintHead(n);
            Console.Write("{0,4}", 0);
            for (int i = 0; i < n; i++)
                Console.Write("{0,12:F6}", xOld[i]);
            Console.WriteLine();

            int k = 0;
            double diff;
            do
            {
                k++;
                for (int i = 0; i < n; i++)
                {
                    double s = a[i, n];
                    for (int j = 0; j < n; j++)
                        if (j != i)
                            s -= a[i, j] * xOld[j];
                    x[i] = s / a[i, i];
                }

                diff = 0;
                for (int i = 0; i < n; i++)
                {
                    if (Math.Abs(x[i] - xOld[i]) > diff)
                        diff = Math.Abs(x[i] - xOld[i]);
                    xOld[i] = x[i];
                }

                Console.Write("{0,4}", k);
                for (int i = 0; i < n; i++)
                    Console.Write("{0,12:F6}", x[i]);
                Console.WriteLine("{0,14:E2}", diff);

                if (diff > 1e10 || double.IsNaN(diff))
                {
                    Console.WriteLine("Значения неограниченно растут - итерации расходятся.");
                    return;
                }
            } while (diff >= EPS && k < MAX_ITER);

            if (diff >= EPS)
            {
                Console.WriteLine("За {0} итераций точность не достигнута.", MAX_ITER);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Точность {0} достигнута за {1} итераций. Решение:", EPS, k);
            for (int i = 0; i < n; i++)
                Console.WriteLine("x{0} = {1,12:F6}", i + 1, x[i]);
            Check(a, x, n);
        }

        static void Seidel(double[,] a, int n)
        {
            Console.WriteLine();
            Console.WriteLine("Метод Зейделя. Система:");
            PrintMatrix(a, n);
            if (!PrintCD(a, n))
                return;
            if (!CheckConvergence(a, n) && !AskContinue())
                return;

            double[] x = new double[n];

            for (int i = 0; i < n; i++)
                x[i] = a[i, n] / a[i, i];

            PrintHead(n);
            Console.Write("{0,4}", 0);
            for (int i = 0; i < n; i++)
                Console.Write("{0,12:F6}", x[i]);
            Console.WriteLine();

            int k = 0;
            double diff;
            do
            {
                k++;
                diff = 0;
                for (int i = 0; i < n; i++)
                {
                    double s = a[i, n];
                    for (int j = 0; j < n; j++)
                        if (j != i)
                            s -= a[i, j] * x[j];
                    s = s / a[i, i];
                    if (Math.Abs(s - x[i]) > diff)
                        diff = Math.Abs(s - x[i]);
                    x[i] = s;
                }

                Console.Write("{0,4}", k);
                for (int i = 0; i < n; i++)
                    Console.Write("{0,12:F6}", x[i]);
                Console.WriteLine("{0,14:E2}", diff);

                if (diff > 1e10 || double.IsNaN(diff))
                {
                    Console.WriteLine("Значения неограниченно растут - итерации расходятся.");
                    return;
                }
            } while (diff >= EPS && k < MAX_ITER);

            if (diff >= EPS)
            {
                Console.WriteLine("За {0} итераций точность не достигнута.", MAX_ITER);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Точность {0} достигнута за {1} итераций. Решение:", EPS, k);
            for (int i = 0; i < n; i++)
                Console.WriteLine("x{0} = {1,12:F6}", i + 1, x[i]);
            Check(a, x, n);
        }
    }
}
