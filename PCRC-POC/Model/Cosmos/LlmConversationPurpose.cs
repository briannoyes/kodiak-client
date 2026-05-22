namespace PCRC.Model.Cosmos;

/// Discriminator for <see cref="LlmConversationTurn.Purpose"/>. Add a value here when wiring a new
/// LLM-using feature; the document set is open-ended per DataModelCosmos.md.
public enum LlmConversationPurpose
{
    RulesDerivation,
    DiscrepancyDetection,
    ContentUnderstanding,
}
