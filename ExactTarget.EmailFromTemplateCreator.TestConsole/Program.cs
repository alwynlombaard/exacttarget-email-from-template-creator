using System;

namespace ExactTarget.EmailFromTemplateCreator.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var t = new EmailCreator(new ExactTargetConfiguration
            {
                ClientId = null,
                ApiUserName= "",
                ApiPassword = "",
                EndPoint = "https://webservice.s6.exacttarget.com/Service.asmx"
            });

            var templateId = t.RetrieveEmailTemplateId("JustGiving Template");

            try
            {
                t.Create(templateId, "test-email", "test-subject", "test body");
                Console.Write("Done");
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Console.ReadKey();
        }
    }
}
