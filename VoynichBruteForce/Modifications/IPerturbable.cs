namespace VoynichBruteForce.Modifications;

/// <summary>
/// Interface for modifiers that support small parameter adjustments (perturbation).
/// Unlike full replacement which creates an entirely new modifier, perturbation
/// makes small tweaks to existing parameters, enabling smoother optimization
/// in the search space.
/// </summary>
public interface IPerturbable
{
    /// <summary>
    /// Creates a new modifier with slightly adjusted parameters.
    /// The modifier remains immutable - this returns a new instance.
    /// </summary>
    /// <param name="random">Random source for choosing perturbation direction.</param>
    /// <returns>A new modifier with perturbed parameters.</returns>
    ITextModifier Perturb(Random random);
}
