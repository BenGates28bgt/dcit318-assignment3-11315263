using System;
using System.IO;
using System.Collections.Generic;

// ---------------- Student Class ----------------
public class Student
{
    public int Id { get; }
    public string FullName { get; }
    public int Score { get; }

    public Student(int id, string fullName, int score)
    {
        Id = id;
        FullName = fullName;
        Score = score;
    }

    public string GetGrade()
    {
        if (Score >= 80 && Score <= 100) return "A";
        if (Score >= 70) return "B";
        if (Score >= 60) return "C";
        if (Score >= 50) return "D";
        return "F";
    }

    public override string ToString() =>
        $"{FullName} (ID: {Id}): Score = {Score}, Grade = {GetGrade()}";
}

// ---------------- Custom Exceptions ----------------
public class InvalidScoreFormatException : Exception
{
    public InvalidScoreFormatException(string message) : base(message) { }
}

public class MissingFieldException : Exception
{
    public MissingFieldException(string message) : base(message) { }
}

// ---------------- StudentResultProcessor ----------------
public class StudentResultProcessor
{
    // Reads students from the text file and validates lines.
    public List<Student> ReadStudentsFromFile(string inputFilePath)
    {
        var students = new List<Student>();

        using (var reader = new StreamReader(inputFilePath))
        {
            string? line;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue; // skip empty lines

                var parts = line.Split(',');

                if (parts.Length < 3)
                    throw new MissingFieldException($"Line {lineNumber}: expected 3 fields (ID, FullName, Score). Line: '{line}'");

                var idPart = parts[0].Trim();
                var namePart = parts[1].Trim();
                var scorePart = parts[2].Trim();

                if (string.IsNullOrEmpty(idPart) || string.IsNullOrEmpty(namePart) || string.IsNullOrEmpty(scorePart))
                    throw new MissingFieldException($"Line {lineNumber}: one or more fields empty. Line: '{line}'");

                // Parse ID (optional check — safer)
                if (!int.TryParse(idPart, out int id))
                    throw new InvalidScoreFormatException($"Line {lineNumber}: ID is not an integer. Line: '{line}'");

                // Parse Score — required by assignment
                if (!int.TryParse(scorePart, out int score))
                    throw new InvalidScoreFormatException($"Line {lineNumber}: Score is not an integer. Line: '{line}'");

                if (score < 0 || score > 100)
                    throw new InvalidScoreFormatException($"Line {lineNumber}: Score out of range (0–100). Line: '{line}'");

                students.Add(new Student(id, namePart, score));
            }
        }

        return students;
    }

    // Writes a formatted report to the output file
    public void WriteReportToFile(List<Student> students, string outputFilePath)
    {
        using (var writer = new StreamWriter(outputFilePath, false))
        {
            foreach (var s in students)
            {
                writer.WriteLine($"{s.FullName} (ID: {s.Id}): Score = {s.Score}, Grade = {s.GetGrade()}");
            }
        }
    }
}

// ---------------- Program (Main) ----------------
class Program
{
    static void Main()
    {
        var processor = new StudentResultProcessor();

        // Use files located next to the running exe. Ensure students_input.txt is set to "Copy if newer".
        string inputFile = Path.Combine(AppContext.BaseDirectory, "students_input.txt");
        string outputFile = Path.Combine(AppContext.BaseDirectory, "students_report.txt");

        try
        {
            var students = processor.ReadStudentsFromFile(inputFile);
            processor.WriteReportToFile(students, outputFile);

            Console.WriteLine($"Report successfully written to: {outputFile}");
            Console.WriteLine("=== Preview ===");
            foreach (var s in students)
                Console.WriteLine(s.ToString());
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Input file not found: {inputFile}");
        }
        catch (InvalidScoreFormatException ex)
        {
            Console.WriteLine($"Invalid score format: {ex.Message}");
        }
        catch (MissingFieldException ex)
        {
            Console.WriteLine($"Missing field: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}
