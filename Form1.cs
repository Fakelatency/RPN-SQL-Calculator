using System.Data;
using System;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Calculator_adop;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;



namespace Calculator_adop
{


    public partial class Form1 : Form
    {
        private string currentCalculation = "";

        private Rectangle buttonOriginalRectangle;
        private Rectangle smallbuttonOriginalRectangle;
        private Rectangle historyOriginalRectangle;
        private Rectangle originalFormSize;
        private Rectangle textOriginalRectangle;
        private System.Windows.Forms.Timer resizeTimer = new System.Windows.Forms.Timer();
        private AppDbContext dbContext;

        public Form1()
        {
            InitializeComponent();
            resizeTimer.Interval = 16;
            resizeTimer.Tick += OnResizeTimerTick;
            dbContext = new AppDbContext();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            originalFormSize = new Rectangle(this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height);
            buttonOriginalRectangle = new Rectangle(button0.Location.X, button0.Location.Y, button0.Width, button0.Height);
            smallbuttonOriginalRectangle = new Rectangle(button10.Location.X, button10.Location.Y, button10.Width, button10.Height);
            textOriginalRectangle = new Rectangle(textBoxOutput.Location.X, textBoxOutput.Location.Y, textBoxOutput.Width, textBoxOutput.Height);
            historyOriginalRectangle = new Rectangle(listView1.Location.X, listView1.Location.Y, listView1.Width, listView1.Height);
            listView1.View = View.Details;
            listView1.Columns.Add("History", -2); 
            listView1.SizeChanged += ListView1_SizeChanged;
            LoadCalculationHistory();
        }

        private void ListView1_SizeChanged(object sender, EventArgs e)
        {
            listView1.Columns[0].Width = listView1.Width - SystemInformation.VerticalScrollBarWidth;
        }

        private void button_Click(object sender, EventArgs e)
        {
            currentCalculation += (sender as Button).Text;
            textBoxOutput.Text = currentCalculation;
        }

        private void resizeControl(Rectangle r, Control c)
        {
            resizeTimer.Stop();
            float xRatio = (float)(this.Width) / (float)(originalFormSize.Width);
            float yRatio = (float)(this.Height) / (float)(originalFormSize.Height);

            int newWidth = (int)(r.Width * xRatio);
            int newHeight = (int)(r.Height * yRatio);

            c.Size = new Size(newWidth, newHeight);
        }

        private void LoadCalculationHistory()
        {
            var history = dbContext.CalculationResults.OrderByDescending(c => c.Id).ToList();

            foreach (var calculation in history)
            {
                ListViewItem item = new ListViewItem($"{calculation.InputExpression} = {calculation.Result ?? 0}");
                listView1.Items.Add(item);
            }
        }

        private void OnResizeTimerTick(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeControl(buttonOriginalRectangle, button0);
            resizeControl(buttonOriginalRectangle, button1);
            resizeControl(buttonOriginalRectangle, button2);
            resizeControl(buttonOriginalRectangle, button3);
            resizeControl(buttonOriginalRectangle, button4);
            resizeControl(buttonOriginalRectangle, button5);
            resizeControl(buttonOriginalRectangle, button6);
            resizeControl(buttonOriginalRectangle, button7);
            resizeControl(buttonOriginalRectangle, button8);
            resizeControl(buttonOriginalRectangle, button9);
            resizeControl(buttonOriginalRectangle, buttonAddition);
            resizeControl(buttonOriginalRectangle, buttonClear);
            resizeControl(buttonOriginalRectangle, buttonClearEntry);
            resizeControl(buttonOriginalRectangle, buttonDecimalPoint);
            resizeControl(buttonOriginalRectangle, buttonDivision);
            resizeControl(buttonOriginalRectangle, buttonEquals);
            resizeControl(buttonOriginalRectangle, buttonLeftBracket);
            resizeControl(buttonOriginalRectangle, buttonMultiplication);
            resizeControl(buttonOriginalRectangle, buttonRightBracket);
            resizeControl(buttonOriginalRectangle, buttonSubstraction);
            resizeControl(smallbuttonOriginalRectangle, button10);
            resizeControl(smallbuttonOriginalRectangle, button11);
            resizeControl(smallbuttonOriginalRectangle, button12);
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }
        private void tableLayoutPanel1_Resize(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeControl(textOriginalRectangle, textBoxOutput);
            resizeControl(historyOriginalRectangle, listView1);
        }


