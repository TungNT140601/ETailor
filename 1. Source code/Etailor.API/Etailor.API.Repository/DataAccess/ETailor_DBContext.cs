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
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<ChatHistory> ChatHistories { get; set; } = null!;
        public virtual DbSet<ComponentStyle> ComponentStyles { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Discount> Discounts { get; set; } = null!;
        public virtual DbSet<Material> Materials { get; set; } = null!;
        public virtual DbSet<MaterialCategory> MaterialCategories { get; set; } = null!;
        public virtual DbSet<MaterialForComponent> MaterialForComponents { get; set; } = null!;
        public virtual DbSet<MaterialType> MaterialTypes { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<OrderMaterial> OrderMaterials { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductBodySize> ProductBodySizes { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<ProductComponent> ProductComponents { get; set; } = null!;
        public virtual DbSet<ProductStage> ProductStages { get; set; } = null!;
        public virtual DbSet<ProductStep> ProductSteps { get; set; } = null!;
        public virtual DbSet<ProfileBodyAttribute> ProfileBodyAttributes { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<TransactionType> TransactionTypes { get; set; } = null!;
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

                entity.Property(e => e.Creater).HasMaxLength(30);

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.Property(e => e.UrlPath).HasMaxLength(255);

                entity.HasOne(d => d.CreaterNavigation)
                    .WithMany(p => p.Blogs)
                    .HasForeignKey(d => d.Creater)
                    .HasConstraintName("FK__Blog__Creater__61F08603");
            });

            modelBuilder.Entity<BodyAttribute>(entity =>
            {
                entity.ToTable("BodyAttribute");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Measure).HasMaxLength(10);

                entity.Property(e => e.ProfileBodyAttributeId).HasMaxLength(30);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.BodyAttributes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__BodyAttri__BodyS__7CD98669");

                entity.HasOne(d => d.ProfileBodyAttribute)
                    .WithMany(p => p.BodyAttributes)
                    .HasForeignKey(d => d.ProfileBodyAttributeId)
                    .HasConstraintName("FK__BodyAttri__Profi__7BE56230");
            });

            modelBuilder.Entity<BodySize>(entity =>
            {
                entity.ToTable("BodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.GuideVideoLink).HasColumnType("text");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaxValidValue).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.MinValidValue).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chat");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Chat__CustomerId__52AE4273");
            });

            modelBuilder.Entity<ChatHistory>(entity =>
            {
                entity.ToTable("ChatHistory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ChatId).HasMaxLength(30);

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.FromCus).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.Message).HasMaxLength(255);

                entity.Property(e => e.ReadTime).HasColumnType("datetime");

                entity.Property(e => e.SendTime).HasColumnType("datetime");

                entity.Property(e => e.StaffReply).HasMaxLength(30);

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.ChatHistories)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK__ChatHisto__ChatI__567ED357");

                entity.HasOne(d => d.StaffReplyNavigation)
                    .WithMany(p => p.ChatHistories)
                    .HasForeignKey(d => d.StaffReply)
                    .HasConstraintName("FK__ChatHisto__Staff__58671BC9");
            });

            modelBuilder.Entity<ComponentStyle>(entity =>
            {
                entity.ToTable("ComponentStyle");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductComponentId).HasMaxLength(30);

                entity.HasOne(d => d.ProductComponent)
                    .WithMany(p => p.ComponentStyles)
                    .HasForeignKey(d => d.ProductComponentId)
                    .HasConstraintName("FK__Component__Produ__1B5E0D89");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Avatar).HasColumnType("text");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.EmailVerified).HasDefaultValueSql("((0))");

                entity.Property(e => e.Fullname).HasMaxLength(100);

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Otp)
                    .HasMaxLength(10)
                    .HasColumnName("OTP");

                entity.Property(e => e.OtpexpireTime)
                    .HasColumnType("datetime")
                    .HasColumnName("OTPExpireTime");

                entity.Property(e => e.Otpused)
                    .HasColumnName("OTPUsed")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.Phone).HasMaxLength(10);

                entity.Property(e => e.PhoneVerified).HasDefaultValueSql("((0))");

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discount");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Code).HasMaxLength(30);

                entity.Property(e => e.ConditionPriceMax).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ConditionPriceMin).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.DiscountPrice).HasMaxLength(30);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("Material");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialCategoryId).HasMaxLength(30);

                entity.Property(e => e.Measure).HasMaxLength(10);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.MaterialCategory)
                    .WithMany(p => p.Materials)
                    .HasForeignKey(d => d.MaterialCategoryId)
                    .HasConstraintName("FK__Material__Materi__26CFC035");
            });

            modelBuilder.Entity<MaterialCategory>(entity =>
            {
                entity.ToTable("MaterialCategory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Handled).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialTypeId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.MaterialType)
                    .WithMany(p => p.MaterialCategories)
                    .HasForeignKey(d => d.MaterialTypeId)
                    .HasConstraintName("FK__MaterialC__Mater__220B0B18");
            });

            modelBuilder.Entity<MaterialForComponent>(entity =>
            {
                entity.ToTable("MaterialForComponent");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialId).HasMaxLength(30);

                entity.Property(e => e.OrderMaterialId).HasMaxLength(30);

                entity.Property(e => e.ProductComponentId).HasMaxLength(30);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Material)
                    .WithMany(p => p.MaterialForComponents)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__MaterialF__Mater__3EA749C6");

                entity.HasOne(d => d.OrderMaterial)
                    .WithMany(p => p.MaterialForComponents)
                    .HasForeignKey(d => d.OrderMaterialId)
                    .HasConstraintName("FK__MaterialF__Order__3F9B6DFF");

                entity.HasOne(d => d.ProductComponent)
                    .WithMany(p => p.MaterialForComponents)
                    .HasForeignKey(d => d.ProductComponentId)
                    .HasConstraintName("FK__MaterialF__Produ__408F9238");
            });

            modelBuilder.Entity<MaterialType>(entity =>
            {
                entity.ToTable("MaterialType");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.ReadTime).HasColumnType("datetime");

                entity.Property(e => e.SendTime).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Notificat__Custo__5D2BD0E6");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.AfterDiscountPrice).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ApproveTime).HasColumnType("datetime");

                entity.Property(e => e.Approver).HasMaxLength(30);

                entity.Property(e => e.BuyTime).HasColumnType("datetime");

                entity.Property(e => e.CancelTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Deposit).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.DiscountCode).HasMaxLength(30);

                entity.Property(e => e.DiscountId).HasMaxLength(30);

                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.IsApproved).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsBuy).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsCancel).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.PaidMoney).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.PayDeposit).HasDefaultValueSql("((0))");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.UnPaidMoney).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.ApproverNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.Approver)
                    .HasConstraintName("FK__Order__Approver__314D4EA8");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Order__CustomerI__2D7CBDC4");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK__Order__DiscountI__2E70E1FD");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.ProfileBodyAttributeId).HasMaxLength(30);

                entity.Property(e => e.Status).HasMaxLength(30);

                entity.Property(e => e.StatusPercent).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderDeta__Order__4460231C");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__OrderDeta__Produ__45544755");

                entity.HasOne(d => d.ProfileBodyAttribute)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProfileBodyAttributeId)
                    .HasConstraintName("FK__OrderDeta__Profi__46486B8E");
            });

            modelBuilder.Entity<OrderMaterial>(entity =>
            {
                entity.ToTable("OrderMaterial");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.ApproveTime).HasColumnType("datetime");

                entity.Property(e => e.Approver).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.InProcess).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsApproved).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MaterialCategoryId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.ApproverNavigation)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.Approver)
                    .HasConstraintName("FK__OrderMate__Appro__3AD6B8E2");

                entity.HasOne(d => d.MaterialCategory)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.MaterialCategoryId)
                    .HasConstraintName("FK__OrderMate__Mater__37FA4C37");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderMaterials)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderMate__Order__370627FE");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Image).HasColumnType("text");

                entity.Property(e => e.IsCustomize).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ProductCategoryId).HasMaxLength(30);

                entity.Property(e => e.UrlPath).HasMaxLength(255);

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .HasConstraintName("FK__Product__Product__038683F8");
            });

            modelBuilder.Entity<ProductBodySize>(entity =>
            {
                entity.ToTable("ProductBodySize");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.BodySizeId).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Measure).HasMaxLength(10);

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.BodySize)
                    .WithMany(p => p.ProductBodySizes)
                    .HasForeignKey(d => d.BodySizeId)
                    .HasConstraintName("FK__ProductBo__BodyS__093F5D4E");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductBodySizes)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductBo__Produ__084B3915");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<ProductComponent>(entity =>
            {
                entity.ToTable("ProductComponent");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.ProductStepId).HasMaxLength(30);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductCo__Produ__1699586C");

                entity.HasOne(d => d.ProductStep)
                    .WithMany(p => p.ProductComponents)
                    .HasForeignKey(d => d.ProductStepId)
                    .HasConstraintName("FK__ProductCo__Produ__178D7CA5");
            });

            modelBuilder.Entity<ProductStage>(entity =>
            {
                entity.ToTable("ProductStage");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MakerId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductId).HasMaxLength(30);

                entity.Property(e => e.StageProcess).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Maker)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.MakerId)
                    .HasConstraintName("FK__ProductSt__Maker__0E04126B");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductStages)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductSt__Produ__0D0FEE32");
            });

            modelBuilder.Entity<ProductStep>(entity =>
            {
                entity.ToTable("ProductStep");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.FinishedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsFinish).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ProductStageId).HasMaxLength(30);

                entity.HasOne(d => d.ProductStage)
                    .WithMany(p => p.ProductSteps)
                    .HasForeignKey(d => d.ProductStageId)
                    .HasConstraintName("FK__ProductSt__Produ__11D4A34F");
            });

            modelBuilder.Entity<ProfileBodyAttribute>(entity =>
            {
                entity.ToTable("ProfileBodyAttribute");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.CustomerId).HasMaxLength(30);

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsByStaff).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.MakerId).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ProfileBodyAttributes)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__ProfileBo__Custo__725BF7F6");

                entity.HasOne(d => d.Maker)
                    .WithMany(p => p.ProfileBodyAttributes)
                    .HasForeignKey(d => d.MakerId)
                    .HasConstraintName("FK__ProfileBo__Maker__753864A1");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name).HasMaxLength(55);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Currency).HasMaxLength(30);

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsSuccess).HasDefaultValueSql("((0))");

                entity.Property(e => e.OrderId).HasMaxLength(30);

                entity.Property(e => e.Platform).HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(30);

                entity.Property(e => e.TransactionTime).HasColumnType("datetime");

                entity.Property(e => e.TransactionTypeId).HasMaxLength(30);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__Transacti__Order__4CF5691D");

                entity.HasOne(d => d.TransactionType)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.TransactionTypeId)
                    .HasConstraintName("FK__Transacti__Trans__4DE98D56");
            });

            modelBuilder.Entity<TransactionType>(entity =>
            {
                entity.ToTable("TransactionType");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staff");

                entity.Property(e => e.Id).HasMaxLength(30);

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Avatar).HasColumnType("text");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DeletedTime).HasColumnType("datetime");

                entity.Property(e => e.Fullname).HasMaxLength(100);

                entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastestUpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.Phone).HasMaxLength(10);

                entity.Property(e => e.RoleId).HasMaxLength(30);

                entity.Property(e => e.Username).HasMaxLength(255);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Staff)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__Staff__RoleId__68D28DBC");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
