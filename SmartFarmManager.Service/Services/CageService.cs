﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class CageService : ICageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<CageResponseModel>> GetCagesAsync(CageFilterModel request)
        {
            var query = _unitOfWork.Cages.FindByCondition(c => !c.IsDeleted);

            // Apply filters
            if (request.FarmId.HasValue)
            {
                query = query.Where(c => c.FarmId == request.FarmId.Value);
            }

            if (!string.IsNullOrEmpty(request.AnimalType))
            {
                query = query.Where(c => c.AnimalType.Contains(request.AnimalType));
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(c => c.Name.Contains(request.Name));
            }

            if (request.BoardStatus.HasValue)
            {
                query = query.Where(c => c.BoardStatus == request.BoardStatus.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CageResponseModel
                {
                    Id = c.Id,
                    PenCode = c.PenCode,    
                    FarmId = c.FarmId,
                    Name = c.Name,
                    Area = c.Area,
                    Location = c.Location,
                    Capacity = c.Capacity,
                    AnimalType = c.AnimalType,
                    BoardCode = c.BoardCode,
                    BoardStatus = c.BoardStatus,
                    CreatedDate = c.CreatedDate,
                    CameraUrl = c.CameraUrl
                })
                .ToListAsync();

            var result = new PaginatedList<CageResponseModel>(items, totalCount, request.PageNumber, request.PageSize);
            return new PagedResult<CageResponseModel>()
            {
                Items=result.Items,
                TotalItems=result.TotalCount,
                CurrentPage=result.CurrentPage,
                PageSize=result.PageSize,
                TotalPages=result.TotalPages,
                HasNextPage=result.HasNextPage,
                HasPreviousPage=result.HasPreviousPage,
            };
        }
    }
}
