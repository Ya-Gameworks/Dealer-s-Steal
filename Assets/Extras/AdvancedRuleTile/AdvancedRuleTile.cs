using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[CreateAssetMenu(menuName = "2D/Tiles/Advanced Rule Tile")]
public class AdvancedRuleTile : RuleTile<AdvancedRuleTile.Neighbor> {
    [Header("Advanced Tile")]
    [Tooltip("If enabled, the tile will connect to these tiles too when the mode is set to \"This\"")]
    public bool AlwaysConnect;
    [Tooltip("Tiles to connect to")]
    public TileBase[] TilesToConnect;
    [Space]
    [Tooltip("Check itseft when the mode is set to \"any\"")]
    public bool CheckSelf = true;
    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Any = 3;
        public const int Specified = 4;
        public const int UnSpecified = 5;
        public const int Nothing = 6;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.This: return CheckThis(tile);
            case Neighbor.NotThis: return CheckNotThis(tile);
            case Neighbor.Any: return CheckAny(tile);
            case Neighbor.Specified: return CheckSpecified(tile);
            case Neighbor.UnSpecified: return CheckUnSpecified(tile);
            case Neighbor.Nothing: return CheckNothing(tile);
        }
        return base.RuleMatch(neighbor, tile);
    }

    /// <summary>
    /// Returns true if the tile is this, or if the tile is one of the tiles specified if always connect is enabled.
    /// </summary>
    /// <param name="tile">Neighboring tile to compare to</param>
    /// <returns></returns>
    bool CheckThis(TileBase tile)
    {
        if (!AlwaysConnect) return tile == this;
        else return TilesToConnect.Contains(tile) || tile == this;

        //.Contains requires "using System.Linq;"
    }

    /// <summary>
    /// Returns true if the tile is not this.
    /// </summary>
    /// <param name="tile">Neighboring tile to compare to</param>
    /// <returns></returns>
    bool CheckNotThis(TileBase tile)
    {
        if (!AlwaysConnect) return tile != this;
        else return !TilesToConnect.Contains(tile) && tile != this;

        //.Contains requires "using System.Linq;"
    }

    /// <summary>
    /// Return true if the tile is not empty, or not this if the check self option is disabled.
    /// </summary>
    /// <param name="tile">Neighboring tile to compare to</param>
    /// <returns></returns>
    bool CheckAny(TileBase tile)
    {
        if (CheckSelf) return tile is not null;
        else return tile is not null && tile != this;
    }

    /// <summary>
    /// Returns true if the tile is one of the specified tiles.
    /// </summary>
    /// <param name="tile">Neighboring tile to compare to</param>
    /// <returns></returns>
    bool CheckSpecified(TileBase tile)
    {
        return TilesToConnect.Contains(tile);

        //.Contains requires "using System.Linq;"
    }
    bool CheckUnSpecified(TileBase tile)
    {
        return !TilesToConnect.Contains(tile);

        //.Contains requires "using System.Linq;"
    }

    /// <summary>
    /// Returns true if the tile is empty.
    /// </summary>
    /// <param name="tile">Neighboring tile to compare to</param>
    /// <param name="tile"></param>
    /// <returns></returns>
    bool CheckNothing(TileBase tile)
    {
        return tile is null;
    }
}