namespace DecorStore.API.Interfaces
{
    /// <summary>
    /// Base interface for all entities with common properties
    /// </summary>
    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}
