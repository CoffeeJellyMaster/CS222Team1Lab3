// DigitalDiary/FirebaseService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    private const string CredentialsFile = "firebase_malupet.json";

    public FirebaseService()
    {
        try
        {
            // Verify credentials file exists
            if (!File.Exists(CredentialsFile))
            {
                throw new FileNotFoundException($"Firebase credentials file not found at: {Path.GetFullPath(CredentialsFile)}");
            }

            // Initialize Firestore with explicit credentials
            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = GetProjectIdFromCredentials(),
                CredentialsPath = CredentialsFile,
                EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction
            }.Build();

            // Test connection immediately
            TestConnectionAsync().Wait();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[FIREBASE INIT ERROR]");
            Console.WriteLine($"Message: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            
            Console.ResetColor();
            throw new ApplicationException("Failed to initialize Firebase Service", ex);
        }
    }

    private string GetProjectIdFromCredentials()
    {
        try
        {
            var json = File.ReadAllText(CredentialsFile);
            var projectId = Newtonsoft.Json.Linq.JObject.Parse(json)["project_id"]?.ToString();
            
            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidDataException("Project ID not found in credentials file");
            }
            
            return projectId;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to parse Firebase credentials", ex);
        }
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            var testQuery = _firestoreDb.Collection("diaries").Limit(1);
            await testQuery.GetSnapshotAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Firestore connection test failed", ex);
        }
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> GetAllEntries()
    {
        try
        {
            var entries = new Dictionary<string, Dictionary<string, string>>();
            var diariesRef = _firestoreDb.Collection("diaries");
            var snapshot = await diariesRef.GetSnapshotAsync();

            foreach (var document in snapshot.Documents)
            {
                entries[document.Id] = ProcessDiaryDocument(document);
            }

            return entries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllEntries: {ex.Message}");
            throw;
        }
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> GetEntriesByDate(string date)
    {
        try
        {
            var filteredEntries = new Dictionary<string, Dictionary<string, string>>();
            var query = _firestoreDb.Collection("diaries")
                .WhereGreaterThanOrEqualTo("lastModified", $"{date}T00:00:00")
                .WhereLessThan("lastModified", $"{date}T23:59:59.999Z");

            var snapshot = await query.GetSnapshotAsync();

            foreach (var document in snapshot.Documents)
            {
                filteredEntries[document.Id] = ProcessDiaryDocument(document);
            }

            return filteredEntries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetEntriesByDate: {ex.Message}");
            throw;
        }
    }

    private Dictionary<string, string> ProcessDiaryDocument(DocumentSnapshot document)
    {
        var entryData = new Dictionary<string, string>();
        var documentData = document.ToDictionary();

        // Add metadata fields
        if (documentData.TryGetValue("lastModified", out var lastModified))
        {
            entryData["lastModified"] = lastModified.ToString();
        }

        // Process all line1-line30 fields in order
        for (int i = 1; i <= 30; i++)
        {
            var lineKey = $"line{i}";
            if (documentData.TryGetValue(lineKey, out var lineValue) && lineValue != null)
            {
                entryData[lineKey] = lineValue.ToString();
            }
        }

        return entryData;
    }
}