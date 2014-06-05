using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ExactTarget.EmailFromTemplateCreator.ExactTargetApi;

namespace ExactTarget.EmailFromTemplateCreator
{
    public class EmailCreator
    {
        private readonly IExactTargetConfiguration _config;
        private readonly SoapClient _client;

        public EmailCreator(IExactTargetConfiguration config)
        {
            _config = config;
            _client = new SoapClient(_config.SoapBinding ?? "ExactTarget.Soap", _config.EndPoint);
            if (_client.ClientCredentials == null) return;
            _client.ClientCredentials.UserName.UserName = _config.ApiUserName;
            _client.ClientCredentials.UserName.Password = _config.ApiPassword;
        }

        public int Create(  int emailTemplateId, 
                            string emailName, 
                            string subject, 
                            string body)
        {
            CreateEMailFromTemplate(emailTemplateId, 
                                    emailName, 
                                    subject, 
                                    body);

            var request = new RetrieveRequest
            {
                ClientIDs = _config.ClientId.HasValue 
                            ? new[]{new ClientID {ID = _config.ClientId.Value, IDSpecified = true }}
                            : null,
                ObjectType = "Email",
                Properties = new[] { "Name", "ID" }
            };

            string requestId;
            APIObject[] results;
            _client.Retrieve(request, out requestId, out results);

            if (results != null && results.Any())
            {
                return results.Cast<Email>()
                       .Where(r => r.Name.Equals(emailName, StringComparison.InvariantCultureIgnoreCase))
                       .Max(r => r.ID);
            }
            return 0;
        }

        private void CreateEMailFromTemplate(int templateId, 
                                     string name, 
                                     string subject, 
                                     string content)
        {
            var emailSoapXml = GetEmailSoapXml( _config.ApiUserName, 
                                                _config.ApiPassword, 
                                                _config.ClientId, 
                                                templateId,
                                                name, 
                                                subject, 
                                                "<![CDATA[" + content + "]]>");
            var reqBytes = new UTF8Encoding().GetBytes(emailSoapXml);
            
            var req = (HttpWebRequest)WebRequest.Create(_config.EndPoint);
            req.Method = "POST";
            req.ContentType = "text/xml;charset=UTF-8";
            req.ContentLength = reqBytes.Length;
            
            using (var reqStream = req.GetRequestStream())
            {
                reqStream.Write(reqBytes, 0, reqBytes.Length);
            }

            var resp = (HttpWebResponse)req.GetResponse();
            var xmlResponse = "";
            var stream = resp.GetResponseStream();
            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    xmlResponse = sr.ReadToEnd();
                }
            }
            Console.WriteLine(xmlResponse);
        }

        private static string GetEmailSoapXml(string userName, 
                                              string password, 
                                              int? clientId, 
                                              int templateId, 
                                              string name, 
                                              string subject, 
                                              string content)
        {
            var s =
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">" +
                "<soap:Header>" +
                "<wsa:Action>Create</wsa:Action>" +
                "<wsa:MessageID>urn:uuid:0caa9e7d-bd29-4dab-b268-668343be00bd</wsa:MessageID>" +
                "<wsa:ReplyTo>" +
                "<wsa:Address>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:Address>" +
                "</wsa:ReplyTo>" +
                "<wsa:To>https://webservice.s6.exacttarget.com/Service.asmx</wsa:To>" +
                "<wsse:Security soap:mustUnderstand=\"1\">" +
                "<wsse:UsernameToken wsu:Id=\"SecurityToken-8ab9d52b-cf40-465b-9464-1a7c7f000460\">" +
                "<wsse:Username>" + userName + "</wsse:Username>" +
                "<wsse:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">" + password + "</wsse:Password>" +
                "</wsse:UsernameToken>" +
                "</wsse:Security>" +
                "</soap:Header>" +
                "<soap:Body>" +
                "<CreateRequest xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">" +
                "<Objects xsi:type=\"par:Email\" xmlns:par=\"http://exacttarget.com/wsdl/partnerAPI\">" +
                "<PartnerKey/>" +
                (clientId.HasValue ?  ("<Client><ID>" + clientId.GetValueOrDefault() + "</ID></Client>") : "") +
                "<ObjectID xsi:nil=\"true\"/>" +
                "<Name>" + name + "</Name>" +
                "<HTMLBody>" + content + "</HTMLBody>" +
                "<TextBody/>" +
                //"<ContentAreas>" +
                //"<PartnerKey xsi:nil=\"true\"/>" +
                //"<Key>DynamicArea</Key>" +
                //"<Content>" + content + "</Content>" +
                //"<IsBlank>false</IsBlank>" +
                //"<IsDynamicContent>false</IsDynamicContent>" +
                //"<IsSurvey>false</IsSurvey>" +
                //"</ContentAreas>" +
                "<Subject>" + subject + "</Subject>" +
                "<IsHTMLPaste>false</IsHTMLPaste>" +
                "<CharacterSet>UTF-8</CharacterSet>" +
                "<Status>New</Status>" +
                "<Template>" +
                "<ID>" + templateId + "</ID>" +
                "</Template>" +
                "</Objects>" +
                "</CreateRequest>" +
                "</soap:Body>" +
                "</soap:Envelope>";
            return s;
        }

        public int RetrieveEmailTemplateId(string name)
        {
            var request = new RetrieveRequest
            {
                ClientIDs = _config.ClientId.HasValue 
                            ? new[] { new ClientID { ID = _config.ClientId.Value, IDSpecified = true } }
                            : null,
                ObjectType = "Template",
                Properties = new[] { "ID", "TemplateName", "ObjectID", "CustomerKey" }
            };
            string requestId;
            APIObject[] results;

            _client.Retrieve(request, out requestId, out results);

            if (results != null && results.Any())
            {
                var t = results.Cast<Template>().FirstOrDefault(r => r.TemplateName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (t != null)
                {
                    return t.ID;
                }
            }

            return 0;
        }

        
    }
}
