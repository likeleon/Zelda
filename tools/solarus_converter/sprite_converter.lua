-- 퀘스트의 스프라이트들을 XML로 변환합니다

local converter = {}

local report = require("report")
require("LuaXml")

local function import_sprite(quest_path, sprite_id)
  print("Importing sprite '" .. sprite_id .. "'")

  local sprite = {}
  sprite.animations = {}

  local env = {}
  function env.animation(properties)
    sprite.animations[properties.name] = properties
  end

  local file = quest_path .. "sprites/" .. sprite_id .. ".dat"
  local chunk, error = loadfile(file)
  if chunk == nil then
    report.error("Error in sprite '" .. sprite_id .. "': " .. error)
  else
    setfenv(chunk, env)
    local success, error = pcall(chunk)

    if not success then
      report.error("Error in sprite '" .. sprite_id .. "': " .. error)
    end
  end

  return sprite
end

local function export_sprite(quest_path, sprite_id, sprite)
  print("Exporting sprite '" .. sprite_id .. "'")

  local root = xml.new("Sprite")

  for name, animation in pairs(sprite.animations) do
    local anim_elem = root:append("Animation")
    anim_elem["Name"] = name
    anim_elem:append("SrcImage")[1] = animation.src_image
    if animation.frame_delay ~= nil then
      anim_elem:append("FrameDelay")[1] = animation.frame_delay
    end
    if animation.frame_to_loop_on ~= nil then
      anim_elem:append("FrameToLoopOn")[1] = animation.frame_to_loop_on
    end
    for _, direction in ipairs(animation.directions) do
      local dir_elem = anim_elem:append("Direction")
      dir_elem:append("X")[1] = direction.x
      dir_elem:append("Y")[1] = direction.y
      dir_elem:append("FrameWidth")[1] = direction.frame_width
      dir_elem:append("FrameHeight")[1] = direction.frame_height
      if direction.origin_x ~= nil then
        dir_elem:append("OriginX")[1] = direction.origin_x
      end
      if direction.origin_y ~= nil then
        dir_elem:append("OriginY")[1] = direction.origin_y
      end
      if direction.num_frames ~= nil then
        dir_elem:append("NumFrames")[1] = direction.num_frames
      end
      if direction.num_columns ~= nil then
        dir_elem:append("NumColumns")[1] = direction.num_columns
      end
    end
  end

  local file = quest_path .. "sprites/" .. sprite_id .. ".xml"
  xml.save(root, file)
end

local function import(quest_path, resources)
  local sprite_resource = resources.sprite
  local sprites = {}

  for _, sprite_element in ipairs(sprite_resource) do
    local sprite_id = sprite_element.id
    local sprite = import_sprite(quest_path, sprite_id)
    sprites[sprite_id] = sprite
  end

  return sprites
end

function converter.convert(quest_path, resources)
  print("Converting sprites")

  local sprites = import(quest_path, resources)
  for sprite_id, sprite in pairs(sprites) do
    export_sprite(quest_path, sprite_id, sprite)
  end
end

return converter
