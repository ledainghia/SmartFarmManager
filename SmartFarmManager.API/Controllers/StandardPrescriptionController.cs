using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.StandardPrescription;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.StandardPrescription;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StandardPrescriptionController : ControllerBase
    {
        private readonly IStandardPrescriptionService _standardPrescriptionService;

        public StandardPrescriptionController(IStandardPrescriptionService service)
        {
            _standardPrescriptionService = service;
        }

        [HttpGet("{diseaseId:guid}")]
        public async Task<IActionResult> GetStandardPrescriptionsByDiseaseId(Guid diseaseId)
        {
            try
            {
                var prescriptions = await _standardPrescriptionService.GetStandardPrescriptionsByDiseaseIdAsync(diseaseId);

                if (prescriptions == null)
                {
                    return NotFound(ApiResult<object>.Fail($"Không có đơn thuốc mẫu cho bệnh với id là: {diseaseId}"));
                }

                return Ok(ApiResult<StandardPrescriptionModel>.Succeed(prescriptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An unexpected error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateStandardPrescription([FromBody] CreateStandardPrescriptionRequest request)
        {
            if (!ModelState.IsValid)
            {
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
                var model = request.MapToModel();
                var result = await _standardPrescriptionService.CreateStandardPrescriptionAsync(model);

                if (!result)
                {
                    throw new Exception("Xảy ra lỗi khi lưu đơn thuốc mẫu!");
                }

                return Ok(ApiResult<string>.Succeed("Đơn thuốc mẫu được tạo thành công!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStandardPrescription(Guid id, [FromBody] UpdateStandardPrescriptionRequest request)
        {
            if (!ModelState.IsValid)
            {
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
                var model = request.MapToModel();
                var result = await _standardPrescriptionService.UpdateStandardPrescriptionAsync(id, model);

                if (!result)
                {
                    throw new Exception("Error while updating Standard Prescription!");
                }

                return Ok(ApiResult<string>.Succeed("Standard Prescription updated successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStandardPrescription(Guid id)
        {
            try
            {
                var result = await _standardPrescriptionService.DeleteStandardPrescriptionAsync(id);
                if (!result)
                {
                    return Ok(ApiResult<string>.Succeed("Khôi phục thành công!"));
                }
            
                return Ok(ApiResult<string>.Succeed("Xóa thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStandardPrescriptions([FromQuery] StandardPrescriptionFilterModel filter)
        {
            if (!ModelState.IsValid)
            {
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
                var result = await _standardPrescriptionService.GetStandardPrescriptionsAsync(filter);
                return Ok(ApiResult<PagedResult<StandardPrescriptionItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetStandardPrescriptionDetail(Guid id)
        {
            try
            {
                var result = await _standardPrescriptionService.GetStandardPrescriptionDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Phiếu chuẩn bị không tìm thấy."));
                }

                return Ok(ApiResult<StandardPrescriptionDetailResponseModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
