using System.Windows;

namespace Badger_Scheduler
{
    public partial class EditEmployeeWindow : Window
    {
        public Employee Employee { get; private set; }

        public EditEmployeeWindow(Employee employee, EmployeePage owner)
        {
            InitializeComponent();
            DataContext = employee;
            Owner = owner;

            Employee = employee;

            // Display current employee data in the text boxes
            txtFirstName.Text = Employee.FirstName;
            txtLastName.Text = Employee.LastName;
            txtPerformanceRating.Text = Employee.PerformanceRating.ToString();
            txtAttendanceRating.Text = Employee.AttendanceRating.ToString();
            chkIsSupervisor.IsChecked = Employee.IsSupervisor;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            // Update the employee object with the modified data
            Employee.FirstName = txtFirstName.Text.ToUpper();
            Employee.LastName = txtLastName.Text.ToUpper();

            if (string.IsNullOrWhiteSpace(Employee.FirstName) || string.IsNullOrWhiteSpace(Employee.LastName))
            {
                MessageBox.Show("Please enter a value for the first name and last name.");
                return;
            }

            int performanceRating;
            if (int.TryParse(txtPerformanceRating.Text, out performanceRating))
            {
                Employee.PerformanceRating = performanceRating;
            }
            int attendanceRating;
            if (int.TryParse(txtAttendanceRating.Text, out attendanceRating))
            {
                Employee.AttendanceRating = attendanceRating;
            }
            Employee.IsSupervisor = chkIsSupervisor.IsChecked.Value;

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

}
