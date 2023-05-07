namespace AzureDevOpsTestCasesAutomationConnector;

public class AssociatedAutomationInfo
{
    public int? TestCaseId { get; set; }
    
    public string? AutomatedTestStorage { get; set; }
    
    public string? NameSpace { get; set; }
    
    public string? TestClass { get; set; }
    
    public string? Method { get; set; }
}