using DataHub.Core.Models.Metadata;

namespace DataHub.Host.Components.Shared;

public class LookupDialogSettings
{
    public required MetaEntity TargetEntity { get; set; }
    public required MetaField TargetPkField { get; set; }
    public MetaField? DisplayField { get; set; }
}
