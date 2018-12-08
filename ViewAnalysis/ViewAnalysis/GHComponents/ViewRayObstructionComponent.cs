using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using ViewAnalysis.Properties;

namespace ViewAnalysis.GHComponents
{
    public class ViewRayObstructionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewRayObstructionComponent class.
        /// </summary>
        public ViewRayObstructionComponent()
          : base("ViewRayObstruction", "ViewRayObstruction",
              "",
               "KPF UI", "ViewAnalysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("SamplePoint", "p", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("ViewRays", "V", "Mesh normals are expected.", GH_ParamAccess.list);
            pManager.AddMeshParameter("ObstacleMesh", "m", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxDistance", "dMax", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("ClearDistance", "D", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get inputs
            Point3d p = new Point3d();
            List<Vector3d> dirs = new List<Vector3d>();
            Mesh mesh = new Mesh();
            double dMax = 0;
            if (!DA.GetDataList("ViewRays", dirs)) return;
            if (!DA.GetData("SamplePoint", ref p)) return;
            if (!DA.GetData("ObstacleMesh", ref mesh)) return;
            if (!DA.GetData("MaxDistance", ref dMax)) return;
            int n = dirs.Count;

            // Check inputs
            if (!p.IsValid)
                throw new ArgumentException("No valid sample point.");
            if (n == 0)
                throw new ArgumentException("No valid view rays.");
            if (!mesh.IsValid)
                throw new ArgumentException("No valid mesh obstacles.");
            if (dMax <= 0)
                throw new ArgumentOutOfRangeException("dMax should be greater than 0.");

            var dists = new List<double>();

            // MeshRay cannot avoid intersecting mesh vertex; move ray point to avoid it
            if (n > 0)
            {
                var nudge = new Vector3d(dirs[0]);
                double amount = 0.001;
                nudge.Unitize();
                nudge *= amount;
                p += nudge;
            }

            for (int i = 0; i < n; i++)
            {
                var dir = dirs[i];
                var ray = new Ray3d(p, dir);
                var x = Intersection.MeshRay(mesh, ray);
                var d = dMax;
                if (x >= 0)
                {
                    var pInter = ray.PointAt(x);
                    d = pInter.DistanceTo(p);
                }
                dists.Add(d);
            }
            DA.SetDataList(0, dists);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.Icon_ViewRayObstruction; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c160e285-e389-4ac5-b23a-143c238f5b4b"); }
        }
    }
}