// See https://aka.ms/new-console-template for more information
using Etailor.API.Ultity;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        var hashPassword = Ultils.HashPassword(password);

        Console.WriteLine("Hash password: " + hashPassword);

        Console.Write("Enter password again: ");

        string passwordAgain = Console.ReadLine();

        if (Ultils.VerifyPassword(passwordAgain, hashPassword))
        {
            Console.WriteLine("Correct");
        }
        else
        {
            Console.WriteLine("Fail");
        }
    }
}