        private void button_Equals_Click(object sender, EventArgs e)
{
    try
    {
        if (!string.IsNullOrEmpty(currentCalculation))
        {
            string formattedCalculation = currentCalculation;
            string onpExpression = ConvertToONP(formattedCalculation);
            double result = CalculateONP(onpExpression);

            textBoxOutput.Text = result.ToString();
            currentCalculation = textBoxOutput.Text;

            try
            {
                var calculationResult = new CalculationResults
                {
                    InputExpression = formattedCalculation,
                    Result = result != null ? Convert.ToInt32(result) : (int?)null,
                };

                if (calculationResult.InputExpression != null && calculationResult.Result != null)
                {
                    dbContext.CalculationResults.Add(calculationResult);
                    dbContext.SaveChanges();

                    string displayString = $"{calculationResult.InputExpression} = {calculationResult.Result}";
                    ListViewItem item = new ListViewItem(displayString);
                    listView1.Items.Insert(0, item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Outer Exception: {ex.Message}");

                Exception innerException = ex.InnerException;
                while (innerException != null)
                {
                    Console.WriteLine($"Inner Exception: {innerException.Message}");
                    innerException = innerException.InnerException;
                }
            }
        }
        else
        {
            textBoxOutput.Text = "Error: Empty input";
        }
    }
    catch (Exception ex)
    {
        textBoxOutput.Text = $"Error: {ex.Message}";
        currentCalculation = "";
    }
}

        private string ConvertToONP(string input)
        {
            StringBuilder output = new StringBuilder();
            Stack<char> operators = new Stack<char>();
            StringBuilder currentNumber = new StringBuilder();

            foreach (char token in input)
            {
                if (char.IsDigit(token) || token == '.')
                {
                    currentNumber.Append(token);
                }
                else
                {
                    if (currentNumber.Length > 0)
                    {
                        output.Append(currentNumber.ToString()).Append(" ");
                        currentNumber.Clear();
                    }

                    if (token == '(')
                    {
                        operators.Push(token);
                    }
                    else if (token == ')')
                    {
                        while (operators.Count > 0 && operators.Peek() != '(')
                        {
                            output.Append(operators.Pop()).Append(" ");
                        }

                        if (operators.Count == 0)
                        {
                            throw new InvalidOperationException("Mismatched parentheses: Missing open parenthesis '('");
                        }

                        operators.Pop();
                    }
                    else if (IsOperator(token))
                    {
                        while (operators.Count > 0 && Priority(operators.Peek()) >= Priority(token))
                        {
                            output.Append(operators.Pop()).Append(" ");
                        }
                        operators.Push(token);
                    }
                }
            }

            if (currentNumber.Length > 0)
            {
                output.Append(currentNumber.ToString()).Append(" ");
            }

            while (operators.Count > 0)
            {
                if (operators.Peek() == '(')
                {
                    throw new InvalidOperationException("Mismatched parentheses: Missing closing parenthesis ')'");
                }
                output.Append(operators.Pop()).Append(" ");
            }

            Console.WriteLine($"Final Output: {output.ToString()}");
            return output.ToString();
        }

        private int Priority(char op)
        {
            switch (op)
            {
                case '+':
                case '-':
                    return 1;
                case '*':
                case '/':
                    return 2;
                default:
                    return 0;
            }
        }

        private double CalculateONP(string onpExpression)
        {
            Stack<double> stack = new Stack<double>();
            string[] tokens = onpExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else if (IsOperator(token[0]))
                {
                    if (stack.Count < 2)
                    {
                        throw new InvalidOperationException("Invalid expression: Not enough operands for the operator");
                    }

                    double operand2 = stack.Pop();
                    double operand1 = stack.Pop();
                    double result = PerformOperation(operand1, operand2, token[0]);
                    stack.Push(result);
                }
            }

            if (stack.Count == 1)
            {
                return stack.Pop();
            }
            else if (stack.Count > 1)
            {
                throw new InvalidOperationException("Invalid expression: Too many operands");
            }
            else
            {
                throw new InvalidOperationException("Invalid expression: No operands found");
            }
        }


        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/';
        }

