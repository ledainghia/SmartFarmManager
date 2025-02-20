using Microsoft.Extensions.Logging;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Repository.Repositories;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Service.BusinessModels.WhiteListDomain;
using SmartFarmManager.Service.Helpers;

namespace SmartFarmManager.Service.Services
{
    public class WhitelistDomainService:IWhitelistDomainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WhitelistDomainService> _logger;

        public WhitelistDomainService(IUnitOfWork unitOfWork, ILogger<WhitelistDomainService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<WhitelistDomain> AddDomainAsync(string domain)
        {
            if (await _unitOfWork.WhiteListDomains.AnyAsync(x=>x.Domain==domain))
            {
                throw new InvalidOperationException("Domain đã tồn tại trong whitelist.");
            }
            var newWhiteListDomain = new WhitelistDomain
            {
                Domain = domain,
                ApiKey = GenerateApiKey(),
                IsActive = true

            };

            var apiKey = GenerateApiKey();
            var whitelistDomain = await _unitOfWork.WhiteListDomains.CreateAsync(newWhiteListDomain);

            _logger.LogInformation("✅ Domain {Domain} đã được thêm vào whitelist với API Key: {ApiKey}", domain, newWhiteListDomain.ApiKey);
            await _unitOfWork.CommitAsync();
            return newWhiteListDomain;
        }
        public async Task<PagedResult<WhiteListDomainItemModel>> GetAllDomainsAsync(int pageNumber, int pageSize)
        {
            var query = _unitOfWork.WhiteListDomains.FindAll(false).AsQueryable();

            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(d=> new WhiteListDomainItemModel
            {
                Domain = d.Domain,
                ApiKey = d.ApiKey,
                CreatedAt = d.CreatedAt,
                IsActive = d.IsActive
            }).ToListAsync();
            var result = new PaginatedList<WhiteListDomainItemModel>(items, totalItems, pageNumber, pageSize);
            return new PagedResult<WhiteListDomainItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
            };
        }

        

        private string GenerateApiKey()
        {
            using var hmac = new HMACSHA256();
            var keyBytes = hmac.Key;
            return Convert.ToBase64String(keyBytes).Replace("=", "").Replace("+", "").Replace("/", "");
        }
    }
}
