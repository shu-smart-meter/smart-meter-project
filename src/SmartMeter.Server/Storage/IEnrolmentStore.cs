using SmartMeter.Model;

namespace SmartMeter.Server.Storage;

// storage interface for enrolled client keys
public interface IEnrolmentStore
{
    // save or overwrite a client record
    Task SaveAsync(EnrolmentRecord record, CancellationToken cancellationToken = default);

    // return the record for a meter, or null when the key is unknown
    Task<EnrolmentRecord?> FindAsync(string meterId, CancellationToken cancellationToken = default);
}