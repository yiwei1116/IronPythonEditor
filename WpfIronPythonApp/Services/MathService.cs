using System;
using System.Collections.Generic;
using System.Linq;
using WpfIronPythonApp.Services.ApiRegistry;

namespace WpfIronPythonApp.Services
{
    /// <summary>
    /// 數學計算服務，提供各種數學運算功能
    /// </summary>
    [ApiService("math", Description = "數學計算服務，提供各種數學運算和統計功能", Version = "1.0.0")]
    public class MathService
    {
        /// <summary>
        /// 計算平均值
        /// </summary>
        [ApiMethod(Description = "計算數字列表的平均值", 
                   Example = "avg = math.average([1, 2, 3, 4, 5])\nprint(f'Average: {avg}')", 
                   Category = "Statistics")]
        public double average([ApiParameter(Description = "數字列表", Example = "[1, 2, 3, 4, 5]")] List<double> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new ArgumentException("數字列表不能為空");

            return numbers.Average();
        }

        /// <summary>
        /// 計算總和
        /// </summary>
        [ApiMethod(Description = "計算數字列表的總和", 
                   Example = "total = math.sum([1, 2, 3, 4, 5])\nprint(f'Sum: {total}')", 
                   Category = "Basic Math")]
        public double sum([ApiParameter(Description = "數字列表", Example = "[1, 2, 3, 4, 5]")] List<double> numbers)
        {
            if (numbers == null)
                throw new ArgumentNullException(nameof(numbers));

            return numbers.Sum();
        }

        /// <summary>
        /// 找出最大值
        /// </summary>
        [ApiMethod(Description = "找出數字列表中的最大值", 
                   Example = "max_val = math.max([1, 5, 3, 9, 2])\nprint(f'Max: {max_val}')", 
                   Category = "Statistics")]
        public double max([ApiParameter(Description = "數字列表", Example = "[1, 5, 3, 9, 2]")] List<double> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new ArgumentException("數字列表不能為空");

            return numbers.Max();
        }

        /// <summary>
        /// 找出最小值
        /// </summary>
        [ApiMethod(Description = "找出數字列表中的最小值", 
                   Example = "min_val = math.min([1, 5, 3, 9, 2])\nprint(f'Min: {min_val}')", 
                   Category = "Statistics")]
        public double min([ApiParameter(Description = "數字列表", Example = "[1, 5, 3, 9, 2]")] List<double> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new ArgumentException("數字列表不能為空");

            return numbers.Min();
        }

        /// <summary>
        /// 計算標準差
        /// </summary>
        [ApiMethod(Description = "計算數字列表的標準差", 
                   Example = "std = math.standard_deviation([1, 2, 3, 4, 5])\nprint(f'Std Dev: {std}')", 
                   Category = "Statistics")]
        public double standard_deviation([ApiParameter(Description = "數字列表", Example = "[1, 2, 3, 4, 5]")] List<double> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new ArgumentException("數字列表不能為空");

            double avg = numbers.Average();
            double sumOfSquares = numbers.Sum(x => Math.Pow(x - avg, 2));
            return Math.Sqrt(sumOfSquares / numbers.Count);
        }

