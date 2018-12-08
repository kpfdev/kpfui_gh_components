using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using ViewAnalysis.Properties;

namespace ViewAnalysis
{
    public class VectorBunchComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VectorBunchComponent()
          : base("VectorBunch", "VectorBunch",
              "",
              "KPF UI", "ViewAnalysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("NormalVectors", "V", "Mesh normals are expected.", GH_ParamAccess.list);
            pManager.AddNumberParameter("angle1", "a1", "the angle (degrees) between the normal vector to the first ring of view rays",
                GH_ParamAccess.item);
            pManager.AddIntegerParameter("n1", "n1", "number of rings of view rays", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n2", "n2", "number of divisions in each ring of view rays", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("VBunch", "VBunch", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get inputs
            List<Vector3d> normals = new List<Vector3d>();
            double a1 = 0;
            int n1 = 1;
            int n2 = 1;
            if (!DA.GetDataList("NormalVectors", normals)) return;
            if (!DA.GetData("angle1", ref a1)) return;
            if (!DA.GetData("n1", ref n1)) return;
            if (!DA.GetData("n2", ref n2)) return;
            int n = normals.Count;

            // Check inputs
            if (n == 0)
                throw new ArgumentException("No valid normal vectors.");
            if (a1 <= 0)
                throw new ArgumentOutOfRangeException("a1 should be greater than 0.");
            if (n1 <= 0)
                throw new ArgumentOutOfRangeException("n1 should be greater than 0.");
            if (n2 <= 0)
                throw new ArgumentOutOfRangeException("n2 should be greater than 0.");

            a1 = a1 * Math.PI / 180;
            double a2 = Math.PI * 2 / n2;
            DataTree<Vector3d> tree = new DataTree<Vector3d>();
            for (int i = 0; i < normals.Count; i++)
            {
                var v = normals[i];
                var u = GetPerpVector(v);
                List<Vector3d> group1 = new List<Vector3d>() { v };
                for (int j = 0; j < n1 - 1; j++)
                {
                    var a = (j + 1) * a1;
                    var w0 = new Vector3d(v);
                    w0.Rotate(a, u);
                    for (int k = 0; k < n2; k++)
                    {
                        var b = k * a2;
                        var w = new Vector3d(w0);
                        w.Rotate(b, v);
                        group1.Add(w);
                    }
                }
                var path = new GH_Path(i);
                tree.AddRange(group1, path);
            }
            DA.SetDataTree(0, tree);
        }

        private Vector3d GetPerpVector(Vector3d v)
        {
            // x*x0 + y*y0 + z*z0 = 0
            double error = 0.000001;
            double x0 = v.X;
            double y0 = v.Y;
            double z0 = v.Z;
            double x = 0, y = 0, z = 0;
            if (Math.Abs(x0) < error && Math.Abs(y0) < error)
            {
                x = 1;
            }
            else
            {
                z = 1;
                if (Math.Abs(x0) < Math.Abs(y0))
                {
                    y = -z * z0 / y0;
                    x = 0;
                }
                else
                {
                    x = -z * z0 / x0;
                    y = 0;
                }
            }
            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.Icon_VectorBunch; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("501a95c8-6d48-4a10-a651-9f1d40efbe68"); }
        }
    }
}
