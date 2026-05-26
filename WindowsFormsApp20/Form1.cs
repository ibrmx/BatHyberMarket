using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Windows.Forms;

namespace WindowsFormsApp20
{
    public partial class Form1 : Form
    {
        string connStr = "Data Source=Bathypermarket.db";

        public Form1()
        {
            InitializeComponent();
            CreateTable();
            ImportFromCsvIfEmpty();
            LoadGrid();
        }

        // ===== CREATE TABLE =====
        private void CreateTable()
        {
            using (var con = new SqliteConnection(connStr))
            {
                con.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS products (
                    productid INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductName TEXT,
                    Category TEXT,
                    UnitPrice REAL,
                    StockQuant INTEGER)";
                new SqliteCommand(sql, con).ExecuteNonQuery();
            }
        }

        // ===== IMPORT CSV (runs once if table is empty) =====
        private void ImportFromCsvIfEmpty()
        {
            string csvPath = @"C:\Users\Brian\Documents\Products.csv";
            if (!System.IO.File.Exists(csvPath)) return;

            using (var con = new SqliteConnection(connStr))
            {
                con.Open();
                long count = (long)new SqliteCommand("SELECT COUNT(*) FROM products", con).ExecuteScalar();
                if (count > 0) return;

                foreach (string line in System.IO.File.ReadAllLines(csvPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] f = ParseCsvLine(line);
                    if (f.Length < 5) continue;

                    string price = f[3].Replace("$", "").Trim();
                    var cmd = new SqliteCommand(
                        "INSERT INTO products (productid, ProductName, Category, UnitPrice, StockQuant) VALUES (@id, @name, @cat, @price, @qty)",
                        con);
                    cmd.Parameters.AddWithValue("@id",    int.Parse(f[0]));
                    cmd.Parameters.AddWithValue("@name",  f[1]);
                    cmd.Parameters.AddWithValue("@cat",   f[2]);
                    cmd.Parameters.AddWithValue("@price", double.Parse(price));
                    cmd.Parameters.AddWithValue("@qty",   int.Parse(f[4]));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var fields = new System.Collections.Generic.List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            foreach (char c in line)
            {
                if (c == '"') { inQuotes = !inQuotes; }
                else if (c == ',' && !inQuotes) { fields.Add(current.ToString()); current.Clear(); }
                else { current.Append(c); }
            }
            fields.Add(current.ToString());
            return fields.ToArray();
        }

        // ===== LOAD GRID =====
        private void LoadGrid()
        {
            using (var con = new SqliteConnection(connStr))
            {
                con.Open();
                var cmd = new SqliteCommand("SELECT * FROM products", con);
                var reader = cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                dataGridView1.DataSource = dt;
            }
        }

        // ===== INSERT =====
        private void button1_Click(object sender, EventArgs e)
        {
            using (var con = new SqliteConnection(connStr))
            {
                con.Open();
                string sql = @"INSERT INTO products (ProductName, Category, UnitPrice, StockQuant)
                               VALUES (@name, @cat, @price, @qty)";
                var cmd = new SqliteCommand(sql, con);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@cat", cmbCategory.Text);
                cmd.Parameters.AddWithValue("@price", txtPrice.Text);
                cmd.Parameters.AddWithValue("@qty", txtQuantity.Text);
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Product Added Successfully!");
            LoadGrid();
            Clear();
        }

        // ===== UPDATE =====
        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            using (var con = new SqliteConnection(connStr))
            {
                con.Open();
                string sql = @"UPDATE products SET
                               ProductName=@name, Category=@cat,
                               UnitPrice=@price, StockQuant=@qty
                               WHERE productid=@id";
                var cmd = new SqliteCommand(sql, con);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@cat", cmbCategory.Text);
                cmd.Parameters.AddWithValue("@price", txtPrice.Text);
                cmd.Parameters.AddWithValue("@qty", txtQuantity.Text);
                cmd.Parameters.AddWithValue("@id", txtid.Text);
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Product Updated Successfully!");
            LoadGrid();
        }

        // ===== DELETE =====
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            using (var con = new SqliteConnection(connStr))
            {
                con.Open();
                string sql = "DELETE FROM products WHERE productid=@id";
                var cmd = new SqliteCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", txtid.Text);
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Product Deleted Successfully!");
            LoadGrid();
            Clear();
        }

        // ===== CLEAR =====
        private void Clear()
        {
            txtid.Text = "";
            txtName.Text = "";
            cmbCategory.Text = "";
            txtPrice.Text = "";
            txtQuantity.Text = "";
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        // ===== EXIT =====
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // ===== CELL CLICK =====
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridView1.CurrentRow;
            txtid.Text = row.Cells["productid"].Value.ToString();
            txtName.Text = row.Cells["ProductName"].Value.ToString();
            cmbCategory.Text = row.Cells["Category"].Value.ToString();
            txtPrice.Text = row.Cells["UnitPrice"].Value.ToString();
            txtQuantity.Text = row.Cells["StockQuant"].Value.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Global variables in the form
        DataTable dt = new DataTable();
        int currentIndex = 0;

        // Helper method to display a record by index
        private void DisplayRecord(int index)
        {
            if (dt.Rows.Count == 0) return;

            txtid.Text = dt.Rows[index]["productid"].ToString();
            txtName.Text = dt.Rows[index]["ProductName"].ToString();
            txtPrice.Text = dt.Rows[index]["UnitPrice"].ToString();
            cmbCategory.Text = dt.Rows[index]["Category"].ToString();
            txtQuantity.Text = dt.Rows[index]["StockQuant"].ToString();
        }
        // ===== FIRST =====
        private void button5_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;
            currentIndex = 0;
            DisplayRecord(currentIndex);
        }
        // ===== PREV =====
        private void button6_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;

            if (currentIndex > 0)
            {
                currentIndex--;
                DisplayRecord(currentIndex);
            }
            else
            {
                MessageBox.Show("This is the first record.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        // ===== NEXT =====
        private void button7_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;

            if (currentIndex < dt.Rows.Count - 1)
            {
                currentIndex++;
                DisplayRecord(currentIndex);
            }
            else
            {
                MessageBox.Show("This is the last record.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        // ===== LAST =====
        private void button8_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;
            currentIndex = dt.Rows.Count - 1;
            DisplayRecord(currentIndex);
        }
    }
}
