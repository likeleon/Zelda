-- 맵을 XML로 변환합니다

local converter = {}

local layer_names = {
  "Low", "Intermediate", "Hight", "NumLayers"
}

local report = require("report")
require("LuaXml")

local function import_map(quest_path, map_id)
  print("Importing map '" .. map_id .. "'")

  local map = {}
  map.properties = {}
  map.tiles = {}
  map.destinations = {}
  map.destructibles = {}

  local env = {}
  function env.properties(properties)
    map.properties = properties
  end
  function env.tile(tile)
    map.tiles[#map.tiles + 1] = tile
  end
  function env.destination(destination)
    map.destinations[#map.destinations + 1] = destination
  end
  function env.chest(chest)
    -- TODO
  end
  function env.destructible(destructible)
    map.destructibles[#map.destructibles + 1] = destructible
  end
  function env.npc(npc)
    -- TODO
  end
  function env.block(block)
    -- TODO
  end

  local file = quest_path .. "maps/" .. map_id .. ".dat"
  local chunk, error = loadfile(file)
  if chunk == nil then
    report.error("Error in map '" .. map_id .. "': " .. error)
  else
    setfenv(chunk, env)
    local success, error = pcall(chunk)

    if not success then
      report.error("Error in map '" .. map_id .. "': " .. error)
    end
  end

  return map
end

local function export_map_entity(entity, entity_elem)
  if entity.name ~= nil then
    entity_elem:append("Name")[1] = entity.name
  end
  entity_elem:append("Layer")[1] = layer_names[entity.layer + 1]
  entity_elem:append("X")[1] = entity.x
  entity_elem:append("Y")[1] = entity.y
end

local function export_map(quest_path, map_id, map)
  print("Exporting map '" .. map_id .. "'")

  local root = xml.new("MapData")

  local properties_elem = root:append("Properties")
  if map.properties.x ~= nil then
    properties_elem:append("X")[1] = map.properties.x
  end
  if map.properties.y ~= nil then
    properties_elem:append("Y")[1] = map.properties.y
  end
  properties_elem:append("Width")[1] = map.properties.width
  properties_elem:append("Height")[1] = map.properties.height
  if map.properties.world ~= nil then
    properties_elem:append("World")[1] = map.properties.world
  end
  if map.properties.floor ~= nil then
    properties_elem:append("Floor")[1] = map.properties.floor
  end
  properties_elem:append("Tileset")[1] = map.properties.tileset
  properties_elem:append("Music")[1] = map.properties.music

  for _, tile in ipairs(map.tiles) do
    local tile_elem = root:append("Tile")
    export_map_entity(tile, tile_elem)
    tile_elem:append("Width")[1] = tile.width
    tile_elem:append("Height")[1] = tile.height
    tile_elem:append("Pattern")[1] = tile.pattern
  end

  for _, destination in ipairs(map.destinations) do
    local destination_elem = root:append("Destination")
    export_map_entity(destination, destination_elem)
    destination_elem:append("Direction")[1] = destination.direction
    if destination.sprite ~= nil then
      destination_elem:append("Sprite")[1] = destination.sprite
    end
    if destination.default ~= nil then
      destination_elem:append("Default")[1] = destination.default
    end
  end

  for _, destructible in ipairs(map.destructibles) do
	local destructible_elem = root:append("Destructible")
	export_map_entity(destructible, destructible_elem)
	if destructible.treasure_name ~= nil then
	  destructible_elem:append("TreasureName")[1] = destructible.treasure_name
	end
	if destructible.treasure_variant ~= nil then
	  destructible_elem:append("TreasureVariant")[1] = destructible.treasure_variant
	end
	if destructible.treasure_savegame_variable ~= nil then
	  destructible_elem:append("TreasureSavegameVariable")[1] = destructible.treasure_savegame_variable
	end
	destructible_elem:append("Sprite")[1] = destructible.sprite
	if destructible.destruction_sound ~= nil then
	  destructible_elem:append("DestructionSound")[1] = destructible.destruction_sound
	end
	if destructible.weight ~= nil then
	  destructible_elem:append("Weight")[1] = destructible.weight
	end
	if destructible.can_be_cut ~= nil then
	  destructible_elem:append("CanBeCut")[1] = destructible.can_be_cut
	end
	if destructible.can_explode ~= nil then
	  destructible_elem:append("CanExplode")[1] = destructible.can_explode
	end
	if destructible.can_regenerate ~= nil then
	  destructible_elem:append("CanRegenerate")[1] = destructible.can_regenerate
	end
	if destructible.damage_on_enemies ~= nil then
	  destructible_elem:append("DamageOnEnemies")[1] = destructible.damage_on_enemies
	end
	if destructible.ground ~= nil then
	  destructible_elem:append("Ground")[1] = destructible.ground
	end
  end

  local file = quest_path .. "maps/" .. map_id .. ".xml"
  xml.save(root, file)
end

local function import(quest_path, resources)
  local map_resource = resources.map
  local maps = {}

  for _, map_element in ipairs(map_resource) do
    local map_id = map_element.id
    local map = import_map(quest_path, map_id)
    maps[map_id] = map
  end

  return maps
end

function converter.convert(quest_path, resources)
  print("Converting maps")

  local maps = import(quest_path, resources)
  for map_id, map in pairs(maps) do
    export_map(quest_path, map_id, map)
  end
end

return converter
