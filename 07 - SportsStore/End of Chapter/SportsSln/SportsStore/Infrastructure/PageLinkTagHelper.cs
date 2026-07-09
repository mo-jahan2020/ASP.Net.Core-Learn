using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SportsStore.Models.ViewModels;

namespace SportsStore.Infrastructure {

    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper {
        private IUrlHelperFactory urlHelperFactory;

        public PageLinkTagHelper(IUrlHelperFactory helperFactory) {
            urlHelperFactory = helperFactory;
        }
        
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public PagingInfo? PageModel { get; set; }

        public string? PageAction { get; set; }

        public bool PageClassesEnabled { get; set; } = false;
        public string PageClass { get; set; } = String.Empty;
        public string PageClassNormal { get; set; } = String.Empty;
        public string PageClassSelected { get; set; } = String.Empty;

        public override void Process(TagHelperContext context,
                TagHelperOutput output) {
            if (ViewContext != null && PageModel != null) {
                IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
                TagBuilder result = new TagBuilder("div");
                // افزودن لینک صفحه اول
                TagBuilder firstPageTag = new TagBuilder("a");
                firstPageTag.Attributes["href"] = urlHelper.Action(PageAction, new { productPage = 1 });
                if (PageClassesEnabled)
                {
                    firstPageTag.AddCssClass(PageClass);
                    firstPageTag.AddCssClass(1 == PageModel.CurrentPage ? PageClassSelected   : PageClassNormal);
                    firstPageTag.AddCssClass(1 == PageModel.CurrentPage ? "disabled" : PageClassNormal);
                }
                firstPageTag.InnerHtml.Append("<<");
                result.InnerHtml.AppendHtml(firstPageTag);

                // افزودن لینک‌های صفحات
                for (int i = 1; i <= PageModel.TotalPages; i++) {
                    TagBuilder tag = new TagBuilder("a");
                    tag.Attributes["href"] = urlHelper.Action(PageAction,
                       new { productPage = i });
                    if (PageClassesEnabled) {
                        tag.AddCssClass(PageClass);
                        tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                        tag.AddCssClass(i == PageModel.CurrentPage ? "disabled" : PageClassNormal);
                    }
                    tag.InnerHtml.Append(i.ToString());
                    result.InnerHtml.AppendHtml(tag);
                }
                // افزودن لینک صفحه آخر
                TagBuilder lastPageTag = new TagBuilder("a");
                lastPageTag.Attributes["href"] = urlHelper.Action(PageAction, new { productPage = PageModel.TotalPages });
                if (PageClassesEnabled)
                {
                    lastPageTag.AddCssClass(PageClass);
                    lastPageTag.AddCssClass(PageModel.TotalPages == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                    lastPageTag.AddCssClass(PageModel.CurrentPage == PageModel.TotalPages ? "disabled" : PageClassNormal);
                }
                lastPageTag.InnerHtml.Append(">>");
                result.InnerHtml.AppendHtml(lastPageTag);

                //افزودن محتوای اصلی
                output.Content.AppendHtml(result.InnerHtml);
            }
        }
    }
}
