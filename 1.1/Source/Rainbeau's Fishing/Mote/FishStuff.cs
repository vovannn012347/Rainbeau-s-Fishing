using UnityEngine;
using Verse;

namespace RBB
{

    public class MoteDualAttached_FishingRod : Mote
    {
        protected MoteAttachLink link2 = MoteAttachLink.Invalid;

        public void Attach(TargetInfo a, TargetInfo b)
        {
            link1 = new MoteAttachLink(a);
            link2 = new MoteAttachLink(b);
        }

        public override void Draw()
        {
            UpdatePositionAndRotation();
            base.Draw();
        }

        protected void UpdatePositionAndRotation()
        {
            if (link1.Linked)
            {
                if (link2.Linked)
                {
                    if (!link1.Target.ThingDestroyed)
                    {
                        link1.UpdateDrawPos();
                    }
                    if (!link2.Target.ThingDestroyed)
                    {
                        link2.UpdateDrawPos();
                    }
                    exactPosition = (link1.LastDrawPos + link2.LastDrawPos) * 0.5f;
                    if (def.mote.rotateTowardsTarget)
                    {
                        exactRotation = link1.LastDrawPos.AngleToFlat(link2.LastDrawPos);
                        this.Rotation = RotFromAngleBiased(exactRotation);
                        exactRotation += 90f;
                    }
                    if (def.mote.scaleToConnectTargets)
                    {
                        exactScale = new Vector3(def.graphicData.drawSize.y, 1f, (link2.LastDrawPos - link1.LastDrawPos).MagnitudeHorizontal());
                    }
                }
                else
                {
                    if (!link1.Target.ThingDestroyed)
                    {
                        link1.UpdateDrawPos();
                    }
                    exactPosition = link1.LastDrawPos + def.mote.attachedDrawOffset;
                }
            }
            exactPosition.y = def.altitudeLayer.AltitudeFor();
        }

        public static Rot4 RotFromAngleBiased(float angle)
        {
            if (angle < 30f)
            {
                return Rot4.North;
            }
            if (angle < 150f)
            {
                return Rot4.East;
            }
            if (angle < 210f)
            {
                return Rot4.South;
            }
            if (angle < 330f)
            {
                return Rot4.West;
            }
            return Rot4.North;
        }
    }

}
