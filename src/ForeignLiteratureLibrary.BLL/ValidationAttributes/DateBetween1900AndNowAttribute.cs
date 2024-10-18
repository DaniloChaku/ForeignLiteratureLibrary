using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.ValidationAttributes;

public class DateBetween1900AndNowAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return null;
        }   

        DateTime date = (DateTime)value;
        if (date < new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc) || date > DateTime.Today)
        {
            return new ValidationResult(ErrorMessage);
        }
        return ValidationResult.Success;
    }
}
