namespace ABDULLAgram.Messages
{
    /// <summary>
    /// Discriminator enum for Message state inheritance.
    /// A message is either Draft OR Sent (disjoint, complete).
    /// Transition: Draft → Sent is allowed, Sent → Draft is FORBIDDEN.
    /// </summary>
    public enum MessageStateType
    {
        Draft,
        Sent
    }
}

