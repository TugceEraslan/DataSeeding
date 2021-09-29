using System;
using System.Collections.Generic;

namespace DataSeeding.Data.EfCore
{
    public partial class Privileges
    {
        public Privileges()
        {
            EmployeePrivileges = new HashSet<EmployeePrivileges>();
        }

        public int Id { get; set; }
        public string PrivilegeName { get; set; }

        public virtual ICollection<EmployeePrivileges> EmployeePrivileges { get; set; }
    }
}
