using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glashandel
{
    class Glass
    {
        private string name;
        private double glassCosts;
        private double cutCosts;
        private double discount;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public double GlassCosts
        {
            get { return glassCosts; }
            set { glassCosts = value; }
        }

        public double CutCosts
        {
            get { return cutCosts; }
            set { cutCosts = value; }
        }

        public double Discount
        {
            get { return discount; }
            set { discount = value; }
        }

        public Glass(string glassName, double glassGlassCosts, double glassCutCosts, double glassDiscount)
        {
            name = glassName;
            glassCosts = glassGlassCosts;
            cutCosts = glassCutCosts;
            discount = glassDiscount;
        }
    }
}
;