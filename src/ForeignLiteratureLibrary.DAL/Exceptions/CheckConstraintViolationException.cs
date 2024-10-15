using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.DAL.Exceptions;

public class CheckConstraintViolationException : Exception
{
    public CheckConstraintViolationException()
    {
    }

    public CheckConstraintViolationException(string? message) : base(message)
    {
    }

    public CheckConstraintViolationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
