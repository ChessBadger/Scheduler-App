using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;

namespace Badger_Scheduler
{

    public partial class EditSiteWindow : Window
    {


        private string _location;
        private string _startTime;
        private string _tableName;
        private string _day;
        public EditSiteWindow(string location, string startTime, string tableName, string day, EditScheduleWindow owner)
        {
            InitializeComponent();



            Owner = owner;
            _location = location;
            _startTime = startTime;
            _tableName = tableName;
            _day = day;



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

            LoadSiteData(location, startTime, tableName, day);

        }

        private void LoadSiteData(string location, string startTime, string tableName, string day)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["WeekSites"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM {tableName} WHERE Location = @Location AND StartTime = @StartTime";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Location", location);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();



                    connection.Open();
                    adapter.Fill(table);
                    connection.Close();

                    if (table.Rows.Count > 0)
                    {

                        //System.Windows.Controls.ComboBoxItem: Sunday

                        DataRow row = table.Rows[0];
                        LocationTextBox.Text = row["Location"].ToString();
                        TimeSpan startTimeSpan;
                        TimeSpan.TryParse(row["StartTime"].ToString(), out startTimeSpan);
                        StartTimeTextBox.Text = startTimeSpan.ToString(@"hh\:mm");
                        PeopleRequiredTextBox.Text = row["PeopleRequired"].ToString();
                        SetComboBoxSelectedIndex(DayComboBox, day);
                        SetComboBoxSelectedIndex(PriorityComboBox, row["Priority"].ToString());
                        SetComboBoxSelectedIndex(SupAssignComboBox, row["Supervisor"].ToString());

                    }
                    else
                    {
                        MessageBox.Show("Site data not found.");
                    }

                }

                
            }

        }

        private void SetComboBoxSelectedIndex(ComboBox comboBox, string value)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i].ToString() == $"System.Windows.Controls.ComboBoxItem: {value}")
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }


        private void RemoveSite(string location, string startTime, string tableName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["WeekSites"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"DELETE FROM {tableName} WHERE Location = @Location AND StartTime = @StartTime";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Location", location);
                    command.Parameters.AddWithValue("@StartTime", startTime);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            RemoveSite(_location, _startTime, _tableName);

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


            // Close the EditSiteWindow
            this.Close();
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSite(_location, _startTime, _tableName);
            this.Close();

        }
    }
}
