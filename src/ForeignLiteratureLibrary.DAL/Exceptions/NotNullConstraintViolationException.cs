using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.DAL.Exceptions;

public class NotNullConstraintViolationException : Exception
{
    public NotNullConstraintViolationException()
    {
    }

    public NotNullConstraintViolationException(string? message) : base(message)
    {
    }

    public NotNullConstraintViolationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
