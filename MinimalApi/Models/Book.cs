using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Models;

[Table("Products")]
public class Book
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
}
