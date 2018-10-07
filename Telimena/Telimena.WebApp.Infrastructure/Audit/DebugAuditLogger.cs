using System.Diagnostics;
using System.Text;

namespace MvcAuditLogger
{
    public class DebugAuditLogger : IAuditLogger
    {
        public void StoreWithoutRequestData(IAudit audit)
        {
            Debug.WriteLine(this.PrepareString(audit, false));

        }

        public void StoreWithRequestData(IAudit audit)
        {
            Debug.WriteLine(this.PrepareString(audit, true));
        }

        internal string PrepareString(IAudit audit, bool includeData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"User: [{audit.UserName}]. ");
            sb.Append($"Ip: [{audit.IPAddress}]. ");
            sb.Append($"Area: [{audit.AreaAccessed}]. ");

            if (includeData)
            {
                sb.Append($"RequestData: [{audit.Data}]");
            }

            return sb.ToString();
        }
    }
}