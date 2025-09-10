using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhaTro.Application.DTOs
{
    public class RoleDto
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
    public class CreateRoleDto
    {
        public string RoleName { get; set; }
    }
}
