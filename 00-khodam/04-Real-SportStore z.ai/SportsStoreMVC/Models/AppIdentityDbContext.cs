using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SportsStore.Models {

    /// <summary>
    /// DbContext مخصوص احراز هویت (Identity)
    /// از این DbContext برای مدیریت کاربران و نقش‌ها استفاده می‌شود.
    /// IdentityDbContext کلاس پایه است که جداول پیش‌فرض Identity را تعریف می‌کند.
    /// </summary>
    public class AppIdentityDbContext : IdentityDbContext<IdentityUser> {

        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options) { }
    }
}
