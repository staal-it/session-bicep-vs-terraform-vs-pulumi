using Pulumi.Automation;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

namespace InlineProgram
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // define our pulumi program "inline"
            var program = PulumiFn.Create(() =>
            {
                // Create an Azure Resource Group
                var resourceGroup = new ResourceGroup("rg-automation-api");

                // Create an Azure resource (Storage Account)
                var storageAccount = new StorageAccount("stgautomationapi", new StorageAccountArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    Sku = new SkuArgs
                    {
                        Name = SkuName.Standard_LRS
                    },
                    Kind = Kind.StorageV2
                });

                var blobEndpoint = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Blob);
                
                return new Dictionary<string, object?>
                {
                    ["blobEndpoint"] = blobEndpoint
                };
            });

            // to destroy our program, we can run "dotnet run destroy"
            var destroy = args.Any() && args[0] == "destroy";

            var projectName = "automation-api-example";
            var stackName = "dev";

            // create or select a stack matching the specified name and project
            // this will set up a workspace with everything necessary to run our inline program (program)
            var stackArgs = new InlineProgramArgs(projectName, stackName, program);
            var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

            Console.WriteLine("successfully initialized stack");

            // for inline programs, we must manage plugins ourselves
            Console.WriteLine("installing plugins...");
            await stack.Workspace.InstallPluginAsync("azure-native", "v2.4.0");
            Console.WriteLine("plugins installed");

            // set stack configuration specifying the region to deploy
            Console.WriteLine("setting up config...");
            await stack.SetConfigAsync("azure-native:location", new ConfigValue("westeurope"));
            Console.WriteLine("config set");

            Console.WriteLine("refreshing stack...");
            await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });
            Console.WriteLine("refresh complete");

            if (destroy)
            {
                Console.WriteLine("destroying stack...");
                await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });
                Console.WriteLine("stack destroy complete");
            }
            else
            {
                Console.WriteLine("updating stack...");
                var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

                if (result.Summary.ResourceChanges != null)
                {
                    Console.WriteLine("update summary:");
                    foreach (var change in result.Summary.ResourceChanges)
                        Console.WriteLine($"    {change.Key}: {change.Value}");
                }

                Console.WriteLine($"Blob url: {result.Outputs["blobEndpoint"].Value}");
            }
        }
    }
}