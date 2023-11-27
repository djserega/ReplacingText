using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplacingText.Models
{
    internal class ReplaceStatus
    {
        public ReplaceStatus(string message)
        {
            Message = message;
        }

        public ReplaceStatus(int percent)
        {
            Percent = percent;
        }

        public ReplaceStatus(string message, int percent)
        {
            Message = message;
            Percent = percent;
        }

        internal string Message { get; set; }
        internal int Percent { get; set; }
    }
}
