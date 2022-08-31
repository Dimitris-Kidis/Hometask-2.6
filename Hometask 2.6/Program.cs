using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


// 1. Connect DB to VS ✅
// 2. Create two tables using using SqlCommand ✅
// 3. Create DataAdapter adn define SELECT, UPDATE, INSERT, DELETE commands. ✅
// 4. Create DataTable, insert new rows and push changes to db through adapter 
// 5. Manipulate data from DataTable so that DataAdapter will use all 4 commands.



namespace App
{
    class Program
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["sqlconnection"].ConnectionString;

        public static DataTable customerTable;
        public static SqlDataAdapter adapter;
        public static void Main(string[] args)
        {
            CreatingTables();
            DataAdapterCommandsWork();
        }

        public static void CreatingTables()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string firstTable =
                    @"CREATE TABLE Person (
                    Id INT PRIMARY KEY,
                    Name NVARCHAR(50) NOT NULL,
                    Age INT NOT NULL CHECK(Age > 0)
                    )";
                string secondTable =
                    @"CREATE TABLE Car (
                    PersonId INT FOREIGN KEY REFERENCES Person(Id),
                    BrandName NVARCHAR(50) NOT NULL,
                    Price DECIMAL(10, 2) NOT NULL
                    )";
                using (var sqlCommand = new SqlCommand(firstTable, connection))
                {
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine("First Table Created Successfully");
                }
                using (var sqlCommand = new SqlCommand(secondTable, connection))
                {
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine("Second Table Created Successfully");
                }
            }
        }

        public static void DataAdapterCommandsWork()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter();
                // Insert
                var insertRows = 
                    @"INSERT INTO Person (Id, Name, Age) VALUES (0, 'Nick', 28);" +
                    "\nINSERT INTO Person (Id, Name, Age) VALUES (1, 'Mia', 18);" +
                    "\nINSERT INTO Person (Id, Name, Age) VALUES (2, 'Selena', 23);" +
                    "\nINSERT INTO Person (Id, Name, Age) VALUES (3, 'Jordan', 34);" +
                    "\nINSERT INTO Person (Id, Name, Age) VALUES (4, 'Mike', 43);";
                using (var sqlCommand = new SqlCommand(insertRows, connection))
                {
                    adapter.InsertCommand = new SqlCommand(insertRows, connection);
                    adapter.InsertCommand.ExecuteNonQuery();
                    Console.WriteLine("Rows Inserted");
                }

                // Select
                var selectQuery = @"SELECT * FROM Person WHERE Age BETWEEN 20 AND 30";
                using (var sqlCommand = new SqlCommand(selectQuery, connection))
                {
                    adapter.SelectCommand = new SqlCommand(selectQuery, connection);
                    adapter.SelectCommand.ExecuteNonQuery();
                    Console.WriteLine("People Selected");
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read()) Console.WriteLine(reader["Id"] + ": " + reader["Name"] + ". Age: " + reader["Age"]);
                    }
                }

                // Update
                var updateQuery = @"UPDATE Person SET Age = Age * 2 WHERE Age BETWEEN 20 AND 30";
                using (var sqlCommand = new SqlCommand(updateQuery, connection))
                {
                    adapter.UpdateCommand = new SqlCommand(updateQuery, connection);
                    adapter.UpdateCommand.ExecuteNonQuery();
                    Console.WriteLine("People Updated");
                }

                // Delete
                var deleteQuery = @"DELETE FROM Person WHERE Age > 40";
                using (var sqlCommand = new SqlCommand(deleteQuery, connection))
                {
                    adapter.DeleteCommand = new SqlCommand(deleteQuery, connection);
                    adapter.DeleteCommand.ExecuteNonQuery();
                    Console.WriteLine("People Deleted");
                }
            }

        }
    }
}