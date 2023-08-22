using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Speckle.Core.Credentials;
using SpeckleQueryExtensions;
using System.Data.SQLite;
using Speckle.Core.Models;
using Speckle.Core.Serialisation;
using Speckle.Core.Transports;
using Newtonsoft.Json.Linq;
using BCFSpeckleLibrary;
using Speckle.Core.Kits;
using Speckle.Core.Api;

namespace QueryExtensionTest
{
  internal class Program
  {
    static void Main()
    {
      var kits = KitManager.Kits;
      foreach (var k in kits)
      {
        Console.WriteLine();
        Console.WriteLine(k.Name);

        foreach (var con in k.Converters)
        {
          Console.Write("Converter: ");
          Console.WriteLine(con);
        }
      }

      Console.WriteLine();
      //var BCFkit = KitManager.Kits.First(kit => kit.Name == "Better Factory Kit");
      //Console.WriteLine(BCFkit);

      Console.WriteLine();
      var tys = KitManager.Types.Where(ty => ty.Name.Contains("BCF")).Select(a => a).ToList();
      foreach (var ty in tys)
      {
        Console.Write("Type: ");
        Console.Write(ty);
        Console.Write("  /  ");
        Console.WriteLine(ty.Name);
      }


      var acc = AccountManager.GetDefaultAccount();
      Console.WriteLine(acc.userInfo.name);
      Console.WriteLine(acc.serverInfo.url);
      var stream = new StreamWrapper("http://bettercncfactory.iaac.net/streams/f96f170ac3");

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("\n\n======== BRANCH TEST\n");
      Console.ResetColor();

      //var branchMan = new BCFBranch(acc, "TestProject2");
      //Console.WriteLine(branchMan.StreamId);
      //Console.WriteLine(branchMan.ProjectIsValid);
      //var kir = branchMan.SyncStream().Result;
      //if (kir != null) Console.WriteLine(string.Join(", ", kir));
      //branchMan.MoveToBranch("nested", new List<string> { "tp_mdf_6", "tp_mdf_7" }).Wait();


      //var sheetMan = new BCFSheetStatus(acc, "mat", 0);
      //sheetMan.SheetToBranch("TestSheet2").Wait();


      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("\n\n======== QUERY TEST\n");
      Console.ResetColor();

      var agent = new QueryAgent(acc)
      {
        Stream = new StreamWrapper("http://bettercncfactory.iaac.net/streams/ec2140af00/branches/done/ecoplex/12")
      };
      agent.AddLayer(10);
      agent.layers[0].AddQuery("sheet_name", "", "!=");
      agent.layers[0].fields = new List<string> { "material", "speckle_type" };
      agent.AddLayer(10);
      agent.layers[1].AddQuery("part_name", "", "!=");
      agent.layers[1].fields = new List<string> { "part_name", "material", "speckle_type" };
      agent.GenerateQueryVariables().Wait();
      agent.GenerateQueryString();
      var queryResult = agent.RunQuery().Result;
      var l0 = agent.GetLayer(0);
      foreach (var shir in l0.Values)
      {
        Console.WriteLine(shir.Item1);
        Console.WriteLine("\n");
        foreach (var kir in shir.Item2)
        {
          Console.WriteLine(kir);
        }
        Console.WriteLine("================");
        Console.WriteLine("\n\n");
      }

      Console.WriteLine("================================================================");
      Console.WriteLine("\n\n");

      var l1 = agent.GetLayer(1);
      foreach (var shir in l1.Values)
      {
        Console.WriteLine(shir.Item1);
        Console.WriteLine("\n");
        foreach (var kir in shir.Item2)
        {
          Console.WriteLine(kir);
        }
        Console.WriteLine("================");
        Console.WriteLine("\n\n");
      }

      foreach (var kvp in queryResult)
      {
        Console.WriteLine(kvp.Key);
        Console.WriteLine(kvp.Value.ToString());
        Console.WriteLine();
      }

      foreach (var kvp in queryResult)
      {
        //var jRes = JObject.Parse(kvp.Value);
        Console.WriteLine("Testing . . .");
        //Console.WriteLine(string.Join(",, ", jRes.Children().Select(r => r.First.ToString()).ToList()));
        Console.WriteLine(string.Join(",, ", ((JObject)kvp.Value).Children().Select(r => r.First.ToString()).ToList()));
      }



      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("\n\n======== PROJECT TEST\n");
      Console.ResetColor();

      var project = new BCFProject(acc, "TestProject2");
      Console.WriteLine(project.StreamId);
      project.branches.ToList().ForEach(branch => Console.WriteLine(branch.Key));
      project.projectParts.ToList().ForEach(part => Console.WriteLine(part.Key));
      Console.WriteLine(project.ValidateProject().Result);
      Console.WriteLine(string.Join("\n", project.validationErrors));


      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("\n\n======== SHEETS TEST\n");
      Console.ResetColor();

      var sheetsMan = new BCFSheets(acc, "mdf", 12);
      Console.WriteLine(sheetsMan.SheetsStream);
      Console.WriteLine(sheetsMan.SheetBranchQueue);
      Console.WriteLine(sheetsMan.SheetBranchDone);

      sheetsMan.ValidateSheets().Wait();
      sheetsMan.Sheets.Keys.ToList().ForEach(n => Console.WriteLine(n));


      Console.WriteLine("Press any key to exit . . .");
      Console.ReadKey();
    }
  }
}
