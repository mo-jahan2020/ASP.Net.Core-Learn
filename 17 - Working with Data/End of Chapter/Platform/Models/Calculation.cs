namespace Platform.Models {

    // مدل داده‌ای که یک محاسبه (مجموع اعداد از 1 تا Count) را در دیتابیس نمایش می‌دهد
    public class Calculation {
        // کلید اصلی (Primary Key) رکورد در دیتابیس
        public long Id { get; set; }
        // مقدار ورودی محاسبه (تا چه عددی جمع زده شده است)
        public int Count { get; set; }
        // نتیجه محاسبه‌شده (مجموع اعداد)
        public long Result { get; set; }
    }
}
