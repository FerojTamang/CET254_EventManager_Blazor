namespace CET254_EventManager.Models
{
    // Shared Id and audit fields for domain models.
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public int Id { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        // Polymorphic validation hook for model-specific rules.
        public virtual bool Validate()
        {
            return true;
        }

        protected void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
