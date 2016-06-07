using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public class Map : ITimerContext
    {
        public string Id { get; }
        public string TilesetId { get; private set; }
        public string MusicId { get; private set; }
        public string World { get; private set; }
        public Game Game { get; private set; }
        public int Floor { get; private set; } = MapData.NoFloor;
        public Rectangle Location { get; private set; }
        public Size Size => Location.Size;
        public int Width => Location.Width;
        public int Height => Location.Height;
        public MapEntities Entities { get; private set; }
        public Rectangle CameraPosition { get; }

        internal Tileset Tileset { get; private set; }
        internal bool HasWorld => !World.IsNullOrEmpty();
        internal bool IsLoaded { get; private set; }
        internal Surface VisibleSurface { get; private set; }
        internal string DestinationName { get; set; }
        internal bool IsStarted { get; private set; }
        internal int Width8 { get; private set; }
        internal int Height8 { get; private set; }
        internal bool IsSuspended { get; private set; }

        readonly Dictionary<EntityType, Action<EntityData>> _entityCreationMethods;

        Surface _backgroundSurface;
        Surface _foregroundSurface;

        public Map(string id)
        {
            Id = id;
            CameraPosition = new Rectangle(new Point(), Core.Video.ModSize);

            _entityCreationMethods = new Dictionary<EntityType, Action<EntityData>>()
             {
                { EntityType.Tile, CreateTile },
                { EntityType.Destination, CreateDestination },
                { EntityType.Destructible, CreateDestructible },
                { EntityType.Chest, CreateChest },
                { EntityType.Npc, CreateNpc },
                { EntityType.Block, CreateBlock },
                { EntityType.DynamicTile, CreateDynamicTile }
             };
        }

        internal Destination GetDestination()
        {
            if (DestinationName == "_same" || DestinationName.SafeSubstring(0, 5) == "_side")
                return null;

            Debug.CheckAssertion(IsLoaded, "This map is not loaded");

            Destination destination = null;
            if (!String.IsNullOrEmpty(DestinationName))
            {
                Entity entity = Entities.FindEntity(DestinationName);
                if (entity == null || entity.Type != EntityType.Destination)
                    Debug.Error("Map '{0}': No such destination: '{1}'".F(Id, DestinationName));
                else
                    destination = entity as Destination;
            }

            return destination ?? Entities.DefaultDestination;
        }

        internal void Load(Game game)
        {
            VisibleSurface = Surface.Create(Core.Video.ModSize, false);
            VisibleSurface.IsSoftwareDestination = false;

            _backgroundSurface = Surface.Create(Core.Video.ModSize, false);
            _backgroundSurface.IsSoftwareDestination = false;

            LoadMapData(game);

            BuildBackgroundSurface();
            BuildForegroundSurface();

            IsLoaded = true;
        }

        internal void Unload()
        {
            if (!IsLoaded)
                return;

            Tileset = null;
            VisibleSurface = null;
            _backgroundSurface = null;
            _foregroundSurface = null;
            Entities = null;

            IsLoaded = false;
        }

        // 맵 데이터 파일을 읽습니다.
        void LoadMapData(Game game)
        {
            string fileName = "maps/" + Id + ".xml";
            var data = XmlLoader.Load<MapData>(Core.Mod.ModFiles, fileName);

            // 읽어낸 데이터로 맵을 초기화합니다
            Game = game;
            Location = new Rectangle(data.Location, data.Size);
            Width8 = data.Size.Width / 8;
            Height8 = data.Size.Height / 8;
            MusicId = data.MusicId;
            World = data.World;
            Floor = data.Floor;
            TilesetId = data.TilesetId;
            Tileset = new Tileset(data.TilesetId);

            Entities = new MapEntities(game, this);

            // 엔티티들을 생성합니다
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
            {
                for (int i = 0; i < data.GetNumEntities((Layer)layer); ++i)
                {
                    var entityData = data.GetEntity(new EntityIndex((Layer)layer, i));
                    var type = entityData.Type;
                    if (!type.CanBeStoredInMapFile())
                        Debug.Error("Illegal entity type in map file: " + type);

                    _entityCreationMethods[type](entityData);
                }
            }
        }

        void BuildBackgroundSurface()
        {
            if (Tileset != null)
            {
                _backgroundSurface.Clear();
                _backgroundSurface.FillWithColor(Tileset.BackgroundColor);
            }
        }

        void BuildForegroundSurface()
        {
            _foregroundSurface = null;

            int screenWidth = VisibleSurface.Width;
            int screenHeight = VisibleSurface.Height;

            // 맵이 스크린 크기에 비해 작다면, 맵 외부에 검정 바를 위치시킵니다
            if (Width >= screenWidth && Height >= screenHeight)
                return; // 해당 사항이 없습니다

            _foregroundSurface = Surface.Create(VisibleSurface.Size, false);

            if (Width < screenWidth)
            {
                int barWidth = (screenWidth - Width) / 2;
                Rectangle dstPosition = new Rectangle(0, 0, barWidth, screenHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
                dstPosition.X = barWidth + Width;
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
            }

            if (Height < screenHeight)
            {
                int barHeight = (screenHeight - Height) / 2;
                Rectangle dstPosition = new Rectangle(0, 0, screenWidth, barHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
                dstPosition.Y = barHeight + Height;
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
            }
        }

        void DrawBackground()
        {
            _backgroundSurface.Draw(VisibleSurface);
        }

        void DrawForeground()
        {
            if (_foregroundSurface != null)
                _foregroundSurface.Draw(VisibleSurface);
        }

        internal void Draw()
        {
            if (IsLoaded)
            {
                DrawBackground();
                Entities.Draw();
                DrawForeground();
            }
        }

        public void DrawSprite(Sprite sprite, Point xy)
        {
            DrawSprite(sprite, xy.X, xy.Y);
        }

        public void DrawSprite(Sprite sprite, int x, int y)
        {
            sprite.Draw(VisibleSurface, x - CameraPosition.X, y - CameraPosition.Y);
        }

        internal void Update()
        {
            CheckSuspended();

            TilePattern.Update();
            Entities.Update();
        }

        public void CheckSuspended()
        {
            bool gameSuspended = Game.IsSuspended;
            if (IsSuspended != gameSuspended)
                SetSuspended(gameSuspended);
        }

        void SetSuspended(bool suspended)
        {
            IsSuspended = suspended;
            Entities.SetSuspended(suspended);
        }

        public void Start()
        {
            IsStarted = true;
            VisibleSurface.SetOpacity(255);

            Core.Audio?.PlayMusic(MusicId);
            Entities.NotifyMapStarted();
            OnStarted(GetDestination());
        }

        protected virtual void OnStarted(Destination destination) { }

        internal int GetDestinationSide()
        {
            if (DestinationName.SafeSubstring(0, 5) == "_side")
            {
                int destinationSide = DestinationName[5] - '0';
                return destinationSide;
            }
            return -1;
        }

        internal bool TestCollisionWithBorder(int x, int y)
        {
            return (x < 0 || y < 0 || x >= Location.Width || y >= Location.Height);
        }

        internal bool TestCollisionWithBorder(Point point)
        {
            return TestCollisionWithBorder(point.X, point.Y);
        }

        // 특정 포인트가 맵의 그라운드와 충돌하는지를 체크합니다.
        // 그라운드는 타일 혹은 타일을 변화시키는 것들(예를 들면, 다이나믹 타일이나 파괴 가능한 아이템)입니다.
        // 포인트가 맵 외부일 경우에도 true를 반환합니다.
        internal bool TestCollisionWithGround(Layer layer, int x, int y, Entity entityToCheck, ref bool foundDiagonalWall)
        {
            bool onObstacle = false;
            int xInTile = 0, yInTile = 0;

            if (TestCollisionWithBorder(x, y))
                return true;

            // 포인트 위치에서의 그라운드 속성을 얻습니다
            Ground ground = GetGround(layer, x, y);

            switch (ground)
            {
                case Ground.Empty:
                case Ground.Traversable:
                case Ground.Grass:
                case Ground.Ice:
                    onObstacle = false;
                    break;

                case Ground.Wall:
                    onObstacle = true;
                    break;
                    
                case Ground.WallTopRight:
                case Ground.WallTopRightWater:
                    // 상단 우측이 장애물로, 이 안에서 다시 체크해야 합니다
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile <= xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.WallTopLeft:
                case Ground.WallTopLeftWater:
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile <= 7 - xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.WallBottomLeft:
                case Ground.WallBottomLeftWater:
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile >=  xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.WallBottomRight:
                case Ground.WallBottomRightWater:
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile >= 7 - xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.LowWall:
                    onObstacle = entityToCheck.IsLowWallObstacle;
                    break;

                case Ground.ShallowWater:
                    onObstacle = entityToCheck.IsShallowWaterObstacle;
                    break;

                case Ground.DeepWater:
                    onObstacle = entityToCheck.IsDeepWaterObstacle;
                    break;

                case Ground.Hole:
                    onObstacle = entityToCheck.IsHoleObstacle;
                    break;

                case Ground.Lava:
                    onObstacle = entityToCheck.IsLavaObstacle;
                    break;

                case Ground.Prickle:
                    onObstacle = entityToCheck.IsPrickleObstacle;
                    break;

                case Ground.Ladder:
                    onObstacle = entityToCheck.IsLadderObstacle;
                    break;
            }

            return onObstacle;
        }

        internal bool TestCollisionWithObstacles(Layer layer, Rectangle collisionBox, Entity entityToCheck)
        {
            // 이 함수는 매우 자주 불리우며, 성능상의 이유로 충돌 박스의 경계선만을 체크합니다.

            // 터레인과의 퉁돌 (에를 들어 타일 혹은 타일을 변화시키는 동적 엔티티)
            int x1 = collisionBox.X;
            int x2 = x1 + collisionBox.Width - 1;
            int y1 = collisionBox.Y;
            int y2 = y1 + collisionBox.Height - 1;

            // 사각형 꼭지점들만을 먼저 체크합니다. 
            // 충돌 박스가 적어도 8x8 픽셀이기 때문에, 대칭 타입을 제외한 모든 터레인은 이 체크만으로도 충분합니다.
            bool foundDiagonalWall = false;
            for (int x = x1; x <= x2; x += 8)
            {
                if (TestCollisionWithGround(layer, x, y1, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x, y2, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x + 7, y1, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x + 7, y2, entityToCheck, ref foundDiagonalWall))
                {
                    return true;
                }
            }

            for (int y = y1; y <= y2; y += 8)
                if (TestCollisionWithGround(layer, x1, y, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x2, y, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x1, y + 7, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x1, y + 7, entityToCheck, ref foundDiagonalWall))
                    return true;

            // 완전한 체크가 필요합니다
            // 꼭지점들만 체크할 경우 'V'와 같이 각도가 큰 벽의 경우 통과가 가능하게 때문에, 충돌 박스의 경계선 모두를 체크해야 합니다.
            if (foundDiagonalWall)
            {
                for (int x = x1; x <= x2; ++x)
                {
                    if (TestCollisionWithGround(layer, x, y1, entityToCheck, ref foundDiagonalWall) ||
                        TestCollisionWithGround(layer, x, y2, entityToCheck, ref foundDiagonalWall))
                    {
                        return true;
                    }
                }

                for (int y = y1; y <= y2; ++y)
                {
                    if (TestCollisionWithGround(layer, x1, y, entityToCheck, ref foundDiagonalWall) ||
                        TestCollisionWithGround(layer, x2, y, entityToCheck, ref foundDiagonalWall))
                    {
                        return true;
                    }
                }
            }

            // 터레인과는 충돌이 없습니다. 다이나믹 엔티티들과의 충돌을 검사합니다.
            return TestCollisionWithEntities(layer, collisionBox, entityToCheck);
        }

        internal bool TestCollisionWithEntities(Layer layer, Rectangle collisionBox, Entity entityToCheck)
        {
            var obstacleEntities = Entities.GetObstacleEntities(layer);
            foreach (var entity in obstacleEntities)
            {
                if (entity != entityToCheck &&
                    entity.IsEnabled &&
                    entity.Overlaps(collisionBox) &&
                    entity.IsObstacleFor(entityToCheck, collisionBox))
                {
                    return true;
                }
            }
            
            return false;
        }

        internal Ground GetGround(Layer layer, int x, int y)
        {
            var groundModifiers = Entities.GetGroundModifiers(layer);
            foreach (var groundModifier in groundModifiers)
            {
                if (groundModifier.IsEnabled &&
                    !groundModifier.IsBeingRemoved &&
                    groundModifier.Overlaps(x, y) &&
                    groundModifier.ModifiedGround != Ground.Empty)
                {
                    return groundModifier.ModifiedGround;
                }
            }
            
            return Entities.GetTileGround(layer, x, y);
        }

        internal void CheckCollisionWithDetectors(Entity entity)
        {
            if (IsSuspended)
                return;

            foreach (var detector in Entities.Detectors.Where(d => d.IsEnabled && !d.IsBeingRemoved))
                detector.CheckCollision(entity);
        }

        internal void CheckCollisionWithDetectors(Entity entity, Sprite sprite)
        {
            if (IsSuspended)
                return;

            foreach (var detector in Entities.Detectors.Where(d => d.IsEnabled && !d.IsBeingRemoved))
                detector.CheckCollision(entity, sprite);
        }

        internal void CheckCollisionFromDetector(Detector detector)
        {
            if (IsSuspended)
                return;

            detector.CheckCollision(Entities.Hero);

            foreach (var entity in Entities.Entities.Where(e => e.IsEnabled && !e.IsBeingRemoved))
                detector.CheckCollision(entity);
        }

        public void CreateTile(EntityData entityData)
        {
            var data = entityData as TileData;
            var pattern = Tileset.GetTilePattern(data.Pattern);

            var size = EntityCreationCheckSize(data.Width, data.Height);
            for (int y = data.XY.Y; y < data.XY.Y + size.Height; y += pattern.Height)
            {
                for (int x = data.XY.X; x < data.XY.X + size.Width; x += pattern.Width)
                {
                    var tile = new Tile(data.Layer, new Point(x, y), pattern.Size, Tileset, data.Pattern);
                    Entities.AddEntity(tile);
                }
            }
        }

        static Size EntityCreationCheckSize(int width, int height)
        {
            if (width < 0 || width % 8 != 0)
                throw new Exception("Invalid width {0}: should be a positive multiple of 8".F(width));

            if (height < 0 || height % 8 != 0)
                throw new Exception("Invalid height {0}: should be a positive multiple of 8".F(height));

            return new Size(width, height);
        }

        public void CreateDestination(EntityData entityData)
        {
            var data = entityData as DestinationData;
            var destination = new Destination(data.Name, data.Layer, data.XY, data.Direction, data.Sprite, data.Default);
            Entities.AddEntity(destination);
        }

        public void CreateDestructible(EntityData entityData)
        {
            var data = entityData as DestructibleData;
            var destructible = new Destructible(
                data.Name,
                data.Layer,
                data.XY,
                data.Sprite,
                new Treasure(Game, data.TreasureName, data.TreasureVariant, data.TreasureSavegameVariable),
                data.Ground);
            destructible.DestructionSound = data.DestructionSound;
            destructible.Weight = data.Weight;
            destructible.CanBeCut = data.CanBeCut;
            destructible.CanExplode = data.CanExplode;
            destructible.CanRegenerate = data.CanRegenerate;
            destructible.DamageOnEnemies = data.DamageOnEnemies;
            Entities.AddEntity(destructible);
        }

        public void CreateChest(EntityData entityData)
        {
            var data = entityData as ChestData;
            if (data.OpeningMethod == ChestOpeningMethod.ByInteractionIfItem)
            {
                if (!data.OpeningCondition.IsNullOrEmpty() ||
                    !Game.Equipment.ItemExists(data.OpeningCondition))
                {
                    string msg = "Bad field 'OpeningCondition' (no such equipement item: '{0}'".F(data.OpeningCondition);
                    throw new Exception(msg);
                }
                var item = Game.Equipment.GetItem(data.OpeningCondition);
                if (!item.IsSaved)
                {
                    string msg = "Bad field 'OpeneingCondition' (equipment item '{0}' is not saved".F(data.OpeningCondition);
                    throw new Exception(msg);
                }
            }

            var chest = new Chest(
                data.Name,
                data.Layer,
                data.XY,
                data.Sprite,
                new Treasure(Game, data.TreasureName, data.TreasureVariant, data.TreasureSavegameVariable));
            chest.OpeningMethod = data.OpeningMethod;
            chest.OpeningCondition = data.OpeningCondition;
            chest.OpeningConditionConsumed = data.OpeningConditionConsumed;
            chest.CannotOpenDialogId = data.CannotOpenDialog;
            Entities.AddEntity(chest);
        }

        public void CreateNpc(EntityData entityData)
        {
            var data = entityData as NpcData;
            var npc = new Npc(
                Game,
                data.Name,
                data.Layer,
                data.XY,
                data.Subtype,
                data.Sprite,
                data.Direction,
                data.Behavior);
            Entities.AddEntity(npc);
        }

        public void CreateBlock(EntityData entityData)
        {
            var data = entityData as BlockData;
            if (data.MaximumMoves < 0 || data.MaximumMoves > 2)
                throw new Exception("Invalid MaximumMoves: {0}".F(data.MaximumMoves));

            var block = new Block(
                data.Name,
                data.Layer,
                data.XY,
                data.Direction,
                data.Sprite,
                data.Pushable,
                data.Pullable,
                data.MaximumMoves);
            Entities.AddEntity(block);
        }

        public void CreateDynamicTile(EntityData entityData)
        {
            var data = entityData as DynamicTileData;

            var dynamicTile = new DynamicTile(
                data.Name,
                data.Layer,
                data.XY,
                EntityCreationCheckSize(data.Width, data.Height),
                Tileset,
                data.Pattern,
                data.EnabledAtStart);
            Entities.AddEntity(dynamicTile);
        }

        public void CreateBomb(EntityData entityData)
        {
            var data = entityData as BombData;

            var bomb = new Bomb(data.Name, data.Layer, data.XY);
            Entities.AddEntity(bomb);
        }

        public T GetEntity<T>(string name) where T : Entity
        {
            var entity = Entities.FindEntity(name);
            if (entity?.IsBeingRemoved == true)
                return null;

            return entity as T;
        }
    }
}
