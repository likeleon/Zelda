-- 퀘스트의 타일셋들을 XML로 변환합니다

local converter = {}
local ground_names = {
  empty = "Empty",
  traversable = "Traversable",
  wall = "Wall",
  low_wall = "LowWall",
  wall_top_right = "WallTopRight",
  wall_top_left = "WallTopLeft",
  wall_bottom_left = "WallBottomLeft",
  wall_bottom_right = "WallBottomRight",
  wall_top_right_water = "WallTopRightWater",
  wall_top_left_water = "WallTopLeftWater",
  wall_bottom_left_water = "WallBottomLeftWater",
  wall_bottom_right_water = "WallBottomRightWater",
  deep_water = "DeepWater",
  shallow_water = "ShallowWater",
  grass = "Grass",
  hole = "Hole",
  ice = "Ice",
  ladder = "Ladder",
  prickles = "Prickles",
  lava = "Lava"
}

local layer_names = {
  "Low", "Intermediate", "Hight", "NumLayers"
}

local scrolling_names = {
 [""] = "None",
 ["parallax"] = "Parallax",
 ["self"] = "Self"
}

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

  local bgcolor_elem = root:append("BackgroundColor")
  bgcolor_elem:append("R")[1] = tileset.background_color[1]
  bgcolor_elem:append("G")[1] = tileset.background_color[2]
  bgcolor_elem:append("B")[1] = tileset.background_color[3]
  if tileset.background_color[4] ~= nil then
    bgcolor_elem:append("A")[1] = tileset.background_color[4]
  end

  for id, pattern in ipairs(tileset.tile_patterns) do
    local pattern_elem = root:append("TilePattern")
    pattern_elem["Id"] = id
    pattern_elem:append("Ground")[1] = ground_names[pattern.ground]
    pattern_elem:append("DefaultLayer")[1] = layer_names[pattern.default_layer + 1]
    if pattern.scrolling ~= nil then
      pattern_elem:append("Scrolling")[1] = scrolling_names[pattern.scrolling]
    end
    if type(pattern.x) == "table" then
      for _, x in ipairs(pattern.x) do
        pattern_elem:append("X")[1] = x
      end
    else
      pattern_elem:append("X")[1] = pattern.x
    end
    if type(pattern.y) == "table" then
      for _, y in ipairs(pattern.y) do
        pattern_elem:append("Y")[1] = y
      end
    else
      pattern_elem:append("Y")[1] = pattern.y
    end
    pattern_elem:append("Width")[1] = pattern.width;
    pattern_elem:append("Height")[1] = pattern.height;
  end

  local file = quest_path .. "tilesets/" .. tileset_id .. ".xml"
  xml.save(root, file)
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
  end
end

return converter
