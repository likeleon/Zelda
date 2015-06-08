-- 언어별 데이터들을 XML로 변환합니다

local converter = {}

local report = require("report")
require("LuaXml")

local function import_strings(quest_path, language_id)
  local strings = {}

  local env = {}
  function env.text(properties)
    strings[properties.key] = properties.value
  end

  local file = quest_path .. "languages/" .. language_id .. "/text/strings.dat"
  local chunk, error = loadfile(file)
  if chunk == nil then
    report.error("Error in language '" .. language_id .. "' strings: " .. error)
  else
    setfenv(chunk, env)
    local success, error = pcall(chunk)

    if not success then
      report.error("Error in language '" .. language_id .. "' strings: " .. error)
    end
  end

  return strings
end

local function import_language(quest_path, language_id)
  print("Importing language '" .. language_id .. "'")

  local language = {}
  language.strings = import_strings(quest_path, language_id)

  return language
end

local function export_strings(quest_path, language_id, strings)
  local root = xml.new("Strings")

  for key, value in pairs(strings) do
    local text_elem = root:append("Text")
    text_elem["Key"] = key
    text_elem[1] = value
  end

  local file = quest_path .. "languages/" .. language_id .. "/text/strings.xml"
  xml.save(root, file)
end

local function export_language(quest_path, language_id, language)
  print("Exporting language '" .. language_id .. "'")

  export_strings(quest_path, language_id, language.strings)
end

local function import(quest_path, resources)
  local language_resource = resources.language
  local languages = {}

  for _, language_element in ipairs(language_resource) do
    local language_id = language_element.id
    local language = import_language(quest_path, language_id)
    languages[language_id] = language
  end

  return languages
end

function converter.convert(quest_path, resources)
  print("Converting languages")

  local languages = import(quest_path, resources)
  for language_id, language in pairs(languages) do
    export_language(quest_path, language_id, language)
  end
end

return converter