        private double PerformOperation(double operand1, double operand2, char operation)
        {
            switch (operation)
            {
                case '+':
                    return operand1 + operand2;
                case '-':
                    return operand1 - operand2;
                case '*':
                    return operand1 * operand2;
                case '/':
                    if (operand2 != 0)
                    {
                        return operand1 / operand2;
                    }
                    else
                    {
                        throw new DivideByZeroException("Cannot divide by zero");
                    }
                default:
                    throw new ArgumentException("Invalid operator");
            }
        }
        private void button_Clear_Click(object sender, EventArgs e)
        {
            textBoxOutput.Text = "0";
            currentCalculation = "";
        }

        private void button_ClearEntry_Click(object sender, EventArgs e)
        {

            if (currentCalculation.Length > 0)
            {
                currentCalculation = currentCalculation.Remove(currentCalculation.Length - 1, 1);
            }
            textBoxOutput.Text = currentCalculation;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            var history = dbContext.CalculationResults.OrderBy(c => c.Id).ToList();

            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("InputExpression,Result");

            foreach (var calculation in history)
            {
                csvContent.AppendLine($"{calculation.InputExpression},{calculation.Result ?? 0}");
            }

            File.WriteAllText("CalculationHistory.csv", csvContent.ToString());

            MessageBox.Show("History exported to CalculationHistory.csv", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string inputFilePath = openFileDialog.FileName;
                    string outputFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), "Results.txt");

                    string[] calculations = File.ReadAllLines(inputFilePath);

                    using (StreamWriter outputFile = new StreamWriter(outputFilePath))
                    {
                        foreach (string calculation in calculations)
                        {
                            try
                            {
                                string onpExpression = ConvertToONP(calculation);
                                double result = CalculateONP(onpExpression);
                                outputFile.WriteLine($"Input: {calculation}, Result: {result}");
                            }
                            catch (Exception ex)
                            {
                                outputFile.WriteLine($"Input: {calculation}, Error: {ex.Message}");
                            }
                        }
                    }

                    MessageBox.Show($"Results written to {outputFilePath}", "Calculation Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the last item from history?", "Delete Last Item", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var lastItem = dbContext.CalculationResults.OrderByDescending(c => c.Id).FirstOrDefault();

                if (lastItem != null)
                {
                    dbContext.CalculationResults.Remove(lastItem);
                    dbContext.SaveChanges();

                    listView1.Items.RemoveAt(listView1.Items.Count - 1);

                    MessageBox.Show("Last item deleted successfully", "Delete Last Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No items to delete", "Delete Last Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    public class AppDbContext : DbContext
    {
        public DbSet<CalculationResults> CalculationResults { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculationResults>()  
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
            modelBuilder.Entity<CalculationResults>().HasKey(c => c.Id);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-0CB08JJ\\SQLEXPRESS;Database=calculatorSQL;User Id=admin;Password=admin;TrustServerCertificate=True;"); // konieczna zmiana na w³asn¹ baze danych (Express) 
        }
    }
    public class CalculationResults
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int Id { get; set; }

        public string? InputExpression { get; set; }
        public int? Result { get; set; }
    }
}
