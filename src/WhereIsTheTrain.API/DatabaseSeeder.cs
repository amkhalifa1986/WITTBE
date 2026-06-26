using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Infrastructure.Persistence;

namespace WhereIsTheTrain.API;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Extract train types from existing train names and populate the lookup table
        await ExtractAndPopulateTrainTypesAsync(context);

        if (!await context.GenderLookups.AnyAsync())
        {
            context.GenderLookups.AddRange(
                new GenderLookup { Id = Guid.Parse("00000000-0000-0000-0000-000000003001"), NameEn = "Male", NameAr = "ذكر" },
                new GenderLookup { Id = Guid.Parse("00000000-0000-0000-0000-000000003002"), NameEn = "Female", NameAr = "أنثى" }
            );
            await context.SaveChangesAsync();
        }

        // Ensure new Admin roles and users are seeded even if operational data already exists
        if (!await context.AdminUsers.AnyAsync())
        {
            // Create Default Admin Role and Privileges if not present
            var seedAdminRole = await context.AdminRoles.FirstOrDefaultAsync(r => r.Id == Guid.Parse("00000000-0000-0000-0000-000000009001"));
            if (seedAdminRole == null)
            {
                seedAdminRole = new AdminRole
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000009001"),
                    Name = "Administrator",
                    Description = "Full access to manage all operational data"
                };
                
                var seedModules = new[] { "Dashboard", "Users", "Trains", "Trips", "Stops", "Lookups", "LostFound", "Suggestions", "Disruptions", "RailwayPaths", "Updates", "Settings" };
                foreach (var m in seedModules)
                {
                    seedAdminRole.Privileges.Add(new AdminRolePrivilege
                    {
                        Id = Guid.NewGuid(),
                        Module = m,
                        CanView = true,
                        CanAdd = true,
                        CanEdit = true,
                        CanDelete = true
                    });
                }
                context.AdminRoles.Add(seedAdminRole);
                await context.SaveChangesAsync();
            }

            // Create Super Admin
            var seedSuperAdmin = new AdminUser
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000009002"),
                DisplayName = "Super Admin",
                Email = "superadmin@whereisthetrain.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
                IsSuperAdmin = true,
                RoleId = null
            };
            context.AdminUsers.Add(seedSuperAdmin);

            // Create Regular Admin
            var seedRegularAdmin = new AdminUser
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000009003"),
                DisplayName = "System Admin",
                Email = "admin@whereisthetrain.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsSuperAdmin = false,
                RoleId = seedAdminRole.Id
            };
            context.AdminUsers.Add(seedRegularAdmin);

            await context.SaveChangesAsync();
        }

        if (await context.Trains.AnyAsync())
            return; // Already seeded

        // Create Admin User in Users table (keep for foreign keys, change role to User)
        var adminUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            DisplayName = "Admin User (Legacy)",
            Email = "admin-legacy@whereisthetrain.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.User,
            EmailConfirmed = true
        };
        context.Users.Add(adminUser);

        // Create Default Admin Role and Privileges
        var adminRole = new AdminRole
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000009001"),
            Name = "Administrator",
            Description = "Full access to manage all operational data"
        };
        
        var modules = new[] { "Dashboard", "Users", "Trains", "Trips", "Stops", "Lookups", "LostFound", "Suggestions", "Disruptions", "RailwayPaths", "Updates", "Settings" };
        foreach (var m in modules)
        {
            adminRole.Privileges.Add(new AdminRolePrivilege
            {
                Id = Guid.NewGuid(),
                Module = m,
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true
            });
        }
        context.AdminRoles.Add(adminRole);

        // Create Super Admin in AdminUsers table
        var superAdmin = new AdminUser
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000009002"),
            DisplayName = "Super Admin",
            Email = "superadmin@whereisthetrain.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
            IsSuperAdmin = true,
            RoleId = null
        };
        context.AdminUsers.Add(superAdmin);

        // Create Regular Admin in AdminUsers table
        var regularAdmin = new AdminUser
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000009003"),
            DisplayName = "System Admin",
            Email = "admin@whereisthetrain.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            IsSuperAdmin = false,
            RoleId = adminRole.Id
        };
        context.AdminUsers.Add(regularAdmin);

        // Create Test User
        var testUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            DisplayName = "Mohammed Test",
            Email = "test@whereisthetrain.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = UserRole.User,
            EmailConfirmed = true
        };
        context.Users.Add(testUser);

        // Create Governorates
        var gov_cairo = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000201"), NameAr = "القاهرة", NameEn = "Cairo" };
        var gov_giza = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000202"), NameAr = "الجيزة", NameEn = "Giza" };
        var gov_benisuef = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000203"), NameAr = "بني سويف", NameEn = "Beni Suef" };
        var gov_minya = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000204"), NameAr = "المنيا", NameEn = "Minya" };
        var gov_asyut = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000205"), NameAr = "أسيوط", NameEn = "Asyut" };
        var gov_sohag = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000206"), NameAr = "سوهاج", NameEn = "Sohag" };
        var gov_qena = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000207"), NameAr = "قنا", NameEn = "Qena" };
        var gov_luxor = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000208"), NameAr = "الأقصر", NameEn = "Luxor" };
        var gov_aswan = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000209"), NameAr = "أسوان", NameEn = "Aswan" };
        var gov_alexandria = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000210"), NameAr = "الإسكندرية", NameEn = "Alexandria" };
        var gov_gharbia = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000211"), NameAr = "الغربية", NameEn = "Gharbia" };
        var gov_beheira = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000212"), NameAr = "البحيرة", NameEn = "Beheira" };
        var gov_portsaid = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000213"), NameAr = "بورسعيد", NameEn = "Port Said" };
        var gov_suez = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000214"), NameAr = "السويس", NameEn = "Suez" };
        var gov_damietta = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000215"), NameAr = "دمياط", NameEn = "Damietta" };
        var gov_dakahlia = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000216"), NameAr = "الدقهلية", NameEn = "Dakahlia" };
        var gov_sharqia = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000217"), NameAr = "الشرقية", NameEn = "Sharqia" };
        var gov_monufia = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000218"), NameAr = "المنوفية", NameEn = "Monufia" };
        var gov_qalyubia = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000219"), NameAr = "القليوبية", NameEn = "Qalyubia" };
        var gov_kafr_el_sheikh = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000220"), NameAr = "كفر الشيخ", NameEn = "Kafr El Sheikh" };
        var gov_ismailia = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000221"), NameAr = "الإسماعيلية", NameEn = "Ismailia" };
        var gov_faiyum = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000222"), NameAr = "الفيوم", NameEn = "Faiyum" };
        var gov_red_sea = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000223"), NameAr = "البحر الأحمر", NameEn = "Red Sea" };
        var gov_new_valley = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000224"), NameAr = "الوادي الجديد", NameEn = "New Valley" };
        var gov_matrouh = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000225"), NameAr = "مطروح", NameEn = "Matrouh" };
        var gov_north_sinai = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000226"), NameAr = "شمال سيناء", NameEn = "North Sinai" };
        var gov_south_sinai = new Governorate { Id = Guid.Parse("00000000-0000-0000-0000-000000000227"), NameAr = "جنوب سيناء", NameEn = "South Sinai" };

        var governorates = new List<Governorate>
        {
            gov_cairo, gov_giza, gov_benisuef, gov_minya, gov_asyut, gov_sohag, gov_qena, gov_luxor, gov_aswan, gov_alexandria,
            gov_gharbia, gov_beheira, gov_portsaid, gov_suez, gov_damietta, gov_dakahlia, gov_sharqia, gov_monufia,
            gov_qalyubia, gov_kafr_el_sheikh, gov_ismailia, gov_faiyum, gov_red_sea, gov_new_valley, gov_matrouh,
            gov_north_sinai, gov_south_sinai
        };
        context.Governorates.AddRange(governorates);

        // Create Cities
        var cairo = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000101"), NameAr = "القاهرة", NameEn = "Cairo", GovernorateId = gov_cairo.Id };
        var giza = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000102"), NameAr = "الجيزة", NameEn = "Giza", GovernorateId = gov_giza.Id };
        var benisuef = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000103"), NameAr = "بني سويف", NameEn = "Beni Suef", GovernorateId = gov_benisuef.Id };
        var minya = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000104"), NameAr = "المنيا", NameEn = "Minya", GovernorateId = gov_minya.Id };
        var asyut = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000105"), NameAr = "أسيوط", NameEn = "Asyut", GovernorateId = gov_asyut.Id };
        var sohag = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000106"), NameAr = "سوهاج", NameEn = "Sohag", GovernorateId = gov_sohag.Id };
        var qena = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000107"), NameAr = "قنا", NameEn = "Qena", GovernorateId = gov_qena.Id };
        var luxor = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000108"), NameAr = "الأقصر", NameEn = "Luxor", GovernorateId = gov_luxor.Id };
        var aswan = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000109"), NameAr = "أسوان", NameEn = "Aswan", GovernorateId = gov_aswan.Id };
        var alexandria = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000110"), NameAr = "الإسكندرية", NameEn = "Alexandria", GovernorateId = gov_alexandria.Id };
        var gharbia = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000111"), NameAr = "الغربية", NameEn = "Gharbia", GovernorateId = gov_gharbia.Id };
        var beheira = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000112"), NameAr = "البحيرة", NameEn = "Beheira", GovernorateId = gov_beheira.Id };
        var portsaid = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000113"), NameAr = "بورسعيد", NameEn = "Port Said", GovernorateId = gov_portsaid.Id };
        var suez = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000114"), NameAr = "السويس", NameEn = "Suez", GovernorateId = gov_suez.Id };
        var damietta = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000115"), NameAr = "دمياط", NameEn = "Damietta", GovernorateId = gov_damietta.Id };
        var dakahlia = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000116"), NameAr = "الدقهلية", NameEn = "Dakahlia", GovernorateId = gov_dakahlia.Id };
        var sharqia = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000117"), NameAr = "الشرقية", NameEn = "Sharqia", GovernorateId = gov_sharqia.Id };
        var monufia = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000118"), NameAr = "المنوفية", NameEn = "Monufia", GovernorateId = gov_monufia.Id };
        var qalyubia = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000119"), NameAr = "القليوبية", NameEn = "Qalyubia", GovernorateId = gov_qalyubia.Id };
        var kafr_el_sheikh = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000120"), NameAr = "كفر الشيخ", NameEn = "Kafr El Sheikh", GovernorateId = gov_kafr_el_sheikh.Id };
        var ismailia = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000121"), NameAr = "الإسماعيلية", NameEn = "Ismailia", GovernorateId = gov_ismailia.Id };
        var faiyum = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000122"), NameAr = "الفيوم", NameEn = "Faiyum", GovernorateId = gov_faiyum.Id };
        var red_sea = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000123"), NameAr = "البحر الأحمر", NameEn = "Red Sea", GovernorateId = gov_red_sea.Id };
        var new_valley = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000124"), NameAr = "الوادي الجديد", NameEn = "New Valley", GovernorateId = gov_new_valley.Id };
        var matrouh = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000125"), NameAr = "مطروح", NameEn = "Matrouh", GovernorateId = gov_matrouh.Id };
        var north_sinai = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000126"), NameAr = "شمال سيناء", NameEn = "North Sinai", GovernorateId = gov_north_sinai.Id };
        var south_sinai = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000127"), NameAr = "جنوب سيناء", NameEn = "South Sinai", GovernorateId = gov_south_sinai.Id };

        var tanta = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000128"), NameAr = "طنطا", NameEn = "Tanta", GovernorateId = gov_gharbia.Id };
        var damanhour = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000129"), NameAr = "دمنهور", NameEn = "Damanhour", GovernorateId = gov_beheira.Id };
        var sidi_gaber = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000130"), NameAr = "سيدي جابر", NameEn = "Sidi Gaber", GovernorateId = gov_alexandria.Id };
        var tima = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000131"), NameAr = "طما", NameEn = "Tima", GovernorateId = gov_sohag.Id };
        var tahta = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000132"), NameAr = "طهطا", NameEn = "Tahta", GovernorateId = gov_sohag.Id };
        var girga = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000133"), NameAr = "جرجا", NameEn = "Girga", GovernorateId = gov_sohag.Id };
        var nagaa_hammadi = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000134"), NameAr = "نجع حمادي", NameEn = "Nagaa Hammadi", GovernorateId = gov_qena.Id };
        var edfu = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000135"), NameAr = "إدفو", NameEn = "Edfu", GovernorateId = gov_aswan.Id };
        var kalabsha = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000136"), NameAr = "كلابشة", NameEn = "Kalabsha", GovernorateId = gov_aswan.Id };
        var kom_ombo = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000137"), NameAr = "كوم أمبو", NameEn = "Kom Ombo", GovernorateId = gov_aswan.Id };
        var esna = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000138"), NameAr = "إسنا", NameEn = "Esna", GovernorateId = gov_luxor.Id };
        var daraw = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000139"), NameAr = "دراو", NameEn = "Daraw", GovernorateId = gov_aswan.Id };
        var mallawi = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000140"), NameAr = "ملوي", NameEn = "Mallawi", GovernorateId = gov_minya.Id };
        var abou_tig = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000141"), NameAr = "أبو تيج", NameEn = "Abou Tij", GovernorateId = gov_asyut.Id };
        var balyana = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000142"), NameAr = "البلينا", NameEn = "Balyana", GovernorateId = gov_sohag.Id };
        var samalut = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000143"), NameAr = "سمالوط", NameEn = "Samalut", GovernorateId = gov_minya.Id };
        var manfalut = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000144"), NameAr = "منفلوط", NameEn = "Manfalut", GovernorateId = gov_asyut.Id };
        var mansoura = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000145"), NameAr = "المنصورة", NameEn = "Mansoura", GovernorateId = gov_dakahlia.Id };
        var zagazig = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000146"), NameAr = "الزقازيق", NameEn = "Zagazig", GovernorateId = gov_sharqia.Id };
        var shibin_el_kom = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000147"), NameAr = "شبين الكوم", NameEn = "Shibin El Kom", GovernorateId = gov_monufia.Id };
        var hurghada = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000148"), NameAr = "الغردقة", NameEn = "Hurghada", GovernorateId = gov_red_sea.Id };
        var kharga = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000149"), NameAr = "الخارجة", NameEn = "Kharga", GovernorateId = gov_new_valley.Id };
        var marsa_matrouh = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000150"), NameAr = "مرسى مطروح", NameEn = "Marsa Matrouh", GovernorateId = gov_matrouh.Id };
        var arish = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000151"), NameAr = "العريش", NameEn = "Arish", GovernorateId = gov_north_sinai.Id };
        var el_tor = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000152"), NameAr = "الطور", NameEn = "El Tor", GovernorateId = gov_south_sinai.Id };
        var kafr_el_dawwar = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000153"), NameAr = "كفر الدوار", NameEn = "Kafr El Dawwar", GovernorateId = gov_beheira.Id };
        var talkha = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000154"), NameAr = "طلخا", NameEn = "Talkha", GovernorateId = gov_dakahlia.Id };
        var desouk = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000155"), NameAr = "دسوق", NameEn = "Desouk", GovernorateId = gov_kafr_el_sheikh.Id };
        var qalyub = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000156"), NameAr = "قليوب", NameEn = "Qalyub", GovernorateId = gov_qalyubia.Id };
        var abu_hummus = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000157"), NameAr = "أبو حمص", NameEn = "Abu Hummus", GovernorateId = gov_beheira.Id };
        var itay_el_baroud = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000158"), NameAr = "إيتاي البارود", NameEn = "Itay El Baroud", GovernorateId = gov_beheira.Id };
        var kafr_el_zayat = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000159"), NameAr = "كفر الزيات", NameEn = "Kafr El Zayat", GovernorateId = gov_gharbia.Id };
        var shubra_el_kheima = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000160"), NameAr = "شبرا الخيمة", NameEn = "Shubra El Kheima", GovernorateId = gov_qalyubia.Id };
        var maghagha = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000161"), NameAr = "مغاغة", NameEn = "Maghagha", GovernorateId = gov_minya.Id };
        var bani_mazar = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000162"), NameAr = "بني مزار", NameEn = "Bani Mazar", GovernorateId = gov_minya.Id };
        var abu_qurqas = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000163"), NameAr = "أبو قرقاص", NameEn = "Abu Qurqas", GovernorateId = gov_minya.Id };
        var deirut = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000164"), NameAr = "ديروط", NameEn = "Deirut", GovernorateId = gov_asyut.Id };
        var qusiya = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000165"), NameAr = "القوصية", NameEn = "Qusiya", GovernorateId = gov_asyut.Id };
        var quesna = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000166"), NameAr = "قويسنا", NameEn = "Quesna", GovernorateId = gov_monufia.Id };
        var helwan = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000167"), NameAr = "حلوان", NameEn = "Helwan", GovernorateId = gov_cairo.Id };
        var obour = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000168"), NameAr = "العبور", NameEn = "Obour", GovernorateId = gov_qalyubia.Id };
        var mit_ghamr = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000169"), NameAr = "ميت غمر", NameEn = "Mit Ghamr", GovernorateId = gov_dakahlia.Id };
        var zifta = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000170"), NameAr = "زفتى", NameEn = "Zifta", GovernorateId = gov_gharbia.Id };
        var senbellawein = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000171"), NameAr = "السنبلاوين", NameEn = "Senbellawein", GovernorateId = gov_dakahlia.Id };
        var el_fashn = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000172"), NameAr = "الفشن", NameEn = "El Fashn", GovernorateId = gov_benisuef.Id };
        var biba = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000173"), NameAr = "ببا", NameEn = "Biba", GovernorateId = gov_benisuef.Id };
        var nasser = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000174"), NameAr = "ناصر", NameEn = "Nasser", GovernorateId = gov_benisuef.Id };
        var akhmim = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000175"), NameAr = "أخميم", NameEn = "Akhmim", GovernorateId = gov_sohag.Id };
        var juhayna = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000176"), NameAr = "جهينة", NameEn = "Juhayna", GovernorateId = gov_sohag.Id };
        var el_maragha = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000177"), NameAr = "المراغة", NameEn = "El Maragha", GovernorateId = gov_sohag.Id };
        var abu_simbel = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000178"), NameAr = "أبو سمبل", NameEn = "Abu Simbel", GovernorateId = gov_aswan.Id };
        var ras_el_bar = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000179"), NameAr = "رأس البر", NameEn = "Ras El Bar", GovernorateId = gov_damietta.Id };
        var sharm_el_sheikh = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000180"), NameAr = "شرم الشيخ", NameEn = "Sharm El Sheikh", GovernorateId = gov_south_sinai.Id };
        var dahab = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000181"), NameAr = "دهب", NameEn = "Dahab", GovernorateId = gov_south_sinai.Id };
        var el_alamein = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000182"), NameAr = "العلمين", NameEn = "El Alamein", GovernorateId = gov_matrouh.Id };

        // Additional cities needed for governorates
        var ashmant = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000183"), NameAr = "اشمنت", NameEn = "Ashmant", GovernorateId = gov_benisuef.Id };
        var atwab = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000184"), NameAr = "اطواب", NameEn = "Atwab", GovernorateId = gov_benisuef.Id };
        var raqqah = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000185"), NameAr = "الرقة", NameEn = "Raqqah", GovernorateId = gov_benisuef.Id };
        var zaytoun_q = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000186"), NameAr = "الزيتون", NameEn = "Zaytoun Q", GovernorateId = gov_benisuef.Id };
        var fant = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000187"), NameAr = "الفنت", NameEn = "Fant", GovernorateId = gov_benisuef.Id };
        var maymun = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000188"), NameAr = "الميمون", NameEn = "Maymun", GovernorateId = gov_benisuef.Id };
        var aba_el_waqf = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000189"), NameAr = "ابا الوقف", NameEn = "Aba el-Waqf", GovernorateId = gov_minya.Id };
        var sidfa = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000190"), NameAr = "صدفا", NameEn = "Sidfa", GovernorateId = gov_asyut.Id };
        var monshaa = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000191"), NameAr = "المنشأة", NameEn = "Monshaa", GovernorateId = gov_sohag.Id };
        var usayrat = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000192"), NameAr = "العسيرات", NameEn = "Usayrat", GovernorateId = gov_sohag.Id };
        var abo_tesht = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000193"), NameAr = "ابو تشت", NameEn = "Abo Tesht", GovernorateId = gov_qena.Id };
        var farshut = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000194"), NameAr = "فرشوط", NameEn = "Farshut", GovernorateId = gov_qena.Id };
        var dishna = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000195"), NameAr = "دشنا", NameEn = "Dishna", GovernorateId = gov_qena.Id };
        var qift = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000196"), NameAr = "قفط", NameEn = "Qift", GovernorateId = gov_qena.Id };
        var qus = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000197"), NameAr = "قوص", NameEn = "Qus", GovernorateId = gov_qena.Id };
        var ballana = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000198"), NameAr = "بلانة", NameEn = "Ballana", GovernorateId = gov_aswan.Id };
        var silwa_bahari = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000199"), NameAr = "سلوا بحري", NameEn = "Silwa Bahari", GovernorateId = gov_aswan.Id };
        var mahamid = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000200"), NameAr = "المحاميد", NameEn = "Mahamid", GovernorateId = gov_aswan.Id };
        var sibaiyyah = new City { Id = Guid.Parse("00000000-0000-0000-0000-000000000201"), NameAr = "السباعية", NameEn = "Sibaiyyah", GovernorateId = gov_aswan.Id };

        var cities = new List<City>
        {
            cairo, giza, benisuef, minya, asyut, sohag, qena, luxor, aswan, alexandria,
            gharbia, beheira, portsaid, suez, damietta, dakahlia, sharqia, monufia,
            qalyubia, kafr_el_sheikh, ismailia, faiyum, red_sea, new_valley, matrouh,
            north_sinai, south_sinai, tanta, damanhour, sidi_gaber, tima, tahta,
            girga, nagaa_hammadi, edfu, kalabsha, kom_ombo, esna, daraw, mallawi,
            abou_tig, balyana, samalut, manfalut, mansoura, zagazig, shibin_el_kom, hurghada,
            kharga, marsa_matrouh, arish, el_tor, kafr_el_dawwar, talkha, desouk,
            qalyub, abu_hummus, itay_el_baroud, kafr_el_zayat, shubra_el_kheima, maghagha, bani_mazar,
            abu_qurqas, deirut, qusiya, quesna, helwan, obour, mit_ghamr, zifta,
            senbellawein, el_fashn, biba, nasser, akhmim, juhayna, el_maragha, abu_simbel,
            ras_el_bar, sharm_el_sheikh, dahab, el_alamein,
            ashmant, atwab, raqqah, zaytoun_q, fant, maymun, aba_el_waqf, sidfa, monshaa,
            usayrat, abo_tesht, farshut, dishna, qift, qus, ballana, silwa_bahari, mahamid, sibaiyyah
        };
        context.Cities.AddRange(cities);

        // Create Stops
        var stops = new List<Stop>
        {
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), NameAr = "محطة رمسيس (القاهرة)", NameEn = "Cairo Central (Ramses)", Code = "CAI", CityId = cairo.Id, Latitude = 30.0626, Longitude = 31.2467, DescriptionAr = "المحطة الرئيسية بالقاهرة", DescriptionEn = "Main station in Cairo" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), NameAr = "الجيزة", NameEn = "Giza", Code = "GIZ", CityId = giza.Id, Latitude = 30.0091, Longitude = 31.2089, DescriptionAr = "محطة الجيزة", DescriptionEn = "Giza station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), NameAr = "بني سويف", NameEn = "Beni Suef", Code = "BSF", CityId = benisuef.Id, Latitude = 29.0661, Longitude = 31.0994 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), NameAr = "المنيا", NameEn = "Minya", Code = "MNY", CityId = minya.Id, Latitude = 28.1099, Longitude = 30.7503 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), NameAr = "أسيوط", NameEn = "Asyut", Code = "ASY", CityId = asyut.Id, Latitude = 27.1783, Longitude = 31.1859 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), NameAr = "سوهاج", NameEn = "Sohag", Code = "SOH", CityId = sohag.Id, Latitude = 26.5569, Longitude = 31.6948 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), NameAr = "قنا", NameEn = "Qena", Code = "QNA", CityId = qena.Id, Latitude = 26.1551, Longitude = 32.7160 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), NameAr = "الأقصر", NameEn = "Luxor", Code = "LXR", CityId = luxor.Id, Latitude = 25.6872, Longitude = 32.6396, DescriptionAr = "محطة المقصد السياحي الرئيسي", DescriptionEn = "Major tourist destination station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), NameAr = "أسوان", NameEn = "Aswan", Code = "ASW", CityId = aswan.Id, Latitude = 24.0889, Longitude = 32.8998, DescriptionAr = "المحطة النهائية الجنوبية", DescriptionEn = "Southern terminus" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000010"), NameAr = "الإسكندرية (محطة مصر)", NameEn = "Alexandria (Misr)", Code = "ALX", CityId = alexandria.Id, Latitude = 31.1928, Longitude = 29.9008, DescriptionAr = "محطة الإسكندرية الرئيسية", DescriptionEn = "Main Alexandria station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000011"), NameAr = "طنطا", NameEn = "Tanta", Code = "TAN", CityId = tanta.Id, Latitude = 30.7865, Longitude = 31.0004 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000012"), NameAr = "دمنهور", NameEn = "Damanhour", Code = "DMH", CityId = damanhour.Id, Latitude = 31.0344, Longitude = 30.4688 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000013"), NameAr = "سيدي جابر", NameEn = "Sidi Gaber", Code = "SDG", CityId = sidi_gaber.Id, Latitude = 31.2201, Longitude = 29.9383, DescriptionAr = "محطة سيدي جابر", DescriptionEn = "Sidi Gaber Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000014"), NameAr = "طما", NameEn = "Tima", Code = "TMA", CityId = tima.Id, Latitude = 26.9077, Longitude = 31.4397, DescriptionAr = "محطة طما", DescriptionEn = "Tima Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000015"), NameAr = "طهطا", NameEn = "Tahta", Code = "THT", CityId = tahta.Id, Latitude = 26.7686, Longitude = 31.4988, DescriptionAr = "محطة طهطا", DescriptionEn = "Tahta Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000016"), NameAr = "جرجا", NameEn = "Girga", Code = "GRG", CityId = girga.Id, Latitude = 26.3359, Longitude = 31.8887, DescriptionAr = "محطة جرجا", DescriptionEn = "Girga Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000017"), NameAr = "نجع حمادي", NameEn = "Nagaa Hammadi", Code = "NHM", CityId = nagaa_hammadi.Id, Latitude = 26.0494, Longitude = 32.2414, DescriptionAr = "محطة نجع حمادي", DescriptionEn = "Nagaa Hammadi Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000018"), NameAr = "إدفو", NameEn = "Edfu", Code = "EDF", CityId = edfu.Id, Latitude = 24.9781, Longitude = 32.8752, DescriptionAr = "محطة إدفو", DescriptionEn = "Edfu Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000019"), NameAr = "كلابشة", NameEn = "Kalabsha", Code = "KLB", CityId = kalabsha.Id, Latitude = 24.5020, Longitude = 32.9360, DescriptionAr = "محطة كلابشة", DescriptionEn = "Kalabsha Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000020"), NameAr = "كوم أمبو", NameEn = "Kom Ombo", Code = "KOB", CityId = kom_ombo.Id, Latitude = 24.4764, Longitude = 32.9469, DescriptionAr = "محطة كوم أمبو", DescriptionEn = "Kom Ombo Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000021"), NameAr = "إسنا", NameEn = "Esna", Code = "ESN", CityId = esna.Id, Latitude = 25.2934, Longitude = 32.5539, DescriptionAr = "محطة إسنا", DescriptionEn = "Esna Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000022"), NameAr = "دراو", NameEn = "Daraw", Code = "DRW", CityId = daraw.Id, Latitude = 24.3986, Longitude = 32.9213, DescriptionAr = "محطة دراو", DescriptionEn = "Daraw Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000023"), NameAr = "ملوي", NameEn = "Mallawi", Code = "MLW", CityId = mallawi.Id, Latitude = 27.7314, Longitude = 30.8417, DescriptionAr = "محطة ملوي", DescriptionEn = "Mallawi Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000024"), NameAr = "أبو تيج", NameEn = "Abou Tij", Code = "ABT", CityId = abou_tig.Id, Latitude = 27.0436, Longitude = 31.3181, DescriptionAr = "محطة أبو تيج", DescriptionEn = "Abou Tij Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000025"), NameAr = "البلينا", NameEn = "Balyana", Code = "BLY", CityId = balyana.Id, Latitude = 26.2307, Longitude = 31.9961, DescriptionAr = "محطة البلينا", DescriptionEn = "Balyana Station" },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000026"), NameAr = "اشمنت", NameEn = "Ashmant", Code = "ASM", CityId = ashmant.Id, Latitude = 29.1504, Longitude = 31.1180 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000027"), NameAr = "اطواب", NameEn = "Atwab", Code = "ATW", CityId = atwab.Id, Latitude = 29.2044, Longitude = 31.1299 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000028"), NameAr = "الرقة", NameEn = "Raqqah", Code = "RAQ", CityId = raqqah.Id, Latitude = 29.2600, Longitude = 31.1427 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000029"), NameAr = "الزيتون", NameEn = "Zaytoun Q", Code = "ZAY", CityId = zaytoun_q.Id, Latitude = 29.1007, Longitude = 31.1070 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000030"), NameAr = "الفشن", NameEn = "El Fashn", Code = "FAS", CityId = el_fashn.Id, Latitude = 28.8200, Longitude = 30.8900 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000031"), NameAr = "الفنت", NameEn = "Fant", Code = "FNT", CityId = fant.Id, Latitude = 28.7504, Longitude = 30.8494 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000032"), NameAr = "الميمون", NameEn = "Maymun", Code = "MAY", CityId = maymun.Id, Latitude = 29.2193, Longitude = 31.1332 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000033"), NameAr = "مغاغة", NameEn = "Maghagha", Code = "MAG", CityId = maghagha.Id, Latitude = 28.7000, Longitude = 30.8200 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000034"), NameAr = "بني مزار", NameEn = "Bani Mazar", Code = "BMZ", CityId = bani_mazar.Id, Latitude = 28.5004, Longitude = 30.7983 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000035"), NameAr = "سمالوط", NameEn = "Samalut", Code = "SAM", CityId = samalut.Id, Latitude = 28.3012, Longitude = 30.7639 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000036"), NameAr = "أبو قرقاص", NameEn = "Abu Qurqas", Code = "ABQ", CityId = abu_qurqas.Id, Latitude = 27.9384, Longitude = 30.7764 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000037"), NameAr = "ديروط", NameEn = "Deirut", Code = "DEI", CityId = deirut.Id, Latitude = 27.5528, Longitude = 30.9252 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000038"), NameAr = "القوصية", NameEn = "Qusiya", Code = "QUY", CityId = qusiya.Id, Latitude = 27.4412, Longitude = 30.9841 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000039"), NameAr = "منفلوط", NameEn = "Manfalut", Code = "MNF", CityId = manfalut.Id, Latitude = 27.3115, Longitude = 31.0446 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000040"), NameAr = "صدفا", NameEn = "Sidfa", Code = "SDF", CityId = sidfa.Id, Latitude = 26.8643, Longitude = 31.4566 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000041"), NameAr = "المراغة", NameEn = "El Maragha", Code = "MAR", CityId = el_maragha.Id, Latitude = 26.6602, Longitude = 31.5973 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000042"), NameAr = "المنشأة", NameEn = "Monshaa", Code = "MON", CityId = monshaa.Id, Latitude = 26.4602, Longitude = 31.7773 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000043"), NameAr = "العسيرات", NameEn = "Usayrat", Code = "USA", CityId = usayrat.Id, Latitude = 26.4014, Longitude = 31.8288 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000044"), NameAr = "ابو تشت", NameEn = "Abo Tesht", Code = "ATS", CityId = abo_tesht.Id, Latitude = 26.1221, Longitude = 32.1219 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000045"), NameAr = "فرشوط", NameEn = "Farshut", Code = "FAR", CityId = farshut.Id, Latitude = 26.0764, Longitude = 32.1784 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000053"), NameAr = "ابا الوقف", NameEn = "Aba el-Waqf", Code = "ABA", CityId = aba_el_waqf.Id, Latitude = 28.5998, Longitude = 30.8133 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000046"), NameAr = "دشنا", NameEn = "Dishna", Code = "DIS", CityId = dishna.Id, Latitude = 26.0704, Longitude = 32.5029 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000047"), NameAr = "قفط", NameEn = "Qift", Code = "QFT", CityId = qift.Id, Latitude = 26.0267, Longitude = 32.7764 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000048"), NameAr = "قوص", NameEn = "Qus", Code = "QUS", CityId = qus.Id, Latitude = 25.9230, Longitude = 32.7605 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000049"), NameAr = "بلانة", NameEn = "Ballana", Code = "BAL", CityId = ballana.Id, Latitude = 24.1493, Longitude = 32.8933 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000050"), NameAr = "سلوا بحري", NameEn = "Silwa Bahari", Code = "SIL", CityId = silwa_bahari.Id, Latitude = 24.6197, Longitude = 32.9367 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000051"), NameAr = "المحاميد", NameEn = "Mahamid", Code = "MAH", CityId = mahamid.Id, Latitude = 25.0316, Longitude = 32.8095 },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000052"), NameAr = "السباعية", NameEn = "Sibaiyyah", Code = "SIB", CityId = sibaiyyah.Id, Latitude = 25.1190, Longitude = 32.6993 },

        };
        context.Stops.AddRange(stops);

        // Train 1: Cairo → Aswan VIP Express (Train 980)
        var train1 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            TrainNumber = "980",
            NameAr = "قطار القاهرة - أسوان VIP السريع", NameEn = "Cairo - Aswan VIP Express",
            DescriptionAr = "خدمة ممتازة نهارية VIP من القاهرة إلى أسوان", DescriptionEn = "Daytime premium VIP service from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train1);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(8, 0, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(8, 20, 0), ScheduledDeparture = new TimeSpan(8, 25, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(9, 30, 0), ScheduledDeparture = new TimeSpan(9, 33, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(11, 0, 0), ScheduledDeparture = new TimeSpan(11, 5, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(13, 0, 0), ScheduledDeparture = new TimeSpan(13, 5, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(14, 45, 0), ScheduledDeparture = new TimeSpan(14, 50, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[15].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(15, 20, 0), ScheduledDeparture = new TimeSpan(15, 23, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[16].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(16, 15, 0), ScheduledDeparture = new TimeSpan(16, 20, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[6].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(17, 0, 0), ScheduledDeparture = new TimeSpan(17, 05, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[7].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(18, 15, 0), ScheduledDeparture = new TimeSpan(18, 25, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[20].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(19, 15, 0), ScheduledDeparture = new TimeSpan(19, 18, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[17].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(20, 10, 0), ScheduledDeparture = new TimeSpan(20, 15, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[18].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(21, 05, 0), ScheduledDeparture = new TimeSpan(21, 08, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[19].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(21, 30, 0), ScheduledDeparture = new TimeSpan(21, 33, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[21].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(21, 50, 0), ScheduledDeparture = new TimeSpan(21, 53, 0) },
            new TrainRouteStop { TrainId = train1.Id, StopId = stops[8].Id, StopOrder = 16, ScheduledArrival = new TimeSpan(22, 25, 0) }
        );

        // Train 2: Cairo → Alexandria Express
        var train2 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            TrainNumber = "903",
            NameAr = "قطار القاهرة - الإسكندرية السريع", NameEn = "Cairo - Alexandria Express",
            DescriptionAr = "خدمة سريعة ممتازة من القاهرة إلى الإسكندرية", DescriptionEn = "Fast express service Cairo to Alexandria",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train2);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train2.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(8, 0, 0) },
            new TrainRouteStop { TrainId = train2.Id, StopId = stops[10].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(9, 15, 0), ScheduledDeparture = new TimeSpan(9, 18, 0) },
            new TrainRouteStop { TrainId = train2.Id, StopId = stops[11].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(9, 55, 0), ScheduledDeparture = new TimeSpan(9, 58, 0) },
            new TrainRouteStop { TrainId = train2.Id, StopId = stops[9].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(10, 30, 0) }
        );

        // Train 3: Cairo → Aswan AC Spanish Express (Train 996)
        var train3 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            TrainNumber = "996",
            NameAr = "قطار القاهرة - أسوان الإسباني المكيف السريع", NameEn = "Cairo - Aswan AC Spanish Express",
            DescriptionAr = "خدمة سريعة إسبانية مريحة ومكيفة من القاهرة إلى أسوان", DescriptionEn = "Comfortable air-conditioned Spanish express service from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train3);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(22, 0, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(22, 20, 0), ScheduledDeparture = new TimeSpan(22, 25, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(23, 35, 0), ScheduledDeparture = new TimeSpan(23, 38, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(1, 15, 0), ScheduledDeparture = new TimeSpan(1, 20, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(3, 20, 0), ScheduledDeparture = new TimeSpan(3, 25, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(5, 10, 0), ScheduledDeparture = new TimeSpan(5, 15, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[15].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(5, 50, 0), ScheduledDeparture = new TimeSpan(5, 53, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[16].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(6, 50, 0), ScheduledDeparture = new TimeSpan(6, 55, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[6].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(7, 40, 0), ScheduledDeparture = new TimeSpan(7, 45, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[7].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(8, 50, 0), ScheduledDeparture = new TimeSpan(9, 0, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[20].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(9, 50, 0), ScheduledDeparture = new TimeSpan(9, 53, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[17].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(10, 40, 0), ScheduledDeparture = new TimeSpan(10, 45, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[19].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(11, 50, 0), ScheduledDeparture = new TimeSpan(11, 53, 0) },
            new TrainRouteStop { TrainId = train3.Id, StopId = stops[8].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(12, 40, 0) }
        );

        // Train 4: Alexandria - Aswan VIP Special Express
        var train4 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
            TrainNumber = "2008",
            NameAr = "قطار الإسكندرية - أسوان VIP الخاص السريع", NameEn = "Alexandria - Aswan VIP Special Express",
            DescriptionAr = "خدمة مباشرة ممتازة VIP من الإسكندرية إلى أسوان", DescriptionEn = "Direct premium VIP service from Alexandria to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train4);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[9].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(20, 0, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[12].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(20, 12, 0), ScheduledDeparture = new TimeSpan(20, 15, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[11].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(20, 55, 0), ScheduledDeparture = new TimeSpan(20, 57, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[10].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(21, 40, 0), ScheduledDeparture = new TimeSpan(21, 43, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[0].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(22, 50, 0), ScheduledDeparture = new TimeSpan(23, 0, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[1].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(23, 25, 0), ScheduledDeparture = new TimeSpan(23, 30, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[2].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(0, 45, 0), ScheduledDeparture = new TimeSpan(0, 47, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(2, 25, 0), ScheduledDeparture = new TimeSpan(2, 28, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[4].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 30, 0), ScheduledDeparture = new TimeSpan(4, 35, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[5].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(6, 0, 0), ScheduledDeparture = new TimeSpan(6, 5, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[6].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(7, 45, 0), ScheduledDeparture = new TimeSpan(7, 50, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[7].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(8, 45, 0), ScheduledDeparture = new TimeSpan(8, 55, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[17].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(10, 15, 0), ScheduledDeparture = new TimeSpan(10, 18, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[19].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(11, 10, 0), ScheduledDeparture = new TimeSpan(11, 13, 0) },
            new TrainRouteStop { TrainId = train4.Id, StopId = stops[8].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(11, 45, 0) }
        );

        // Train 5: Alexandria - Aswan AC Spanish Express
        var train5 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
            TrainNumber = "1902",
            NameAr = "قطار الإسكندرية - أسوان الإسباني المكيف السريع", NameEn = "Alexandria - Aswan AC Spanish Express",
            DescriptionAr = "خدمة سريعة إسبانية مريحة ومكيفة من الإسكندرية إلى أسوان", DescriptionEn = "Comfortable air-conditioned Spanish express service from Alexandria to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train5);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[9].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(20, 10, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[12].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(20, 22, 0), ScheduledDeparture = new TimeSpan(20, 25, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[11].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(21, 5, 0), ScheduledDeparture = new TimeSpan(21, 7, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[10].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(21, 50, 0), ScheduledDeparture = new TimeSpan(21, 53, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[0].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(23, 5, 0), ScheduledDeparture = new TimeSpan(23, 15, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[1].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(23, 40, 0), ScheduledDeparture = new TimeSpan(23, 45, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[2].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(1, 0, 0), ScheduledDeparture = new TimeSpan(1, 2, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(2, 45, 0), ScheduledDeparture = new TimeSpan(2, 48, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[4].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 55, 0), ScheduledDeparture = new TimeSpan(5, 0, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[13].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(5, 40, 0), ScheduledDeparture = new TimeSpan(5, 42, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[14].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(6, 5, 0), ScheduledDeparture = new TimeSpan(6, 7, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[5].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(6, 35, 0), ScheduledDeparture = new TimeSpan(6, 40, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[15].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(7, 15, 0), ScheduledDeparture = new TimeSpan(7, 18, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[16].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(8, 5, 0), ScheduledDeparture = new TimeSpan(8, 10, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[6].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(8, 45, 0), ScheduledDeparture = new TimeSpan(8, 50, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[7].Id, StopOrder = 16, ScheduledArrival = new TimeSpan(9, 50, 0), ScheduledDeparture = new TimeSpan(10, 0, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[17].Id, StopOrder = 17, ScheduledArrival = new TimeSpan(11, 30, 0), ScheduledDeparture = new TimeSpan(11, 33, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[19].Id, StopOrder = 18, ScheduledArrival = new TimeSpan(12, 35, 0), ScheduledDeparture = new TimeSpan(12, 38, 0) },
            new TrainRouteStop { TrainId = train5.Id, StopId = stops[8].Id, StopOrder = 19, ScheduledArrival = new TimeSpan(13, 20, 0) }
        );

        // Train 6: Alexandria - Aswan AC Russian Express
        var train6 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
            TrainNumber = "3008",
            NameAr = "قطار الإسكندرية - أسوان الروسي المكيف السريع", NameEn = "Alexandria - Aswan AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية حديثة ومكيفة من الإسكندرية إلى أسوان", DescriptionEn = "Modern air-conditioned Russian passenger service from Alexandria to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train6);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[9].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(5, 45, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[12].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(5, 55, 0), ScheduledDeparture = new TimeSpan(5, 58, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[11].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(6, 39, 0), ScheduledDeparture = new TimeSpan(6, 42, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[10].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(7, 24, 0), ScheduledDeparture = new TimeSpan(7, 28, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[0].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(8, 25, 0), ScheduledDeparture = new TimeSpan(8, 40, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[1].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(9, 0, 0), ScheduledDeparture = new TimeSpan(9, 5, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[2].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(10, 25, 0), ScheduledDeparture = new TimeSpan(10, 28, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(12, 10, 0), ScheduledDeparture = new TimeSpan(12, 15, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[4].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(14, 18, 0), ScheduledDeparture = new TimeSpan(14, 25, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[5].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(15, 55, 0), ScheduledDeparture = new TimeSpan(16, 0, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[16].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(17, 20, 0), ScheduledDeparture = new TimeSpan(17, 25, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[7].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(19, 0, 0), ScheduledDeparture = new TimeSpan(19, 10, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[17].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(20, 43, 0), ScheduledDeparture = new TimeSpan(20, 48, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[19].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(21, 44, 0), ScheduledDeparture = new TimeSpan(21, 49, 0) },
            new TrainRouteStop { TrainId = train6.Id, StopId = stops[8].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(22, 25, 0) }
        );

        // Train 7: Cairo - Aswan VIP Express (Train 2010)
        var train7 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000007"),
            TrainNumber = "2010",
            NameAr = "قطار القاهرة - أسوان VIP السريع", NameEn = "Cairo - Aswan VIP Express",
            DescriptionAr = "خدمة سريعة ممتازة ومكيفة VIP من القاهرة إلى أسوان", DescriptionEn = "Premium air-conditioned VIP express from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train7);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(10, 0, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(10, 20, 0), ScheduledDeparture = new TimeSpan(10, 25, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[3].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(12, 45, 0), ScheduledDeparture = new TimeSpan(12, 50, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[4].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(14, 35, 0), ScheduledDeparture = new TimeSpan(14, 40, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[5].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(16, 15, 0), ScheduledDeparture = new TimeSpan(16, 20, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(17, 25, 0), ScheduledDeparture = new TimeSpan(17, 28, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[6].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(18, 05, 0), ScheduledDeparture = new TimeSpan(18, 10, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[7].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(19, 10, 0), ScheduledDeparture = new TimeSpan(19, 20, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[20].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(20, 05, 0), ScheduledDeparture = new TimeSpan(20, 08, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[17].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(20, 55, 0), ScheduledDeparture = new TimeSpan(21, 0, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[18].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(21, 40, 0), ScheduledDeparture = new TimeSpan(21, 42, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[19].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(22, 05, 0), ScheduledDeparture = new TimeSpan(22, 08, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[21].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(22, 22, 0), ScheduledDeparture = new TimeSpan(22, 25, 0) },
            new TrainRouteStop { TrainId = train7.Id, StopId = stops[8].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(22, 55, 0) }
        );

        // Train 8: Cairo - Aswan VIP Express (Train 982)
        var train8 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000008"),
            TrainNumber = "982",
            NameAr = "قطار القاهرة - أسوان VIP السريع", NameEn = "Cairo - Aswan VIP Express",
            DescriptionAr = "خدمة ممتازة بعد الظهر VIP من القاهرة إلى أسوان", DescriptionEn = "Afternoon premium VIP train Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train8);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(12, 0, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(12, 20, 0), ScheduledDeparture = new TimeSpan(12, 25, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(13, 30, 0), ScheduledDeparture = new TimeSpan(13, 33, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(15, 05, 0), ScheduledDeparture = new TimeSpan(15, 10, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(17, 05, 0), ScheduledDeparture = new TimeSpan(17, 10, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[22].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(17, 45, 0), ScheduledDeparture = new TimeSpan(17, 47, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[23].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(18, 05, 0), ScheduledDeparture = new TimeSpan(18, 07, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[5].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(18, 40, 0), ScheduledDeparture = new TimeSpan(18, 45, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[15].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(19, 15, 0), ScheduledDeparture = new TimeSpan(19, 18, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[16].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(20, 10, 0), ScheduledDeparture = new TimeSpan(20, 15, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[6].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(20, 55, 0), ScheduledDeparture = new TimeSpan(21, 0, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[7].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(22, 10, 0), ScheduledDeparture = new TimeSpan(22, 20, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[17].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(0, 05, 0), ScheduledDeparture = new TimeSpan(0, 10, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[19].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(1, 25, 0), ScheduledDeparture = new TimeSpan(1, 28, 0) },
            new TrainRouteStop { TrainId = train8.Id, StopId = stops[8].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(2, 24, 0) }
        );

        // Train 9: Cairo - Aswan VIP Express (Train 2006)
        var train9 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000009"),
            TrainNumber = "2006",
            NameAr = "قطار القاهرة - أسوان VIP السريع", NameEn = "Cairo - Aswan VIP Express",
            DescriptionAr = "خدمة ليلية VIP من القاهرة إلى أسوان", DescriptionEn = "Night VIP train service Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train9);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(17, 15, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(17, 35, 0), ScheduledDeparture = new TimeSpan(17, 40, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[3].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(20, 0, 0), ScheduledDeparture = new TimeSpan(20, 05, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[4].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(21, 50, 0), ScheduledDeparture = new TimeSpan(21, 55, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[5].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(23, 25, 0), ScheduledDeparture = new TimeSpan(23, 30, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(0, 35, 0), ScheduledDeparture = new TimeSpan(0, 38, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[6].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(1, 15, 0), ScheduledDeparture = new TimeSpan(1, 20, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[7].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(2, 20, 0), ScheduledDeparture = new TimeSpan(2, 30, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[17].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 0, 0), ScheduledDeparture = new TimeSpan(4, 05, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[19].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(5, 10, 0), ScheduledDeparture = new TimeSpan(5, 13, 0) },
            new TrainRouteStop { TrainId = train9.Id, StopId = stops[8].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(6, 0, 0) }
        );

        // Train 10: Cairo - Aswan AC Spanish Express (Train 988)
        var train10 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000010"),
            TrainNumber = "988",
            NameAr = "قطار القاهرة - أسوان الإسباني المكيف السريع", NameEn = "Cairo - Aswan AC Spanish Express",
            DescriptionAr = "خدمة ليلية إسبانية مكيفة من القاهرة إلى أسوان", DescriptionEn = "Overnight air-conditioned Spanish express service from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train10);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(19, 0, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(19, 20, 0), ScheduledDeparture = new TimeSpan(19, 25, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(20, 30, 0), ScheduledDeparture = new TimeSpan(20, 33, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(22, 0, 0), ScheduledDeparture = new TimeSpan(22, 5, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(0, 5, 0), ScheduledDeparture = new TimeSpan(0, 10, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(1, 50, 0), ScheduledDeparture = new TimeSpan(1, 55, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[16].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(3, 0, 0), ScheduledDeparture = new TimeSpan(3, 3, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[6].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(3, 40, 0), ScheduledDeparture = new TimeSpan(3, 45, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[7].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 50, 0), ScheduledDeparture = new TimeSpan(5, 0, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[17].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(6, 30, 0), ScheduledDeparture = new TimeSpan(6, 35, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[19].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(7, 15, 0), ScheduledDeparture = new TimeSpan(7, 18, 0) },
            new TrainRouteStop { TrainId = train10.Id, StopId = stops[8].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(7, 50, 0) }
        );

        // Train 11: Cairo - Aswan AC Spanish Express (Train 986)
        var train11 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000011"),
            TrainNumber = "986",
            NameAr = "قطار القاهرة - أسوان الإسباني المكيف السريع", NameEn = "Cairo - Aswan AC Spanish Express",
            DescriptionAr = "خدمة نهارية إسبانية مكيفة من القاهرة إلى أسوان", DescriptionEn = "Daytime air-conditioned Spanish express service from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train11);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(9, 45, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(10, 5, 0), ScheduledDeparture = new TimeSpan(10, 10, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[3].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(12, 40, 0), ScheduledDeparture = new TimeSpan(12, 45, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[4].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(14, 40, 0), ScheduledDeparture = new TimeSpan(14, 45, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[5].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(16, 30, 0), ScheduledDeparture = new TimeSpan(16, 35, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(17, 40, 0), ScheduledDeparture = new TimeSpan(17, 43, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[6].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(18, 20, 0), ScheduledDeparture = new TimeSpan(18, 25, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[7].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(19, 30, 0), ScheduledDeparture = new TimeSpan(19, 40, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[17].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(21, 10, 0), ScheduledDeparture = new TimeSpan(21, 15, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[19].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(21, 50, 0), ScheduledDeparture = new TimeSpan(21, 53, 0) },
            new TrainRouteStop { TrainId = train11.Id, StopId = stops[8].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(22, 20, 0) }
        );

        // Train 12: Cairo - Aswan AC Russian Express (Train 978)
        var train12 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000012"),
            TrainNumber = "978",
            NameAr = "Cairo - Aswan AC Russian Express", NameEn = "Cairo - Aswan AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية مكيفة في الصباح الباكر من القاهرة إلى أسوان", DescriptionEn = "Early morning air-conditioned Russian passenger service from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train12);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(6, 30, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(6, 50, 0), ScheduledDeparture = new TimeSpan(6, 55, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(8, 0, 0), ScheduledDeparture = new TimeSpan(8, 3, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(9, 30, 0), ScheduledDeparture = new TimeSpan(9, 35, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(11, 30, 0), ScheduledDeparture = new TimeSpan(11, 35, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(13, 10, 0), ScheduledDeparture = new TimeSpan(13, 15, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[15].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(13, 45, 0), ScheduledDeparture = new TimeSpan(13, 48, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[16].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(14, 40, 0), ScheduledDeparture = new TimeSpan(14, 45, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[6].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(15, 25, 0), ScheduledDeparture = new TimeSpan(15, 30, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[7].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(16, 30, 0), ScheduledDeparture = new TimeSpan(16, 40, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[17].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(18, 10, 0), ScheduledDeparture = new TimeSpan(18, 15, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[19].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(18, 50, 0), ScheduledDeparture = new TimeSpan(18, 53, 0) },
            new TrainRouteStop { TrainId = train12.Id, StopId = stops[8].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(19, 25, 0) }
        );

        // Train 13: Cairo - Aswan AC Russian Express (Train 2012)
        var train13 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000013"),
            TrainNumber = "2012",
            NameAr = "Cairo - Aswan AC Russian Express", NameEn = "Cairo - Aswan AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية مكيفة مساءً من القاهرة إلى أسوان", DescriptionEn = "Evening air-conditioned Russian passenger service from Cairo to Aswan",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train13);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(17, 30, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(17, 50, 0), ScheduledDeparture = new TimeSpan(17, 55, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[3].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(20, 10, 0), ScheduledDeparture = new TimeSpan(20, 15, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[4].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(22, 10, 0), ScheduledDeparture = new TimeSpan(22, 15, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[5].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(23, 50, 0), ScheduledDeparture = new TimeSpan(23, 55, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(0, 55, 0), ScheduledDeparture = new TimeSpan(0, 58, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[6].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(1, 35, 0), ScheduledDeparture = new TimeSpan(1, 40, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[7].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(2, 40, 0), ScheduledDeparture = new TimeSpan(2, 50, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[17].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 30, 0), ScheduledDeparture = new TimeSpan(4, 35, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[19].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(5, 10, 0), ScheduledDeparture = new TimeSpan(5, 13, 0) },
            new TrainRouteStop { TrainId = train13.Id, StopId = stops[8].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(5, 45, 0) }
        );

        // Train 14: Cairo - Luxor VIP Express (Train 976)
        var train14 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000014"),
            TrainNumber = "976",
            NameAr = "قطار القاهرة - الأقصر VIP السريع", NameEn = "Cairo - Luxor VIP Express",
            DescriptionAr = "خدمة ممتازة VIP من القاهرة إلى الأقصر", DescriptionEn = "Premium VIP service from Cairo to Luxor",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train14);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(20, 0, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(20, 20, 0), ScheduledDeparture = new TimeSpan(20, 25, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(21, 30, 0), ScheduledDeparture = new TimeSpan(21, 33, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(23, 0, 0), ScheduledDeparture = new TimeSpan(23, 5, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(1, 0, 0), ScheduledDeparture = new TimeSpan(1, 5, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[13].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(1, 40, 0), ScheduledDeparture = new TimeSpan(1, 42, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[14].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(2, 0, 0), ScheduledDeparture = new TimeSpan(2, 2, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[5].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(2, 40, 0), ScheduledDeparture = new TimeSpan(2, 45, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[15].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(3, 15, 0), ScheduledDeparture = new TimeSpan(3, 18, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[24].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(3, 35, 0), ScheduledDeparture = new TimeSpan(3, 38, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[16].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(4, 30, 0), ScheduledDeparture = new TimeSpan(4, 35, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[6].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(5, 15, 0), ScheduledDeparture = new TimeSpan(5, 20, 0) },
            new TrainRouteStop { TrainId = train14.Id, StopId = stops[7].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(6, 15, 0) }
        );

        // Train 15: Alexandria - Luxor VIP Express (Train 934)
        var train15 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000015"),
            TrainNumber = "934",
            NameAr = "قطار الإسكندرية - الأقصر VIP السريع", NameEn = "Alexandria - Luxor VIP Express",
            DescriptionAr = "خدمة ممتازة VIP من الإسكندرية إلى الأقصر عبر القاهرة", DescriptionEn = "Premium VIP service from Alexandria to Luxor via Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train15);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[9].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(22, 0, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[12].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(22, 12, 0), ScheduledDeparture = new TimeSpan(22, 15, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[11].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(22, 55, 0), ScheduledDeparture = new TimeSpan(22, 57, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[10].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(23, 40, 0), ScheduledDeparture = new TimeSpan(23, 43, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[0].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(1, 0, 0), ScheduledDeparture = new TimeSpan(1, 10, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[1].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(1, 30, 0), ScheduledDeparture = new TimeSpan(1, 35, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[2].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(2, 40, 0), ScheduledDeparture = new TimeSpan(2, 43, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(4, 10, 0), ScheduledDeparture = new TimeSpan(4, 15, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[4].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(6, 10, 0), ScheduledDeparture = new TimeSpan(6, 15, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[13].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(6, 50, 0), ScheduledDeparture = new TimeSpan(6, 52, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[14].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(7, 10, 0), ScheduledDeparture = new TimeSpan(7, 12, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[5].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(7, 45, 0), ScheduledDeparture = new TimeSpan(7, 50, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[15].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(8, 20, 0), ScheduledDeparture = new TimeSpan(8, 23, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[16].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(9, 15, 0), ScheduledDeparture = new TimeSpan(9, 20, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[6].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(10, 0, 0), ScheduledDeparture = new TimeSpan(10, 5, 0) },
            new TrainRouteStop { TrainId = train15.Id, StopId = stops[7].Id, StopOrder = 16, ScheduledArrival = new TimeSpan(11, 0, 0) }
        );

        // Train 16: Cairo - Luxor AC Russian Express (Train 80)
        var train16 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000016"),
            TrainNumber = "80",
            NameAr = "قطار القاهرة - الأقصر الروسي المكيف السريع", NameEn = "Cairo - Luxor AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية مكيفة عادية من القاهرة إلى الأقصر", DescriptionEn = "Standard air-conditioned Russian passenger service from Cairo to Luxor",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train16);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(9, 0, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(9, 20, 0), ScheduledDeparture = new TimeSpan(9, 25, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(10, 35, 0), ScheduledDeparture = new TimeSpan(10, 38, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(12, 20, 0), ScheduledDeparture = new TimeSpan(12, 25, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[4].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(14, 30, 0), ScheduledDeparture = new TimeSpan(14, 35, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(16, 20, 0), ScheduledDeparture = new TimeSpan(16, 25, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[15].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(17, 0, 0), ScheduledDeparture = new TimeSpan(17, 3, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[16].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(18, 0, 0), ScheduledDeparture = new TimeSpan(18, 5, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[6].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(18, 45, 0), ScheduledDeparture = new TimeSpan(18, 50, 0) },
            new TrainRouteStop { TrainId = train16.Id, StopId = stops[7].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(19, 40, 0) }
        );

        // Train 17: Alexandria - Luxor AC Russian Express (Train 158)
        var train17 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000017"),
            TrainNumber = "158",
            NameAr = "قطار الإسكندرية - الأقصر الروسي المكيف السريع", NameEn = "Alexandria - Luxor AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية مكيفة من الإسكندرية إلى الأقصر عبر القاهرة", DescriptionEn = "Air-conditioned Russian passenger service from Alexandria to Luxor via Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train17);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[9].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(7, 15, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[12].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(7, 27, 0), ScheduledDeparture = new TimeSpan(7, 30, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[11].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(8, 10, 0), ScheduledDeparture = new TimeSpan(8, 13, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[10].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(8, 55, 0), ScheduledDeparture = new TimeSpan(8, 58, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[0].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(10, 15, 0), ScheduledDeparture = new TimeSpan(10, 30, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[1].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(10, 50, 0), ScheduledDeparture = new TimeSpan(10, 55, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[2].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(12, 5, 0), ScheduledDeparture = new TimeSpan(12, 8, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(13, 50, 0), ScheduledDeparture = new TimeSpan(13, 55, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[4].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(15, 55, 0), ScheduledDeparture = new TimeSpan(16, 0, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[5].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(17, 40, 0), ScheduledDeparture = new TimeSpan(17, 45, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[15].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(18, 20, 0), ScheduledDeparture = new TimeSpan(18, 23, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[16].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(19, 20, 0), ScheduledDeparture = new TimeSpan(19, 25, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[6].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(20, 5, 0), ScheduledDeparture = new TimeSpan(20, 10, 0) },
            new TrainRouteStop { TrainId = train17.Id, StopId = stops[7].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(21, 0, 0) }
        );

        // Train 18: Cairo - Asyut AC Russian Express (Train 972)
        var train18 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000018"),
            TrainNumber = "972",
            NameAr = "قطار القاهرة - أسيوط الروسي المكيف السريع", NameEn = "Cairo - Asyut AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية مكيفة من القاهرة إلى أسيوط", DescriptionEn = "Air-conditioned Russian passenger service from Cairo to Asyut",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train18);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[0].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(14, 20, 0) },
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[1].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(14, 40, 0), ScheduledDeparture = new TimeSpan(14, 45, 0) },
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[2].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(15, 55, 0), ScheduledDeparture = new TimeSpan(15, 58, 0) },
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[3].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(18, 20, 0), ScheduledDeparture = new TimeSpan(18, 25, 0) },
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[22].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(19, 30, 0), ScheduledDeparture = new TimeSpan(19, 33, 0) },
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[23].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(21, 05, 0), ScheduledDeparture = new TimeSpan(21, 08, 0) },
            new TrainRouteStop { TrainId = train18.Id, StopId = stops[4].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(23, 24, 0) }
        );

        // Train 19: Aswan - Cairo VIP Express (Train 981)
        var train19 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000019"),
            TrainNumber = "981",
            NameAr = "قطار أسوان - القاهرة VIP السريع", NameEn = "Aswan - Cairo VIP Express",
            DescriptionAr = "خدمة ممتازة نهارية VIP من أسوان إلى القاهرة", DescriptionEn = "Daytime premium VIP service from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train19);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(5, 30, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[21].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(6, 2, 0), ScheduledDeparture = new TimeSpan(6, 5, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[19].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(6, 22, 0), ScheduledDeparture = new TimeSpan(6, 25, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[18].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(6, 47, 0), ScheduledDeparture = new TimeSpan(6, 50, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[17].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(7, 40, 0), ScheduledDeparture = new TimeSpan(7, 45, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[20].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(8, 37, 0), ScheduledDeparture = new TimeSpan(8, 40, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[7].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(9, 30, 0), ScheduledDeparture = new TimeSpan(9, 40, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[6].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(10, 50, 0), ScheduledDeparture = new TimeSpan(10, 55, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[16].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(11, 35, 0), ScheduledDeparture = new TimeSpan(11, 40, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[15].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(12, 32, 0), ScheduledDeparture = new TimeSpan(12, 35, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[5].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(13, 05, 0), ScheduledDeparture = new TimeSpan(13, 10, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[4].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(14, 50, 0), ScheduledDeparture = new TimeSpan(14, 55, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[3].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(16, 50, 0), ScheduledDeparture = new TimeSpan(16, 55, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[2].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(18, 22, 0), ScheduledDeparture = new TimeSpan(18, 25, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[1].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(19, 15, 0), ScheduledDeparture = new TimeSpan(19, 20, 0) },
            new TrainRouteStop { TrainId = train19.Id, StopId = stops[0].Id, StopOrder = 16, ScheduledArrival = new TimeSpan(19, 35, 0) }
        );

        // Train 20: Aswan - Cairo AC Spanish Express (Train 997)
        var train20 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000020"),
            TrainNumber = "997",
            NameAr = "قطار أسوان - القاهرة الإسباني المكيف السريع", NameEn = "Aswan - Cairo AC Spanish Express",
            DescriptionAr = "خدمة سريعة إسبانية مريحة ومكيفة من أسوان إلى القاهرة", DescriptionEn = "Comfortable air-conditioned Spanish express service from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train20);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(20, 45, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(21, 32, 0), ScheduledDeparture = new TimeSpan(21, 35, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(22, 40, 0), ScheduledDeparture = new TimeSpan(22, 45, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[20].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(23, 37, 0), ScheduledDeparture = new TimeSpan(23, 40, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[7].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(0, 30, 0), ScheduledDeparture = new TimeSpan(0, 40, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[6].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(1, 45, 0), ScheduledDeparture = new TimeSpan(1, 50, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[16].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(2, 35, 0), ScheduledDeparture = new TimeSpan(2, 40, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[15].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(3, 32, 0), ScheduledDeparture = new TimeSpan(3, 35, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[5].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 10, 0), ScheduledDeparture = new TimeSpan(4, 15, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[4].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(5, 55, 0), ScheduledDeparture = new TimeSpan(6, 0, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[3].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(7, 55, 0), ScheduledDeparture = new TimeSpan(8, 0, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[2].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(9, 12, 0), ScheduledDeparture = new TimeSpan(9, 15, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[1].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(9, 35, 0), ScheduledDeparture = new TimeSpan(9, 40, 0) },
            new TrainRouteStop { TrainId = train20.Id, StopId = stops[0].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(9, 55, 0) }
        );

        // Train 21: Aswan - Alexandria VIP Special Express (Train 2009)
        var train21 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000021"),
            TrainNumber = "2009",
            NameAr = "قطار أسوان - الإسكندرية VIP الخاص السريع", NameEn = "Aswan - Alexandria VIP Special Express",
            DescriptionAr = "خدمة مباشرة ممتازة VIP من أسوان إلى الإسكندرية عبر القاهرة", DescriptionEn = "Direct premium VIP service from Aswan to Alexandria via Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train21);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(18, 10, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(18, 42, 0), ScheduledDeparture = new TimeSpan(18, 45, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(19, 37, 0), ScheduledDeparture = new TimeSpan(19, 40, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(21, 0, 0), ScheduledDeparture = new TimeSpan(21, 10, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[6].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(22, 5, 0), ScheduledDeparture = new TimeSpan(22, 10, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(23, 50, 0), ScheduledDeparture = new TimeSpan(23, 55, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[4].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(1, 20, 0), ScheduledDeparture = new TimeSpan(1, 25, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(3, 10, 0), ScheduledDeparture = new TimeSpan(3, 13, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[2].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(4, 40, 0), ScheduledDeparture = new TimeSpan(4, 42, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[1].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(6, 15, 0), ScheduledDeparture = new TimeSpan(6, 20, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[0].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(6, 40, 0), ScheduledDeparture = new TimeSpan(6, 50, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[10].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(7, 45, 0), ScheduledDeparture = new TimeSpan(7, 48, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[11].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(8, 30, 0), ScheduledDeparture = new TimeSpan(8, 32, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[12].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(9, 10, 0), ScheduledDeparture = new TimeSpan(9, 13, 0) },
            new TrainRouteStop { TrainId = train21.Id, StopId = stops[9].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(9, 30, 0) }
        );

        // Train 22: Aswan - Alexandria AC Spanish Express (Train 1903)
        var train22 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000022"),
            TrainNumber = "1903",
            NameAr = "قطار أسوان - الإسكندرية الإسباني المكيف السريع", NameEn = "Aswan - Alexandria AC Spanish Express",
            DescriptionAr = "خدمة سريعة إسبانية مريحة ومكيفة من أسوان إلى الإسكندرية عبر القاهرة", DescriptionEn = "Comfortable air-conditioned Spanish express service from Aswan to Alexandria via Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train22);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(17, 0, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(17, 35, 0), ScheduledDeparture = new TimeSpan(17, 38, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(18, 30, 0), ScheduledDeparture = new TimeSpan(18, 33, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(20, 0, 0), ScheduledDeparture = new TimeSpan(20, 10, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[6].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(21, 10, 0), ScheduledDeparture = new TimeSpan(21, 15, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(22, 0, 0), ScheduledDeparture = new TimeSpan(22, 5, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[15].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(22, 55, 0), ScheduledDeparture = new TimeSpan(22, 58, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[5].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(23, 35, 0), ScheduledDeparture = new TimeSpan(23, 40, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[14].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(0, 8, 0), ScheduledDeparture = new TimeSpan(0, 10, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[13].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(0, 30, 0), ScheduledDeparture = new TimeSpan(0, 32, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[4].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(1, 10, 0), ScheduledDeparture = new TimeSpan(1, 15, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[3].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(3, 5, 0), ScheduledDeparture = new TimeSpan(3, 8, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[2].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(4, 25, 0), ScheduledDeparture = new TimeSpan(4, 27, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[1].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(5, 30, 0), ScheduledDeparture = new TimeSpan(5, 35, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[0].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(5, 50, 0), ScheduledDeparture = new TimeSpan(6, 5, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[10].Id, StopOrder = 16, ScheduledArrival = new TimeSpan(7, 15, 0), ScheduledDeparture = new TimeSpan(7, 18, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[11].Id, StopOrder = 17, ScheduledArrival = new TimeSpan(8, 5, 0), ScheduledDeparture = new TimeSpan(8, 8, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[12].Id, StopOrder = 18, ScheduledArrival = new TimeSpan(8, 45, 0), ScheduledDeparture = new TimeSpan(8, 48, 0) },
            new TrainRouteStop { TrainId = train22.Id, StopId = stops[9].Id, StopOrder = 19, ScheduledArrival = new TimeSpan(9, 0, 0) }
        );

        // Train 23: Aswan - Alexandria AC Russian Express (Train 3007)
        var train23 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000023"),
            TrainNumber = "3007",
            NameAr = "قطار أسوان - الإسكندرية الروسي المكيف السريع", NameEn = "Aswan - Alexandria AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية حديثة ومكيفة من أسوان إلى الإسكندرية عبر القاهرة", DescriptionEn = "Modern air-conditioned Russian passenger service from Aswan to Alexandria via Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train23);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(10, 0, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(10, 35, 0), ScheduledDeparture = new TimeSpan(10, 38, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(11, 30, 0), ScheduledDeparture = new TimeSpan(11, 33, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(13, 0, 0), ScheduledDeparture = new TimeSpan(13, 10, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[16].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(14, 45, 0), ScheduledDeparture = new TimeSpan(14, 50, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[5].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(16, 0, 0), ScheduledDeparture = new TimeSpan(16, 5, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[4].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(17, 35, 0), ScheduledDeparture = new TimeSpan(17, 40, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[3].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(19, 40, 0), ScheduledDeparture = new TimeSpan(19, 45, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[2].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(21, 10, 0), ScheduledDeparture = new TimeSpan(21, 13, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[1].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(22, 15, 0), ScheduledDeparture = new TimeSpan(22, 20, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[0].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(22, 30, 0), ScheduledDeparture = new TimeSpan(22, 45, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[10].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(23, 45, 0), ScheduledDeparture = new TimeSpan(23, 48, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[11].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(0, 30, 0), ScheduledDeparture = new TimeSpan(0, 33, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[12].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(1, 15, 0), ScheduledDeparture = new TimeSpan(1, 18, 0) },
            new TrainRouteStop { TrainId = train23.Id, StopId = stops[9].Id, StopOrder = 15, ScheduledArrival = new TimeSpan(1, 30, 0) }
        );

        // Train 24: Aswan - Cairo VIP Express (Train 2011)
        var train24 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000024"),
            TrainNumber = "2011",
            NameAr = "قطار أسوان - القاهرة VIP السريع", NameEn = "Aswan - Cairo VIP Express",
            DescriptionAr = "خدمة سريعة ممتازة ومكيفة VIP من أسوان إلى القاهرة", DescriptionEn = "Premium air-conditioned VIP express from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train24);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(5, 15, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[21].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(5, 42, 0), ScheduledDeparture = new TimeSpan(5, 45, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[19].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(6, 0, 0), ScheduledDeparture = new TimeSpan(6, 3, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[18].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(6, 22, 0), ScheduledDeparture = new TimeSpan(6, 25, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[17].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(7, 5, 0), ScheduledDeparture = new TimeSpan(7, 10, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[20].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(8, 0, 0), ScheduledDeparture = new TimeSpan(8, 3, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[7].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(8, 50, 0), ScheduledDeparture = new TimeSpan(9, 0, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[6].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(10, 0, 0), ScheduledDeparture = new TimeSpan(10, 5, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[16].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(10, 42, 0), ScheduledDeparture = new TimeSpan(10, 45, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[5].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(11, 50, 0), ScheduledDeparture = new TimeSpan(11, 55, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[4].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(13, 25, 0), ScheduledDeparture = new TimeSpan(13, 30, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[3].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(15, 15, 0), ScheduledDeparture = new TimeSpan(15, 20, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[1].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(17, 40, 0), ScheduledDeparture = new TimeSpan(17, 45, 0) },
            new TrainRouteStop { TrainId = train24.Id, StopId = stops[0].Id, StopOrder = 14, ScheduledArrival = new TimeSpan(18, 0, 0) }
        );

        // Train 25: Aswan - Cairo VIP Express (Train 983)
        var train25 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000025"),
            TrainNumber = "983",
            NameAr = "قطار أسوان - القاهرة VIP السريع", NameEn = "Aswan - Cairo VIP Express",
            DescriptionAr = "خدمة ممتازة VIP من أسوان إلى القاهرة", DescriptionEn = "Premium VIP service from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train25);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(7, 30, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(8, 15, 0), ScheduledDeparture = new TimeSpan(8, 18, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(9, 30, 0), ScheduledDeparture = new TimeSpan(9, 35, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(11, 20, 0), ScheduledDeparture = new TimeSpan(11, 30, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[6].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(12, 35, 0), ScheduledDeparture = new TimeSpan(12, 40, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(13, 20, 0), ScheduledDeparture = new TimeSpan(13, 25, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[15].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(14, 15, 0), ScheduledDeparture = new TimeSpan(14, 18, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[5].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(14, 50, 0), ScheduledDeparture = new TimeSpan(14, 55, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[4].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(16, 30, 0), ScheduledDeparture = new TimeSpan(16, 35, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[3].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(18, 30, 0), ScheduledDeparture = new TimeSpan(18, 35, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[2].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(20, 0, 0), ScheduledDeparture = new TimeSpan(20, 3, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[1].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(21, 20, 0), ScheduledDeparture = new TimeSpan(21, 25, 0) },
            new TrainRouteStop { TrainId = train25.Id, StopId = stops[0].Id, StopOrder = 13, ScheduledArrival = new TimeSpan(21, 40, 0) }
        );

        // Train 26: Aswan - Cairo VIP Express (Train 2007)
        var train26 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000026"),
            TrainNumber = "2007",
            NameAr = "قطار أسوان - القاهرة VIP السريع", NameEn = "Aswan - Cairo VIP Express",
            DescriptionAr = "خدمة ليلية ممتازة VIP من أسوان إلى القاهرة", DescriptionEn = "Night premium VIP service from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train26);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(15, 15, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(15, 55, 0), ScheduledDeparture = new TimeSpan(15, 58, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(17, 0, 0), ScheduledDeparture = new TimeSpan(17, 5, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(18, 35, 0), ScheduledDeparture = new TimeSpan(18, 45, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[6].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(19, 45, 0), ScheduledDeparture = new TimeSpan(19, 50, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(20, 30, 0), ScheduledDeparture = new TimeSpan(20, 33, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[5].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(21, 35, 0), ScheduledDeparture = new TimeSpan(21, 40, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[4].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(23, 10, 0), ScheduledDeparture = new TimeSpan(23, 15, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[3].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(1, 10, 0), ScheduledDeparture = new TimeSpan(1, 15, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[1].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(4, 15, 0), ScheduledDeparture = new TimeSpan(4, 20, 0) },
            new TrainRouteStop { TrainId = train26.Id, StopId = stops[0].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(4, 35, 0) }
        );

        // Train 27: Aswan - Cairo AC Spanish Express (Train 989)
        var train27 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000027"),
            TrainNumber = "989",
            NameAr = "قطار أسوان - القاهرة الإسباني المكيف السريع", NameEn = "Aswan - Cairo AC Spanish Express",
            DescriptionAr = "خدمة سريعة إسبانية مريحة ومكيفة من أسوان إلى القاهرة", DescriptionEn = "Comfortable air-conditioned Spanish express service from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train27);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(23, 0, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(23, 35, 0), ScheduledDeparture = new TimeSpan(23, 38, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(0, 30, 0), ScheduledDeparture = new TimeSpan(0, 35, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(2, 0, 0), ScheduledDeparture = new TimeSpan(2, 10, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[6].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(3, 15, 0), ScheduledDeparture = new TimeSpan(3, 20, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(4, 5, 0), ScheduledDeparture = new TimeSpan(4, 8, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[5].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(5, 10, 0), ScheduledDeparture = new TimeSpan(5, 15, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[4].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(7, 5, 0), ScheduledDeparture = new TimeSpan(7, 10, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[3].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(9, 5, 0), ScheduledDeparture = new TimeSpan(9, 10, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[2].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(10, 20, 0), ScheduledDeparture = new TimeSpan(10, 23, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[1].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(11, 15, 0), ScheduledDeparture = new TimeSpan(11, 20, 0) },
            new TrainRouteStop { TrainId = train27.Id, StopId = stops[0].Id, StopOrder = 12, ScheduledArrival = new TimeSpan(11, 34, 0) }
        );

        // Train 28: Aswan - Cairo AC Russian Express (Train 2013)
        var train28 = new Train
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000028"),
            TrainNumber = "2013",
            NameAr = "قطار أسوان - القاهرة الروسي المكيف السريع", NameEn = "Aswan - Cairo AC Russian Express",
            DescriptionAr = "خدمة ركاب روسية مريحة ومكيفة من أسوان إلى القاهرة", DescriptionEn = "Comfortable air-conditioned Russian passenger service from Aswan to Cairo",
            IsActive = true,
            CreatedById = adminUser.Id
        };
        context.Trains.Add(train28);

        context.TrainRouteStops.AddRange(
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[8].Id, StopOrder = 1, ScheduledDeparture = new TimeSpan(14, 5, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[19].Id, StopOrder = 2, ScheduledArrival = new TimeSpan(14, 40, 0), ScheduledDeparture = new TimeSpan(14, 43, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[17].Id, StopOrder = 3, ScheduledArrival = new TimeSpan(15, 35, 0), ScheduledDeparture = new TimeSpan(15, 40, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[7].Id, StopOrder = 4, ScheduledArrival = new TimeSpan(17, 10, 0), ScheduledDeparture = new TimeSpan(17, 20, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[6].Id, StopOrder = 5, ScheduledArrival = new TimeSpan(18, 25, 0), ScheduledDeparture = new TimeSpan(18, 30, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[16].Id, StopOrder = 6, ScheduledArrival = new TimeSpan(19, 15, 0), ScheduledDeparture = new TimeSpan(19, 18, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[5].Id, StopOrder = 7, ScheduledArrival = new TimeSpan(20, 30, 0), ScheduledDeparture = new TimeSpan(20, 35, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[4].Id, StopOrder = 8, ScheduledArrival = new TimeSpan(22, 10, 0), ScheduledDeparture = new TimeSpan(22, 15, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[3].Id, StopOrder = 9, ScheduledArrival = new TimeSpan(0, 10, 0), ScheduledDeparture = new TimeSpan(0, 15, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[1].Id, StopOrder = 10, ScheduledArrival = new TimeSpan(2, 50, 0), ScheduledDeparture = new TimeSpan(2, 55, 0) },
            new TrainRouteStop { TrainId = train28.Id, StopId = stops[0].Id, StopOrder = 11, ScheduledArrival = new TimeSpan(3, 10, 0) }
        );

        // Create today's trips for all trains
        var today = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday();
        var tomorrow = today.AddDays(1);

        var trip1 = new Trip { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), TrainId = train1.Id, TripDate = today, StatusId = TripStatuses.InTransit };
        var trip2 = new Trip { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), TrainId = train2.Id, TripDate = today, StatusId = TripStatuses.Scheduled };
        var trip3 = new Trip { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), TrainId = train3.Id, TripDate = today, StatusId = TripStatuses.Scheduled };
        var trip4 = new Trip { Id = Guid.Parse("30000000-0000-0000-0000-000000000004"), TrainId = train2.Id, TripDate = tomorrow, StatusId = TripStatuses.Scheduled };

        context.Trips.AddRange(trip1, trip2, trip3, trip4);

        // Test user follows trip1
        context.TripFollowers.Add(new TripFollower
        {
            UserId = testUser.Id,
            TripId = trip1.Id,
            PersonalStatus = PersonalTripStatus.Started
        });

        // Add some live updates to trip1
        context.TripLiveUpdates.AddRange(
            new TripLiveUpdate
            {
                TripId = trip1.Id,
                AuthorId = testUser.Id,
                Content = "Train just departed from Cairo Central, running on time!",
                StatusTag = "OnTime",
                Latitude = 30.0626,
                Longitude = 31.2467
            },
            new TripLiveUpdate
            {
                TripId = trip1.Id,
                AuthorId = testUser.Id,
                Content = "Arrived at Giza station, 3 minutes delay",
                StatusTag = "Delayed",
                Latitude = 30.0091,
                Longitude = 31.2089
            }
        );

        // Sample Lost & Found post
        var samplePost = new LostFoundPost
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            AuthorId = testUser.Id,
            Title = "Lost Black Leather Wallet",
            Description = "I lost my black leather wallet on train 980 yesterday. It contains my ID and credit cards. Please contact me if found.",
            Type = LostFoundType.Lost,
            TrainNumber = "980",
            ContactInfo = "test@whereisthetrain.com",
            Status = LostFoundStatus.Published
        };
        context.LostFoundPosts.Add(samplePost);

        // Sample comment
        context.LostFoundComments.Add(new LostFoundComment
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
            PostId = samplePost.Id,
            AuthorId = adminUser.Id,
            Content = "Please check with the station master office at Cairo Central. They often collect lost items.",
            IsHidden = false
        });

        // Seed default system settings
        context.SystemSettings.Add(new SystemSetting
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000009999"),
            LostFoundPostAutoPublish = true,
            LostFoundCommentAutoPublish = true,
            TripLiveUpdateAutoPublish = true
        });

        // Seed default lookups
        context.StatusTagLookups.AddRange(
            new StatusTagLookup { Id = Guid.NewGuid(), Code = "OnTime", NameAr = "في الموعد", NameEn = "On Time" },
            new StatusTagLookup { Id = Guid.NewGuid(), Code = "Delayed", NameAr = "متأخر", NameEn = "Delayed" },
            new StatusTagLookup { Id = Guid.NewGuid(), Code = "Crowded", NameAr = "مزدحم", NameEn = "Crowded" },
            new StatusTagLookup { Id = Guid.NewGuid(), Code = "Empty", NameAr = "فارغ", NameEn = "Empty" },
            new StatusTagLookup { Id = Guid.NewGuid(), Code = "Cancelled", NameAr = "ملغي", NameEn = "Cancelled" },
            new StatusTagLookup { Id = Guid.NewGuid(), Code = "AtStation", NameAr = "في المحطة", NameEn = "At Station" }
        );

        context.TripStatusLookups.AddRange(
            new TripStatusLookup { Id = TripStatuses.Scheduled, Code = "Scheduled", NameAr = "مجدول", NameEn = "Scheduled", Color = "#71717a" },
            new TripStatusLookup { Id = TripStatuses.Departed, Code = "Departed", NameAr = "غادر", NameEn = "Departed", Color = "#3b82f6" },
            new TripStatusLookup { Id = TripStatuses.InTransit, Code = "InTransit", NameAr = "في الطريق", NameEn = "In Transit", Color = "#6366f1" },
            new TripStatusLookup { Id = TripStatuses.Arrived, Code = "Arrived", NameAr = "وصل", NameEn = "Arrived", Color = "#10b981" },
            new TripStatusLookup { Id = TripStatuses.Cancelled, Code = "Cancelled", NameAr = "ملغي", NameEn = "Cancelled", Color = "#ef4444" },
            new TripStatusLookup { Id = TripStatuses.Delayed, Code = "Delayed", NameAr = "متأخر", NameEn = "Delayed", Color = "#f59e0b" }
        );

        context.CrowdLevelLookups.AddRange(
            new CrowdLevelLookup { Id = Guid.NewGuid(), Code = "EmptyChairs", NameAr = "كراسي شاغرة", NameEn = "Empty Chairs" },
            new CrowdLevelLookup { Id = Guid.NewGuid(), Code = "FullChairs", NameAr = "كراسي ممتلئة", NameEn = "Full Chairs" },
            new CrowdLevelLookup { Id = Guid.NewGuid(), Code = "AisleCrowded", NameAr = "الممرات مزدحمة", NameEn = "Aisle Crowded" }
        );

        if (!context.DashboardGalleryItems.Any())
        {
            context.DashboardGalleryItems.AddRange(
                new DashboardGalleryItem 
                { 
                    Id = Guid.NewGuid(), 
                    ImagePath = "/hero-banner.png", 
                    CaptionAr = "مرحباً بك في نظام تتبع القطارات الذكي", 
                    CaptionEn = "Welcome to the Smart Train Tracking System", 
                    IsVisible = true 
                },
                new DashboardGalleryItem 
                { 
                    Id = Guid.NewGuid(), 
                    ImagePath = "/scenic-route.png", 
                    CaptionAr = "استكشف مناظر الرحلات الريفية الخلابة عبر خطوطنا", 
                    CaptionEn = "Explore breathtaking scenic routes along our valley lines", 
                    IsVisible = true 
                }
            );
        }

        await ExtractAndPopulateTrainTypesAsync(context);

        await context.SaveChangesAsync();

    }

    public static async Task ExtractAndPopulateTrainTypesAsync(ApplicationDbContext context)
    {
        var trains = await context.Trains.ToListAsync();
        foreach (var train in trains)
        {
            string nameEn = train.NameEn;
            string nameAr = train.NameAr;

            string typeEn = "";
            string typeAr = "";

            if (nameEn.Contains("VIP Special Express") || nameAr.Contains("VIP الخاص السريع"))
            {
                typeEn = "VIP Special Express";
                typeAr = "VIP الخاص السريع";
            }
            else if (nameEn.Contains("VIP Express") || nameAr.Contains("VIP السريع"))
            {
                typeEn = "VIP Express";
                typeAr = "VIP السريع";
            }
            else if (nameEn.Contains("AC Spanish Express") || nameAr.Contains("الإسباني المكيف السريع"))
            {
                typeEn = "AC Spanish Express";
                typeAr = "الإسباني المكيف السريع";
            }
            else if (nameEn.Contains("AC Russian Express") || nameAr.Contains("الروسي المكيف السريع"))
            {
                typeEn = "AC Russian Express";
                typeAr = "الروسي المكيف السريع";
            }
            else if (nameEn.Contains("Express") || nameAr.Contains("السريع"))
            {
                typeEn = "Express";
                typeAr = "السريع";
            }
            else
            {
                int dashIndex = nameEn.LastIndexOf(" - ");
                if (dashIndex >= 0 && dashIndex + 3 < nameEn.Length)
                {
                    string suffixEn = nameEn.Substring(dashIndex + 3).Trim();
                    string[] parts = suffixEn.Split(' ', 2);
                    if (parts.Length > 1)
                    {
                        typeEn = parts[1];
                    }
                    else
                    {
                        typeEn = suffixEn;
                    }
                }
                else
                {
                    typeEn = "Express";
                }

                int dashIndexAr = nameAr.LastIndexOf(" - ");
                if (dashIndexAr >= 0 && dashIndexAr + 3 < nameAr.Length)
                {
                    string suffixAr = nameAr.Substring(dashIndexAr + 3).Trim();
                    string[] partsAr = suffixAr.Split(' ', 2);
                    if (partsAr.Length > 1)
                    {
                        typeAr = partsAr[1];
                    }
                    else
                    {
                        typeAr = suffixAr;
                    }
                }
                else
                {
                    typeAr = "السريع";
                }
            }

            var trainType = await context.TrainTypes.FirstOrDefaultAsync(t => t.NameEn == typeEn);
            if (trainType == null)
            {
                trainType = new TrainType
                {
                    Id = Guid.NewGuid(),
                    NameEn = typeEn,
                    NameAr = typeAr,
                    MarkerPngUrl = "/markers/default-train.png"
                };
                context.TrainTypes.Add(trainType);
                await context.SaveChangesAsync();
            }

            train.TrainTypeId = trainType.Id;
        }

        await context.SaveChangesAsync();
    }
}

