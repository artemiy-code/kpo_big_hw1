using System.Diagnostics;

namespace hseBank
{
    public enum CategoryType { Income, Expense }

    public class BankAccount
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public decimal Balance { get; private set; }
        public List<Operation> Operations { get; private set; }

        public BankAccount(string name, decimal initialBalance)
        {
            Id = Guid.NewGuid();
            Name = name;
            Balance = initialBalance;
            Operations = new List<Operation>();
        }

        public void UpdateBalance(decimal amount)
        {
            Balance += amount;
        }

        public void AddOperation(Operation operation)
        {
            Operations.Add(operation);
            if (operation.Type == CategoryType.Income)
            {
                UpdateBalance(operation.Amount);
            }
            else
            {
                UpdateBalance(-operation.Amount);
            }
        }

        public void RecalculateBalance()
        {
            decimal newBalance = 0;
            foreach (var op in Operations)
            {
                if (op.Type == CategoryType.Income)
                {
                    newBalance += op.Amount;
                }
                else
                {
                    newBalance -= op.Amount;
                }
            }
            Balance = newBalance;
        }
    }

    public class Category
    {
        public Guid Id { get; private set; }
        public CategoryType Type { get; private set; }
        public string Name { get; private set; }

        public Category(string name, CategoryType type)
        {
            Id = Guid.NewGuid();
            Name = name;
            Type = type;
        }
    }

    public class Operation
    {
        public Guid Id { get; private set; }
        public CategoryType Type { get; private set; }
        public BankAccount Account { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Description { get; private set; }
        public Category Category { get; private set; }

        public Operation(CategoryType type, BankAccount account, decimal amount, Category category, string description = "")
        {
            Id = Guid.NewGuid();
            Type = type;
            Account = account;
            Amount = amount;
            Category = category;
            Description = description;
            Date = DateTime.Now;
        }
    }

    public class FinanceFactory
    {
        public static BankAccount CreateBankAccount(string name, decimal initialBalance)
        {
            return new BankAccount(name, initialBalance);
        }

        public static Category CreateCategory(string name, CategoryType type)
        {
            return new Category(name, type);
        }

        public static Operation CreateOperation(CategoryType type, BankAccount account, decimal amount, Category category, string description = "")
        {
            if (type == CategoryType.Expense && account.Balance < amount)
                throw new InvalidOperationException("Недостаточно средств для выполнения операции.");

            var operation = new Operation(type, account, amount, category, description);
            account.AddOperation(operation);
            return operation;
        }
    }

    public class FinanceAnalytics
    {
        public static decimal GetIncomeExpenseDifference(BankAccount account)
        {
            decimal income = 0;
            decimal expense = 0;

            foreach (var op in account.Operations)
            {
                if (op.Type == CategoryType.Income)
                {
                    income += op.Amount;
                }
                else
                {
                    expense += op.Amount;
                }
            }

            return income - expense;
        }

        public static Dictionary<string, decimal> GetGroupedExpenses(BankAccount account)
        {
            return account.Operations
                .Where(op => op.Type == CategoryType.Expense)
                .GroupBy(op => op.Category.Name)
                .ToDictionary(g => g.Key, g => g.Sum(op => op.Amount));
        }
    }

    public class ExecutionTimer
    {
        public static void MeasureExecutionTime(Action method, string description)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            method();
            stopwatch.Stop();
            Console.WriteLine($"{description} выполнено за {stopwatch.ElapsedMilliseconds} мс.");
        }
    }

    class Program
    {
        static void Main()
        {
            var account = FinanceFactory.CreateBankAccount("Основной счет: Артем", 12000);

            var salaryCategory = FinanceFactory.CreateCategory("Зарплата", CategoryType.Income);
            var foodCategory = FinanceFactory.CreateCategory("Еда", CategoryType.Expense);
            var creditCategory = FinanceFactory.CreateCategory("Ипотека", CategoryType.Expense);

            ExecutionTimer.MeasureExecutionTime(() =>
            {
                var income1 = FinanceFactory.CreateOperation(CategoryType.Income, account, 30000, salaryCategory, "Аванс за февраль");
                var income2 = FinanceFactory.CreateOperation(CategoryType.Income, account, 30000, salaryCategory, "Зарплата за февраль");
                var expense1 = FinanceFactory.CreateOperation(CategoryType.Expense, account, 20000, foodCategory, "Покупка продуктов");
                var expense2 = FinanceFactory.CreateOperation(CategoryType.Expense, account, 50000, creditCategory, "Платеж по ипотеке");
            }, "Добавление операций");

            Console.WriteLine($"Счет: {account.Name}, Баланс: {account.Balance} руб.");

            Console.WriteLine($"Прибыль по карте: {FinanceAnalytics.GetIncomeExpenseDifference(account)} руб.");

            var groupedExpenses = FinanceAnalytics.GetGroupedExpenses(account);
            Console.WriteLine("Расходы по категориям:");
            foreach (var expense in groupedExpenses)
            {
                Console.WriteLine($"{expense.Key}: {expense.Value} руб.");
            }
        }
    }
}