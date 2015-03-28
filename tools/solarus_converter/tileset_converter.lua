-- 퀘스트의 타일셋들을 XML로 변환합니다

local converter = {}

local report = require("report")
require("LuaXml")

local function import_tileset(quest_path, tileset_id)
  print("Importing tileset '" .. tileset_id .. "'")

  local tileset = {}
  tileset.tile_patterns = {}

  local env = {}
  function env.background_color(color)
    tileset.background_color = color
  end
  function env.tile_pattern(properties)
    tileset.tile_patterns[properties.id] = properties
  end

  local file = quest_path .. "tilesets/" .. tileset_id .. ".dat"
  local chunk, error = loadfile(file)
  if chunk == nil then
    report.error("Error in tileset '" .. tileset_id .. "': " .. error)
  else
    setfenv(chunk, env)
    local success, error = pcall(chunk)

    if not success then
      report.error("Error in tileset '" .. tileset_id .. "': " .. error)
    end
  end

  return tileset
end

local function export_tileset(quest_path, tileset_id, tileset)
  print("Exporting tileset '" .. tileset_id .. "'")

  local root = xml.new("Tileset")

  local background_color = root:append("BackgroundColor")
  background_color:append("R")[1] = tileset.background_color[1]
  background_color:append("G")[1] = tileset.background_color[2]
  background_color:append("B")[1] = tileset.background_color[3]
  if tileset.background_color[4] ~= nil then
    background_color:append("A")[1] = tileset.background_color[4]
  end

  for id, pattern in ipairs(tileset.tile_patterns) do
    local tile_pattern = root:append("TilePattern")
    tile_pattern["Id"] = id
  end
  print(root)
end

local function import(quest_path, resources)
  print("Importing tilesets")

  local tileset_resource = resources.tileset
  local tilesets = {}

  for _, tileset_element in ipairs(tileset_resource) do
    local tileset_id = tileset_element.id
    local tileset = import_tileset(quest_path, tileset_id)
    tilesets[tileset_id] = tileset
  end

  return tilesets
end

function converter.convert(quest_path, resources)
  print("Converting tilesets")

  local tilesets = import(quest_path, resources)
  for tileset_id, tileset in pairs(tilesets) do
    export_tileset(quest_path, tileset_id, tileset)
    print("Tileset '" .. tileset_id .. "'")
    print("background_color: " .. tileset.background_color[1] .. ", " .. tileset.background_color[2] .. ", " .. tileset.background_color[3])
    for id, v in pairs(tileset.tile_patterns) do
      print(id .. ": " .. v.ground .. ", " .. v.default_layer)
    end
  end
end

return converter
