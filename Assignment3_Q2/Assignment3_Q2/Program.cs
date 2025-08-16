using System;
using System.Collections.Generic;
using System.Linq;

// =================== GENERIC REPOSITORY ===================
public class Repository<T>
{
    private readonly List<T> items = new();

    public void Add(T item) => items.Add(item);

    public List<T> GetAll() => new(items);

    public T? GetById(Func<T, bool> predicate) => items.FirstOrDefault(predicate);

    public bool Remove(Func<T, bool> predicate)
    {
        var item = items.FirstOrDefault(predicate);
        if (item is null) return false;
        items.Remove(item);
        return true;
    }
}

// =================== DOMAIN CLASSES ===================
public class Patient
{
    public int Id { get; }
    public string Name { get; }
    public int Age { get; }
    public string Gender { get; }

    public Patient(int id, string name, int age, string gender)
    {
        Id = id;
        Name = name;
        Age = age;
        Gender = gender;
    }

    public override string ToString() => $"{Name} (ID: {Id}, Age: {Age}, Gender: {Gender})";
}

public class Prescription
{
    public int Id { get; }
    public int PatientId { get; }
    public string MedicationName { get; }
    public DateTime DateIssued { get; }

    public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
    {
        Id = id;
        PatientId = patientId;
        MedicationName = medicationName;
        DateIssued = dateIssued;
    }

    public override string ToString() =>
        $"Prescription {Id}: {MedicationName} — {DateIssued:yyyy-MM-dd}";
}

// =================== APP (Collections + Generics) ===================
public class HealthSystemApp
{
    private readonly Repository<Patient> _patientRepo = new();
    private readonly Repository<Prescription> _prescriptionRepo = new();
    private Dictionary<int, List<Prescription>> _prescriptionMap = new();

    // --- Seed 2–3 patients and 4–5 prescriptions (valid PatientIds) ---
    public void SeedData()
    {
        _patientRepo.Add(new Patient(1, "Alice Mensah", 29, "Female"));
        _patientRepo.Add(new Patient(2, "Kwesi Boateng", 41, "Male"));
        _patientRepo.Add(new Patient(3, "Esi Owusu", 35, "Female"));

        _prescriptionRepo.Add(new Prescription(101, 1, "Amoxicillin 500mg", DateTime.Today.AddDays(-10)));
        _prescriptionRepo.Add(new Prescription(102, 1, "Paracetamol 1g", DateTime.Today.AddDays(-7)));
        _prescriptionRepo.Add(new Prescription(103, 2, "Ibuprofen 400mg", DateTime.Today.AddDays(-5)));
        _prescriptionRepo.Add(new Prescription(104, 2, "Cetirizine 10mg", DateTime.Today.AddDays(-2)));
        _prescriptionRepo.Add(new Prescription(105, 3, "Azithromycin 500mg", DateTime.Today.AddDays(-1)));
    }

    // --- Group prescriptions by PatientId into Dictionary<int, List<Prescription>> ---
    public void BuildPrescriptionMap()
    {
        _prescriptionMap = _prescriptionRepo
            .GetAll()
            .GroupBy(p => p.PatientId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.DateIssued).ToList()
            );
    }

    // --- Retrieve prescriptions for a specific patient from the map ---
    public List<Prescription> GetPrescriptionsByPatientId(int patientId)
    {
        return _prescriptionMap.TryGetValue(patientId, out var list)
            ? list
            : new List<Prescription>();
    }

    // --- Print helpers ---
    public void PrintAllPatients()
    {
        Console.WriteLine("=== All Patients ===");
        foreach (var p in _patientRepo.GetAll())
        {
            Console.WriteLine(p);
        }
        Console.WriteLine();
    }

    public void PrintPrescriptionsForPatient(int id)
    {
        var patient = _patientRepo.GetById(p => p.Id == id);
        if (patient is null)
        {
            Console.WriteLine($"No patient found with ID {id}.");
            return;
        }

        Console.WriteLine($"=== Prescriptions for {patient.Name} (ID: {patient.Id}) ===");
        var prescriptions = GetPrescriptionsByPatientId(id);

        if (prescriptions.Count == 0)
        {
            Console.WriteLine("No prescriptions found.");
        }
        else
        {
            foreach (var pr in prescriptions)
            {
                Console.WriteLine(pr);
            }
        }
        Console.WriteLine();
    }
}

public class Program
{
    public static void Main()
    {
        var app = new HealthSystemApp();

        app.SeedData();                 // i. Add patients and prescriptions
        app.BuildPrescriptionMap();     // ii. Build Dictionary<int, List<Prescription>>
        app.PrintAllPatients();         // iii. Print all patients

        // iv. Select a patient and print prescriptions
        // (Using patient ID 1 from SeedData; you can change this to 2 or 3 to test)
        app.PrintPrescriptionsForPatient(1);
    }
}
