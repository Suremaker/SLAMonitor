using System.Collections.Generic;
using System.Linq;

namespace Wonga.SLAMonitor.Async
{
    /// <summary>
    /// SLA definition provider
    /// </summary>
    public class SlaProvider
    {
        private readonly List<SlaDefinition> _definitions = new List<SlaDefinition>();

        /// <summary>
        /// Returns message definitions for specified requestMessage object.
        /// </summary>
        public IEnumerable<MessageDefinition> GetRequestDefinitions(object requestMessage)
        {
            return _definitions.Where(d => d.Request.Type.IsInstanceOfType(requestMessage)).Select(d => d.Request);
        }


        /// <summary>
        /// Returns all SLA definitions for specified responseMessage.
        /// </summary>
        public IEnumerable<SlaDefinition> GetSlaDefinitionsFor(object responseMessage)
        {
            return _definitions.Where(d => d.Response.Type.IsInstanceOfType(responseMessage));
        }

        /// <summary>
        /// Adds SLA definition
        /// </summary>
        public void Add(SlaDefinition definition)
        {
            _definitions.Add(definition);
        }
    }
}