namespace SmartFarmManager.API.Payloads.Responses.Picture
{
    public class PictureResponse
    {
        public Guid Id { get; set; }
        public string Image { get; set; }
        public DateTime? DateCaptured { get; set; }
    }
}
