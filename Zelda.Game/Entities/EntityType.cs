
namespace Zelda.Game.Entities
{
    public enum EntityType
    {
        // 맵 파일에서 정의되는 것들
        Tile,
        Destination,
        Teletransporter,
        Pickable,
        Destructible,
        Chest,
        Jumper,
        Enemy,
        Npc,
        Block,
        DynamicTile,
        Switch,
        Wall,
        Sensor,
        Crystal,
        CrystalBlock,
        ShopTreasure,
        Stream,
        Door,
        Stairs,
        Separator,
        Custom,

        // 런타임에 게임에서 생성되고 맵 파일에는 저장되지 않는 것들
        Hero,
        CarriedItem,
        Boomerang,
        Explosion,
        Arrow,
        Bomb,
        Fire,
        Hookshot
    }
}
