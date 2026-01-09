using DataHub.Core.Models.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DataHub.Core.Services
{
    public class QueryParserService
    {
        private readonly ILogger<QueryParserService> _logger;

        public QueryParserService(ILogger<QueryParserService> logger)
        {
            _logger = logger;
        }
        public SavedCriteria ParseQuery(string query)
        {
            _logger.LogInformation("Parsing query: {Query}", query);
            var savedCriteria = new SavedCriteria();
            query = query.Trim();

            // Normalize whitespace
            query = Regex.Replace(query, @"\s+", " ");
            _logger.LogInformation("Query after whitespace normalization: '{Query}'", query);

            string filterQuery = query;
            string sortQuery = string.Empty;

            // Try to find "ORDERBY" (case-insensitive)
            int orderByKeywordIndex = query.IndexOf("ORDERBY", StringComparison.OrdinalIgnoreCase);

            if (orderByKeywordIndex != -1)
            {
                // filterQuery should be the part before "ORDERBY"
                filterQuery = query.Substring(0, orderByKeywordIndex).Trim();

                // sortQuery should be the part after "ORDERBY"
                sortQuery = query.Substring(orderByKeywordIndex + "ORDERBY".Length).Trim();

                savedCriteria.Sorts = ParseSorts(sortQuery);
                _logger.LogInformation("Extracted filterQuery: '{FilterQuery}', sortQuery: '{SortQuery}' (Manual Split - Adjusted)", filterQuery, sortQuery);
            }
            else
            {
                _logger.LogInformation("ORDERBY keyword not found in query (Manual Split check - Adjusted).");
            }

            if (!string.IsNullOrWhiteSpace(filterQuery))
            {
                savedCriteria.Filters = ParseFilters(filterQuery);
            }

            _logger.LogInformation("Parsed Filters Count: {FilterCount}, Sorts Count: {SortsCount}", savedCriteria.Filters?.Count ?? 0, savedCriteria.Sorts?.Count ?? 0);
            return savedCriteria;
        }

        private List<SortCriterion> ParseSorts(string sortQuery)
        {
            _logger.LogInformation("Parsing sorts: {SortQuery}", sortQuery);
            var sorts = new List<SortCriterion>();
            var sortParts = sortQuery.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in sortParts)
            {
                var match = Regex.Match(part.Trim(), @"^(\w+)(\s+(ASC|DESC))?$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var sortCriterion = new SortCriterion
                    {
                        FieldName = match.Groups[1].Value,
                        SortDirection = match.Groups[3].Success ? match.Groups[3].Value.ToUpper() : "ASC"
                    };
                    _logger.LogInformation("  Parsed Sort: FieldName={FieldName}, SortDirection={SortDirection}", sortCriterion.FieldName, sortCriterion.SortDirection);
                    sorts.Add(sortCriterion);
                }
                else
                {
                    _logger.LogWarning("  Failed to parse sort part: {Part}", part);
                }
            }
            return sorts;
        }

        private List<FilterCriterion> ParseFilters(string filterQuery)
        {
            _logger.LogInformation("Parsing filters: {FilterQuery}", filterQuery);
            return ParseLogicalExpression(filterQuery.Trim());
        }

        private List<FilterCriterion> ParseLogicalExpression(string query)
        {
            _logger.LogInformation("Parsing logical expression: {Query}", query);
            query = query.Trim();

            // Recursively remove outer parentheses if they enclose the entire expression
            string currentQuery = query;
            string trimmedQuery = TrimOuterParentheses(currentQuery);
            while (trimmedQuery.Length < currentQuery.Length)
            {
                currentQuery = trimmedQuery;
                trimmedQuery = TrimOuterParentheses(currentQuery);
            }
            query = currentQuery;

            int nestingLevel = 0;
            int foundOrIndex = -1;
            int foundAndIndex = -1;

            // Find the leftmost top-level OR or AND
            for (int i = 0; i < query.Length; i++)
            {
                Console.WriteLine($"DEBUG: ParseLogicalExpression - Loop: i={i}, char='{query[i]}', nestingLevel={nestingLevel}");
                if (query[i] == '(') nestingLevel++;
                else if (query[i] == ')') nestingLevel--;

                if (nestingLevel == 0)
                {
                    if (i + 3 < query.Length && query.Substring(i, 4).ToUpper() == " OR ")
                    {
                        foundOrIndex = i;
                        
                        break; // Prioritize OR, so break immediately
                    }
                    else if (i + 4 < query.Length && query.Substring(i, 5).ToUpper() == " AND ")
                    {
                        foundAndIndex = i;
                        
                        // Don't break, keep looking for OR (OR has lower precedence)
                    }
                }
            }

            if (foundOrIndex != -1)
            {
                
                var left = ParseLogicalExpression(query.Substring(0, foundOrIndex));
                var right = ParseLogicalExpression(query.Substring(foundOrIndex + 4));
                if (right.Any())
                {
                    right.First().LogicalOperator = "OR";
                }
                return left.Concat(right).ToList();
            }
            else if (foundAndIndex != -1)
            {
                
                var left = ParseLogicalExpression(query.Substring(0, foundAndIndex));
                var right = ParseLogicalExpression(query.Substring(foundAndIndex + 5));
                if (right.Any())
                {
                    right.First().LogicalOperator = "AND";
                }
                return left.Concat(right).ToList();
            }

            // Base case: no logical operators, should be a single criterion
            _logger.LogInformation("Base case: Parsing single criterion: {Query}", query);
            if (string.IsNullOrWhiteSpace(query)) return new List<FilterCriterion>();
            var criterion = ParseSingleCriterion(query);
            
            return new List<FilterCriterion> { criterion };
        }

        private string TrimOuterParentheses(string query)
        {
            query = query.Trim();
            if (query.Length > 1 && query.StartsWith("(") && query.EndsWith(")"))
            {
                int nestingLevel = 0;
                // Check if the parentheses actually wrap the entire expression
                // Iterate up to query.Length - 1 (excluding the last character, which is the closing parenthesis)
                for (int i = 0; i < query.Length - 1; i++)
                {
                    if (query[i] == '(') nestingLevel++;
                    else if (query[i] == ')') nestingLevel--;

                    // If nestingLevel drops to 0 before the end of the string (excluding the last char),
                    // it means the parentheses do not wrap the entire expression.
                    if (nestingLevel == 0 && i < query.Length - 1)
                    {
                        return query; // Not entirely wrapped
                    }
                }
                // If we reach here, it means nestingLevel never dropped to 0 prematurely,
                // and the outermost parentheses are a valid wrapper.
                return query.Substring(1, query.Length - 2).Trim();
            }
            return query;
        }

        private FilterCriterion ParseSingleCriterion(string condition)
        {
            _logger.LogInformation("Parsing single criterion: {Condition}", condition);
            var regex = new Regex(
                @"^\s*(\w+)\s+(EQ|NEQ|GT|GTE|LT|LTE|CONTAINS|STARTSWITH|ENDSWITH|IN|NOTIN|ISNULL|ISNOTNULL|ISTRUE|ISFALSE)\s+(.*)$\s*",
                RegexOptions.IgnoreCase
            );

            var match = regex.Match(condition.Trim());

            if (!match.Success)
            {
                _logger.LogError("Invalid filter condition: {Condition}", condition);
                throw new ArgumentException($"Invalid filter condition: {condition}");
            }

            var fieldName = match.Groups[1].Value.Trim();
            var op = match.Groups[2].Value.ToUpper();
            var value = match.Groups[3].Value.Trim();

            _logger.LogInformation("  Parsed FieldName={FieldName}, Operator={Operator}, Value={Value}", fieldName, op, value);

            // Remove single quotes from string literals
            if (value.StartsWith("'") && value.EndsWith("'") && value.Length > 1)
            {
                value = value.Substring(1, value.Length - 2);
            }

            FilterOperator operatorEnum = op switch
            {
                "EQ" => FilterOperator.Equals,
                "NEQ" => FilterOperator.NotEquals,
                "GT" => FilterOperator.GreaterThan,
                "GTE" => FilterOperator.GreaterThanOrEqual,
                "LT" => FilterOperator.LessThan,
                "LTE" => FilterOperator.LessThanOrEqual,
                "CONTAINS" => FilterOperator.Contains,
                "STARTSWITH" => FilterOperator.StartsWith,
                "ENDSWITH" => FilterOperator.EndsWith,
                "IN" => FilterOperator.In,
                "NOTIN" => FilterOperator.NotIn,
                "ISNULL" => FilterOperator.IsNull,
                "ISNOTNULL" => FilterOperator.IsNotNull,
                "ISTRUE" => FilterOperator.IsTrue,
                "ISFALSE" => FilterOperator.IsFalse,
                _ => throw new ArgumentException($"Unknown operator: {op}"),
            };

            var criterion = new FilterCriterion
            {
                FieldName = fieldName,
                Operator = operatorEnum
            };

            if (operatorEnum == FilterOperator.In || operatorEnum == FilterOperator.NotIn)
            {
                var valuesString = value.Trim('(', ')');
                criterion.Values = valuesString.Split(',')
                                               .Select(v => v.Trim())
                                               .ToList();
            }
            else
            {
                criterion.Value = value;
            }
            
            return criterion;
        }
    }
}
