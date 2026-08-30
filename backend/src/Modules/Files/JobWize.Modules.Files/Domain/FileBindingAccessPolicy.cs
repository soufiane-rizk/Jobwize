namespace JobWize.Modules.Files.Domain;

public enum FileBindingAccessPolicy
{
    OwnerOnly = 1,
    OwnerAndAdministrators = 2,
    ResourceViewers = 3
}
