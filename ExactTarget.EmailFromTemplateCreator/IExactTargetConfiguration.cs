namespace ExactTarget.EmailFromTemplateCreator
{
    public interface IExactTargetConfiguration
    {
        string ApiUserName { get; set; }
        string ApiPassword { get; set; }
        string SoapBinding { get; set; }
        string EndPoint { get; set; }
        int? ClientId { get; set; }
    }
}