        /// <summary>
        /// 計算中位數
        /// </summary>
        [ApiMethod(Description = "計算數字列表的中位數", 
                   Example = "median = math.median([1, 3, 5, 7, 9])\nprint(f'Median: {median}')", 
                   Category = "Statistics")]
        public double median([ApiParameter(Description = "數字列表", Example = "[1, 3, 5, 7, 9]")] List<double> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new ArgumentException("數字列表不能為空");

            var sorted = numbers.OrderBy(x => x).ToList();
            int count = sorted.Count;

            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        /// <summary>
        /// 計算階乘
        /// </summary>
        [ApiMethod(Description = "計算數字的階乘", 
                   Example = "fact = math.factorial(5)\nprint(f'5! = {fact}')", 
                   Category = "Basic Math")]
        public long factorial([ApiParameter(Description = "要計算階乘的數字", Example = "5")] int n)
        {
            if (n < 0)
                throw new ArgumentException("階乘不能計算負數");

            if (n == 0 || n == 1)
                return 1;

            long result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        /// <summary>
        /// 計算冪次
        /// </summary>
        [ApiMethod(Description = "計算數字的冪次", 
                   Example = "result = math.power(2, 8)\nprint(f'2^8 = {result}')", 
                   Category = "Basic Math")]
        public double power([ApiParameter(Description = "底數", Example = "2")] double baseNum,
                           [ApiParameter(Description = "指數", Example = "8")] double exponent)
        {
            return Math.Pow(baseNum, exponent);
        }

        /// <summary>
        /// 計算平方根
        /// </summary>
        [ApiMethod(Description = "計算數字的平方根", 
                   Example = "sqrt_val = math.sqrt(16)\nprint(f'√16 = {sqrt_val}')", 
                   Category = "Basic Math")]
        public double sqrt([ApiParameter(Description = "要計算平方根的數字", Example = "16")] double number)
        {
            if (number < 0)
                throw new ArgumentException("不能計算負數的平方根");

            return Math.Sqrt(number);
        }

        /// <summary>
        /// 計算絕對值
        /// </summary>
        [ApiMethod(Description = "計算數字的絕對值", 
                   Example = "abs_val = math.abs(-5)\nprint(f'|-5| = {abs_val}')", 
                   Category = "Basic Math")]
        public double abs([ApiParameter(Description = "數字", Example = "-5")] double number)
        {
            return Math.Abs(number);
        }

        /// <summary>
        /// 四捨五入
        /// </summary>
        [ApiMethod(Description = "將數字四捨五入到指定小數位數", 
                   Example = "rounded = math.round(3.14159, 2)\nprint(f'Rounded: {rounded}')", 
                   Category = "Basic Math")]
        public double round([ApiParameter(Description = "要四捨五入的數字", Example = "3.14159")] double number,
                           [ApiParameter(Description = "小數位數", Example = "2", IsOptional = true)] int digits = 0)
        {
            return Math.Round(number, digits);
        }

        /// <summary>
        /// 計算最大公約數
        /// </summary>
        [ApiMethod(Description = "計算兩個數字的最大公約數", 
                   Example = "gcd = math.gcd(48, 18)\nprint(f'GCD(48, 18) = {gcd}')", 
                   Category = "Number Theory")]
        public int gcd([ApiParameter(Description = "第一個數字", Example = "48")] int a,
                      [ApiParameter(Description = "第二個數字", Example = "18")] int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Math.Abs(a);
        }

        /// <summary>
        /// 計算最小公倍數
        /// </summary>
        [ApiMethod(Description = "計算兩個數字的最小公倍數", 
                   Example = "lcm = math.lcm(12, 8)\nprint(f'LCM(12, 8) = {lcm}')", 
                   Category = "Number Theory")]
        public int lcm([ApiParameter(Description = "第一個數字", Example = "12")] int a,
                      [ApiParameter(Description = "第二個數字", Example = "8")] int b)
        {
            return Math.Abs(a * b) / gcd(a, b);
        }

        /// <summary>
        /// 檢查是否為質數
        /// </summary>
        [ApiMethod(Description = "檢查數字是否為質數", 
                   Example = "is_prime = math.is_prime(17)\nprint(f'17 is prime: {is_prime}')", 
                   Category = "Number Theory")]
        public bool is_prime([ApiParameter(Description = "要檢查的數字", Example = "17")] int number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (int i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        /// <summary>
        /// 生成斐波那契數列
        /// </summary>
        [ApiMethod(Description = "生成指定長度的斐波那契數列", 
                   Example = "fib = math.fibonacci(10)\nprint(f'Fibonacci: {fib}')", 
                   Category = "Sequences")]
        public List<long> fibonacci([ApiParameter(Description = "數列長度", Example = "10")] int count)
        {
            if (count <= 0)
                throw new ArgumentException("數列長度必須大於0");

            var result = new List<long>();
            if (count >= 1) result.Add(0);
            if (count >= 2) result.Add(1);

            for (int i = 2; i < count; i++)
            {
                result.Add(result[i - 1] + result[i - 2]);
            }

            return result;
        }

        /// <summary>
        /// 計算統計摘要
        /// </summary>
        [ApiMethod(Description = "計算數字列表的統計摘要", 
                   Example = "stats = math.statistics([1, 2, 3, 4, 5])\nprint(f'Mean: {stats[\"mean\"]}')", 
                   Category = "Statistics")]
        public Dictionary<string, double> statistics([ApiParameter(Description = "數字列表", Example = "[1, 2, 3, 4, 5]")] List<double> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new ArgumentException("數字列表不能為空");

            return new Dictionary<string, double>
            {
                ["count"] = numbers.Count,
                ["sum"] = sum(numbers),
                ["mean"] = average(numbers),
                ["min"] = min(numbers),
                ["max"] = max(numbers),
                ["median"] = median(numbers),
                ["std_dev"] = standard_deviation(numbers)
            };
        }
    }
} 