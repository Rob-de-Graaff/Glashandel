using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glashandel
{
    public partial class Form1 : Form
    {
        private Glass _newGlass1, _newGlass2;
        private Dictionary<string, Glass> glassDict;
        private List<string> orderList;
        private Dictionary<int, double> remainderRegDict;
        private Dictionary<int, double> remainderSpecDict;
        private double _priceTotal;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            glassDict = new Dictionary<string, Glass>();
            orderList = new List<string>();
            remainderRegDict = new Dictionary<int, double>();
            remainderSpecDict = new Dictionary<int, double>();

            _newGlass1 = new Glass("Gewoon glas", 30, 10, 0.95);
            _newGlass2 = new Glass("Speciaal glas", 55, 25, 0.95);

            glassDict.Add(_newGlass1.Name, _newGlass1);
            glassDict.Add(_newGlass2.Name, _newGlass2);

            // Displays name property
            listBoxGlass.DataSource = new BindingSource(glassDict, null);
            listBoxGlass.DisplayMember = "Key";

            // Displays calculation, total
            labelTicketsTotal.Text = $@"(glaskosten * opp + snijkosten * #besteling) * korting";
            labelPriceTotal.Text = $@"Totaal: € {Math.Round(_priceTotal, 2)},-";

            // tests
            remainderRegDict.Add(0, 0.3);
            remainderRegDict.Add(1, 0.1);
            remainderRegDict.Add(2, 0.5);
        }

        private void ButtonAddOrder_Click(object sender, EventArgs e)
        {
            string glassName;
            if (double.TryParse(textBoxSurface.Text, out double resultSurface) && resultSurface > 0)
            {
                glassName = listBoxGlass.SelectedItem.ToString();
                int indexStart = glassName.IndexOf("[") + 1;
                int indexEnd = glassName.IndexOf(",");
                int lenght = indexEnd - indexStart;
                glassName = glassName.Substring(indexStart, lenght);
                
                orderList.Add(glassName + ":" + resultSurface);

                // Displays name property
                listBoxOrders.DataSource = null;
                listBoxOrders.DataSource = orderList;
            }
            else
            {
                MessageBox.Show($@"Surface must contain only numbers > 0");
            }
        }

        private void ButtonCalculate_Click(object sender, EventArgs e)
        {
            double regularGlassTotal = 0;
            double specialGlassTotal = 0;
            int regularGlassCounter = 0;
            int specialGlassCounter = 0;
            double remainingSurface = 0;
            double surfaceRegular = 0;
            double surfaceSpecial = 0;
            double glassCostsReg = 0;
            double glassCostsSpec = 0;
            double cutCostsReg = 0;
            double cutCostsSpec = 0;
            double glassDiscount = 1;
            double surfaceRegularTotal = 0;
            double surfaceSpecialTotal = 0;
            string rest = "";

            if (listBoxOrders != null)
            {
                foreach (string order in orderList.OrderByDescending(entry => entry))
                {
                    int indexStart = 0;
                    int indexEnd = order.IndexOf(":");
                    int lenght = indexEnd - indexStart;
                    string orderName = order.Substring(indexStart, lenght);
                    indexStart = order.IndexOf(":")+1;
                    indexEnd = order.Length;
                    lenght = indexEnd - indexStart;
                    double orderSurface = double.Parse(order.Substring(indexStart, lenght));

                    switch (orderName)
                    {
                        case "Gewoon glas":
                            #region exception fix

                            // Adds a value to the dictionary to prevent System.InvalidOperationException: 'Sequence contains no elements'
                            //if (remainderRegDict.Count == 0)
                            //{
                            //    remainderRegDict.Add(0, 0.0);
                            //}

                            #endregion

                            // Checks if new glass, or remainder glass must be cut
                            if (orderSurface > remainderRegDict.Values.DefaultIfEmpty().Max())
                            {
                                remainingSurface = CalculateRemainder(orderSurface, remainderRegDict,"new glass");
                            }
                            else if (remainderRegDict.ContainsValue(orderSurface))
                            {
                                remainingSurface = CalculateRemainder(orderSurface, remainderRegDict,"remaining glass full");
                            }
                            else
                            {
                                remainingSurface = CalculateRemainder(orderSurface, remainderRegDict,"remaining glass");
                                rest = "R";
                            }

                            //remainderCostsReg.Add(remainingSurface);
                            regularGlassCounter++;
                            surfaceRegular = remainingSurface;
                            surfaceRegularTotal += surfaceRegular;
                            glassCostsReg = _newGlass1.GlassCosts;
                            cutCostsReg = _newGlass1.CutCosts;

                            regularGlassTotal += glassCostsReg * surfaceRegular;
                            break;
                        case "Speciaal glas":
                            #region exception fix

                            // Adds a value to the dictionary to prevent InvalidOperationException: 'Collection was modified; enumeration operation may not execute.'
                            //if (remainderSpecDict.Count == 0)
                            //{
                            //    remainderSpecDict.Add(0, 0.0);
                            //}

                            #endregion

                            // Checks if new glass, or remainder glass must be cut
                            if (orderSurface > remainderSpecDict.Values.DefaultIfEmpty().Max())
                            {
                                remainingSurface = CalculateRemainder(orderSurface, remainderSpecDict, "new glass");
                            }
                            else if (remainderSpecDict.ContainsValue(orderSurface))
                            {
                                remainingSurface = CalculateRemainder(orderSurface, remainderSpecDict, "remaining glass full");
                            }
                            else
                            {
                                remainingSurface = CalculateRemainder(orderSurface, remainderSpecDict, "remaining glass");
                            }

                            specialGlassCounter++;
                            surfaceSpecial = remainingSurface;
                            surfaceSpecialTotal += surfaceSpecial;
                            glassCostsSpec = _newGlass2.GlassCosts;
                            cutCostsSpec = _newGlass2.CutCosts;

                            specialGlassTotal += glassCostsSpec * surfaceSpecial;
                            break;
                    }
                }

                // Checks if total glass cocsts >= 145 cutcosts = 0
                // Checks if total glass costs > 250 
                if (regularGlassTotal + specialGlassTotal >= 145)
                {
                    cutCostsReg = 0;
                    cutCostsSpec = 0;
                }
                
                if (regularGlassTotal + specialGlassTotal > 250)
                {
                    glassDiscount = _newGlass1.Discount;
                }

                regularGlassTotal += cutCostsReg * regularGlassCounter;
                specialGlassTotal += cutCostsSpec * specialGlassCounter;

                _priceTotal += (regularGlassTotal + specialGlassTotal) * glassDiscount;
                
                //Total+= (glaskosten*opp+snijkosten*#besteling + glaskosten*opp+snijkosten*#besteling)*korting
                labelTicketsTotal.Text = $@"(glaskosten € {glassCostsReg + glassCostsSpec} * opp {surfaceRegularTotal + surfaceSpecialTotal} {rest} + snijkosten € {cutCostsReg + cutCostsSpec} * #besteling {regularGlassCounter + specialGlassCounter}) * korting {(1-glassDiscount)*100}%";
                labelPriceTotal.Text = $@"Totaal: € {Math.Round(_priceTotal, 2):0.00},-";

                //Resets display and variables
                //textBoxSurface.Text = "0";
                //listBoxOrders.DataSource = null;
                orderList.Clear();
                _priceTotal = 0;
            }
            else
            {
                MessageBox.Show("You must have ordered glass.");
            }
        }

        private double CalculateRemainder(double order, Dictionary<int, double> dict, string state)
        {
            double remainder=0;
            Double ceil = 0;
            int dictKey = 0;

            switch (state)
            {
                case "new glass":
                    // Calculates the remaining glass
                    ceil = Math.Ceiling(order);
                    remainder = ceil - order;

                    // Adds remaining glass to dictionary
                    dictKey = dict.Count;

                    // Checks if value is not 0
                    if (remainder != 0)
                    {
                        dict.Add(dictKey, remainder);
                    }

                    // Rounds order up because of cost calculations and sets it as remainder
                    remainder = ceil;
                    break;
                case "remaining glass full":
                    dict.Remove(dict.FirstOrDefault(entry => entry.Value == order).Key);

                    // Sets remainder equal to order
                    remainder = order;
                    break;
                case "remaining glass":
                    // Checks which value can be subtracted
                    // Updates value 
                    foreach (KeyValuePair<int, double> entry in dict.OrderBy(entry => entry.Value))
                    {
                        remainder = entry.Value - order;
                        if (remainder > 0)
                        {
                            dict[entry.Key] = remainder;
                            break;
                        }
                    }
                    remainder = order;
                    break;
            }

            // removes each 0 value
            foreach (KeyValuePair<int, double> entry in dict.Where(entry => entry.Value == 0))
            {
                dict.Remove(entry.Key);
            }
            
            return remainder;
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            textBoxSurface.Text = "0";
            listBoxOrders.DataSource = null;
            orderList.Clear();
            _priceTotal = 0;
        }
    }
}
