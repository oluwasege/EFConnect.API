using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Models
{
    public class MessageParams
    {
        public int PageNumber { get; set; } = 1;
        private const int MaxPageSize = 50;
        private int pageSize = 10;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

        public int UserId { get; set; }
        public string MessageContainer { get; set; } = "Unread";
    }
}
