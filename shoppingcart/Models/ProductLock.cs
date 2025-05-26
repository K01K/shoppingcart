public class ProductLock
{
    public int Id { get; set; }
    public string ProductId { get; set; }
    public string BasketId { get; set; }
    public string UserId { get; set; }
    public DateTime LockedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}