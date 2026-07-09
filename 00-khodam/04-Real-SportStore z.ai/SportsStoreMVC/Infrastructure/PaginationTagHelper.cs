using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportsStore.Models.ViewModels;

namespace SportsStore.Infrastructure {

    /// <summary>
    /// کلاس Extension Method برای ساخت لینک‌های صفحه‌بندی
    /// این متدها در View ها برای ساخت دکمه‌های صفحه‌بندی استفاده می‌شوند.
    /// </summary>
    public static class PaginationTagHelper {

        /// <summary>
        /// ساخت HTML لینک‌های صفحه‌بندی
        /// </summary>
        /// <param name="html">Helper</param>
        /// <param name="pagingInfo">اطلاعات صفحه‌بندی</param>
        /// <param name="pageAction">اکشن URL برای لینک‌ها</param>
        /// <param name="pageUrlValues">تابع تولید URL برای هر صفحه</param>
        public static HtmlString PageLinks(
            this IHtmlHelper html,
            PagingInfo pagingInfo,
            Func<int, string> pageUrl) {

            var result = new System.Text.StringBuilder();

            // دکمه Previous
            result.Append("<nav aria-label=\"Page navigation\">");
            result.Append("<ul class=\"pagination justify-content-center\">");

            // لینک صفحه قبلی
            if (pagingInfo.CurrentPage > 1) {
                result.AppendFormat(
                    "<li class=\"page-item\"><a class=\"page-link\" href=\"{0}\">قبلی</a></li>",
                    pageUrl(pagingInfo.CurrentPage - 1));
            } else {
                result.Append("<li class=\"page-item disabled\"><span class=\"page-link\">قبلی</span></li>");
            }

            // لینک هر صفحه
            for (int i = 1; i <= pagingInfo.TotalPages; i++) {
                if (i == pagingInfo.CurrentPage) {
                    // صفحه فعلی - غیرفعال و هایلایت شده
                    result.AppendFormat(
                        "<li class=\"page-item active\"><span class=\"page-link\">{0}</span></li>", i);
                } else {
                    result.AppendFormat(
                        "<li class=\"page-item\"><a class=\"page-link\" href=\"{0}\">{1}</a></li>",
                        pageUrl(i), i);
                }
            }

            // لینک صفحه بعدی
            if (pagingInfo.CurrentPage < pagingInfo.TotalPages) {
                result.AppendFormat(
                    "<li class=\"page-item\"><a class=\"page-link\" href=\"{0}\">بعدی</a></li>",
                    pageUrl(pagingInfo.CurrentPage + 1));
            } else {
                result.Append("<li class=\"page-item disabled\"><span class=\"page-link\">بعدی</span></li>");
            }

            result.Append("</ul>");
            result.Append("</nav>");
            return new HtmlString(result.ToString());
        }
    }
}
