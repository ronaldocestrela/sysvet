using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(GenerateCpf());
    }

    static string GenerateCpf()
    {
        var random = new Random();
        int[] cpf = new int[11];
        int sum1 = 0, sum2 = 0;

        for (int i = 0; i < 9; i++)
        {
            cpf[i] = random.Next(0, 10);
            sum1 += cpf[i] * (10 - i);
            sum2 += cpf[i] * (11 - i);
        }

        int d1 = sum1 % 11;
        cpf[9] = d1 < 2 ? 0 : 11 - d1;

        sum2 += cpf[9] * 2;
        int d2 = sum2 % 11;
        cpf[10] = d2 < 2 ? 0 : 11 - d2;

        return string.Join("", cpf);
    }
}
