using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RBB
{
    public class RecipeWorkerCounter_IngridientResources : RecipeWorkerCounter
    {
        public override bool CanCountProducts(Bill_Production bill)
        {
            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            int num = 0;
            List<ThingDef> childThingDefs = bill.ingredientFilter.AllowedThingDefs.ToList();
            for (int i = 0; i < childThingDefs.Count; i++)
            {
                num += bill.Map.resourceCounter.GetCount(childThingDefs[i]);
            }
            return num;
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return ThingCategoryDefOf.ResourcesRaw.label;
        }

        public override bool CanPossiblyStoreInStockpile(Bill_Production bill, Zone_Stockpile stockpile)
        {
            foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
            {
                if (!allowedThingDef.butcherProducts.NullOrEmpty())
                {
                    if (!stockpile.GetStoreSettings().AllowedToAccept(allowedThingDef))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
