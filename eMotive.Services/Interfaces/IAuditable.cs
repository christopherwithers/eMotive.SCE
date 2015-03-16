using eMotive.Services.Objects.Audit;

namespace eMotive.Services.Interfaces
{
    public interface IAuditable
    {
        bool RollBack(AuditRecord record);
    }
}
