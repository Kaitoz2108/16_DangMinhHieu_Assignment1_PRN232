using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public short AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ProfileUpdateRequest
{
    [Required(ErrorMessage = "Account Name is required.")]
    [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters.")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters.")]
    public string AccountEmail { get; set; } = string.Empty;

    [StringLength(70, MinimumLength = 1, ErrorMessage = "Password must be at most 70 characters.")]
    public string? AccountPassword { get; set; }
}

public class CategoryDto
{
    public short CategoryId { get; set; }

    [Required(ErrorMessage = "Category Name is required.")]
    [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category Description is required.")]
    [StringLength(250, ErrorMessage = "Category Description cannot exceed 250 characters.")]
    public string CategoryDesciption { get; set; } = string.Empty;

    public short? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool? IsActive { get; set; } = true;
    public int NewsArticleCount { get; set; }
}

public class TagDto
{
    public int TagId { get; set; }

    [Required(ErrorMessage = "Tag Name is required.")]
    [StringLength(50, ErrorMessage = "Tag Name cannot exceed 50 characters.")]
    public string TagName { get; set; } = string.Empty;

    [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters.")]
    public string? Note { get; set; }
}

public class SystemAccountDto
{
    public short AccountId { get; set; }

    [Required(ErrorMessage = "Account Name is required.")]
    [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters.")]
    public string? AccountName { get; set; }

    [Required(ErrorMessage = "Account Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(70, ErrorMessage = "Account Email cannot exceed 70 characters.")]
    public string? AccountEmail { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public int? AccountRole { get; set; }

    public string? RoleName => AccountRole switch
    {
        0 => "Admin",
        1 => "Staff",
        2 => "Lecturer",
        _ => "Unknown"
    };

    [StringLength(70, ErrorMessage = "Password cannot exceed 70 characters.")]
    public string? AccountPassword { get; set; }
}

public class NewsArticleDto
{
    public string NewsArticleId { get; set; } = string.Empty;

    [StringLength(400, ErrorMessage = "News Title cannot exceed 400 characters.")]
    public string? NewsTitle { get; set; }

    [Required(ErrorMessage = "Headline is required.")]
    [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters.")]
    public string Headline { get; set; } = string.Empty;

    public DateTime? CreatedDate { get; set; }

    [StringLength(4000, ErrorMessage = "News Content cannot exceed 4000 characters.")]
    public string? NewsContent { get; set; }

    [StringLength(400, ErrorMessage = "News Source cannot exceed 400 characters.")]
    public string? NewsSource { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public short? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public bool? NewsStatus { get; set; } = true;

    public short? CreatedById { get; set; }
    public string? CreatedByName { get; set; }

    public short? UpdatedById { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public List<int> TagIds { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
}

public class ReportFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
