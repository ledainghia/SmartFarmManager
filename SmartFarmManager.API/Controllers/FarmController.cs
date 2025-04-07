﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Farm;
using SmartFarmManager.API.Payloads.Responses.Auth;
using SmartFarmManager.API.Payloads.Responses.Farm;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.CostingReport;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;
using Sprache;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmController : ControllerBase
    {
        private readonly IFarmService _farmService;
        private readonly IStaffService _staffService;

        public FarmController(IFarmService farmService, IStaffService staffService)
        {
            _farmService = farmService;
            _staffService = staffService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFarm([FromBody] CreateFarmRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            try
            {
                var farmId = await _farmService.CreateFarmAsync(new FarmModel
                {
                    Name = request.Name,
                    Address = request.Address,
                    Area = request.Area,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email
                });

                var response = ApiResult<object>.Succeed(new { Id = farmId });

                // Trả về 201 Created cùng với response body và URL của tài nguyên mới
                return CreatedAtAction(nameof(GetFarmById), new { id = farmId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFarmById(Guid id)
        {
            try
            {
                var farmModel = await _farmService.GetFarmByIdAsync(id);
                if (farmModel == null)
                {
                    return NotFound(ApiResult<string>.Fail("Farm not found."));
                }

                var response = new FarmResponse
                {
                    Id = farmModel.Id,
                    Name = farmModel.Name,
                    Address = farmModel.Address,
                    Area = farmModel.Area,
                    PhoneNumber = farmModel.PhoneNumber,
                    Email = farmModel.Email
                };

                return Ok(ApiResult<FarmResponse>.Succeed(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFarms([FromQuery] string? search)
        {
            try
            {
                var farmModels = await _farmService.GetAllFarmsAsync(search);

                var responses = farmModels.Select(f => new FarmResponse
                {
                    Id = f.Id,
                    Name = f.Name,
                    Address = f.Address,
                    Area = f.Area,
                    PhoneNumber = f.PhoneNumber,
                    Email = f.Email
                });

                return Ok(ApiResult<IEnumerable<FarmResponse>>.Succeed(responses));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFarm(Guid id, [FromBody] UpdateFarmRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            try
            {
                var updated = await _farmService.UpdateFarmAsync(id, new FarmModel
                {
                    Name = request.Name,
                    Address = request.Address,
                    Area = request.Area,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email
                });

                if (!updated)
                {
                    return NotFound(ApiResult<string>.Fail("Farm not found."));
                }

                return Ok(ApiResult<string>.Succeed("Farm successfully updated."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFarm(Guid id)
        {
            try
            {
                var deleted = await _farmService.DeleteFarmAsync(id);
                if (!deleted)
                {
                    return NotFound(ApiResult<string>.Fail("Farm not found."));
                }

                return Ok(ApiResult<string>.Succeed("Farm successfully deleted."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("{farmId:guid}/users")]
        public async Task<IActionResult> GetUsersByFarmId(Guid farmId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var paginatedUsers = await _staffService.GetStaffFarmsByFarmIdAsync(farmId, pageIndex, pageSize);

                if (paginatedUsers == null || !paginatedUsers.Items.Any())
                {
                    return NotFound(ApiResult<string>.Fail("No users found for the given FarmId."));
                }

                 return Ok(ApiResult<PagedResult<UserModel>>.Succeed(paginatedUsers));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("{farmId}")]
        public async Task<IActionResult> GetCostingReportsByFarm(
        Guid farmId,
        [FromQuery] int? month,
        [FromQuery] int? year)
        {
            var reports = await _farmService.GetCostingReportsByFarmAsync(farmId, month, year);
            return Ok(ApiResult<IEnumerable<CostingReportModel>>.Succeed(reports));
        }
    }

}
