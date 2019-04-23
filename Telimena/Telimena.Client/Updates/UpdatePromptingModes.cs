namespace TelimenaClient
{
    /// <summary>
    /// How should the 'update opportunity' be communicated to the user
    /// </summary>
    public enum UpdatePromptingModes
    {
        /// <summary>
        /// Download the update packages before presenting the 'there is an update available, would you like to continue' notification.
        /// <para>This is good for large updates. The User don't need to wait for the download to complete, however the update interrupts their work.</para>
        /// </summary>
        PromptAfterDownload,

        /// <summary>
        /// Present the 'there is an update available, would you like to continue' notification before downloading the update packages.
        /// <para>This is good for quick updates right after app start. User 'starts the update' before they start using the app, and they need to wait for the download to complete</para>
        /// </summary>
        PromptBeforeDownload,

   

        /// <summary>
        /// The user is not given a choice at all, the update happens
        /// </summary>
        DontPrompt
    }
}