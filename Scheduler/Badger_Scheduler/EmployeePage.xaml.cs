using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Configuration;


namespace Badger_Scheduler
{
  
    public partial class EmployeePage : Window
    {
        private List<Employee> employees = new List<Employee>();

        public EmployeePage()
        {
            InitializeComponent();
            dgEmployees.ItemsSource = employees;
            LoadEmployeesFromDatabase();

        }

        private void LoadEmployeesFromDatabase()
        {
            // Get the connection string from the app.config file
            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            // Open a connection to the database using the connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Define the SQL query to retrieve all employees from the Employees table
                string query = "SELECT * FROM Employees";

                // Create a SqlCommand object with the SQL query and the SqlConnection object
                SqlCommand command = new SqlCommand(query, connection);

                // Execute the SQL query and retrieve the results using a SqlDataReader object
                SqlDataReader reader = command.ExecuteReader();

                // Loop through each row in the result set and create an Employee object with the data
                while (reader.Read())
                {
                    int employeeID = reader.GetInt32(0);
                    string firstName = reader.GetString(1);
                    string lastName = reader.GetString(2);
                    int performanceRating = reader.GetInt32(3);
                    int attendanceRating = reader.GetInt32(4);
                    bool isSupervisor = reader.GetBoolean(5);

                    Employee employee = new Employee(firstName, lastName, performanceRating, attendanceRating, isSupervisor, employeeID);

                    // Add the Employee object to the employees list
                    employees.Add(employee);
                }
            }

            // Refresh the datagrid to display the retrieved employees
            dgEmployees.Items.Refresh();
        }


        private bool IsValidRating(int rating)
        {
            return rating >= 1 && rating <= 10;
        }

        private void ClearInputFields()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtPerformanceRating.Text = "";
            txtAttendanceRating.Text = "";
            txtEmployeeID.Text = "";
            chkIsSupervisor.IsChecked = false;

            dgEmployees.Items.Refresh();
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            string firstName = txtFirstName.Text.Trim().ToUpper();
            string lastName = txtLastName.Text.Trim().ToUpper();
            int performanceRating;
            int attendanceRating;
            int employeeID;

            // Verify that both the first name and last name fields have values

            try
            {
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    MessageBox.Show("Please enter a value for the first name and last name.");
                    return;
                }

                // Verify that the performance and attendance rating inputs are valid integers
                if (int.TryParse(txtPerformanceRating.Text, out performanceRating) && int.TryParse(txtAttendanceRating.Text, out attendanceRating) && int.TryParse(txtEmployeeID.Text, out employeeID))
                {
                    // Verify that the performance and attendance ratings are within the valid range
                    if (IsValidRating(performanceRating) && IsValidRating(attendanceRating))
                    {
                        // Get the value of the "Is Supervisor" checkbox
                        bool isSupervisor = chkIsSupervisor.IsChecked.Value;

                        // Create a new Employee object with the input values
                        Employee employee = new Employee(firstName, lastName, performanceRating, attendanceRating, isSupervisor, employeeID);

                        // Add the new employee to the list and the database
                        employees.Add(employee);
                        InsertEmployeeToDatabase(employee);

                        // Clear the input fields and refresh the data grid
                        ClearInputFields();
                    }
                    else
                    {
                        MessageBox.Show("Invalid rating. Please enter a number between 1 and 10.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid rating. Please enter a number between 1 and 10.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InsertEmployeeToDatabase(Employee employee)
        {
            // Retrieve the connection string from the application configuration file
            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            // Create a new database connection object and open the connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Construct the SQL query to insert a new record into the "Employees" table
                string query = "INSERT INTO Employees (EmployeeID, FirstName, LastName, PerformanceRating, AttendanceRating, IsSupervisor) " +
                               "VALUES (@EmployeeID, @FirstName, @LastName, @PerformanceRating, @AttendanceRating, @IsSupervisor)";

                // Create a new SQL command object with the constructed query and the open connection
                SqlCommand command = new SqlCommand(query, connection);

                // Add parameter values to the command object for each of the employee's properties
                command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@PerformanceRating", employee.PerformanceRating);
                command.Parameters.AddWithValue("@AttendanceRating", employee.AttendanceRating);
                command.Parameters.AddWithValue("@IsSupervisor", employee.IsSupervisor);

                // Execute the command to insert the new employee record into the database
                command.ExecuteNonQuery();
            }
        }


        private void dgEmployees_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dgEmployees.SelectedItem != null)
            {
                Employee selectedEmployee = (Employee)dgEmployees.SelectedItem;

                // Open a new window to edit the selected employee record
                EditEmployeeWindow editWindow = new EditEmployeeWindow(selectedEmployee, this);
                editWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if (editWindow.ShowDialog() == true)
                {
                    // Update the employee record in the list and in the database
                    selectedEmployee = editWindow.Employee;
                    UpdateEmployeeInDatabase(selectedEmployee);

                    dgEmployees.Items.Refresh();
                }
            }
        }

   
        private void UpdateEmployeeInDatabase(Employee employee)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, PerformanceRating = @PerformanceRating, " +
                    "AttendanceRating = @AttendanceRating, IsSupervisor = @IsSupervisor WHERE EmployeeID = @EmployeeID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@PerformanceRating", employee.PerformanceRating);
                command.Parameters.AddWithValue("@AttendanceRating", employee.AttendanceRating);
                command.Parameters.AddWithValue("@IsSupervisor", employee.IsSupervisor);

                command.ExecuteNonQuery();
            }
        }


    }
}


