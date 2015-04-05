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

  local env = {}
  function env.properties(properties)
    map.properties = properties
  end
  function env.tile(tile)
    map.tiles[#map.tiles + 1] = tile
  end
  function env.destination(destination)
    -- TODO
  end
  function env.chest(chest)
    -- TODO
  end
  function env.destructible(destructible)
    -- TODO
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
    if tile.name ~= nil then
      tile_elem:append("Name")[1] = tile.name
    end
    tile_elem:append("Layer")[1] = layer_names[tile.layer + 1]
    tile_elem:append("X")[1] = tile.x
    tile_elem:append("Y")[1] = tile.y
    tile_elem:append("Width")[1] = tile.width
    tile_elem:append("Height")[1] = tile.height
    tile_elem:append("Pattern")[1] = tile.pattern
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
