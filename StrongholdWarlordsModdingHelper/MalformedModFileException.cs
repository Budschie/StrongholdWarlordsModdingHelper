using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrongholdWarlordsModdingHelper
{
    class MalformedModFileException : Exception
    {
        public MalformedModFileException() : base()
        {
        }

        public MalformedModFileException(string message) : base(message)
        {
        }
    }
}
