using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using SpeckleQuery;
using SpeckleQueryGH.Properties;

namespace ConnectorGrasshopperExtension.QueryComponents
{
  public class QueryResultComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the QueryResultComponent class.
    /// </summary>
    public QueryResultComponent()
      : base("Query Result", "QRes",
      "Expand the results form a Query.",
      "Speckle 2", "Query")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Query", "Q", "The Query Results.", GH_ParamAccess.item);
      pManager.AddIntegerParameter("Layer", "L", "The Layer Number.", GH_ParamAccess.item, 0);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Object ID", "OID", "List of Objects.", GH_ParamAccess.list);
      pManager.AddTextParameter("Fields", "FLD", "Objects' Fields.", GH_ParamAccess.tree);
      pManager.AddTextParameter("Children", "CHL", "Object's Children.", GH_ParamAccess.tree);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      QueryAgent agent = new QueryAgent();
      if (!DA.GetData(0, ref agent)) return;

      int layerNum = 0;
      if (!DA.GetData(1, ref layerNum)) return;

      var results = agent.GetLayer(layerNum);

      if (results == null)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Query has no Results.");
        return;
      }

      var objectIds = results.Keys.ToList();
      DA.SetDataList(0, objectIds);

      var fieldsTree = new GH_Structure<GH_String>();
      var childrenTree = new GH_Structure<GH_String>();
      foreach (var res in results.ToList())
      {
        var path = DA.ParameterTargetPath(0).AppendElement(objectIds.IndexOf(res.Key));

        var fieldsJson = (JObject)res.Value.Item1;

        var fieldValues = fieldsJson.Properties().Select(p => new GH_String((string)p.Value)).ToList();
        fieldsTree.AppendRange(fieldValues, path);

        var childrenJson = (JArray)res.Value.Item2;
        if (childrenJson == null) continue;

        var childrenValues = childrenJson
          .Select(childJson =>
          {
            var childData = childJson.SelectToken("$.result_data") ?? childJson.SelectToken("$.data");
            var childVals = ((JObject)childData).Properties()
              .Select(p => new GH_String(p.Value.ToString())).ToList();

            return childVals;
          }).ToList();

        childrenValues.ForEach(childVal => childrenTree.AppendRange(childVal, path.AppendElement(childrenValues.IndexOf(childVal))));
      }

      DA.SetDataTree(1, fieldsTree);
      DA.SetDataTree(2, childrenTree);
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        //You can add image files to your project resources and access them like this:
        // return Resources.IconForThisComponent;
        return Resources.QueryResult;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("333C88AC-4F3A-435C-AB63-C125CADF2EB0"); }
    }
  }
}
