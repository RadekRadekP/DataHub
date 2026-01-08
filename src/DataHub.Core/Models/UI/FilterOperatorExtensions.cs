using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DataHub.Core.Models.UI
{
    public static class FilterOperatorExtensions
    {
        public static string GetDescription(this FilterOperator value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            DescriptionAttribute? attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static IEnumerable<FilterOperator> GetAllOperatorValues()
        {
            return Enum.GetValues(typeof(FilterOperator)).Cast<FilterOperator>();
        }

        public static string ToShortCodeString(this FilterOperator value)
        {
            return value switch
            {
                FilterOperator.Contains => "CONTAINS",
                FilterOperator.NotContains => "NOTCONTAINS",
                FilterOperator.StartsWith => "STARTSWITH",
                FilterOperator.EndsWith => "ENDSWITH",
                FilterOperator.Equals => "EQ",
                FilterOperator.NotEquals => "NEQ",
                FilterOperator.GreaterThan => "GT",
                FilterOperator.GreaterThanOrEqual => "GTE",
                FilterOperator.LessThan => "LT",
                FilterOperator.LessThanOrEqual => "LTE",
                FilterOperator.In => "IN",
                FilterOperator.NotIn => "NOTIN",
                FilterOperator.IsNull => "ISNULL",
                FilterOperator.IsNotNull => "ISNOTNULL",
                FilterOperator.IsTrue => "ISTRUE",
                FilterOperator.IsFalse => "ISFALSE",
                _ => value.ToString().ToUpper()
            };
        }
    }
}