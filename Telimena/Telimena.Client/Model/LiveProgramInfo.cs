namespace TelimenaClient
{
    /// <summary>
    /// Holds live &amp; static data about a program
    /// Live data is loaded from the cloud, whereas static is loaded when Telimena is constructed
    /// </summary>
    public class LiveProgramInfo 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveProgramInfo"/> class.
        /// </summary>
        /// <param name="programInfo">The program information.</param>
        public LiveProgramInfo(ProgramInfo programInfo)
        {
            this.Program = programInfo;
        }

        /// <summary>
        /// Static program info
        /// </summary>
        /// <value>The program.</value>
        public ProgramInfo Program { get;  }

        /// <summary>
        /// Gets or sets the name of the updater for this app.
        /// </summary>
        /// <value>The name of the updater.</value>
        public string UpdaterName { get; set; }

        /// <summary>
        /// ID of the program
        /// </summary>
        /// <value>The program identifier.</value>
        public int ProgramId { get; set; }

        /// <summary>
        /// ID of the current user of the app
        /// </summary>
        public int UserId { get; set; }
    }
}