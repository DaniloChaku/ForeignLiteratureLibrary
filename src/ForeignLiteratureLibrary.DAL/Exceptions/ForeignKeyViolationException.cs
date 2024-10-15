using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.DAL.Exceptions;

public class ForeignKeyViolationException : Exception
{
    public ForeignKeyViolationException()
    {
    }

    public ForeignKeyViolationException(string? message) : base(message)
    {
    }

    public ForeignKeyViolationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
