using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.WhiteListDomain
{
    public class AddDomainRequest
    {
        [Required(ErrorMessage = "Domain là bắt buộc.")]
        //[RegularExpression(@"^[A-Za-z0-9-]{1,63}\.[A-Za-z]{2,6}$", ErrorMessage = "Định dạng domain không hợp lệ. Vui lòng kiểm tra lại.")]
        public string Domain { get; set; }
    }
}
