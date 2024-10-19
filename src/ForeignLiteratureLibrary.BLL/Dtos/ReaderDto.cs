using ForeignLiteratureLibrary.BLL.ValidationAttributes;
using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class ReaderDto
{
    public int ReaderID { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "LibraryCardNumber cannot exceed 20 characters.")]
    public string LibraryCardNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(255)]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public List<LoanDto> Loans { get; set; } =[];

    public Reader ToEntity()
    {
        return new Reader
        {
            ReaderID = ReaderID,
            LibraryCardNumber = this.LibraryCardNumber.Trim(),
            ReaderFullName = this.FullName.Trim(),
            EmailAddress = this.Email?.Trim(),
            PhoneNumber = this.Phone?.Trim(),
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
            ReaderID = reader.ReaderID,
            LibraryCardNumber = reader.LibraryCardNumber.Trim(),
            FullName = reader.ReaderFullName.Trim(),
            Email = reader.EmailAddress?.Trim(),
            Phone = reader.PhoneNumber?.Trim(),
            Loans = reader.Loans.Select(l => l.ToDto()).ToList()
        };
    }
}