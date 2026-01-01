using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Data;

/// <summary>
/// Database seeder for initial data
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created and migrations applied
            await _context.Database.MigrateAsync();

            // Seed admin roles
            await SeedRolesAsync();

            // Seed customer roles
            await SeedCustomerRolesAsync();

            // Seed admin user
            await SeedAdminUserAsync();

            // Seed categories
            //var categories = await SeedCategoriesAsync();

            // Seed brands
            //var brands = await SeedBrandsAsync();

            // Seed products
            //await SeedProductsAsync(categories, brands);

            // Seed sample customer
            //await SeedSampleCustomerAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        // Admin roles (UserType.Admin is the default)
        var roles = new[]
        {
            new ApplicationRole("Admin", "Full system access", UserType.Admin),
            new ApplicationRole("SalesRep", "Sales representative with order and customer management", UserType.Admin)
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
            {
                await _roleManager.CreateAsync(role);
                _logger.LogInformation("Created role: {RoleName}", role.Name);
            }
        }
    }

    private async Task SeedCustomerRolesAsync()
    {
        // Customer roles with their permissions
        var customerRoles = new[]
        {
            new
            {
                Role = new ApplicationRole("CustomerAdmin", "Bayi Yöneticisi - Full dealer access", UserType.Customer),
                Permissions = new[] { CustomerPermissionScopes.All }
            },
            new
            {
                Role = new ApplicationRole("CustomerPurchasing", "Satın Alma - Purchasing and order management", UserType.Customer),
                Permissions = new[]
                {
                    CustomerPermissionScopes.OrdersRead,
                    CustomerPermissionScopes.OrdersCreate,
                    CustomerPermissionScopes.OrdersEdit,
                    CustomerPermissionScopes.CatalogRead,
                    CustomerPermissionScopes.PricesRead,
                    CustomerPermissionScopes.AddressesRead
                }
            },
            new
            {
                Role = new ApplicationRole("CustomerAccounting", "Muhasebe - Accounting and finance", UserType.Customer),
                Permissions = new[]
                {
                    CustomerPermissionScopes.OrdersRead,
                    CustomerPermissionScopes.BalanceRead,
                    CustomerPermissionScopes.ProfileRead,
                    CustomerPermissionScopes.AddressesRead
                }
            },
            new
            {
                Role = new ApplicationRole("CustomerEmployee", "Çalışan - Basic employee access", UserType.Customer),
                Permissions = new[]
                {
                    CustomerPermissionScopes.CatalogRead,
                    CustomerPermissionScopes.OrdersRead
                }
            }
        };

        foreach (var item in customerRoles)
        {
            if (!await _roleManager.RoleExistsAsync(item.Role.Name!))
            {
                var result = await _roleManager.CreateAsync(item.Role);
                if (result.Succeeded)
                {
                    // Add permissions as claims
                    foreach (var permission in item.Permissions)
                    {
                        await _roleManager.AddClaimAsync(item.Role, new System.Security.Claims.Claim("permission", permission));
                    }
                    _logger.LogInformation("Created customer role: {RoleName} with {PermissionCount} permissions",
                        item.Role.Name, item.Permissions.Length);
                }
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@b2bcommerce.com";
        const string adminPassword = "Admin123!";

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            _logger.LogInformation("Admin user already exists");
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
            _logger.LogInformation("Created admin user: {Email}", adminEmail);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user: {Errors}", errors);
        }
    }

    private async Task<Dictionary<string, Category>> SeedCategoriesAsync()
    {
        var categories = new Dictionary<string, Category>();

        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already seeded");
            var existingCategories = await _context.Categories.ToListAsync();
            return existingCategories.ToDictionary(c => c.Name, c => c);
        }

        // Parent categories
        var electronics = new Category("Electronics", "Electronic devices and components", null, 1);
        var officeSupplies = new Category("Office Supplies", "Office equipment and supplies", null, 2);
        var industrialEquipment = new Category("Industrial Equipment", "Heavy machinery and industrial tools", null, 3);
        var safetyEquipment = new Category("Safety Equipment", "Personal protective equipment and safety gear", null, 4);

        _context.Categories.AddRange(electronics, officeSupplies, industrialEquipment, safetyEquipment);
        await _context.SaveChangesAsync();

        // Sub-categories for Electronics
        var computers = new Category("Computers", "Desktop and laptop computers", electronics.Id, 1);
        var networking = new Category("Networking", "Routers, switches, and networking equipment", electronics.Id, 2);
        var peripherals = new Category("Peripherals", "Monitors, keyboards, mice, and accessories", electronics.Id, 3);

        // Sub-categories for Office Supplies
        var furniture = new Category("Furniture", "Office desks, chairs, and storage", officeSupplies.Id, 1);
        var stationery = new Category("Stationery", "Paper, pens, and writing supplies", officeSupplies.Id, 2);
        var printers = new Category("Printers & Scanners", "Printing and scanning equipment", officeSupplies.Id, 3);

        _context.Categories.AddRange(computers, networking, peripherals, furniture, stationery, printers);
        await _context.SaveChangesAsync();

        categories["Electronics"] = electronics;
        categories["Office Supplies"] = officeSupplies;
        categories["Industrial Equipment"] = industrialEquipment;
        categories["Safety Equipment"] = safetyEquipment;
        categories["Computers"] = computers;
        categories["Networking"] = networking;
        categories["Peripherals"] = peripherals;
        categories["Furniture"] = furniture;
        categories["Stationery"] = stationery;
        categories["Printers & Scanners"] = printers;

        _logger.LogInformation("Created {Count} categories", categories.Count);
        return categories;
    }

    private async Task<Dictionary<string, Brand>> SeedBrandsAsync()
    {
        var brands = new Dictionary<string, Brand>();

        if (await _context.Brands.AnyAsync())
        {
            _logger.LogInformation("Brands already seeded");
            var existingBrands = await _context.Brands.ToListAsync();
            return existingBrands.ToDictionary(b => b.Name, b => b);
        }

        var brandData = new[]
        {
            new Brand("TechPro", "Professional technology solutions"),
            new Brand("OfficeMaster", "Premium office equipment"),
            new Brand("SafetyFirst", "Industrial safety equipment"),
            new Brand("IndustrialMax", "Heavy-duty industrial machinery"),
            new Brand("NetGear Pro", "Enterprise networking solutions"),
            new Brand("ErgoDesk", "Ergonomic office furniture")
        };

        foreach (var brand in brandData)
        {
            _context.Brands.Add(brand);
            brands[brand.Name] = brand;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} brands", brands.Count);
        return brands;
    }

    private async Task SeedProductsAsync(Dictionary<string, Category> categories, Dictionary<string, Brand> brands)
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already seeded");
            return;
        }

        var products = new List<Product>();
        var currency = "USD";

        // Computers
        if (categories.TryGetValue("Computers", out var computersCategory) &&
            brands.TryGetValue("TechPro", out var techProBrand))
        {
            var laptop = Product.Create(
                "TechPro Business Laptop 15\"",
                "15.6\" Full HD display, Intel Core i7, 16GB RAM, 512GB SSD",
                "TP-LAP-001",
                new Money(1299.99m, currency),
                50,
                1,
                0.18m);
            laptop.UpdateBasicInfo(laptop.Name, laptop.Description, techProBrand.Id);
            laptop.AddToCategory(computersCategory.Id, isPrimary: true, displayOrder: 0);
            laptop.UpdatePricing(
                new Money(1299.99m, currency),
                new Money(1249.99m, currency),  // Tier 1
                new Money(1199.99m, currency),  // Tier 2
                new Money(1149.99m, currency),  // Tier 3
                new Money(1099.99m, currency),  // Tier 4
                new Money(1049.99m, currency)); // Tier 5
            products.Add(laptop);

            var desktop = Product.Create(
                "TechPro Workstation Tower",
                "Intel Xeon processor, 32GB ECC RAM, 1TB NVMe SSD, NVIDIA Quadro graphics",
                "TP-WRK-001",
                new Money(2499.99m, currency),
                25,
                1,
                0.18m);
            desktop.UpdateBasicInfo(desktop.Name, desktop.Description, techProBrand.Id);
            desktop.AddToCategory(computersCategory.Id, isPrimary: true, displayOrder: 0);
            desktop.UpdatePricing(
                new Money(2499.99m, currency),
                new Money(2399.99m, currency),
                new Money(2299.99m, currency),
                new Money(2199.99m, currency),
                new Money(2099.99m, currency),
                new Money(1999.99m, currency));
            products.Add(desktop);
        }

        // Networking
        if (categories.TryGetValue("Networking", out var networkingCategory) &&
            brands.TryGetValue("NetGear Pro", out var netGearBrand))
        {
            var router = Product.Create(
                "Enterprise Router NG-5000",
                "Dual-band WiFi 6, 10Gbps throughput, VPN support, 8 LAN ports",
                "NG-RTR-5000",
                new Money(899.99m, currency),
                100,
                1,
                0.18m);
            router.UpdateBasicInfo(router.Name, router.Description, netGearBrand.Id);
            router.AddToCategory(networkingCategory.Id, isPrimary: true, displayOrder: 0);
            router.UpdatePricing(
                new Money(899.99m, currency),
                new Money(849.99m, currency),
                new Money(799.99m, currency),
                new Money(749.99m, currency),
                new Money(699.99m, currency),
                new Money(649.99m, currency));
            products.Add(router);

            var networkSwitch = Product.Create(
                "Managed Switch 48-Port",
                "48 Gigabit ports, 4 SFP+ uplinks, Layer 3, PoE+",
                "NG-SW-048",
                new Money(1599.99m, currency),
                30,
                1,
                0.18m);
            networkSwitch.UpdateBasicInfo(networkSwitch.Name, networkSwitch.Description, netGearBrand.Id);
            networkSwitch.AddToCategory(networkingCategory.Id, isPrimary: true, displayOrder: 0);
            products.Add(networkSwitch);
        }

        // Peripherals
        if (categories.TryGetValue("Peripherals", out var peripheralsCategory) &&
            brands.TryGetValue("TechPro", out var techPro))
        {
            var monitor = Product.Create(
                "TechPro 27\" 4K Monitor",
                "27\" 4K UHD IPS display, USB-C, 60Hz, HDR400",
                "TP-MON-027",
                new Money(499.99m, currency),
                75,
                1,
                0.18m);
            monitor.UpdateBasicInfo(monitor.Name, monitor.Description, techPro.Id);
            monitor.AddToCategory(peripheralsCategory.Id, isPrimary: true, displayOrder: 0);
            monitor.UpdatePricing(
                new Money(499.99m, currency),
                new Money(479.99m, currency),
                new Money(459.99m, currency),
                new Money(439.99m, currency),
                new Money(419.99m, currency),
                new Money(399.99m, currency));
            products.Add(monitor);

            var keyboard = Product.Create(
                "Mechanical Keyboard Pro",
                "Cherry MX switches, RGB backlight, programmable keys",
                "TP-KB-PRO",
                new Money(149.99m, currency),
                200,
                5,
                0.18m);
            keyboard.UpdateBasicInfo(keyboard.Name, keyboard.Description, techPro.Id);
            keyboard.AddToCategory(peripheralsCategory.Id, isPrimary: true, displayOrder: 0);
            products.Add(keyboard);
        }

        // Furniture
        if (categories.TryGetValue("Furniture", out var furnitureCategory) &&
            brands.TryGetValue("ErgoDesk", out var ergoDeskBrand))
        {
            var desk = Product.Create(
                "ErgoDesk Standing Desk",
                "Electric height adjustable, 60\" x 30\" surface, memory presets",
                "ED-DSK-060",
                new Money(799.99m, currency),
                40,
                1,
                0.18m);
            desk.UpdateBasicInfo(desk.Name, desk.Description, ergoDeskBrand.Id);
            desk.AddToCategory(furnitureCategory.Id, isPrimary: true, displayOrder: 0);
            desk.UpdatePricing(
                new Money(799.99m, currency),
                new Money(769.99m, currency),
                new Money(739.99m, currency),
                new Money(709.99m, currency),
                new Money(679.99m, currency),
                new Money(649.99m, currency));
            products.Add(desk);

            var chair = Product.Create(
                "ErgoChair Executive",
                "Full mesh back, lumbar support, adjustable armrests, headrest",
                "ED-CHR-EXE",
                new Money(599.99m, currency),
                60,
                1,
                0.18m);
            chair.UpdateBasicInfo(chair.Name, chair.Description, ergoDeskBrand.Id);
            chair.AddToCategory(furnitureCategory.Id, isPrimary: true, displayOrder: 0);
            products.Add(chair);
        }

        // Safety Equipment
        if (categories.TryGetValue("Safety Equipment", out var safetyCategory) &&
            brands.TryGetValue("SafetyFirst", out var safetyBrand))
        {
            var helmet = Product.Create(
                "Industrial Safety Helmet",
                "ANSI/ISEA certified, adjustable suspension, UV resistant",
                "SF-HLM-001",
                new Money(45.99m, currency),
                500,
                10,
                0.18m);
            helmet.UpdateBasicInfo(helmet.Name, helmet.Description, safetyBrand.Id);
            helmet.AddToCategory(safetyCategory.Id, isPrimary: true, displayOrder: 0);
            helmet.UpdatePricing(
                new Money(45.99m, currency),
                new Money(42.99m, currency),
                new Money(39.99m, currency),
                new Money(36.99m, currency),
                new Money(33.99m, currency),
                new Money(29.99m, currency));
            products.Add(helmet);

            var gloves = Product.Create(
                "Cut-Resistant Gloves",
                "Level A4 cut resistance, touchscreen compatible, sizes S-XXL",
                "SF-GLV-CR4",
                new Money(24.99m, currency),
                1000,
                12,
                0.18m);
            gloves.UpdateBasicInfo(gloves.Name, gloves.Description, safetyBrand.Id);
            gloves.AddToCategory(safetyCategory.Id, isPrimary: true, displayOrder: 0);
            products.Add(gloves);

            var vest = Product.Create(
                "High-Visibility Safety Vest",
                "Class 2 certified, reflective strips, breathable mesh",
                "SF-VST-HV2",
                new Money(19.99m, currency),
                800,
                20,
                0.18m);
            vest.UpdateBasicInfo(vest.Name, vest.Description, safetyBrand.Id);
            vest.AddToCategory(safetyCategory.Id, isPrimary: true, displayOrder: 0);
            products.Add(vest);
        }

        // Industrial Equipment
        if (categories.TryGetValue("Industrial Equipment", out var industrialCategory) &&
            brands.TryGetValue("IndustrialMax", out var industrialBrand))
        {
            var pallet = Product.Create(
                "Electric Pallet Jack",
                "3300 lb capacity, lithium battery, 48\" forks",
                "IM-PLJ-3300",
                new Money(4999.99m, currency),
                10,
                1,
                0.18m);
            pallet.UpdateBasicInfo(pallet.Name, pallet.Description, industrialBrand.Id);
            pallet.AddToCategory(industrialCategory.Id, isPrimary: true, displayOrder: 0);
            pallet.UpdatePricing(
                new Money(4999.99m, currency),
                new Money(4799.99m, currency),
                new Money(4599.99m, currency),
                new Money(4399.99m, currency),
                new Money(4199.99m, currency),
                new Money(3999.99m, currency));
            products.Add(pallet);

            var compressor = Product.Create(
                "Industrial Air Compressor",
                "80 gallon, 5HP motor, 175 PSI max, two-stage",
                "IM-CMP-080",
                new Money(1899.99m, currency),
                15,
                1,
                0.18m);
            compressor.UpdateBasicInfo(compressor.Name, compressor.Description, industrialBrand.Id);
            compressor.AddToCategory(industrialCategory.Id, isPrimary: true, displayOrder: 0);
            products.Add(compressor);
        }

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} products", products.Count);
    }

    private async Task SeedSampleCustomerAsync()
    {
        const string customerEmail = "customer@acmecorp.com";
        const string customerPassword = "Customer123!";

        var existingUser = await _userManager.FindByEmailAsync(customerEmail);
        if (existingUser != null)
        {
            _logger.LogInformation("Sample customer already exists");
            return;
        }

        // Check if customer entity exists
        var existingCustomer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (existingCustomer is not null)
        {
            _logger.LogInformation("Sample customer entity already exists");
            return;
        }

        // Create customer entity using factory method
        var customer = Customer.Create(
            title: "Acme Corporation",
            taxOffice: "Manhattan",
            taxNo: "1234567890",
            establishmentYear: 2010,
            website: "https://acme.example.com"
        );

        // Approve the customer
        customer.Approve();

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Create primary contact for the customer
        var contact = CustomerContact.Create(
            customerId: customer.Id,
            firstName: "John",
            lastName: "Smith",
            email: customerEmail,
            position: "Procurement Manager",
            phone: "+1-555-123-4567",
            isPrimary: true
        );
        _context.Set<CustomerContact>().Add(contact);

        // Create addresses for the customer
        var billingAddress = CustomerAddress.Create(
            customerId: customer.Id,
            title: "Billing Address",
            addressType: CustomerAddressType.Billing,
            address: "123 Business Ave, New York, NY 10001, USA",
            isDefault: true
        );
        _context.Set<CustomerAddress>().Add(billingAddress);

        var shippingAddress = CustomerAddress.Create(
            customerId: customer.Id,
            title: "Shipping Address",
            addressType: CustomerAddressType.Shipping,
            address: "456 Warehouse Blvd, Newark, NJ 07102, USA",
            isDefault: true
        );
        _context.Set<CustomerAddress>().Add(shippingAddress);

        await _context.SaveChangesAsync();

        // Create user account linked to customer
        var customerUser = new ApplicationUser
        {
            UserName = customerEmail,
            Email = customerEmail,
            FirstName = "John",
            LastName = "Smith",
            CustomerId = customer.Id,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(customerUser, customerPassword);
        if (result.Succeeded)
        {
            // Assign CustomerAdmin role (will be created by SeedCustomerRolesAsync)
            await _userManager.AddToRoleAsync(customerUser, "CustomerAdmin");
            _logger.LogInformation("Created sample customer: {Email} (Company: {Company})",
                customerEmail, customer.Title);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create sample customer user: {Errors}", errors);
        }
    }
}
