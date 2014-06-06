using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExactTarget.EmailFromTemplateCreator
{
    public class EmailCreator : IEmailCreator
    {
        private readonly IExactTargetConfiguration _config;

        public EmailCreator(IExactTargetConfiguration config)
        {
            _config = config;
        }

        public int Create(int emailTemplateId,
                            string emailName,
                            string subject,
                            string htmlBody)
        {
            return Create(emailTemplateId, emailName, subject, htmlBody, new KeyValuePair<string, string>());
        }

        public int Create(int emailTemplateId,
            string emailName,
            string subject,
            KeyValuePair<string, string> contentArea )
        {
            return Create(emailTemplateId, emailName, subject, null, contentArea);
        }

        private int Create(  int emailTemplateId, 
                            string emailName, 
                            string subject, 
                            string htmlBody,
                            KeyValuePair<string, string> contentArea)
        {
            return  CreateEmailFromTemplate(emailTemplateId, 
                                    emailName, 
                                    subject, 
                                    htmlBody,
                                    contentArea);
        }

        private int CreateEmailFromTemplate(int templateId, 
                                     string name, 
                                     string subject, 
                                     string htmlBody,
                                     KeyValuePair<string, string> contentAreas)
        {
            var emailSoapXml = GetEmailSoapXml( _config.ApiUserName, 
                                                _config.ApiPassword, 
                                                _config.ClientId, 
                                                templateId,
                                                name, 
                                                subject, 
                                                htmlBody,
                                                contentAreas);
            var xmlSoapResponse = HttpPost(emailSoapXml).Result;

            CreateResponse response;
            try
            {
                response = GetResponseFromSoapResonse(xmlSoapResponse);
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to deserialize response from ExactTarget: " + xmlSoapResponse, ex);
            }

            if (response == null)
            {
                throw new Exception(string.Format("Error reponse from ExactTarget: " + xmlSoapResponse));
            }
            
            if (response.Results.Any(r => !r.StatusCode.Equals("OK", StringComparison.InvariantCultureIgnoreCase)))
            {
                var result = response.Results.First(r => !r.StatusCode.Equals("OK", StringComparison.InvariantCultureIgnoreCase));
                throw new Exception(string.Format("Error response from ExactTarget StatusCode:{0} StatusMessage:{1}\n\n{2}", result.StatusCode, result.StatusMessage, xmlSoapResponse));
            }

            if (!response.OverallStatus.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(string.Format("Error reponse from ExactTarget: " + xmlSoapResponse));
            }

           

            return response.Results.Any() ? response.Results.First().NewId : 0;
        }

        private async Task<string> HttpPost(string emailSoapXml)
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.PostAsync(new Uri(_config.EndPoint), new StringContent(emailSoapXml, Encoding.UTF8, "text/xml")))
                {
                    return await result.Content.ReadAsStringAsync();
                }
            }
        }

        private CreateResponse GetResponseFromSoapResonse(string xmlResponse)
        {
            // ReSharper disable PossibleNullReferenceException
            var xdoc = XDocument.Parse(xmlResponse);
            XNamespace ns = "http://exacttarget.com/wsdl/partnerAPI";
            return xdoc.Descendants(ns + "CreateResponse").Select(x => new CreateResponse
            {

                OverallStatus = x.Element(ns + "OverallStatus").Value,

                Results = new List<Result>
                    {
                        new Result
                        {
                            NewId = int.Parse(x.Descendants(ns + "Results").FirstOrDefault().Element(ns + "NewID").Value),
                            StatusCode = x.Descendants(ns + "Results").FirstOrDefault().Element(ns + "StatusCode").Value,
                            StatusMessage =
                                x.Descendants(ns + "Results").FirstOrDefault().Element(ns + "StatusMessage").Value
                        }
                    }
            }).FirstOrDefault();
            // ReSharper restore PossibleNullReferenceException
        }

        private  string GetEmailSoapXml(string userName, 
                                              string password, 
                                              int? clientId, 
                                              int templateId, 
                                              string name, 
                                              string subject, 
                                              string htmlBody, 
                                              KeyValuePair<string, string> contentArea)
        {

            var contentAreaXml = !string.IsNullOrEmpty( contentArea.Key)
                                 ? "<ContentAreas>" +
                                     "<PartnerKey xsi:nil=\"true\"/>" +
                                     "<Key>" + contentArea.Key + "</Key>" +
                                     "<Content><![CDATA[" + contentArea.Value + "]]></Content>" +
                                     "<IsBlank>false</IsBlank>" +
                                     "<IsDynamicContent>false</IsDynamicContent>" +
                                     "<IsSurvey>false</IsSurvey>"
                                     + "</ContentAreas>"
                                 : "";

            var s =
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">" +
                "<soap:Header>" +
                "<wsa:Action>Create</wsa:Action>" +
                "<wsa:MessageID>urn:uuid:0caa9e7d-bd29-4dab-b268-668343be00bd</wsa:MessageID>" +
                "<wsa:ReplyTo>" +
                "<wsa:Address>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:Address>" +
                "</wsa:ReplyTo>" +
                "<wsa:To>" + _config.EndPoint + "</wsa:To>" +
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
                "<HTMLBody><![CDATA[" + htmlBody + "]]></HTMLBody>" +
                "<TextBody/>" +
                contentAreaXml +
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
    }

    public class CreateResponse
    {
        public string OverallStatus { get; set; }
        public List<Result> Results;

        public CreateResponse()
        {
            Results = new List<Result>();
        }
    }

    public class Result
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public int  NewId { get; set; }
    }
}
