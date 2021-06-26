using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace RBB
{
    public class FishingEfficiencyOffsetPerBuilding
    {
        public ThingDef building;

        public float offset;

        public FishingEfficiencyOffsetPerBuilding()
        {
        }

        public FishingEfficiencyOffsetPerBuilding(ThingDef building, float offset)
        {
            this.building = building;
            this.offset = offset;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name != "li")
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "building", xmlRoot);
                offset = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
            }
            else
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "building", xmlRoot.InnerText);
                offset = -3.40282347E+38f;
            }
        }
    }

    public class CompProperties_FishingStatAffector : CompProperties_AffectedByFacilities
    {
        public List<FishingEfficiencyOffsetPerBuilding> defs = new List<FishingEfficiencyOffsetPerBuilding>();
        
        public SimpleCurve curve;

        [NoTranslate]
        public string explanationKey;

        [NoTranslate]
        public string explanationKeyAbstract;

        public CompProperties_FishingStatAffector()
        {
            this.compClass = typeof(CompFishingStatAffector);
        }
    }
}
