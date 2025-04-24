using System;

namespace Digital_Diary;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Digital Diary!");
        
        var program = new Program(); // Create an instance
        program.Menu();
        
        Diary diary = new Diary("My First Entry", "This is my first entry using .NET 9 and Rider!");
        diary.DisplayEntry();
    }

    public void Menu()
    {
        Console.WriteLine("Testing Lang\n");
    }
    
    // public static void Menu()
    // {
    //     Console.WriteLine("🛠️ Menu function is running...");
    // }
    
}