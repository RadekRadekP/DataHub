using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging; // Add for ILogger
using System.Linq;
using System.Linq.Expressions;
using DataHub.Core.Models.UI; // For FilterCriterion, FilterOperator
using System.Globalization; // For CultureInfo


namespace DataHub.Core.Services
{
    public static class ExpressionBuilder
    {
        public static Expression? BuildCombinedExpression<T>(
            IEnumerable<FilterCriterion> criteria, 
            ParameterExpression parameter)
        {
            if (criteria == null || !criteria.Any())
            {
                return null;
            }

            Expression? combinedExpression = null;

            foreach (var criterion in criteria.Where(c => !string.IsNullOrWhiteSpace(c.FieldName)))
            {
                try
                {
                    var propertyExpression = Expression.PropertyOrField(parameter, criterion.FieldName);
                    var filterExpression = BuildExpression(propertyExpression, criterion, parameter);

                    if (filterExpression != null)
                    {
                        if (combinedExpression == null)
                        {
                            combinedExpression = filterExpression;
                        }
                        else
                        {
                            // Use the logical operator from the criterion itself.
                            // The first criterion will have a null LogicalOperator, so this works.
                            if ("OR".Equals(criterion.LogicalOperator, StringComparison.OrdinalIgnoreCase))
                            {
                                combinedExpression = Expression.OrElse(combinedExpression, filterExpression);
                            }
                            else // Default to AND if operator is null, empty, or "AND"
                            {
                                combinedExpression = Expression.AndAlso(combinedExpression, filterExpression);
                            }
                        }
                    }
                }
                catch (ArgumentException ex) // Catches invalid FieldName
                {
                    // Log or handle the error if a field name is not found on the type T.
                    // For now, we write to console and skip the invalid criterion.
                    Console.WriteLine($"Warning: ExpressionBuilder: Could not find property '{criterion.FieldName}' on type '{typeof(T).Name}'. Skipping criterion. Error: {ex.Message}");
                }
            }

            return combinedExpression;
        }

