using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab2_Calculator
{
    public partial class Form1 : Form
    {
        private CalculatorEngine engine = new CalculatorEngine();
        private TextBox txtDisplay;

        private Button btnCustom1, btnCustom2;
        private Button lastClickedCustom;

        private ICommand commandForCustom1;
        private ICommand commandForCustom2;

        public Form1()
        {
            this.Text = "Настраиваемый калькулятор (Паттерн Команда)";
            this.Size = new Size(400, 580);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- ДИСПЛЕЙ ---
            txtDisplay = new TextBox { Location = new Point(20, 20), Width = 340, Height = 40, Font = new Font("Arial", 16), ReadOnly = true };
            this.Controls.Add(txtDisplay);

            // --- ЦИФРОВЫЕ КНОПКИ (Жестко заданные) ---
            string[] digits = { "7", "8", "9", "4", "5", "6", "1", "2", "3", "0" };
            int x = 20, y = 80;
            foreach (var digit in digits)
            {
                Button btn = new Button { Text = digit, Location = new Point(x, y), Width = 70, Height = 50 };
                // Жесткая привязка команды ввода цифры
                btn.Click += (s, e) => { new DigitCommand(engine, digit).Execute(); UpdateDisplay(); };
                this.Controls.Add(btn);
                x += 80;
                if (x > 280) { x = 20; y += 60; }
            }

            // --- НАСТРАИВАЕМЫЕ КНОПКИ ---
            commandForCustom1 = new ClearCommand(engine);
            commandForCustom2 = new MemoryAddCommand(engine);

            btnCustom1 = new Button { Text = "Кнопка 1 (Clear)", Location = new Point(20, 380), Width = 150, Height = 50 };
            btnCustom2 = new Button { Text = "Кнопка 2 (Mem+)", Location = new Point(210, 380), Width = 150, Height = 50 };

            // При нажатии мы просто запоминаем кнопку и выполняем текущую команду
            btnCustom1.Click += (s, e) => { lastClickedCustom = btnCustom1; commandForCustom1.Execute(); UpdateDisplay(); };
            btnCustom2.Click += (s, e) => { lastClickedCustom = btnCustom2; commandForCustom2.Execute(); UpdateDisplay(); };
            this.Controls.Add(btnCustom1);
            this.Controls.Add(btnCustom2);

            // --- ИНТЕРФЕЙС НАСТРОЙКИ ---
            ComboBox cmbCommands = new ComboBox { Location = new Point(20, 450), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCommands.Items.AddRange(new string[] { "Очистить (Clear)", "В память (M+)", "Из памяти (MR)" });
            cmbCommands.SelectedIndex = 0;
            this.Controls.Add(cmbCommands);

            Button btnAssign = new Button { Text = "Назначить функцию", Location = new Point(230, 448), Width = 130 };
            btnAssign.Click += (s, e) =>
            {
                if (lastClickedCustom == null) { MessageBox.Show("Сначала нажмите на настраиваемую кнопку!"); return; }

                // Создаем выбранную команду
                ICommand newCommand = null;
                if (cmbCommands.SelectedIndex == 0) newCommand = new ClearCommand(engine);
                if (cmbCommands.SelectedIndex == 1) newCommand = new MemoryAddCommand(engine);
                if (cmbCommands.SelectedIndex == 2) newCommand = new MemoryRecallCommand(engine);

                // Переназначаем
                if (lastClickedCustom == btnCustom1) { commandForCustom1 = newCommand; btnCustom1.Text = cmbCommands.Text; }
                if (lastClickedCustom == btnCustom2) { commandForCustom2 = newCommand; btnCustom2.Text = cmbCommands.Text; }
                MessageBox.Show("Функция кнопки изменена!");
            };
            this.Controls.Add(btnAssign);
        }

        private void UpdateDisplay()
        {
            txtDisplay.Text = engine.CurrentInput;
        }

        // Пустой метод, чтобы дизайнер не ругался (теперь он ВНУТРИ класса)
        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}