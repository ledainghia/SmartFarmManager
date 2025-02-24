using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.WhiteListDomain;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.WhiteListDomain;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class WhitelistDomainController : ControllerBase
    {
        private readonly IWhitelistDomainService _whitelistDomainService;
        private readonly ILogger<WhitelistDomainController> _logger;

        public WhitelistDomainController(IWhitelistDomainService whitelistDomainService, ILogger<WhitelistDomainController> logger)
        {
            _whitelistDomainService = whitelistDomainService;
            _logger = logger;
        }

        [HttpPost()]
        public async Task<IActionResult> AddDomain([FromBody] AddDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Collect validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();

                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
                {
                    { "Errors", errors.ToArray() }
                }));
            }

            try
            {
                var whitelistDomain = await _whitelistDomainService.AddDomainAsync(request.Domain);
                return Ok(ApiResult<string>.Succeed($"Domain '{whitelistDomain.Domain}' đã được thêm vào whitelist với API Key: {whitelistDomain.ApiKey}"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResult<string>.Fail($"Lỗi: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllWhitelistDomains([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Lấy danh sách domains với phân trang
                var pagedResult = await _whitelistDomainService.GetAllDomainsAsync(pageNumber, pageSize);

                if (pagedResult.Items == null || pagedResult.TotalItems == 0)
                {
                    return NoContent();  // Trả về 204 nếu không có dữ liệu
                }

                return Ok(ApiResult<PagedResult<WhiteListDomainItemModel>>.Succeed(pagedResult));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy danh sách domain whitelist: {ex.Message}");
                return StatusCode(500, ApiResult<string>.Fail("Lỗi hệ thống khi lấy danh sách domain."));
            }
        }
    }


}
