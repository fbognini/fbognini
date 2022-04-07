using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Validations
{
    public class CustomValidator<T> : AbstractValidator<T>
    {
        public override ValidationResult Validate(ValidationContext<T> context)
        {
            var validationResult = base.Validate(context);
            if (!validationResult.IsValid)
            {
                var failures = validationResult.Errors.ToList();
                if (failures.Count != 0)
                    throw new Exceptions.ValidationException(failures);
            }

            return validationResult;
        }
    }
}
