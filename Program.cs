// DigitalDiary/Program.cs
using System;

class Program
{
    static void Main(string[] args)
    {
        var firebaseService = new FirebaseService();
        var diary = new Diary("diary.txt", firebaseService);
        try { System.IO.File.Delete("diary.txt"); } catch { /* Ignore not exisx */ }
        
        try
        {
            diary.FetchAllEntriesFromFirebase().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from Firebase: {ex.Message}");
            Console.WriteLine("Using local storage only.");
        }

        // Main menu loop
        bool running = true;
        while (running)
        {
            Console.WriteLine("\nDigital Diary Menu:");
            Console.WriteLine("1. Write a New Entry");
            Console.WriteLine("2. View All Entries");
            Console.WriteLine("3. Search Entry by Date");
            Console.WriteLine("4. Exit");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    diary.WriteNewEntry();
                    break;
                case "2":
                    diary.ViewAllEntries();
                    break;
                case "3":
                    diary.SearchByDate();
                    break;
                case "4":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }
}