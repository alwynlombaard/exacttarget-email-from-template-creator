using System;
using System.Collections.Generic;

namespace ExactTarget.EmailFromTemplateCreator.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var emailCreator = new EmailCreator(new ExactTargetConfiguration
            {
                ApiUserName = "",
                ApiPassword = "",
                EndPoint = "https://webservice.s6.exacttarget.com/Service.asmx"
            });

            const int templateId = 1802;

            try
            {
                var id = emailCreator.Create(templateId,
                    "test-email",
                    "test-subject",
                    new KeyValuePair<string, string>("ContentAreaName", "<p>Test content</p>")
                    );

                Console.Write("Done {0}", id);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Console.ReadKey();
        }
    }
}
