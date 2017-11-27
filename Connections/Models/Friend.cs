using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Connections.Models
{
    public class Friend
    {
        public int Id { get; set; }
        //Person to Accept Request
        public int FriendID { get; set; }
        //Person Making Connect Request
        public int UserID { get; set; }
        public bool StatusOfRequest { get; set; }

        //bool initially false on connect
        //accept and add to friend list, change bool to true
    }
}
