Console.WriteLine("Azure DevOps Test Cases Automation Connector");

IList<AssociatedAutomationInfo> associatedAutomationInfos;

var tfsUrl = ConfigurationManager.AppSettings.Get("TfsUrl");

var personalAccessToken = ConfigurationManager.AppSettings.Get("PersonalAccessToken");

var testCaseAssociationCsvPath = ConfigurationManager.AppSettings.Get("TestCaseAssociationCsvPath");

using (new AssertionScope())
{
    tfsUrl.Should().NotBeNullOrEmpty();
    personalAccessToken.Should().NotBeNullOrEmpty();
    testCaseAssociationCsvPath.Should().NotBeNullOrEmpty();
}

Console.WriteLine($"TFS URL: {tfsUrl}\nPersonal Access Token: {personalAccessToken}");

using (var reader = new StreamReader(testCaseAssociationCsvPath!))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<AssociatedAutomationInfo>().ToList();

    associatedAutomationInfos = records;
}

associatedAutomationInfos.Should().NotBeNullOrEmpty();

Console.WriteLine($"\n{testCaseAssociationCsvPath}");
foreach (var associatedAutomationInfo in associatedAutomationInfos)
{
    Console.WriteLine($"{associatedAutomationInfo.TestCaseId} - " +
                      $"{associatedAutomationInfo.AutomatedTestStorage} - " +
                      $"{associatedAutomationInfo.NameSpace} - " +
                      $"{associatedAutomationInfo.TestClass} - " +
                      $"{associatedAutomationInfo.Method}");
}

Console.WriteLine("Performing test methods association with DevOps Test Cases...");
foreach (var associatedAutomationInfo in associatedAutomationInfos)
{
    Console.WriteLine($"{associatedAutomationInfo.TestCaseId} - " +
                      $"{associatedAutomationInfo.AutomatedTestStorage} - " +
                      $"{associatedAutomationInfo.NameSpace} - " +
                      $"{associatedAutomationInfo.TestClass} - " +
                      $"{associatedAutomationInfo.Method}");

    try
    {
        var devOpsTestCaseFields = new Dictionary<string, object>
        {
            { "Microsoft.VSTS.TCM.AutomatedTestId", 
                associatedAutomationInfo.TestCaseId! },
            { "Microsoft.VSTS.TCM.AutomatedTestStorage", 
                associatedAutomationInfo.AutomatedTestStorage! },
            { "Microsoft.VSTS.TCM.AutomatedTestName", 
                $"{associatedAutomationInfo.NameSpace!}.{associatedAutomationInfo.TestClass}.{associatedAutomationInfo.Method}" }
        };
        
        Console.WriteLine("Updating DevOps Test Case with below data:");
        foreach (var devOpsTestCaseField in devOpsTestCaseFields)
        {
            Console.WriteLine($"{devOpsTestCaseField.Key} - {devOpsTestCaseField.Value}");
        }

        var connection = new VssConnection(new Uri(tfsUrl!), new VssBasicCredential(string.Empty, personalAccessToken));
        
        var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
        
        var patchDocument = new JsonPatchDocument();
        
        patchDocument.AddRange(
            devOpsTestCaseFields.Keys.Select(key => 
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/" + key, Value = devOpsTestCaseFields[key] }));

        var workItem = witClient.UpdateWorkItemAsync(patchDocument, (int)associatedAutomationInfo.TestCaseId!).Result;
        
        workItem.Url.Should().NotBeNullOrEmpty();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}