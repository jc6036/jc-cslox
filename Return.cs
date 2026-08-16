using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace my_jlox
{
    public class ReturnException : Exception
    {
        public object? value;

        public ReturnException(object? value) : base("") // It's insane to use an exception handler to pass values up the stack lmao, but following the book
        {                                                // I would 100% not do this otherwise
            this.value = value; 
        }
    }
}
