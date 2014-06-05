using System.Collections.Generic;

namespace ExactTarget.EmailFromTemplateCreator
{
    public interface IEmailCreator
    {
        int Create(int emailTemplateId,
            string emailName,
            string subject,
            string htmlBody);

        int Create(int emailTemplateId,
            string emailName,
            string subject,
            KeyValuePair<string, string> contentArea);

    }
}