namespace SmartFarmManager.Service.BusinessModels.Auth
{
    public class UserDetailsModel
    {
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public List<Guid> FarmIds { get; set; } = new List<Guid>();
        public List<Guid> CageIds { get; set; } = new List<Guid>();
        public List<Guid> FarmBatchIds { get; set; } = new List<Guid>();
    }
}
