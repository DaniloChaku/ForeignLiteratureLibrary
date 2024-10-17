using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.DAL.Exceptions;

public class BookEditionUnavailableException : Exception
{
    public BookEditionUnavailableException()
    {
    }

    public BookEditionUnavailableException(string? message) : base(message)
    {
    }

    public BookEditionUnavailableException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
