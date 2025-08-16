using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// -------------------- Marker interface --------------------
public interface IInventoryEntity
{
    int Id { get; }
}

// -------------------- Immutable record --------------------
public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

// -------------------- Generic Inventory Logger --------------------
public class InventoryLogger<T> where T : IInventoryEntity
{
    private readonly List<T> _log = new();
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public InventoryLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void Add(T item)
    {
        _log.Add(item);
    }

    public List<T> GetAll() => new(_log);

    public void SaveToFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(_log, _jsonOptions);
            File.WriteAllText(_filePath, json);
            Console.WriteLine($"Saved {_log.Count} items to '{_filePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to file '{_filePath}': {ex.Message}");
            throw;
        }
    }

    public void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"No file found at '{_filePath}'. Logger is empty.");
                _log.Clear();
                return;
            }

            var json = File.ReadAllText(_filePath);
            var items = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
            _log.Clear();
            if (items != null) _log.AddRange(items);
            Console.WriteLine($"Loaded {_log.Count} items from '{_filePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading from file '{_filePath}': {ex.Message}");
            throw;
        }
    }
}

// -------------------- InventoryApp (integration) --------------------
public class InventoryApp
{
    private readonly InventoryLogger<InventoryItem> _logger;

    // default file is inventory_log.json in the runtime folder
    public InventoryApp(string? filePath = null)
    {
        var fp = filePath ?? Path.Combine(AppContext.BaseDirectory, "inventory_log.json");
        _logger = new InventoryLogger<InventoryItem>(fp);
    }

    public void SeedSampleData()
    {
        // Add 3–5 sample items
        _logger.Add(new InventoryItem(1, "Hammer", 25, DateTime.UtcNow));
        _logger.Add(new InventoryItem(2, "Nails (100pcs)", 200, DateTime.UtcNow));
        _logger.Add(new InventoryItem(3, "Screwdriver", 40, DateTime.UtcNow));
        _logger.Add(new InventoryItem(4, "Rice (5kg)", 50, DateTime.UtcNow));
        _logger.Add(new InventoryItem(5, "LED Bulb", 120, DateTime.UtcNow));
    }

    public void SaveData() => _logger.SaveToFile();

    public void LoadData() => _logger.LoadFromFile();

    public void PrintAllItems()
    {
        var items = _logger.GetAll();
        if (items.Count == 0)
        {
            Console.WriteLine("No items in inventory.");
            return;
        }

        Console.WriteLine("=== Inventory Items ===");
        foreach (var it in items)
        {
            Console.WriteLine($"ID:{it.Id} Name:{it.Name} Qty:{it.Quantity} Added:{it.DateAdded:yyyy-MM-dd}");
        }
    }
}

// -------------------- Program (Main) --------------------
class Program
{
    static void Main()
    {
        // 1) Seed data and save
        var app = new InventoryApp();
        app.SeedSampleData();
        Console.WriteLine("Seeded sample data:");
        app.PrintAllItems();
        app.SaveData();

        // 2) Simulate clearing memory and a fresh run
        Console.WriteLine("\n--- Simulating a new session (clearing memory) ---\n");
        var freshApp = new InventoryApp();
        freshApp.LoadData();
        freshApp.PrintAllItems();

        Console.WriteLine("\nDone. The inventory is persisted in inventory_log.json (next to the exe).");
    }
}
