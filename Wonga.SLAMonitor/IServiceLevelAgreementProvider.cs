using System;

namespace Wonga.SLAMonitor
{
    public interface IServiceLevelAgreementProvider
    {
        TimeSpan GetServiceLevelAgreement(Type key);
    }
}