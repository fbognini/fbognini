using FluentValidation;
using System;
using System.Linq;

namespace fbognini.Core.Validations
{
    public static class ExtensionsMethods
    {
        public static string ToStringWithNull<TProperty>(this TProperty c)
        {
            if (c == null)
                return "null";

            return c.ToString();
        }

        public static IRuleBuilderOptions<T, TProperty> In<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, params TProperty[] validOptions)
        {
            string formatted;
            if (validOptions == null || validOptions.Length == 0)
            {
                throw new ArgumentException("At least one valid option is expected", nameof(validOptions));
            }
            else if (validOptions.Length == 1)
            {
                formatted = validOptions[0].ToStringWithNull();
            }
            else
            {
                // format like: option1, option2 or option3
                formatted = $"{string.Join(", ", validOptions.Select(vo => vo.ToStringWithNull()).ToArray(), 0, validOptions.Length - 1)} or {validOptions.Last()}";
            }

            return ruleBuilder
                .Must(validOptions.Contains)
                .WithMessage($"{{PropertyName}} must be one of these values: {formatted}");
        }

        ///// <summary>
        ///// Predicate builder which makes the validated property available
        ///// </summary>
        ///// <param name="rule"></param>
        ///// <param name="predicate"></param>
        ///// <param name="applyConditionTo"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <typeparam name="TProperty"></typeparam>
        ///// <returns></returns>
        //public static IRuleBuilderOptions<T, TProperty> When<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Func<T, TProperty, bool> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        //{
        //    return rule.Configure(config =>
        //    {
        //        config.ApplyCondition(ctx => predicate((T)ctx.InstanceToValidate, (TProperty)ctx.PropertyValue), applyConditionTo);
        //    });
        //}
    }
}
