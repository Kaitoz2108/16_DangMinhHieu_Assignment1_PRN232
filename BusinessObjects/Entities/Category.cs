using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Entities;

public partial class Category
{
    [Key]
    public short CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    [Column("CategoryDescription")]
    public string CategoryDesciption { get; set; } = null!;

    [NotMapped]
    public string CategoryDescription
    {
        get => CategoryDesciption;
        set => CategoryDesciption = value;
    }

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();

    public virtual Category? ParentCategory { get; set; }
}
