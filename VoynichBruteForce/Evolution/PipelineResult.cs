using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Evolution;

public class PipelineResult
{
    /// <summary>
    /// Abbreviated description of the algorithm used in this pipeline to modify the source text.
    /// </summary>
    public string PipelineDescription { get; }

    /// <summary>
    /// This pipeline's total deviation from the Voynich's empirical statistical profile. 0.0 would be a perfect match.
    /// </summary>
    public double TotalErrorScore { get; init; }

    /// <summary>
    /// Estimation of how difficult this pipeline would have been to execute for the Voynich author(s).
    /// </summary>
    public int TotalCognitiveLoad { get; set; }

    /// <summary>
    /// Deltas from the Voynich for each ranking method.
    /// </summary>
    public List<RankerResult> Results { get; set; }

    public PipelineResult(List<ITextModifier> modifiers, List<RankerResult> results)
    {
        Results = results;
        PipelineDescription = string.Join(" -> ", modifiers.Select(m => m.Name));
        TotalCognitiveLoad = modifiers.Sum(s => s.CognitiveCost.Value);

        var initialError = results.Sum(r => r.NormalizedError * r.Weight.ToMultiplier());
        double cognitiveLoad = modifiers.Sum(m => m.CognitiveCost.Value);

        /*
         * We want to penalise very complex solutions, because the more complex they are,
         * the less likey they were actually performed by the Voynich authors.
         * But it should not be an overriding factor: the creators of the Voynich were clearly a unique breed!
         * We will try to nudge the evolution towards a *simple* solution which emulates the Voynich, if one exists.
         * But that's just an 'if'. We'll accept a complex one, within reason.
         */

        // Apply a soft penalty.
        if (cognitiveLoad > CognitiveComplexity.SoftWallComplexity)
        {
            initialError *= 2.0;
        }

        // Apply a hard wall - arbitrarily decide this was too much for the original authors.
        if (cognitiveLoad > CognitiveComplexity.HardWallComplexity)
        {
            initialError += 1000;
        }

        TotalErrorScore = initialError;
    }
}