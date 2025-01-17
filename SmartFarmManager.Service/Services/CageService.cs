using MailKit;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.GrowthStage;
using SmartFarmManager.Service.BusinessModels.TaskDaily;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
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
            // Lấy dữ liệu ban đầu từ UnitOfWork
            var query = _unitOfWork.Cages.FindAll(false, x => x.Farm)
                .Include(c => c.CageStaffs)
                .ThenInclude(cs => cs.StaffFarm)
                .Include(c => c.FarmingBatches).ThenInclude(c => c.GrowthStages)
                .AsQueryable();

            // Áp dụng các bộ lọc
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
            // Đếm tổng số bản ghi (chạy trên SQL)
            var totalCount = await query.CountAsync();

            // Phân trang và chọn dữ liệu cần thiết (chạy trên SQL)
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
                    CameraUrl = c.CameraUrl,
                    StaffId = c.CageStaffs.FirstOrDefault().StaffFarmId, // Lấy StaffId từ CageStaff
                    StaffName = c.CageStaffs.FirstOrDefault().StaffFarm.FullName,
                    // Lấy thông tin FarmingBatch phù hợp
                    FarmingBatch = c.FarmingBatches
                .Where(fb => fb.StartDate < DateTimeUtils.VietnamNow() && fb.CompleteAt == null && fb.Status == FarmingBatchStatusEnum.Active)
                .Select(fb => new FarmingBatchModel
                {
                    Id = fb.Id,
                    Name = fb.Name,
                    StartDate = fb.StartDate,
                    CompleteAt = fb.CompleteAt,
                    Status = fb.Status,
                    CleaningFrequency = fb.CleaningFrequency,
                    Quantity = fb.Quantity,
                    AffectedQuantity = fb.AffectedQuantity,
                    GrowthStageDetails = fb.GrowthStages.Where(gs =>
                        gs.AgeStartDate.HasValue &&
                        gs.AgeEndDate.HasValue &&
                        gs.AgeStartDate.Value.Date <= DateTimeUtils.VietnamNow().Date &&
                        gs.AgeEndDate.Value.Date >= DateTimeUtils.VietnamNow().Date)
                        .Select(gs => new GrowthStageDetailModel
                        {
                            Id = gs.Id,
                            Name = gs.Name,
                            WeightAnimal = gs.WeightAnimal,
                            Quantity = gs.Quantity,
                            AgeStart = gs.AgeStart,
                            AgeEnd = gs.AgeEnd,
                            AgeStartDate = gs.AgeStartDate,
                            AgeEndDate = gs.AgeEndDate,
                            Status = gs.Status,
                            RecommendedWeightPerSession = gs.RecommendedWeightPerSession,
                            WeightBasedOnBodyMass = gs.WeightBasedOnBodyMass,
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefault()

                })
                .ToListAsync();

            // Kết quả trả về dạng phân trang
            return new PagedResult<CageResponseModel>
            {
                Items = items,
                TotalItems = totalCount,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
            };
        }

        public async Task<CageDetailModel> GetCageByIdAsync(Guid cageId)
        {
            // Lấy dữ liệu từ repository
            var cage = await _unitOfWork.Cages.FindByCondition(x => x.Id == cageId, false, c => c.Farm).Include(c => c.CageStaffs)
                .ThenInclude(cs => cs.StaffFarm).FirstOrDefaultAsync();

            // Xử lý khi không tìm thấy cage
            if (cage == null || cage.IsDeleted)
            {
                throw new KeyNotFoundException("Cage not found.");
            }

            // Trả về DTO
            return new CageDetailModel
            {
                Id = cage.Id,
                PenCode = cage.PenCode,
                FarmId = cage.FarmId,
                Name = cage.Name,
                Area = cage.Area,
                Location = cage.Location,
                Capacity = cage.Capacity,
                AnimalType = cage.AnimalType,
                BoardCode = cage.BoardCode,
                BoardStatus = cage.BoardStatus,
                CreatedDate = cage.CreatedDate,
                CameraUrl = cage.CameraUrl,
                StaffId = cage.CageStaffs.FirstOrDefault().StaffFarmId, // Lấy StaffId từ CageStaff
                StaffName = cage.CageStaffs.FirstOrDefault().StaffFarm.FullName
            };
        }


        public async Task<List<CageResponseModel>> GetUserCagesAsync(Guid userId)
        {
            // Lấy danh sách cages mà user thuộc
            var userCages = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.StaffFarmId == userId && !cs.Cage.IsDeleted)
                .Include(cs => cs.Cage)
                .Select(cs => new CageResponseModel
                {
                    Id = cs.Cage.Id,
                    PenCode = cs.Cage.PenCode,
                    FarmId = cs.Cage.FarmId,
                    Name = cs.Cage.Name,
                    Area = cs.Cage.Area,
                    Location = cs.Cage.Location,
                    Capacity = cs.Cage.Capacity,
                    AnimalType = cs.Cage.AnimalType,
                    BoardCode = cs.Cage.BoardCode,
                    BoardStatus = cs.Cage.BoardStatus,
                    CreatedDate = cs.Cage.CreatedDate,
                    CameraUrl = cs.Cage.CameraUrl
                })
                .ToListAsync();

            // Kiểm tra nếu không có cage nào
            if (!userCages.Any())
            {
                throw new ArgumentException($"No cages found for user with ID {userId}.");
            }

            return userCages;
        }

        public async Task<Guid> CreateCageAsync(CageModel model)
        {
            var cage = new Cage
            {
                FarmId = model.FarmId,
                Name = model.Name,
                Area = model.Area,
                Capacity = model.Capacity,
                Location = model.Location,
                AnimalType = model.AnimalType,
                CreatedDate = DateTimeUtils.VietnamNow(),
            };

            var id = await _unitOfWork.Cages.CreateAsync(cage);
            await _unitOfWork.CommitAsync();
            return id;
        }


        public async Task<IEnumerable<CageModel>> GetAllCagesAsync(string? search)
        {
            var cages = await _unitOfWork.Cages.FindAllAsync(c => string.IsNullOrEmpty(search) || c.Name.Contains(search));

            return cages.Select(c => new CageModel
            {
                Id = c.Id,
                FarmId = c.FarmId,
                Name = c.Name,
                Area = c.Area,
                Capacity = c.Capacity,
                Location = c.Location,
                AnimalType = c.AnimalType
            });
        }

        public async Task<bool> UpdateCageAsync(Guid id, CageModel model)
        {
            var cage = await _unitOfWork.Cages.GetByIdAsync(id);
            if (cage == null) return false;

            cage.FarmId = model.FarmId;
            cage.Name = model.Name;
            cage.Area = model.Area;
            cage.Capacity = model.Capacity;
            cage.Location = model.Location;
            cage.AnimalType = model.AnimalType;
            cage.ModifiedDate = DateTimeUtils.VietnamNow();

            await _unitOfWork.Cages.UpdateAsync(cage);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteCageAsync(Guid id)
        {
            var cage = await _unitOfWork.Cages.GetByIdAsync(id);
            if (cage == null) return false;

            cage.IsDeleted = true;
            cage.DeletedDate = DateTimeUtils.VietnamNow();

            await _unitOfWork.Cages.UpdateAsync(cage);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}
