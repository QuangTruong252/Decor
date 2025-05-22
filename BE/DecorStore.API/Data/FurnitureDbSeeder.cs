using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Data
{
    public class FurnitureDbSeeder
    {
        private readonly ApplicationDbContext _context;

        public FurnitureDbSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedFurnitureData()
        {
            // Check if we already have furniture data
            if (await _context.Categories.CountAsync() > 5 && await _context.Products.CountAsync() > 20)
            {
                Console.WriteLine("Database already has furniture data. Skipping seeding.");
                return;
            }

            // Clear existing data if needed
            await ClearExistingData();

            // Add main categories
            var categories = new List<Category>
            {
                // Main categories
                new Category { Name = "Living Room", Slug = "living-room", Description = "Furniture for your living space", ImageUrl = "/images/categories/living-room.jpg" },
                new Category { Name = "Bedroom", Slug = "bedroom", Description = "Furniture for your bedroom", ImageUrl = "/images/categories/bedroom.jpg" },
                new Category { Name = "Dining Room", Slug = "dining-room", Description = "Furniture for your dining area", ImageUrl = "/images/categories/dining-room.jpg" },
                new Category { Name = "Home Office", Slug = "home-office", Description = "Furniture for your workspace", ImageUrl = "/images/categories/home-office.jpg" },
                new Category { Name = "Outdoor", Slug = "outdoor", Description = "Furniture for your outdoor spaces", ImageUrl = "/images/categories/outdoor.jpg" }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Add subcategories
            var livingRoomId = categories.First(c => c.Name == "Living Room").Id;
            var bedroomId = categories.First(c => c.Name == "Bedroom").Id;
            var diningRoomId = categories.First(c => c.Name == "Dining Room").Id;
            var homeOfficeId = categories.First(c => c.Name == "Home Office").Id;
            var outdoorId = categories.First(c => c.Name == "Outdoor").Id;

            var subcategories = new List<Category>
            {
                // Living Room subcategories
                new Category { Name = "Sofas", Slug = "sofas", Description = "Comfortable seating for your living room", ParentId = livingRoomId, ImageUrl = "/images/categories/sofas.jpg" },
                new Category { Name = "Coffee Tables", Slug = "coffee-tables", Description = "Stylish tables for your living room", ParentId = livingRoomId, ImageUrl = "/images/categories/coffee-tables.jpg" },
                new Category { Name = "TV Stands", Slug = "tv-stands", Description = "Furniture for your entertainment system", ParentId = livingRoomId, ImageUrl = "/images/categories/tv-stands.jpg" },
                new Category { Name = "Bookshelves", Slug = "bookshelves", Description = "Storage solutions for your books and decor", ParentId = livingRoomId, ImageUrl = "/images/categories/bookshelves.jpg" },

                // Bedroom subcategories
                new Category { Name = "Beds", Slug = "beds", Description = "Comfortable beds for a good night's sleep", ParentId = bedroomId, ImageUrl = "/images/categories/beds.jpg" },
                new Category { Name = "Nightstands", Slug = "nightstands", Description = "Bedside tables for your essentials", ParentId = bedroomId, ImageUrl = "/images/categories/nightstands.jpg" },
                new Category { Name = "Wardrobes", Slug = "wardrobes", Description = "Storage solutions for your clothes", ParentId = bedroomId, ImageUrl = "/images/categories/wardrobes.jpg" },
                new Category { Name = "Dressers", Slug = "dressers", Description = "Stylish storage for your bedroom", ParentId = bedroomId, ImageUrl = "/images/categories/dressers.jpg" },

                // Dining Room subcategories
                new Category { Name = "Dining Tables", Slug = "dining-tables", Description = "Tables for your dining area", ParentId = diningRoomId, ImageUrl = "/images/categories/dining-tables.jpg" },
                new Category { Name = "Dining Chairs", Slug = "dining-chairs", Description = "Comfortable seating for your dining area", ParentId = diningRoomId, ImageUrl = "/images/categories/dining-chairs.jpg" },
                new Category { Name = "Buffets & Sideboards", Slug = "buffets-sideboards", Description = "Storage and display for your dining room", ParentId = diningRoomId, ImageUrl = "/images/categories/buffets.jpg" },

                // Home Office subcategories
                new Category { Name = "Desks", Slug = "desks", Description = "Workspaces for productivity", ParentId = homeOfficeId, ImageUrl = "/images/categories/desks.jpg" },
                new Category { Name = "Office Chairs", Slug = "office-chairs", Description = "Comfortable seating for your workspace", ParentId = homeOfficeId, ImageUrl = "/images/categories/office-chairs.jpg" },
                new Category { Name = "Filing Cabinets", Slug = "filing-cabinets", Description = "Storage solutions for your documents", ParentId = homeOfficeId, ImageUrl = "/images/categories/filing-cabinets.jpg" },

                // Outdoor subcategories
                new Category { Name = "Patio Sets", Slug = "patio-sets", Description = "Complete furniture sets for your outdoor space", ParentId = outdoorId, ImageUrl = "/images/categories/patio-sets.jpg" },
                new Category { Name = "Outdoor Chairs", Slug = "outdoor-chairs", Description = "Comfortable seating for your outdoor area", ParentId = outdoorId, ImageUrl = "/images/categories/outdoor-chairs.jpg" },
                new Category { Name = "Garden Accessories", Slug = "garden-accessories", Description = "Decorative items for your garden", ParentId = outdoorId, ImageUrl = "/images/categories/garden-accessories.jpg" }
            };

            await _context.Categories.AddRangeAsync(subcategories);
            await _context.SaveChangesAsync();

            // Get subcategory IDs for products
            var sofasId = subcategories.First(c => c.Name == "Sofas").Id;
            var coffeeTablesId = subcategories.First(c => c.Name == "Coffee Tables").Id;
            var tvStandsId = subcategories.First(c => c.Name == "TV Stands").Id;
            var bookshelvesId = subcategories.First(c => c.Name == "Bookshelves").Id;

            var bedsId = subcategories.First(c => c.Name == "Beds").Id;
            var nightstandsId = subcategories.First(c => c.Name == "Nightstands").Id;
            var wardrobesId = subcategories.First(c => c.Name == "Wardrobes").Id;
            var dressersId = subcategories.First(c => c.Name == "Dressers").Id;

            var diningTablesId = subcategories.First(c => c.Name == "Dining Tables").Id;
            var diningChairsId = subcategories.First(c => c.Name == "Dining Chairs").Id;
            var buffetsId = subcategories.First(c => c.Name == "Buffets & Sideboards").Id;

            var desksId = subcategories.First(c => c.Name == "Desks").Id;
            var officeChairsId = subcategories.First(c => c.Name == "Office Chairs").Id;
            var filingCabinetsId = subcategories.First(c => c.Name == "Filing Cabinets").Id;

            var patioSetsId = subcategories.First(c => c.Name == "Patio Sets").Id;
            var outdoorChairsId = subcategories.First(c => c.Name == "Outdoor Chairs").Id;
            var gardenAccessoriesId = subcategories.First(c => c.Name == "Garden Accessories").Id;

            // Add products - first part
            var products = new List<Product>();

            // Add sofa products
            AddSofaProducts(products, sofasId);

            // Add coffee table products
            AddCoffeeTableProducts(products, coffeeTablesId);

            // Add TV stand products
            AddTvStandProducts(products, tvStandsId);

            // Add bookshelf products
            AddBookshelfProducts(products, bookshelvesId);

            // Add bed products
            AddBedProducts(products, bedsId);

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Add more products in second batch
            products = new List<Product>();

            // Add nightstand products
            AddNightstandProducts(products, nightstandsId);

            // Add wardrobe products
            AddWardrobeProducts(products, wardrobesId);

            // Add dresser products
            AddDresserProducts(products, dressersId);

            // Add dining table products
            AddDiningTableProducts(products, diningTablesId);

            // Add dining chair products
            AddDiningChairProducts(products, diningChairsId);

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Add final batch of products
            products = new List<Product>();

            // Add buffet products
            AddBuffetProducts(products, buffetsId);

            // Add desk products
            AddDeskProducts(products, desksId);

            // Add office chair products
            AddOfficeChairProducts(products, officeChairsId);

            // Add filing cabinet products
            AddFilingCabinetProducts(products, filingCabinetsId);

            // Add patio set products
            AddPatioSetProducts(products, patioSetsId);

            // Add outdoor chair products
            AddOutdoorChairProducts(products, outdoorChairsId);

            // Add garden accessory products
            AddGardenAccessoryProducts(products, gardenAccessoriesId);

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Add product images
            await AddProductImages();

            Console.WriteLine("Furniture data seeding completed successfully.");
        }

        private async Task ClearExistingData()
        {
            // Remove existing products and categories if needed
            var existingImages = await _context.Images.ToListAsync();
            _context.Images.RemoveRange(existingImages);

            var existingProducts = await _context.Products.ToListAsync();
            _context.Products.RemoveRange(existingProducts);

            var existingCategories = await _context.Categories.ToListAsync();
            _context.Categories.RemoveRange(existingCategories);

            await _context.SaveChangesAsync();
            Console.WriteLine("Cleared existing data.");
        }

        private void AddSofaProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Modern Sectional Sofa",
                    Slug = "modern-sectional-sofa",
                    Description = "Elegant L-shaped sectional sofa with premium fabric upholstery, perfect for contemporary living spaces. Dimensions: 280cm x 180cm x 85cm. Materials: Solid wood frame, high-density foam, premium fabric.",
                    Price = 1299.99m,
                    OriginalPrice = 1499.99m,
                    StockQuantity = 15,
                    SKU = "SOF-MS001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Velvet 3-Seater Sofa",
                    Slug = "velvet-3-seater-sofa",
                    Description = "Luxurious velvet sofa with deep cushions and elegant gold-finished legs. Dimensions: 220cm x 95cm x 85cm. Materials: Velvet upholstery, solid wood frame, gold-finished metal legs.",
                    Price = 899.99m,
                    OriginalPrice = 1099.99m,
                    StockQuantity = 10,
                    SKU = "SOF-VS001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Compact Loveseat",
                    Slug = "compact-loveseat",
                    Description = "Space-saving loveseat perfect for apartments and small living spaces. Dimensions: 150cm x 85cm x 80cm. Materials: Polyester fabric, engineered wood frame, foam padding.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 20,
                    SKU = "SOF-CL001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Modular Sofa System",
                    Slug = "modular-sofa-system",
                    Description = "Customizable modular sofa that can be arranged in multiple configurations to suit your space. Dimensions: Variable. Materials: Premium fabric, high-resilience foam, solid wood base.",
                    Price = 1899.99m,
                    OriginalPrice = 2199.99m,
                    StockQuantity = 8,
                    SKU = "SOF-MS002",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Leather Recliner Sofa",
                    Slug = "leather-recliner-sofa",
                    Description = "Premium leather sofa with reclining function and built-in cup holders. Dimensions: 240cm x 95cm x 90cm. Materials: Genuine leather, engineered wood frame, high-density foam.",
                    Price = 1599.99m,
                    OriginalPrice = 1899.99m,
                    StockQuantity = 12,
                    SKU = "SOF-LR001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddCoffeeTableProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Minimalist Coffee Table",
                    Slug = "minimalist-coffee-table",
                    Description = "Sleek coffee table with tempered glass top and solid oak legs, adding a touch of sophistication to your living room. Dimensions: 120cm x 60cm x 45cm. Materials: Tempered glass, solid oak.",
                    Price = 349.99m,
                    OriginalPrice = 399.99m,
                    StockQuantity = 25,
                    SKU = "TBL-CT001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Marble Top Coffee Table",
                    Slug = "marble-top-coffee-table",
                    Description = "Elegant coffee table featuring a genuine marble top and brass-finished metal base. Dimensions: 100cm x 60cm x 40cm. Materials: Marble, brass-finished metal.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 15,
                    SKU = "TBL-MT001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Nesting Coffee Table Set",
                    Slug = "nesting-coffee-table-set",
                    Description = "Set of 3 nesting tables that can be arranged together or separately as needed. Dimensions: Large: 90cm x 50cm x 45cm, Medium: 70cm x 40cm x 40cm, Small: 50cm x 30cm x 35cm. Materials: Engineered wood, metal frame.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 20,
                    SKU = "TBL-NS001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Storage Coffee Table",
                    Slug = "storage-coffee-table",
                    Description = "Practical coffee table with hidden storage compartments and lift-up top. Dimensions: 110cm x 70cm x 45cm. Materials: Engineered wood, metal hardware.",
                    Price = 399.99m,
                    OriginalPrice = 449.99m,
                    StockQuantity = 18,
                    SKU = "TBL-ST001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Coffee Table",
                    Slug = "industrial-coffee-table",
                    Description = "Rustic coffee table with reclaimed wood top and black metal frame in industrial style. Dimensions: 120cm x 70cm x 45cm. Materials: Reclaimed wood, black metal.",
                    Price = 449.99m,
                    OriginalPrice = 499.99m,
                    StockQuantity = 15,
                    SKU = "TBL-IN001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddTvStandProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Scandinavian TV Stand",
                    Slug = "scandinavian-tv-stand",
                    Description = "Modern TV stand with ample storage space and cable management system, designed in Scandinavian style. Dimensions: 180cm x 45cm x 55cm. Materials: Engineered wood, oak veneer.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 20,
                    SKU = "TVS-SC001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Floating TV Console",
                    Slug = "floating-tv-console",
                    Description = "Wall-mounted TV console that creates a floating effect, perfect for minimalist interiors. Dimensions: 160cm x 40cm x 30cm. Materials: High-quality MDF, matte finish.",
                    Price = 399.99m,
                    OriginalPrice = 449.99m,
                    StockQuantity = 15,
                    SKU = "TVS-FL001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Corner TV Stand",
                    Slug = "corner-tv-stand",
                    Description = "Space-saving corner TV stand designed to fit perfectly in room corners. Dimensions: 120cm x 50cm x 55cm. Materials: Engineered wood, glass shelves.",
                    Price = 349.99m,
                    OriginalPrice = 399.99m,
                    StockQuantity = 18,
                    SKU = "TVS-CR001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Entertainment Center with Fireplace",
                    Slug = "entertainment-center-with-fireplace",
                    Description = "Multifunctional entertainment center featuring an electric fireplace insert for added ambiance. Dimensions: 200cm x 45cm x 60cm. Materials: Engineered wood, tempered glass, electric fireplace insert.",
                    Price = 899.99m,
                    OriginalPrice = 999.99m,
                    StockQuantity = 10,
                    SKU = "TVS-EF001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial TV Cabinet",
                    Slug = "industrial-tv-cabinet",
                    Description = "Rustic TV cabinet with metal accents and distressed wood finish in industrial style. Dimensions: 170cm x 45cm x 50cm. Materials: Reclaimed wood, black metal frame.",
                    Price = 549.99m,
                    OriginalPrice = 649.99m,
                    StockQuantity = 12,
                    SKU = "TVS-IN001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddBookshelfProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "5-Tier Bookshelf",
                    Slug = "5-tier-bookshelf",
                    Description = "Versatile 5-tier bookshelf with open design, perfect for displaying books and decorative items. Dimensions: 80cm x 30cm x 180cm. Materials: Engineered wood, metal frame.",
                    Price = 249.99m,
                    OriginalPrice = 299.99m,
                    StockQuantity = 25,
                    SKU = "BSH-5T001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Ladder Bookcase",
                    Slug = "ladder-bookcase",
                    Description = "Trendy ladder-style bookcase that leans against the wall, combining style and functionality. Dimensions: 60cm x 40cm x 190cm. Materials: Solid wood, metal accents.",
                    Price = 199.99m,
                    OriginalPrice = 249.99m,
                    StockQuantity = 20,
                    SKU = "BSH-LD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Geometric Bookshelf",
                    Slug = "geometric-bookshelf",
                    Description = "Modern bookshelf with geometric design, creating a unique display for your books and decor. Dimensions: 120cm x 30cm x 150cm. Materials: Engineered wood, metal supports.",
                    Price = 329.99m,
                    OriginalPrice = 379.99m,
                    StockQuantity = 15,
                    SKU = "BSH-GM001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Corner Bookshelf Unit",
                    Slug = "corner-bookshelf-unit",
                    Description = "Space-efficient corner bookshelf that maximizes storage in unused corner spaces. Dimensions: 60cm x 60cm x 180cm. Materials: Engineered wood, adjustable shelves.",
                    Price = 179.99m,
                    OriginalPrice = 219.99m,
                    StockQuantity = 18,
                    SKU = "BSH-CR001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Pipe Bookshelf",
                    Slug = "industrial-pipe-bookshelf",
                    Description = "Rustic bookshelf featuring wood shelves and industrial pipe frame for an urban loft aesthetic. Dimensions: 100cm x 35cm x 175cm. Materials: Solid wood shelves, metal pipes.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 12,
                    SKU = "BSH-IP001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddBedProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Platform Bed Frame",
                    Slug = "platform-bed-frame",
                    Description = "Contemporary platform bed with upholstered headboard and solid wood base, providing both style and comfort. Dimensions: 210cm x 180cm x 100cm (King size). Materials: Solid pine wood, premium fabric.",
                    Price = 899.99m,
                    OriginalPrice = 999.99m,
                    StockQuantity = 10,
                    SKU = "BED-PF001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Canopy Bed",
                    Slug = "canopy-bed",
                    Description = "Elegant canopy bed with four posts and a frame, creating a luxurious focal point for your bedroom. Dimensions: 220cm x 180cm x 210cm (King size). Materials: Solid wood, metal frame.",
                    Price = 1299.99m,
                    OriginalPrice = 1499.99m,
                    StockQuantity = 8,
                    SKU = "BED-CB001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Storage Bed with Drawers",
                    Slug = "storage-bed-with-drawers",
                    Description = "Practical bed frame with built-in drawers for additional storage space. Dimensions: 210cm x 150cm x 90cm (Queen size). Materials: Engineered wood, metal hardware.",
                    Price = 799.99m,
                    OriginalPrice = 899.99m,
                    StockQuantity = 15,
                    SKU = "BED-SD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Adjustable Bed Base",
                    Slug = "adjustable-bed-base",
                    Description = "Electric adjustable bed base with multiple positions for reading, watching TV, or sleeping. Dimensions: 200cm x 150cm x variable height (Queen size). Materials: Metal frame, electric motor.",
                    Price = 1499.99m,
                    OriginalPrice = 1699.99m,
                    StockQuantity = 10,
                    SKU = "BED-AB001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Wooden Sleigh Bed",
                    Slug = "wooden-sleigh-bed",
                    Description = "Classic sleigh bed with curved headboard and footboard, crafted from solid wood. Dimensions: 220cm x 180cm x 120cm (King size). Materials: Solid mahogany wood, wood veneer.",
                    Price = 1199.99m,
                    OriginalPrice = 1399.99m,
                    StockQuantity = 8,
                    SKU = "BED-SL001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private void AddNightstandProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Minimalist Nightstand",
                    Slug = "minimalist-nightstand",
                    Description = "Compact nightstand with drawer and open shelf, perfect for keeping essentials within reach. Dimensions: 45cm x 40cm x 55cm. Materials: Solid oak, metal handles.",
                    Price = 199.99m,
                    OriginalPrice = 249.99m,
                    StockQuantity = 30,
                    SKU = "NST-MN001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Floating Bedside Shelf",
                    Slug = "floating-bedside-shelf",
                    Description = "Space-saving wall-mounted nightstand that creates a floating effect, ideal for small bedrooms. Dimensions: 40cm x 30cm x 15cm. Materials: Engineered wood, hidden mounting hardware.",
                    Price = 99.99m,
                    OriginalPrice = 129.99m,
                    StockQuantity = 25,
                    SKU = "NST-FB001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "3-Drawer Nightstand",
                    Slug = "3-drawer-nightstand",
                    Description = "Spacious nightstand with three drawers for ample bedside storage. Dimensions: 50cm x 45cm x 60cm. Materials: Solid wood, metal drawer slides.",
                    Price = 249.99m,
                    OriginalPrice = 299.99m,
                    StockQuantity = 20,
                    SKU = "NST-3D001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Marble Top Nightstand",
                    Slug = "marble-top-nightstand",
                    Description = "Elegant nightstand featuring a genuine marble top and gold-finished metal base. Dimensions: 45cm x 45cm x 55cm. Materials: Marble, gold-finished metal.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 15,
                    SKU = "NST-MT001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Nightstand",
                    Slug = "industrial-nightstand",
                    Description = "Rustic nightstand with metal frame and wood top in industrial style. Dimensions: 45cm x 40cm x 60cm. Materials: Reclaimed wood, black metal frame.",
                    Price = 179.99m,
                    OriginalPrice = 219.99m,
                    StockQuantity = 18,
                    SKU = "NST-IN001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddWardrobeProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "3-Door Wardrobe",
                    Slug = "3-door-wardrobe",
                    Description = "Spacious wardrobe with three doors, hanging rail, and shelves for complete clothing storage. Dimensions: 150cm x 60cm x 200cm. Materials: Engineered wood, metal hardware.",
                    Price = 699.99m,
                    OriginalPrice = 799.99m,
                    StockQuantity = 15,
                    SKU = "WRD-3D001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Sliding Door Wardrobe",
                    Slug = "sliding-door-wardrobe",
                    Description = "Space-saving wardrobe with sliding doors, perfect for bedrooms with limited space. Dimensions: 200cm x 60cm x 220cm. Materials: Engineered wood, mirror doors, aluminum tracks.",
                    Price = 899.99m,
                    OriginalPrice = 999.99m,
                    StockQuantity = 10,
                    SKU = "WRD-SD001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Corner Wardrobe Unit",
                    Slug = "corner-wardrobe-unit",
                    Description = "Clever corner wardrobe that maximizes storage in bedroom corners. Dimensions: 100cm x 100cm x 200cm. Materials: Engineered wood, metal hanging rail.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 12,
                    SKU = "WRD-CU001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Open Concept Wardrobe",
                    Slug = "open-concept-wardrobe",
                    Description = "Modern open wardrobe system with customizable shelves, rails, and storage compartments. Dimensions: 180cm x 50cm x 190cm. Materials: Metal frame, wood shelves.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 15,
                    SKU = "WRD-OC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Mirrored Wardrobe",
                    Slug = "mirrored-wardrobe",
                    Description = "Elegant wardrobe with full-length mirror doors, combining storage with functionality. Dimensions: 180cm x 60cm x 210cm. Materials: Engineered wood, mirror glass, metal hardware.",
                    Price = 799.99m,
                    OriginalPrice = 899.99m,
                    StockQuantity = 10,
                    SKU = "WRD-MR001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private void AddDresserProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "6-Drawer Dresser",
                    Slug = "6-drawer-dresser",
                    Description = "Spacious dresser with six drawers for ample clothing storage. Dimensions: 140cm x 50cm x 80cm. Materials: Solid wood, metal drawer slides.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 15,
                    SKU = "DRS-6D001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Dresser with Mirror",
                    Slug = "dresser-with-mirror",
                    Description = "Classic dresser with attached mirror, perfect for bedroom or dressing area. Dimensions: 120cm x 45cm x 160cm (with mirror). Materials: Engineered wood, glass mirror.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 12,
                    SKU = "DRS-MR001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Low Profile Dresser",
                    Slug = "low-profile-dresser",
                    Description = "Modern low-profile dresser that can double as a TV stand or display surface. Dimensions: 160cm x 45cm x 60cm. Materials: Engineered wood, metal accents.",
                    Price = 449.99m,
                    OriginalPrice = 499.99m,
                    StockQuantity = 18,
                    SKU = "DRS-LP001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Tall Chest of Drawers",
                    Slug = "tall-chest-of-drawers",
                    Description = "Space-saving vertical chest with multiple drawers for maximizing storage in smaller spaces. Dimensions: 80cm x 50cm x 120cm. Materials: Solid wood, metal handles.",
                    Price = 399.99m,
                    OriginalPrice = 449.99m,
                    StockQuantity = 20,
                    SKU = "DRS-TC001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Style Dresser",
                    Slug = "industrial-style-dresser",
                    Description = "Rustic dresser with metal frame and wood drawers in industrial style. Dimensions: 120cm x 45cm x 75cm. Materials: Reclaimed wood, black metal frame.",
                    Price = 549.99m,
                    OriginalPrice = 649.99m,
                    StockQuantity = 12,
                    SKU = "DRS-IS001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddDiningTableProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Extendable Dining Table",
                    Slug = "extendable-dining-table",
                    Description = "Versatile dining table that extends to accommodate extra guests, perfect for both everyday meals and special occasions. Dimensions: 160-220cm x 90cm x 75cm. Materials: Solid oak, metal frame.",
                    Price = 799.99m,
                    OriginalPrice = 899.99m,
                    StockQuantity = 15,
                    SKU = "TBL-ED001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Round Dining Table",
                    Slug = "round-dining-table",
                    Description = "Elegant round dining table, perfect for creating an intimate dining experience. Dimensions: 120cm diameter x 75cm height. Materials: Solid wood, pedestal base.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 18,
                    SKU = "TBL-RD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Glass Top Dining Table",
                    Slug = "glass-top-dining-table",
                    Description = "Modern dining table with tempered glass top and stylish metal base. Dimensions: 180cm x 90cm x 75cm. Materials: Tempered glass, stainless steel base.",
                    Price = 699.99m,
                    OriginalPrice = 799.99m,
                    StockQuantity = 12,
                    SKU = "TBL-GT001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Compact Dining Table",
                    Slug = "compact-dining-table",
                    Description = "Space-saving dining table designed for apartments and small dining areas. Dimensions: 120cm x 80cm x 75cm. Materials: Engineered wood, metal legs.",
                    Price = 399.99m,
                    OriginalPrice = 449.99m,
                    StockQuantity = 20,
                    SKU = "TBL-CD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Farmhouse Dining Table",
                    Slug = "farmhouse-dining-table",
                    Description = "Rustic farmhouse-style dining table with distressed finish and sturdy construction. Dimensions: 200cm x 100cm x 75cm. Materials: Solid pine wood, hand-distressed finish.",
                    Price = 899.99m,
                    OriginalPrice = 999.99m,
                    StockQuantity = 10,
                    SKU = "TBL-FH001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private void AddDiningChairProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Modern Dining Chair Set",
                    Slug = "modern-dining-chair-set",
                    Description = "Set of 4 ergonomic dining chairs with comfortable padding and stylish design, complementing any dining table. Dimensions: 45cm x 55cm x 85cm (each). Materials: Solid beech wood, premium fabric.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 20,
                    SKU = "CHR-MD001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Upholstered Dining Chairs",
                    Slug = "upholstered-dining-chairs",
                    Description = "Set of 2 fully upholstered dining chairs with elegant button-tufted backrest. Dimensions: 50cm x 60cm x 90cm (each). Materials: Solid wood frame, premium velvet upholstery.",
                    Price = 349.99m,
                    OriginalPrice = 399.99m,
                    StockQuantity = 15,
                    SKU = "CHR-UD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Wooden Dining Chairs",
                    Slug = "wooden-dining-chairs",
                    Description = "Set of 4 classic wooden dining chairs with timeless design. Dimensions: 45cm x 50cm x 85cm (each). Materials: Solid oak wood, natural finish.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 12,
                    SKU = "CHR-WD001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Dining Chairs",
                    Slug = "industrial-dining-chairs",
                    Description = "Set of 4 industrial-style dining chairs with metal frame and wood seats. Dimensions: 45cm x 50cm x 80cm (each). Materials: Metal frame, solid wood seat.",
                    Price = 399.99m,
                    OriginalPrice = 449.99m,
                    StockQuantity = 18,
                    SKU = "CHR-ID001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Stackable Dining Chairs",
                    Slug = "stackable-dining-chairs",
                    Description = "Set of 6 stackable dining chairs, perfect for saving space when not in use. Dimensions: 45cm x 50cm x 80cm (each). Materials: Molded plastic, metal legs.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 20,
                    SKU = "CHR-SD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddBuffetProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Modern Sideboard",
                    Slug = "modern-sideboard",
                    Description = "Sleek sideboard with ample storage space for dining essentials and decorative items. Dimensions: 160cm x 45cm x 80cm. Materials: Engineered wood, high-gloss finish.",
                    Price = 699.99m,
                    OriginalPrice = 799.99m,
                    StockQuantity = 15,
                    SKU = "BUF-MS001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Wine Cabinet",
                    Slug = "wine-cabinet",
                    Description = "Specialized cabinet for storing wine bottles and glasses, perfect for entertaining. Dimensions: 100cm x 45cm x 90cm. Materials: Solid wood, tempered glass doors.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 12,
                    SKU = "BUF-WC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Buffet Cabinet with Hutch",
                    Slug = "buffet-cabinet-with-hutch",
                    Description = "Traditional buffet with upper hutch for display and storage in dining rooms. Dimensions: 140cm x 50cm x 200cm. Materials: Solid wood, glass display shelves.",
                    Price = 999.99m,
                    OriginalPrice = 1199.99m,
                    StockQuantity = 8,
                    SKU = "BUF-BH001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Compact Credenza",
                    Slug = "compact-credenza",
                    Description = "Space-saving credenza for smaller dining areas with adjustable shelves. Dimensions: 120cm x 40cm x 75cm. Materials: Engineered wood, metal hardware.",
                    Price = 449.99m,
                    OriginalPrice = 499.99m,
                    StockQuantity = 18,
                    SKU = "BUF-CC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Buffet",
                    Slug = "industrial-buffet",
                    Description = "Rustic buffet with metal accents and distressed wood finish in industrial style. Dimensions: 150cm x 45cm x 85cm. Materials: Reclaimed wood, black metal frame.",
                    Price = 749.99m,
                    OriginalPrice = 849.99m,
                    StockQuantity = 10,
                    SKU = "BUF-IB001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddDeskProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Ergonomic Desk",
                    Slug = "ergonomic-desk",
                    Description = "Spacious desk with height adjustment and cable management, designed for optimal productivity and comfort. Dimensions: 140cm x 70cm x 75cm. Materials: Engineered wood, metal frame.",
                    Price = 449.99m,
                    OriginalPrice = 499.99m,
                    StockQuantity = 25,
                    SKU = "DSK-ER001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "L-Shaped Corner Desk",
                    Slug = "l-shaped-corner-desk",
                    Description = "Spacious L-shaped desk that fits perfectly in corners, providing ample workspace. Dimensions: 160cm x 160cm x 75cm. Materials: Engineered wood, metal frame.",
                    Price = 599.99m,
                    OriginalPrice = 699.99m,
                    StockQuantity = 15,
                    SKU = "DSK-LS001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Standing Desk",
                    Slug = "standing-desk",
                    Description = "Adjustable height desk that allows for both sitting and standing positions for a healthier work environment. Dimensions: 120cm x 60cm x 70-120cm (adjustable). Materials: Engineered wood, electric motor lift system.",
                    Price = 799.99m,
                    OriginalPrice = 899.99m,
                    StockQuantity = 12,
                    SKU = "DSK-SD001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Compact Writing Desk",
                    Slug = "compact-writing-desk",
                    Description = "Space-saving desk perfect for small home offices or apartments. Dimensions: 100cm x 50cm x 75cm. Materials: Solid wood, metal legs.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 20,
                    SKU = "DSK-CW001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Industrial Desk",
                    Slug = "industrial-desk",
                    Description = "Rustic desk with metal frame and wood top in industrial style. Dimensions: 130cm x 65cm x 75cm. Materials: Reclaimed wood, black metal frame.",
                    Price = 399.99m,
                    OriginalPrice = 449.99m,
                    StockQuantity = 15,
                    SKU = "DSK-ID001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddOfficeChairProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Executive Office Chair",
                    Slug = "executive-office-chair",
                    Description = "Premium office chair with adjustable height, lumbar support, and 360° swivel, ensuring comfort during long work hours. Dimensions: 65cm x 65cm x 110-120cm. Materials: Mesh fabric, premium leather, aluminum base.",
                    Price = 349.99m,
                    OriginalPrice = 399.99m,
                    StockQuantity = 20,
                    SKU = "CHR-EO001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Ergonomic Mesh Chair",
                    Slug = "ergonomic-mesh-chair",
                    Description = "Breathable mesh chair with ergonomic design for proper posture and all-day comfort. Dimensions: 60cm x 60cm x 100-110cm. Materials: Mesh fabric, adjustable armrests, nylon base.",
                    Price = 249.99m,
                    OriginalPrice = 299.99m,
                    StockQuantity = 25,
                    SKU = "CHR-EM001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Task Chair",
                    Slug = "task-chair",
                    Description = "Simple yet comfortable task chair for everyday office use. Dimensions: 55cm x 55cm x 90-100cm. Materials: Fabric upholstery, plastic base.",
                    Price = 149.99m,
                    OriginalPrice = 179.99m,
                    StockQuantity = 30,
                    SKU = "CHR-TC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Kneeling Chair",
                    Slug = "kneeling-chair",
                    Description = "Ergonomic kneeling chair that promotes better posture and reduces lower back strain. Dimensions: 50cm x 70cm x 70cm. Materials: Wood frame, cushioned seat and knee rest.",
                    Price = 199.99m,
                    OriginalPrice = 229.99m,
                    StockQuantity = 15,
                    SKU = "CHR-KC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Gaming Office Chair",
                    Slug = "gaming-office-chair",
                    Description = "Racing-style gaming chair that doubles as an office chair with extra padding and support. Dimensions: 70cm x 70cm x 125-135cm. Materials: PU leather, memory foam padding, metal frame.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 18,
                    SKU = "CHR-GO001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private void AddFilingCabinetProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "3-Drawer Filing Cabinet",
                    Slug = "3-drawer-filing-cabinet",
                    Description = "Practical filing cabinet with three drawers for organizing documents and office supplies. Dimensions: 40cm x 50cm x 100cm. Materials: Metal construction, lockable drawers.",
                    Price = 199.99m,
                    OriginalPrice = 249.99m,
                    StockQuantity = 20,
                    SKU = "FIL-3D001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Lateral File Cabinet",
                    Slug = "lateral-file-cabinet",
                    Description = "Wide lateral filing cabinet with two drawers for storing letter and legal size documents. Dimensions: 90cm x 50cm x 70cm. Materials: Engineered wood, metal hardware.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 15,
                    SKU = "FIL-LF001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Mobile Pedestal",
                    Slug = "mobile-pedestal",
                    Description = "Compact mobile filing cabinet that fits under most desks with casters for easy movement. Dimensions: 40cm x 50cm x 60cm. Materials: Metal construction, lockable drawers.",
                    Price = 149.99m,
                    OriginalPrice = 179.99m,
                    StockQuantity = 25,
                    SKU = "FIL-MP001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Wooden File Cabinet",
                    Slug = "wooden-file-cabinet",
                    Description = "Elegant wooden filing cabinet that blends with home office furniture. Dimensions: 45cm x 55cm x 75cm. Materials: Solid wood, metal drawer slides.",
                    Price = 249.99m,
                    OriginalPrice = 299.99m,
                    StockQuantity = 18,
                    SKU = "FIL-WF001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Fireproof File Cabinet",
                    Slug = "fireproof-file-cabinet",
                    Description = "Specialized filing cabinet with fire-resistant construction to protect important documents. Dimensions: 50cm x 60cm x 80cm. Materials: Fire-resistant steel, insulation.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 10,
                    SKU = "FIL-FF001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private void AddPatioSetProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Patio Dining Set",
                    Slug = "patio-dining-set",
                    Description = "Weather-resistant dining set including a table and 6 chairs, perfect for outdoor gatherings and meals. Dimensions: Table: 180cm x 90cm x 75cm, Chairs: 55cm x 60cm x 85cm. Materials: Weather-resistant teak wood, waterproof fabric.",
                    Price = 1199.99m,
                    OriginalPrice = 1399.99m,
                    StockQuantity = 10,
                    SKU = "OUT-PD001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Outdoor Sofa Set",
                    Slug = "outdoor-sofa-set",
                    Description = "Comfortable outdoor sofa set with coffee table, perfect for relaxing in your garden or patio. Dimensions: Sofa: 220cm x 85cm x 70cm, Chairs: 80cm x 85cm x 70cm, Table: 100cm x 60cm x 45cm. Materials: All-weather wicker, aluminum frame, water-resistant cushions.",
                    Price = 1499.99m,
                    OriginalPrice = 1699.99m,
                    StockQuantity = 8,
                    SKU = "OUT-OS001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Bistro Set",
                    Slug = "bistro-set",
                    Description = "Compact 3-piece bistro set with table and two chairs, ideal for small balconies or patios. Dimensions: Table: 60cm diameter x 70cm height, Chairs: 45cm x 45cm x 80cm. Materials: Powder-coated steel, tempered glass table top.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 20,
                    SKU = "OUT-BS001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Adirondack Chair Set",
                    Slug = "adirondack-chair-set",
                    Description = "Classic Adirondack chair set with side table, perfect for casual outdoor seating. Dimensions: Chairs: 70cm x 85cm x 90cm, Table: 50cm x 50cm x 50cm. Materials: Weather-resistant HDPE lumber, stainless steel hardware.",
                    Price = 499.99m,
                    OriginalPrice = 599.99m,
                    StockQuantity = 15,
                    SKU = "OUT-AC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Fire Pit Table Set",
                    Slug = "fire-pit-table-set",
                    Description = "Outdoor seating set with built-in fire pit table, perfect for evening gatherings. Dimensions: Table: 120cm diameter x 65cm height, Chairs: 70cm x 70cm x 80cm. Materials: Cast aluminum, fire-resistant table top, weather-resistant cushions.",
                    Price = 1299.99m,
                    OriginalPrice = 1499.99m,
                    StockQuantity = 8,
                    SKU = "OUT-FP001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private void AddOutdoorChairProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Garden Lounge Chair",
                    Slug = "garden-lounge-chair",
                    Description = "Comfortable lounge chair with adjustable backrest, ideal for relaxing in your garden or by the pool. Dimensions: 200cm x 60cm x 35cm. Materials: Weather-resistant wicker, waterproof cushions.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 15,
                    SKU = "OUT-GL001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Outdoor Rocking Chair",
                    Slug = "outdoor-rocking-chair",
                    Description = "Weather-resistant rocking chair for relaxing on your porch or patio. Dimensions: 85cm x 65cm x 100cm. Materials: Teak wood, ergonomic design.",
                    Price = 249.99m,
                    OriginalPrice = 299.99m,
                    StockQuantity = 18,
                    SKU = "OUT-RC001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Folding Deck Chair",
                    Slug = "folding-deck-chair",
                    Description = "Portable folding chair perfect for beach, camping, or garden use. Dimensions: 60cm x 50cm x 80cm (open), 60cm x 10cm x 80cm (folded). Materials: Aluminum frame, durable polyester fabric.",
                    Price = 79.99m,
                    OriginalPrice = 99.99m,
                    StockQuantity = 30,
                    SKU = "OUT-FD001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Hanging Egg Chair",
                    Slug = "hanging-egg-chair",
                    Description = "Stylish hanging chair with stand, creating a cozy spot in your garden or patio. Dimensions: 95cm x 95cm x 195cm. Materials: Powder-coated steel frame, weather-resistant wicker, plush cushions.",
                    Price = 399.99m,
                    OriginalPrice = 499.99m,
                    StockQuantity = 12,
                    SKU = "OUT-HE001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Stackable Outdoor Chairs",
                    Slug = "stackable-outdoor-chairs",
                    Description = "Set of 4 stackable outdoor chairs, perfect for easy storage when not in use. Dimensions: 55cm x 60cm x 85cm (each). Materials: Powder-coated aluminum, textilene fabric.",
                    Price = 249.99m,
                    OriginalPrice = 299.99m,
                    StockQuantity = 20,
                    SKU = "OUT-SO001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                }
            });
        }

        private void AddGardenAccessoryProducts(List<Product> products, int categoryId)
        {
            products.AddRange(new List<Product>
            {
                new Product
                {
                    Name = "Garden Planter Box",
                    Slug = "garden-planter-box",
                    Description = "Decorative planter box for flowers and plants, enhancing your outdoor space. Dimensions: 80cm x 30cm x 40cm. Materials: Weather-resistant wood, drainage holes.",
                    Price = 99.99m,
                    OriginalPrice = 129.99m,
                    StockQuantity = 25,
                    SKU = "GAR-PB001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Outdoor Lantern Set",
                    Slug = "outdoor-lantern-set",
                    Description = "Set of 3 decorative lanterns for ambient lighting in your garden or patio. Dimensions: Large: 25cm x 25cm x 40cm, Medium: 20cm x 20cm x 30cm, Small: 15cm x 15cm x 25cm. Materials: Powder-coated metal, tempered glass.",
                    Price = 79.99m,
                    OriginalPrice = 99.99m,
                    StockQuantity = 30,
                    SKU = "GAR-OL001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Garden Fountain",
                    Slug = "garden-fountain",
                    Description = "Elegant garden fountain creating a peaceful atmosphere with the sound of flowing water. Dimensions: 60cm diameter x 90cm height. Materials: Cast stone, water pump included.",
                    Price = 299.99m,
                    OriginalPrice = 349.99m,
                    StockQuantity = 15,
                    SKU = "GAR-GF001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                },
                new Product
                {
                    Name = "Outdoor Rug",
                    Slug = "outdoor-rug",
                    Description = "Weather-resistant rug to define and enhance your outdoor living space. Dimensions: 200cm x 150cm. Materials: Synthetic fibers, UV-resistant, easy to clean.",
                    Price = 129.99m,
                    OriginalPrice = 159.99m,
                    StockQuantity = 20,
                    SKU = "GAR-OR001",
                    CategoryId = categoryId,
                    IsFeatured = false,
                    IsActive = true
                },
                new Product
                {
                    Name = "Solar Garden Lights",
                    Slug = "solar-garden-lights",
                    Description = "Set of 8 solar-powered pathway lights for illuminating garden walkways. Dimensions: 15cm x 15cm x 45cm (each). Materials: Stainless steel, solar panels, LED lights.",
                    Price = 69.99m,
                    OriginalPrice = 89.99m,
                    StockQuantity = 35,
                    SKU = "GAR-SL001",
                    CategoryId = categoryId,
                    IsFeatured = true,
                    IsActive = true
                }
            });
        }

        private async Task AddProductImages()
        {
            // Get all products
            var products = await _context.Products.ToListAsync();
            var images = new List<Image>();

            // Add placeholder images for each product
            foreach (var product in products)
            {
                // Create 2-3 images per product
                int imageCount = new Random().Next(2, 4);

                for (int i = 1; i <= imageCount; i++)
                {
                    images.Add(new Image
                    {
                        ProductId = product.Id,
                        FileName = $"{product.Slug}-{i}.jpg",
                        FilePath = $"/images/products/{product.Slug}-{i}.jpg",
                        AltText = product.Name,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.Images.AddRangeAsync(images);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Added {images.Count} product images.");
        }
    }
}
