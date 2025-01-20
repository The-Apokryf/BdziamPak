using BdziamPak.PackageModel;

namespace BdziamPak.Sources.Model;

/// <summary>
///     Class representing json file structure - index of BdziamPak packages
/// </summary>
public class BdziamPakSourceIndex
{
    /// <summary>
    ///     Name of the source
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Description of the source
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     References to the BdziamPaks
    /// </summary>
    public List<BdziamPakMetadata> Paks { get; set; }
}