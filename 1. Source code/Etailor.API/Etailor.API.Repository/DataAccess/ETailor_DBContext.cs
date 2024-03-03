using System;
using System.Collections;
using System.Collections.Generic;
using Etailor.API.Repository.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<ChatHistory> ChatHistories { get; set; } = null!;
        public virtual DbSet<Component> Components { get; set; } = null!;
        public virtual DbSet<ComponentStage> ComponentStages { get; set; } = null!;
        public virtual DbSet<ComponentType> ComponentTypes { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<CustomerClient> CustomerClients { get; set; } = null!;
        public virtual DbSet<Discount> Discounts { get; set; } = null!;
        public virtual DbSet<Mastery> Masteries { get; set; } = null!;
        public virtual DbSet<Material> Materials { get; set; } = null!;
        public virtual DbSet<MaterialCategory> MaterialCategories { get; set; } = null!;
        public virtual DbSet<MaterialType> MaterialTypes { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderMaterial> OrderMaterials { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductBodySize> ProductBodySizes { get; set; } = null!;
        public virtual DbSet<ProductComponent> ProductComponents { get; set; } = null!;
        public virtual DbSet<ProductComponentMaterial> ProductComponentMaterials { get; set; } = null!;
        public virtual DbSet<ProductStage> ProductStages { get; set; } = null!;
        public virtual DbSet<ProductTemplate> ProductTemplates { get; set; } = null!;
        public virtual DbSet<ProfileBody> ProfileBodies { get; set; } = null!;
        public virtual DbSet<TemplateBodySize> TemplateBodySizes { get; set; } = null!;
        public virtual DbSet<TemplateStage> TemplateStages { get; set; } = null!;
        public virtual DbSet<Staff> Staff { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=tungnt-dbcloud.database.windows.net;uid=tungnt;pwd=123456789aA@;database=ETailor_DB;TrustServerCertificate=True;", b => b.MigrationsAssembly("Etailor.API.WebAPI"));
            optionsBuilder.EnableSensitiveDataLogging();
            //            if (!optionsBuilder.IsConfigured)
            //            {
            //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
            //                optionsBuilder.UseSqlServer("server=tungnt-dbcloud.database.windows.net;uid=tungnt;pwd=123456789aA@;database=ETailor_DB;TrustServerCertificate=True;");
            //            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable("Blog");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.Property(e => e.Hastag).HasMaxLength(255);

                entity.Property(e => e.Thumbnail).HasColumnType("text");

                entity.Property(e => e.UrlPath).HasMaxLength(255);

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Blogs)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__Blog__StaffId__22951AFD");
            });

            modelBuilder.Entity<BodyAttribute>(entity =>
            {
                entity.ToTable("BodyAttribute");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.ProfileBodyId).HasMaxLength(30);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.BodyAttributes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__BodyAttri__BodyS__3D7E1B63");

                entity.HasOne(d => d.ProfileBody)
                    .WithMany(p => p.BodyAttributes)
                    .HasForeignKey(d => d.ProfileBodyId)
                    .HasConstraintName("FK__BodyAttri__Profi__3C89F72A");
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

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaxValidValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.MinValidValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

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

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Chat__CustomerId__125EB334");
            });

            modelBuilder.Entity<ChatHistory>(entity =>
            {
                entity.ToTable("ChatHistory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ChatId).HasMaxLength(30);

                entity.Property(e => e.FromCus).HasDefaultValueSql("((1))");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.Message).HasMaxLength(500);

                entity.Property(e => e.ReadTime).HasColumnType("datetime");

                entity.Property(e => e.ReplierId).HasMaxLength(30);

                entity.Property(e => e.SendTime).HasColumnType("datetime");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.ChatHistories)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK__ChatHisto__ChatI__162F4418");

                entity.HasOne(d => d.Replier)
                    .WithMany(p => p.ChatHistories)
                    .HasForeignKey(d => d.ReplierId)
                    .HasConstraintName("FK__ChatHisto__Repli__17236851");
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.ToTable("Component");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ComponentTypeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.Index).HasColumnType("int");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.Default).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductTemplateId).HasMaxLength(30);

                entity.HasOne(d => d.ComponentType)
                    .WithMany(p => p.Components)
                    .HasForeignKey(d => d.ComponentTypeId)
                    .HasConstraintName("FK__Component__Compo__5A1A5A11");

                entity.HasOne(d => d.ProductTemplate)
                    .WithMany(p => p.Components)
                    .HasForeignKey(d => d.ProductTemplateId)
                    .HasConstraintName("FK__Component__Produ__5B0E7E4A");
            });

            modelBuilder.Entity<ComponentStage>(entity =>
            {
                entity.ToTable("ComponentStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ComponentTypeId).HasMaxLength(30);

                entity.Property(e => e.TemplateStageId).HasMaxLength(30);

                entity.HasOne(d => d.ComponentType)
                    .WithMany(p => p.ComponentStages)
                    .HasForeignKey(d => d.ComponentTypeId)
                    .HasConstraintName("FK__Component__Compo__5649C92D");

                entity.HasOne(d => d.TemplateStage)
                    .WithMany(p => p.ComponentStages)
                    .HasForeignKey(d => d.TemplateStageId)
                    .HasConstraintName("FK__Component__Templ__573DED66");
            });

            modelBuilder.Entity<ComponentType>(entity =>
            {
                entity.ToTable("ComponentType");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CategoryId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ComponentTypes)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Component__Categ__52793849");
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

                entity.Property(e => e.Gender).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

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

                entity.Property(e => e.SecrectKeyLogin).HasMaxLength(20);

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
                    .HasConstraintName("FK__CustomerC__Custo__2B2A60FE");
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discount");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Code).HasMaxLength(30);

                entity.Property(e => e.ConditionPriceMax).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.ConditionPriceMin).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DiscountPercent).HasColumnType("float(18, 0)");

                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Mastery>(entity =>
            {
                entity.ToTable("Mastery");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CategoryId).HasMaxLength(30);

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Masteries)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Mastery__Categor__2E06CDA9");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Masteries)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__Mastery__StaffId__2EFAF1E2");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("Material");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialCategoryId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.MaterialCategory)
                    .WithMany(p => p.Materials)
                    .HasForeignKey(d => d.MaterialCategoryId)
                    .HasConstraintName("FK__Material__Materi__004002F9");
            });

            modelBuilder.Entity<MaterialCategory>(entity =>
            {
                entity.ToTable("MaterialCategory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialTypeId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.PricePerUnit).HasColumnType("decimal");

                entity.HasOne(d => d.MaterialType)
                    .WithMany(p => p.MaterialCategories)
                    .HasForeignKey(d => d.MaterialTypeId)
                    .HasConstraintName("FK__MaterialC__Mater__7C6F7215");
            });

            modelBuilder.Entity<MaterialType>(entity =>
            {
                entity.ToTable("MaterialType");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Unit).HasMaxLength(10);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.ReadTime).HasColumnType("datetime");

                entity.Property(e => e.SendTime).HasColumnType("datetime");

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Notificat__Custo__1CDC41A7");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__Notificat__Staff__1DD065E0");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.AfterDiscountPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.ApproveTime).HasColumnType("datetime");

                entity.Property(e => e.CancelTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CreaterId).HasMaxLength(30);

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.Deposit).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountCode).HasMaxLength(30);

                entity.Property(e => e.DiscountId).HasMaxLength(30);

                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.PaidMoney).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.PayDeposit).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18, 0)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.TotalProduct).HasDefaultValueSql("((0))");

                entity.Property(e => e.UnPaidMoney).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Creater)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CreaterId)
                    .HasConstraintName("FK__Order__CreaterId__62AFA012");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Order__CustomerI__61BB7BD9");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK__Order__DiscountI__63A3C44B");
            });

            modelBuilder.Entity<OrderMaterial>(entity =>
            {
                entity.ToTable("OrderMaterial");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsCusMaterial).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialId).HasMaxLength(30);

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(18, 3)")
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Material)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__OrderMate__Mater__08D548FA");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderMate__Order__09C96D33");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.AmountAfterRefund).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.PaymentRefundId).HasMaxLength(30);

                entity.Property(e => e.PayTime).HasColumnType("datetime");

                entity.Property(e => e.Platform).HasMaxLength(50);

                entity.Property(e => e.PayType).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__Payment__OrderId__0E8E2250");

                entity.HasOne(d => d.PaymentRefund)
                    .WithMany(d => d.RefundOfPayments)
                    .HasForeignKey(d => d.PaymentRefundId)
                    .HasConstraintName("FK__Payment__PaymentRefund__1A25A48");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.EvidenceImage).HasColumnType("text");

                entity.Property(e => e.SaveOrderComponents).HasColumnType("text");

                entity.Property(e => e.FinishTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.FabricMaterialId).HasMaxLength(30);

                entity.Property(e => e.ReferenceProfileBodyId).HasMaxLength(30);

                entity.Property(e => e.ProductTemplateId).HasMaxLength(30);

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 0)")
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__Product__OrderId__6B44E613");

                entity.HasOne(d => d.ReferenceProfileBody)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ReferenceProfileBodyId)
                    .HasConstraintName("FK__Product__ProfileBody__6B44E613");

                entity.HasOne(d => d.FabricMaterial)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.FabricMaterialId)
                    .HasConstraintName("FK__Product__Fabric_Material__6B44E613");

                entity.HasOne(d => d.ProductTemplate)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductTemplateId)
                    .HasConstraintName("FK__Product__Product__6C390A4C");
            });

            modelBuilder.Entity<ProductBodySize>(entity =>
            {
                entity.ToTable("ProductBodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.ProductBodySizes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__ProductBo__BodyS__2759D01A");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductBodySizes)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductBo__Produ__2665ABE1");
            });

            modelBuilder.Entity<ProductComponent>(entity =>
            {
                entity.ToTable("ProductComponent");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ComponentId).HasMaxLength(30);

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductStageId).HasMaxLength(30);

                entity.HasOne(d => d.Component)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.ComponentId)
                    .HasConstraintName("FK__ProductCo__Compo__041093DD");

                entity.HasOne(d => d.ProductStage)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.ProductStageId)
                    .HasConstraintName("FK__ProductCo__Produ__0504B816");
            });

            modelBuilder.Entity<ProductComponentMaterial>(entity =>
            {
                entity.ToTable("ProductComponentMaterial");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.MaterialId).HasMaxLength(30);

                entity.Property(e => e.ProductComponentId).HasMaxLength(30);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Material)
                    .WithMany(p => p.ProductComponentMaterials)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__ProductCo__Mater__32CB82C6");

                entity.HasOne(d => d.ProductComponent)
                    .WithMany(p => p.ProductComponentMaterials)
                    .HasForeignKey(d => d.ProductComponentId)
                    .HasConstraintName("FK__ProductCo__Produ__31D75E8D");
            });

            modelBuilder.Entity<ProductStage>(entity =>
            {
                entity.ToTable("ProductStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.FinishTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.Property(e => e.StageNum).HasDefaultValueSql("((0))");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.Property(e => e.TaskIndex).HasDefaultValueSql("((0))");

                entity.Property(e => e.TemplateStageId).HasMaxLength(30);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductSt__Produ__72E607DB");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__ProductSt__Staff__70FDBF69");

                entity.HasOne(d => d.TemplateStage)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.TemplateStageId)
                    .HasConstraintName("FK__ProductSt__Templ__71F1E3A2");
            });

            modelBuilder.Entity<ProductTemplate>(entity =>
            {
                entity.ToTable("ProductTemplate");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CategoryId).HasMaxLength(30);

                entity.Property(e => e.CollectionImage).HasColumnType("text");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(2550);

                entity.Property(e => e.ThumbnailImage).HasColumnType("text");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 0)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UrlPath).HasMaxLength(255);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ProductTemplates)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductTe__Categ__442B18F2");
            });

            modelBuilder.Entity<ProfileBody>(entity =>
            {
                entity.ToTable("ProfileBody");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.StaffId).HasMaxLength(30);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ProfileBodies)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__ProfileBo__Custo__33008CF0");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.ProfileBodies)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__ProfileBo__Staff__33F4B129");
            });

            modelBuilder.Entity<TemplateBodySize>(entity =>
            {
                entity.ToTable("TemplateBodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.ProductTemplateId).HasMaxLength(30);

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.TemplateBodySizes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__TemplateB__BodyS__49E3F248");

                entity.HasOne(d => d.ProductTemplate)
                    .WithMany(p => p.TemplateBodySizes)
                    .HasForeignKey(d => d.ProductTemplateId)
                    .HasConstraintName("FK__TemplateB__Produ__48EFCE0F");
            });

            modelBuilder.Entity<TemplateStage>(entity =>
            {
                entity.ToTable("TemplateStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.InactiveTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(155);

                entity.Property(e => e.ProductTemplateId).HasMaxLength(30);

                entity.Property(e => e.StageNum).HasDefaultValueSql("((0))");

                entity.Property(e => e.TemplateStageId).HasMaxLength(30);

                entity.HasOne(d => d.ProductTemplate)
                    .WithMany(p => p.TemplateStages)
                    .HasForeignKey(d => d.ProductTemplateId)
                    .HasConstraintName("FK__TemplateS__Produ__4CC05EF3");

                entity.HasOne(d => d.TemplateStageNavigation)
                    .WithMany(p => p.InverseTemplateStageNavigation)
                    .HasForeignKey(d => d.TemplateStageId)
                    .HasConstraintName("FK__TemplateS__Templ__4DB4832C");
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

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastLoginDeviceToken).HasMaxLength(255);

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.Phone).HasMaxLength(10);

                entity.Property(e => e.Role).HasDefaultValueSql("((2))");

                entity.Property(e => e.SecrectKeyLogin).HasMaxLength(20);

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
