using System;
using System.Collections.Generic;
using System.Text;
using Duber.Domain.SharedKernel.Model;
using Newtonsoft.Json;

namespace Duber.Domain.ACL.Translators
{
    public class PaymentInfoTranslator
    {
        public static PaymentInfo Translate(string responseContent)
        {
            var paymentInfoList = JsonConvert.DeserializeObject<List<string>>(responseContent);
            if (paymentInfoList.Count != 5)
                throw new InvalidOperationException("The payment service response is not consistent.");

            return new PaymentInfo(
                int.Parse(paymentInfoList[3]),
                Enum.Parse<PaymentStatus>(paymentInfoList[0]),
                paymentInfoList[2],
                paymentInfoList[1]
            );
        }
    }
}
