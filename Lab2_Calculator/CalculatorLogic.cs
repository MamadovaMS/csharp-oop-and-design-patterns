using System;

namespace Lab2_Calculator
{
    // 1. ИНТЕРФЕЙС КОМАНДЫ
    public interface ICommand
    {
        void Execute();
    }

    // 2. ПОЛУЧАТЕЛЬ (Движок)
    public class CalculatorEngine
    {
        public string CurrentInput { get; set; } = "";
        public double Memory { get; set; } = 0;
        public double Result { get; set; } = 0;

        public void AddDigit(string digit) { CurrentInput += digit; }
        public void Clear() { CurrentInput = ""; Result = 0; }
        public void SaveToMemory() { Memory = double.TryParse(CurrentInput, out double val) ? val : 0; }
        public void LoadFromMemory() { CurrentInput = Memory.ToString(); }

        public void Calculate(string op)
        {
            double current = double.TryParse(CurrentInput, out double val) ? val : 0;
            if (op == "+") Result += current;
            if (op == "-") Result -= current;
            if (op == "*") Result *= current;
            if (op == "/") Result = current != 0 ? Result / current : 0;
            CurrentInput = Result.ToString();
        }
    }

    // 3. КОНКРЕТНЫЕ КОМАНДЫ
    public class DigitCommand : ICommand
    {
        private CalculatorEngine _engine;
        private string _digit;
        public DigitCommand(CalculatorEngine engine, string digit) { _engine = engine; _digit = digit; }
        public void Execute() { _engine.AddDigit(_digit); }
    }

    public class ClearCommand : ICommand
    {
        private CalculatorEngine _engine;
        public ClearCommand(CalculatorEngine engine) { _engine = engine; }
        public void Execute() { _engine.Clear(); }
    }

    public class MemoryAddCommand : ICommand
    {
        private CalculatorEngine _engine;
        public MemoryAddCommand(CalculatorEngine engine) { _engine = engine; }
        public void Execute() { _engine.SaveToMemory(); }
    }

    public class MemoryRecallCommand : ICommand
    {
        private CalculatorEngine _engine;
        public MemoryRecallCommand(CalculatorEngine engine) { _engine = engine; }
        public void Execute() { _engine.LoadFromMemory(); }
    }
}