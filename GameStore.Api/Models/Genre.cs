namespace GameStore.Api.Models;

public class Genre : IAuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}