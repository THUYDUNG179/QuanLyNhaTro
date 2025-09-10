using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhaTro.Application.DTOs
{
    public class RoomStatusDto
    {
        public int RoomStatusId { get; set; }

        public string StatusName { get; set; } = null!;

    }
    public class CreateRoomStatusDto
    {
        public string StatusName { get; set; }
    }
}
