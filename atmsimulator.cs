using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;


//holds the details of a bank account
class Account
{
    public string Number = "";         //account number, used to log in
    public string Pin = "";            //PIN, used to log in
    public decimal Balance;            //money currently in the account
}

//entry point, starts the program and opens the ATM window
class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new AtmForm());
    }
}

// ATM window, holds the screen, the buttons, and all the logic for the ATM
class AtmForm : Form
{
    //file where accounts are saved, each line represents an account
    const string FilePath = "accounts.txt";   // line format: number,pin,balance
    List<Account> accounts = new();           //all accounts, loaded from the file
    Account? current;                         //the account currently logged in, null if no one is logged in

//input boxes
    TextBox txtAcct = new() { Left = 100, Top = 20, Width = 150 };

    //shows dots instead of the actual characters typed, for security
    TextBox txtPin = new() { Left = 100, Top = 55, Width = 150, UseSystemPasswordChar = true };
    TextBox txtAmount = new() { Left = 100, Top = 20, Width = 150 };

    //label to show messages (balance, errors, etc.)
    Label lblMsg = new() { Left = 20, Top = 190, Width = 260 };

    //two panels, one for the login screen and one for the menu screen
    Panel loginPanel = new() { Left = 0, Top = 0, Width = 300, Height = 180 };
    Panel menuPanel = new() { Left = 0, Top = 0, Width = 300, Height = 180, Visible = false };

    public AtmForm()
    {
        Text = "ATM";
        Width = 320;
        Height = 270;

        //read saved accounts from the file when the program starts
        LoadAccounts();

        // Login screen
        var btnLogin = new Button { Text = "Login", Left = 100, Top = 95 };
        btnLogin.Click += (s, e) =>
        {
            //look for an account where the number and PIN match what the user entered 
            current = accounts.Find(a => a.Number == txtAcct.Text && a.Pin == txtPin.Text);
            if (current == null) { lblMsg.Text = "Invalid account or PIN."; return; }

            //login successful, switch to the menu screen
            loginPanel.Visible = false;
            menuPanel.Visible = true;
            lblMsg.Text = "Welcome.";
        };

        //add the controls to the login panel
        loginPanel.Controls.AddRange(new Control[] {
            new Label { Text = "Account", Left = 20, Top = 23, AutoSize = true }, txtAcct,
            new Label { Text = "PIN", Left = 20, Top = 58, AutoSize = true }, txtPin, btnLogin });

        // Menu screen
        var btnBal = new Button { Text = "Balance", Left = 20, Top = 60 };
        var btnDep = new Button { Text = "Deposit", Left = 105, Top = 60 };
        var btnWd = new Button { Text = "Withdraw", Left = 190, Top = 60 };
        var btnOut = new Button { Text = "Logout", Left = 100, Top = 110 };

        //balance button shows the current balance of the logged-in account
        btnBal.Click += (s, e) => lblMsg.Text = $"Balance: {current!.Balance:F2}";

        //deposit button adds money to the logged-in account
        btnDep.Click += (s, e) =>
        {
            if (!TryAmount(out var amt)) return;   //stop if amount is invalid
            current!.Balance += amt;
            SaveAccounts();
            lblMsg.Text = "Deposit successful.";
        };

        //withdraw button removes money from the logged-in account
        btnWd.Click += (s, e) =>
        {
            if (!TryAmount(out var amt)) return;
            if (amt > current!.Balance) { lblMsg.Text = "Insufficient funds."; return; }
            current.Balance -= amt;
            SaveAccounts();
            lblMsg.Text = "Please take your cash.";
        };

        //logout button returns to the login screen
        btnOut.Click += (s, e) =>
        {
            current = null;
            txtAcct.Text = txtPin.Text = txtAmount.Text = "";
            menuPanel.Visible = false;
            loginPanel.Visible = true;
            lblMsg.Text = "";
        };

        //add everything to the menu panel
        menuPanel.Controls.AddRange(new Control[] {
            new Label { Text = "Amount", Left = 20, Top = 23, AutoSize = true }, txtAmount,
            btnBal, btnDep, btnWd, btnOut });

        //add both screens and the message label to the window
        Controls.AddRange(new Control[] { loginPanel, menuPanel, lblMsg });
    }

//check if the amount entered is a valid number greater than 0
    bool TryAmount(out decimal amt)
    {
        if (decimal.TryParse(txtAmount.Text, out amt) && amt > 0) return true;
        lblMsg.Text = "Enter a valid amount.";
        return false;
    }

    // Read data back from the file
    void LoadAccounts()
    {
        //if the file doesn't exist, create it with some default accounts(demo file)
        if (!File.Exists(FilePath))
        {
            accounts.Add(new Account { Number = "1001", Pin = "1234", Balance = 5000 });
            accounts.Add(new Account { Number = "1002", Pin = "4321", Balance = 1200 });
            SaveAccounts();
            return;
        }
        //read each line, split it by commas, 
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