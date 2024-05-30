using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace StoreInventoryApp
{
    class Program
    {
        static string connectionString; // Declare connectionString as static

        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            connectionString = configuration.GetConnectionString("DefaultConnection"); // Initialize connectionString

            Console.WriteLine("Store Items");

            // Display existing items
            DisplayItems();

            // Loop to add new items
            while (true)
            {
                AddNewItem();

                Console.Write("Do you want to add more items? (yes/no): ");
                string response = Console.ReadLine().ToLower();

                if (response == "no")
                {
                    break;
                }
            }

            // Ask for username
            Console.Write("Enter your name: ");
            string username = Console.ReadLine();

            // Send email
            SendMail("#####", GenerateEmailBody(username), "Test Mail");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Method to display items from the database
        static void DisplayItems()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT itemCode, description, qty, price, amount FROM items";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("Item Code\tDescription\t\tQuantity\tPrice\t\tAmount");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["itemCode"],-10}\t{reader["description"],-20}\t{reader["qty"],-8}\t{Convert.ToDouble(reader["price"]).ToString("0.00"),10}\t{Convert.ToDouble(reader["amount"]).ToString("0.00"),10}");
                    }
                }
            }
        }

        // Method to add a new item to the database
        static void AddNewItem()
        {
            Console.WriteLine("\nAdd New Item");
            Console.Write("Enter Item Code: ");
            string itemCode = Console.ReadLine();
            Console.Write("Enter Description: ");
            string description = Console.ReadLine();
            Console.Write("Enter Quantity: ");
            int qty = int.Parse(Console.ReadLine());
            Console.Write("Enter Price: ");
            double price = double.Parse(Console.ReadLine());
            double amount = qty * price;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO items (itemCode, description, qty, price, amount) " +
                               "VALUES (@itemCode, @description, @qty, @price, @amount)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@itemCode", itemCode);
                command.Parameters.AddWithValue("@description", description);
                command.Parameters.AddWithValue("@qty", qty);
                command.Parameters.AddWithValue("@price", price.ToString("0.00")); // Ensure price has two decimal places
                command.Parameters.AddWithValue("@amount", amount.ToString("0.00")); // Ensure amount has two decimal places

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} row(s) affected.");
            }
        }

        // Method to generate email body from items in the database
        static string GenerateEmailBody(string username)
        {
            // Get current date and time
            string currentDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            // Variables to store subtotal, VAT, and total
            double subtotal = 0;
            double vat = 0;
            double total = 0;

            string emailBody = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Receipt</title>
                <style>
                    /* Receipt Styling */
                    body {{
                        font-family: sans-serif;
                        margin: 0;
                        padding: 2rem;
                    }}
                    .receipt-container {{
                        border: 1px solid #ddd;
                        border-radius: 5px;
                        padding: 2rem;
                        width: 600px;
                    }}
                    .receipt-header {{
                        text-align: center;
                        margin-bottom: 2rem;
                    }}
                    .receipt-header h1 {{
                        font-size: 1.5rem;
                        margin: 0;
                    }}
                    .receipt-info-container {{
                        border: 2px solid #c7e5fc;
                        border-radius: 5px;
                        padding: 1rem;
                        margin-bottom: 2rem;
                    }}
                    .receipt-info {{
                        display: flex;
                        justify-content: space-between;
                    }}
                    .receipt-info p {{
                        margin: 0;
                    }}
                    .receipt-table-container {{
                        border: 1px solid #ddd;
                        border-radius: 5px;
                        padding: 1rem;
                    }}
                    .receipt-table {{
                        width: 100%;
                        border-collapse: separate;
                        border-spacing: 0 1rem;
                    }}
                    .receipt-table thead th {{
                        border-bottom: 1px solid #ddd;
                        padding: 1rem 0.5rem;
                        text-align: left;
                        background-color: #c0c3c3;
                    }}
                    .receipt-table tbody tr td {{
                        padding: 0.5rem 1rem;
                        border: 1px solid white;
                        border-radius: 5px;
                        background-color: white;
                        margin: 5px; /* Add margin to create a gap */
                    }}
                    .receipt-table tfoot tr:last-child td {{
                        border-bottom: none;
                        font-weight: bold;
                    }}
                </style>
            </head>
            <body>
                <div class='receipt-container'>
                    <div class='receipt-header'>
                        <h1>Test Invoice</h1>
                    </div>
                    <div class='receipt-info-container'>
                        <div class='receipt-info'>
                            <p>
                                Username: {username}<br>
                                Bill Date: {currentDate}<br>
                                Billed Time: {currentTime}
                            </p>
                        </div>
                    </div>
                    <div class='receipt-table-container'>
                        <table class='receipt-table'>
                            <thead>
                                <tr>
                                    <th>Item Code</th>
                                    <th>Description</th>
                                    <th>Quantity</th>
                                    <th>Price</th>
                                    <th>Amount</th>
                                </tr>
                            </thead>
                            <tbody>";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT itemCode, description, qty, price, amount FROM items";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        emailBody += "<tr>";
                        emailBody += $"<td>{reader["itemCode"]}</td>";
                        emailBody += $"<td>{reader["description"]}</td>";
                        emailBody += $"<td>{reader["qty"]}</td>";
                        emailBody += $"<td>{Convert.ToDouble(reader["price"]).ToString("0.00")}</td>";
                        emailBody += $"<td>{Convert.ToDouble(reader["amount"]).ToString("0.00")}</td>";
                        emailBody += "</tr>";

                        // Increment subtotal
                        subtotal += Convert.ToDouble(reader["amount"]);
                    }
                }

                // Calculate VAT and total
                vat = subtotal * 0.15;
                total = subtotal + vat;
            }

            emailBody += @"
                            </tbody>
                            <tfoot>
                                <tr>
                                    <th colspan='4'>Subtotal</th>
                                    <td id='subtotal'>" + subtotal.ToString("0.00") + @"</td>
                                </tr>
                                <tr>
                                    <th colspan='4'>VAT (15%)</th>
                                    <td id='vat'>" + vat.ToString("0.00") + @"</td>
                                </tr>
                                <tr>
                                    <th colspan='4'>Total</th>
                                    <td id='total'>" + total.ToString("0.00") + @"</td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>
            </body>
            </html>";

            return emailBody;
        }

        // Method to send email
        private static string SendMail(string recipient, string mailBody, string subject)
        {
            try
            {
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress("##");
                    mailMessage.Subject = subject;
                    mailMessage.Body = mailBody;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(recipient));

                    using (SmtpClient smtp = new SmtpClient("#", ))
                    {
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = true;

                        Console.WriteLine("smtp.Send...");

                        smtp.Credentials = new System.Net.NetworkCredential("#");
                        smtp.Send(mailMessage);
                    }
                }

                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
