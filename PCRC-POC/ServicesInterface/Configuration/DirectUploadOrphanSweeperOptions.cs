namespace PCRC.ServicesInterface.Configuration;

/// Controls the cadence and grace period of the direct-upload orphan sweep
/// (KodiakMultiSelectContractUploadSequence.puml — "Orphan sweep" section).
public sealed class DirectUploadOrphanSweeperOptions
{
    public const string SectionName = "Uploads:DirectOrphanSweeper";

    /// How often the sweeper wakes up. Default: every 15 minutes.
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);

    /// A direct upload is considered orphaned once it has been Pending this long without anyone
    /// calling Finalize. Should be comfortably larger than the SAS expiry handed to clients in
    /// BeginDirect (currently 1 hour).
    public TimeSpan MaxAge { get; set; } = TimeSpan.FromHours(2);

    /// Set to false to disable the background sweeper (e.g., in integration tests that drive
    /// SweepDirectUploadOrphansAsync directly).
    public bool Enabled { get; set; } = true;
}