using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Windows;

namespace Badger_Scheduler
{
    public partial class GenerateScheduleWindow : Window
    {
        private List<Employee> employees = new List<Employee>();
        private Dictionary<string, HashSet<int>> selectedEmployeesPerTable = new Dictionary<string, HashSet<int>>();
        private HashSet<int> siteEmployees = new HashSet<int>();
        public GenerateScheduleWindow()
        {
            InitializeComponent();
            GetTableName();
        }

        private void GetTableName()
        {
            // Get the connection string from the configuration file
            string connectionString = ConfigurationManager.ConnectionStrings["WeekSites"].ConnectionString;

            // Create a SqlConnection object with the connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Get the list of all table names in the database
                SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection);
                SqlDataReader reader = command.ExecuteReader();

                // Iterate through the list of table names and display them in a messagebox
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    GetSiteData(tableName);
                }

                // Close the reader and the connection
                reader.Close();
                connection.Close();
            }
        }

        private void GetSiteData(string tableName)
        {
            // Get the connection string from the configuration file
            string connectionString = ConfigurationManager.ConnectionStrings["WeekSites"].ConnectionString;


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand rowCountCommand = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", connection);
                int rowCount = (int)rowCountCommand.ExecuteScalar();
                SqlDataReader reader = rowCountCommand.ExecuteReader();
                reader.Close();

                SqlCommand siteDataCommand = new SqlCommand($"SELECT Supervisor FROM {tableName}", connection);
                SqlDataReader siteDataReader = siteDataCommand.ExecuteReader();

                while (siteDataReader.Read())
                {
                    string supervisorName = siteDataReader.GetString(0);
                    GetSupervisorId(supervisorName, tableName);
                }

                siteDataReader.Close();

                connection.Close();


            }

            // Create a SqlConnection object with the connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand object to retrieve the values from the database
                SqlCommand command = new SqlCommand($"SELECT Location, PeopleRequired, Priority, Supervisor FROM {tableName}", connection);

                // Execute the query and get the results in a SqlDataReader object
                SqlDataReader reader = command.ExecuteReader();

                // Iterate through the results and display the values in a messagebox
                while (reader.Read())
                {
                    string location = reader.GetString(0);
                    int peopleRequired = reader.GetInt32(1);
                    string priority = reader.GetString(2);
                    MessageBox.Show($"Table Name (Day): {tableName} \n Location: {location} \n PeopleRequired {peopleRequired}");
                    CalculateEmployeeWeight(priority, location, peopleRequired, tableName);
                }

                // Close the reader and the connection
                reader.Close();
                connection.Close();
            }

        }

        private void GetSupervisorId(string supervisorName, string tableName)
        {
            int supervisorId = 0;

            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand($"SELECT EmployeeID FROM Employees WHERE FirstName + ' ' + LastName = '{supervisorName}'", connection);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    supervisorId = reader.GetInt32(0);
                }

                reader.Close();
                connection.Close();
            }

            if (!selectedEmployeesPerTable.ContainsKey(tableName))
            {
                selectedEmployeesPerTable[tableName] = new HashSet<int>();
            }

            selectedEmployeesPerTable[tableName].Add(supervisorId);

            //MessageBox.Show(supervisorId.ToString() + ' ' + supervisorName);
        }

        private void CalculateEmployeeWeight(string priority, string location, int peopleRequired, string tableName)
        {

            string weight;
            int sum = 0;

            switch (priority)
            {
                case "Balanced":
                    weight = "overallRating";
                    break;
                case "Attendance":
                    weight = "attendanceRating";
                    break;
                default:
                    weight = "performanceRating";
                    break;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (weight != "overallRating")
                {
                    string query = $"SELECT {weight} FROM Employees";

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        sum += reader.GetInt32(0);
                    }

                    reader.Close();
                    connection.Close();
                }
                else
                {
                    string query = "SELECT AttendanceRating, PerformanceRating FROM Employees";

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int attendance = reader.GetInt32(0);
                        int performance = reader.GetInt32(1);

                        sum += attendance + performance;
                    }

                    reader.Close();
                    connection.Close();
                }
            }

            DateTime shiftTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 0, 0);

            RandomEmployees(sum, peopleRequired, weight, tableName, location, shiftTime);
        }

        private void RandomEmployees(int sum, int peopleRequired, string weight, string tableName, string location, DateTime shiftTime)
        {
            if (!selectedEmployeesPerTable.ContainsKey(tableName))
            {
                selectedEmployeesPerTable[tableName] = new HashSet<int>();
            }

            int totalEmployees;
            int originalPeopleRequired = peopleRequired;
            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand countCommand = new SqlCommand("SELECT COUNT(*) FROM Employees WHERE AttendanceRating != 0 AND PerformanceRating != 0", connection);

                totalEmployees = (int)countCommand.ExecuteScalar();

                connection.Close();
            }

            int availableEmployees = totalEmployees - selectedEmployeesPerTable[tableName].Count;

            if (availableEmployees <= 0)
            {
                MessageBox.Show("All employees have been scheduled. There are no more available employees.");
                return;
            }

            if (peopleRequired > availableEmployees)
            {
                MessageBox.Show($"Only {availableEmployees} employees are available. Scheduling as many as possible.");
                peopleRequired = availableEmployees;
            }

            Random rand = new Random();

            // Generate random numbers and select employees until the required number of people are selected
            for (int i = 0; i < peopleRequired;)
            {
                int randomNumber = rand.Next(1, sum + 1); // Generate a random number between 1 and the total employee weights

                int cumulativeWeight = 0;
                int selectedEmployeeId = -1;
                string selectedEmployeeName = "";
                connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command;
                    SqlDataReader reader;

                    if (weight != "overallRating")
                    {
                        string query = $"SELECT EmployeeID, {weight}, FirstName FROM Employees";
                        command = new SqlCommand(query, connection);
                    }
                    else
                    {
                        string query = "SELECT EmployeeID, AttendanceRating, PerformanceRating, FirstName FROM Employees";
                        command = new SqlCommand(query, connection);
                    }

                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int employeeWeight = weight != "overallRating" ? reader.GetInt32(1) : reader.GetInt32(1) + reader.GetInt32(2);
                        cumulativeWeight += employeeWeight;

                        if (cumulativeWeight >= randomNumber)
                        {
                            selectedEmployeeId = reader.GetInt32(0);
                            selectedEmployeeName = weight != "overallRating" ? reader.GetString(2) : reader.GetString(3);
                            break;
                        }
                    }


                    reader.Close();
                }

                if (!selectedEmployeesPerTable[tableName].Contains(selectedEmployeeId))
                {
                    selectedEmployeesPerTable[tableName].Add(selectedEmployeeId);

                    siteEmployees.Add(selectedEmployeeId);

                    // Do something with the selected employee ID, such as displaying it in a messagebox
                    MessageBox.Show($"Selected employee ID: {selectedEmployeeId} Employee Name: {selectedEmployeeName}");

                    // Increment the counter only if a unique employee has been selected
                    i++;
                }


            }


        }

    }
}



