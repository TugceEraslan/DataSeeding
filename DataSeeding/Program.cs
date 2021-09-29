using DataSeeding.Data.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataSeeding
{


    //Entity Classes
    //Product(Id,Name,Price) => Product(Id,Name,Price)

    class ShopContext : DbContext
    {
        public DbSet<Product> Products { get; set; }   //Db set e entity class larımızı aldık liste olarak tanımladık
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public static readonly ILoggerFactory MyLoggerFactory =
            LoggerFactory.Create(builder => { builder.AddConsole(); });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)  // hangi database yada provider ile çalışacağımızı belirtiyoruz
        {
            optionsBuilder
                .UseLoggerFactory(MyLoggerFactory)  // Bu satırda ilgili oluşturduğumuz metodu çağırmış oluruz
                                                    // .UseSqlServer(@"Data Source=TUGCEERASLAN-PC;Initial Catalog=ShopDb;Integrated Security=SSPI"); // Yolu bu şekilde vermezsek tabloyu bulamıyor
               .UseMySql(@"server=localhost;port=3306;database=DataSeeding;user=root;password=mysql1234");
            //.UseSqlServer(@"Data Source=TUGCEERASLAN-PC;Initial Catalog=ShopDb;Integrated Security=SSPI"); // Yolu bu şekilde vermezsek tabloyu bulamıyor
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)  // username e göre bir arama yapıyorsak username alanına bir index ve benzersiz ( IsUnique() ) bir index bırakmak faydalı olacaktır
                .IsUnique();


            modelBuilder.Entity<Product>()
                .ToTable("Urunler");

            modelBuilder.Entity<Customer>()
                .Property(p => p.IdentityNumber)
                .HasMaxLength(11)
                .IsRequired();


            modelBuilder.Entity<ProductCategory>()
                .HasKey(t => new { t.ProductId, t.CategoryId });  //Entity<ProductCategory>() bu entity nin 2 tane id si var primary key olarak


            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId);  // ProductCategory tablosunun ProductId si yabancı anahtarı olduğunu belirtiyoruz

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId);
        }
    }

    // One to Many =>
    // mysql de veritabanı oluşturmak için önce migrations oluştur
    // dotnet ef migrations add OneToManyRelation cümleciği ile 
    // sonra oluşan migrationsu veritabanına aktar dotnet ef database update cümleciği ile 

    public static class SeedingData
    {
        public static void Seed(DbContext context)   // Dışardan aldığım parametreyi kullandım
        {
          if(context.Database.GetPendingMigrations().Count()==0)   //  GetPendingMigrations().Count()==0 bekleyen bir migrations yoksa yani bütün migrationslar database aktarılmışsa test verilerini ekleyebiliriz
            {
                if(context is ShopContext)
                {
                    ShopContext _context = context as ShopContext;  // ShopContext i cast ederiz

                    if(_context.Products.Count()==0)
                    {
                        _context.Products.AddRange(Products);
                    }
                    if(_context.Categories.Count()==0)
                    {
                        _context.Categories.AddRange(Categories);
                    }
                }

                context.SaveChanges();

            }
        }

        private static Product[] Products =
        {
            new Product(){Name="Samsung S6", Price=2000},
            new Product(){Name="Samsung S7", Price=3000},
            new Product(){Name="Samsung S8", Price=4000},
            new Product(){Name="Samsung S9", Price=5000}
        };

        private static Category[] Categories =
        {
            new Category(){Name="Telefon"},
            new Category(){Name="Elektronik"},
            new Category(){Name="Bilgisayar"}
        };
    }
    class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(15), MinLength(8)]
        public string Username { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Email { get; set; }
        public Customer Customer { get; set; }  // navigation proporty (Customer ile ilişkilendirme)
        public List<Address> Addresses { get; set; } //Bir kişinin birden fazla adderesi olabilir
    }

    class Customer
    {
        [Column("customer_id")]   // uygulama tarafındaki Id bilgisi database tarafındaki customer_id ye denk gelir
        public int Id { get; set; }

        [Required]
        public string IdentityNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName { get; set; }  //   FullName = FirstName + LastName i uygulamada göstereceğiz ama database de tutmayacak isek NotMapped özelliği veririz. 
        public User User { get; set; }  //navigation proporty (User ile ilişkilendirme)
        public int UserId { get; set; }  // bir kere kullanılacak. Unique olarak işaretlenecek
    }

    class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TaxNumber { get; set; }
    }


    class Address    // User ile Address tablosu arasında bire çok bir ilişki var
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public User User { get; set; }  //navigation proporty
        public int UserId { get; set; }  // int=> null,1,2,3

    }

    // One to One
    // Many to Many

    class Product
    {

        // Primary key(Id,<type_name>Id)
        // Id ye otomatik sayı göndermesini istemiyorsak. Örneğin barkod numarası var ve biz bunu tanımlamak istiyorsak.

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Bir kayıt oluştuğu zaman tek bir kez gelecek ve bir daha değiştirilemeyecek
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime InsertedDate { get; set; } = DateTime.Now;  // Başlangıç tarihi set edilecek ve bir daha değişmeyecek

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]  // Son güncelleme tarihi için değiştirilebilir bir alan olacak 
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now; // O andaki zaman set edilecek ve update olursa tekrar değişebilecek
        public List<ProductCategory> ProductCategories { get; set; }

    }


    class Category
    {
        public int Id { get; set; }


        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
    }


    // [NotMapped]   // ProductCategory Entity sini database içinde bir tablo olarak oluşturmayacak

    [Table("UrunKategorileri")]   //  uygulama tarafındaki ProductCategory database tarafındaki UrunKategorileri tablosuna karşılık gelecek
    class ProductCategory
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

    }


    class Program
    {

        static void Main(string[] args)
        {

            //SeedingData.Seed(new ShopContext());
            
            using(var db= new NorthwindContext())
            {
                var products = db.Products.ToList();   //Products ları liste şeklinde alalım

                foreach (var item in products)
                {
                    Console.WriteLine(item.ProductName);
                }

            }

        }

    }
}
