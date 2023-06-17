using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Configuration;
using System;
using System.Globalization;
using System.Data;

namespace Badger_Scheduler
{

    public partial class EditScheduleWindow : Window
    {

        public EditScheduleWindow()
        {
            InitializeComponent();
            // Get the connection string from App.config
            string connectionString = ConfigurationManager.ConnectionStrings["BadgerEmployees"].ConnectionString;

            // Define the SQL query to select supervisors from the Employees table
            string sql = "SELECT EmployeeID, FirstName, LastName FROM Employees WHERE IsSupervisor = 1 ORDER BY FirstName, LastName";

            // Create a new SqlConnection object and open the connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a new SqlCommand object with the SQL query and SqlConnection object
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Create a new SqlDataReader object to read the results of the query
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Loop through the rows returned by the query
                        while (reader.Read())
                        {
                            // Get the values of the EmployeeID, FirstName, and LastName columns for the current row
                            int employeeId = (int)reader["EmployeeID"];
                            string firstName = (string)reader["FirstName"];
                            string lastName = (string)reader["LastName"];

                            // Create a new ComboBoxItem with the employee's full name as the content
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = $"{firstName} {lastName}";
                            item.Tag = employeeId; // Set the Tag property of the ComboBoxItem to the EmployeeID

                            // Add the ComboBoxItem to the supAssign ComboBox
                            SupAssignComboBox.Items.Add(item);
                        }
                    }
                }
            }

        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string day = DayComboBox.Text.ToUpper();
            string location = LocationTextBox.Text.ToUpper();
            string startTimeText = StartTimeTextBox.Text;
            string priority = PriorityComboBox.Text;
            string supervisor = SupAssignComboBox.Text;

            // Verify that the text boxes are not blank
            if (string.IsNullOrEmpty(day) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(startTimeText) || string.IsNullOrEmpty(priority) || string.IsNullOrEmpty(supervisor))
            {
                MessageBox.Show("Please fill in all the fields.");
                return;
            }

            // Parse the start time input to a DateTime object
            DateTime startTime;
            if (!DateTime.TryParseExact(startTimeText, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
            {
                // If the input is not in "HH:mm" format, try to parse it as just the hour part (e.g. "4")
                int hour;
                if (int.TryParse(startTimeText, out hour) && hour >= 0 && hour <= 23)
                {
                    startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, 0, 0);
                }
                else
                {
                    // The input is invalid, show an error message and return
                    MessageBox.Show("Invalid start time input. Please enter a valid time in 'H:mm' or 'H' format.");
                    return;
                }
            }

            // Verify that the number of employees input is valid
            int peopleRequired;
            if (!int.TryParse(PeopleRequiredTextBox.Text, out peopleRequired))
            {
                MessageBox.Show("Invalid number of employees input. Please enter a valid integer.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["WeekSites"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string tableName = $"{day}Sites";
                string sql = $"INSERT INTO {tableName} (Location, StartTime, PeopleRequired, Priority, Supervisor) VALUES (@Location, @StartTime, @PeopleRequired, @Priority, @Supervisor)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Location", location);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@PeopleRequired", peopleRequired);
                    command.Parameters.AddWithValue("@Priority", priority);
                    command.Parameters.AddWithValue("@Supervisor", supervisor);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            // Clear the input fields
            LocationTextBox.Text = "";
            StartTimeTextBox.Text = "";
            PeopleRequiredTextBox.Text = "";
            DayComboBox.SelectedItem = null;
            PriorityComboBox.SelectedItem = null;
            SupAssignComboBox.SelectedItem = null;

            // Refresh the data grid
            LoadDataForSelectedDay();

        }

        private void DayFilterComboBox_DropDownClosed(object sender, EventArgs e)
        {
            LoadDataForSelectedDay();
        }

        private void LoadDataForSelectedDay()
        {
            string day = DayFilterComboBox.Text;

            if (string.IsNullOrEmpty(day))
            {
                ScheduleDataGrid.ItemsSource = null;
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["WeekSites"].ConnectionString;
            string tableName = $"{day}Sites";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT Location, StartTime, PeopleRequired, Priority, Supervisor FROM {tableName}";
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                DataTable table = new DataTable();

                connection.Open();
                adapter.Fill(table);
                connection.Close();

                ScheduleDataGrid.ItemsSource = table.DefaultView;
                ScheduleDataGrid.Items.Refresh();
            }
        }

        private void ScheduleDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataRowView selectedRow = ScheduleDataGrid.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                string day = DayFilterComboBox.Text;
                string tableName = $"{day}Sites";
                string location = selectedRow["Location"].ToString();
                string startTime = selectedRow["StartTime"].ToString();

                EditSiteWindow editSiteWindow = new EditSiteWindow(location, startTime, tableName, day, this);
                editSiteWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                editSiteWindow.ShowDialog();

                // Refresh the DataGrid after closing the EditSiteWindow
                LoadDataForSelectedDay();
            }


        }
    }
}
