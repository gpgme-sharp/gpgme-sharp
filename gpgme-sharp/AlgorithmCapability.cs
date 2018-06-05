using System;
using System.Collections.Generic;

namespace Libgpgme
{
    public class AlgorithmCapabilityAttribute : Attribute
    {
        protected AlgorithmCapability _type;

        public AlgorithmCapabilityAttribute(AlgorithmCapability type) {
            _type = type;
        }

        public AlgorithmCapability Type {
            get { return _type; }
        }

        internal static string GetKeyUsageText(AlgorithmCapability type) {
            var caps = new List<string>();
            if ((type & AlgorithmCapability.CanAuth) == AlgorithmCapability.CanAuth) {
                caps.Add("auth");
            }
            if ((type & AlgorithmCapability.CanSign) == AlgorithmCapability.CanSign) {
                caps.Add("sign");
            }
            if ((type & AlgorithmCapability.CanEncrypt) == AlgorithmCapability.CanEncrypt) {
                caps.Add("encrypt");
            }
            return caps.Count > 0 
                ? string.Join(",", caps.ToArray()) 
                : null;
        }
    }
}