using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


// 1. Connect DB to VS ✅
// 2. Create two tables using using SqlCommand ✅
// 3. Create DataAdapter and define SELECT, UPDATE, INSERT, DELETE commands. ✅
// 4. Create DataTable, insert new rows and push changes to db through adapter. ✅
// 5. Manipulate data from DataTable so that DataAdapter will use all 4 commands. ✅



namespace App
{
    class Program
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["sqlconnection"].ConnectionString;

        public static DataTable personTable;
        public static SqlDataAdapter adapter;
        public static void Main(string[] args)
        {
            CreatingTables();
            InsertSomeData();

            personTable = GetPersonTable();
            adapter = GetPersonAdapter();
            adapter.Fill(personTable);


            InsertNewRowInPersonTable(new Person(6, "John", 35));
            UpdateRowInPersonTableById(new Person(99, "99", 99), 1);
            DeleteRowInPersonTableById(6);
            SelectInPersonTableById(3);

        }

        public static void CreatingTables()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string dropping = "DROP TABLE IF EXISTS Car;\nDROP TABLE IF EXISTS Person;";
                using (var sqlCommand = new SqlCommand(dropping, connection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
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

        public static void InsertSomeData()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter();
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

            }

        }

        public static DataTable GetPersonTable()
        {
            DataTable dt = new DataTable("Person");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Age", typeof(int));
            return dt;
        }

        public static SqlDataAdapter GetPersonAdapter()
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Person;", connectionString);

            // select
            //adapter.SelectCommand = new SqlCommand("SELECT Id, Name, Age FROM Person WHERE Id = @Id");
            //SqlParameter selParam = adapter.SelectCommand.Parameters.Add("@Id", SqlDbType.Int);
            //    selParam.Value = 1;
            //selParam.SourceColumn = "Id";
            //selParam.SourceVersion = DataRowVersion.Original;

            // insert
            adapter.InsertCommand = new SqlCommand("INSERT INTO Person(Id, Name, Age) VALUES (@Id, @Name, @Age)");
            adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 255, "Id");
            adapter.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50, "Name");
            adapter.InsertCommand.Parameters.Add("@Age", SqlDbType.Int, 255, "Age");

            // update
            adapter.UpdateCommand = new SqlCommand("UPDATE Person SET Id = @Id, Name = @Name, Age = @Age WHERE Id = @Id2");
            adapter.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 255, "Id");
            adapter.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50, "Name");
            adapter.UpdateCommand.Parameters.Add("@Age", SqlDbType.Int, 255, "Age");
            SqlParameter updParam = adapter.UpdateCommand.Parameters.Add("@Id2", SqlDbType.Int);
            updParam.SourceColumn = "Id";
            updParam.SourceVersion = DataRowVersion.Original;

            // delete
            adapter.DeleteCommand = new SqlCommand("DELETE FROM Person WHERE Id = @Id");
            SqlParameter delParam = adapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int);
            delParam.SourceColumn = "Id";
            delParam.SourceVersion = DataRowVersion.Original;

            return adapter;
        }

        public static void InsertNewRowInPersonTable(Person person)
        {
            DataRow dr = personTable.NewRow();
            dr["Id"] = $"{person.Id}";
            dr["Name"] = $"{person.Name}";
            dr["Age"] = $"{person.Age}";
            personTable.Rows.Add(dr);

            using (var connection = new SqlConnection(connectionString))
            {
                adapter.InsertCommand.Connection = connection;
                adapter.Update(personTable);
            }
        }

        public static void UpdateRowInPersonTableById(Person person, int id)
        {
            foreach (DataRow row in personTable.Rows)
            {
                if ((int)row["id"] == id)
                {
                    row["Id"] = person.Id;
                    row["Name"] = person.Name;
                    row["Age"] = person.Age;
                }
            }

            using (var connection = new SqlConnection(connectionString))
            {
                adapter.UpdateCommand.Connection = connection;
                adapter.Update(personTable);
            }
        }

        public static void DeleteRowInPersonTableById(int id)
        {
            for (int i = personTable.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = personTable.Rows[i];
                if ((int)dr["Id"] == id) dr.Delete();
            }

            using (var connection = new SqlConnection(connectionString))
            {
                adapter.DeleteCommand.Connection = connection;
                adapter.Update(personTable);
            }
        }

        public static void SelectInPersonTableById(int id)
        {
            for (int i = personTable.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = personTable.Rows[i];
                if ((int)dr["Id"] == id)
                {
                    Console.WriteLine("ID: " + dr["Id"] + ", Name: " + dr["Name"] + ", Age: " + dr["Age"]);
                }
            }
        }
    }

    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public Person(int id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
    }
}