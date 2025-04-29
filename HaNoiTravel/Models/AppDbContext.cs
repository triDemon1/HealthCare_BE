using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Bookingstatus> Bookingstatuses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderdetail> Orderdetails { get; set; }

    public virtual DbSet<Orderstatus> Orderstatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Paymentmethod> Paymentmethods { get; set; }

    public virtual DbSet<Paymentstatus> Paymentstatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Servicegroup> Servicegroups { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Subjecttype> Subjecttypes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionItem> TransactionItems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-JUT8TOT\\SQLEXPRESS;Database=HealthCare;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Addressid).HasName("PK__ADDRESSE__DE5657C1E2A16250");

            entity.ToTable("ADDRESSES");

            entity.Property(e => e.Addressid).HasColumnName("ADDRESSID");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("CITY");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("COUNTRY");
            entity.Property(e => e.Customerid).HasColumnName("CUSTOMERID");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("STREET");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("WARD");

            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ADDRESSES__CUSTO__160F4887");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Bookingid).HasName("PK__BOOKINGS__651D52BD986AF026");

            entity.ToTable("BOOKINGS");

            entity.Property(e => e.Bookingid).HasColumnName("BOOKINGID");
            entity.Property(e => e.Actualendtime)
                .HasPrecision(3)
                .HasColumnName("ACTUALENDTIME");
            entity.Property(e => e.Actualstarttime)
                .HasPrecision(3)
                .HasColumnName("ACTUALSTARTTIME");
            entity.Property(e => e.Addressid).HasColumnName("ADDRESSID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Customerid).HasColumnName("CUSTOMERID");
            entity.Property(e => e.Notes).HasColumnName("NOTES");
            entity.Property(e => e.Priceatbooking)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("PRICEATBOOKING");
            entity.Property(e => e.Scheduledendtime)
                .HasPrecision(3)
                .HasColumnName("SCHEDULEDENDTIME");
            entity.Property(e => e.Scheduledstarttime)
                .HasPrecision(3)
                .HasColumnName("SCHEDULEDSTARTTIME");
            entity.Property(e => e.Serviceid).HasColumnName("SERVICEID");
            entity.Property(e => e.Staffid).HasColumnName("STAFFID");
            entity.Property(e => e.Statusid).HasColumnName("STATUSID");
            entity.Property(e => e.Subjectid).HasColumnName("SUBJECTID");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Address).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Addressid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKINGS__ADDRES__1AD3FDA4");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKINGS__CUSTOM__18EBB532");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Serviceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKINGS__SERVIC__1CBC4616");

            entity.HasOne(d => d.Staff).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("FK__BOOKINGS__STAFFI__1DB06A4F");

            entity.HasOne(d => d.Status).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Statusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKINGS__STATUS__245D67DE");

            entity.HasOne(d => d.Subject).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Subjectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKINGS__SUBJEC__1EA48E88");
        });

        modelBuilder.Entity<Bookingstatus>(entity =>
        {
            entity.HasKey(e => e.Statusid).HasName("PK__BOOKINGS__D135272EE6990F19");

            entity.ToTable("BOOKINGSTATUS");

            entity.HasIndex(e => e.Statusname, "UQ__BOOKINGS__3144C0271E3B25D7").IsUnique();

            entity.Property(e => e.Statusid).HasColumnName("STATUSID");
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasColumnName("STATUSNAME");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("PK__CATEGORI__A50F989605C8918A");

            entity.ToTable("CATEGORIES");

            entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("NAME");
            entity.Property(e => e.Parentcatoregoryid).HasColumnName("PARENTCATOREGORYID");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("PK__CUSTOMER__61DBD7889315C536");

            entity.ToTable("CUSTOMERS");

            entity.Property(e => e.Customerid).HasColumnName("CUSTOMERID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Dateofbirth).HasColumnName("DATEOFBIRTH");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("FIRSTNAME");
            entity.Property(e => e.Gender).HasColumnName("GENDER");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("LASTNAME");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");
            entity.Property(e => e.Userid).HasColumnName("USERID");

            entity.HasOne(d => d.User).WithMany(p => p.Customers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUSTOMERS__USERI__151B244E");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("PK__ORDERS__491E41921DF62C66");

            entity.ToTable("ORDERS");

            entity.Property(e => e.Orderid).HasColumnName("ORDERID");
            entity.Property(e => e.Addressid).HasColumnName("ADDRESSID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Customerid).HasColumnName("CUSTOMERID");
            entity.Property(e => e.Orderdate)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ORDERDATE");
            entity.Property(e => e.Orderstatusid).HasColumnName("ORDERSTATUSID");
            entity.Property(e => e.Totalamount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("TOTALAMOUNT");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Addressid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDERS__ADDRESSI__19DFD96B");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDERS__CUSTOMER__17F790F9");

            entity.HasOne(d => d.Orderstatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Orderstatusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDERS__ORDERSTA__2645B050");
        });

        modelBuilder.Entity<Orderdetail>(entity =>
        {
            entity.HasKey(e => e.Orderdetailid).HasName("PK__ORDERDET__5999A0EDFCA7B58B");

            entity.ToTable("ORDERDETAILS");

            entity.Property(e => e.Orderdetailid).HasColumnName("ORDERDETAILID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Orderid).HasColumnName("ORDERID");
            entity.Property(e => e.Priceatpurchase)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("PRICEATPURCHASE");
            entity.Property(e => e.Productid).HasColumnName("PRODUCTID");
            entity.Property(e => e.Quantity).HasColumnName("QUANTITY");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDERDETA__ORDER__25518C17");

            entity.HasOne(d => d.Product).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDERDETA__PRODU__236943A5");
        });

        modelBuilder.Entity<Orderstatus>(entity =>
        {
            entity.HasKey(e => e.Orderstatusid).HasName("PK__ORDERSTA__D8683934A09FF58D");

            entity.ToTable("ORDERSTATUS");

            entity.HasIndex(e => e.Statusname, "UQ__ORDERSTA__3144C0279CC6210D").IsUnique();

            entity.Property(e => e.Orderstatusid).HasColumnName("ORDERSTATUSID");
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasColumnName("STATUSNAME");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("PK__PAYMENTS__F9599AC8009652D0");

            entity.ToTable("PAYMENTS");

            entity.Property(e => e.Paymentid).HasColumnName("PAYMENTID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Paymentdate)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("PAYMENTDATE");
            entity.Property(e => e.Transactionid).HasColumnName("TRANSACTIONID");
            entity.Property(e => e.TransactionidPaymentGateway)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("TRANSACTIONID_PaymentGateway");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Transactionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENTS__TRANSA__1332DBDC");
        });

        modelBuilder.Entity<Paymentmethod>(entity =>
        {
            entity.HasKey(e => e.Paymentmethodid).HasName("PK__PAYMENTM__889FEE163CD76AD6");

            entity.ToTable("PAYMENTMETHOD");

            entity.HasIndex(e => e.Methodname, "UQ__PAYMENTM__59AC00D8250B63EF").IsUnique();

            entity.Property(e => e.Paymentmethodid).HasColumnName("PAYMENTMETHODID");
            entity.Property(e => e.Methodname)
                .HasMaxLength(100)
                .HasColumnName("METHODNAME");
        });

        modelBuilder.Entity<Paymentstatus>(entity =>
        {
            entity.HasKey(e => e.Paymentstatusid).HasName("PK__PAYMENTS__6F8CB0ACA37C60D9");

            entity.ToTable("PAYMENTSTATUS");

            entity.HasIndex(e => e.Statusname, "UQ__PAYMENTS__3144C0279E2838EF").IsUnique();

            entity.Property(e => e.Paymentstatusid).HasColumnName("PAYMENTSTATUSID");
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasColumnName("STATUSNAME");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Productid).HasName("PK__PRODUCTS__34980AA25D421504");

            entity.ToTable("PRODUCTS");

            entity.HasIndex(e => e.Sku, "UQ__PRODUCTS__CA1ECF0D73A3D158").IsUnique();

            entity.Property(e => e.Productid).HasColumnName("PRODUCTID");
            entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            entity.Property(e => e.Imageurl)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasColumnName("IMAGEURL");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("ISACTIVE");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("NAME");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("PRICE");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.Stockquantity).HasColumnName("STOCKQUANTITY");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTS__CATEGO__208CD6FA");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("PK__ROLES__006568E9EDFFB27D");

            entity.ToTable("ROLES");

            entity.HasIndex(e => e.Rolename, "UQ__ROLES__7E6818972383BBD2").IsUnique();

            entity.Property(e => e.Roleid)
                .ValueGeneratedNever()
                .HasColumnName("ROLEID");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("ROLENAME");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Serviceid).HasName("PK__SERVICES__CE63E076372FD08C");

            entity.ToTable("SERVICES");

            entity.Property(e => e.Serviceid).HasColumnName("SERVICEID");
            entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            entity.Property(e => e.Duration).HasColumnName("DURATION");
            entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("NAME");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("PRICE");
            entity.Property(e => e.Servicegroupid).HasColumnName("SERVICEGROUPID");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SERVICES__CATEGO__2180FB33");

            entity.HasOne(d => d.Servicegroup).WithMany(p => p.Services)
                .HasForeignKey(d => d.Servicegroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SERVICES__SERVIC__1BC821DD");
        });

        modelBuilder.Entity<Servicegroup>(entity =>
        {
            entity.HasKey(e => e.Servicegroupid).HasName("PK__SERVICEG__EDE1FE4778F5A7CC");

            entity.ToTable("SERVICEGROUPS");

            entity.Property(e => e.Servicegroupid).HasColumnName("SERVICEGROUPID");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Staffid).HasName("PK__STAFF__28B5063B047D68C8");

            entity.ToTable("STAFF");

            entity.HasIndex(e => e.Phonenumber, "UQ__STAFF__8F2B07B14093EEDA").IsUnique();

            entity.Property(e => e.Staffid).HasColumnName("STAFFID");
            entity.Property(e => e.Createat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEAT");
            entity.Property(e => e.Expyear).HasColumnName("EXPYEAR");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("FIRSTNAME");
            entity.Property(e => e.Isavailable)
                .HasDefaultValue(true)
                .HasColumnName("ISAVAILABLE");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("LASTNAME");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PHONENUMBER");
            entity.Property(e => e.Skills).HasColumnName("SKILLS");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");
            entity.Property(e => e.Userid).HasColumnName("USERID");

            entity.HasOne(d => d.User).WithMany(p => p.Staff)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__STAFF__USERID__14270015");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Subjectid).HasName("PK__SUBJECTS__C97AA6F5C5B18769");

            entity.ToTable("SUBJECTS");

            entity.Property(e => e.Subjectid).HasColumnName("SUBJECTID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Customerid).HasColumnName("CUSTOMERID");
            entity.Property(e => e.Dateofbirth).HasColumnName("DATEOFBIRTH");
            entity.Property(e => e.Gender).HasColumnName("GENDER");
            entity.Property(e => e.Imageurl)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasColumnName("IMAGEURL");
            entity.Property(e => e.Medicalnotes).HasColumnName("MEDICALNOTES");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("NAME");
            entity.Property(e => e.Typeid).HasColumnName("TYPEID");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Customer).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SUBJECTS__CUSTOM__17036CC0");

            entity.HasOne(d => d.Type).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.Typeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SUBJECTS__TYPEID__2739D489");
        });

        modelBuilder.Entity<Subjecttype>(entity =>
        {
            entity.HasKey(e => e.Typeid).HasName("PK__SUBJECTT__B2802A01AC61E36A");

            entity.ToTable("SUBJECTTYPES");

            entity.HasIndex(e => e.Subjectname, "UQ__SUBJECTT__4CE96D2E0594F016").IsUnique();

            entity.Property(e => e.Typeid).HasColumnName("TYPEID");
            entity.Property(e => e.Subjectname)
                .HasMaxLength(100)
                .HasColumnName("SUBJECTNAME");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__TRANSACT__55433A4B26DE396F");

            entity.ToTable("TRANSACTIONS");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.PaymentMethodId).HasColumnName("PaymentMethodID");
            entity.Property(e => e.PaymentStatusId).HasColumnName("PaymentStatusID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TransactionDate)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TransactionIdPaymentGateway)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("TransactionID_PaymentGateway");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");

            entity.HasOne(d => d.Customer).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TRANSACTI__Custo__0A9D95DB");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TRANSACTI__Payme__0B91BA14");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TRANSACTI__Payme__0C85DE4D");
        });

        modelBuilder.Entity<TransactionItem>(entity =>
        {
            entity.HasKey(e => e.TransactionItemId).HasName("PK__TRANSACT__0D2BBCBAAC878250");

            entity.ToTable("TRANSACTION_ITEMS");

            entity.Property(e => e.TransactionItemId).HasColumnName("TransactionItemID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ItemAmount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

            entity.HasOne(d => d.Booking).WithMany(p => p.TransactionItems)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__TRANSACTI__Booki__114A936A");

            entity.HasOne(d => d.Order).WithMany(p => p.TransactionItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__TRANSACTI__Order__10566F31");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionItems)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TRANSACTI__Trans__0F624AF8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK__USER__7B9E7F3517A092A8");

            entity.ToTable("USER");

            entity.HasIndex(e => e.Email, "UQ__USER__161CF72466A4FA0C").IsUnique();

            entity.HasIndex(e => e.Phonenumber, "UQ__USER__8F2B07B13C7C564C").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__USER__B15BE12E5079C2EC").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("USERID");
            entity.Property(e => e.Createdat)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PASSWORDHASH");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PHONENUMBER");
            entity.Property(e => e.Roleid).HasColumnName("ROLEID");
            entity.Property(e => e.Updatedat)
                .HasPrecision(3)
                .HasColumnName("UPDATEDAT");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("USERNAME");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USER__ROLEID__1F98B2C1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
