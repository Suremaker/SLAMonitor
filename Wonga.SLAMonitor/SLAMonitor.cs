using System;
using System.Diagnostics;
using Castle.DynamicProxy;
using log4net;

namespace Wonga.SLAMonitor
{
    public class SLAMonitor : IInterceptor
    {
        private readonly IServiceLevelAgreementProvider _slaProvider;
        private static readonly ILog logger = LogManager.GetLogger(typeof(SLAMonitor));

        public SLAMonitor(IServiceLevelAgreementProvider slaProvider)
        {
            _slaProvider = slaProvider;
        }

        public void Intercept(IInvocation invocation)
        {
            logger.DebugFormat("Looking up key {0}",invocation.TargetType);
            var sla = _slaProvider.GetServiceLevelAgreement(invocation.TargetType);
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                invocation.Proceed();
            }
            finally
            {
                stopwatch.Stop();

                Action<string, object, object> log;

                if (stopwatch.ElapsedMilliseconds > sla.TotalMilliseconds)
                {
                    log = logger.ErrorFormat;
                }
                else
                {
                    log = logger.InfoFormat;
                }

                log("SLA={0} ResponseTime={1} milliseconds", invocation.TargetType.FullName, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
