using System.ComponentModel.DataAnnotations;

namespace Startup_Proj.CustomValidation
{
    public class YearValidation : ValidationAttribute
    {
        public string _error_message { get; set; }="Year must less then jun 01 {0}";
        public int _min_year { get; set; } = 2004;

        public YearValidation()
        {
        }

        public YearValidation(int min_year)
        {
            _min_year = min_year;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime date= (DateTime)value;
                int year = date.Year;
                if (year >= _min_year)
                {
                    return new ValidationResult(string.Format(ErrorMessage ?? _error_message, _min_year));
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            return null;
        }

    }
}
