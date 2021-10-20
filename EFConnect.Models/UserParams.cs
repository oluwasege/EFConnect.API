using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Models
{
    public class UserParams
    {
        public int PageNumber { get; set; } = 1;
        private const int MaxPageSize = 50;
        private int pageSize = 12;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }   // if requested value is greater than our max allowed - return the max
        }
        public int UserId { get; set; }
        public string Specialty { get; set; } = "All";
        public string OrderBy { get; set; }
        public bool Followees { get; set; } = false;
        public bool Followers { get; set; } = false;
    }
}
