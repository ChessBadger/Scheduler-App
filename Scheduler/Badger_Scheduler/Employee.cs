using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger_Scheduler
{
    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PerformanceRating { get; set; }
        public int AttendanceRating { get; set; }
        public int OverallRating { get { return PerformanceRating + AttendanceRating; } }
        public bool IsSupervisor { get; set; }
        public int EmployeeID { get; set; }



        public Employee(string firstName, string lastName, int performanceRating, int attendanceRating, bool isSupervisor, int employeeID)
        {
            FirstName = firstName;
            LastName = lastName;
            PerformanceRating = performanceRating;
            AttendanceRating = attendanceRating;
            IsSupervisor = isSupervisor;
            EmployeeID = employeeID;
        }
    }
}
