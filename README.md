Create an ExactTarget email from a template via the API
=======================================

This library makes it possible to create an ExactTarget email from a specified template with a content area.

How to use
----------


Get it from Nuget:
```
PM> Install-Package ExactTarget.EmailFromTemplate
```

```C#
var emailCreator = new EmailCreator(new ExactTargetConfiguration
{
	ApiUserName = "your api user name",
	ApiPassword = "your api password",
	//your end point given to you by ET
	EndPoint = "https://webservice.s6.exacttarget.com/Service.asmx",
	ClientId = 123 //optional business unit id
});

//the template to create the email from
const int templateId = 1802; 

try
{
	var emailId = emailCreator.Create(templateId,
		"test-email",
		"test-subject",
		new KeyValuePair<string, string>("ContentAreaName", "<p>Test content</p>")
		);
}
catch (Exception ex)
{
	...
}			
			
```