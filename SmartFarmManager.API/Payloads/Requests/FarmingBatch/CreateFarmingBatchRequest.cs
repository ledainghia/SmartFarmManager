﻿using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FarmingBatch
{
    public class CreateFarmingBatchRequest
    {
        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        public Guid CageId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Species { get; set; }

        [Required]
        public int CleaningFrequency { get; set; }

        [Required]
        public int Quantity { get; set; }



        public CreateFarmingBatchModel MapToModel()
        {
            return new CreateFarmingBatchModel
            {
                TemplateId = this.TemplateId,
                CageId = this.CageId,
                Name = this.Name,
                Species = this.Species,
                CleaningFrequency = this.CleaningFrequency,
                Quantity = this.Quantity,
            };
        }
    }

}
