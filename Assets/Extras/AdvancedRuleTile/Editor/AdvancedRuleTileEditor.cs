
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    [CustomEditor(typeof(AdvancedRuleTile))]
    [CanEditMultipleObjects]

    public class AdvancedRuleTileEditor : RuleTileEditor
    {
        public Texture2D AnyIcon;
        public Texture2D SpecifiedIcon;
        public Texture2D UnSpecifiedIcon;
        public Texture2D NothingIcon;

        public override void RuleOnGUI(Rect rect, Vector3Int position, int neighbor)
        {
            switch (neighbor)
            {
                case AdvancedRuleTile.Neighbor.Any:
                    GUI.DrawTexture(rect, AnyIcon);
                    return;
                case AdvancedRuleTile.Neighbor.Specified:
                    GUI.DrawTexture(rect, SpecifiedIcon);
                    return;
                case AdvancedRuleTile.Neighbor.Nothing:
                    GUI.DrawTexture(rect, NothingIcon);
                    return;
                case AdvancedRuleTile.Neighbor.UnSpecified:
                    GUI.DrawTexture(rect, UnSpecifiedIcon);
                    return;
            }

            base.RuleOnGUI(rect, position, neighbor);
        }
    }
}
#endif