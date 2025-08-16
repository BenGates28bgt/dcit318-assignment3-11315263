using System;
using System.Collections.Generic;
using System.Linq;

// ------------------- MARKER INTERFACE -------------------
public interface IInventoryItem
{
    int Id { get; }
    string Name { get; }
    int Quantity { get; set; }
}

// ------------------- PRODUCT TYPES -------------------
public class ElectronicItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public string Brand { get; }
    public int WarrantyMonths { get; }

    public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        Brand = brand;
        WarrantyMonths = warrantyMonths;
    }

    public override string ToString()
        => $"[Electronic] ID:{Id} Name:{Name} Brand:{Brand} Qty:{Quantity} Warranty:{WarrantyMonths}mo";
}

public class GroceryItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; }

    public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        ExpiryDate = expiryDate;
    }

    public override string ToString()
        => $"[Grocery] ID:{Id} Name:{Name} Qty:{Quantity} Expiry:{ExpiryDate:yyyy-MM-dd}";
}

// ------------------- CUSTOM EXCEPTIONS -------------------
public class DuplicateItemException : Exception
{
    public DuplicateItemException(string message) : base(message) { }
}

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message) { }
}

public class InvalidQuantityException : Exception
{
    public InvalidQuantityException(string message) : base(message) { }
}

// ------------------- GENERIC INVENTORY REPOSITORY -------------------
public class InventoryRepository<T> where T : IInventoryItem
{
    private readonly Dictionary<int, T> _items = new();

    public void AddItem(T item)
    {
        if (_items.ContainsKey(item.Id))
            throw new DuplicateItemException($"An item with ID {item.Id} already exists.");
        _items.Add(item.Id, item);
    }

    public T GetItemById(int id)
    {
        if (!_items.TryGetValue(id, out var item))
            throw new ItemNotFoundException($"Item with ID {id} was not found.");
        return item;
    }

    public void RemoveItem(int id)
    {
        if (!_items.Remove(id))
            throw new ItemNotFoundException($"Cannot remove: item with ID {id} not found.");
    }

    public List<T> GetAllItems()
        => _items.Values.ToList();

    public void UpdateQuantity(int id, int newQuantity)
    {
        if (newQuantity < 0)
            throw new InvalidQuantityException($"Quantity cannot be negative (attempted {newQuantity}).");

        var item = GetItemById(id); // will throw ItemNotFoundException if absent
        item.Quantity = newQuantity;
        _items[id] = item;
    }
}

// ------------------- WAREHOUSE MANAGER -------------------
public class WareHouseManager
{
    private readonly InventoryRepository<ElectronicItem> _electronics = new();
    private readonly InventoryRepository<GroceryItem> _groceries = new();

    // Seed 2–3 items of each type
    public void SeedData()
    {
        // Electronics
        _electronics.AddItem(new ElectronicItem(1, "Smartphone", 10, "Samsung", 24));
        _electronics.AddItem(new ElectronicItem(2, "Laptop", 5, "Dell", 12));
        _electronics.AddItem(new ElectronicItem(3, "Bluetooth Speaker", 15, "JBL", 6));

        // Groceries
        _groceries.AddItem(new GroceryItem(101, "Rice (5kg)", 50, DateTime.Today.AddMonths(12)));
        _groceries.AddItem(new GroceryItem(102, "Beans (2kg)", 30, DateTime.Today.AddMonths(6)));
        _groceries.AddItem(new GroceryItem(103, "Cooking Oil (1L)", 20, DateTime.Today.AddMonths(8)));
    }

    // Generic print helper
    public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
    {
        var items = repo.GetAllItems();
        if (!items.Any())
        {
            Console.WriteLine("No items to display.");
            return;
        }

        foreach (var it in items)
        {
            Console.WriteLine(it);
        }
    }

    // Add item with try-catch to handle duplicates
    public void AddGroceryItemSafe(GroceryItem item)
    {
        try
        {
            _groceries.AddItem(item);
            Console.WriteLine($"Added grocery item: {item}");
        }
        catch (DuplicateItemException ex)
        {
            Console.WriteLine($"Add failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error adding grocery: {ex.Message}");
        }
    }

    public void AddElectronicItemSafe(ElectronicItem item)
    {
        try
        {
            _electronics.AddItem(item);
            Console.WriteLine($"Added electronic item: {item}");
        }
        catch (DuplicateItemException ex)
        {
            Console.WriteLine($"Add failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error adding electronic: {ex.Message}");
        }
    }

    // Increase stock (adds quantity) with try-catch
    public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
    {
        try
        {
            var item = repo.GetItemById(id);
            int newQty = item.Quantity + quantity;
            repo.UpdateQuantity(id, newQty);
            Console.WriteLine($"Updated quantity for ID {id} ({item.Name}) → {newQty}");
        }
        catch (ItemNotFoundException ex)
        {
            Console.WriteLine($"Cannot increase stock: {ex.Message}");
        }
        catch (InvalidQuantityException ex)
        {
            Console.WriteLine($"Invalid quantity: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error increasing stock: {ex.Message}");
        }
    }

    // Remove item by id with try-catch
    public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
    {
        try
        {
            repo.RemoveItem(id);
            Console.WriteLine($"Removed item with ID {id}");
        }
        catch (ItemNotFoundException ex)
        {
            Console.WriteLine($"Remove failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error removing item: {ex.Message}");
        }
    }

    // Convenience wrappers that use internal repos
    public void IncreaseGroceryStock(int id, int quantity) => IncreaseStock(_groceries, id, quantity);
    public void IncreaseElectronicStock(int id, int quantity) => IncreaseStock(_electronics, id, quantity);
    public void RemoveGroceryById(int id) => RemoveItemById(_groceries, id);
    public void RemoveElectronicById(int id) => RemoveItemById(_electronics, id);
    public void PrintAllGroceries() { Console.WriteLine("=== Groceries ==="); PrintAllItems(_groceries); }
    public void PrintAllElectronics() { Console.WriteLine("=== Electronics ==="); PrintAllItems(_electronics); }
}

// ------------------- PROGRAM (MAIN) -------------------
class Program
{
    static void Main()
    {
        var manager = new WareHouseManager();

        // Seed data and show inventory
        manager.SeedData();
        manager.PrintAllGroceries();
        Console.WriteLine();
        manager.PrintAllElectronics();
        Console.WriteLine();

        // 1) Try to add a duplicate item (should trigger DuplicateItemException)
        Console.WriteLine("Attempting to add a duplicate grocery item (ID 101)...");
        var duplicateGrocery = new GroceryItem(101, "Rice (5kg) - Duplicate", 10, DateTime.Today.AddMonths(12));
        manager.AddGroceryItemSafe(duplicateGrocery);
        Console.WriteLine();

        // 2) Try to remove a non-existent item (should trigger ItemNotFoundException)
        Console.WriteLine("Attempting to remove non-existent electronic item (ID 999)...");
        manager.RemoveElectronicById(999);
        Console.WriteLine();

        // 3) Try to update with invalid quantity (negative result) - should trigger InvalidQuantityException
        Console.WriteLine("Attempting to set invalid quantity (negative) for Grocery ID 102...");
        // We'll attempt to reduce a lot (simulate invalid new quantity)
        manager.IncreaseGroceryStock(102, -1000); // initial 30 + (-1000) => negative -> invalid
        Console.WriteLine();

        // Show final inventory after attempted operations
        Console.WriteLine("Final inventory state:");
        manager.PrintAllGroceries();
        Console.WriteLine();
        manager.PrintAllElectronics();
    }
}
