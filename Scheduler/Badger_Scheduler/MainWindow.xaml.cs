using System.Collections.Generic;
using System.Windows;

namespace Badger_Scheduler
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
   
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EmployeePage employeePage = new EmployeePage();
                employeePage.Owner = this;
                employeePage.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                employeePage.ShowDialog();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEditSchedule_Click(object sender, RoutedEventArgs e)
        {
            EditScheduleWindow editScheduleWindow = new EditScheduleWindow();
            editScheduleWindow.Owner = this;
            editScheduleWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            editScheduleWindow.ShowDialog();
        }
        private void btnGenerateSchedule_Click(object sender, RoutedEventArgs e)
        {
            GenerateScheduleWindow generateScheduleWindow= new GenerateScheduleWindow();
            generateScheduleWindow.Owner = this;
            generateScheduleWindow.WindowStartupLocation= WindowStartupLocation.CenterOwner;
            generateScheduleWindow.ShowDialog();
        }
    }
}
