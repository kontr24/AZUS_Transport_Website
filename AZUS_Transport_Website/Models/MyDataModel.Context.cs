//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AZUS_Transport_Website.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ASUZ_Transport_DBEntitie : DbContext
    {
        public ASUZ_Transport_DBEntitie()
            : base("name=ASUZ_Transport_DBEntitie")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Applications> Applications { get; set; }
        public virtual DbSet<Cars> Cars { get; set; }
        public virtual DbSet<Divisions> Divisions { get; set; }
        public virtual DbSet<ModelCars> ModelCars { get; set; }
        public virtual DbSet<StatusCars> StatusCars { get; set; }
        public virtual DbSet<Statuses> Statuses { get; set; }
        public virtual DbSet<StatusesDone> StatusesDone { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<TypeCars> TypeCars { get; set; }
        public virtual DbSet<Users> Users { get; set; }
    }
}
