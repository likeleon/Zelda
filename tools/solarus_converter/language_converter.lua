-- 언어별 데이터들을 XML로 변환합니다

local converter = {}

local skip_names = {
 ["none"] = "None",
 ["current"] = "Current",
 ["all"] = "All",
 ["unchanged"] = "Unchanged"
}

local report = require("report")
require("LuaXml")

local function import_strings(quest_path, language_id)
  local strings = {}
  local string_keys = {}

  local env = {}
  function env.text(properties)
    strings[properties.key] = properties.value
    string_keys[#string_keys + 1] = properties.key
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

  return strings, string_keys
end

local function import_dialogs(quest_path, language_id)
  local dialogs = {}
  local dialog_ids = {}

  local env = {}
  function env.dialog(properties)
    dialogs[properties.id] = properties
    dialog_ids[#dialog_ids + 1] = properties.id
  end

  local file = quest_path .. "languages/" .. language_id .. "/text/dialogs.dat"
    local chunk, error = loadfile(file)
  if chunk == nil then
    report.error("Error in language '" .. language_id .. "' dialogs: " .. error)
  else
    setfenv(chunk, env)
    local success, error = pcall(chunk)

    if not success then
      report.error("Error in language '" .. language_id .. "' dialogs: " .. error)
    end
  end

  return dialogs, dialog_ids
end

local function import_language(quest_path, language_id)
  print("Importing language '" .. language_id .. "'")

  local language = {}
  language.strings, language.string_keys = import_strings(quest_path, language_id)
  language.dialogs, language.dialog_ids = import_dialogs(quest_path, language_id)

  return language
end

local function export_strings(quest_path, language_id, strings, string_keys)
  local root = xml.new("Strings")

  for _, key in ipairs(string_keys) do
    local value = strings[key]
    local text_elem = root:append("Text")
    text_elem["Key"] = key
    text_elem[1] = value
  end

  local file = quest_path .. "languages/" .. language_id .. "/text/strings.xml"
  xml.save(root, file)
end

local function export_dialogs(quest_path, language_id, dialogs, dialog_ids)
  local root = xml.new("Dialogs")

  for _, id in ipairs(dialog_ids) do
    local dialog = dialogs[id]
    local dialog_elem = root:append("Dialog")
    dialog_elem["Id"] = id
    dialog_elem:append("Text")[1] = dialog.text
    if dialog.icon ~= nil then
      dialog_elem:append("Icon")[1] = dialog.icon
    end
    if dialog.skip ~= nil then
      dialog_elem:append("Skip")[1] = skip_names[dialog.skip]
    end
    if dialog.question ~= nil then
      dialog_elem:append("Question")[1] = dialog.question
    end
    if dialog.next ~= nil then
      dialog_elem:append("Next")[1] = dialog.next
    end
    if dialog.next2 ~= nil then
      dialog_elem:append("Next2")[1] = dialog.next2
    end
  end

  local file = quest_path .. "languages/" .. language_id .. "/text/dialogs.xml"
  xml.save(root, file)
end

local function export_language(quest_path, language_id, language)
  print("Exporting language '" .. language_id .. "'")

  export_strings(quest_path, language_id, language.strings, language.string_keys)
  export_dialogs(quest_path, language_id, language.dialogs, language.dialog_ids)
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
