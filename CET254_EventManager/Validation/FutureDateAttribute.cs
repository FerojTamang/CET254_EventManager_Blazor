using System.ComponentModel.DataAnnotations;

namespace CET254_EventManager.Validation
{
    // Custom validation attribute plugs into ModelState and blocks creating events in the past.
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
        {
            ErrorMessage = "Date must be today or in the future.";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is DateTime date)
            {
                return date.Date >= DateTime.Today;
            }

            return false;
        }
    }
}
