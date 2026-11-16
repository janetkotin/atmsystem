using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

class Account
{
    public string Number = "";
    public string Pin = "";
    public decimal Balance;
}

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new AtmForm());
    }
}

class AtmForm : Form
{
    const string FilePath = "accounts.txt";   // line format: number,pin,balance
    List<Account> accounts = new();
    Account? current;

    TextBox txtAcct = new() { Left = 100, Top = 20, Width = 150 };
    TextBox txtPin = new() { Left = 100, Top = 55, Width = 150, UseSystemPasswordChar = true };
    TextBox txtAmount = new() { Left = 100, Top = 20, Width = 150 };
    Label lblMsg = new() { Left = 20, Top = 190, Width = 260 };
    Panel loginPanel = new() { Left = 0, Top = 0, Width = 300, Height = 180 };
    Panel menuPanel = new() { Left = 0, Top = 0, Width = 300, Height = 180, Visible = false };

    public AtmForm()
    {
        Text = "ATM";
        Width = 320;
        Height = 270;
        LoadAccounts();

        // Login screen
        var btnLogin = new Button { Text = "Login", Left = 100, Top = 95 };
        btnLogin.Click += (s, e) =>
        {
            current = accounts.Find(a => a.Number == txtAcct.Text && a.Pin == txtPin.Text);
            if (current == null) { lblMsg.Text = "Invalid account or PIN."; return; }
            loginPanel.Visible = false;
            menuPanel.Visible = true;
            lblMsg.Text = "Welcome.";
        };
        loginPanel.Controls.AddRange(new Control[] {
            new Label { Text = "Account", Left = 20, Top = 23, AutoSize = true }, txtAcct,
            new Label { Text = "PIN", Left = 20, Top = 58, AutoSize = true }, txtPin, btnLogin });

        // Menu screen
        var btnBal = new Button { Text = "Balance", Left = 20, Top = 60 };
        var btnDep = new Button { Text = "Deposit", Left = 105, Top = 60 };
        var btnWd = new Button { Text = "Withdraw", Left = 190, Top = 60 };
        var btnOut = new Button { Text = "Logout", Left = 100, Top = 110 };

        btnBal.Click += (s, e) => lblMsg.Text = $"Balance: {current!.Balance:F2}";

        btnDep.Click += (s, e) =>
        {
            if (!TryAmount(out var amt)) return;
            current!.Balance += amt;
            SaveAccounts();
            lblMsg.Text = "Deposit successful.";
        };

        btnWd.Click += (s, e) =>
        {
            if (!TryAmount(out var amt)) return;
            if (amt > current!.Balance) { lblMsg.Text = "Insufficient funds."; return; }
            current.Balance -= amt;
            SaveAccounts();
            lblMsg.Text = "Please take your cash.";
        };

        btnOut.Click += (s, e) =>
        {
            current = null;
            txtAcct.Text = txtPin.Text = txtAmount.Text = "";
            menuPanel.Visible = false;
            loginPanel.Visible = true;
            lblMsg.Text = "";
        };

        menuPanel.Controls.AddRange(new Control[] {
            new Label { Text = "Amount", Left = 20, Top = 23, AutoSize = true }, txtAmount,
            btnBal, btnDep, btnWd, btnOut });

        Controls.AddRange(new Control[] { loginPanel, menuPanel, lblMsg });
    }

    bool TryAmount(out decimal amt)
    {
        if (decimal.TryParse(txtAmount.Text, out amt) && amt > 0) return true;
        lblMsg.Text = "Enter a valid amount.";
        return false;
    }

    // Read data back from the file
    void LoadAccounts()
    {
        if (!File.Exists(FilePath))
        {
            accounts.Add(new Account { Number = "1001", Pin = "1234", Balance = 5000 });
            accounts.Add(new Account { Number = "1002", Pin = "4321", Balance = 1200 });
            SaveAccounts();
            return;
        }
        foreach (var line in File.ReadAllLines(FilePath))
        {
            var p = line.Split(',');
            accounts.Add(new Account { Number = p[0], Pin = p[1], Balance = decimal.Parse(p[2]) });
        }
    }

    // Save data to the file
    void SaveAccounts()
    {
        File.WriteAllLines(FilePath, accounts.Select(a => $"{a.Number},{a.Pin},{a.Balance}"));
    }
}