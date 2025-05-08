
// DigitalDiary/Diary.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class Diary
{
    private readonly string _filePath;
    private readonly FirebaseService _firebaseService;

    public Diary(string filePath, FirebaseService firebaseService)
    {
        _filePath = filePath;
        _firebaseService = firebaseService;
    }

    public async Task FetchAllEntriesFromFirebase()
    {
        try
        {
            var entries = await _firebaseService.GetAllEntries();
            using (var writer = new StreamWriter(_filePath, false))
            {
                foreach (var entry in entries)
                {
                    writer.WriteLine($"Entry: {entry.Key}");
                    for (int i = 1; i <= 30; i++)
                    {
                        if (entry.Value.ContainsKey($"line{i}"))
                        {
                            writer.WriteLine($"{i}. {entry.Value[$"line{i}"]}");
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from Firebase: {ex.Message}");
            File.WriteAllText(_filePath, string.Empty);
        }
    }

    public void WriteNewEntry()
    {
        var url = "https://diary-malupet.web.app/";
        
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
               
                try
                {
                    Process.Start(new ProcessStartInfo("chrome.exe", url) { UseShellExecute = true });
                    return;
                }
                catch
                {
                   
                    try
                    {
                        Process.Start(new ProcessStartInfo("msedge.exe", url) { UseShellExecute = true });
                        return;
                    }
                    catch
                    {
                      
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
           
                try
                {
                    Process.Start("open", $"-a Safari {url}");
                    return;
                }
                catch
                {
                   
                    try
                    {
                        Process.Start("open", $"-a Google\\ Chrome {url}");
                        return;
                    }
                    catch
                    {

                        Process.Start("open", url);
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                
                try
                {
                    Process.Start("xdg-open", url);
                    return;
                }
                catch
                {
                    try
                    {
                        Process.Start("google-chrome", url);
                        return;
                    }
                    catch
                    {
                        Process.Start("firefox", url);
                    }
                }
            }
            else
            {
              
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open browser: {ex.Message}");
            Console.WriteLine($"Please manually visit: {url}");
        }
    }

    public void ViewAllEntries()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine("No entries found.");
            return;
        }

        Console.WriteLine("\nAll Diary Entries:");
        Console.WriteLine(File.ReadAllText(_filePath));
    }

    public void SearchByDate()
    {
        Console.WriteLine("\nSearch by Date");
        Console.Write("Year (YYYY): ");
        var year = Console.ReadLine();
        Console.Write("Month (MM): ");
        var month = Console.ReadLine().PadLeft(2, '0');
        Console.Write("Day (DD): ");
        var day = Console.ReadLine().PadLeft(2, '0');

        var dateString = $"{year}-{month}-{day}";
        
        try
        {
            // Try Firebase first
            var entries = _firebaseService.GetEntriesByDate(dateString).Result;
            if (entries.Any())
            {
                Console.WriteLine($"\nEntries for {dateString}:");
                foreach (var entry in entries)
                {
                    Console.WriteLine($"Entry: {entry.Key}");
                    for (int i = 1; i <= 30; i++)
                    {
                        if (entry.Value.ContainsKey($"line{i}"))
                        {
                            Console.WriteLine($"{i}. {entry.Value[$"line{i}"]}");
                        }
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                // Fallback to local file
                Console.WriteLine("No entries found in Firebase, checking local file...");
                var lines = File.ReadAllLines(_filePath);
                bool found = false;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(dateString))
                    {
                        found = true;
                        Console.WriteLine(lines[i]);
                        i++;
                        while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                        {
                            Console.WriteLine(lines[i]);
                            i++;
                        }
                        Console.WriteLine();
                    }
                }
                
                if (!found) Console.WriteLine("No entries found for this date.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching entries: {ex.Message}");
        }
    }
}