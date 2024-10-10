using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class ReaderDto
{
    [Required]
    [StringLength(20, ErrorMessage = "LibraryCardNumber cannot exceed 20 characters.")]
    public string LibraryCardNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1900-01-01", "9999-12-31", ErrorMessage = "DateOfBirth must be between 1900-01-01 and today.")]
    public DateTime DateOfBirth { get; set; }

    [StringLength(255)]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1900-01-01", "9999-12-31", ErrorMessage = "RegistrationDate must be between 1900-01-01 and today.")]
    public DateTime RegistrationDate { get; set; }

    public List<BookEditionLoanDto> Loans { get; set; } =[];

    public Reader ToEntity()
    {
        return new Reader
        {
            LibraryCardNumber = this.LibraryCardNumber.Trim(),
            FullName = this.FullName.Trim(),
            DateOfBirth = this.DateOfBirth,
            Email = this.Email?.Trim(),
            Phone = this.Phone?.Trim(),
            RegistrationDate = this.RegistrationDate,
            Loans = this.Loans.ConvertAll(l => l.ToEntity())
        };
    }
}

public static class ReaderExtensions
{
    public static ReaderDto ToDto(this Reader reader)
    {
        return new ReaderDto
        {
            LibraryCardNumber = reader.LibraryCardNumber.Trim(),
            FullName = reader.FullName.Trim(),
            DateOfBirth = reader.DateOfBirth,
            Email = reader.Email?.Trim(),
            Phone = reader.Phone?.Trim(),
            RegistrationDate = reader.RegistrationDate,
            Loans = reader.Loans.Select(l => l.ToDto()).ToList()
        };
    }
}