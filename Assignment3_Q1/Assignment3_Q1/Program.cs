using System;
using System.Collections.Generic;

// ===== Step a: Define Transaction record =====
public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

// ===== Step b: Define ITransactionProcessor interface =====
public interface ITransactionProcessor
{
    void Process(Transaction transaction);
}

// ===== Step c: Implement the three processors =====
public class BankTransferProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Bank Transfer] Processed {transaction.Amount:C} for {transaction.Category}");
    }
}

public class MobileMoneyProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Mobile Money] Processed {transaction.Amount:C} for {transaction.Category}");
    }
}

public class CryptoWalletProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Crypto Wallet] Processed {transaction.Amount:C} for {transaction.Category}");
    }
}

// ===== Step d: Create Account base class =====
public class Account
{
    public string AccountNumber { get; private set; }
    public decimal Balance { get; protected set; }

    public Account(string accountNumber, decimal initialBalance)
    {
        AccountNumber = accountNumber;
        Balance = initialBalance;
    }

    public virtual void ApplyTransaction(Transaction transaction)
    {
        Balance -= transaction.Amount;
    }
}

// ===== Step e: Sealed SavingsAccount class =====
public sealed class SavingsAccount : Account
{
    public SavingsAccount(string accountNumber, decimal initialBalance)
        : base(accountNumber, initialBalance) { }

    public override void ApplyTransaction(Transaction transaction)
    {
        if (transaction.Amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
        }
        else
        {
            Balance -= transaction.Amount;
            Console.WriteLine($"Transaction applied. New balance: {Balance:C}");
        }
    }
}

// ===== Step f: FinanceApp class =====
public class FinanceApp
{
    private List<Transaction> _transactions = new();

    public void Run()
    {
        // i. Instantiate a SavingsAccount
        SavingsAccount account = new("ACC12345", 1000m);

        // ii. Create three sample transactions
        Transaction t1 = new(1, DateTime.Now, 150m, "Groceries");
        Transaction t2 = new(2, DateTime.Now, 300m, "Utilities");
        Transaction t3 = new(3, DateTime.Now, 120m, "Entertainment");

        // iii. Process transactions
        new MobileMoneyProcessor().Process(t1);
        new BankTransferProcessor().Process(t2);
        new CryptoWalletProcessor().Process(t3);

        // iv. Apply transactions to account
        account.ApplyTransaction(t1);
        account.ApplyTransaction(t2);
        account.ApplyTransaction(t3);

        // v. Add all transactions to _transactions
        _transactions.AddRange(new[] { t1, t2, t3 });
    }
}

// ===== Main entry point =====
public class Program
{
    public static void Main()
    {
        FinanceApp app = new();
        app.Run();
    }
}
