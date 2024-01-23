using System;
using System.Collections.Generic;
using Etailor.API.Repository.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Etailor.API.Repository.DataAccess
{
    public partial class ETailor_DBContext : DbContext
    {
        public ETailor_DBContext()
        {
        }

        public ETailor_DBContext(DbContextOptions<ETailor_DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Blog> Blogs { get; set; } = null!;
        public virtual DbSet<BodyAttribute> BodyAttributes { get; set; } = null!;
        public virtual DbSet<BodySize> BodySizes { get; set; } = null!;
        public virtual DbSet<Catalog> Catalogs { get; set; } = null!;
        public virtual DbSet<CatalogBodySize> CatalogBodySizes { get; set; } = null!;
        public virtual DbSet<CatalogStage> CatalogStages { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<ChatHistory> ChatHistories { get; set; } = null!;
        public virtual DbSet<Component> Components { get; set; } = null!;
        public virtual DbSet<ComponentType> ComponentTypes { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<CustomerClient> CustomerClients { get; set; } = null!;
        public virtual DbSet<Discount> Discounts { get; set; } = null!;
        public virtual DbSet<Material> Materials { get; set; } = null!;
        public virtual DbSet<MaterialCategory> MaterialCategories { get; set; } = null!;
        public virtual DbSet<MaterialType> MaterialTypes { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderMaterial> OrderMaterials { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductBodySize> ProductBodySizes { get; set; } = null!;
        public virtual DbSet<ProductComponent> ProductComponents { get; set; } = null!;
        public virtual DbSet<ProductStage> ProductStages { get; set; } = null!;
        public virtual DbSet<ProfileBody> ProfileBodies { get; set; } = null!;
        public virtual DbSet<Skill> Skills { get; set; } = null!;
        public virtual DbSet<SkillForStage> SkillForStages { get; set; } = null!;
        public virtual DbSet<SkillOfStaff> SkillOfStaffs { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<Staff> Staff { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable("Blog");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.Property(e => e.UrlPath).HasMaxLength(255);

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Blogs)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__Blog__StaffId__65F62111");
            });

            modelBuilder.Entity<BodyAttribute>(entity =>
            {
                entity.ToTable("BodyAttribute");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.ProfileBodyId).HasMaxLength(30);

                entity.Property(e => e.Unit).HasMaxLength(10);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.BodyAttributes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__BodyAttri__BodyS__01D345B0");

                entity.HasOne(d => d.ProfileBody)
                    .WithMany(p => p.BodyAttributes)
                    .HasForeignKey(d => d.ProfileBodyId)
                    .HasConstraintName("FK__BodyAttri__Profi__00DF2177");
            });

            modelBuilder.Entity<BodySize>(entity =>
            {
                entity.ToTable("BodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodyIndex).HasDefaultValueSql("((0))");

                entity.Property(e => e.BodyPart).HasMaxLength(50);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.GuideVideoLink).HasColumnType("text");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaxValidValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.MinValidValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Catalog>(entity =>
            {
                entity.ToTable("Catalog");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CategoryId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(2550);

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 0)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UrlPath).HasMaxLength(255);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Catalogs)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Catalog__Categor__0880433F");
            });

            modelBuilder.Entity<CatalogBodySize>(entity =>
            {
                entity.ToTable("CatalogBodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CatalogId).HasMaxLength(30);

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.CatalogBodySizes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__CatalogBo__BodyS__0E391C95");

                entity.HasOne(d => d.Catalog)
                    .WithMany(p => p.CatalogBodySizes)
                    .HasForeignKey(d => d.CatalogId)
                    .HasConstraintName("FK__CatalogBo__Catal__0D44F85C");
            });

            modelBuilder.Entity<CatalogStage>(entity =>
            {
                entity.ToTable("CatalogStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CatalogId).HasMaxLength(30);

                entity.Property(e => e.CatalogStageId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(155);

                entity.Property(e => e.StageNum).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Catalog)
                    .WithMany(p => p.CatalogStages)
                    .HasForeignKey(d => d.CatalogId)
                    .HasConstraintName("FK__CatalogSt__Catal__11158940");

                entity.HasOne(d => d.CatalogStageNavigation)
                    .WithMany(p => p.InverseCatalogStageNavigation)
                    .HasForeignKey(d => d.CatalogStageId)
                    .HasConstraintName("FK__CatalogSt__Catal__1209AD79");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chat");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Chat__CustomerId__56B3DD81");
            });

            modelBuilder.Entity<ChatHistory>(entity =>
            {
                entity.ToTable("ChatHistory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ChatId).HasMaxLength(30);

                entity.Property(e => e.FromCus).HasDefaultValueSql("((1))");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.Message).HasMaxLength(500);

                entity.Property(e => e.ReadTime).HasColumnType("datetime");

                entity.Property(e => e.SendTime).HasColumnType("datetime");

                entity.Property(e => e.StaffReply).HasMaxLength(30);

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.ChatHistories)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK__ChatHisto__ChatI__5A846E65");

                entity.HasOne(d => d.StaffReplyNavigation)
                    .WithMany(p => p.ChatHistories)
                    .HasForeignKey(d => d.StaffReply)
                    .HasConstraintName("FK__ChatHisto__Staff__5B78929E");
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.ToTable("Component");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CatalogId).HasMaxLength(30);

                entity.Property(e => e.ComponentTypeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Catalog)
                    .WithMany(p => p.Components)
                    .HasForeignKey(d => d.CatalogId)
                    .HasConstraintName("FK__Component__Catal__39237A9A");

                entity.HasOne(d => d.ComponentType)
                    .WithMany(p => p.Components)
                    .HasForeignKey(d => d.ComponentTypeId)
                    .HasConstraintName("FK__Component__Compo__382F5661");
            });

            modelBuilder.Entity<ComponentType>(entity =>
            {
                entity.ToTable("ComponentType");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CatalogStageId).HasMaxLength(30);

                entity.Property(e => e.CategoryId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.CatalogStage)
                    .WithMany(p => p.ComponentTypes)
                    .HasForeignKey(d => d.CatalogStageId)
                    .HasConstraintName("FK__Component__Catal__345EC57D");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ComponentTypes)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Component__Categ__336AA144");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Avatar).HasColumnType("text");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.EmailVerified).HasDefaultValueSql("((0))");

                entity.Property(e => e.Fullname).HasMaxLength(100);

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.SecrectKeyLogin).HasMaxLength(20);

                entity.Property(e => e.Otpnumber)
                    .HasMaxLength(10)
                    .HasColumnName("OTPNumber");

                entity.Property(e => e.OtptimeLimit)
                    .HasColumnType("datetime")
                    .HasColumnName("OTPTimeLimit");

                entity.Property(e => e.Otpused)
                    .HasColumnName("OTPUsed")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.Phone).HasMaxLength(10);

                entity.Property(e => e.PhoneVerified).HasDefaultValueSql("((0))");

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            modelBuilder.Entity<CustomerClient>(entity =>
            {
                entity.ToTable("CustomerClient");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ClientToken).HasMaxLength(255);

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.IpAddress).HasMaxLength(30);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerClients)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__CustomerC__Custo__6E8B6712");
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discount");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Code).HasMaxLength(30);

                entity.Property(e => e.ConditionPriceMax).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.ConditionPriceMin).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("Material");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialCategoryId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Unit).HasMaxLength(10);

                entity.HasOne(d => d.MaterialCategory)
                    .WithMany(p => p.Materials)
                    .HasForeignKey(d => d.MaterialCategoryId)
                    .HasConstraintName("FK__Material__Materi__43A1090D");
            });

            modelBuilder.Entity<MaterialCategory>(entity =>
            {
                entity.ToTable("MaterialCategory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialTypeId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.MaterialType)
                    .WithMany(p => p.MaterialCategories)
                    .HasForeignKey(d => d.MaterialTypeId)
                    .HasConstraintName("FK__MaterialC__Mater__3FD07829");
            });

            modelBuilder.Entity<MaterialType>(entity =>
            {
                entity.ToTable("MaterialType");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.ReadTime).HasColumnType("datetime");

                entity.Property(e => e.SendTime).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Notificat__Custo__61316BF4");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.AfterDiscountPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.ApproveTime).HasColumnType("datetime");

                entity.Property(e => e.Approver).HasMaxLength(30);

                entity.Property(e => e.CancelTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.Deposit).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountCode).HasMaxLength(30);

                entity.Property(e => e.DiscountId).HasMaxLength(30);

                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsApproved).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.PaidMoney).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.PayDeposit).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18, 0)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.TotalProduct).HasDefaultValueSql("((0))");

                entity.Property(e => e.UnPaidMoney).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.ApproverNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.Approver)
                    .HasConstraintName("FK__Order__Approver__1A9EF37A");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Order__CustomerI__19AACF41");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK__Order__DiscountI__1B9317B3");
            });

            modelBuilder.Entity<OrderMaterial>(entity =>
            {
                entity.ToTable("OrderMaterial");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ApproveTime).HasColumnType("datetime");

                entity.Property(e => e.Approver).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsApproved).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialId).HasMaxLength(30);

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.HasOne(d => d.ApproverNavigation)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.Approver)
                    .HasConstraintName("FK__OrderMate__Appro__4E1E9780");

                entity.HasOne(d => d.Material)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__OrderMate__Mater__4C364F0E");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderMate__Order__4D2A7347");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CatalogId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.StatusPercent)
                    .HasColumnType("decimal(18, 0)")
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Catalog)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CatalogId)
                    .HasConstraintName("FK__Product__Catalog__251C81ED");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__Product__OrderId__24285DB4");
            });

            modelBuilder.Entity<ProductBodySize>(entity =>
            {
                entity.ToTable("ProductBodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.Unit).HasMaxLength(10);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.ProductBodySizes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__ProductBo__BodyS__6ABAD62E");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductBodySizes)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductBo__Produ__69C6B1F5");
            });

            modelBuilder.Entity<ProductComponent>(entity =>
            {
                entity.ToTable("ProductComponent");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ComponentId).HasMaxLength(30);

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductStageId).HasMaxLength(30);

                entity.HasOne(d => d.Component)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.ComponentId)
                    .HasConstraintName("FK__ProductCo__Compo__477199F1");

                entity.HasOne(d => d.Material)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__ProductCo__Mater__4959E263");

                entity.HasOne(d => d.ProductStage)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.ProductStageId)
                    .HasConstraintName("FK__ProductCo__Produ__4865BE2A");
            });

            modelBuilder.Entity<ProductStage>(entity =>
            {
                entity.ToTable("ProductStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CatalogStageId).HasMaxLength(30);

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.FinishTime).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.HasOne(d => d.CatalogStage)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.CatalogStageId)
                    .HasConstraintName("FK__ProductSt__Catal__2BC97F7C");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductSt__Produ__2CBDA3B5");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__ProductSt__Staff__2AD55B43");
            });

            modelBuilder.Entity<ProfileBody>(entity =>
            {
                entity.ToTable("ProfileBody");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ProfileBodies)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__ProfileBo__Custo__7755B73D");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.ProfileBodies)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__ProfileBo__Staff__7849DB76");
            });

            modelBuilder.Entity<Skill>(entity =>
            {
                entity.ToTable("Skill");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(55);
            });

            modelBuilder.Entity<SkillForStage>(entity =>
            {
                entity.ToTable("SkillForStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ProductStageId).HasMaxLength(30);

                entity.Property(e => e.SkillId).HasMaxLength(30);

                entity.HasOne(d => d.ProductStage)
                    .WithMany(p => p.SkillForStages)
                    .HasForeignKey(d => d.ProductStageId)
                    .HasConstraintName("FK__SkillForS__Produ__308E3499");

                entity.HasOne(d => d.Skill)
                    .WithMany(p => p.SkillForStages)
                    .HasForeignKey(d => d.SkillId)
                    .HasConstraintName("FK__SkillForS__Skill__2F9A1060");
            });

            modelBuilder.Entity<SkillOfStaff>(entity =>
            {
                entity.ToTable("SkillOfStaff");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.SkillId).HasMaxLength(30);

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.HasOne(d => d.Skill)
                    .WithMany(p => p.SkillOfStaffs)
                    .HasForeignKey(d => d.SkillId)
                    .HasConstraintName("FK__SkillOfSt__Skill__6DCC4D03");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.SkillOfStaffs)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__SkillOfSt__Staff__6EC0713C");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Currency).HasMaxLength(30);

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.Platform).HasMaxLength(50);

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.Property(e => e.TransactionTime).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__Transacti__Order__52E34C9D");
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staff");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Avatar).HasColumnType("text");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Fullname).HasMaxLength(100);

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.Phone).HasMaxLength(10);

                entity.Property(e => e.Role).HasDefaultValueSql("((2))");

                entity.Property(e => e.Username).HasMaxLength(255);

                entity.Property(e => e.SecrectKeyLogin).HasMaxLength(20);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