        public static Expression? BuildExpression(Expression propertyExpression, FilterCriterion criterion, ParameterExpression parameter)
        {
            // Use the actual type of the property, including Nullable<T>
            Type actualPropertyType = propertyExpression.Type; // Opravené: PropertyExpression nie je typ, použite priamo propertyExpression.Type
            // For conversion and comparison, use the underlying type if it's nullable, otherwise the actual type.
            Type underlyingPropertyType = Nullable.GetUnderlyingType(actualPropertyType) ?? actualPropertyType;

            Console.WriteLine($"ExpressionBuilder IN/NotIn: Property Type: {underlyingPropertyType.Name}");

            if (criterion.Operator == FilterOperator.In || criterion.Operator == FilterOperator.NotIn)
            {
                if (criterion.Values == null || !criterion.Values.Any()) return null;

                Console.WriteLine($"ExpressionBuilder IN/NotIn: criterion.FieldName='{criterion.FieldName}', Values from QueryParserService: '{string.Join(" | ", criterion.Values.Select(v => v?.ToString() ?? "null"))}'");

                // Build a series of OR conditions for the IN operator
                Expression? inExpression = null;
                foreach (var value in criterion.Values) // Use criterion.Values directly
                {
                    var constantValue = Expression.Constant(Convert.ChangeType(value, underlyingPropertyType), underlyingPropertyType);
                    var equalsExpression = Expression.Equal(propertyExpression, constantValue);
                    inExpression = inExpression == null ? equalsExpression : Expression.OrElse(inExpression, equalsExpression);
                }

                if (inExpression == null) return null;

                Console.WriteLine($"ExpressionBuilder IN/NotIn: Generated IN Expression (OR conditions): {inExpression.ToString()}");

                // If the operator is NotIn, negate the entire OR expression
                return criterion.Operator == FilterOperator.NotIn ? Expression.Not(inExpression) : inExpression;
            }
            else if (criterion.Operator == FilterOperator.IsNull)
            {
                return Expression.Equal(propertyExpression, Expression.Constant(null, actualPropertyType));
            }
            else if (criterion.Operator == FilterOperator.IsNotNull)
            {
                return Expression.NotEqual(propertyExpression, Expression.Constant(null, actualPropertyType));
            }
            else if (criterion.Operator == FilterOperator.IsTrue || criterion.Operator == FilterOperator.IsFalse)
            {
                // This logic handles boolean checks and was previously missing.
                // It ensures that a valid expression is always returned, resolving the lambda conversion error.
                if (underlyingPropertyType == typeof(bool))
                {
                    bool valueToCompare = criterion.Operator == FilterOperator.IsTrue;
                    var constant = Expression.Constant(valueToCompare, actualPropertyType);
                    return Expression.Equal(propertyExpression, constant);
                }
                else
                {
                    Console.WriteLine($"Warning: ExpressionBuilder: Operator '{criterion.Operator}' is only applicable to boolean properties. Field '{criterion.FieldName}' is of type '{underlyingPropertyType.Name}'. Skipping criterion.");
                    return null;
                }
            }
            else if (criterion.Value != null || 
                     (underlyingPropertyType == typeof(string) && (criterion.Operator == FilterOperator.Contains || criterion.Operator == FilterOperator.NotContains))) // Povolit prázdný řetězec pro string contains
            {
                object? typedValue;
                string valueToConvert = criterion.Value?.ToString() ?? string.Empty;

                try { typedValue = ConvertType(valueToConvert, underlyingPropertyType); }
                catch (Exception ex) { Console.WriteLine($"Warning: Could not convert value '{valueToConvert}' for field '{criterion.FieldName}' to type '{underlyingPropertyType.Name}'. Skipping. Error: {ex.Message}"); return null; }

                // If conversion results in null, but the original value was not an empty string (or it's not a string type),
                // then it's likely a parse error for non-string types, so we should not proceed with comparison operators.
                // For string types, an empty string is a valid value for 'Contains'.
                if (typedValue == null && underlyingPropertyType != typeof(string) && !string.IsNullOrEmpty(valueToConvert))
                {
                     Console.WriteLine($"Warning: Conversion of '{valueToConvert}' to type '{underlyingPropertyType.Name}' for field '{criterion.FieldName}' resulted in null. Skipping criterion.");
                     return null;
                }

                // Prepare target expression and value, especially for DateTime.Date comparisons
                Expression targetPropertyExpression = propertyExpression;
                object? targetTypedValue = typedValue;
                Type constantType = actualPropertyType; // Use the actual property type for the constant, including Nullable<T>

                var constantExpression = Expression.Constant(targetTypedValue, constantType);

                switch (criterion.Operator)
                {
                    case FilterOperator.Equals:
                        if (underlyingPropertyType == typeof(string))
                        {
                            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                            var lowerProperty = Expression.Call(propertyExpression, toLowerMethod!); // Convert property to lowercase
                            var lowerConstant = Expression.Constant((typedValue as string)?.ToLower(), typeof(string)); // Convert constant to lowercase
                            return Expression.Equal(lowerProperty, lowerConstant);
                        }
                        return Expression.Equal(targetPropertyExpression, constantExpression);
                    case FilterOperator.NotEquals:
                        if (underlyingPropertyType == typeof(string))
                        {
                            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                            var lowerProperty = Expression.Call(propertyExpression, toLowerMethod!); // Convert property to lowercase
                            var lowerConstant = Expression.Constant((typedValue as string)?.ToLower(), typeof(string)); // Convert constant to lowercase
                            return Expression.NotEqual(lowerProperty, lowerConstant);
                        }
                        return Expression.NotEqual(targetPropertyExpression, constantExpression);
                    case FilterOperator.LessThan:
                        if (typedValue == null) return null; // Cannot compare null with <
                        return Expression.LessThan(targetPropertyExpression, constantExpression);
                    case FilterOperator.LessThanOrEqual:
                        if (typedValue == null) return null;
                        return Expression.LessThanOrEqual(targetPropertyExpression, constantExpression);
                    case FilterOperator.GreaterThan:
                        if (typedValue == null) return null;
                        return Expression.GreaterThan(targetPropertyExpression, constantExpression);
                    case FilterOperator.GreaterThanOrEqual:
                        if (typedValue == null) return null;
                        return Expression.GreaterThanOrEqual(targetPropertyExpression, constantExpression);
                    case FilterOperator.Contains:
                        if (underlyingPropertyType == typeof(string))
                        {
                            string stringValueForContains = (typedValue as string) ?? string.Empty;
                            var stringConstant = Expression.Constant(stringValueForContains.ToLower(), typeof(string)); // Convert search value to lower
                            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                            var containsMethodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                            // Handle null property values and perform case-insensitive contains
                            var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
                            var containsCall = Expression.Call(Expression.Call(propertyExpression, toLowerMethod!), containsMethodInfo!, stringConstant);
                            return Expression.AndAlso(nullCheck, containsCall);
                        }
                        // If 'Contains' is used on a non-string type, treat it as 'Equals'.
                        Console.WriteLine($"Info: ExpressionBuilder: Operator 'Contains' used on non-string field '{criterion.FieldName}'. Defaulting to 'Equals'.");
                        return Expression.Equal(targetPropertyExpression, constantExpression);
                    case FilterOperator.StartsWith:
                        if (underlyingPropertyType == typeof(string))
                        {
                            string stringValueForStartsWith = (typedValue as string) ?? string.Empty;
                            var stringConstant = Expression.Constant(stringValueForStartsWith.ToLower(), typeof(string)); // Convert search value to lower
                            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                            var startsWithMethodInfo = typeof(string).GetMethod("StartsWith", new[] { typeof(string) }); // Use overload without StringComparison

                            // Call .ToLower().StartsWith(value.ToLower())
                            return Expression.Call(Expression.Call(propertyExpression, toLowerMethod!), startsWithMethodInfo!, stringConstant);
                        }
                        return null;
                    case FilterOperator.EndsWith:
                        if (underlyingPropertyType == typeof(string))
                        {
                            string stringValueForEndsWith = (typedValue as string) ?? string.Empty;
                            var stringConstant = Expression.Constant(stringValueForEndsWith.ToLower(), typeof(string)); // Convert search value to lower
                            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                            var endsWithMethodInfo = typeof(string).GetMethod("EndsWith", new[] { typeof(string) }); // Use overload without StringComparison

                            // Call .ToLower().EndsWith(value.ToLower())
                            return Expression.Call(Expression.Call(propertyExpression, toLowerMethod!), endsWithMethodInfo!, stringConstant);
                        }
                        return null;
                    case FilterOperator.NotContains:
                        if (underlyingPropertyType == typeof(string))
                        {
                            string stringValueForNotContains = (typedValue as string) ?? string.Empty;
                            var stringConstant = Expression.Constant(stringValueForNotContains.ToLower(), typeof(string)); // Convert search value to lower
                            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                            var containsMethodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) }); // Use overload without StringComparison

                            // Call !.ToLower().Contains(value.ToLower())
                            var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
                            var containsCall = Expression.Call(Expression.Call(propertyExpression, toLowerMethod!), containsMethodInfo!, stringConstant);
                            return Expression.AndAlso(nullCheck, Expression.Not(containsCall));
                        }
                        return null;
                    default: return null;
                }
            }
            // If none of the above conditions are met (e.g., operator requires a value but none is provided)
            return null;
        }

        private static object? ConvertType(string? value, Type targetType)
        {
            if (value is null)
            {
                if (targetType == typeof(bool) || targetType == typeof(bool?)) return false;
                return null;
            }

            // Remove leading/trailing single quotes if present
            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                value = value.Substring(1, value.Length - 2);
            }

            // Pro booleovské typy chceme povolit parsování "true"/"false" i když je hodnota prázdná pro jiné typy
            if (string.IsNullOrEmpty(value) && targetType != typeof(bool) && targetType != typeof(bool?))
            {
                 // Pro string typy může být prázdný řetězec validní hodnota pro některé operace (např. Contains)
                if (targetType == typeof(string)) return value;
                return null;
            }

            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value, true);
            if (underlyingType == typeof(Guid))
                return Guid.Parse(value);
            if (underlyingType == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value);
            if (underlyingType == typeof(DateTime))
            {
                // Try common date formats, then a general parse
                string[] dateFormats = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy", "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy", "o" };
                if (DateTime.TryParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    return parsedDate;
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDate)) // More general parse
                    return parsedDate;
                throw new FormatException($"Cannot parse '{value}' as DateTime.");
            }
            if (underlyingType == typeof(bool))
            {
                // Trim value before parsing for bool
                string trimmedValue = value.Trim();
                 if (bool.TryParse(trimmedValue, out bool parsedBool))
                    return parsedBool;
                // Allow 1/0 for true/false
                if (trimmedValue == "1") return true;
                if (trimmedValue == "0") return false;
                // Allow common string representations
                if (trimmedValue.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
                if (trimmedValue.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
                if (trimmedValue.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
                if (trimmedValue.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
                // If the value is empty, and it's a boolean type, treat it as false.
                if (string.IsNullOrEmpty(trimmedValue)) return false;
                throw new FormatException($"Cannot parse '{value}' as bool.");
            }
            if (underlyingType == typeof(int)) return int.Parse(value, CultureInfo.InvariantCulture);
            if (underlyingType == typeof(long)) return long.Parse(value, CultureInfo.InvariantCulture);
            if (underlyingType == typeof(decimal)) return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            if (underlyingType == typeof(double)) return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            if (underlyingType == typeof(float)) return float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);

            // Fallback for other types
            return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }
    }
}
