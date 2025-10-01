using Microsoft.EntityFrameworkCore;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.ModuleEntitys
{
    public class RunDataEntity<T> where T : unmanaged
    {
        [Key]
        public T Id { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

       
        public DateOnly CreateDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
       
    }
}
