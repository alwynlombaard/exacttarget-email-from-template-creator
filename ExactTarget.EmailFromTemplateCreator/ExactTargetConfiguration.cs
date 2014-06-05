namespace ExactTarget.EmailFromTemplateCreator
{
    public class ExactTargetConfiguration : IExactTargetConfiguration
    {
        public string ApiUserName { get; set; }
        public string ApiPassword { get; set; }
        public string SoapBinding { get; set; }
        public string EndPoint { get; set; }
        public int? ClientId { get; set; }
    }
}