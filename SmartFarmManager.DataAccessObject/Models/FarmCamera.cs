﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class FarmCamera
{
    public Guid FarmCameraId { get; set; }

    public Guid FarmId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public int ChannelId { get; set; }

    public virtual Farm Farm { get; set; }
}