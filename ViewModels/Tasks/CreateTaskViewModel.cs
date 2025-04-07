using System.ComponentModel.DataAnnotations;

public class CreateTaskViewModel
{
    [Required]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public string Status { get; set; } = "ToDo"; // Или другой статус по умолчанию

    public DateTime? DueDate { get; set; }
}
