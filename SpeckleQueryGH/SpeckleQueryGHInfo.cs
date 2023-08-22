using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SpeckleQueryGH
{
    public class SpeckleQueryGHInfo : GH_AssemblyInfo
    {
        public override string Name => "Speckle Query";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "A plugin for running GraphQL queries on Speckle Objects.";

        public override Guid Id => new Guid("37b759ee-9ae7-459b-a880-7a7dfe4d8672");

        //Return a string identifying you or your company.
        public override string AuthorName => "Ardeshir Talaei";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "ardeshir.talaei@iaac.net";
    }
}