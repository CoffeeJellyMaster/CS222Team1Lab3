namespace Digital_Diary;

public class Diary
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; }

    public Diary(string title, string content)
    {
        Title = title;
        Content = content;
        CreatedAt = DateTime.Now;
    }

    public void DisplayEntry()
    {
        Console.WriteLine($"\n{Title} - {CreatedAt}");
        Console.WriteLine(Content);
    }
}