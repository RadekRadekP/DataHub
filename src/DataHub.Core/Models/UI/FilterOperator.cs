using System.ComponentModel;

namespace DataHub.Core.Models.UI
{
    public enum FilterOperator
    {
        [Description("Obsahuje")]
        Contains,
        [Description("Neobsahuje")]
        NotContains,
        [Description("Začíná na")]
        StartsWith,
        [Description("Končí na")]
        EndsWith,
        [Description("Je rovno")]
        Equals,
        [Description("Není rovno")]
        NotEquals,
        [Description("Je větší než")]
        GreaterThan,
        [Description("Je větší nebo rovno")]
        GreaterThanOrEqual,
        [Description("Je menší než")]
        LessThan,
        [Description("Je menší nebo rovno")]
        LessThanOrEqual,
        [Description("Je v seznamu")]
        In,
        [Description("Není v seznamu")]
        NotIn,
        [Description("Je null")]
        IsNull,
        [Description("Není null")]
        IsNotNull,
        [Description("Je pravda")]
        IsTrue,
        [Description("Není pravda")]
        IsFalse
    }
}