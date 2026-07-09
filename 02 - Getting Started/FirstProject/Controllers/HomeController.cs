using Microsoft.AspNetCore.Mvc;
using FirstProject.Models;

namespace FirstProject.Controllers {

    public class HomeController : Controller
    {

        public ViewResult Index()//تابع برای نمایش صفحه اصلی
        {
            return View();
        }
        [HttpGet]
        public ViewResult RsvpForm()//تابع برای نمایش فرم در متد گت یعنی فراخوانی مستقیم از بیرون
        {
            return View();
        }
        [HttpPost]
        public ViewResult RsvpForm(GuestResponse guestResponse)//فراخوانی با متد پست یعنی فراخوانی در صورت کلیک روی دکمه سابمیت
        {
            if(ModelState.IsValid)
            {
                Repository.AddResponse(guestResponse);//مقدار را به مخزن اضافه می‌کند
                return View("Thanks", guestResponse);//فرم را به نمای "Thanks" هدایت می‌کند
            }   
            else
            {
                return View();//اگر مدل معتبر نباشد، فرم را دوباره نمایش می‌دهد
            }   
           
        }
        public ViewResult ListResponses()
        {
            return View(Repository.Responses.Where(r => r.WillAttend == true));
        }
        
    }
}
