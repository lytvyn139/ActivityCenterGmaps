using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ActivityCenter.Validations
{
     public class StrongPasswordAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var reg = new Regex(@"^(?=.*?\d)(?=.*?[A-Z])(?=.*?[a-z])[A-Za-z\d,!@#$%^&*+=]{8,}$");
                if ((string)value == null)
                {
                    return new ValidationResult("Password must have at least 1 number, 1 letter, 1 Uppercase letter and 1 special character.");
                }
                else
                {
                    if (!reg.IsMatch((string)value))
                        return new ValidationResult("Password must have at least 1 number, 1 letter, 1 Uppercase letter and 1 special character.");
                    return ValidationResult.Success;
                }
            }
        }
}