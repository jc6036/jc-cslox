using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace my_jlox
{
    public class RuntimeError : Exception
    {
        public Token token { get; set; }
        public RuntimeError(Token token, string message) : base(message)
        {            
            this.token = token;
        }
    }
}
